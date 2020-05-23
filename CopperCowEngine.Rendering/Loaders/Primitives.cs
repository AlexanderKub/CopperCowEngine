using System;
using System.Collections.Generic;
using System.Numerics;

namespace CopperCowEngine.Rendering.Loaders
{
    public static class Primitives
    {
        public static Vector4 White = new Vector4(0.92f, 0.92f, 0.92f, 1.0f);
        public static Vector4 Red = new Vector4(1f, 0f, 0f, 1f);
        public static Vector4 Blue = new Vector4(0f, 0f, 1f, 1f);
        public static Vector4 Yellow = new Vector4(1f, 1f, 0f, 1f);
        public static Vector4 Green = new Vector4(0f, 1f, 0f, 1f);

        private static ModelGeometry _planeWithUv;
        private static ModelGeometry _cube;

        private static bool _needUpdateCeil;
        private static int _ceilSizeX = 10;
        private static int _ceilSizeY = 10;
        private static ModelGeometry _ceil;

        private static Dictionary<int, ModelGeometry> _sphereCache;

        public static ModelGeometry PlaneWithUv
        {
            get
            {
                if (_planeWithUv != null)
                {
                    return _planeWithUv;
                }

                var vertices = new[] {
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                };

                var uvs = new[] {
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                };

                var indices = new[] {
                    0, 1, 3,
                    1, 2, 3
                };

                var normals = new[] {
                    new Vector3(0, 1f, 0),
                    new Vector3(0, 1f, 0),
                    new Vector3(0, 1f, 0),
                    new Vector3(0, 1f, 0),
                };

                _planeWithUv = new ModelGeometry(
                    vertices,
                    null,
                    uvs,
                    indices,
                    null,
                    normals
                );
                return _planeWithUv;
            }
        }

        public static ModelGeometry Cube => _cube ?? (_cube = GenCube());

