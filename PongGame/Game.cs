using SharpDX;
using SharpDX.Direct3D;
using EngineCore;

namespace PongGame
{
    class Game : Engine
    {
        private int RedScore;
        private int BlueScore;
        private int MaxScore = 7;
        private bool isGameStarted;
        public Game(string name) : base(name) { }

        public override void LoadMaterials() {
            MaterialTable.Add("M_Red", new Material() {
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = Vector3.Right,
                }
            });
            MaterialTable.Add("M_Blue", new Material() {
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = Vector3.ForwardLH,
                }
            });
            MaterialTable.Add("M_Green", new Material() {
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = Vector3.Up,
                }
            });
            MaterialTable.Add("M_Ball", new Material() {
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = new Vector3(0.63f, 0.0f, 1.0f),
                }
            });
        }

        public override void OnStart() {
            ClearColor = Color.Black;

            SetMainCamera(
                AddCamera(
                    "MainCamera",
                    new Camera(),
                    new Transform() {
                        Position = new Vector3(0f, 40f, 0f),
                        Rotation = Quaternion.Identity,
                    }
                )
            );

            Light LightObj = new Light() {
                ambientColor = new Vector4(0.1f, 0.1f, 0.1f, 1f),
                diffuseColor = new Vector4(0.74f, 0.74f, 0.74f, 1f),
                specularColor = new Vector4(0.76f, 0.76f, 0.76f, 1f),
                radius = 10,
                Type = Light.LightType.Directional,
                EnableShadows = true,
            };
            AddLight("Light", LightObj, new Vector3(0f, 10f, 10f), Quaternion.RotationYawPitchRoll(30f, 30f, 30f), true);

            MainCamera.gameObject.transform.Rotation = Quaternion.RotationYawPitchRoll(
                MathUtil.DegreesToRadians(0f), MathUtil.DegreesToRadians(89.99f), 0
            );

            CreateCeils();
            CreateWalls();
            CreatePlayers();
            CreateBall();
            CreateScore();
        }

        public override void Update() {
            if (!isGameStarted) {
                if (Input.IsKeyDown(System.Windows.Forms.Keys.Space)) {
                    isGameStarted = true;
                    m_Ball.StartGame();
                }
            } else {
                if (Input.IsKeyDown(System.Windows.Forms.Keys.R)) {
                    ResetGame();
                }
            }

            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                Quit();
            }
        }

        private ScorePoint[] RedScores;
        private ScorePoint[] BlueScores;
        private void CreateScore() {
            RedScore = 0;
            RedScores = new ScorePoint[MaxScore];
            for (int i = 0; i < RedScores.Length; i++) {
                RedScores[i] = CreateScorePoint(i, true);
            }
            
            BlueScore = 0;
            BlueScores = new ScorePoint[MaxScore];
            for (int i = 0; i < BlueScores.Length; i++) {
                BlueScores[i] = CreateScorePoint(i, false);
            }
        }
        
        private ScorePoint CreateScorePoint(int i, bool isRed) {
            GameObject GO = AddGameObject(
                (isRed ? "RedScores" : "BlueScores") + i,
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.01f,
                    Position = new Vector3(-25f, 1f, isRed ? (i * 4f - 28f) : (28f - i * 4f)),
                },
                new Renderer() {
                    Geometry = Primitives.Cube(isRed ? Primitives.Red : Primitives.Blue),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = isRed ? GetMaterial("M_Red") : GetMaterial("M_Blue"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );

            return (ScorePoint)GO.AddComponent(new ScorePoint());
        }

        public void Goal(Player.TeamType Team) {
            if(Team == Player.TeamType.Red) {
                if (RedScore < RedScores.Length) {
                    RedScores[RedScore].TargetScale = Vector3.One * 2f - Vector3.Up;
                }
                RedScore++;
                if (RedScore >= MaxScore) {
                    //TODO: Win red team message
                    ResetGame();
                }
            } else {
                if (BlueScore < BlueScores.Length) {
                    BlueScores[BlueScore].TargetScale = Vector3.One * 2f - Vector3.Up;
                }
                BlueScore++;
                if (BlueScore >= MaxScore) {
                    //TODO: Win blue team message
                    ResetGame();
                }
            }
        }

        private void ResetGame() {
            RedScore = 0;
            BlueScore = 0;
            for (int i = 0; i < MaxScore; i++) {
                RedScores[i].Hide();
                BlueScores[i].Hide();
            }
            isGameStarted = false;
            m_Ball.ResetGame();
        }

        private Ball m_Ball;
        private void CreateBall() {
            GameObject GO = AddGameObject(
                "Ball",
                new Transform() {
                    Position = Vector3.Up,
                }, 
                new Renderer() {
                    Geometry = Primitives.Cube(),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Ball"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );

            m_Ball = (Ball)GO.AddComponent(new Ball() {
                Speed = 60f,
                RedPlayerTransform = RedPlayer.gameObject.transform,
                BluePlayerTransform = BluePlayer.gameObject.transform,
                LeftWall = LeftWall,
                RightWall = RightWall
            });
        }

        private Player RedPlayer;
        private Player BluePlayer;
        private void CreatePlayers() {
            GameObject GO = AddGameObject(
                "RedPlayer",
                new Transform() {
                    Position = Vector3.Up,
                },
                new Renderer() {
                    Geometry = Primitives.Cube(Primitives.Red),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Red"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );
            RedPlayer = (Player)GO.AddComponent(new Player() {
                Team = Player.TeamType.Red,
                LeftWall = LeftWall,
                RightWall = RightWall
            });

            GO = AddGameObject(
                "BluePlayer",
                new Transform() {
                    Position = Vector3.Up,
                },
                new Renderer() {
                    Geometry = Primitives.Cube(Primitives.Blue),
                    Topology = PrimitiveTopology.TriangleList,
                    RendererMaterial = GetMaterial("M_Blue"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );
            BluePlayer = (Player)GO.AddComponent(new Player() {
                Team = Player.TeamType.Blue,
                LeftWall = LeftWall,
                RightWall = RightWall
            });
        }

        private Transform LeftWall;
        private Transform RightWall;
        private void CreateWalls() {
            LeftWall = AddGameObject(
                "LeftWall",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = new Vector3(2f, 2f, 58f),
                    Position = new Vector3(-20f, 1f, 0),
                }, new Renderer() {
                    Topology = PrimitiveTopology.TriangleList,
                    Geometry = Primitives.Cube(),
                    RendererMaterial = GetMaterial("M_Green"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            ).transform;

            RightWall = AddGameObject(
                "LeftWall",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = new Vector3(2f, 2f, 58f),
                    Position = new Vector3(20f, 1f, 0),
                },
                new Renderer() {
                    Topology = PrimitiveTopology.TriangleList,
                    Geometry = Primitives.Cube(),
                    RendererMaterial = GetMaterial("M_Green"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            ).transform;
        }

        private void CreateCeils() {
            Primitives.CeilSizeX = 20;
            Primitives.CeilSizeY = 29;
            GameObject Ceil = AddGameObject(
                "Ceil",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 4f,
                    Position = Vector3.Zero,
                }, 
                new Renderer() {
                    Topology = PrimitiveTopology.LineList,
                    Geometry = Primitives.Ceil,
                    RendererMaterial = GetMaterial("M_Green"),
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );
        }

    }
}
