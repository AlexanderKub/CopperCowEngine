using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11.Editor;

namespace PureProject
{
    internal static class ShaderCompiler
    {
        public static void CompileShaders()
        {
            CreateMaterials();

            // Depth Pre Pass
            /*D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthPrePassVS", 
                "VSMain");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthMaskedPrePassVS", 
                "VSMain", "MASKED");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthAndVelocityPrePassVS", 
                "VSMain","VELOCITY");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthAndVelocityMaskedPrePassVS", 
                "VSMain", "VELOCITY", "MASKED");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthMaskedPrePassPS", 
                "PSMain", "MASKED");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthAndVelocityPrePassPS", 
                "PSMain", "VELOCITY");
            D3D11AssetsImporter.CompileShader(@"Forward\DepthPrePass.hlsl",
                "DepthAndVelocityMaskedPrePassPS", 
                "PSMain", "VELOCITY", "MASKED");*/
            
            // Forward Lightning
            D3D11AssetsImporter.CompileShader(@"Forward\StandardLightPass2.hlsl",
                "ForwardStandardVS", 
                "VSMain");
            D3D11AssetsImporter.CompileShader(@"Forward\StandardLightPass2.hlsl",
                "ForwardStandardPS", 
                "PSMain", "NON_TEXTURED", "LDR");
            D3D11AssetsImporter.CompileShader(@"Forward\StandardLightPass2.hlsl",
                "ForwardStandardTexturedPS", 
                "PSMain", "LDR");
            D3D11AssetsImporter.CompileShader(@"Forward\StandardLightPass2.hlsl",
                "ForwardStandardHdrPS", 
                "PSMain", "NON_TEXTURED");
            D3D11AssetsImporter.CompileShader(@"Forward\StandardLightPass2.hlsl",
                "ForwardStandardHdrTexturedPS", 
                "PSMain");
            
            /*D3D11AssetsImporter.CompileShader(@"Forward\SkyDomePass.hlsl",
                "ForwardSkyDomePS", 
                "PSMain", "LDR");
            D3D11AssetsImporter.CompileShader(@"Forward\SkyDomePass.hlsl",
                "ForwardSkyDomeHdrPS", 
                "PSMain");*/
            
            // Down Scaling
            /*D3D11AssetsImporter.CompileShader(@"Common\DownScalingComputeShader.hlsl",
                "DownsamplingFirstCS", 
                "DownScaleFirstPass");
            D3D11AssetsImporter.CompileShader(@"Common\DownScalingComputeShader.hlsl",
                "DownsamplingFirstMsaaCS", 
                "DownScaleFirstPass", "MSAA");
            D3D11AssetsImporter.CompileShader(@"Common\DownScalingComputeShader.hlsl",
                "DownsamplingWithBlurFirstCS", 
                "DownScaleFirstPass", "BLUR");
            D3D11AssetsImporter.CompileShader(@"Common\DownScalingComputeShader.hlsl",
                "DownsamplingWithBlurFirstMsaaCS", 
                "DownScaleFirstPass", "MSAA", "BLUR");
            
            D3D11AssetsImporter.CompileShader(@"Common\DownScalingComputeShader.hlsl",
                "DownsamplingSecondCS", 
                "DownScaleSecondPass");
            D3D11AssetsImporter.CompileShader(@"Common\Bloom.hlsl",
                "BloomCS", 
                "BrightPass");
            D3D11AssetsImporter.CompileShader(@"Common\BloomFilter.hlsl",
                "BloomVerticalFilterCS", 
                "VerticalFilterPass");
            D3D11AssetsImporter.CompileShader(@"Common\BloomFilter.hlsl",
                "BloomHorizontalFilterCS", 
                "HorizontalFilterPass");*/
            
            // Postprocessing
            D3D11AssetsImporter.CompileShader(@"Common\ScreenQuad.hlsl",
                "ScreenQuadVS", 
                "VSMain");
            D3D11AssetsImporter.CompileShader(@"Common\ScreenQuad.hlsl",
                "ScreenQuadHdrPS", 
                "PSMain");
            D3D11AssetsImporter.CompileShader(@"Common\ScreenQuad.hlsl",
                "ScreenQuadHdrMsaaPS", 
                "PSMain", "MSAA");
            D3D11AssetsImporter.CompileShader(@"Common\ScreenQuad.hlsl",
                "ScreenQuadHdrBloomPS", 
                "PSMain", "BLOOM");
            D3D11AssetsImporter.CompileShader(@"Common\ScreenQuad.hlsl",
                "ScreenQuadHdrBloomMsaaPS", 
                "PSMain", "MSAA", "BLOOM");
            
            // IBL Prerender
            /*D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_SphereToCubeMapVS", 
                "VS_SphereToCubeMap");
            D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_SphereToCubeMapPS", 
                "PS_SphereToCubeMap");
            D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_IrradiancePS", 
                "PS_Irradiance");
            D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_PreFilteredPS", 
                "PS_PreFiltered");
            D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_IntegrateBRDFxPS", 
                "PS_IntegrateBRDF");
            D3D11AssetsImporter.CompileShader(@"Editor\IBL_AssetsPreRender.hlsl",
                "IBL_PR_IntegrateQuadVS", 
                "VS_IntegrateQuad");
            
            D3D11AssetsImporter.CompileShader(@"Editor\PBR_IBL_ConvertEquirecToCube.hlsl",
                "PBR_IBL_EquirecToCubeCS", 
                "CSMain");
            D3D11AssetsImporter.CompileShader(@"Editor\PBR_IBL_PreComputeIrradiance.hlsl",
                "PBR_IBL_IrradianceCS", 
                "CSMain");
            D3D11AssetsImporter.CompileShader(@"Editor\PBR_IBL_PreComputeFiltered.hlsl",
                "PBR_IBL_PreFilteredCS", 
                "CSMain");
            D3D11AssetsImporter.CompileShader(@"Editor\PBR_PreComputeBrdf.hlsl",
                "PBR_IBL_BrdfCS", 
                "CSMain");*/
            

            //D3D11AssetsImporter.CubeMapPrerender("C:\\Repos\\CopperCowEngine\\RawContent\\Tokyo_BigSight_3k.hdr", "Tokio");
            //D3D11AssetsImporter.CubeMapPrerender("C:\\Repos\\CopperCowEngine\\RawContent\\moonless_golf_2k.hdr", "Tokio");
            
            //D3D11AssetsImporter.CubeMapPrerender("C:\\Repos\\CopperCowEngine\\RawContent\\Mt-Washington-Cave-Room_Ref.hdr", "Tokio");
            //D3D11AssetsImporter.CubeMapPrerender("C:\\Repos\\CopperCowEngine\\RawContent\\kiara_1_dawn_2k.hdr", "Tokio");
            //D3D11AssetsImporter.BrdfIntegrate("StandardBRDFxLUT", true);
            
            //EditorAssetsManager.GetManager().CreateMeshAsset("C:\\Repos\\CopperCowEngine\\RawContent\\Models_F0701A047\\FarmCow.fbx", "CowMesh", 0.033f);
            //EditorAssetsManager.GetManager().CreateMeshAsset("C:\\Repos\\CopperCowEngine\\RawContent\\Cow.obj", "CowMesh", 0.0033f);
            /*EditorAssetsManager.GetManager().CreateMeshAsset("C:\\Repos\\CopperCowEngine\\RawContent\\cube1m.FBX", "Cube1mMesh");
            EditorAssetsManager.GetManager().CreateMeshAsset("C:\\Repos\\CopperCowEngine\\RawContent\\skysphere_mesh.FBX", "SkyDomeMesh");*/

            Console.ReadKey();
        }

