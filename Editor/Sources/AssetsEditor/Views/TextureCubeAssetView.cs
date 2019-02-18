using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class TextureCubeAssetView : BaseAssetView
    {
        private GameObject PreviewGO;
        private Material TestMaterial;

        public override void Init(PreviewEngine engine) {
            base.Init(engine);
            TestMaterial = new Material() {
                Name = "M_TestCubeTexture",
                PropetyBlock = new MaterialPropetyBlock() {
                    MetallicValue = -1.0f,
                    Tile = Vector2.One,
                },
            };
            TestMaterial.LoadMapsAndInitSampler();

            PreviewGO = EngineRef.AddGameObject(
                "TestCubeMapMesh",
                new Transform() {
                    Position = Vector3.Zero,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.002f,
                },
                new Renderer() {
                    Geometry = Primitives.Sphere(46),
                    RendererMaterial = TestMaterial,
                    SpecificType = Renderer.SpecificTypeEnum.ReflectionSphere,
                }
            );
            PreviewGO.AddComponent(new PreviewBehaviour());
            PreviewGO.SelfActive = false;
        }

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(false, false, false, true, false);
            PreviewGO.GetComponent<PreviewBehaviour>().Reset();
            EngineRef.MainCamera.gameObject.transform.Position = new Vector3(0f, 0, 1f);
            EngineRef.MainCamera.gameObject.transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
            PreviewGO.SelfActive = true;

            TestMaterial.AlbedoMapAsset = assetName;
            TestMaterial.LoadMapsAndInitSampler();
        }

        public override void Hide() {
            base.Hide();
            PreviewGO.SelfActive = false;
        }

        public override void ChangeZoom(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().ScaleOffset = value;
        }
        public override void ChangeYaw(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Yaw = value;
        }
        public override void ChangePitch(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Pitch = value;
        }
        public override void ChangePosView(int v) {
            Vector3 pos = Vector3.Zero;
            Quaternion rot = Quaternion.Zero;
            switch (v) {
                case 0:
                    //Forward
                    pos = Vector3.ForwardLH * 1f;
                    rot = Quaternion.LookAtLH(pos, Vector3.Zero, Vector3.Up);
                    break;
                case 1:
                    //Back
                    pos = Vector3.BackwardLH * 1f;
                    rot = Quaternion.LookAtLH(pos, Vector3.Zero, Vector3.Up);
                    break;
                case 2:
                    //Top
                    pos = Vector3.Up * 1f;
                    rot = Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.50001f, 0);
                    break;
                case 3:
                    //Bottom
                    pos = Vector3.Down * 1f;
                    rot = Quaternion.RotationYawPitchRoll(0, -MathUtil.Pi * 0.50001f, 0);
                    break;
                case 4:
                    //Right
                    pos = Vector3.Right * 1f;
                    rot = Quaternion.LookAtLH(Vector3.Zero, pos, Vector3.Up);
                    break;
                case 5:
                    //Left
                    pos = Vector3.Left * 1f;
                    rot = Quaternion.LookAtLH(Vector3.Zero, pos, Vector3.Up);
                    break;
                default:
                    break;
            }
            EngineRef.MainCamera.gameObject.transform.Position = pos;
            EngineRef.MainCamera.gameObject.transform.Rotation = rot;
        }
    }
}
