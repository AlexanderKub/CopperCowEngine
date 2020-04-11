using System;
using System.Numerics;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Core;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin;
using CopperCowEngine.ECS.Builtin.Components;

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
            _engine.OnQuit += OnEngineQuit;

            _ecsContext = new EngineEcsContext(_engine);

            _engine.Bootstrap();
        }

        private void OnEngineStart()
        {
            //_meshInfo = MeshAssetsLoader.LoadMeshInfo(PrimitivesMesh.Sphere);
            _meshInfo = MeshAssetsLoader.LoadMeshInfo("CowMesh");
            _materialInfo = MaterialLoader.LoadMaterialInfo("CopperMaterial");

            //_ecsContext.CreateSystem<TranslationSystem>();

           var cameraEntity =  _ecsContext.CreateCameraEntity(CameraSetup.Default);
            _ecsContext.AddComponent(cameraEntity, new FreeControl
            {
                Speed = 2f,
            });

            var subRoot = _ecsContext.CreateTransformEntity(Vector3.UnitY, Quaternion.Identity);
            var root = _ecsContext.CreateTransformEntity(Vector3.Zero, Quaternion.Identity);
            _ecsContext.AddComponent(subRoot, new LocalToParent());
            _ecsContext.AddComponent(subRoot, new Parent { Value = root });

            var nOverTwo = 3;

            var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI);

            for (var i = -nOverTwo; i < nOverTwo - 1; i++)
            {
                for (var j = -nOverTwo; j < nOverTwo - 1; j++)
                {
                    _ecsContext.CreateRenderedEntity(_meshInfo, _materialInfo, subRoot,
                        -Vector3.UnitX * (i + 1) + Vector3.UnitY * j + Vector3.UnitZ * (Math.Abs(i + 1) / (float)(nOverTwo)) * 1.5f, 
                        rotationQuaternion, 0.5f);
                }
            }

            var grandRoot = _ecsContext.CreateTransformEntity(Vector3.UnitZ * 3f, Quaternion.Identity);
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

        private void OnEngineQuit()
        {
            _ecsContext.Dispose();
        }

        public class TranslationSystem : ComponentSystem<Required<Translation, Rotation>, Optional, Excepted<Parent, CameraSetup>>
        {
            protected override void Update()
            {
                foreach (var e in Iterator)
                {
                    ref var rotation = ref e.Sibling<Rotation>();
                    rotation.Value = Quaternion.CreateFromAxisAngle(Vector3.UnitY, Time.Current * 0.5f);

                    ref var translation = ref e.Sibling<Translation>();
                    translation.Value += Vector3.UnitZ * (float)Math.Sin(Time.Current * 0.5f) * Time.Delta;
                }
            }
        }
    }
}
