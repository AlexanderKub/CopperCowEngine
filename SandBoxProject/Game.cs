using System;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;

namespace SandBoxProject
{
    public class Game : Engine
    {
        Light LightObj;

        public Game(string name):base(name) { }

        public override void LoadMaterials()
        {
            Material mat;
            mat = new Material()
            {
                Name = "M_Cow",
                AlbedoMapAsset = "CowAlbedoMap",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One * 3.5f,
                },
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Cat",
                AlbedoMapAsset = "CatAlbedoMap",
                PropetyBlock = new MaterialPropetyBlock() {
                    Tile = Vector2.One * 3.5f,
                }
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_RockFloor",
                AlbedoMapAsset = "RockFloorAlbedoMap",
                NormalMapAsset = "RockFloorNormalMap",
                RoughnessMapAsset = "RockFloorRoughnessMap",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    MetallicValue = 0.1f,
                    Tile = Vector2.One * 16,
                },
                OcclusionMapAsset = "RockFloorOcclusionMap",
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Stone",
                AlbedoMapAsset = "RockAlbedoMap",
                NormalMapAsset = "RockNormalMap",
                OcclusionMapAsset = "RockOcclusionMap",
                PropetyBlock = new MaterialPropetyBlock() {
                    RoughnessValue = 0.8f,
                    MetallicValue = 0.1f,
                },
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Soldier",
                AlbedoMapAsset = "SoldierAlbedoMap",
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Metal",
                AlbedoMapAsset = "MetalAlbedoMap",
                NormalMapAsset = "MetalNormalMap",
                RoughnessMapAsset = "MetalRoughnessMap",
                MetallicMapAsset= "MetalMetallicMap",
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_SnowRock",
                AlbedoMapAsset = "SnowRockAlbedoMap",
                NormalMapAsset = "SnowRockNormalMap",
                RoughnessMapAsset = "SnowRockRoughnessMap",
                MetallicMapAsset = "SnowRockMetallicMap",
                OcclusionMapAsset = "SnowRockOcclusionMap",
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Copper",
                AlbedoMapAsset = "CopperAlbedoMap",
                NormalMapAsset = "CopperNormalMap",
                RoughnessMapAsset = "CopperRoughnessMap",
                MetallicMapAsset = "CopperMetallicMap",
            };
            MaterialTable.Add(mat.Name, mat);

