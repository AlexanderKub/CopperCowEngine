using AssetsManager;
using AssetsManager.AssetsMeta;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EngineCore.D3D11.SRITypeEnums;
using Buffer = SharpDX.Direct3D11.Buffer;
using Resource = SharpDX.Direct3D11.Resource;

namespace EngineCore.D3D11
{

    #region Sampler states
    internal partial class SRITypeEnums
    {
        public enum SamplerType
        {
            PointClamp,
            PointWrap,

            BilinearClamp,
            BilinearWrap,

            TrilinearClamp,
            TrilinearWrap,

            AnisotropicWrap,
            ShadowMap,
            PreIntegratedSampler,
            // What the sampler (Who?)
            IBLSampler,
        }
    }

    internal partial class SharedRenderItemsStorage
    {
        private SamplerState[] SamplersArray;

        public SamplerState GetSamplerState(SRITypeEnums.SamplerType type)
        {
            return SamplersArray[(int)type];
        }

        // Point Texture Filtering - D3D11_FILTER_MIN_MAG_MIP_POINT - Filter.MinMagMipPoint
        // Bilinear Texture Filtering - D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT - Filter.MinMagLinearMipPoint
        // Trilinear Texture Filtering - D3D11_FILTER_MIN_MAG_MIP_LINEAR - Filter.MinMagMipLinear
        // Anisotropic Texture Filtering - D3D11_FILTER_ANISOTROPIC - Filter.Anisotropic

        private void InitSamplers()
        {
            SamplersArray = new SamplerState[Enum.GetNames(typeof(SamplerType)).Length];
            
            SamplersArray[(int)SamplerType.PointClamp] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipPoint,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.PointClamp].DebugName = "PointClampSampler";

            SamplersArray[(int)SamplerType.PointWrap] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipPoint,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.PointWrap].DebugName = "PointWrapSampler";

