using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Components
{
    public class Light : IEntityComponent
    {
        public enum LightType
        {
            Directional,
            Spot,
            Point,
            Capsule,
        }
        public LightType Type;
        public Vector3 Color = Vector3.One;
        public float Intensity = 1;
        public float Radius;
        public bool IsCastShadows;
        public Matrix ViewProjection;
    }
}