        private static ModelGeometry GenCube()
        {
            const int n = 34;
            const int m = 24;
            var indices = new int[n];
            var colors = new Vector4[m];
            var normals = new Vector3[m];
            var uvs = new Vector2[m];

            for (var i = 0; i < n; i++)
            {
                indices[i] = i;
                if (i >= m)
                {
                    continue;
                }
                colors[i] = Vector4.One;
                if (i < 4)
                {
                    normals[i] = new Vector3(0, 0, -1f);
                }
                else if (i < 8)
                {
                    normals[i] = new Vector3(0, 0, 1f);
                }
                else if (i < 12)
                {
                    normals[i] = new Vector3(0, -1f, 0);
                }
                else if (i < 16)
                {
                    normals[i] = new Vector3(0, 1f, 0);
                }
                else if (i < 20)
                {
                    normals[i] = new Vector3(-1f, 0, 0);
                }
                else if (i < 24)
                {
                    normals[i] = new Vector3(1f, 0, 0);
                }

                if (i % 4 != 0)
                {
                    continue;
                }
                uvs[i] = new Vector2(1, 1);
                uvs[i + 1] = new Vector2(0, 1);
                uvs[i + 3] = new Vector2(1, 0);
                uvs[i + 2] = new Vector2(0, 0);
            }

            return new ModelGeometry(
                new[] {
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
                new[] {
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
                new[] {
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

        public static int CeilSizeX
        {
            get => _ceilSizeX;
            set
            {
                _needUpdateCeil = _ceilSizeX != value;
                _ceilSizeX = value;
            }
        }

        public static int CeilSizeY
        {
            get => _ceilSizeY;
            set
            {
                _needUpdateCeil = _ceilSizeY != value;
                _ceilSizeY = value;
            }
        }


        public static ModelGeometry Ceil
        {
            get
            {
                if (_ceil == null || _needUpdateCeil)
                {
                    var white = new Vector4(0.9f, 0.9f, 0.9f, 1f);

                    var s = _ceilSizeX;
                    var l = _ceilSizeY;
                    var n = (s + l + 2) * 2;

                    var vertices = new Vector3[n];
                    var colors = new Vector4[n];
                    var indices = new int[n];

                    const float lineLength = 0.5f;

                    var j = 0;
                    float x, y;
                    for (var i = 0; i < l + 1; i++)
                    {
                        x = lineLength * (s / 2f);
                        if (s % 2 != 0)
                        {
                            x += 0.25f;
                        }

                        y = lineLength * (i - l / 2);
                        if (l % 2 != 0)
                        {
                            y -= 0.25f;
                        }

                        vertices[j] = new Vector3(0f + x, 0f, 0f + y);
                        vertices[j + 1] = new Vector3(0f - x, 0f, 0f + y);

                        colors[j] = colors[j + 1] = white;

                        indices[j] = j;
                        indices[j + 1] = j + 1;

                        j += 2;
                    }

                    for (var i = 0; i < s + 1; i++)
                    {
                        x = lineLength * (i - s / 2);
                        if (s % 2 != 0)
                        {
                            x -= 0.25f;
                        }
                        y = lineLength * (l / 2f);
                        if (l % 2 != 0)
                        {
                            y += 0.25f;
                        }

                        vertices[j] = new Vector3(0f + x, 0f, 0f + y);
                        vertices[j + 1] = new Vector3(0f + x, 0f, 0f - y);

                        colors[j] = colors[j + 1] = white;

                        indices[j] = j;
                        indices[j + 1] = j + 1;

                        j += 2;
                    }

                    _ceil = new ModelGeometry(vertices, colors, null, indices, null, null);
                }
                _needUpdateCeil = false;
                return _ceil;
            }
        }

        public static ModelGeometry Sphere(int tessellation)
        {
            if (_sphereCache == null)
            {
                _sphereCache = new Dictionary<int, ModelGeometry>();
            }

            if (_sphereCache.ContainsKey(tessellation))
            {
                return _sphereCache[tessellation];
            }

            var verticalSegments = tessellation;
            var horizontalSegments = tessellation * 2;

            var vertices = new Vector3[(verticalSegments + 1) * (horizontalSegments + 1)];
            var normals = new Vector3[(verticalSegments + 1) * (horizontalSegments + 1)];
            var uvs = new Vector2[(verticalSegments + 1) * (horizontalSegments + 1)];
            var indices = new int[(verticalSegments) * (horizontalSegments + 1) * 6];

            const float radius = 1.0f * 0.5f;

            var vertexCount = 0;
            // Create rings of vertices at progressively higher latitudes.
            for (var i = 0; i <= verticalSegments; i++)
            {
                var v = 1.0f - (float)i / verticalSegments;

                var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
                var dy = (float)Math.Sin(latitude);
                var dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (var j = 0; j <= horizontalSegments; j++)
                {
                    var u = (float)j / horizontalSegments;

                    var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var dx = (float)Math.Sin(longitude);
                    var dz = (float)Math.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    var normal = new Vector3(dx, dy, dz);
                    var textureCoordinate = new Vector2(u, v);

                    vertices[vertexCount] = normal * radius;
                    normals[vertexCount] = normal;
                    uvs[vertexCount] = textureCoordinate;
                    vertexCount++;
                }
            }

            // Fill the index buffer with triangles joining each pair of latitude rings.
            var stride = horizontalSegments + 1;

            var indexCount = 0;
            for (var i = 0; i < verticalSegments; i++)
            {
                for (var j = 0; j <= horizontalSegments; j++)
                {
                    var nextI = i + 1;
                    var nextJ = (j + 1) % stride;

                    indices[indexCount++] = (i * stride + j);
                    indices[indexCount++] = (i * stride + nextJ);
                    indices[indexCount++] = (nextI * stride + j);

                    indices[indexCount++] = (i * stride + nextJ);
                    indices[indexCount++] = (nextI * stride + nextJ);
                    indices[indexCount++] = (nextI * stride + j);
                }
            }

            _sphereCache[tessellation] = new ModelGeometry(
                vertices,
                null,
                uvs,
                indices,
                null,
                normals
            );

            return _sphereCache[tessellation];
        }
    }
}
