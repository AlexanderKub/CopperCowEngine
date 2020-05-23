using CopperCowEngine.ECS.Builtin.Systems;

namespace CopperCowEngine.ECS.Builtin
{
    public partial class EngineEcsContext
    {
        private int _initializationOrder;

        private int _simulationOrder;

        private int _presentationOrder;

        public void CreateSystem<T>(SystemOrderGroup orderGroup) where T : ComponentSystem, new()
        {
            var orderValue = orderGroup switch
            {
                SystemOrderGroup.Initialization => (10000 + _initializationOrder++),
                SystemOrderGroup.Simulation => (20000 + _simulationOrder++),
                SystemOrderGroup.Presentation => (30000 + _presentationOrder++),
                _ => 0
            };
            CreateSystem<T>(orderValue);
        }

        private void CreateEngineSystems()
        {
            // TODO: solve ordering problem
            CreateSystem<InputSystem>(SystemOrderGroup.Initialization);
            //CreateSystem<ConsoleSystem>(SystemOrderGroup.Initialization);

            CreateSystem<FreeControlSystem>(SystemOrderGroup.Simulation);
            CreateSystem<TrsToLocalToWorldSystem>(SystemOrderGroup.Simulation);
            CreateSystem<TrsToLocalToParentSystem>(SystemOrderGroup.Simulation);
            CreateSystem<LocalToParentSystem>(SystemOrderGroup.Simulation);

            CreateSystem<RenderingPrepareSystem>(SystemOrderGroup.Presentation);
            CreateSystem<CameraScreenAspectSystem>(SystemOrderGroup.Presentation);
            CreateSystem<CameraSystem>(SystemOrderGroup.Presentation);
            CreateSystem<LightsSystem>(SystemOrderGroup.Presentation);
            CreateSystem<RenderingSystem>(SystemOrderGroup.Presentation);
            CreateSystem<ConsoleSystem>(SystemOrderGroup.Presentation);
        }
    }
}
