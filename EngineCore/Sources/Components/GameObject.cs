using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    public class GameObject
    {
        public string Name;
        public bool SelfActive = true;
        private bool IsActive {
            get {
                if (transform.Parent) {
                    return transform.Parent.gameObject.IsActive && SelfActive;
                }
                return SelfActive;
            }
        }
        public List<Component> Components;
        public Transform transform;

        internal GameObject(string name) {
            Name = name;
            Components = new List<Component>();
        }

        public Component AddComponent(Component add) {
            bool existFlag = false;
            foreach (var item in Components) {
                if (item.GetType() == add.GetType())
                {
                    existFlag = true;
                    break;
                }
            }
            if (existFlag) {
                Engine.Log("This GameObject already has component: " + add.GetType().ToString());
                return null;
            }
            //TODO: Check if component already exist on gameObject.
            add.gameObject = this;
            add.Init();
            Components.Add(add);

            if(add.GetType() == typeof(Transform)) {
                if (transform) {
                    return transform;
                }
                transform = (Transform)add;
            }

            return add;
        }

        public T GetComponent<T>() {
            Type typeParam = typeof(T);
            T ReturnValue = default(T);

            for (int i = 0; i < Components.Count; i++) {
                if (Components[i].GetType() == typeParam) {
                    ReturnValue = (T)Convert.ChangeType(Components[i], typeParam);
                }
            }

            return ReturnValue;
        }

        public void Update() {
            if (!IsActive) {
                return;
            }
            Components.ForEach((x) => {
                x.Update();
            });
        }
        
        public void Draw() {
            if (!IsActive) {
                return;
            }
            Components.ForEach((x) => {
                if(x is Light) {
                    return;
                }
                x.Draw();
            });
        }

        public void Destroy() {
            Components.ForEach((x) => {
                x.Destroy();
            });
            Components.Clear();
        }

        public static implicit operator bool(GameObject foo)
        {
            return !object.ReferenceEquals(foo, null);
        }
    }
}
