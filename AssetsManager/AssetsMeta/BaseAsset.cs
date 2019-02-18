using System.IO;

namespace AssetsManager.AssetsMeta
{
    public abstract class BaseAsset
    {
        public string Name;
        public AssetTypes Type;
        public bool IsInvalid
        {
            get {
                return this.Type == AssetTypes.Invalid;
            }
        }

        public BaseAsset() { }
        public BaseAsset(BaseAsset source) {
            this.Name = source.Name;
            this.Type = source.Type;
        }

        public virtual bool ImportAsset(string path, string ext) { return true; }

        public virtual void SaveAsset(BinaryWriter writer) {
            writer.Write(Name);
            writer.Write((int)Type);
        }

        public virtual bool LoadAsset(BinaryReader reader) {
            this.Name = reader.ReadString();
            int t = reader.ReadInt32();
            AssetTypes type = (AssetTypes)t;
            if ((this.Type != AssetTypes.Meta && this.Type != type) || 
                (this.Type == AssetTypes.Meta && ((MetaAsset)this).InfoType != type)) {
                this.Type = AssetTypes.Invalid;
                return false;
            }
            return true;
        }

        public override string ToString() {
            return this.Name;
        }
    }
}
