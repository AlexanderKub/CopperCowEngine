using AssetsManager.Loaders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    public class Primitives
    {
        public static Vector4 White = new Vector4(0.92f, 0.92f, 0.92f, 1.0f);
        public static Vector4 Red = new Vector4(1f, 0f, 0f, 1f);
        public static Vector4 Blue = new Vector4(0f, 0f, 1f, 1f);
        public static Vector4 Yellow = new Vector4(1f, 1f, 0f, 1f);
        public static Vector4 Green = new Vector4(0f, 1f, 0f, 1f);

        private static ModelGeometry m_PlaneWithUV;
        public static ModelGeometry PlaneWithUV {
            get {
                if(m_PlaneWithUV == null) {
                    Vector3[] Verts = new Vector3[] {
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, 0.5f, -0.5f),
                        new Vector3(-0.5f, 0.5f, -0.5f),
                    };

                    Vector2[] UVs = new Vector2[] {
                        new Vector2(1f, 1f),
                        new Vector2(0f, 1f),
                        new Vector2(0f, 0f),
                        new Vector2(1f, 0f),
                    };

                    int[] Indexes = new int[] {
                        0, 1, 3,
                        1, 2, 3
                    };

                    Vector3[] Normals = new Vector3[] {
                        new Vector3(0, 1f, 0),
                        new Vector3(0, 1f, 0),
                        new Vector3(0, 1f, 0),
                        new Vector3(0, 1f, 0),
                    };

                    m_PlaneWithUV = new ModelGeometry(
                        Verts,
                        null,
                        UVs,
                        Indexes,
                        null,
                        Normals
                    );
                }
                return m_PlaneWithUV;
            }
        }

        public static ModelGeometry Cube() {
            return Cube(White);
        }

        public static ModelGeometry Cube(Vector4 Color) {
            int n = 34;
            int m = 24;
            int[] indxs = new int[n];
            Vector4[] colors = new Vector4[m];
            Vector3[] normals = new Vector3[m];
            Vector2[] uvs = new Vector2[m];
            for (int i = 0; i < n; i++) {
                indxs[i] = i;
                if(i >= m) {
                    continue;
                } 
                colors[i] = Color;
                if(i < 4) {
                    normals[i] = new Vector3(0, 0, -1f);
                } else if(i< 8) {
                    normals[i] = new Vector3(0, 0, 1f);
                } else if (i < 12) {
                    normals[i] = new Vector3(0, -1f, 0);
                } else if (i < 16) {
                    normals[i] = new Vector3(0, 1f, 0);
                } else if (i < 20) {
                    normals[i] = new Vector3(-1f, 0, 0);
                } else if (i < 24) {
                    normals[i] = new Vector3(1f, 0, 0);
                }
                if (i % 4 == 0) {
                    uvs[i] = new Vector2(0, 0);
                    uvs[i + 1] = new Vector2(1, 0);
                    uvs[i + 3] = new Vector2(0, 1);
                    uvs[i + 2] = new Vector2(1, 1);
                }
            }
            return new ModelGeometry(
                new Vector3[] {
                    //Back
                    new Vector3(-0.5f, -0.5f, -0.5f), //0
                    new Vector3(0.5f, -0.5f, -0.5f), //1
                    new Vector3(0.5f, 0.5f, -0.5f), //2
                    new Vector3(-0.5f, 0.5f, -0.5f), //3

                    //Front
                    new Vector3(-0.5f, -0.5f, 0.5f), //4
                    new Vector3(0.5f, -0.5f, 0.5f),  //5
                    new Vector3(0.5f, 0.5f, 0.5f),   //6
                    new Vector3(-0.5f, 0.5f, 0.5f),  //7
                     
                    //Down
                    new Vector3(-0.5f, -0.5f, -0.5f), //8
                    new Vector3(-0.5f, -0.5f, 0.5f), //9
                    new Vector3(0.5f, -0.5f, 0.5f), //10
                    new Vector3(0.5f, -0.5f, -0.5f), //11

                    //Up
                    new Vector3(-0.5f, 0.5f, 0.5f), //12
                    new Vector3(-0.5f, 0.5f, -0.5f), //13
                    new Vector3(0.5f, 0.5f, -0.5f), //14
                    new Vector3(0.5f, 0.5f, 0.5f), //15
                    
                    //Left
                    new Vector3(-0.5f, 0.5f, 0.5f), //16
                    new Vector3(-0.5f, -0.5f, 0.5f), //17
                    new Vector3(-0.5f, -0.5f, -0.5f), //18
                    new Vector3(-0.5f, 0.5f, -0.5f), //19

                    //Right
                    new Vector3(0.5f, -0.5f, -0.5f), //20
                    new Vector3(0.5f, -0.5f, 0.5f), //21
                    new Vector3(0.5f, 0.5f, 0.5f), //22
                    new Vector3(0.5f, 0.5f, -0.5f), //23
                },
                colors,
                uvs,
                new int[] {
                    0, 3, 1,
                    1, 3, 2,

                    5, 7, 4,
                    6, 7, 5,

                    8, 11, 9,
                    11, 10, 9,

                    12, 15, 13,
                    14, 13, 15,

                    16, 19, 17,
                    17, 19, 18,

                    20, 23, 21,
                    21, 23, 22,
                },
                new int[] {
                    0, 4, 1, 2, 3, 4,
                    1, 6, 2, 6, 3, 0,
                    
                    5, 1, 4, 3, 7, 6,
                    6, 1, 5, 4, 7, 2,
                
                    8, 3, 9, 5, 11, 3,
                    11, 0, 9, 6, 10, 6,

                    12, 0, 13, 0, 15, 6,
                    15, 5, 14, 5, 12, 3,

                    19, 6, 16, 6, 17, 0,
                    17, 1, 18, 1, 19, 7,

                    20, 0, 21, 6, 23, 0,
                    23, 1, 21, 7, 22, 7,
                },
                normals
            );
        }

        private static bool m_NeedUpdateCeil;
        private static int m_CeilSizeX = 10;
        public static int CeilSizeX
        {
            get {
                return m_CeilSizeX;
            }
            set {
                if(m_CeilSizeX != value) {
                    m_CeilSizeX = value;
                    m_NeedUpdateCeil = true;
                }
            }
        }

        private static int m_CeilSizeY = 10;
        public static int CeilSizeY
        {
            get {
                return m_CeilSizeY;
            }
            set {
                if (m_CeilSizeY != value) {
                    m_CeilSizeY = value;
                    m_NeedUpdateCeil = true;
                }
            }
        }

        private static ModelGeometry m_Ceil;
        public static ModelGeometry Ceil
        {
            get {
                if (m_Ceil == null || m_NeedUpdateCeil) {
                    Vector4 white = new Vector4(0.9f, 0.9f, 0.9f, 1f);
                    
                    int s = m_CeilSizeX;
                    int l = m_CeilSizeY;
                    int n = (s + l + 2) * 2;

                    Vector3[] Vertecies = new Vector3[n];
                    Vector4[] Colors = new Vector4[n];
                    int[] Indcs = new int[n];

                    float lineLength = 0.5f;
                    
                    int j = 0;
                    float x, y;
                    for (int i = 0; i < l + 1; i++) {
                        x = lineLength * (s / 2);
                        if (s % 2 != 0) {
                            x += 0.25f;
                        }
                        y = lineLength * (i - l / 2);
                        if(l % 2 != 0) {
                            y -= 0.25f;
                        }

                        Vertecies[j] = new Vector3(0f + x, 0f, 0f + y);
                        Vertecies[j + 1] = new Vector3(0f - x, 0f, 0f + y);

                        Colors[j] = Colors[j + 1] = white;

                        Indcs[j] = j;
                        Indcs[j + 1] = j + 1;

                        j += 2;
                    }

                    for (int i = 0; i < s + 1; i++) {
                        x = lineLength * (i - s / 2);
                        if (s % 2 != 0) {
                            x -= 0.25f;
                        }
                        y = lineLength * (l / 2);
                        if (l % 2 != 0) {
                            y += 0.25f;
                        }

                        Vertecies[j] = new Vector3(0f + x, 0f, 0f + y);
                        Vertecies[j + 1] = new Vector3(0f + x, 0f, 0f - y);

                        Colors[j] = Colors[j + 1] = white;

                        Indcs[j] = j;
                        Indcs[j + 1] = j + 1;

                        j += 2;
                    }

                    m_Ceil = new ModelGeometry(Vertecies, Colors, null, Indcs, null, null);
                }
                m_NeedUpdateCeil = false;
                return m_Ceil;
            }
        }

        private static Dictionary<int, ModelGeometry> SphereCache;
        public static ModelGeometry Sphere(int resolution)
        {
            if (SphereCache == null) {
                SphereCache = new Dictionary<int, ModelGeometry>();
            }

            if (SphereCache.ContainsKey(resolution)) {
                return SphereCache[resolution];
            }

            Vector3[] verteces = new Vector3[resolution * resolution + 1];
            Vector3[] normals = new Vector3[resolution * resolution + 1];
            Vector2[] uvs = new Vector2[resolution * resolution + 1];

            Vector3[] arr_verteces = new Vector3[resolution * resolution * 6];
            Vector2[] arr_uvs = new Vector2[resolution * resolution * 6];
            Vector3[] arr_normals = new Vector3[resolution * resolution * 6];
            int[] indeces = new int[resolution * resolution * 6];

            float PI = (float)Math.PI;
            int i, j, iPos;
            float phi, x, y, z;
            for (i = 0; i < resolution; i++) {
                float theta = (PI * i) / (resolution);
                for (j = 0; j < resolution; j++) {
                    phi = (2 * PI * j) / (resolution);
                    x = (float)(Math.Sin(theta) * Math.Cos(phi)) * 0.5f;
                    y = (float)(Math.Sin(theta) * Math.Sin(phi)) * 0.5f;
                    z = (float)(Math.Cos(theta)) * 0.5f;
                    verteces[i * resolution + j] = new Vector3(x, y, z);
                    normals[i * resolution + j] = verteces[i * resolution + j];
                    uvs[i * resolution + j] = new Vector2(theta, phi);
                }
            }

            verteces[resolution * resolution] = new Vector3(0, 0, -0.5f);
            normals[resolution * resolution] = new Vector3(0, 0, -0.5f);
            uvs[resolution * resolution] = new Vector2(0, 0);

            int iNext = 0;

            for (j = 0; j < resolution; j++) {
                for (i = 0; i < resolution; i++) {
                    if (i == resolution - 1) {
                        iNext = 0;
                    } else {
                        iNext = i + 1;
                    }

                    iPos = (j * resolution * 6) + (i * 6);
                    indeces[iPos] = j * resolution + i;
                    indeces[iPos + 2] = j * resolution + iNext;

                    if (j != resolution - 1) {
                        indeces[iPos + 1] = ((j + 1) * resolution) + i;
                    } else {
                        indeces[iPos + 1] = resolution * resolution;
                    }

                    indeces[iPos + 3] = indeces[iPos + 2];
                    indeces[iPos + 4] = indeces[iPos + 1];

                    if (j != resolution - 1) {
                        indeces[iPos + 5] = ((j + 1) * resolution) + iNext;
                    } else {
                        indeces[iPos + 5] = resolution * resolution;
                    }
                }
            }

            SphereCache[resolution] = new ModelGeometry(
                verteces,
                null,
                uvs,
                indeces,
                null,
                normals
            );
            
            return SphereCache[resolution];
        }
    }
}
