using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetsManager;
using EngineCore;
using static EngineCore.Engine;
using EngineCore.ShaderGraph;
using ThreadsQueuing;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin;
using CopperCowEngine.ECS.Builtin.Components;
using SharpDX;

namespace ECSTestProject
{
    public class TestEcsASystem : ComponentSystem<Required<LocalToWorld>>
    {
        protected override void Update()
        {
            foreach (var e in Iterator)
            {
                var matrix = e.Sibling<LocalToWorld>().Value;
                //Console.WriteLine(matrix.ToString());
            }
        }
    }

    public class TranslationSystem : ComponentSystem<Required<Translation>>
    {
        //bool removed = false;

        protected override void Update()
        {
            foreach (var e in Iterator)
            {
                ref var translation = ref e.Sibling<Translation>();
                translation.Value += Vector3.Up;

                /*if (!removed)
                {
                    removed = true;
                    Context.RemoveComponent<Translation>(e.Entity);
                }*/
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            /*var context = new EngineEcsContext();

            context.CreateSystem<TranslationSystem>();

            context.CreateSystem<TestEcsASystem>();

            for (var i = 0; i < 10000; i++)
            {
                context.CreateEntity(new LocalToWorld()
                {
                    Value = Matrix.Identity,
                }, new Translation()
                {
                    Value = Vector3.Up,
                });
            }

            var stopwatch = new Stopwatch();

            for (var i = 0; i < 10; i++)
            {
                stopwatch.Restart();
                context.Update();
                Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
            }

            Console.ReadKey();
            return;*/

            var AM = AssetsManagerInstance.GetManager();

            #region PreRenderUtils

            bool startPreRenderUtils = false;
            //startPreRenderUtils = true;
            if (startPreRenderUtils) {
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_SphereToCubeMapVS", "VS_SphereToCubeMap", true);
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_SphereToCubeMapPS", "PS_SphereToCubeMap", true);
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_IrradiancePS", "PS_Irradiance", true);
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_PreFilteredPS", "PS_PreFiltered", true);
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_IntegrateBRDFxPS", "PS_IntegrateBRDF", true);
                //AM.ImportShaderAsset(@"Commons\IBL_AssetsPreRender.hlsl", "IBL_PR_IntegrateQuadVS", "VS_IntegrateQuad", true);


                //AssetsManagerInstance.GetManager().CubeMapPrerender("Mt-Washington-Cave-Room_Ref.hdr", "House");
                AssetsManagerInstance.GetManager().CubeMapPrerender("C:\\Repos\\CopperCowEngine\\RawContent\\Tokyo_BigSight_3k.hdr", "House");
                //AssetsManagerInstance.GetManager().CubeMapPrerender("moonless_golf_2k.hdr", "House");
                //AssetsManagerInstance.GetManager().BRDFIntegrate("StandardBRDFxLUT");

                Console.ReadKey();
                return;
            }

            #endregion

            //var t = new TestShadersCompiler();
            //t.TestItAll();
            // Console.ReadKey();
            /*MultiThreadQueue test = new MultiThreadQueue(2);
            for (int i = 0; i < 100; i++)
            {
                test.Enqueue("Test" + i);
            }
            Console.ReadKey();*/

            //ImportContent();
            Console.ReadKey();

            EngineConfiguration config = new EngineConfiguration()
            {
                AppName = "ECS SandBox",
                RenderBackend = EngineConfiguration.RenderBackendEnum.D3D11,
                RenderPath = RenderPathEnum.Deffered,
                //RenderPath = RenderPathEnum.Forward,
                EnableHDR = true,
                EnableMSAA = EngineConfiguration.MSAAEnabled.X4,
                DebugMode = true,
            };

            //Game game = new Game();
            PBRTest game = new PBRTest();
            game.Run(config);
        }

        #region Content import
        private static string RawContentPath = @"C:\Repos\CopperCowEngine\RawContent\";

        private static void ImportContent()
        {
            var AM = AssetsManagerInstance.GetManager();
            HotReload_NOT(AM);
            //ImportMeshes(AM);
            //ImportTextures(AM);
            //CreateMaterials(AM);
            //ImportShaders(AM);
        }

        private static void HotReload_NOT(AssetsManagerInstance AM)
        {
            AM.ImportAsset(@"Deffered\CommonVS.hlsl", "CommonVS", true);
            //AM.ImportShaderAsset(@"Commons\ScreenQuadPS.hlsl", "ScreenQuadPS", "PSMain", true);
            //AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "MSAA_LightPassPS", "PSMain", "MSAA", true);
            //AM.ImportShaderAsset(@"Commons\ScreenQuadPS.hlsl", "MSAA_ScreenQuadPS", "PSMain", "MSAA", true);

            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassPS", "PSMain", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferPS", "FillGBufferPS", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferSkyboxPS", "FillGBufferPS",
                "TEXTURE_CUBE_ALBEDO_MAP", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferMaskedPS", "FillGBufferPS",
                "MASKED", true);
            //AM.ImportShaderAsset(@"Deffered\DownscalingCS.hlsl", "DownsamplingFirstCS", "DownScaleFirstPass", true);
            //AM.ImportShaderAsset(@"Deffered\DownscalingCS.hlsl", "DownsamplingSecondCS", "DownScaleSecondPass", true);
            AM.ImportShaderAsset(@"Commons\ScreenQuadPS.hlsl", "ScreenQuadPS", "PSMain", true);
            AM.ImportShaderAsset(@"Commons\ScreenQuadVS.hlsl", "ScreenQuadVS", "VSMain", true);

            Dictionary<string, string> pathNamePairs = new Dictionary<string, string>();
            //pathNamePairs.Add("Textures/SnowRock/rock-snow-ice1-2k_Base_Color.png", "SnowRockAlbedoMap");
            foreach (string path in pathNamePairs.Keys) {
                AM.ImportAsset(RawContentPath + path, pathNamePairs[path], true);
            }
            //AM.CreateMaterialAsset("SnowRockMaterial", "SnowRockAlbedoMap",
            //    "SnowRockNormalMap", "SnowRockRoughnessMap", 
            //    "SnowRockMetallicMap", "SnowRockOcclusionMap");
        }

