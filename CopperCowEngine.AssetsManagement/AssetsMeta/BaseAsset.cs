using System.IO;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public abstract class BaseAsset
    {
        public string Name;
        public AssetTypes Type;
        public bool IsInvalid => this.Type == AssetTypes.Invalid;

        protected BaseAsset() { }

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

        public override string ToString()
        {
            return Name;
        }
    }
}