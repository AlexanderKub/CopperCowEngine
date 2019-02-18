﻿using System;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using Editor.AssetsEditor.Views;
using System.Collections.Generic;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace Editor.AssetsEditor
{
    class PreviewEngine: Engine
    {
        public PreviewEngine():base() {

        }

        public PreviewEngine(System.Windows.Forms.Control surface):base(surface) {

        }

        public override void OnStart() {
            ClearColor = Color.Blue;
            CreateScene();
        }

        private void CreateScene() {
            CreateSkySphere();
            SetMainCamera(AddCamera<Camera>("MainCamera", new Vector3(0f, 1f, 0f), Quaternion.RotationYawPitchRoll(MathUtil.Pi * 0.5f, 0, 0)));
            Light LightObj = new Light() {
                ambientColor = Vector4.One * 0.25f,
                radius = 20,
                Type = Light.LightType.Directional,
                EnableShadows = true,
            };
            AddLight("Light", LightObj, new Vector3(0f, 5.5f, 0.1f),
                Quaternion.RotationYawPitchRoll(-MathUtil.Pi * 0.5f, -MathUtil.Pi * 0.5f, 0), true);
            
            AssetViewsInit();
        }

        private MeshAssetView MeshAssetViewRef;
        private Texture2DAssetView Texture2DAssetViewRef;
        private TextureCubeAssetView TextureCubeAssetViewRef;
        private MaterialAssetView MaterialAssetViewRef;
        private BaseAssetView CurrentAssetView;

        private void AssetViewsInit() {
            MeshAssetViewRef = new MeshAssetView();
            MeshAssetViewRef.Init(this);
            Texture2DAssetViewRef = new Texture2DAssetView();
            Texture2DAssetViewRef.Init(this);
            TextureCubeAssetViewRef = new TextureCubeAssetView();
            TextureCubeAssetViewRef.Init(this);
            MaterialAssetViewRef = new MaterialAssetView();
            MaterialAssetViewRef.Init(this);
        }

        public void MeshViewChangePivotAndFileScale(Vector3 pivot, float scale) {
            //MeshAssetViewRef.ChangePivotAndFileScale(pivot, scale);
        }

        public void TestSave() {
            //MeshAssetViewRef.ChangePivotAndFileScale(Vector3.BackwardLH * -0.5f, 1f);
           // MeshAssetViewRef.SaveChanging();
        }

        public void UpdateAssetPreview(BaseAsset asset) {
            CurrentAssetView.Update(asset);
        }
        
        public void PreviewAsset(MetaAsset asset) {
            if (asset == null) {
                return;
            }
            
            CurrentAssetView?.Hide();
            CurrentAssetView = null;

            switch (asset.InfoType) {
                case AssetTypes.Invalid:
                    break;
                case AssetTypes.Mesh:
                    CurrentAssetView = MeshAssetViewRef;
                    break;
                case AssetTypes.Texture2D:
                    CurrentAssetView = Texture2DAssetViewRef;
                    break;
                case AssetTypes.TextureCube:
                    CurrentAssetView = TextureCubeAssetViewRef;
                    break;
                case AssetTypes.Material:
                    CurrentAssetView = MaterialAssetViewRef;
                    break;
                case AssetTypes.Shader:
                    break;
                case AssetTypes.Meta:
                    break;
                default:
                    break;
            }
            CurrentAssetView?.Show(asset.Name);
        }

        #region View UIControls
        public void ChangeZoom(float value) {
            CurrentAssetView?.ChangeZoom(value);
        }

        public void ChangeYaw(float value) {
            CurrentAssetView?.ChangeYaw(value / 180f * MathUtil.Pi);
        }

        public void ChangePitch(float value) {
            CurrentAssetView?.ChangePitch(value / 180f * MathUtil.Pi);
        }

        public void ChangePosView(int v) {
            CurrentAssetView?.ChangePosView(v);
        }

        public Action<bool, bool, bool, bool, bool> OnSetViewsControlsEnabled;
        internal void SetViewsControlsEnabled(bool zoom, bool yaw, bool pitch, bool viewPos, bool meshType) {
            OnSetViewsControlsEnabled?.Invoke(zoom, yaw, pitch, viewPos, meshType);
        }
        #endregion


        public override void Update() {
            if (Input.IsKeyDown(System.Windows.Forms.Keys.Escape)) {
                //Quit();
            }
        }
    }
}
