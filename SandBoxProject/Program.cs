using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineCore;
using AssetsManager;
using AssetsManager.AssetsMeta;

namespace SandBoxProject
{
    class Program
    {
        static void Main(string[] args) {
            //TODO: Image assets, cleanup resources, Asset importer
            bool reimport = false;
            if (reimport) {
                AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
                //Textures import
                AM.ImportAsset("RawContent/Textures/cow.jpg", "CowAlbedoMap");
                AM.ImportAsset("RawContent/Textures/cat.jpg", "CatAlbedoMap");

                AM.ImportAsset("RawContent/Textures/RockFloor/RF_diff.jpg", "RockFloorAlbedoMap");
                AM.ImportAsset("RawContent/Textures/RockFloor/RF_normal.jpg", "RockFloorNormalMap");
                AM.ImportAsset("RawContent/Textures/RockFloor/RF_rough.jpg", "RockFloorRoughnessMap");
                AM.ImportAsset("RawContent/Textures/RockFloor/RF_occ.jpg", "RockFloorOcclusionMap");

                AM.ImportAsset("RawContent/Textures/Rock/Rock_d.png", "RockAlbedoMap");
                AM.ImportAsset("RawContent/Textures/Rock/Rock_n.png", "RockNormalMap");
                AM.ImportAsset("RawContent/Textures/Rock/Rock_ao.png", "RockOcclusionMap");
            
                AM.ImportAsset("RawContent/Textures/textureJPG.jpg", "SoldierAlbedoMap");
            
                AM.ImportAsset("RawContent/Textures/MetalMat/metal-splotchy-albedo.png", "MetalAlbedoMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/metal-splotchy-normal-dx.png", "MetalNormalMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/metal-splotchy-rough.png", "MetalRoughnessMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/metal-splotchy-metal.png", "MetalMetallicMap");

                AM.ImportAsset("RawContent/Textures/MetalMat/oxidized-copper-albedo.png", "CopperAlbedoMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/oxidized-copper-normal-ue.png", "CopperNormalMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/oxidized-copper-roughness.png", "CopperRoughnessMap");
                AM.ImportAsset("RawContent/Textures/MetalMat/oxidized-copper-metal.png", "CopperMetallicMap");

                AM.ImportAsset("RawContent/Textures/SnowRock/rock-snow-ice1-2k_Base_Color.png", "SnowRockAlbedoMap");
                AM.ImportAsset("RawContent/Textures/SnowRock/rock-snow-ice1-2k_Normal-dx.png", "SnowRockNormalMap");
                AM.ImportAsset("RawContent/Textures/SnowRock/rock-snow-ice1-2k_Roughness.png", "SnowRockRoughnessMap");
                AM.ImportAsset("RawContent/Textures/SnowRock/rock-snow-ice1-2k_Metallic.png", "SnowRockMetallicMap");
                AM.ImportAsset("RawContent/Textures/SnowRock/rock-snow-ice1-2k_Ambient_Occlusion.png", "SnowRockOcclusionMap");

                //Meshes import
                AM.ImportAsset("RawContent/Cow.obj", "CowMesh");
                AM.ImportAsset("RawContent/hat.obj", "HatMesh");
                AM.ImportAsset("RawContent/Rock.obj", "RockMesh");
                AM.ImportAsset("RawContent/Cube1m.fbx", "Cube1mMesh");
                AM.ImportAsset("RawContent/soldier_1.fbx", "SoldierMesh");
                AM.ImportAsset("RawContent/skySphere.FBX", "SkySphereMesh");

                //Shaders import
                AM.ImportAsset("PBR/DefferedPBRShader.hlsl", "DefferedPBRShader");
                AM.ImportAsset("PBR/DefferedPBRQuadShader.hlsl", "DefferedPBRQuadShader");
                AM.ImportAsset("Unlit/SkySphereShader.hlsl", "SkySphereShader");
                AM.ImportAsset("Unlit/ReflectionShader.hlsl", "ReflectionShader");

                AM.ImportAsset("Deprecated/DepthShadows.hlsl", "DepthShadows");
                AM.ImportAsset("Deprecated/UIShader.hlsl", "UIShader");
                AM.ImportAsset("Deprecated/ExampleGeometryShader.hlsl", "ExampleGeometryShader");
                AM.ImportAsset("Deprecated/TriangleShader.hlsl", "TriangleShader");

                //CubeMaps import
                AM.CreateCubeMapAsset("RawContent/Textures/Skybox/yellowcloud.jpg", "SkyboxCubeMap");
                AM.CreateCubeMapAsset("RawContent/Textures/Skybox/yellowcloudirrad.bmp", "SkyboxIrradianceCubeMap");
                AM.CreateCubeMapAsset("RawContent/Textures/Skybox/miramar.bmp", "MiraSkyboxCubeMap");
                Console.ReadKey();
                return;
            }

            Engine game = new Game("SandBox");
            game.Run();
        }
    }
}
