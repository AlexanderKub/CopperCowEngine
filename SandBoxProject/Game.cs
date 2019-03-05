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
            AddTestScene();
        }

        private void CreateMap()
        {
            CreateSkySphere();
            AddCamera<FreeCamera>("MainCamera", new Vector3(0f, 5f, -10f), Quaternion.Identity);

            LightObj = new Light()
            {
                LightColor = Vector4.One * 0.5f,
                radius = 20,
                Type = Light.LightType.Directional,
                EnableShadows = true,
            };
            float rad = MathUtil.DegreesToRadians(-45f);
            Vector3 lightpos = new Vector3(0f, 12f, 10f) * 10f;
            GameObject go = AddLight("Light", LightObj, lightpos,
                Quaternion.RotationYawPitchRoll(0, rad, 0), true);

            Random random = new Random();
            float range = 120f;
            for (int i = 0; i < 100; i++){
                AddLight("PointLight", new Light() {
                    Type = Light.LightType.Point,
                    radius = RandomUtil.NextFloat(random, 5f, 10f),
                    LightColor = RandomUtil.NextVector4(random, Vector4.One * 0.2f, Vector4.One),
                    LightIntensity = 1f,
                }, new Vector3(RandomUtil.NextFloat(random, -range * 0.5f, range * 0.5f),
                    RandomUtil.NextFloat(random, 0, 5.0f), RandomUtil.NextFloat(random, -range * 0.5f, range * 0.5f)), Quaternion.Identity);
            }
            
            var GO = AddGameObject("455");
            GO.transform.WorldPosition = Vector3.ForwardLH + Vector3.Up * 4f;
            GO.transform.LocalScale = Vector3.One * 5f;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Sphere(16), GetMaterial("M_Copper"));
        }

        public override void Update() {
            LightObj.gameObject.transform.WorldPosition = new Vector3((float)Math.Cos(Time.Time * 0.5f) * 5.5f, 5f, (float)Math.Sin(Time.Time * 0.5f) * 5.5f);
            LightObj.gameObject.transform.WorldRotation = Quaternion.RotationYawPitchRoll(0, Time.Time * 1f, 0f);
            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                Quit();
            }
        }

        private void AddTestScene() {
            var GO = AddGameObject("Cow");
            GO.transform.WorldPosition = new Vector3(0, 0, 2.5f);
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = Vector3.One * 0.0025f;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(AssetsLoader.LoadMesh("CowMesh"), GetMaterial("M_Cow"));

            float floorLength = 120f;
            GameObject Floor = AddGameObject("Floor");
            Floor.transform.WorldRotation = Quaternion.Identity;
            Floor.transform.WorldScale = new Vector3(floorLength, 0.25f, floorLength);
            Floor.transform.WorldPosition = Vector3.Down * 0.15f;
            Floor.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorT");
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = new Vector3(floorLength, 5f, 0.25f);
            GO.transform.WorldPosition = Vector3.ForwardLH * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTT");
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = new Vector3(floorLength, 5f, 0.25f);
            GO.transform.WorldPosition = -Vector3.ForwardLH * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTTT");
            GO.transform.WorldRotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
            GO.transform.WorldScale = new Vector3(0.25f, 5f, floorLength);
            GO.transform.WorldPosition = Vector3.Right * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            GO = AddGameObject("FloorTTTT");
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = new Vector3(0.25f, 5f, floorLength);
            GO.transform.WorldPosition = -Vector3.Right * floorLength * 0.5f + Vector3.Up * 2.45f;
            GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                Tile = new Vector2(16, 0.75f),
            };
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_RockFloor"));

            Random random = new Random();
            float domH = 10f;
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = RandomUtil.NextVector3(random, 
                    new Vector3(-floorLength * 0.5f + 10f, domH * 0.5f, -floorLength * 0.5f + 10f), 
                    new Vector3(floorLength * 0.5f - 10f, domH * 0.5f, floorLength * 0.5f - 10f));
                GO = AddGameObject("House" + i);
                GO.transform.WorldRotation = Quaternion.Identity;
                GO.transform.WorldScale = new Vector3(10f, domH, 10f);
                GO.transform.WorldPosition = pos;
                GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Metal"));
            }
        }
    }
}
