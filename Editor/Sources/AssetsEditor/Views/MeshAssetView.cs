using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class MeshAssetView: BaseAssetView
    {
        private GameObject PreviewGO;
        private Renderer PreviewRenderer;
        private GameObject ViewGO;

        public override void Init(PreviewEngine engine) {
            base.Init(engine);
            PreviewRenderer = new Renderer() {
                Geometry = Primitives.Sphere(30),
                RendererMaterial = AssetsLoader.LoadMaterial("CopperMaterial"),
            };
            PreviewGO = EngineRef.AddGameObject(
                "TestMesh",
                new Transform() {
                    Position = Vector3.Zero,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.002f,
                },
                PreviewRenderer
            );
            PreviewGO.SelfActive = false;
            PreviewGO.AddComponent(new PreviewBehaviour());

            ViewGO = EngineRef.AddGameObject(
                "MeshView",
                new Transform() {
                    Position = Vector3.Zero,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                },
                null
            );
            ViewGO.AddComponent(new FollowPreviewBehaviour(PreviewGO.transform));

            Primitives.CeilSizeX = 12;
            Primitives.CeilSizeY = 12;
            GameObject Ceil = EngineRef.AddGameObject(
                "Ceil",
                new Transform() {
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 6.6666f * 0.075f,
                    Position = Vector3.Zero,
                    Parent = ViewGO.transform,
                },
                new Renderer() {
                    Topology = PrimitiveTopology.LineList,
                    Geometry = Primitives.Ceil,
                    RendererMaterial = Material.DefaultMaterial,
                    CustomPropertyBlock = new MaterialPropetyBlock() {
                        AlbedoColor = Vector3.One * 0.2f,
                    },
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );

            EngineRef.AddGameObject(
                "MeshView",
                new Transform() {
                    Position = Vector3.Zero,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One * 0.075f,
                    Parent = ViewGO.transform,
                },
                new Renderer() {
                    Geometry = Primitives.Sphere(16),
                    RendererMaterial = Material.DefaultMaterial,
                    CustomPropertyBlock = new MaterialPropetyBlock() {
                        AlbedoColor = Vector3.One,
                    },
                    SpecificType = Renderer.SpecificTypeEnum.Unlit,
                }
            );

            string[] axis = new string[] { "OX", "OY", "OZ" };
            Vector3[] Offsets = new Vector3[] { Vector3.Right, Vector3.Up, Vector3.ForwardLH };
            for (int i = 0; i < 3; i++) {
                EngineRef.AddGameObject(
                    "MeshView" + axis,
                    new Transform() {
                        Position = Offsets[i] * 2f * 0.075f,
                        Rotation = Quaternion.Identity,
                        Scale = new Vector3(i == 0 ? 4f : 0.4f, i == 1 ? 4f : 0.4f, i == 2 ? 4f : 0.4f) * 0.075f,
                        Parent = ViewGO.transform,
                    },
                    new Renderer() {
                        Geometry = Primitives.Cube(),
                        RendererMaterial = Material.DefaultMaterial,
                        CustomPropertyBlock = new MaterialPropetyBlock() {
                            AlbedoColor = Offsets[i],
                        },
                        SpecificType = Renderer.SpecificTypeEnum.Unlit,
                    }
                );
            }
            ViewGO.SelfActive = false;
        }

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, false);
            PreviewGO.GetComponent<PreviewBehaviour>().Reset();
            EngineRef.MainCamera.gameObject.transform.Position = new Vector3(0f, 1f, 2f);
            EngineRef.MainCamera.gameObject.transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, MathUtil.Pi * 0.15f, 0);

            PreviewGO.transform.Position = Vector3.Zero;
            PreviewGO.GetComponent<Renderer>().UpdateMesh(AssetsLoader.LoadMesh(assetName));

            PreviewGO.SelfActive = true;
            ViewGO.SelfActive = true;
        }

        public override void Hide() {
            base.Hide();
            PreviewGO.SelfActive = false;
            ViewGO.SelfActive = false;
            // ChangePivotAndFileScale(MeshAssetRef.Pivot, MeshAssetRef.FileScale);
        }

        /*public override void SaveChanging() {
            MeshAssetRef.Pivot = PreviewRenderer.Geometry.Pivot;
            MeshAssetRef.FileScale = PreviewRenderer.Geometry.FileScale;
            AssetsManagerInstance.GetManager().SaveAssetChanging(MeshAssetRef);
        }*/

        public void ChangePivotAndFileScale(Vector3 pivot, float scale) {
            PreviewRenderer.Geometry.Pivot = pivot;
            PreviewRenderer.Geometry.FileScale = scale;
            PreviewRenderer.UpdateMesh();
        }

        public override void ChangeZoom(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().ScaleOffset = value;
        }
        public override void ChangeYaw(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Yaw = MathUtil.Pi * 2f - value;
        }
        public override void ChangePitch(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Pitch = MathUtil.Pi * 2f - value;
        }
    }
}
