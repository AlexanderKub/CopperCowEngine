using CopperCowEngine.ECS.Builtin.Singletons;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class InputSystem : ComponentlessSystem
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;

            ref var input = ref Context.GetSingletonComponent<InputSingleton>();

            if (input.IsButtonDown(Buttons.Esc))
            {
                engine.Quit();
                return;
            }

            input.UpdateMousePosition(engine.Input.MousePosition);
        }
    }
}
