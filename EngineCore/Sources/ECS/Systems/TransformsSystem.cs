using EngineCore.ECS.Components;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Systems
{
    public class TransformsSystem : BasicSystem<Requires<Components.Transform>>
    {
        public override void Update(Timer timer)
        {
            Entity[] entities = GetEntities().OrderByDescending((e) => {
                return e.GetComponent<Components.Transform>().IsNeedUpdate;
            }).ToArray();
            
            foreach (var entity in entities)
            {
                if (!UpdateMatrix(entity.GetComponent<Components.Transform>()))
                {
                    break;
                }
            }
        }

        private bool UpdateMatrix(Components.Transform transform)
        {
            if (!transform.IsNeedUpdate)
            {
                return false;
            }

            transform.PreviousTransformMatrix = transform.TransformMatrix;
            transform.TransformMatrix = Matrix.Identity;
            transform.TransformMatrix *= Matrix.Scaling(transform.Scale);
            transform.TransformMatrix *= Matrix.RotationQuaternion(transform.Rotation);
            transform.TransformMatrix *= Matrix.Translation(transform.Position);

            transform.IsNeedUpdate = false;
            return true;
        }
    }
}
