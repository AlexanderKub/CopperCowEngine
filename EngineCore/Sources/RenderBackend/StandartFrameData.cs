using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightType = EngineCore.ECS.Components.Light.LightType;

namespace EngineCore
{
    public class StandardFrameData : IFrameData
    {
        public struct CameraData
        {
            public Matrix View;
            public Matrix Projection;
            public Matrix ViewProjection;
            public Matrix PreviousView;
            public Matrix PreviousViewProjection;
            public Vector3 Position;
            public Vector3 Forward;
            public BoundingFrustum Frustrum;
            public int index;
        }

        public struct RendererData
        {
            public int EntityId;
            public Matrix PreviousTransformMatrix;
            public Matrix TransformMatrix;
            public string MeshName;
            public string MaterialName;
            public int MaterialQueue;
            public bool IsDynamic;
        }

        public struct LightData
        {
            public LightType Type;

            public Vector3 Position;
            public Vector3 Direction;
            public float Radius;

            public bool IsCastShadows;
            public Matrix ViewProjection;
            public BoundingFrustum Frustrum;

            public Vector3 Color;
            public float Intensity;

            public int index;
        }

        public List<CameraData> CamerasList;
        public Dictionary<int, List<int>> PerCameraRenderersList;
        public Dictionary<int, List<int>> PerCameraLightsList;
        public Dictionary<int, List<int>> PerLightRenderersList;
        public List<RendererData> RenderersList;
        public List<LightData> LightsList;

        // TODO: change struct.
        /*****************************************
         * Cameras -> Scene per camera (shader and mesh sorted).
         * Lights -> Scene per light (only opaque).
         *****************************************/
        public StandardFrameData()
        {
            CamerasList = new List<CameraData>();
            LightsList = new List<LightData>();
            RenderersList = new List<RendererData>();
            PerCameraRenderersList = new Dictionary<int, List<int>>();
            PerCameraLightsList = new Dictionary<int, List<int>>();
            PerLightRenderersList = new Dictionary<int, List<int>>();
        }

        // Sky sphere hack TODO: Remove in future.
        private static Matrix SkySphereMatrix = Matrix.Scaling(Vector3.One * 5000f) * 
            Matrix.RotationQuaternion(Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0));

        public void AddCameraData(CameraData data)
        {
            data.index = CamerasList.Count;
            PerCameraRenderersList.Add(data.index, new List<int>());
            PerCameraLightsList.Add(data.index, new List<int>());
            CamerasList.Add(data);
            /*if (data.index > 0) {
                return;
            }
            RenderersList.Add(new RendererData()
            {
                MaterialName = "SkySphereMaterial",
                MeshName = "SkyDomeMesh",//SkyDomeMesh SkySphereMesh
                MaterialQueue = -1000,
                TransformMatrix = SkySphereMatrix * Matrix.Translation(data.Position),
            });*/
        }

        public void AddLightData(LightData data)
        {
            data.index = LightsList.Count;
            LightsList.Add(data);
            PerLightRenderersList.Add(data.index, new List<int>());
        }

        public void AddRendererData(RendererData data)
        {
            RenderersList.Add(data);
        }

        public void AddLightDataToCamera(int cameraIndex, int lightIndex)
        {
            PerCameraLightsList[cameraIndex].Add(lightIndex);
        }

        public void AddRendererDataToCamera(int cameraIndex, int rendererIndex)
        {
            PerCameraRenderersList[cameraIndex].Add(rendererIndex);
        }

        public void AddRendererDataToLight(int lightIndex, int rendererIndex)
        {
            PerLightRenderersList[lightIndex].Add(rendererIndex);
        }

        public override void Reset()
        {
            PerCameraRenderersList.Clear();
            PerCameraLightsList.Clear();
            PerLightRenderersList.Clear();
            CamerasList.Clear();
            LightsList.Clear();
            RenderersList.Clear();
        }


        public class ByRendererProps : IComparer<RendererData>
        {
            private int a, b, c;
            public int Compare(RendererData x, RendererData y)
            {
                a = x.MaterialQueue.CompareTo(y.MaterialQueue);
                if (a != 0) {
                    return a;
                }
                b = x.MaterialName.CompareTo(y.MaterialName);
                if (b != 0) {
                    return b;
                }
                c = x.MeshName.CompareTo(y.MeshName);
                if (c != 0) {
                    return c;
                }
                return x.EntityId.CompareTo(y.EntityId);
            }
        }
    }
}
