using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    abstract public class Component {
        public GameObject gameObject;
        public virtual void Init() { }
        public virtual void Update() { }
        public virtual void Draw() { }
        public virtual void Destroy() { }
    }
}
