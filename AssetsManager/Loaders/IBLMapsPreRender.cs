using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

namespace AssetsManager.Loaders
{
    /// <summary>
    /// Class for converting Spherical Environment Map from .hdr format
    /// to TextureCubeAsset and calculate PreFiltered and Irradiance maps from it.
    /// </summary>
    internal class IBLMapsPreRender
    {
        struct CubeFaceCamera
        {
            public Matrix View;
            public Matrix Projection;
        }

        struct BRDFParamsBufferStruct
        {
            public float Roughness;
            public Vector3 filler;
        }

        Buffer ConstantsBuffer;
        Buffer BRDFParamsBuffer;

        VertexShader SphereToCubeMapVS;
        VertexShader IntegrateQuadVS;
        PixelShader SphereToCubeMapPS;
        PixelShader IrradiancePS;
        PixelShader PreFilteredPS;
        PixelShader IntegrateBRDFxPS;
        InputLayout CustomInputLayout;
        SamplerState Sampler;

        Texture2D InputMap;
        Texture2D ConvertedCubeMap;
        Texture2D IrradianceCubeMap;
        Texture2D PreFilteredCubeMap;
        Texture2D IntegrateBRDFxMap;

        ShaderResourceView InputSRV;
        ShaderResourceView ConvertedCubeSRV;

        DeviceContext[] contextList;
        RenderTargetView[] OutputRTVs = new RenderTargetView[6];
        RenderTargetView IntegrateBRDFxRTV;
        CubeFaceCamera[] Cameras = new CubeFaceCamera[6];
        ViewportF Viewport;

        Device m_Device;
        DeviceContext GetContext {
            get {
                return m_Device?.ImmediateContext;
            }
        }

        const int OutputMapSize = 512;
        const int IrradianceSize = 32;
        const int PreFilteredSize = 512;
        const int PreFilteredMipsCount = 6;
        const int BRDFxMapSize = 512;

        readonly int Threads = 2;
        int OutputResolution = OutputMapSize;

        enum RenderState
        {
            CubeMap,
            IrradianceMap,
            PreFilteredMap,
            IntegrateBRDF,
        }
        RenderState CurrentState;

        public void Init(string sourcePath)
        {
            if (!InitDevice()) {
                return;
            }
            
            #region InputMap
            DataBox[] initData = GetHDRTextureData(sourcePath, out int InputWidth, out int InputHeight);
            Texture2DDescription InputTextureDesc = new Texture2DDescription()
            {
                Format = Format.R32G32B32_Float,
                Width = InputWidth,
                Height = InputHeight,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };
            InputMap = ToDispose(new Texture2D(m_Device, InputTextureDesc, initData));
            InputMap.DebugName = "InputMap";

            ShaderResourceViewDescription descSRV = new ShaderResourceViewDescription()
            {
                Format = InputTextureDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };
            descSRV.Texture2D.MipLevels = InputTextureDesc.MipLevels;
            descSRV.Texture2D.MostDetailedMip = 0;
            InputSRV = ToDispose(new ShaderResourceView(m_Device, InputMap, descSRV));
            InputSRV.DebugName = "InputSRV";
            #endregion

            #region OutputMap
            Texture2DDescription OutputTextureDescription = new Texture2DDescription()
            {
                Width = OutputResolution,
                Height = OutputResolution,
                ArraySize = 6,
                MipLevels = 0,
                Format = Format.R16G16B16A16_Float,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            };
            ConvertedCubeMap = ToDispose(new Texture2D(m_Device, OutputTextureDescription));
            ConvertedCubeMap.DebugName = "ConvertedCubeMap";
            OutputTextureDescription.OptionFlags = ResourceOptionFlags.TextureCube;

            descSRV = new ShaderResourceViewDescription()
            {
                Format = OutputTextureDescription.Format,
                Dimension = ShaderResourceViewDimension.TextureCube,
            };
            descSRV.TextureCube.MipLevels = -1;
            descSRV.TextureCube.MostDetailedMip = 0;
            ConvertedCubeSRV = ToDispose(new ShaderResourceView(m_Device, ConvertedCubeMap, descSRV));
            ConvertedCubeSRV.DebugName = "ConvertedCubeSRV";
            #endregion

            #region IrradianceMap
            OutputTextureDescription.Width = IrradianceSize;
            OutputTextureDescription.Height = IrradianceSize;
            OutputTextureDescription.MipLevels = 1;
            IrradianceCubeMap = ToDispose(new Texture2D(m_Device, OutputTextureDescription));
            IrradianceCubeMap.DebugName = "IrradianceCubeMap";
            #endregion

            #region PreFilteredMap
            OutputTextureDescription.Width = PreFilteredSize;
            OutputTextureDescription.Height = PreFilteredSize;
            OutputTextureDescription.MipLevels = PreFilteredMipsCount;
            PreFilteredCubeMap = ToDispose(new Texture2D(m_Device, OutputTextureDescription));
            PreFilteredCubeMap.DebugName = "PreFilteredCubeMap";
            #endregion

            SetupShadersAndBuffers();
        }

