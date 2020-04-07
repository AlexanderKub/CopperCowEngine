using System.IO;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public abstract class BaseAsset
    {
        public string Name;
        public AssetTypes Type;
        public bool IsInvalid => this.Type == AssetTypes.Invalid;

        protected BaseAsset() { }

        protected BaseAsset(BaseAsset source)
        {
            InternalCopyValues(source);
        }

        private void InternalCopyValues(BaseAsset source)
        {
            Name = source.Name;
            Type = source.Type;
            CopyValues(source);
        }

        public abstract void CopyValues(BaseAsset source);

        public virtual bool ImportAsset(string path, string ext) { return true; }

        public virtual void SaveAsset(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write((int)Type);
        }

        public virtual bool LoadAsset(BinaryReader reader)
        {
            Name = reader.ReadString();
            var t = reader.ReadInt32();
            var type = (AssetTypes)t;
            if ((Type == AssetTypes.Meta || this.Type == type) &&
                (Type != AssetTypes.Meta || ((MetaAsset) this).InfoType == type))
            {
                return true;
            }
            Type = AssetTypes.Invalid;
            return false;
        }

        public virtual bool IsSame(BaseAsset other)
        {
            var same = true;
            same &= Name == other.Name;
            same &= Type == other.Type;
            return same;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}