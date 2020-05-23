using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.Loaders;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        internal class CachedMesh
        {
            public ModelGeometry Geometry;
            public Buffer VertexBuffer;
            public Buffer IndexBuffer;
            public int IndexCount => Geometry.Indexes.Length;
        }

        private Dictionary<Guid, CachedMesh> _meshesCache;

        private BufferDescription _vertexBufferDescription;

        private BufferDescription _indexBufferDescription;

        private void InitMeshesCache()
        {
            _meshesCache = new Dictionary<Guid, CachedMesh>();
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
            foreach (var (_, value) in _meshesCache)
            {
                value.VertexBuffer.Dispose();
                value.IndexBuffer.Dispose();
            }
            _meshesCache.Clear();
            _meshesCache = null;
        }

        public CachedMesh GetMesh(Guid meshGuid)
        {
            if (_meshesCache.ContainsKey(meshGuid))
            {
                return _meshesCache[meshGuid];
            }

            var meshInstance = MeshAssetsLoader.GetMeshGeometry(meshGuid);

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

            tmp.VertexBuffer.DebugName = meshGuid + "VertexBuffer";
            tmp.IndexBuffer.DebugName = meshGuid + "IndexBuffer";
            _meshesCache.Add(meshGuid, tmp);
            MeshAssetsLoader.DropCachedMesh(meshGuid);

            return _meshesCache[meshGuid];
        }
    }
}
