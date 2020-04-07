using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.Loaders;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        internal struct CachedMesh
        {
            public ModelGeometry Geometry;
            public Buffer VertexBuffer;
            public Buffer IndexBuffer;
            public int IndexCount => Geometry.Indexes.Length;
        }

        private Dictionary<string, CachedMesh> _meshesCache;

        private BufferDescription _vertexBufferDescription;

        private BufferDescription _indexBufferDescription;

        private void InitMeshesCache()
        {
            _meshesCache = new Dictionary<string, CachedMesh>();
            // Create buffers descriptions.
            _vertexBufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
            };
            _indexBufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
            };
        }

        private void DisposeMeshesCache()
        {
            foreach (var mesh in _meshesCache)
            {
                mesh.Value.VertexBuffer.Dispose();
                mesh.Value.IndexBuffer.Dispose();
            }
            _meshesCache.Clear();
            _meshesCache = null;
        }

        public CachedMesh GetMesh(string name)
        {
            if (_meshesCache.ContainsKey(name))
            {
                return _meshesCache[name];
            }

            ModelGeometry meshInstance;
            if (name.StartsWith("Primitives."))
            {
                switch (name)
                {
                    case "Primitives.Sphere":
                        meshInstance = Primitives.Sphere(16);
                        break;
                    case "Primitives.LVSphere":
                        meshInstance = Primitives.Sphere(6);
                        break;
                    case "Primitives.Cube":
                        meshInstance = Primitives.Cube;
                        break;
                    default:
                        meshInstance = Primitives.Cube;
                        break;
                }
            }
            else
            {
                meshInstance = MeshAssetsLoader.LoadMesh(name);
            }

            if (meshInstance.Points == null)
            {
                meshInstance = Primitives.Cube;
            }

            var tmp = new CachedMesh()
            {
                Geometry = meshInstance,
                VertexBuffer = Buffer.Create(_renderBackend.Device, meshInstance.Points, _vertexBufferDescription),
                IndexBuffer = Buffer.Create(_renderBackend.Device, meshInstance.Indexes, _indexBufferDescription),
            };

            tmp.VertexBuffer.DebugName = name + "VertexBuffer";
            tmp.IndexBuffer.DebugName = name + "IndexBuffer";
            _meshesCache.Add(name, tmp);
            MeshAssetsLoader.DropCachedMesh(name);

            return _meshesCache[name];
        }
    }
}
