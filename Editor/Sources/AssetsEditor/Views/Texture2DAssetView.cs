using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using EngineCore.ECS;
using EngineCore.ECS.Components;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class Texture2DAssetView : BaseAssetView
    {
        private Entity PreviewEntity;
        private Renderer RendererComponent;
        private Material TestMaterial;
        private float ratio = 1f;

        public override void Init(PreviewEngine engine)
        {
            base.Init(engine);
            PreviewEntity = EngineRef.ECSWorld.CreateEntityWith<Transform, Renderer>("TestTextureEntity");

            PreviewEntity.GetComponent<Transform>().SetTransformations(Vector3.Zero, Quaternion.Identity, new Vector3(1, 1, 0.1f));
            RendererComponent = PreviewEntity.GetComponent<Renderer>();

            EngineRef.ECSWorld.Refresh();

            TestMaterial = new Material(new EngineCore.ShaderGraph.MetaMaterial()
            {
                shadingMode = EngineCore.ShaderGraph.MetaMaterial.ShadingMode.Unlit,
            })
            {
                Name = "M_TextureTest",
                AlbedoMapAsset = "",
                PropetyBlock = new MaterialPropetyBlock()
                {
                    MetallicValue = 0.5f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One,
                },
            };
            RendererComponent.SetMeshAndMaterial(AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube),
                AssetsLoader.LoadMaterialInfo(TestMaterial));
            /*PreviewGO = EngineRef.AddGameObject("TestQuad");
            PreviewGO.transform.Position = Vector3.Zero;
            PreviewGO.transform.Rotation = Quaternion.Identity;
            PreviewGO.transform.Scale = (Vector3.Right + Vector3.Up) * 3.85f + Vector3.ForwardLH * 0.1f;
            PreviewGO.GetComponent<DeprecatedRenderer>().SpecificType = DeprecatedRenderer.SpecificTypeEnum.Unlit;
            PreviewGO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.PlaneWithUV, TestMaterial);
            PreviewGO.SelfActive = false;*/
        }

        public override void Show(string assetName)
        {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(false, false, false, false, false);

            EngineRef.CameraEntity.GetComponent<Transform>().SetTransformations(new Vector3(0f, 0, -2f),
                Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0));

            PreviewEntity.SetActive(true);
            EngineRef.ECSWorld.Refresh();

            Texture2DAsset asset = AssetsManagerInstance.GetManager().LoadAsset<Texture2DAsset>(assetName);
            ratio = (float)asset.Data.Width / asset.Data.Height;
            Vector3 scale;
            if (ratio >= 1) {
                scale = (Vector3.Right + Vector3.Up / ratio) * 3.85f;
            } else {
                scale = (Vector3.Right + Vector3.Up * ratio) * 3.85f;
            }
            scale += Vector3.ForwardLH * 0.1f;
            PreviewEntity.GetComponent<Transform>().Scale = scale;

            TestMaterial.AlbedoMapAsset = assetName;
        }

        public override void Hide()
        {
            base.Hide();
            PreviewEntity.SetActive(false);
            EngineRef.ECSWorld.Refresh();
            AssetsLoader.DropCachedTexture(TestMaterial.AlbedoMapAsset);
        }

    }
}
