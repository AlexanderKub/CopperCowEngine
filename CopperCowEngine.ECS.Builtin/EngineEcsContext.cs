using System.Numerics;
using System.Windows.Forms;
using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.ECS.Builtin.Systems;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.ECS.Builtin
{
    public class EngineEcsContext : EcsContext
    {
        public EngineEcsContext(Engine engine)
        {
            GetSingletonComponent<EngineHolder>().Engine = engine;

            CreateSystem<InputSystem>();
            CreateSystem<FreeControlSystem>();
            CreateSystem<TrsToLocalToWorldSystem>();
            CreateSystem<TrsToLocalToParentSystem>();
            CreateSystem<LocalToParentSystem>();
            CreateSystem<CameraScreenAspectSystem>();
            CreateSystem<CameraSystem>();
            CreateSystem<RenderingSystem>();

            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.Init();
            
            engine.Input.OnKeyDown += input.KeyDown;
            engine.Input.OnKeyUp += input.KeyUp;
        }

        public Entity CreateRenderedEntity(MeshInfo mesh, MaterialInfo material, 
            Vector3 position, Quaternion rotation, float scale = 1f)
        {
            return CreateRenderedEntity(mesh, material, Entity.Null, position, rotation, scale);
        }

        public Entity CreateRenderedEntity(MeshInfo mesh, MaterialInfo material, Entity parent,
            Vector3 position, Quaternion rotation, float scale = 1f)
        {
            var entity = CreateEntity(typeof(LocalToWorld),
                typeof(Translation), typeof(Rotation), typeof(Scale),
                typeof(Mesh), typeof(Material));

            ref var translationData = ref GetComponent<Translation>(entity);
            translationData.Value = position;
            ref var rotationData = ref GetComponent<Rotation>(entity);
            rotationData.Value = rotation;
            ref var scaleData = ref GetComponent<Scale>(entity);
            scaleData.Value = scale;

            if (!parent.Equals(Entity.Null))
            {
                AddComponent(entity, new LocalToParent());
                AddComponent(entity, new Parent { Value = parent });
            }

            ref var meshData = ref GetComponent<Mesh>(entity);
            meshData.Name = mesh.Name;
            meshData.Bounds = mesh.Bounds;

            ref var materialData = ref GetComponent<Material>(entity);
            materialData.Name = material.Name;
            materialData.Queue = material.Queue;

            return entity;
        }

        public Entity CreateTransformEntity(Vector3 position, Quaternion rotation, float scale = 1f)
        {
            return CreateTransformEntity(Entity.Null, position, rotation, scale);
        }

        public Entity CreateTransformEntity(Entity parent, Vector3 position, Quaternion rotation, float scale = 1f)
        {
            var translation = new Translation { Value = position };
            var rotationData = new Rotation { Value = rotation };
            var scaleData = new Scale { Value = scale };

            if (parent.Equals(Entity.Null))
            {
                return CreateEntity(new LocalToWorld(), translation, rotationData, scaleData);
            }

            return CreateEntity(new LocalToWorld(), new LocalToParent(), new Parent { Value = parent }, 
                translation, rotationData, scaleData);
        }

        public Entity CreateCameraEntity(CameraSetup cameraSetupData)
        {
            return CreateCameraEntity(cameraSetupData, Vector3.Zero, Quaternion.Identity);
        }

        public Entity CreateCameraEntity(CameraSetup cameraSetupData, Vector3 position)
        {
            return CreateCameraEntity(cameraSetupData, position, Quaternion.Identity);
        }

        public Entity CreateCameraEntity(CameraSetup cameraSetupData, Quaternion rotation)
        {
            return CreateCameraEntity(cameraSetupData, Vector3.Zero, rotation);
        }

        public Entity CreateCameraEntity(CameraSetup cameraSetupData, Vector3 position, Quaternion rotation)
        {
            var translation = new Translation { Value = position };

            var rotationData = new Rotation { Value = rotation };

            return CreateEntity(new LocalToWorld(), translation, rotationData, cameraSetupData , 
                new CameraProjection(), new CameraViewProjection(), new CameraScreenAspect());
        }
        
        protected override void Dispose(bool disposing)
        {
            var engine = GetSingletonComponent<EngineHolder>().Engine;

            engine.Input.OnKeyDown -= KeyDown;
            engine.Input.OnKeyUp -= KeyUp;

            base.Dispose(disposing);
        }

        private void KeyDown(Keys keys)
        {
            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.KeyDown(keys);
        }

        private void KeyUp(Keys keys)
        {
            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.KeyUp(keys);
        }
    }
}
