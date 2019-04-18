using System;
using SharpDX;
using SharpDX.Direct3D;
using EngineCore;
using AssetsManager.Loaders;

namespace KatamariGame
{
    class Game : Engine
    {
        public Game(string name):base(name) { }
        public override EngineCore.RenderTechnique.RenderPath GetRenderPath() {
            return EngineCore.RenderTechnique.RenderPath.Forward;
        }

        GameObject Player;
        Material catMat = new Material()
        {
            AlbedoMapAsset = "CatAlbedoMap",
        };
        Material cowMat = new Material()
        {
            AlbedoMapAsset = "CopperAlbedoMap",
            NormalMapAsset = "CopperNormalMap",
            MetallicMapAsset = "CopperMetallicMap",
            RoughnessMapAsset = "CopperRoughnessMap",
        };

        Material playerMat = new Material() {
            AlbedoMapAsset = "SnowRockAlbedoMap",
            NormalMapAsset = "SnowRockNormalMap",
            RoughnessMapAsset = "SnowRockRoughnessMap",
            MetallicMapAsset = "SnowRockMetallicMap",
            OcclusionMapAsset = "SnowRockOcclusionMap",
        };

        public override void OnStart() {
            ClearColor = Color.DarkCyan;
            catMat.LoadMapsAndInitSampler();
            cowMat.LoadMapsAndInitSampler();
            playerMat.LoadMapsAndInitSampler();

            CreateSkySphere();
            CreateFloor();

            Player = CreatePlayer();

            FollowCamera camera = AddCamera<FollowCamera>("MainCamera") as FollowCamera;
            camera.Distance = 10f;
            camera.Target = Player.transform;

            DeprecatedLight LightObj = new DeprecatedLight() {
                LightColor = Vector4.One * 0.5f,
                radius = 20,
                Type = DeprecatedLight.LightType.Directional,
                EnableShadows = true,
            };
            AddLight("Light", LightObj, new Vector3(0f, 10f, 10f), Quaternion.RotationYawPitchRoll(-MathUtil.Pi * 0.5f, -MathUtil.Pi * 0.5f, 0), true);

            /*SetMainCamera(
                AddCamera(
                    "MainCamera",
                    new FreeCamera() {
                        Speed = 10f,
                    },
                    new Transform() {
                        Position = new Vector3(0, 0, 0),
                        Rotation = Quaternion.Identity,
                    }
                )
            );*/

            CreateLevelPickups();
        }

        public override void Update() {
            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                Quit();
            }
        }

        //Bad Hat model Pivot
        private void CreateHat() {
            Vector3 pos = new Vector3();
            Vector3 rot = Vector3.Zero;
            float scale = Generator.NextFloat(0.2f, 2f);

            pos.Y = scale * 1f;
            pos.X = Generator.NextFloat(-100f, 100f);
            pos.Z = Generator.NextFloat(-100f, 100f);
            
            rot.X = Generator.NextFloat(0, 360f);

            AddHat(pos, rot, scale);
        }

        private void AddHat(Vector3 position, Vector3 yawPitchRoll, float scale) {
            new PickupObject(
                "Hat",
                position,
                Quaternion.RotationYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z),
                Vector3.One * 0.05f * scale,
                AssetsLoader.LoadMesh("HatMesh"),
                catMat,
                scale * 0.5f
            );
        }
        private void CreateCow() {
            Vector3 pos = new Vector3();
            Vector3 rot = Vector3.Zero;
            float scale = Generator.NextFloat(1f, 3f);

            pos.Y = 0;
            pos.X = Generator.NextFloat(-100f, 100f);
            pos.Z = Generator.NextFloat(-100f, 100f);

            rot.X = Generator.NextFloat(0, 360f);

            AddCow(pos, rot, scale);
        }

        private void AddCow(Vector3 position, Vector3 yawPitchRoll, float scale) {
            new PickupObject(
                "Cow",
                position,
                Quaternion.RotationYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z),
                Vector3.One * 3.3333f *scale,
                AssetsLoader.LoadMesh("CowMesh"),
                cowMat,
                3f * scale
            );
        }

        private void CreateCube() {
            Vector3 pos = new Vector3();
            Vector3 rot = Vector3.Zero;
            float scale = Generator.NextFloat(1f, 3f);

            pos.Y = scale * 0.5f;
            pos.X = Generator.NextFloat(-100f, 100f);
            pos.Z = Generator.NextFloat(-100f, 100f);

            rot.X = Generator.NextFloat(0, 360f);

            AddCube(pos, rot, scale);
        }

        private ModelGeometry[] CubesArray = new ModelGeometry[] {
            Primitives.Cube(Primitives.Red),
            Primitives.Cube(Primitives.Green),
            Primitives.Cube(Primitives.Yellow)
        };

        private void AddCube(Vector3 position, Vector3 yawPitchRoll, float scale) {
            new PickupObject(
                "SmallCube",
                position,
                Quaternion.RotationYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z),
                Vector3.One * scale,
                CubesArray[Generator.Next(CubesArray.Length)],
                Material.DefaultMaterial,
                scale * 0.5f
            );
        }

        private Random Generator = new Random();
        private void CreateLevelPickups() {
            for (int i = 0; i < 25; i++) {
                CreateCube();
            }
            /*for (int i = 0; i < 10; i++) {
                CreateHat();
            }*/
            for (int i = 0; i < 10; i++) {
                CreateCow();
            }
        }

        private GameObject CreatePlayer() {
            GameObject PlayerGO = AddGameObject("Player", true);
            PlayerGO.transform.Rotation = Quaternion.Identity;
            PlayerGO.transform.Scale = Vector3.One;
            PlayerGO.transform.Position = new Vector3(0, 0.5f, 0);
            PlayerGO.AddComponent(new CBoundingSphere() {
                Radius = 1f,
            });
            PlayerController PC = (PlayerController)(PlayerGO.AddComponent(new PlayerController()));

            GameObject GO = AddGameObject("SpherePlayer");
            GO.transform.RelativeRotation = Quaternion.Identity;
            GO.transform.RelativeScale = Vector3.One * 2f;
            GO.transform.RelativePosition = new Vector3(0, 0, 0);
            GO.transform.Parent = PlayerGO.transform;
            GO.GetComponent<DeprecatedRenderer>().RendererMaterial = playerMat;
            GO.GetComponent<DeprecatedRenderer>().UpdateMesh(Primitives.Sphere(30));
            PC.SetVisualTransform(GO.transform);

            return PlayerGO;
        }

        private GameObject CreateFloor()
        {
            Material mat = new Material()
            {
                //AlbedoMapAsset = "CatAlbedoMap",
                AlbedoMapAsset = "DebugTextureMap",
                PropetyBlock = new MaterialPropetyBlock() {
                    MetallicValue = 0.15f,
                    RoughnessValue = 0.8f,
                    Tile = new Vector2(2f, 2f),
                },
            };
            mat.LoadMapsAndInitSampler();

            GameObject FloorGO = AddGameObject("Floor");
            FloorGO.transform.Position = -0.5f * Vector3.Up;
            FloorGO.transform.Scale = new Vector3(200f, 200f, 1f);
            FloorGO.transform.Rotation = Quaternion.RotationYawPitchRoll(0, -MathUtil.DegreesToRadians(90f), 0);
            FloorGO.GetComponent<DeprecatedRenderer>().RendererMaterial = mat;
            FloorGO.GetComponent<DeprecatedRenderer>().UpdateMesh(Primitives.PlaneWithUV);
            return FloorGO;
        }
    }
}