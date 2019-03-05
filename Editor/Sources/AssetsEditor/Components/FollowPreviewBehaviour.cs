using EngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.AssetsEditor.Components
{
    class FollowPreviewBehaviour: Component
    {
        private Transform Target;

        public FollowPreviewBehaviour(Transform target) {
            Target = target;
        }

        public override void Update() {
            gameObject.transform.LocalRotation = Target.LocalRotation;
            gameObject.transform.LocalScale = Target.LocalScale;
        }
    }
}