            SamplersArray[(int)SamplerType.BilinearClamp] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.BilinearClamp].DebugName = "BilinearClampSampler";

            SamplersArray[(int)SamplerType.BilinearWrap] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.BilinearWrap].DebugName = "BilinearWrapSampler";

            SamplersArray[(int)SamplerType.TrilinearClamp] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.TrilinearClamp].DebugName = "TrilinearClampSampler";

            SamplersArray[(int)SamplerType.TrilinearWrap] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.TrilinearWrap].DebugName = "TrilinearWrapSampler";

            SamplersArray[(int)SamplerType.AnisotropicWrap] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                Filter = Filter.Anisotropic,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaximumAnisotropy = 16,
                ComparisonFunction = Comparison.Always,
                MaximumLod = float.MaxValue,
            });
            SamplersArray[(int)SamplerType.AnisotropicWrap].DebugName = "AnisotropicWrapSampler";

            SamplersArray[(int)SamplerType.ShadowMap] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                BorderColor = Color.Black,
                Filter = Filter.ComparisonMinMagMipLinear,
                ComparisonFunction = Comparison.Less,
            });
            SamplersArray[(int)SamplerType.ShadowMap].DebugName = "ShadowMapSampler";

            SamplersArray[(int)SamplerType.PreIntegratedSampler] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                //Filter = Filter.MinMagLinearMipPoint,
                Filter = Filter.MinMagMipPoint, // Point
            });
            SamplersArray[(int)SamplerType.PreIntegratedSampler].DebugName = "PreIntegratedSampler";

            SamplersArray[(int)SamplerType.IBLSampler] = new SamplerState(m_RenderBackend.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagLinearMipPoint, // Bilinear
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0.0f,
            });
            SamplersArray[(int)SamplerType.IBLSampler].DebugName = "IBLSampler";
        }

        private void DisposeSamplers()
        {
            for (int i = 0; i < SamplersArray.Length; i++)
            {
                SamplersArray[i]?.Dispose();
                SamplersArray[i] = null;
            }
            SamplersArray = null;
        }
    }
    #endregion

    #region Input layouts
    internal partial class SharedRenderItemsStorage
    {
        public InputLayout StandardInputLayout;

        public InputElement[] StandardInputElements = new[] {
            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
        };

        private void InitInputLayouts()
        {
            AssetsLoader.GetShader<VertexShader>("CommonVS", out ShaderSignature signature);
            StandardInputLayout = new InputLayout(m_RenderBackend.Device, signature, StandardInputElements);
            StandardInputLayout.DebugName = "StandardInputLayout";
        }

        private void DisposeInputLayouts()
        {
            StandardInputLayout.Dispose();
        }
    }
    #endregion

    #region Depth stencil states

    internal partial class SRITypeEnums
    {
        public enum DepthStencilStates
        {
            Greater,
            Less,
            EqualAndDisableWrite,
            GreaterAndDisableWrite,
            LessEqualAndDisableWrite,
            Disabled,
        }
    }

    internal partial class SharedRenderItemsStorage
    {
        private DepthStencilState[] DepthStencilStatesArray;

        public DepthStencilState GetDepthStencilState(SRITypeEnums.DepthStencilStates type)
        {
            return DepthStencilStatesArray[(int)type];
        }

        private void InitDepthStencilStates()
        {
            DepthStencilStatesArray = new DepthStencilState[Enum.GetNames(typeof(SRITypeEnums.DepthStencilStates)).Length];
            DepthStencilStateDescription DepthStencilStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Greater,
                IsStencilEnabled = false,
            };
            int id = (int)SRITypeEnums.DepthStencilStates.Greater;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.Greater";

            id = (int)SRITypeEnums.DepthStencilStates.Less;
            DepthStencilStateDesc.DepthComparison = Comparison.Less;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.Less";

            id = (int)SRITypeEnums.DepthStencilStates.GreaterAndDisableWrite;
            DepthStencilStateDesc.DepthWriteMask = DepthWriteMask.Zero;
            DepthStencilStateDesc.DepthComparison = Comparison.Greater;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.GreaterAndDisableWrite";

            id = (int)SRITypeEnums.DepthStencilStates.EqualAndDisableWrite;
            DepthStencilStateDesc.DepthComparison = Comparison.Equal;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.EqualAndDisableWrite";

            id = (int)SRITypeEnums.DepthStencilStates.LessEqualAndDisableWrite;
            DepthStencilStateDesc.DepthComparison = Comparison.LessEqual;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.LessEqualAndDisableWrite";

            id = (int)SRITypeEnums.DepthStencilStates.Disabled;
            DepthStencilStateDesc.IsDepthEnabled = false;
            DepthStencilStateDesc.DepthComparison = Comparison.Always;
            DepthStencilStatesArray[id] = new DepthStencilState(m_RenderBackend.Device, DepthStencilStateDesc);
            DepthStencilStatesArray[id].DebugName = "DepthStencilStates.Disabled";
        }

        private void DisposeDepthStencilStates()
        {
            for (int i = 0; i < DepthStencilStatesArray.Length; i++)
            {
                DepthStencilStatesArray[i]?.Dispose();
                DepthStencilStatesArray[i] = null;
            }
            DepthStencilStatesArray = null;
        }
    }
    #endregion

    #region Blend states
    internal partial class SRITypeEnums
    {
        public enum BlendStates
        {
            Opaque,
            AlphaEnabledBlending,
            DepthOnlyAlphaTest,
            DepthOnlyAlphaToCoverage,
            Additive,
        }
    }

    internal partial class SharedRenderItemsStorage
    {
        public SharpDX.Mathematics.Interop.RawColor4 BlendFactor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);

        private BlendState[] BlendStatesArray;

        public BlendState GetBlendState(SRITypeEnums.BlendStates type)
        {
            return BlendStatesArray[(int)type];
        }

        private void InitBlendStates()
        {
            BlendStatesArray = new BlendState[Enum.GetNames(typeof(SRITypeEnums.BlendStates)).Length];
            BlendStateDescription BlendStateDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            BlendStateDesc.RenderTarget[0].IsBlendEnabled = false;
            BlendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            BlendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            BlendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            BlendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            BlendStatesArray[(int)SRITypeEnums.BlendStates.Opaque] = new BlendState(m_RenderBackend.Device, BlendStateDesc);
            BlendStatesArray[(int)SRITypeEnums.BlendStates.Opaque].DebugName = "BlendStates.Opaque";

            BlendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            BlendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            BlendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            BlendStatesArray[(int)SRITypeEnums.BlendStates.AlphaEnabledBlending] = new BlendState(m_RenderBackend.Device, BlendStateDesc);
            BlendStatesArray[(int)SRITypeEnums.BlendStates.AlphaEnabledBlending].DebugName = "BlendStates.AlphaEnabledBlending";

            BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
            BlendStatesArray[(int)SRITypeEnums.BlendStates.DepthOnlyAlphaTest] = new BlendState(m_RenderBackend.Device, BlendStateDesc);
            BlendStatesArray[(int)SRITypeEnums.BlendStates.DepthOnlyAlphaTest].DebugName = "BlendStates.DepthOnlyAlphaTest";

            BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
            BlendStateDesc.AlphaToCoverageEnable = true;
            BlendStatesArray[(int)SRITypeEnums.BlendStates.DepthOnlyAlphaToCoverage] = new BlendState(m_RenderBackend.Device, BlendStateDesc);
            BlendStatesArray[(int)SRITypeEnums.BlendStates.DepthOnlyAlphaToCoverage].DebugName = "BlendStates.DepthOnlyAlphaToCoverage";

            BlendStateDesc = new BlendStateDescription();
            BlendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            BlendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            BlendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            BlendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.One;
            BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            BlendStatesArray[(int)SRITypeEnums.BlendStates.Additive] = new BlendState(m_RenderBackend.Device, BlendStateDesc);
            BlendStatesArray[(int)SRITypeEnums.BlendStates.Additive].DebugName = "BlendStates.Additive";
        }

        private void DisposeBlendStates()
        {
            for (int i = 0; i < BlendStatesArray.Length; i++)
            {
                BlendStatesArray[i]?.Dispose();
                BlendStatesArray[i] = null;
            }
            BlendStatesArray = null;
        }
    }
    #endregion

    #region Rasterizer states
    internal partial class SRITypeEnums
    {
        public enum RasterizerStates
        {
            SolidFrontCull,
            SolidBackCull,
            SolidNoneCull,
            WireframeFrontCull,
            WireframeBackCull,
            WireframeNoneCull,
        }
    }

    internal partial class SharedRenderItemsStorage
    {
        private RasterizerState[] RasterizerStatesArray;

        public RasterizerState GetRasterizerState(SRITypeEnums.RasterizerStates type)
        {
            return RasterizerStatesArray[(int)type];
        }

        private void InitRasterizerState()
        {
            bool isMSEnabled = m_RenderBackend.SampleCount > 1;
            RasterizerStatesArray = new RasterizerState[Enum.GetNames(typeof(SRITypeEnums.RasterizerStates)).Length];
            RasterizerStateDescription desc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Front,
                IsMultisampleEnabled = isMSEnabled,
                /*IsDepthClipEnabled = true,
                SlopeScaledDepthBias = 0.0001f,
                DepthBiasClamp = 2.0f,
                DepthBias = 25000,*/
            };
            for (int i = 0; i < RasterizerStatesArray.Length; i++)
            {
                desc.CullMode = i % 3 == 0 ? CullMode.Front : (i % 3 == 1 ? CullMode.Back : CullMode.None);
                desc.FillMode = i > 2 ? FillMode.Wireframe : FillMode.Solid;
                RasterizerStatesArray[i] = new RasterizerState(m_RenderBackend.Device, desc);
                RasterizerStatesArray[i].DebugName = $"{desc.CullMode.ToString()}{desc.FillMode.ToString()}RasterizerState";
            }
        }

        private void DisposeRasterizerState()
        {
            for (int i = 0; i < RasterizerStatesArray.Length; i++)
            {
                RasterizerStatesArray[i]?.Dispose();
                RasterizerStatesArray[i] = null;
            }
            RasterizerStatesArray = null;
        }
    }
    #endregion

    #region Meshes cache
    internal partial class SharedRenderItemsStorage
    {
        internal struct CachedMesh
        {
            public AssetsManager.Loaders.ModelGeometry geometry;
            public Buffer vertexBuffer;
            public Buffer indexBuffer;
            public int IndexCount { get { return geometry.Indexes.Length; } }
        }

        private Dictionary<string, CachedMesh> m_MeshesCache;

        private BufferDescription m_VertexBufferDescription;

        private BufferDescription m_IndexBufferDescription;

        private void InitMeshesCache()
        {
            m_MeshesCache = new Dictionary<string, CachedMesh>();
            // Create buffers descriptions.
            m_VertexBufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
            };
            m_IndexBufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
            };
        }

        private void DisposeMeshesCache()
        {
            foreach (var mesh in m_MeshesCache)
            {
                mesh.Value.vertexBuffer.Dispose();
                mesh.Value.indexBuffer.Dispose();
            }
            m_MeshesCache.Clear();
            m_MeshesCache = null;
        }
    
        public CachedMesh GetMesh(string name)
        {
            if (!m_MeshesCache.ContainsKey(name))
            {
                AssetsManager.Loaders.ModelGeometry MeshInstance;
                if (name.StartsWith("Primitives."))
                {
                    switch (name)
                    {
                        case "Primitives.Sphere":
                            MeshInstance = Primitives.Sphere(16);
                            break;
                        case "Primitives.LVSphere":
                            MeshInstance = Primitives.Sphere(6);
                            break;
                        case "Primitives.Cube":
                            MeshInstance = Primitives.Cube;
                            break;
                        default:
                            MeshInstance = Primitives.Cube;
                            break;
                    }
                }
                else
                {
                    MeshInstance = AssetsLoader.LoadMesh(name);
                }

                if (MeshInstance.Points == null) {
                    MeshInstance = Primitives.Cube;
                }

                var tmp = new CachedMesh()
                {
                    geometry = MeshInstance,
                    vertexBuffer = Buffer.Create(m_RenderBackend.Device, MeshInstance.Points, m_VertexBufferDescription),
                    indexBuffer = Buffer.Create(m_RenderBackend.Device, MeshInstance.Indexes, m_IndexBufferDescription),
                };

                tmp.vertexBuffer.DebugName = name + "VertexBuffer";
                tmp.indexBuffer.DebugName = name + "IndexBuffer";
                m_MeshesCache.Add(name, tmp);
                AssetsLoader.DropCachedMesh(name);
            }
            return m_MeshesCache[name];
        }
    }
    #endregion

    #region Textures cache
    internal partial class SharedRenderItemsStorage
    {
        private Texture2D DebugTexture {
            get {
                return LoadTextureAsset("DebugTextureMap");
            }
        }

        private Texture2D DebugCubeTexture {
            get {
                return LoadCubeTextureAsset("SkyboxCubeMap", out TextureCubeAsset t);
            }
        }

        // SkyboxIrradianceCubeMap MiraSkyboxIrradianceCubeMap NightskyIrradianceCubeMap HouseIrradianceCubeMap
        public ShaderResourceView IrradianceMap {
            get {
                return LoadTextureSRV("HouseIrradianceCubeMap", true);
            }
        }

        public ShaderResourceView PreFilteredMap {
            get {
                return LoadTextureSRV("HousePreFilteredCubeMap", true);
            }
        }

        public ShaderResourceView BRDFxLUT {
            get {
                return LoadTextureSRV("StandardBRDFxLUT", false);
            }
        }

        private Dictionary<string, ShaderResourceView> m_TexturesCache;

        private Dictionary<string, Texture2D> m_Textures2DCache;

        private void InitTexturesCache()
        {
            m_TexturesCache = new Dictionary<string, ShaderResourceView>();
            m_Textures2DCache = new Dictionary<string, Texture2D>();
        }

        private void DisposeTexturesCache()
        {
            foreach (var srv in m_TexturesCache) {
                srv.Value?.Dispose();
            }
            m_TexturesCache.Clear();
            m_TexturesCache = null;

            foreach (var texture in m_Textures2DCache) {
                texture.Value?.Dispose();
            }
            m_Textures2DCache.Clear();
            m_Textures2DCache = null;
        }

        public Texture2D LoadTextureAsset(string assetName)
        {
            if (m_Textures2DCache.ContainsKey(assetName)) {
                return m_Textures2DCache[assetName];
            }

            Texture2DAsset asset = AssetsManagerInstance.GetManager().LoadAsset<Texture2DAsset>(assetName);
            if (asset.IsInvalid) {
                Debug.Log("AssetManager", "Texture " + assetName + " couldn't be loaded.");
                return DebugTexture;
            }
            Debug.Log("AssetManager", "Texture " + assetName + " loaded.");

            bool ForceNoMips = false;

            int stride = asset.Data.Width * 4;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(asset.Data.Buffer, 0);
            DataBox dataBox = new DataBox(ptr, stride, stride * asset.Data.Height);
            Texture2D texture2D = new Texture2D(
                m_RenderBackend.Device,
                new Texture2DDescription()
                {
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                    ArraySize = 1,
                    BindFlags = asset.Data.GetMips ? 
                        (BindFlags.ShaderResource | BindFlags.RenderTarget) : BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = asset.Data.GetFormat,
                    MipLevels = asset.Data.GetMips ? 0 : 1,
                    OptionFlags = asset.Data.GetMips ? 
                        ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }
            );
            texture2D.DebugName = assetName + "Texture";
            m_RenderBackend.Device.ImmediateContext.UpdateSubresource(dataBox,
                texture2D, Resource.CalculateSubResourceIndex(0, 0, 0));

            asset = null;
            m_Textures2DCache.Add(assetName, texture2D);

            return texture2D;
        }

        public Texture2D LoadCubeTextureAsset(string assetName, out TextureCubeAsset asset)
        {
            if (m_Textures2DCache.ContainsKey(assetName)) {
                asset = null;
                return m_Textures2DCache[assetName];
            }

            asset = AssetsManagerInstance.GetManager().LoadAsset<TextureCubeAsset>(assetName);
            if (asset.IsInvalid) {
                Debug.Log("AssetManager", "TextureCube " + assetName + " couldn't be loaded.");
                return DebugCubeTexture;
            }
            Debug.Log("AssetManager", "TextureCube " + assetName + " loaded.");

            Format format;
            switch (asset.Data.BytesPerChannel) {
                case 2:
                    format = Format.R16G16B16A16_Float;
                    break;
                case 4:
                    format = Format.R32G32B32A32_Float;
                    break;
                default:
                    format = Format.R8G8B8A8_UNorm;
                    if (asset.Data.ColorSpace == AssetsManager.Loaders.ColorSpaceEnum.Gamma) {
                        format = Format.R8G8B8A8_UNorm_SRgb;
                    }
                    break;
            }

            int stride = asset.Data.ChannelsCount * asset.Data.BytesPerChannel;
            int miplvls = asset.Data.MipLevels;

            DataBox[] initData = new DataBox[6 * miplvls];

            for (int mip = 0; mip < miplvls; mip++) {
                int mipSize = (int)(asset.Data.Width * Math.Pow(0.5, mip));
                for (int i = 0; i < 6; i++) {
                    byte[] bts = asset.Data.Buffer[i][mip];
                    IntPtr ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0);
                    var dataBox = new DataBox(ptr, stride * mipSize, stride * mipSize * mipSize);
                    initData[i * miplvls + mip] = dataBox;
                }
            }

            Texture2D cubeTexture = new Texture2D(
                m_RenderBackend.Device,
                new Texture2DDescription()
                {
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                    ArraySize = 6,
                    Format = format,
                    BindFlags = BindFlags.ShaderResource,
                    OptionFlags = ResourceOptionFlags.TextureCube,
                    SampleDescription = new SampleDescription(1, 0),
                    MipLevels = asset.Data.MipLevels,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                },
                initData
            );
            cubeTexture.DebugName = assetName + "Texture";

            asset = null;
            m_Textures2DCache.Add(assetName, cubeTexture);

            return cubeTexture;
        }

        public ShaderResourceView LoadTextureSRV(string assetName)
        {
            return LoadTextureSRV(assetName, false);
        }

        public ShaderResourceView LoadTextureSRV(string assetName, bool IsCubeMap)
        {
            if (m_TexturesCache.ContainsKey(assetName))
            {
                return m_TexturesCache[assetName];
            }
            // TODO: create some flag
            if (assetName.EndsWith("CubeMap"))
            {
                IsCubeMap = true;
            }

            TextureCubeAsset asset = null;
            Texture2D texture = IsCubeMap ? LoadCubeTextureAsset(assetName, out asset) : LoadTextureAsset(assetName);

            if (texture == null) { return null; }

            ShaderResourceViewDescription descSRV = new ShaderResourceViewDescription();

            descSRV.Format = texture.Description.Format;

            if (IsCubeMap) {
                descSRV.Dimension = ShaderResourceViewDimension.TextureCube;
                descSRV.TextureCube.MipLevels = texture.Description.MipLevels;
                descSRV.TextureCube.MostDetailedMip = 0;
            }  else {
                descSRV.Dimension = ShaderResourceViewDimension.Texture2D;
                descSRV.Texture2D.MipLevels = texture.Description.MipLevels == 0 ? - 1 : texture.Description.MipLevels;
                descSRV.Texture2D.MostDetailedMip = 0;
            }

            ShaderResourceView result;
            if (IsCubeMap) {
                result = new ShaderResourceView(m_RenderBackend.Device, texture, descSRV);
            } else {
                result = new ShaderResourceView(m_RenderBackend.Device, texture);
                m_RenderBackend.Context.GenerateMips(result);
            }
            result.DebugName = assetName + "SRV";

            m_TexturesCache.Add(assetName, result);
            return result;
        }

        public void DropCachedTexture(string assetName)
        {
            if (!m_Textures2DCache.ContainsKey(assetName)) {
                return;
            }
            m_Textures2DCache[assetName].Dispose();
            m_Textures2DCache.Remove(assetName);

            if (!m_TexturesCache.ContainsKey(assetName)) {
                return;
            }
            m_TexturesCache[assetName].Dispose();
            m_TexturesCache.Remove(assetName);
            Debug.Log("AssetManager", "Texture " + assetName + " unloaded.");
        }
    }
    #endregion

    #region Render targets
    internal partial class SharedRenderItemsStorage
    {
        public class RenderTargetPack : IDisposable
        {
            static private Texture2DDescription textureDescription = new Texture2DDescription()
            {
                MipLevels = 1,
                ArraySize = 1,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };

            static private ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MipLevels = 1,
                    MostDetailedMip = 0,
                }
            };

            static private RenderTargetViewDescription renderTargetDescription = new RenderTargetViewDescription()
            {
                Dimension = RenderTargetViewDimension.Texture2D,
            };

            public Format TargetFormat { get; private set; }

            public Texture2D Map { get; private set; }

            public ShaderResourceView ResourceView { get; private set; }

            public RenderTargetView View { get; private set; }

            private readonly string Name;
            private readonly int SamplesCount;

            public RenderTargetPack(string name, int samples)
            {
                Name = name;
                SamplesCount = samples;
            }

            public void Create(SharpDX.Direct3D11.Device device, int Width, int Height, Format format)
            {
                TargetFormat = format;

                Dispose();
                textureDescription.SampleDescription = new SampleDescription(SamplesCount, 0);

                textureDescription.Format = format;
                textureDescription.Width = Width;
                textureDescription.Height = Height;
                Map = new Texture2D(device, textureDescription);
                Map.DebugName = Name + "Map";

                shaderResourceDescription.Dimension = SamplesCount > 1 ?
                    ShaderResourceViewDimension.Texture2DMultisampled :
                    ShaderResourceViewDimension.Texture2D;

                shaderResourceDescription.Format = format;
                ResourceView = new ShaderResourceView(device, Map, shaderResourceDescription);
                ResourceView.DebugName = Name + "SRV";

                renderTargetDescription.Dimension = SamplesCount > 1 ?
                    RenderTargetViewDimension.Texture2DMultisampled :
                    RenderTargetViewDimension.Texture2D;
                renderTargetDescription.Format = format;
                View = new RenderTargetView(device, Map, renderTargetDescription);
                View.DebugName = Name + "RTV";
            }

            public void Resize(SharpDX.Direct3D11.Device device, int Width, int Height)
            {
                Create(device, Width, Height, TargetFormat);
            }

            public void Dispose()
            {
                Map?.Dispose();
                Map = null;
                ResourceView?.Dispose();
                ResourceView = null;
                View?.Dispose();
                View = null;
            }
        }

        private Dictionary<string, RenderTargetPack> m_RenderTargetPacks;

        public RenderTargetPack CreateRenderTarget(string Name, int Width, int Height, Format format, int samples)
        {
            if (m_RenderTargetPacks.ContainsKey(Name))
            {
                Debug.Warning("CreateRenderTarget", $"Target {Name} already exist.");
                return m_RenderTargetPacks[Name];
            }

            RenderTargetPack tmp = new RenderTargetPack(Name, samples);
            tmp.Create(m_RenderBackend.Device, Width, Height, format);
            m_RenderTargetPacks.Add(Name, tmp);
            return tmp;
        }

        public void ResizeRenderTarget(string Name, int Width, int Height)
        {
            if (!m_RenderTargetPacks.ContainsKey(Name))
            {
                Debug.Warning("ResizeRenderTarget", $"Target {Name} not exist.");
                return;
            }
            m_RenderTargetPacks[Name].Resize(m_RenderBackend.Device, Width, Height);
        }

        public class DepthStencilTargetPack : IDisposable
        {
            static private Texture2DDescription textureDescription = new Texture2DDescription()
            {
                Format = Format.R32G8X24_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            static private ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float_X8X24_Typeless,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                },
            };

            static private DepthStencilViewDescription DSViewDescription = new DepthStencilViewDescription
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            };

            public Texture2D Map { get; private set; }
            public ShaderResourceView ResourceView { get; private set; }
            public DepthStencilView View { get; private set; }

            private readonly string Name;
            private readonly int SamplesCount;

            public DepthStencilTargetPack(string name, int samples)
            {
                Name = name;
                SamplesCount = samples;
            }

            public void Create(SharpDX.Direct3D11.Device device, int Width, int Height)
            {
                Dispose();

                textureDescription.Width = Width;
                textureDescription.Height = Height;
                //textureDescription.Format = Format.R32_Typeless;
                textureDescription.SampleDescription = new SampleDescription(SamplesCount, 0);
                Map = new Texture2D(device, textureDescription);
                Map.DebugName = Name + "Map";
                    
                ResourceView?.Dispose();
                //shaderResourceDescription.Format = Format.R32_Float;
                shaderResourceDescription.Dimension = SamplesCount > 1 ? 
                    ShaderResourceViewDimension.Texture2DMultisampled : 
                    ShaderResourceViewDimension.Texture2D;
                ResourceView = new ShaderResourceView(device, Map, shaderResourceDescription);
                ResourceView.DebugName = Name + "SRV";

                View?.Dispose();
                //DSViewDescription.Format = Format.D32_Float;
                DSViewDescription.Dimension = SamplesCount > 1 ?
                    DepthStencilViewDimension.Texture2DMultisampled :
                    DepthStencilViewDimension.Texture2D;
                View = new DepthStencilView(device, Map, DSViewDescription);
                View.DebugName = Name + "RTV";
            }

            public void Resize(SharpDX.Direct3D11.Device device, int Width, int Height)
            {
                Create(device, Width, Height);
            }

            public void Dispose()
            {
                Map?.Dispose();
                Map = null;
                ResourceView?.Dispose();
                ResourceView = null;
                View?.Dispose();
                View = null;
            }
        }

        private Dictionary<string, DepthStencilTargetPack> m_DepthStencilTargetPacks;

        public DepthStencilTargetPack CreateDepthRenderTarget(string Name, int Width, int Height, int samples)
        {
            if (m_DepthStencilTargetPacks.ContainsKey(Name))
            {
                Debug.Warning("CreateDepthRenderTarget", $"Target {Name} already exist.");
                return m_DepthStencilTargetPacks[Name];
            }

            DepthStencilTargetPack tmp = new DepthStencilTargetPack(Name, samples);
            tmp.Create(m_RenderBackend.Device, Width, Height);
            m_DepthStencilTargetPacks.Add(Name, tmp);
            return tmp;
        }

        public void ResizeDepthRenderTarget(string Name, int Width, int Height)
        {
            if (!m_DepthStencilTargetPacks.ContainsKey(Name))
            {
                Debug.Warning("ResizeRenderTarget", $"Target {Name} not exist.");
                return;
            }
            m_DepthStencilTargetPacks[Name].Resize(m_RenderBackend.Device, Width, Height);
        }

        private void InitRenderTargets()
        {
            m_RenderTargetPacks = new Dictionary<string, RenderTargetPack>();
            m_DepthStencilTargetPacks = new Dictionary<string, DepthStencilTargetPack>();
        }

        private void DisposeRenderTargets()
        {
            foreach (var item in m_RenderTargetPacks)
            {
                item.Value.Dispose();
            }
            m_RenderTargetPacks.Clear();
            m_RenderTargetPacks = null;

            foreach (var item in m_DepthStencilTargetPacks)
            {
                item.Value.Dispose();
            }
            m_DepthStencilTargetPacks.Clear();
            m_DepthStencilTargetPacks = null;
        }
    }
    #endregion

    #region Base
    internal partial class SharedRenderItemsStorage
    {

        private D3D11RenderBackend m_RenderBackend;

        public SharedRenderItemsStorage(D3D11RenderBackend RenderBackend)
        {
            m_RenderBackend = RenderBackend;
            InitSamplers();
            InitDepthStencilStates();
            InitBlendStates();
            InitMeshesCache();
            InitRasterizerState();
            InitTexturesCache();
            InitInputLayouts();
            InitRenderTargets();
        }

        public void Dispose()
        {
            DisposeSamplers();
            DisposeInputLayouts();
            DisposeBlendStates();
            DisposeRasterizerState();
            DisposeMeshesCache();
            DisposeDepthStencilStates();
            DisposeRenderTargets();
            DisposeTexturesCache();
            m_RenderBackend = null;
        }
    }
    #endregion
}
