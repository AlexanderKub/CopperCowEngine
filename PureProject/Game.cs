using System;
using System.Numerics;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Core;
using CopperCowEngine.Engine;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.ShaderGraph;
using CopperCowEngine.ScriptEngine.Lua;

namespace PureProject
{
    internal class Game
    {
        private readonly Engine _engine;

        private readonly EngineEcsContext _ecsContext;

        private MeshInfo _meshInfo;

        private MaterialInfo _materialInfo;

        public Game()
        {
            ShaderCompiler.CompileShaders();
            var renderingOption = new RenderingOptions<D3D11RenderBackend>(
                new RenderingConfiguration
                {
                    AppName = "PureProject",
                    //RenderPath = RenderPathType.Deferred,
                    RenderPath = RenderPathType.Forward,
                    DebugMode = true,
                    //EnableMsaa = MsaaEnabled.Off,
                    EnableMsaa = MsaaEnabled.X8,
                    EnableHdr = true,
                    PostProcessing = new PostProcessingConfiguration
                    {
                        Bloom = new BloomSettings(true),
                        MotionBlur = new MotionBlurSettings(true)
                    },
                }, false, false);

            var loopProvider = new DefaultEngineLoopProvider();
            loopProvider.OnUpdate += OnUpdate;
            loopProvider.OnStart += OnEngineStart;
            loopProvider.OnQuit += OnEngineQuit;

            var scriptingOption = new ScriptEngineOptions<LuaScriptEngine>();

            var configuration = new EngineConfiguration
            {
                Rendering = renderingOption,
                ScriptEngine = scriptingOption,
                EngineLoopProvider = loopProvider,
            };

            _engine = new Engine(configuration);
            _ecsContext = new EngineEcsContext(_engine);
            _engine.Bootstrap();
        }

        private void OnEngineStart()
        {
            var cowMesh = MeshAssetsLoader.LoadMesh("CowMesh");
            var cubeMeshInfo = MeshAssetsLoader.GetMeshInfo(PrimitivesMesh.Cube);
            var sphereMeshInfo = MeshAssetsLoader.GetMeshInfo(PrimitivesMesh.Sphere);
            _meshInfo = MeshAssetsLoader.GetMeshInfo(cowMesh);
            _meshInfo = sphereMeshInfo;

            var copperMaterial = MaterialLoader.LoadMaterial("CopperMaterial");
            var splotchyMaterial = MaterialLoader.LoadMaterial("MetalSplotchyMaterial");
            var snowMaterial = MaterialLoader.LoadMaterial("SnowRockMaterial");

            _materialInfo = MaterialLoader.GetMaterialInfo(copperMaterial);

            //_ecsContext.CreateSystem<TranslationSystem>(SystemOrderGroup.Simulation);
            _ecsContext.CreateSystem<LightDebugSystem>(SystemOrderGroup.Simulation);

            var cameraEntity = _ecsContext.CreateCameraEntity(CameraSetup.Default);
            _ecsContext.AddComponent(cameraEntity, new FreeControl
            {
                Speed = 2f,
            });

            var subRoot = _ecsContext.CreateTransformEntity(Vector3.UnitY, Quaternion.Identity);
            var root = _ecsContext.CreateTransformEntity(Vector3.Zero, Quaternion.Identity);
            _ecsContext.AddComponent(subRoot, new LocalToParent());
            _ecsContext.AddComponent(subRoot, new Parent { Value = root });

            var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI);

            var grandRoot = _ecsContext.CreateTransformEntity(Vector3.UnitZ * 10f - Vector3.UnitY * 4f, Quaternion.Identity);
            _ecsContext.AddComponent(root, new LocalToParent());
            _ecsContext.AddComponent(root, new Parent { Value = grandRoot });


            var matMeta = new MaterialMeta
            {
                BlendMode = MaterialMeta.BlendModeType.Opaque,
                CullMode = MaterialMeta.CullModeType.Back,
                MaterialDomain = MaterialMeta.MaterialDomainType.Surface,
                ShadingMode = MaterialMeta.ShadingModeType.Default,
                Wireframe = false,
                OpacityMaskClipValue = 0f,
            };

