using System;
using System.Collections.Generic;
using SharpDX;

namespace EngineCore
{
    public class BoundingScene
    {
        public List<CBoundingSphere> BS_List;
        private List<string> AlreadyChecked;

        public BoundingScene() {
            BS_List = new List<CBoundingSphere>();
            AlreadyChecked = new List<string>();
        }

        public void PhysicUpdate() {
            AlreadyChecked.Clear();
            for (int i = 0; i < BS_List.Count; i++) {
                for (int j = 0; j < BS_List.Count; j++) {
                    if (i == j || AlreadyChecked.Contains(i + "+" + j) || AlreadyChecked.Contains(j + "+" + i) || j >= BS_List.Count) {
                        continue;
                    }
                    if (i >= BS_List.Count) {
                        return;
                    }

                    if (BS_List[i].HasContact(BS_List[j])) {
                        AlreadyChecked.Add(i + "+" + j);
                        BS_List[i].OnContactEvent(BS_List[j]);
                        BS_List[j].OnContactEvent(BS_List[i]);
                    }
                }
            }
        }
    }
}