        private static void ImportShaders(AssetsManagerInstance AM)
        {
            AM.ImportAsset(@"Commons\CommonVS.hlsl", "CommonVS", true);

            AM.ImportShaderAsset(@"Deffered\LightVolumes.hlsl", "LightVolumesVS", "LightVolumesVS", true);
            AM.ImportShaderAsset(@"Deffered\LightVolumes.hlsl", "LightVolumesHS", "LightVolumesHS", true);
            AM.ImportShaderAsset(@"Deffered\LightVolumes.hlsl", "PointLightVolumeDS", "LightVolumeDS", "POINT_LIGHT", true);
            AM.ImportShaderAsset(@"Deffered\LightVolumes.hlsl", "SpotLightVolumeDS", "LightVolumeDS", "SPOT_LIGHT", true);
            AM.ImportShaderAsset(@"Deffered\LightVolumes.hlsl", "CapsuleLightVolumeDS", "LightVolumeDS", "CAPSULE_LIGHT", true);

            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferVS", "FillGBufferVS", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferPS", "FillGBufferPS", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferSkyboxPS", "FillGBufferPS",
                "TEXTURE_CUBE_ALBEDO_MAP", true);
            AM.ImportShaderAsset(@"Deffered\FillGBuffer.hlsl", "FillGBufferMaskedPS", "FillGBufferPS",
                "MASKED", true);

            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassVS", "VSMain", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassPS", "PSMain", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "MSAA_LightPassPS", "PSMain", "MSAA", true);

            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassDirectionalPS", "PSDirectionalLight", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassDirectionalPS", "PSDirectionalLight", "MSAA", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassPointPS", "PSPointLight", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "MSAA_LightPassPointPS", "PSPointLight", "MSAA", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "LightPassPointQuadPS", "PSPointLight", "SQUAD", true);
            AM.ImportShaderAsset(@"Deffered\LightPass.hlsl", "MSAA_LightPassPointQuadPS", "PSPointLight", "SQUAD", "MSAA", true);

            AM.ImportShaderAsset(@"Commons\ScreenQuadPS.hlsl", "ScreenQuadPS", "PSMain", true);
            AM.ImportShaderAsset(@"Commons\ScreenQuadPS.hlsl", "MSAA_ScreenQuadPS", "PSMain", "MSAA", true);
            AM.ImportAsset(@"Commons\ScreenQuadVS.hlsl", "ScreenQuadVS", true);

