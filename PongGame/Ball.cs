using SharpDX;
using System;
using EngineCore;

namespace PongGame
{
    class Ball : Component
    {
        public float Speed;
        public Transform RedPlayerTransform;
        public Transform BluePlayerTransform;
        public Transform LeftWall;
        public Transform RightWall;

        private Vector3 m_Vector;
        private Vector3 ZeroPosition;
        private Random m_RandomGen;

        private Transform m_transform;
        public override void Init() {
            m_transform = gameObject.transform;
            m_transform.Rotation = Quaternion.Identity;
            m_transform.Scale = new Vector3(2f, 2f, 2f);
            ZeroPosition = m_transform.Position;
            m_RandomGen = new Random();
        }

        public void StartGame() {
            m_Vector = RandomDirection(m_RandomGen.NextDouble() > 0.5 ? 1f : -1f);
        }

        public void ResetGame() {
            m_transform.Position = ZeroPosition;
            m_Vector = Vector3.Zero;
        }

        private bool onGoal;
        public override void Update() {
            float m_Time = Engine.Instance.Time.Time;
            m_transform.Rotation = Quaternion.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(m_Time * 100f)) *
                Quaternion.RotationAxis(Vector3.ForwardLH, MathUtil.DegreesToRadians(m_Time * 100f));
            m_transform.Position += m_Vector * Speed * Engine.Instance.Time.DeltaTime;

            if(m_Vector.X < 0) {
                if(m_transform.Position.X - m_transform.Scale.X * 0.5f < LeftWall.Position.X + LeftWall.Scale.X * 0.5f) {
                    WallBounce();
                }
            } else {
                if (m_transform.Position.X + m_transform.Scale.X * 0.5f > RightWall.Position.X - RightWall.Scale.X * 0.5f) {
                    WallBounce();
                }
            }

            if (m_Vector.Z < 0) {
                if (m_transform.Position.Z - m_transform.Scale.Z * 0.5f < 
                    RedPlayerTransform.Position.Z + RedPlayerTransform.Scale.Z * 0.5f) {

                    if (!onGoal && m_transform.Position.X + m_transform.Scale.X * 0.5 > 
                        RedPlayerTransform.Position.X - RedPlayerTransform.Scale.X * 0.5f
                        && m_transform.Position.X - m_transform.Scale.X * 0.5f < 
                        RedPlayerTransform.Position.X + RedPlayerTransform.Scale.X * 0.5f) {
                        Bounce(1f, RedPlayerTransform.Position.X - m_transform.Position.X);
                    } else {
                        onGoal = true;
                        if (m_transform.Position.Z < RedPlayerTransform.Position.Z - RedPlayerTransform.Scale.Z * 2f) {
                            Goal(Player.TeamType.Blue);
                        }
                    }
                }
            } else {
                if (m_transform.Position.Z + m_transform.Scale.Z * 0.5f > 
                    BluePlayerTransform.Position.Z - BluePlayerTransform.Scale.Z * 0.5f) {

                    if (!onGoal && m_transform.Position.X + m_transform.Scale.X * 0.5 > 
                        BluePlayerTransform.Position.X - BluePlayerTransform.Scale.X * 0.5f
                        && m_transform.Position.X - m_transform.Scale.X * 0.5f < 
                        BluePlayerTransform.Position.X + BluePlayerTransform.Scale.X * 0.5f) {
                        Bounce(-1f, BluePlayerTransform.Position.X - m_transform.Position.X);
                    } else {
                        onGoal = true;
                        if (m_transform.Position.Z > BluePlayerTransform.Position.Z + BluePlayerTransform.Scale.Z * 2f) {
                            Goal(Player.TeamType.Red);
                        }
                    }
                }
            }
        }

        private void Bounce(float dir, float angle) {
            m_Vector = (dir * 5f * Vector3.ForwardLH + Vector3.Left * angle);
            m_Vector.Normalize();
        }
        
        private void WallBounce() {
            m_Vector = new Vector3(-1f * m_Vector.X, m_Vector.Y, m_Vector.Z);
        }

        private void Goal(Player.TeamType Team) {
            m_transform.Position = ZeroPosition;
            m_Vector = RandomDirection(Team == Player.TeamType.Red ? -1f : 1f);
            onGoal = false;
            ((Game)Engine.Instance).Goal(Team);
        }

        private Vector3 RandomDirection(float dir) {
            Vector3 newVector = (dir * 5f * Vector3.ForwardLH + Vector3.Left * m_RandomGen.NextFloat(-5f, 5f));
            newVector.Normalize();
            return newVector;
        }
    }
}
