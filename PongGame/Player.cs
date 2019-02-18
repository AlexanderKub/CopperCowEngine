﻿using SharpDX;
using EngineCore;

namespace PongGame
{
    class Player: Component
    {
        public enum TeamType
        {
            Red,
            Blue
        }

        public TeamType Team;
        public Transform LeftWall;
        public Transform RightWall;

        private Vector3 RedTeamStartPoint = new Vector3(0, 1f, -28f);
        private Vector3 BlueTeamStartPoint = new Vector3(0, 1f, 28f);
        private float Speed = 3f;

        private Transform m_transform;
        public override void Init() {
            m_transform = gameObject.transform;
            m_transform.Rotation = Quaternion.Identity;
            m_transform.Scale = new Vector3(8f, 2f, 2f);
            m_transform.Position = Team == TeamType.Red ? RedTeamStartPoint : BlueTeamStartPoint;
        }

        public override void Update() {

            Vector2 posOffset = Vector2.Zero;
            if (Team == TeamType.Red) {
                if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.D)) {
                    posOffset += Vector2.UnitX;
                }
                if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.A)) {
                    posOffset -= Vector2.UnitX;
                }
            } else {
                //TODO: Bot controller
                if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.Right)) {
                    posOffset += Vector2.UnitX;
                }
                if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.Left)) {
                    posOffset -= Vector2.UnitX;
                }
            }

            m_transform.Position += (Matrix.RotationQuaternion(m_transform.Rotation).Right * posOffset.X +
            Matrix.RotationQuaternion(m_transform.Rotation).Forward * posOffset.Y) *
            10f * Speed * Engine.Instance.Time.DeltaTime;

            if (m_transform.Position.X + m_transform.Scale.X * 0.5f > RightWall.Position.X - RightWall.Scale.X * 0.5f) {
                m_transform.Position = new Vector3(
                    RightWall.Position.X - RightWall.Scale.X * 0.5f - m_transform.Scale.X * 0.5f,
                    m_transform.Position.Y,
                    m_transform.Position.Z
                );
            }

            if (m_transform.Position.X - m_transform.Scale.X * 0.5f < LeftWall.Position.X + LeftWall.Scale.X * 0.5f) {
                m_transform.Position = new Vector3(
                    LeftWall.Position.X + LeftWall.Scale.X * 0.5f + m_transform.Scale.X * 0.5f, 
                    m_transform.Position.Y, 
                    m_transform.Position.Z
                );
            }
        }
    }
}
