using System;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Core;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using SharpDX;

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
            var renderingOption = new RenderingOption<D3D11RenderBackend>(
                new RenderingConfiguration
                {
                    AppName = "PureProject",
                    RenderPath = RenderPathType.Deferred,
                    DebugMode = true,
                    EnableMsaa = MsaaEnabled.Off,
                    EnableHdr = true,
                }, false, false);

            var configuration = new EngineConfiguration()
            {
                Rendering = renderingOption,
            };

            _engine = new Engine(configuration);
            _engine.OnBootstrapped += OnEngineStart;
            _engine.OnBeforeFrame += OnBeforeFrame;
            _engine.OnAfterFrame += OnAfterFrame;

            _ecsContext = new EngineEcsContext(_engine);

            _engine.Bootstrap();
        }

        private void OnEngineStart()
        {
            _meshInfo = MeshAssetsLoader.LoadMeshInfo("CowMesh");//PrimitivesMesh.Sphere
            _materialInfo = MaterialLoader.LoadMaterialInfo("CopperMaterial");

            _ecsContext.CreateSystem<TranslationSystem>();

            _ecsContext.CreateCameraEntity(CameraSetup.Default);

            var subRoot = _ecsContext.CreateTransformEntity(Vector3.Up, Quaternion.Identity);
            var root = _ecsContext.CreateTransformEntity(Vector3.Zero, Quaternion.Identity);
            _ecsContext.AddComponent(subRoot, new LocalToParent());
            _ecsContext.AddComponent(subRoot, new Parent { Value = root });

            var nOverTwo = 3;

            for (var i = -nOverTwo; i < nOverTwo - 1; i++)
            {
                for (var j = -nOverTwo; j < nOverTwo - 1; j++)
                {
                    _ecsContext.CreateRenderedEntity(_meshInfo, _materialInfo, subRoot,
                        Vector3.Left * (i + 1) + Vector3.Up * j + Vector3.ForwardLH * (Math.Abs(i + 1) / (float)(nOverTwo)) * 1.5f, 
                        Quaternion.Identity, 0.5f);
                }
            }

            var grandRoot = _ecsContext.CreateTransformEntity(Vector3.ForwardLH * 3f, Quaternion.Identity);
            _ecsContext.AddComponent(root, new LocalToParent());
            _ecsContext.AddComponent(root, new Parent { Value = grandRoot });

            //DebugDataChunks.View(_ecsContext);
        }

        private void OnBeforeFrame()
        {
            _ecsContext.Update();
        }

        private void OnAfterFrame()
        {
            _engine.RenderingFrameData.Reset();
        }

        public class TranslationSystem : ComponentSystem<Required<Translation, Rotation>, Optional, Excepted<Parent, CameraSetup>>
        {
            protected override void Update()
            {
                foreach (var e in Iterator)
                {
                    ref var rotation = ref e.Sibling<Rotation>();
                    rotation.Value = Quaternion.RotationAxis(Vector3.Up, Time.Current * 0.5f);

                    ref var translation = ref e.Sibling<Translation>();
                    translation.Value += Vector3.ForwardLH * (float)Math.Sin(Time.Current * 0.5f) * Time.Delta;
                }
            }
        }
    }
}