        private static void CreateMaterials()
        {
            var manager = EditorAssetsManager.GetManager();
            manager.CreateAssetFile(new MaterialAsset
            {
                Name = "CopperMaterial",
                EmissiveColor = Vector3.Zero,
                AlbedoColor = Vector3.One,
                AlphaValue = 1,
                MetallicValue = 0.5f,
                RoughnessValue = 0.5f,
                Tile = Vector2.One * 8f,
                Shift = Vector2.Zero,
                AlbedoMapAsset = "CopperAlbedoMap",
                EmissiveMapAsset = null,
                MetallicMapAsset = "CopperMetallicMap",
                NormalMapAsset = "CopperNormalMap",
                OcclusionMapAsset = null,
                RoughnessMapAsset = "CopperRoughnessMap",
            }, true);
            manager.CreateAssetFile(new MaterialAsset
            {
                Name = "SnowRockMaterial",
                EmissiveColor = Vector3.Zero,
                AlbedoColor = Vector3.One,
                AlphaValue = 1,
                MetallicValue = 0.5f,
                RoughnessValue = 0.5f,
                Tile = Vector2.One * 5f,
                Shift = Vector2.Zero,
                AlbedoMapAsset = "SnowRockAlbedoMap",
                EmissiveMapAsset = null,
                MetallicMapAsset = "SnowRockMetallicMap",
                NormalMapAsset = "SnowRockNormalMap",
                OcclusionMapAsset = "SnowRockOcclusionMap",
                RoughnessMapAsset = "SnowRockRoughnessMap",
            }, true);
            manager.CreateAssetFile(new MaterialAsset
            {
                Name = "MetalSplotchyMaterial",
                EmissiveColor = Vector3.Zero,
                AlbedoColor = Vector3.One,
                AlphaValue = 1,
                MetallicValue = 0.5f,
                RoughnessValue = 0.5f,
                Tile = Vector2.One * 7f,
                Shift = Vector2.Zero,
                AlbedoMapAsset = "MetalSplotchyAlbedoMap",
                EmissiveMapAsset = null,
                MetallicMapAsset = "MetalSplotchyMetallicMap",
                NormalMapAsset = "MetalSplotchyNormalMap",
                OcclusionMapAsset = null,
                RoughnessMapAsset = "MetalSplotchyRoughnessMap",
            }, true);
        }

    }
}
