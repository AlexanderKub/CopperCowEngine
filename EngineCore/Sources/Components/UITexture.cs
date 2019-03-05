using AssetsManager.Loaders;
using SharpDX;
using SharpDX.Direct3D11;

namespace EngineCore
{
    class UITexture : GameObject
    {
        public UITexture(string name, string texturePath, Vector2 position, Vector2 scale, ShaderResourceView targertTexture) :base(name) {
            AddComponent(new UIRenderer() {
                gameObject = this,
                TexturePath = texturePath,
                Geometry = new ModelGeometry(
                    new Vector3[] {
                        new Vector3(-0.5f, 0.5f, 0),
                        new Vector3(-0.5f, -0.5f, 0),
                        new Vector3(0.5f, -0.5f, 0),
                        new Vector3(0.5f, 0.5f, 0),
                    },
                    new Vector4[] {
                        Vector4.One,
                        Vector4.One,
                        Vector4.One,
                        Vector4.One,
                    },
                    new Vector2[] {
                        new Vector2(1f, 0),
                        new Vector2(1f, 1f),
                        new Vector2(0, 1f),
                        new Vector2(0, 0),
                    },
                    new int[] {
                        0, 2, 1,
                        3, 2, 0,
                    },
                    null,
                    new Vector3[] {
                        new Vector3(1f, 1f, 0),
                        new Vector3(1f, 1f, 0),
                        new Vector3(1f, 1f, 0),
                        new Vector3(1f, 1f, 0),
                    }
                ),
                TargetTexture = targertTexture,
            });
            AddComponent(new Transform() {
                gameObject = this,
                WorldPosition = new Vector3(position, 0),
                WorldScale = new Vector3(scale, 1),
                WorldRotation = Quaternion.Identity,
            });
        }
    }
}
