using SharpDX;
using SharpDX.Direct3D;
using EngineCore;
using EngineCore.RenderTechnique;

namespace PongGame
{
    class Game : Engine
    {
        private int RedScore;
        private int BlueScore;
        private int MaxScore = 7;
        private bool isGameStarted;
        public Game(string name) : base(name) { }
        public override RenderPath GetRenderPath()
        {
            return RenderPath.Forward;
        }

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

            AddCamera<Camera>("MainCamera", new Vector3(0f, 40f, 0f), Quaternion.RotationYawPitchRoll(
                MathUtil.DegreesToRadians(0f), MathUtil.DegreesToRadians(89.99f), 0
            ));

            Light LightObj = new Light() {
                LightColor = new Vector4(0.1f, 0.1f, 0.1f, 1f),
                radius = 10,
                Type = Light.LightType.Directional,
                EnableShadows = true,
            };
            AddLight("Light", LightObj, new Vector3(0f, 10f, 10f), Quaternion.RotationYawPitchRoll(30f, 30f, 30f), true);

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
            GameObject GO = AddGameObject((isRed ? "RedScores" : "BlueScores") + i);
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = Vector3.One * 0.01f;
            GO.transform.WorldPosition = new Vector3(-25f, 1f, isRed ? (i * 4f - 28f) : (28f - i * 4f));
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), isRed ? GetMaterial("M_Red") : GetMaterial("M_Blue"));
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
            GameObject GO = AddGameObject("Ball");
            GO.transform.WorldPosition = Vector3.Up;
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Ball"));

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
            GameObject GO = AddGameObject("RedPlayer");
            GO.transform.WorldPosition = Vector3.Up;
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Red"));
            RedPlayer = (Player)GO.AddComponent(new Player() {
                Team = Player.TeamType.Red,
                LeftWall = LeftWall,
                RightWall = RightWall
            });

            GO = AddGameObject("BluePlayer");
            GO.transform.WorldPosition = Vector3.Up;
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Blue"));
            BluePlayer = (Player)GO.AddComponent(new Player() {
                Team = Player.TeamType.Blue,
                LeftWall = LeftWall,
                RightWall = RightWall
            });
        }

        private Transform LeftWall;
        private Transform RightWall;
        private void CreateWalls() {
            GameObject GO = AddGameObject("LeftWall");
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = new Vector3(2f, 2f, 58f);
            GO.transform.WorldPosition = new Vector3(-20f, 1f, 0);
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Green"));
            LeftWall = GO.transform;

            GO = AddGameObject("RightWall");
            GO.transform.WorldRotation = Quaternion.Identity;
            GO.transform.WorldScale = new Vector3(2f, 2f, 58f);
            GO.transform.WorldPosition = new Vector3(20f, 1f, 0);
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), GetMaterial("M_Green"));
            RightWall = GO.transform;
        }

        private void CreateCeils() {
            Primitives.CeilSizeX = 20;
            Primitives.CeilSizeY = 29;
            GameObject Ceil = AddGameObject("Ceil");
            Ceil.transform.WorldRotation = Quaternion.Identity;
            Ceil.transform.WorldScale = Vector3.One * 4f;
            Ceil.transform.WorldPosition = Vector3.Zero;
            Ceil.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            Ceil.GetComponent<Renderer>().Topology = PrimitiveTopology.LineList;
            Ceil.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Ceil, GetMaterial("M_Green"));
        }

    }
}
