using System;
using SharpDX;
using EngineCore;
using EngineCore.ECS;
using EngineCore.ECS.Components;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECSTestProject
{
    public class PBRTest : Engine
    {
        private TestSystem testSystem;
        enum MeshTypeEnum {
            Sphere,
            Cow,
        }
        private MeshTypeEnum CurrenMesh = MeshTypeEnum.Cow;

        protected override void OnStart()
        {
            testSystem = ECSWorld.AddSystem<TestSystem>();

            Entity entity;
            Light light;

            entity = ECSWorld.CreateEntityWith<Transform, Camera>("MainCamera");
            entity.GetComponent<Transform>().Position = -Vector3.ForwardLH * 10f + Vector3.Up * 3.5f;
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Light>("DirLight");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Up * 20 - Vector3.Right * 18, 
                Quaternion.RotationYawPitchRoll(0, -MathUtil.PiOverTwo, 0));
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Directional;
            light.Intensity = 5f;
            light.IsCastShadows = true;
            entity.AddComponent<TestComponent>();
            entity.AddComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube), 
                AssetsLoader.LoadMaterialInfo(Material.DefaultMaterial));
            //entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Light>("PointLight");
            entity.GetComponent<Transform>().SetTransformations(-Vector3.ForwardLH * 2 + Vector3.Up * 2,
                Quaternion.Identity, Vector3.One * 0.25f);
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Capsule;
            light.Type = Light.LightType.Spot;
            light.Type = Light.LightType.Point;
            light.Intensity = 20f;
            light.Radius = 5f;
            entity.AddComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("DefaultMaterial"));
            entity.AddComponent<TestComponent>();
            //entity.SetActive(true);

            EngineCore.ShaderGraph.MetaMaterial opaqueMeta = new EngineCore.ShaderGraph.MetaMaterial()
            {
                blendMode = EngineCore.ShaderGraph.MetaMaterial.BlendMode.Opaque,
                cullMode = EngineCore.ShaderGraph.MetaMaterial.CullMode.Back,
            };

            Material mat;
            Quaternion rot = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);

            AssetsLoader.MeshInfo meshInfo = AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Sphere);
            switch (CurrenMesh) {
                case MeshTypeEnum.Cow:
                    meshInfo = AssetsLoader.LoadMeshInfo("CowMesh");
                    break;
            }

            float[] roughs = new float[6] { 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
            float[] metalnes = new float[6] { 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    mat = new Material(opaqueMeta)
                    {
                        Name = "M_Test" + i + "_" + j,
                        PropetyBlock = new MaterialPropetyBlock()
                        {
                            AlbedoColor = Vector3.One * 1.0f,
                            MetallicValue = metalnes[i],
                            RoughnessValue = roughs[j],
                        },
                    };
                    entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMesh" + i);
                    entity.GetComponent<Transform>().SetTransformations(Vector3.Right * (i - 3) * 1.75f + Vector3.ForwardLH * 1.75f * (j - 3) + Vector3.Up * 0.8f * j, rot);
                    entity.GetComponent<Renderer>().SetMeshAndMaterial(meshInfo, AssetsLoader.LoadMaterialInfo(mat));
                    entity.SetActive(true);
                }
            }

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMeshWithMat1");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 1.75f * (-4) + Vector3.ForwardLH * 1.75f * (-3), rot);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(meshInfo, AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMeshWithMat2");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 1.75f * (-4) + Vector3.ForwardLH * 1.75f * (-2) + Vector3.Up * 0.8f * 1, rot);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(meshInfo, AssetsLoader.LoadMaterialInfo("MetalSplotchyMaterial"));
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMeshWithMat3");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 1.75f * (-4) + Vector3.ForwardLH * 1.75f * (-1) + Vector3.Up * 0.8f * 2, rot);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(meshInfo, AssetsLoader.LoadMaterialInfo("SnowRockMaterial"));
            entity.SetActive(true);

            mat = new Material(new EngineCore.ShaderGraph.MetaMaterial()
            {
                blendMode = EngineCore.ShaderGraph.MetaMaterial.BlendMode.Translucent,
                cullMode = EngineCore.ShaderGraph.MetaMaterial.CullMode.Back,
                //Wireframe = true,
            })
            {
                Name = "M_Test",
                AlbedoMapAsset = "CowAlbedoMap",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    AlphaValue = 0.49f,
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One * 3.5f,
                },
            };
            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMeshWithMat4");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 1.75f * (-4) + Vector3.ForwardLH * 1.75f * (0) + Vector3.Up * 0.8f * 3, rot);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(meshInfo, AssetsLoader.LoadMaterialInfo(mat));
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("TestMeshWithMat5");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Up * 1.25f + Vector3.ForwardLH * 1.25f + Vector3.Left * 1.5f, 
                Quaternion.RotationYawPitchRoll(0, -MathUtil.PiOverFour * 0.5f, 0), new Vector3(15, 0.5f, 15));
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube), 
                AssetsLoader.LoadMaterialInfo("SnowRockMaterial"));
            entity.SetActive(true);

            /*entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Zero, Quaternion.Identity, Vector3.One * 10);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(
                AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube1");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Up * 10, Quaternion.Identity, Vector3.One);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(
                AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube2");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 10, Quaternion.Identity, Vector3.One);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(
                AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube3");
            entity.GetComponent<Transform>().SetTransformations(Vector3.ForwardLH * 10, Quaternion.Identity, Vector3.One);
            entity.GetComponent<Renderer>().SetMeshAndMaterial(
                AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);*/

            ECSWorld.Refresh();
        }

        protected override void Update(Timer timer)
        {
            testSystem.Update(timer);
        }

        protected override void OnQuit()
        {

        }
    }
}
