using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsManager.Loaders
{
    internal class ObjLoader
    {
        public class FaceIndices
        {
            public int vi;
            public int vu;
            public int vn;
        }

        public class GeometryBuffer
        {
            
            private List<ObjectData> objects;
            public List<Vector3> vertices;
            public List<Vector2> uvs;
            public List<Vector3> normals;

            private ObjectData current;
            private class ObjectData
            {
                public string name;
                public List<GroupData> groups;
                public List<FaceIndices> allFaces;
                public ObjectData() {
                    groups = new List<GroupData>();
                    allFaces = new List<FaceIndices>();
                }
            }

            private GroupData curgr;
            private class GroupData
            {
                public string name;
                public string materialName;
                public List<FaceIndices> faces;
                public GroupData() {
                    faces = new List<FaceIndices>();
                }
                public bool IsEmpty { get { return faces.Count == 0; } }
            }

            public GeometryBuffer() {
                objects = new List<ObjectData>();
                ObjectData d = new ObjectData() {
                    name = "default"
                };
                objects.Add(d);
                current = d;

                GroupData g = new GroupData() {
                    name = "default"
                };
                d.groups.Add(g);
                curgr = g;

                vertices = new List<Vector3>();
                uvs = new List<Vector2>();
                normals = new List<Vector3>();
            }

            public void PushObject(string name) {
                if (IsEmpty) objects.Remove(current);

                ObjectData n = new ObjectData();
                n.name = name;
                objects.Add(n);

                GroupData g = new GroupData() {
                    name = "default"
                };
                n.groups.Add(g);

                curgr = g;
                current = n;
            }

            public void PushGroup(string name) {
                if (curgr.IsEmpty) current.groups.Remove(curgr);
                GroupData g = new GroupData {
                    name = name
                };
                current.groups.Add(g);
                curgr = g;
            }

            public void PushMaterialName(string name) {
                if (!curgr.IsEmpty) PushGroup(name);
                if (curgr.name == "default") curgr.name = name;
                curgr.materialName = name;
            }

            public void PushVertex(Vector3 v) {
                vertices.Add(v);
            }

            public void PushUV(Vector2 v) {
                v.Y = 1 - v.Y;
                uvs.Add(v);
            }

            public void PushNormal(Vector3 v) {
                normals.Add(v);
            }

            public void PushFace(FaceIndices f) {
                curgr.faces.Add(f);
                current.allFaces.Add(f);
            }

            public int NumObjects { get { return objects.Count; } }
            public bool IsEmpty { get { return vertices.Count == 0; } }
            public bool HasUVs { get { return uvs.Count > 0; } }
            public bool HasNormals { get { return normals.Count > 0; } }

            public ModelGeometry GetGeometry() {
                List<Vector3> tvertices = new List<Vector3>();
                Dictionary<string, int> vertsDictionary = new Dictionary<string, int>();
                List<Vector2> tuvs = new List<Vector2>();
                List<Vector3> tnormals = new List<Vector3>();
                List<int> ttris = new List<int>();
                
                ObjectData od = objects[0];

                string newKey;
                int k = 0;
                int vNum;
                foreach (FaceIndices fi in od.allFaces) {
                    newKey = fi.vi + "|" + fi.vn + "|" + fi.vu;
                    if (!vertsDictionary.TryGetValue(newKey, out vNum)) {
                        vertsDictionary.Add(newKey, k);
                        vNum = k;
                        tvertices.Add(vertices[fi.vi]);
                        if (HasNormals) {
                            tnormals.Add(normals[fi.vn]);
                        }
                        if (HasUVs) {
                            tuvs.Add(uvs[fi.vu]);
                        }
                        k++;
                    }
                    ttris.Add(vNum);
                }
                
                ModelGeometry Model = new ModelGeometry(
                    tvertices.ToArray(),
                    null,
                    HasUVs ? tuvs.ToArray() : null, 
                    ttris.ToArray(),
                    null,
                    HasNormals? tnormals.ToArray() : null
                );

                tvertices.Clear();
                vertsDictionary.Clear();
                tuvs.Clear();
                tnormals.Clear();
                ttris.Clear();

                return Model;
            }
        }
        
        static public Dictionary<string, AssetsManager.Loaders.ModelGeometry> CachedModels = new Dictionary<string, ModelGeometry>();
        static public ModelGeometry Load(string objPath) {
            if(CachedModels.ContainsKey(objPath)) {
                return CachedModels[objPath];
            }

            string l;
            GeometryBuffer gBuffer = new GeometryBuffer();
            using (StreamReader sr = new StreamReader(objPath)) {
                while ((l = sr.ReadLine()) != null) {
                    ProcessOBJLine(gBuffer, l);
                }
            }
            ModelGeometry MG = gBuffer.GetGeometry();
            CachedModels.Add(objPath, MG);
            //Engine.Log("ModelGeometry.Count = " + MG.Count.ToString());

            return MG;
        }

        private const string O = "o";
        private const string G = "g";
        private const string V = "v";
        private const string VT = "vt";
        private const string VN = "vn";
        private const string F = "f";
        private const string MTL = "mtllib";
        private const string UML = "usemtl";

        static private int[] rightQuadsWay = new int[6] {
            1, 2, 3,
            1, 3, 4,
        };
        static private int[] rightTrisWay = new int[3] {
            1, 2, 3,
        };

        static private void ProcessOBJLine(GeometryBuffer gBuffer, string line) {
            string l = line;
            int iSchrp = l.IndexOf("#");
            if (iSchrp != -1)
                l = l.Substring(0, iSchrp);
            l = l.Trim().Replace("  ", " ");
            string[] p = l.Split(" ".ToCharArray());

            switch (p[0]) {
                case O:
                    gBuffer.PushObject(p[1].Trim());
                    break;
                case G:
                    gBuffer.PushGroup(p[1].Trim());
                    break;
                case V:
                    gBuffer.PushVertex(new Vector3(Cf(p[1]), Cf(p[2]), Cf(p[3])));
                    break;
                case VT:
                    gBuffer.PushUV(new Vector2(Cf(p[1]), Cf(p[2])));
                    break;
                case VN:
                    gBuffer.PushNormal(new Vector3(Cf(p[1]), Cf(p[2]), Cf(p[3])));
                    break;
                case F:
                    string[] c;
                    FaceIndices fi;
                    if (p.Length - 1 == 3) {
                        //For Triangles
                        for (int j = 0; j < 3; j++) {
                            c = p[rightTrisWay[j]].Trim().Split("/".ToCharArray());
                            fi = new FaceIndices();
                            fi.vi = Ci(c[0]) - 1;
                            if (c.Length > 1 && c[1] != string.Empty)
                                fi.vu = Ci(c[1]) - 1;
                            if (c.Length > 2 && c[2] != string.Empty)
                                fi.vn = Ci(c[2]) - 1;
                            gBuffer.PushFace(fi);
                        }
                    } else {
                        //For Quads
                        for (int j = 0; j < 6; j++) {
                            c = p[rightQuadsWay[j]].Trim().Split("/".ToCharArray());
                            fi = new FaceIndices();
                            fi.vi = Ci(c[0]) - 1;
                            if (c.Length > 1 && c[1] != string.Empty)
                                fi.vu = Ci(c[1]) - 1;
                            if (c.Length > 2 && c[2] != string.Empty)
                                fi.vn = Ci(c[2]) - 1;
                            gBuffer.PushFace(fi);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static System.Globalization.CultureInfo CultInfo = 
            new System.Globalization.CultureInfo("en-US");

        private static float Cf(string v) {
            return Convert.ToSingle(v.Trim(), CultInfo);
        }

        private static int Ci(string v) {
            if (string.IsNullOrEmpty(v))
                return 1;
            return Convert.ToInt32(v.Trim(), CultInfo);
        }
    }
}