        bool InitDevice()
        {
            DriverType CurrentDriverType = DriverType.Null;
            DriverType[] driverTypes = new DriverType[] {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference,
            };

            DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
            FeatureLevel[] levels = new FeatureLevel[] {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
            };

            foreach (var driverType in driverTypes) {
                m_Device = new Device(driverType, deviceCreationFlags, levels);
                if (m_Device != null) {
                    m_Device.DebugName = "The Device";
                    CurrentDriverType = driverType;
                    break;
                }
            }

            if (m_Device == null) {
                Console.WriteLine("Device not created!");
                return false;
            }

            // Create context list
            if (Threads == 1) {
                contextList = null;
            } else {
                contextList = new DeviceContext[Threads];
                for (var i = 0; i < Threads; i++) {
                    contextList[i] = ToDispose(new DeviceContext(m_Device));
                    contextList[i].DebugName = $"Context#{i}";
                }
            }
            //Console.WriteLine($"Device was created! DriverType: {CurrentDriverType.ToString()}");
            return true;
        }

        void SetupShadersAndBuffers()
        {
            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            AssetsMeta.ShaderAsset meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapPS");
            ShaderBytecode shaderBytecode = new ShaderBytecode(meta.Bytecode);
            SphereToCubeMapPS = ToDispose(new PixelShader(m_Device, shaderBytecode));
            SphereToCubeMapPS.DebugName = "SphereToCubeMapPS";

            meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IrradiancePS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            IrradiancePS = ToDispose(new PixelShader(m_Device, shaderBytecode));
            IrradiancePS.DebugName = "IrradiancePS";

            meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_PreFilteredPS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            PreFilteredPS = ToDispose(new PixelShader(m_Device, shaderBytecode));
            PreFilteredPS.DebugName = "PreFilteredPS";

            meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapVS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            SphereToCubeMapVS = ToDispose(new VertexShader(m_Device, shaderBytecode));
            SphereToCubeMapVS.DebugName = "SphereToCubeMapVS";

            CustomInputLayout = ToDispose(new InputLayout(m_Device, 
                ShaderSignature.GetInputSignature(shaderBytecode), new InputElement[] {
                new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
            }));

            // Create the per environment map buffer ViewProjection matrices
            ConstantsBuffer = ToDispose(new Buffer(
                m_Device,
                Utilities.SizeOf<Matrix>() * 2,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0)
            );
            ConstantsBuffer.DebugName = "ConstantsBuffer";

            BRDFParamsBuffer = ToDispose(new Buffer(
                m_Device,
                Utilities.SizeOf<BRDFParamsBufferStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0)
            );
            BRDFParamsBuffer.DebugName = "BRDFParamsBuffer";

            Sampler = ToDispose(new SamplerState(m_Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear, // Trilinear
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunction = Comparison.Never,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            }));
            Sampler.DebugName = "DefaultTrilinearSampler";
        }

