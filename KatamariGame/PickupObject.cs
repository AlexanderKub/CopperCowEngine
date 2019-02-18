using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using EngineCore;
using AssetsManager.Loaders;

namespace KatamariGame
{
    public class PickupObject : GameObject
    {

        private FollowCamera FollCam;
        public PickupObject(
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            ModelGeometry geometry,
            Material material,
            float radius
        ):base(name) {

            AddComponent(new Transform() {
                Position = position,
                Rotation = rotation,
                Scale = scale,
            });

            AddComponent(new Renderer() {
                Geometry = geometry,
                RendererMaterial = material,
            });


            CBoundingSphere BSC = new CBoundingSphere() {
                Radius = radius,
            };

            BSC.OnContact = ((CBoundingSphere other, GameObject self) => {
                if (other.gameObject.Name == "Player") {
                    PlayerController PC = other.gameObject.GetComponent<PlayerController>();
                    if (BSC.Radius < other.Radius) {
                        self.Components.Remove(BSC);
                        BSC.Destroy();
                        self.transform.SetParent(PC.GetVisualTransform());
                        other.Radius += BSC.Radius * 0.5f;
                        other.gameObject.transform.Position += Vector3.Up * BSC.Radius * 0.5f;
                        if (FollCam == null) {
                            FollCam = Engine.Instance.MainCamera.gameObject.GetComponent<FollowCamera>();
                        }
                        FollCam.Distance += BSC.Radius * 1.25f;
                    } else {
                        PC.gameObject.transform.Rotation *= Quaternion.RotationAxis(PC.gameObject.transform.TransformMatrix.Up,
                            MathUtil.DegreesToRadians(180f));
                    }
                }
            });
            AddComponent(BSC);
        }
    }
}
