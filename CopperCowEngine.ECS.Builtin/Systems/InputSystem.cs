using CopperCowEngine.ECS.Builtin.Singletons;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class InputSystem : ComponentlessSystem
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;

            ref var input = ref Context.GetSingletonComponent<InputSingleton>();

            input.UpdateMousePosition(engine.Input.MousePosition);
        }
    }
}