        void SetViewPoint(Vector3 camera)
        {
            // The LookAt targets for view matrices
            var targets = new[] {
                camera + Vector3.UnitX, // +X
                camera - Vector3.UnitX, // -X
                camera + Vector3.UnitY, // +Y
                camera - Vector3.UnitY, // -Y
                camera + Vector3.UnitZ, // +Z
                camera - Vector3.UnitZ  // -Z
            };

            // The "up" vector for view matrices
            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                +Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            // Create view and projection matrix for each face
            for (int i = 0; i < 6; i++) {
                Cameras[i].View = Matrix.LookAtLH(camera, targets[i], upVectors[i]);
                Cameras[i].Projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1.0f, 0.1f, 100.0f);
            }
        }

        void CreateRenderTargetsFromMap(Texture2D Map, bool withMips)
        {
            RenderTargetViewDescription RTVdesc = new RenderTargetViewDescription()
            {
                Format = Map.Description.Format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
            };
            RTVdesc.Texture2DArray.ArraySize = 1;

            for (int i = 0; i < OutputRTVs.Length; i++) {
                OutputRTVs[i]?.Dispose();
            }

            if (withMips) {
                OutputRTVs = new RenderTargetView[6 * PreFilteredMipsCount];
                for (int j = 0; j < 6; j++) {
                    for (int i = 0; i < PreFilteredMipsCount; i++) {
                        RTVdesc.Texture2DArray.MipSlice = i;
                        RTVdesc.Texture2DArray.FirstArraySlice = j;
                        OutputRTVs[j * PreFilteredMipsCount + i] = ToDispose(new RenderTargetView(m_Device, Map, RTVdesc));
                        OutputRTVs[j * PreFilteredMipsCount + i].DebugName = $"CubeRTVFace{j}Mip{i}";
                    }
                }
                return;
            }

            OutputRTVs = new RenderTargetView[6];

            RTVdesc.Texture2DArray.MipSlice = 0;
            for (int i = 0; i < OutputRTVs.Length; i++) {
                RTVdesc.Texture2DArray.FirstArraySlice = i;
                OutputRTVs[i]?.Dispose();
                OutputRTVs[i] = ToDispose(new RenderTargetView(m_Device, Map, RTVdesc));
                OutputRTVs[i].DebugName = $"CubeRTVFace{i}";
            }
        }

        DataBox[] GetHDRTextureData(string sourcePath, out int InputWidth, out int InputHeight)
        {
            float[] imageFloats = TextureLoader.LoadHDRTexture(sourcePath, out InputWidth, out InputHeight, out int pixelSize);
            if (InputWidth == 0) {
                Console.WriteLine("Texture not loaded!");
                return null;
            }

            IntPtr pSrcBits = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(imageFloats, 0);
            DataBox[] initData = new DataBox[1];
            initData[0].DataPointer = pSrcBits;
            initData[0].RowPitch = InputWidth * pixelSize;
            initData[0].SlicePitch = InputWidth * InputHeight * pixelSize;
            return initData;
        }
        
        public void Render(string path)
        {
            if (GetContext == null) {
                return;
            }

            CurrentState = RenderState.CubeMap;
            SetViewPoint(Vector3.Zero);
            OutputResolution = OutputMapSize;
            Viewport = new Viewport(0, 0, OutputResolution, OutputResolution);
            CreateRenderTargetsFromMap(ConvertedCubeMap, false);
            UpdateThreaded(path);

            CurrentState = RenderState.IrradianceMap;
            Viewport = new Viewport(0, 0, IrradianceSize, IrradianceSize);
            OutputResolution = IrradianceSize;
            CreateRenderTargetsFromMap(IrradianceCubeMap, false);
            UpdateThreaded(path);

            CurrentState = RenderState.PreFilteredMap;
            Viewport = new Viewport(0, 0, PreFilteredSize, PreFilteredSize);
            OutputResolution = PreFilteredSize;
            CreateRenderTargetsFromMap(PreFilteredCubeMap, true);
            UpdateThreaded(path);
        }

        public void RenderBRDF(string outputPath)
        {
            CurrentState = RenderState.IntegrateBRDF;

            if (GetContext == null) {
                if (!InitDevice()) {
                    return;
                }
            }
            SetViewPoint(Vector3.Zero);
            InitBRDFxResources();
            Viewport = new Viewport(0, 0, BRDFxMapSize, BRDFxMapSize);
            UpdateCubeFace(GetContext, 0);
            IntegrateBRDFxMap.Save(GetContext, m_Device, outputPath, false);
        }

        private void InitBRDFxResources()
        {
            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            AssetsMeta.ShaderAsset meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapPS");

            meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IntegrateBRDFxPS");
            ShaderBytecode shaderBytecode = new ShaderBytecode(meta.Bytecode);
            IntegrateBRDFxPS = ToDispose(new PixelShader(m_Device, shaderBytecode));
            IntegrateBRDFxPS.DebugName = "IntegrateBRDFxPS";
            
            meta = AM.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IntegrateQuadVS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            IntegrateQuadVS = ToDispose(new VertexShader(m_Device, shaderBytecode));
            IntegrateQuadVS.DebugName = "IntegrateQuadVS";

            if (CustomInputLayout == null) {
                CustomInputLayout = ToDispose(new InputLayout(m_Device,
                    ShaderSignature.GetInputSignature(shaderBytecode), new InputElement[] {
                    new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
                }));
            }

            if (ConstantsBuffer == null) {
                // Create the per environment map buffer ViewProjection matrices
                ConstantsBuffer = ToDispose(new Buffer(
                    m_Device,
                    Utilities.SizeOf<Matrix>() * 2,
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None, 0)
                );
                ConstantsBuffer.DebugName = "ConstantsBuffer";
            }

            IntegrateBRDFxMap = ToDispose(new Texture2D(m_Device, new Texture2DDescription()
            {
                Width = BRDFxMapSize,
                Height = BRDFxMapSize,
                Format = Format.R16G16_Float, //R8G8B8A8_UNorm for debug
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
            }));

            IntegrateBRDFxRTV = ToDispose(new RenderTargetView(m_Device, IntegrateBRDFxMap, new RenderTargetViewDescription()
            {
                Dimension = RenderTargetViewDimension.Texture2D,
                Format = IntegrateBRDFxMap.Description.Format,
                Texture2D = new RenderTargetViewDescription.Texture2DResource()
                {
                    MipSlice = 0,
                }
            }));
        }

        void UpdateCubeFace(DeviceContext context, int index)
        {
            UpdateCubeFace(context, index, -1);
        }

        void UpdateCubeFace(DeviceContext context, int index, int mip)
        {
            // Prepare pipeline
            context.ClearState();

            context.Rasterizer.SetViewport(Viewport);
            context.InputAssembler.InputLayout = CustomInputLayout;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            if (CurrentState == RenderState.IntegrateBRDF) {
                context.OutputMerger.SetRenderTargets(null, IntegrateBRDFxRTV);
                context.ClearRenderTargetView(IntegrateBRDFxRTV, Color.CornflowerBlue);
                context.VertexShader.Set(IntegrateQuadVS);
                context.PixelShader.Set(IntegrateBRDFxPS);
                context.Draw(4, 0);
                UnbindContextRTV(context);
                return;
            }

            int rtvIndex = mip < 0 ? index : (index * PreFilteredMipsCount + mip);
            context.OutputMerger.SetRenderTargets(null, OutputRTVs[rtvIndex]);
            // Render the scene using the view, projection, RTV and DSV of this cube face
            context.ClearRenderTargetView(OutputRTVs[rtvIndex], Color.CornflowerBlue);


            Matrix[] ViewProj = new Matrix[]
            {
                Cameras[index].View,
                Cameras[index].Projection,
            };
            context.UpdateSubresource(ViewProj, ConstantsBuffer);
            context.VertexShader.Set(SphereToCubeMapVS);
            context.VertexShader.SetConstantBuffer(0, ConstantsBuffer);
            context.PixelShader.SetConstantBuffer(1, BRDFParamsBuffer);
            context.PixelShader.SetSampler(0, Sampler);

            switch (CurrentState) {
                case RenderState.CubeMap:
                    context.PixelShader.Set(SphereToCubeMapPS);
                    context.PixelShader.SetShaderResource(0, InputSRV);
                    break;
                case RenderState.IrradianceMap:
                    context.PixelShader.Set(IrradiancePS);
                    context.PixelShader.SetShaderResource(1, ConvertedCubeSRV);
                    break;
                case RenderState.PreFilteredMap:
                    context.PixelShader.Set(PreFilteredPS);
                    context.PixelShader.SetShaderResource(1, ConvertedCubeSRV);
                    break;
            }

            context.Draw(14, 0);
            UnbindContextRTV(context);
        }

        private void UnbindContextRTV(DeviceContext context)
        {
            // Unbind the RTV and DSV
            context.VertexShader.Set(null);
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.Set(null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.PixelShader.SetShaderResources(0, new ShaderResourceView[2]);
            context.PixelShader.SetSampler(0, null);
            context.OutputMerger.ResetTargets();
        }

        void UpdatePreFilteredFaces(DeviceContext context, int startIndex, int endIndex)
        {
            int mipSize;
            float roughness;

            for (int mip = 0; mip < PreFilteredMipsCount; mip++) {
                mipSize = (int)(PreFilteredSize * Math.Pow(0.5, mip));
                roughness = mip / (float)(PreFilteredMipsCount - 1);
                BRDFParamsBufferStruct cbParams = new BRDFParamsBufferStruct()
                {
                    Roughness = roughness,
                };
                context.UpdateSubresource(ref cbParams, BRDFParamsBuffer);
                for (var j = startIndex; j <= endIndex; j++) {
                    Viewport = new Viewport(0, 0, mipSize, mipSize);
                    UpdateCubeFace(context, j, mip);
                }
            }
        }

        void UpdateThreaded(string path)
        {
            var contexts = contextList ?? new DeviceContext[] { this.GetContext };
            CommandList[] commands = new CommandList[contexts.Length];
            int batchSize = 6 / contexts.Length;

            Task[] tasks = new Task[contexts.Length];

            for (var i = 0; i < contexts.Length; i++) {
                var contextIndex = i;

                tasks[i] = Task.Run(() => {
                    var context = contexts[contextIndex];

                    int startIndex = batchSize * contextIndex;
                    int endIndex = Math.Min(startIndex + batchSize, 5);
                    if (contextIndex == contexts.Length - 1) {
                        endIndex = 5;
                    }

                    if (CurrentState == RenderState.PreFilteredMap) {
                        UpdatePreFilteredFaces(context, startIndex, endIndex);
                    } else {
                        for (var j = startIndex; j <= endIndex; j++) {
                            UpdateCubeFace(context, j);
                        }
                    }

                    if (context.TypeInfo == DeviceContextType.Deferred) {
                        commands[contextIndex] = ToDispose(context.FinishCommandList(false));
                    }
                });
            }
            Task.WaitAll(tasks);

            // Execute command lists (if any)
            for (var i = 0; i < contexts.Length; i++) {
                if (contexts[i].TypeInfo == DeviceContextType.Deferred && commands[i] != null) {
                    GetContext.ExecuteCommandList(commands[i], false);
                    commands[i].Dispose();
                    commands[i] = null;
                }
            }

            switch (CurrentState) {
                case RenderState.CubeMap:
                    GetContext.GenerateMips(ConvertedCubeSRV);
                    ConvertedCubeMap.Save(GetContext, m_Device, $"{path}CubeMap", false);
                    break;
                case RenderState.IrradianceMap:
                    IrradianceCubeMap.Save(GetContext, m_Device, $"{path}IrradianceCubeMap", false);
                    break;
                case RenderState.PreFilteredMap:
                    PreFilteredCubeMap.Save(GetContext, m_Device, $"{path}PreFilteredCubeMap", true);
                    break;
            }
        }

        #region Dispose section
        List<IDisposable> ToDisposeList = new List<IDisposable>();

        T ToDispose<T>(T obj) where T : IDisposable
        {
            ToDisposeList.Add(obj);
            return obj;
        }

        public void Dispose()
        {
            ToDisposeList.Reverse();
            foreach (var item in ToDisposeList) {
                item?.Dispose();
            }
            ToDisposeList.Clear();
            ToDisposeList = null;

            if (contextList != null) {
                foreach (var item in contextList) {
                    item?.Dispose();
                }
                contextList = null;
            }

            GetContext.ClearState();
            GetContext.Flush();
            GetContext.Dispose();
            m_Device?.Dispose();
            SaveToWICImage.DisposeFactory();
        }
        #endregion
    }
}
