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
    public class Game : Engine
    {
        private TestSystem testSystem;
        protected override void OnStart()
        {
            testSystem = ECSWorld.AddSystem<TestSystem>();

            Entity entity;
            Transform transform;
            Renderer renderer;
            Light light;
            Random rnd = new Random();

            entity = ECSWorld.CreateEntityWith<Transform, Camera>("MainCamera");
            entity.GetComponent<Transform>().Position = -Vector3.ForwardLH * 2f;
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Light>("DirLight");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Up * 200 - Vector3.Right * 183, 
                Quaternion.RotationYawPitchRoll(MathUtil.PiOverTwo, MathUtil.PiOverFour, 0));
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Directional;
            light.Intensity = 0.5f;
            light.IsCastShadows = true;
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            /*entity = ECSWorld.CreateEntityWith<Transform, Light>("PointLight");
            entity.GetComponent<Transform>().Position = Vector3.Right * 8 - Vector3.Up;
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Point;
            light.Radius = 3f;
            light.Intensity = 1.0f;
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Light>("CapsuleLight");
            entity.GetComponent<Transform>().Position = Vector3.Zero - Vector3.Up;
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Capsule;
            light.Radius = 3f;
            light.Intensity = 1.0f;
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Light>("SpotLight");
            entity.GetComponent<Transform>().Position = -Vector3.Right * 8 - Vector3.Up;
            light = entity.GetComponent<Light>();
            light.Type = Light.LightType.Spot;
            light.Radius = 3f;
            light.Intensity = 1.0f;
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);*/

            /*Vector3 tmp = new Vector3(40, 0, 40);
            for (int i = 0; i < 500; i++)
            {
                entity = ECSWorld.CreateEntityWith<Transform, Light>($"PointLight_{i}");

                transform = entity.GetComponent<Transform>();
                transform.Position = Vector3.Up * 3f + RandomUtil.NextVector3(rnd, -tmp, tmp + Vector3.Up * 20f);
                transform.Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.PiOverTwo, 0);

                light = entity.GetComponent<Light>();
                light.Radius = RandomUtil.NextFloat(rnd, 2f, 8f);
                light.Color = RandomUtil.NextVector3(rnd, Vector3.One * 0.2f, Vector3.One);
                light.Type = Light.LightType.Point;

                entity.AddComponent<TestComponent>();
                entity.SetActive(true);
            }*/

            Quaternion rot = Quaternion.RotationYawPitchRoll(0, -MathUtil.PiOverTwo, 0);
            for (int i = 0; i < 382; i++)//382
            {
                entity = ECSWorld.CreateEntityWith<Transform, Renderer>($"SponzaMesh_{i}");
                entity.GetComponent<Transform>().Rotation = rot;

                renderer = entity.GetComponent<Renderer>();
                renderer.SetMesh(AssetsLoader.LoadMeshInfo(i == 0 ? "SponzaMesh" : "SponzaMesh_" + i));

                //RETARD STYLE BINDING
                if (i == 3)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFabricRedMaterial"));
                }
                else if (i >= 8 && i <= 15)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaColumnCMaterial"));
                }
                else if (i == 16)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaArchMaterial"));
                }
                else if (i == 380)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaRoofMaterial"));
                }
                else if (i == 116)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFloorMaterial"));
                }
                else if (i >= 117 && i <= 120)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaColumnAMaterial"));
                }
                else if (i >= 281 && i <= 288)
                {
                    if (i == 281 || i == 288)
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFabricRedMaterial"));
                    }
                    else if ((i >= 282 && i < 284) || i == 286 || i == 287)
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFabricBlueMaterial"));
                    }
                    else
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFabricGreenMaterial"));
                    }
                }
                else if (i >= 376 && i <= 377)
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("LionMaterial"));
                }
                else if (i >= 319 && i <= 329)
                {
                    if (i == 319 || i == 326)
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaCurtainBlueMaterial"));
                    }
                    else if ((i >= 321 && i < 324) || i == 327)
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaCurtainRedMaterial"));
                    }
                    else
                    {
                        renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaCurtainGreenMaterial"));
                    }
                }
                else
                {
                    renderer.SetMaterial(AssetsLoader.LoadMaterialInfo("SponzaFloorMaterial"));
                }
                
                entity.SetActive(true);
            }

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cow");
            entity.GetComponent<Transform>().Position = Vector3.Zero + Vector3.One * 2f;
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo("CowMesh"), 
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube");
            entity.GetComponent<Transform>().SetTransformations(Vector3.Right * 5f + Vector3.Up * 50f, RandomQuat(rnd));
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cow1");
            entity.GetComponent<Transform>().SetTransformations(Vector3.ForwardLH * 5f + Vector3.One * 2f, RandomQuat(rnd));
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo("CowMesh"),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube1");
            entity.GetComponent<Transform>().SetTransformations(-Vector3.Right * 5f + Vector3.One * 2f, RandomQuat(rnd));
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo("CopperMaterial"));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);


            Material mat = new Material(new EngineCore.ShaderGraph.MetaMaterial()
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
            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Sphere");
            entity.GetComponent<Transform>().Position = Vector3.Up * 2f;
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Sphere),
                AssetsLoader.LoadMaterialInfo(mat));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);
            
            mat = new Material(new EngineCore.ShaderGraph.MetaMaterial()
            {
                blendMode = EngineCore.ShaderGraph.MetaMaterial.BlendMode.Masked,
                cullMode = EngineCore.ShaderGraph.MetaMaterial.CullMode.None,
                //Wireframe = true,
            }) {
                Name = "M_TestAlpha",
                AlbedoMapAsset = "WoodenLatticeMap",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                },
            };
            entity = ECSWorld.CreateEntityWith<Transform, Renderer>("Cube2");
            entity.GetComponent<Transform>().Position = Vector3.Up;
            entity.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo(mat));
            entity.AddComponent<TestComponent>();
            entity.SetActive(true);

            ECSWorld.Refresh();
        }

        private Quaternion RandomQuat(Random rnd)
        {
            return Quaternion.RotationYawPitchRoll(
                    RandomUtil.NextFloat(rnd, -MathUtil.Pi, MathUtil.Pi),
                    RandomUtil.NextFloat(rnd, -MathUtil.Pi, MathUtil.Pi),
                    RandomUtil.NextFloat(rnd, -MathUtil.Pi, MathUtil.Pi));
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
