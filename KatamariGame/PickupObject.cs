using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using EngineCore;
using AssetsManager.Loaders;

namespace KatamariGame
{
    public class PickupObject
    {
        private GameObject go;
        private FollowCamera FollCam;
        public PickupObject(
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            ModelGeometry geometry,
            Material material,
            float radius
        ) {
            go = Engine.Instance.AddGameObject(name);
            go.transform.Position = position;
            go.transform.Rotation = rotation;
            go.transform.Scale = scale;
            go.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(geometry, material);


            CBoundingSphere BSC = new CBoundingSphere() {
                Radius = radius,
            };

            BSC.OnContact = ((CBoundingSphere other, GameObject self) => {
                if (other.gameObject.Name == "Player") {
                    PlayerController PC = other.gameObject.GetComponent<PlayerController>();
                    if (BSC.Radius < other.Radius) {
                        BSC.Destroy();
                        self.transform.SetParent(PC.GetVisualTransform());
                        other.Radius += BSC.Radius * 0.5f;
                        other.transform.Position += Vector3.Up * BSC.Radius * 0.5f;
                        if (FollCam == null) {
                            FollCam = Engine.Instance.MainCamera.gameObject.GetComponent<FollowCamera>();
                        }
                        FollCam.Distance += BSC.Radius * 1.25f;
                    } else {
                        PC.transform.Rotation *= Quaternion.RotationAxis(PC.transform.TransformMatrix.Up,
                            MathUtil.DegreesToRadians(180f));
                    }
                }
            });
            go.AddComponent(BSC);
        }
    }
}
