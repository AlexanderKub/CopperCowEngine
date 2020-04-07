using System.Collections.Generic;

namespace CopperCowEngine.Rendering.Data
{
    public class StandardFrameData : FrameData
    {
        public readonly List<CameraData> CamerasList;

        public readonly List<LightData> LightsList;

        public readonly Dictionary<int, List<int>> PerCameraRenderers;
        public readonly Dictionary<int, List<int>> PerCameraLights;
        public readonly Dictionary<int, List<int>> PerLightRenderers;

        public readonly List<RendererData> RenderersList;


        // TODO: change struct.
        /*****************************************
         * Cameras -> Scene per camera (shader and mesh sorted).
         * Lights -> Scene per light (only opaque).
         *****************************************/
        public StandardFrameData()
        {
            CamerasList = new List<CameraData>();
            LightsList = new List<LightData>();
            PerCameraRenderers = new Dictionary<int, List<int>>();
            PerCameraLights = new Dictionary<int, List<int>>();
            PerLightRenderers = new Dictionary<int, List<int>>();
            RenderersList = new List<RendererData>();
        }

        public override void Reset()
        {
            PerCameraLights.Clear();
            PerCameraRenderers.Clear();
            PerLightRenderers.Clear();
            CamerasList.Clear();
            LightsList.Clear();
            RenderersList.Clear();
        }

        public void AddCameraData(CameraData data)
        {
            data.Index = CamerasList.Count;
            PerCameraRenderers.Add(data.Index, new List<int>());
            PerCameraLights.Add(data.Index, new List<int>());
            CamerasList.Add(data);
        }

        public void AddLightData(LightData data)
        {
            data.Index = LightsList.Count;
            LightsList.Add(data);
            PerLightRenderers.Add(data.Index, new List<int>());
        }

        public int AddRendererData(RendererData data)
        {
            RenderersList.Add(data);

            return RenderersList.Count - 1;
        }

        public void AddLightDataToCamera(int cameraIndex, int lightIndex)
        {
            PerCameraLights[cameraIndex].Add(lightIndex);
        }

        public void AddRendererDataToCamera(int cameraIndex, int rendererIndex)
        {
            PerCameraRenderers[cameraIndex].Add(rendererIndex);
        }

        public void AddRendererDataToLight(int lightIndex, int rendererIndex)
        {
            PerLightRenderers[lightIndex].Add(rendererIndex);
        }


        /*public class ByRendererProps : IComparer<RendererData>
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
        }*/
    }
}