            mat = new Material()
            {
                Name = "M_Test",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    AlbedoColor = Vector3.Right * 0.5f,
                    MetallicValue = 0.0f,
                    RoughnessValue = 0.0f,
                }
            };
            MaterialTable.Add(mat.Name, mat);

            foreach (string key in MaterialTable.Keys)
            {
                MaterialTable[key].LoadMapsAndInitSampler();
            }
        }

        public override void OnStart() {
            ClearColor = Color.Blue;
            CreateMap();
            AddTestScene();
        }

        private void CreateMap()
        {
            CreateSkySphere();
            SetMainCamera(AddCamera<FreeCamera>("MainCamera", new Vector3(0f, 5f, -10f), Quaternion.Identity));

            LightObj = new Light()
            {
                ambientColor = Vector4.One * 0.25f,
                radius = 20,
                Type = Light.LightType.Directional,
                EnableShadows = true,
            };
            AddLight("Light", LightObj, new Vector3(0f, 5.5f, 0.1f), 
                Quaternion.RotationYawPitchRoll(-(float)Math.PI * 0.5f, -(float)Math.PI * 0.5f, 0), true);

            AddGameObject(
                "ReflectionSphere",
                new Transform()
                {
                    Position = Vector3.Up * 3.5f,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                },
                new Renderer()
                {
                    Geometry = Primitives.Sphere(30),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = Material.GetSkySphereMaterial(),
                    SpecificType = Renderer.SpecificTypeEnum.ReflectionSphere,
                }
            );

            Vector3 TestColor = new Vector3(0.47f, 0.78f, 0.73f);
            TestColor = Vector3.One * 0.84f;

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    AddGameObject(
                        "Sphere0_" + i,
                        new Transform()
                        {
                            Position = Vector3.Up * 9.5f + Vector3.Left * 1.5f * (i + 1) + Vector3.ForwardLH * 1.5f * (j + 1),
                            Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0),
                            Scale = Vector3.One,
                        },
                        new Renderer()
                        {
                            Geometry = Primitives.Sphere(32),
                            Topology = PrimitiveTopology.TriangleList,
                            RendererMaterial = GetMaterial("M_Test"),
                            CustomPropertyBlock = new MaterialPropetyBlock()
                            {
                                AlbedoColor = TestColor,
                                RoughnessValue = 0.0f + 0.1666f * i,
                                MetallicValue = 0.0f + 0.1666f * j,
                            },
                        }
                    );
                }
            }

            AddGameObject(
                "Sphere1",
                new Transform()
                {
                    Position = Vector3.Up * 8f,
                    Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0),
                    Scale = Vector3.One,
                },
                new Renderer()
                {
                    Geometry = Primitives.Sphere(30),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Copper"),
                }
            );

            AddGameObject(
                "Sphere2",
                new Transform()
                {
                    Position = Vector3.Up * 6.5f,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                },
                new Renderer()
                {
                    Geometry = Primitives.Sphere(30),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Metal"),
                }
            );

        }

        public override void Update() {
            //LightObj.gameObject.transform.Position = new Vector3((float)Math.Cos(Time.Time * 0.5f) * 5.5f, 5f, (float)Math.Sin(Time.Time * 0.5f) * 5.5f);
            //LightObj.gameObject.transform.Rotation = Quaternion.RotationYawPitchRoll(0, Time.Time * 1f, 0f);
            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                Quit();
            }
        }

        private void AddTestScene() {
            AddGameObject(
                "Cow",
                new Transform()
                {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.0025f,
                    Position = new Vector3(0, 0, 2.5f),
                },
                new Renderer()
                {
                    Geometry = AssetsLoader.LoadMesh("CowMesh"),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Cow"),
                }
            );

            /*GameObject Hat = AddGameObject(
                "Hat",
                new Transform()
                {
                    Rotation = Quaternion.RotationYawPitchRoll(0, -MathUtil.Pi * 0.5f, 0),
                    Scale = Vector3.One * 0.1f,
                    Position = new Vector3(0, 5f, 30.6f),
                }
            );

            Hat.AddComponent(new Renderer()
            {
                Geometry = AssetsLoader.LoadMesh("HatMesh"),
                Topology = PrimitiveTopology.TriangleList,
                RendererMaterial = GetMaterial("M_Cat"),
            });*/

            AddGameObject(
                "SmallCube",
                new Transform()
                {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                    Position = new Vector3(0, 2f, 6f),
                },
                new Renderer()
                {
                    Geometry = Primitives.Cube(new Vector4(1f, 0, 0, 1f)),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Metal"),
                }
            );

            Material StoneMaterial = GetMaterial("M_Stone");
            AddGameObject(
                "Rock",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 1f,
                    Position = new Vector3(3f, 0.2f, -10f),
                }, new Renderer() {
                    Geometry = AssetsLoader.LoadMesh("RockMesh"),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = StoneMaterial,
                }
            );
            AddGameObject(
                "Rock",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 1f,
                    Position = new Vector3(-3f, 0.2f, -10f),
                }, new Renderer() {
                    Geometry = AssetsLoader.LoadMesh("RockMesh"),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = StoneMaterial,
                }
            );
            AddGameObject(
                "Cube1m",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                    Position = Vector3.Left * 4f + Vector3.Up * 0.5f,
                }, new Renderer() {
                    Geometry = AssetsLoader.LoadMesh("Cube1mMesh"),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_SnowRock"),
                }
            );
            AddGameObject(
                "Cube1m",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                    Position = Vector3.Left * 4f + Vector3.Up * 1.5f,
                }, new Renderer() {
                    Geometry = AssetsLoader.LoadMesh("Cube1mMesh"),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_SnowRock"),
                }
            );
            AddGameObject(
                "Soldier",
                new Transform() {
                    Position = Vector3.Left * 4f + Vector3.BackwardLH,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.6f,
                },
                new Renderer() {
                    Geometry = AssetsLoader.LoadMesh("SoldierMesh"),
                    RendererMaterial = GetMaterial("M_Soldier"),
                }
            );

            GameObject Floor = AddGameObject(
                "Floor",
                new Transform()
                {
                    Rotation = Quaternion.Identity,
                    Scale = new Vector3(25f, 0.25f, 25f),
                    Position = Vector3.Down * 0.15f,
                },
                new Renderer()
                {
                    Geometry = Primitives.Cube(),
                    RendererMaterial = GetMaterial("M_RockFloor"),
                }
            );
        }
    }
}
