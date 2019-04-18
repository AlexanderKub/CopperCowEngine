using EngineCore.ECS;
using EngineCore.ECS.Systems;
using EngineCore.ECS.Components;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineCore;

namespace ECSTestProject
{
    public class TestSystem : BasicSystem<Requires<Transform, TestComponent>>
    {
        protected override void OnInit()
        {
            Debug.Log("TestSystem init.");
        }

        public override void Update(Timer timer)
        {
            SingletonInput input = WorldRef.GetSingletonComponent<SingletonInput>();

            Entity[] entities = GetEntities();

            Transform transform;
            TestComponent test;

            foreach (Entity entity in entities)
            {
                test = entity.GetComponent<TestComponent>();
               /* if (input.IsButtonDown(SingletonInput.Buttons.LEFT))
                {
                    test.HorizontalDirection = -1;
                }
                if (input.IsButtonDown(SingletonInput.Buttons.RIGHT))
                {
                    test.HorizontalDirection = 1;
                }

                if (input.IsButtonDown(SingletonInput.Buttons.DOWN))
                {
                    test.VerticalDirection = -1;
                }
                if (input.IsButtonDown(SingletonInput.Buttons.UP))
                {
                    test.VerticalDirection = 1;
                }*/

                transform = entity.GetComponent<Transform>();
                transform.Rotation *= Quaternion.RotationYawPitchRoll(timer.DeltaTime * test.HorizontalDirection, 0, 0);//timer.DeltaTime * test.VerticalDirection
                //transform.Scale = Vector3.One * ((float)Math.Sin(timer.Time * 3f) * 0.05f + 0.2f);
                transform.Position += (Vector3.ForwardLH + Vector3.Left) * 4 * (float)Math.Cos(timer.Time) * 0.01f;
            }
        }

        protected override void OnDestroy()
        {
            Debug.Log("TestSystem destroy.");
        }
    }
}