            //Forward
            AM.ImportAsset(@"PBR\PBRForwardPS.hlsl", "PBRForwardPS", true);
            AM.ImportAsset(@"Unlit\FwdSkySpherePS.hlsl", "FwdSkySpherePS", true);
            AM.ImportAsset(@"ForwardPlus\LightCullingCS.hlsl", "LightCullingCS");
            AM.ImportAsset(@"ForwardPlus\ForwardPlusPosOnlyVS.hlsl", "ForwardPlusPosOnlyVS", true);
            AM.ImportAsset(@"ForwardPlus\ForwardPlusPosTexVS.hlsl", "ForwardPlusPosTexVS", true);
            AM.ImportAsset(@"ForwardPlus\ForwardPlusPosTexPS.hlsl", "ForwardPlusPosTexPS", true);
            AM.ImportAsset(@"ForwardPlus\ForwardPlusScenePS.hlsl", "ForwardPlusScenePS", true);

            AM.ImportAsset(@"Unlit\ReflectionSpherePS.hlsl", "ReflectionSpherePS", true);

            AM.ImportAsset(@"Commons\DepthShadowsVS.hlsl", "DepthShadowsVS", true);
            AM.ImportAsset(@"Commons\DepthShadowsPS.hlsl", "DepthShadowsPS", true);
            AM.ImportAsset(@"Commons\DownSamplingPS.hlsl", "DownSamplingPS", true);
            AM.ImportAsset(@"Unlit\VelocityPS.hlsl", "VelocityPS", true);
            AM.ImportAsset(@"Unlit\MaskedVelocityPS.hlsl", "MaskedVelocityPS", true);
        }

        private struct NamePlusScale
        {
            public string name;
            public float scale;

            public NamePlusScale(string n, float s)
            {
                name = n;
                scale = s;
            }

            public NamePlusScale(string n)
            {
                name = n;
                scale = 1.0f;
            }
        }

        private static void ImportMeshes(AssetsManagerInstance AM)
        {

            Dictionary<string, NamePlusScale> pathNamePairs = new Dictionary<string, NamePlusScale>();

            pathNamePairs.Add("Sponza.fbx", new NamePlusScale("SponzaMesh", 0.03f));

            pathNamePairs.Add("Cow.obj", new NamePlusScale("CowMesh", 0.0033f));

            pathNamePairs.Add("skysphere_mesh.FBX", new NamePlusScale("SkyDomeMesh"));
            pathNamePairs.Add("cube1m.FBX", new NamePlusScale("Cube1mMesh"));

            foreach (string path in pathNamePairs.Keys) {
                AM.CreateMeshAsset(RawContentPath + path, pathNamePairs[path].name, pathNamePairs[path].scale);
            }
        }

        private static void ImportTextures(AssetsManagerInstance AM)
        {
            Dictionary<string, string> pathNamePairs = new Dictionary<string, string>();

            pathNamePairs.Add(@"Textures\WoodenLatticeMap.png", "WoodenLatticeMap");
            pathNamePairs.Add(@"SponzaMaps\sponza_floor_a_diff.png", "SponzaFloorAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_floor_a_diff_NRM.png", "SponzaFloorNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_roof_diff.png", "SponzaRoofAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_roof_diff_NRM.png", "SponzaRoofNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_a_diff.png", "SponzaColumnAAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_a_ddn.png", "SponzaColumnANormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_b_diff.png", "SponzaColumnBAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_b_ddn.png", "SponzaColumnBNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_c_diff.png", "SponzaColumnCAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_column_c_ddn.png", "SponzaColumnCNormal");
            pathNamePairs.Add(@"SponzaMaps\lion.png", "LionAlbedo");
            pathNamePairs.Add(@"SponzaMaps\lion_ddn.png", "LionNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_curtain_blue_diff.png", "SponzaCurtainBlueAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_curtain_blue_diff_NRM.png", "SponzaCurtainBlueNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_curtain_diff.png", "SponzaCurtainRedAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_curtain_green_diff.png", "SponzaCurtainGreenAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_fabric_blue_diff.png", "SponzaFabricBlueAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_fabric_blue_diff_NRM.png", "SponzaFabricBlueNormal");
            pathNamePairs.Add(@"SponzaMaps\sponza_fabric_diff.png", "SponzaFabricRedAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_fabric_green_diff.png", "SponzaFabricGreenAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_arch_diff.png", "SponzaArcAlbedo");
            pathNamePairs.Add(@"SponzaMaps\sponza_arch_diff_NRM.png", "SponzaArcNormal");

            pathNamePairs.Add(@"Textures\MetalMat\oxidized-copper-albedo.png", "CopperAlbedoMap");
            pathNamePairs.Add(@"Textures\MetalMat\oxidized-copper-normal-ue.png", "CopperNormalMap");
            pathNamePairs.Add(@"Textures\MetalMat\oxidized-copper-roughness.png", "CopperRoughnessMap");
            pathNamePairs.Add(@"Textures\MetalMat\oxidized-copper-metal.png", "CopperMetallicMap");
            pathNamePairs.Add(@"Textures\DebugTextureGrid.jpg", "DebugTextureMap");
            pathNamePairs.Add(@"Textures\cow.jpg", "CowAlbedoMap");

            pathNamePairs.Add(@"Textures\MetalMat\metal-splotchy-albedo.png", "MetalSplotchyAlbedoMap");
            pathNamePairs.Add(@"Textures\MetalMat\metal-splotchy-normal-dx.png", "MetalSplotchyNormalMap");
            pathNamePairs.Add(@"Textures\MetalMat\metal-splotchy-rough.png", "MetalSplotchyRoughnessMap");
            pathNamePairs.Add(@"Textures\MetalMat\metal-splotchy-metal.png", "MetalSplotchyMetallicMap");

            pathNamePairs.Add(@"Textures\SnowRock\rock-snow-ice1-2k_Base_Color.png", "SnowRockAlbedoMap");
            pathNamePairs.Add(@"Textures\SnowRock\rock-snow-ice1-2k_Normal-dx.png", "SnowRockNormalMap");
            pathNamePairs.Add(@"Textures\SnowRock\rock-snow-ice1-2k_Roughness.png", "SnowRockRoughnessMap");
            pathNamePairs.Add(@"Textures\SnowRock\rock-snow-ice1-2k_Metallic.png", "SnowRockMetallicMap");
            pathNamePairs.Add(@"Textures\SnowRock\rock-snow-ice1-2k_Ambient_Occlusion.png", "SnowRockOcclusionMap");

            foreach (string path in pathNamePairs.Keys) {
                AM.ImportAsset(RawContentPath + path, pathNamePairs[path], true);
            }
        }

        private static void CreateMaterials(AssetsManagerInstance AM)
        {
            AM.CreateMaterialAsset("SponzaCurtainBlueMaterial", "SponzaCurtainBlueAlbedo", "SponzaCurtainBlueNormal");
            AM.CreateMaterialAsset("SponzaCurtainRedMaterial", "SponzaCurtainRedAlbedo", "SponzaCurtainBlueNormal");
            AM.CreateMaterialAsset("SponzaCurtainGreenMaterial", "SponzaCurtainGreenAlbedo", "SponzaCurtainBlueNormal");
            AM.CreateMaterialAsset("SponzaFabricBlueMaterial", "SponzaFabricBlueAlbedo", "SponzaFabricBlueNormal");
            AM.CreateMaterialAsset("SponzaFabricRedMaterial", "SponzaFabricRedAlbedo", "SponzaFabricBlueNormal");
            AM.CreateMaterialAsset("SponzaFabricGreenMaterial", "SponzaFabricGreenAlbedo", "SponzaFabricBlueNormal");
            AM.CreateMaterialAsset("SponzaArchMaterial", "SponzaArcAlbedo", "SponzaArcNormal");
            AM.CreateMaterialAsset("SponzaFloorMaterial", "SponzaFloorAlbedo", "SponzaFloorNormal");
            AM.CreateMaterialAsset("SponzaRoofMaterial", "SponzaRoofAlbedo", "SponzaRoofNormal");
            AM.CreateMaterialAsset("SponzaColumnAMaterial", "SponzaColumnAAlbedo", "SponzaColumnANormal");
            AM.CreateMaterialAsset("SponzaColumnBMaterial", "SponzaColumnBAlbedo", "SponzaColumnBNormal");
            AM.CreateMaterialAsset("SponzaColumnCMaterial", "SponzaColumnCAlbedo", "SponzaColumnCNormal");
            AM.CreateMaterialAsset("LionMaterial", "LionAlbedo", "LionNormal");

            AM.CreateMaterialAsset("CopperMaterial", "CopperAlbedoMap", "CopperNormalMap", "CopperRoughnessMap", 
                "CopperMetallicMap");

            AM.CreateMaterialAsset("MetalSplotchyMaterial", "MetalSplotchyAlbedoMap",
                "MetalSplotchyNormalMap", "MetalSplotchyRoughnessMap", "MetalSplotchyMetallicMap");

            AM.CreateMaterialAsset("SnowRockMaterial", "SnowRockAlbedoMap", "SnowRockNormalMap", 
                "SnowRockRoughnessMap", "SnowRockMetallicMap", "SnowRockOcclusionMap");
        }
        #endregion
    }
}
