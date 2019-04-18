using System;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;

namespace SandBoxProject
{
    public class Game : Engine
    {
        DeprecatedLight LightObj;

        public Game(string name):base(name) { }

        public override EngineCore.RenderTechnique.RenderPath GetRenderPath()
        {
            return EngineCore.RenderTechnique.RenderPath.ForwardPlus;
        }

        public override void LoadMaterials()
        {
            Material mat;
            mat = new Material()
            {
                Name = "M_Cow",
                AlbedoMapAsset = "CowAlbedoMap",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    AlphaValue = 0.49f,
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
            //AddTestScene();
        }

        GameObject[] lightsGO;

        private void CreateMap()
        {
            CreateSkySphere();
            AddCamera<FreeCamera>("MainCamera", new Vector3(0f, 5f, -10f), Quaternion.Identity);

            LightObj = new DeprecatedLight()
            {
                LightColor = Vector4.One * 0.5f,
                radius = 20,
                Type = DeprecatedLight.LightType.Directional,
                EnableShadows = true,
            };
            float rad = MathUtil.DegreesToRadians(-45f);
            Vector3 lightpos = new Vector3(0f, 12f, 10f) * 10f;
            GameObject go = AddLight("Light", LightObj, lightpos,
                Quaternion.RotationYawPitchRoll(0, rad, 0), true);

            Random random = new Random();
            float range = 120f;
            lightsGO = new GameObject[100];
            for (int i = 0; i < 100; i++){
                lightsGO[i] = AddLight("PointLight", new DeprecatedLight() {
                    Type = DeprecatedLight.LightType.Point,
                    radius = RandomUtil.NextFloat(random, 5f, 10f),
                    LightColor = RandomUtil.NextVector4(random, Vector4.One * 0.2f, Vector4.One),
                    LightIntensity = 1f,
                }, new Vector3(RandomUtil.NextFloat(random, -range * 0.5f, range * 0.5f),
                    RandomUtil.NextFloat(random, 0, 10.0f), 
                    RandomUtil.NextFloat(random, -range * 0.5f, range * 0.5f)), Quaternion.Identity);
            }

            var rootGO = AddGameObject("455", true);
            rootGO.transform.Position = Vector3.Zero;
            rootGO.transform.Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.DegreesToRadians(-90), 0);
            rootGO.transform.Scale = Vector3.One;

            var GO = AddGameObject("Sponza");
            GO.transform.Parent = rootGO.transform;
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(AssetsLoader.LoadMesh("SponzaMesh"), GetMaterial("M_Metal"));

            for (int i = 1; i < 381; i++)
            {
                GO = AddGameObject("Sponza_" + i);
                GO.transform.Parent = rootGO.transform;
                GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(AssetsLoader.LoadMesh("SponzaMesh_" + i), GetMaterial("M_Metal"));
            }
        }

        public override void Update() {
            LightObj.transform.Position = new Vector3((float)Math.Cos(Time.Time * 0.5f) * 5.5f, 5f, (float)Math.Sin(Time.Time * 0.5f) * 5.5f);
            LightObj.transform.Rotation = Quaternion.RotationYawPitchRoll(0, Time.Time * 0.21f, 0f);
            for (int i = 0; i < 100; i++)
            {
                lightsGO[i].transform.Position += (float)Math.Sin(Time.Time * 0.5f) * Time.DeltaTime * 2f;
            }
            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                Quit();
            }
        }

        private void AddTestScene() {
            var GO = AddGameObject("Cow");
            GO.transform.Position = new Vector3(0, 0, 2.5f);
            GO.transform.Rotation = Quaternion.Identity;
            GO.transform.Scale = Vector3.One * 0.0025f;
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(AssetsLoader.LoadMesh("CowMesh"), GetMaterial("M_Cow"));

            float floorLength = 120f;
            GameObject Floor = AddGameObject("Floor");
            Floor.transform.Rotation = Quaternion.Identity;
            Floor.transform.Scale = new Vector3(floorLength, 0.25f, floorLength);
            Floor.transform.Position = Vector3.Down * 0.15f;
            Floor.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorT");
            GO.transform.Rotation = Quaternion.Identity;
            GO.transform.Scale = new Vector3(floorLength, 5f, 0.25f);
            GO.transform.Position = Vector3.ForwardLH * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTT");
            GO.transform.Rotation = Quaternion.Identity;
            GO.transform.Scale = new Vector3(floorLength, 5f, 0.25f);
            GO.transform.Position = -Vector3.ForwardLH * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTTT");
            GO.transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
            GO.transform.Scale = new Vector3(0.25f, 5f, floorLength);
            GO.transform.Position = Vector3.Right * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTTTT");
            GO.transform.Rotation = Quaternion.Identity;
            GO.transform.Scale = new Vector3(0.25f, 5f, floorLength);
            GO.transform.Position = -Vector3.Right * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            Random random = new Random();
            float domH = 10f;
            for (int i = 0; i < 50; i++)
            {
                Vector3 pos = RandomUtil.NextVector3(random, 
                    new Vector3(-floorLength * 0.5f + 10f, domH * 0.5f, -floorLength * 0.5f + 10f), 
                    new Vector3(floorLength * 0.5f - 10f, domH * 0.5f, floorLength * 0.5f - 10f));
                GO = AddGameObject("House" + i);
                GO.transform.Rotation = Quaternion.Identity;
                GO.transform.Scale = new Vector3(10f, domH, 10f);
                GO.transform.Position = pos;
                GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Metal"));
            }
        }
    }
}
