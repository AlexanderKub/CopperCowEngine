using System;
using System.IO;
using System.Numerics;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor.FileSystemWorker;
using CopperCowEngine.AssetsManagement.Editor.Loaders;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.Editor
{
    public static class AssetsExtensions
    {
        public static void SaveAsset(this BaseAsset asset, BinaryWriter writer)
        {
            writer.Write(asset.Name);
            writer.Write((int) asset.Type);
            switch (asset)
            {
                case MaterialAsset thisAsset:
                    MaterialSave(thisAsset, writer);
                    return;
                case MeshAsset thisAsset:
                    MeshSave(thisAsset, writer);
                    return;
                case ShaderAsset thisAsset:
                    ShaderSave(thisAsset, writer);
                    return;
                case Texture2DAsset thisAsset:
                    SaveTexture2D(thisAsset, writer);
                    return;
                case TextureCubeAsset thisAsset:
                    SaveTextureCube(thisAsset, writer);
                    return;
            }
        }

        public static void CopyValues<T>(this BaseAsset asset, T source) where T : BaseAsset
        {
            if (!(asset is MaterialAsset thisAsset))
            {
                return;
            }
            MaterialCopy(thisAsset, source);
            asset.InternalCopy(source);
        }

        public static bool IsSame<T>(this BaseAsset asset, T other) where T : BaseAsset
        {
            if (asset.Name != other.Name || asset.Type != other.Type)
            {
                return false;
            }

            if (asset is MaterialAsset thisAsset)
            {
                return MaterialIsSame(thisAsset, other);
            }

            return false;
        }

        private static void InternalCopy(this BaseAsset asset, BaseAsset source)
        {
            asset.Name = source.Name;
            asset.Type = source.Type;
        }

        public static bool ImportAsset(this MeshAsset asset, string path, string ext, float fileScale = 1.0f)
        {
            asset.FileScale = fileScale;

            var modelGeometries = new ModelGeometry[1];

            switch (ext)
            {
                case "obj":
                    modelGeometries[0] = ObjLoader.Load(path);
                    break;
                case "fbx":
                {
                    modelGeometries = FbxLoaderLoader.Load(path);
                    if (modelGeometries.Length > 1)
                    {
                        asset.SubAssets = new MeshAsset[modelGeometries.Length - 1];
                        for (var i = 1; i < modelGeometries.Length; i++)
                            asset.SubAssets[i - 1] = new MeshAsset
                            {
                                Name = asset.Name + "_" + i,
                                FileScale = asset.FileScale,
                                Pivot = Vector3.Zero,
                                Vertices = modelGeometries[i].Points,
                                Indexes = modelGeometries[i].Indexes,
                                BoundingMinimum = modelGeometries[i].BoundingMinimum,
                                BoundingMaximum = modelGeometries[i].BoundingMaximum
                            };
                    }

                    break;
                }
                default:
                    Console.WriteLine("Unknown mesh extension: {0}", ext);
                    return false;
            }

            asset.Pivot = Vector3.Zero;
            asset.Vertices = modelGeometries[0].Points;
            asset.Indexes = modelGeometries[0].Indexes;
            asset.BoundingMinimum = modelGeometries[0].BoundingMinimum;
            asset.BoundingMaximum = modelGeometries[0].BoundingMaximum;
            return true;
        }

        private static void MaterialCopy(MaterialAsset asset, BaseAsset source)
        {
            if (!(source is MaterialAsset materialAsset))
            {
                return;
            }

            asset.AlbedoColor = materialAsset.AlbedoColor;
            asset.AlphaValue = materialAsset.AlphaValue;
            asset.EmissiveColor = materialAsset.EmissiveColor;
            asset.RoughnessValue = materialAsset.RoughnessValue;
            asset.MetallicValue = materialAsset.MetallicValue;
            asset.Tile = materialAsset.Tile;
            asset.Shift = materialAsset.Shift;
            asset.AlbedoMapAsset = materialAsset.AlbedoMapAsset;
            asset.EmissiveMapAsset = materialAsset.EmissiveMapAsset;
            asset.NormalMapAsset = materialAsset.NormalMapAsset;
            asset.RoughnessMapAsset = materialAsset.RoughnessMapAsset;
            asset.MetallicMapAsset = materialAsset.MetallicMapAsset;
            asset.OcclusionMapAsset = materialAsset.OcclusionMapAsset;
        }

        private static void MaterialSave(MaterialAsset asset, BinaryWriter writer)
        {
            writer.Write(SerializeBlock.GetBytes(asset.AlbedoColor));
            writer.Write(asset.AlphaValue);
            writer.Write(SerializeBlock.GetBytes(asset.EmissiveColor));
            writer.Write(asset.RoughnessValue);
            writer.Write(asset.MetallicValue);
            writer.Write(SerializeBlock.GetBytes(asset.Tile));
            writer.Write(SerializeBlock.GetBytes(asset.Shift));
            writer.Write(asset.AlbedoMapAsset ?? string.Empty);
            writer.Write(asset.EmissiveMapAsset ?? string.Empty);
            writer.Write(asset.NormalMapAsset ?? string.Empty);
            writer.Write(asset.RoughnessMapAsset ?? string.Empty);
            writer.Write(asset.MetallicMapAsset ?? string.Empty);
            writer.Write(asset.OcclusionMapAsset ?? string.Empty);
        }

        private static void MeshSave(MeshAsset asset, BinaryWriter writer)
        {
            int i;
            writer.Write(asset.FileScale);
            writer.Write(SerializeBlock.GetBytes(asset.Pivot));
            writer.Write(asset.Vertices.Length);
            for (i = 0; i < asset.Vertices.Length; i++)
            {
                var bytes = SerializeBlock.GetBytes(asset.Vertices[i]);
                writer.Write(bytes);
            }

            writer.Write(asset.Indexes.Length);
            for (i = 0; i < asset.Indexes.Length; i++) writer.Write(asset.Indexes[i]);
            writer.Write(SerializeBlock.GetBytes(asset.BoundingMinimum));
            writer.Write(SerializeBlock.GetBytes(asset.BoundingMaximum));
            if (asset.SubAssets == null) return;
            foreach (var subAsset in asset.SubAssets) EditorFileSystemWorker.CreateAssetFile(subAsset, true);
        }

        private static void ShaderSave(ShaderAsset asset, BinaryWriter writer)
        {
            writer.Write((int) asset.ShaderType);
            writer.Write(asset.Bytecode?.Length ?? 0);
            if (asset.Bytecode != null) writer.Write(asset.Bytecode);
        }

        private static void SaveTexture2D(Texture2DAsset asset, BinaryWriter writer)
        {
            writer.Write(asset.Data.Width);
            writer.Write(asset.Data.Height);
            writer.Write((int) asset.Data.ChannelsCount);
            writer.Write((int) asset.Data.BytesPerChannel);
            writer.Write((int) asset.Data.ColorSpace);
            writer.Write(asset.Data.Buffer);
        }

        private static void SaveTextureCube(TextureCubeAsset asset, BinaryWriter writer)
        {
            writer.Write(asset.Data.Width);
            writer.Write(asset.Data.Height);
            writer.Write(asset.Data.ChannelsCount);
            writer.Write(asset.Data.BytesPerChannel);
            writer.Write((int) asset.Data.ColorSpace);
            writer.Write(asset.Data.MipLevels);
            for (var mip = 0; mip < asset.Data.MipLevels; mip++)
            {
                for (var i = 0; i < 6; i++)
                {
                    writer.Write(asset.Data.Buffer[i][mip]);
                }
            }
        }

        private static bool MaterialIsSame(MaterialAsset asset, BaseAsset other)
        {
            if (!(other is MaterialAsset otherAsset))
            {
                return false;
            }

            var same = asset.AlbedoColor == otherAsset.AlbedoColor;
            same &= Math.Abs(asset.AlphaValue - otherAsset.AlphaValue) < float.Epsilon;
            same &= asset.EmissiveColor == otherAsset.EmissiveColor;
            same &= Math.Abs(asset.RoughnessValue - otherAsset.RoughnessValue) < float.Epsilon;
            same &= Math.Abs(asset.MetallicValue - otherAsset.MetallicValue) < float.Epsilon;
            same &= asset.Tile == otherAsset.Tile;
            same &= asset.Shift == otherAsset.Shift;
            same &= asset.AlbedoMapAsset == otherAsset.AlbedoMapAsset;
            same &= asset.EmissiveMapAsset == otherAsset.EmissiveMapAsset;
            same &= asset.NormalMapAsset == otherAsset.NormalMapAsset;
            same &= asset.RoughnessMapAsset == otherAsset.RoughnessMapAsset;
            same &= asset.MetallicMapAsset == otherAsset.MetallicMapAsset;
            same &= asset.OcclusionMapAsset == otherAsset.OcclusionMapAsset;

            return same;
        }
    }
}