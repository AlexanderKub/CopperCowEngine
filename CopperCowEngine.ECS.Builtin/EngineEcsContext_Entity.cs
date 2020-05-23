using System.Diagnostics.CodeAnalysis;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.Rendering.Loaders;
using System.Numerics;
using CopperCowEngine.ECS.Builtin.Extensions;

namespace CopperCowEngine.ECS.Builtin
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class EngineEcsContext
    {
        public Entity CreateRenderedEntity(MeshInfo mesh, MaterialInfo material, 
            Vector3 position, Quaternion rotation, float scale = 1f)
        {
            return CreateRenderedEntity(mesh, material, Entity.Null, position, rotation, scale);
        }
        
        public Entity CreateRenderedEntity(MeshInfo mesh, MaterialInfo material, Entity parent,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var entity = CreateEntityWithoutData(typeof(LocalToWorld),
                typeof(Translation), typeof(Rotation), typeof(NonUniformScale),
                typeof(Mesh), typeof(Material));

            ref var translationData = ref GetComponent<Translation>(entity);
            translationData.Value = position;
            ref var rotationData = ref GetComponent<Rotation>(entity);
            rotationData.Value = rotation;
            ref var scaleData = ref GetComponent<NonUniformScale>(entity);
            scaleData.Value = scale;

            if (!parent.Equals(Entity.Null))
            {
                AddComponent(entity, new LocalToParent());
                AddComponent(entity, new Parent { Value = parent });
            }

            GetComponent<Mesh>(entity) = mesh.CreateMesh();
            GetComponent<Material>(entity) = material.CreateMaterial();

            return entity;
        }

        public Entity CreateRenderedEntity(MeshInfo mesh, MaterialInfo material, Entity parent,
            Vector3 position, Quaternion rotation, float scale = 1f)
        {
            var entity = CreateEntityWithoutData(typeof(LocalToWorld),
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

            GetComponent<Mesh>(entity) = mesh.CreateMesh();
            GetComponent<Material>(entity) = material.CreateMaterial();

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
        
        public Entity CreateLightEntity(DirectionalLight light, Vector3 position)
        {
            var translation = new Translation { Value = position };

            return CreateEntity(translation, light);
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
    }
}