            var roughs = new [] { 0.0001f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
            var metalnes = new [] { 0.0001f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

            var scaleVector = Vector3.One;//new Vector3(0.85f, 0.5f, 0.5f);

            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    var rough = roughs[j];
                    var metal = metalnes[i];
                    var matInstance = new MaterialInstance(matMeta)
                    {
                        Name = $"PBRTestMaterial_r_{rough}_m_{metal}",
                        PropertyBlock = new MaterialPropertyBlock
                        {
                            AlbedoColor = Vector3.One * 0.89f,
                            MetallicValue = metal,
                            RoughnessValue = rough,
                            AlphaValue = 1f,
                            Shift = Vector2.Zero,
                            Tile = Vector2.One,
                        }
                    };
                    var matInfo = MaterialLoader.GetMaterialInfo(matInstance);
                    _ecsContext.CreateRenderedEntity(_meshInfo, matInfo, subRoot, 
                        new Vector3((i - 3) * 1.75f, 1.8f * j, 1.75f * (j - 3)), rotationQuaternion, scaleVector);
                }
            }

            var translucentMaterial = new MaterialInstance(new MaterialMeta
                {
                    BlendMode = MaterialMeta.BlendModeType.Translucent,
                    CullMode = MaterialMeta.CullModeType.None,
                })
            {
                Name = "PBRTestMaterial_Translucent",
                PropertyBlock = new MaterialPropertyBlock
                {
                    AlphaValue = 0.49f,
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One * 3.5f,
                }
            };

            var maskedMaterial = new MaterialInstance(new MaterialMeta
            {
                BlendMode = MaterialMeta.BlendModeType.Masked,
                CullMode = MaterialMeta.CullModeType.None,
                OpacityMaskClipValue = 0.33f,
            })
            {
                Name = "PBRTestMaterial_Masked",
                AlbedoMapAsset = "WoodenLatticeMap",
                PropertyBlock = new MaterialPropertyBlock
                {
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One * 3.5f,
                }
            };

            var wireframeMaterial = new MaterialInstance(new MaterialMeta
            {
                BlendMode = MaterialMeta.BlendModeType.Translucent,
                CullMode = MaterialMeta.CullModeType.None,
                Wireframe = true,
            })
            {
                Name = "PBRTestMaterial_Wireframe",
                PropertyBlock = new MaterialPropertyBlock
                {
                    AlbedoColor = Vector3.UnitX,
                    AlphaValue = 0.49f,
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One * 3.5f,
                }
            };

            var mats = new[]
            {
                MaterialLoader.GetMaterialInfo(splotchyMaterial),
                MaterialLoader.GetMaterialInfo(snowMaterial),
                MaterialLoader.GetMaterialInfo(translucentMaterial),
                MaterialLoader.GetMaterialInfo(maskedMaterial),
                MaterialLoader.GetMaterialInfo(wireframeMaterial),
                MaterialLoader.GetMaterialInfo(copperMaterial),
            };
            
            for (var j = 0; j < 6; j++) 
            {
                _ecsContext.CreateRenderedEntity(_meshInfo, mats[j % mats.Length], subRoot,
                    Vector3.UnitX * (6 - 3) * 1.75f + Vector3.UnitY * j + Vector3.UnitZ * 1.75f * (j - 3) + Vector3.UnitY * 0.8f * j, 
                    rotationQuaternion, scaleVector);
            }

            _ecsContext.CreateRenderedEntity(cubeMeshInfo, mats[0], subRoot,
                Vector3.UnitZ * 1f + Vector3.UnitY * 4f, 
                Quaternion.CreateFromYawPitchRoll(0, MathF.PI * 0.25f, 0), new Vector3(17f, 17f, 0.5f));

            //DebugDataChunks.View(_ecsContext);
        }

        private void OnUpdate()
        {
            _ecsContext.Update();
        }

        private void OnEngineQuit()
        {
            _ecsContext.Dispose();
        }

        private class TranslationSystem : ComponentSystem<Required<Translation, Rotation>, Optional, Excepted<Parent, CameraSetup, DirectionalLight>>
        {
            protected override void Update()
            {
                foreach (var e in Iterator)
                {
                    ref var rotation = ref e.Sibling<Rotation>();
                    rotation.Value = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Time.Current * 0.5f);

                    ref var translation = ref e.Sibling<Translation>();
                    translation.Value += Vector3.UnitZ * (float)Math.Sin(Time.Current * 0.5f) * Time.Delta;
                }
            }
        }
    }
}
