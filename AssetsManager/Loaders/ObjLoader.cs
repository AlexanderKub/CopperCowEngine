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
            public int Vi;
            public int Vu;
            public int Vn;
        }

        public class GeometryBuffer
        {

            private readonly List<ObjectData> _objects;
            public List<Vector3> Vertices;
            public List<Vector2> Uvs;
            public List<Vector3> Normals;

            private ObjectData _current;
            private class ObjectData
            {
                public string Name;
                public readonly List<GroupData> Groups;
                public readonly List<FaceIndices> AllFaces;
                public ObjectData()
                {
                    Groups = new List<GroupData>();
                    AllFaces = new List<FaceIndices>();
                }
            }

            private GroupData _currentGroup;

            private class GroupData
            {
                public string Name;
                public string MaterialName;
                public readonly List<FaceIndices> Faces;

                public GroupData()
                {
                    Faces = new List<FaceIndices>();
                }

                public bool IsEmpty => Faces.Count == 0;
            }

            public GeometryBuffer()
            {
                _objects = new List<ObjectData>();
                var d = new ObjectData()
                {
                    Name = "default"
                };
                _objects.Add(d);
                _current = d;

                var g = new GroupData()
                {
                    Name = "default"
                };
                d.Groups.Add(g);
                _currentGroup = g;

                Vertices = new List<Vector3>();
                Uvs = new List<Vector2>();
                Normals = new List<Vector3>();
            }

            public void PushObject(string name)
            {
                if (IsEmpty) _objects.Remove(_current);

                var n = new ObjectData
                {
                    Name = name
                };
                _objects.Add(n);

                var g = new GroupData()
                {
                    Name = "default"
                };
                n.Groups.Add(g);

                _currentGroup = g;
                _current = n;
            }

            public void PushGroup(string name)
            {
                if (_currentGroup.IsEmpty) _current.Groups.Remove(_currentGroup);
                var g = new GroupData
                {
                    Name = name
                };
                _current.Groups.Add(g);
                _currentGroup = g;
            }

            public void PushMaterialName(string name)
            {
                if (!_currentGroup.IsEmpty)
                {
                    PushGroup(name);
                }
                if (_currentGroup.Name == "default")
                {
                    _currentGroup.Name = name;
                }
                _currentGroup.MaterialName = name;
            }

            public void PushVertex(Vector3 v)
            {
                Vertices.Add(v);
            }

            public void PushUv(Vector2 v)
            {
                v.Y = 1 - v.Y;
                Uvs.Add(v);
            }

            public void PushNormal(Vector3 v)
            {
                Normals.Add(v);
            }

            public void PushFace(FaceIndices f)
            {
                _currentGroup.Faces.Add(f);
                _current.AllFaces.Add(f);
            }

            public int NumObjects => _objects.Count;
            public bool IsEmpty => Vertices.Count == 0;
            public bool HasUVs => Uvs.Count > 0;
            public bool HasNormals => Normals.Count > 0;

            public ModelGeometry GetGeometry()
            {
                var tmpVertices = new List<Vector3>();
                var verticesDictionary = new Dictionary<string, int>();
                var tmpUVs = new List<Vector2>();
                var tmpNormals = new List<Vector3>();
                var tmpTriangles = new List<int>();

                var od = _objects[0];

                var k = 0;
                foreach (var fi in od.AllFaces)
                {
                    var newKey = fi.Vi + "|" + fi.Vn + "|" + fi.Vu;
                    if (!verticesDictionary.TryGetValue(newKey, out var vNum))
                    {
                        verticesDictionary.Add(newKey, k);
                        vNum = k;
                        tmpVertices.Add(Vertices[fi.Vi]);
                        if (HasNormals)
                        {
                            tmpNormals.Add(Normals[fi.Vn]);
                        }
                        if (HasUVs)
                        {
                            tmpUVs.Add(Uvs[fi.Vu]);
                        }
                        k++;
                    }
                    tmpTriangles.Add(vNum);
                }

                var model = new ModelGeometry(
                    tmpVertices.ToArray(),
                    null,
                    HasUVs ? tmpUVs.ToArray() : null,
                    tmpTriangles.ToArray(),
                    null,
                    HasNormals ? tmpNormals.ToArray() : null
                );

                tmpVertices.Clear();
                verticesDictionary.Clear();
                tmpUVs.Clear();
                tmpNormals.Clear();
                tmpTriangles.Clear();

                return model;
            }
        }

        public static Dictionary<string, ModelGeometry> CachedModels = new Dictionary<string, ModelGeometry>();

        public static ModelGeometry Load(string objPath)
        {
            if (CachedModels.ContainsKey(objPath))
            {
                return CachedModels[objPath];
            }

            var geometryBuffer = new GeometryBuffer();
            using (var sr = new StreamReader(objPath))
            {
                string l;
                while ((l = sr.ReadLine()) != null)
                {
                    ProcessObjLine(geometryBuffer, l);
                }
            }
            var modelGeometry = geometryBuffer.GetGeometry();
            CachedModels.Add(objPath, modelGeometry);
            //Engine.Log("ModelGeometry.Count = " + MG.Count.ToString());

            return modelGeometry;
        }

        private const string O = "o";
        private const string G = "g";
        private const string V = "v";
        private const string VT = "vt";
        private const string VN = "vn";
        private const string F = "f";
        private const string MTL = "mtllib";
        private const string UML = "usemtl";

        private static readonly int[] RightQuadsWay = {
            1, 2, 3,
            1, 3, 4,
        };

        private static readonly int[] RightTrianglesWay = {
            1, 2, 3,
        };

        private static void ProcessObjLine(GeometryBuffer geometryBuffer, string line)
        {
            var l = line;
            var indexOfSharp = l.IndexOf("#", StringComparison.Ordinal);
            if (indexOfSharp != -1)
            {
                l = l.Substring(0, indexOfSharp);
            }

            l = l.Trim().Replace("  ", " ");
            var p = l.Split(" ".ToCharArray());

            switch (p[0])
            {
                case O:
                    geometryBuffer.PushObject(p[1].Trim());
                    break;
                case G:
                    geometryBuffer.PushGroup(p[1].Trim());
                    break;
                case V:
                    geometryBuffer.PushVertex(new Vector3(Cf(p[1]), Cf(p[2]), Cf(p[3])));
                    break;
                case VT:
                    geometryBuffer.PushUv(new Vector2(Cf(p[1]), Cf(p[2])));
                    break;
                case VN:
                    geometryBuffer.PushNormal(new Vector3(Cf(p[1]), Cf(p[2]), Cf(p[3])));
                    break;
                case F:
                    string[] c;
                    FaceIndices fi;
                    if (p.Length - 1 == 3)
                    {
                        //For Triangles
                        for (var j = 0; j < 3; j++)
                        {
                            c = p[RightTrianglesWay[j]].Trim().Split("/".ToCharArray());
                            fi = new FaceIndices
                            {
                                Vi = Ci(c[0]) - 1
                            };
                            if (c.Length > 1 && c[1] != string.Empty)
                            {
                                fi.Vu = Ci(c[1]) - 1;
                            }
                            if (c.Length > 2 && c[2] != string.Empty)
                            {
                                fi.Vn = Ci(c[2]) - 1;
                            }
                            geometryBuffer.PushFace(fi);
                        }
                    }
                    else
                    {
                        //For Quads
                        for (var j = 0; j < 6; j++)
                        {
                            c = p[RightQuadsWay[j]].Trim().Split("/".ToCharArray());
                            fi = new FaceIndices
                            {
                                Vi = Ci(c[0]) - 1
                            };
                            if (c.Length > 1 && c[1] != string.Empty)
                            {
                                fi.Vu = Ci(c[1]) - 1;
                            }
                            if (c.Length > 2 && c[2] != string.Empty)
                            {
                                fi.Vn = Ci(c[2]) - 1;
                            }
                            geometryBuffer.PushFace(fi);
                        }
                    }
                    break;
            }
        }

        private static readonly System.Globalization.CultureInfo CultInfo = new System.Globalization.CultureInfo("en-US");

        private static float Cf(string v)
        {
            return Convert.ToSingle(v.Trim(), CultInfo);
        }

        private static int Ci(string v)
        {
            return string.IsNullOrEmpty(v) ? 1 : Convert.ToInt32(v.Trim(), CultInfo);
        }
    }
}
