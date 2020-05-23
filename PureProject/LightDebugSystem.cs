using System;
using System.Numerics;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;

namespace PureProject
{
    public class LightDebugSystem : ComponentlessSystem
    {
        private bool _requestToggle;
        private Entity _lightEntity;
        private bool _lightActive;
        private readonly Random _generator = new Random();

        protected override void OnCreate()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;
            engine.ScriptEngine.RegisterFunction("light", this, "ToggleLight");
        }

        protected override void Update()
        {
            if (!_requestToggle)
            {
                return;
            }
            _requestToggle = false;

            if (_lightActive)
            {
                _lightActive = false;
                DeferredContext.DestroyEntity(_lightEntity);
            }
            else
            {
                _lightActive = true;
                _lightEntity = DeferredContext.CreateEntity(typeof(Translation), typeof(DirectionalLight), typeof(LightColor));

                DeferredContext.SetComponent(_lightEntity, new Translation { Value = Vector3.UnitY * 10});
                DeferredContext.SetComponent(_lightEntity, new DirectionalLight { Direction = new Vector3(1, -1, 1), Intensity = 2f });
                DeferredContext.SetComponent(_lightEntity, new LightColor
                {
                    Value = new Vector3((float)_generator.NextDouble(), (float)_generator.NextDouble(), (float)_generator.NextDouble())
                });
            }
        }

        private void ToggleLight()
        {
            _requestToggle = true;
        }
    }
}