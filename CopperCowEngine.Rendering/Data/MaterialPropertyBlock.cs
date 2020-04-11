using System.Numerics;

namespace CopperCowEngine.Rendering.Data
{
    public struct MaterialPropertyBlock
    {
        public Vector3 AlbedoColor;

        public float AlphaValue;

        public float MetallicValue;

        public float RoughnessValue;

        public Vector2 Shift;

        public Vector2 Tile;

        public MaterialPropertyBlock(MaterialPropertyBlock block)
        {
            AlbedoColor = block.AlbedoColor;

            AlphaValue = block.AlphaValue;

            MetallicValue = block.MetallicValue;

            RoughnessValue = block.RoughnessValue;

            Shift = block.Shift;

            Tile = block.Tile;
        }

        public static MaterialPropertyBlock Default = new MaterialPropertyBlock()
        {
            AlbedoColor = Vector3.One,

            AlphaValue = 1.0f,

            MetallicValue = 0.0f,

            RoughnessValue = 0.5f,

            Shift = Vector2.Zero,

            Tile = Vector2.One
        };
    }
}