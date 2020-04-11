using System;
using System.Numerics;
using CopperCowEngine.Core;
using CopperCowEngine.Core.Utils;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class FreeControlSystem : ComponentSystem<Required<FreeControl, Translation, Rotation>>
    {
        protected override void Update()
        {
            var input = Context.GetSingletonComponent<InputSingleton>();

            var moveForward = input.IsButtonDown(Buttons.Up);
            var moveBackward = input.IsButtonDown(Buttons.Down);
            var moveLeft = input.IsButtonDown(Buttons.Left);
            var moveRight = input.IsButtonDown(Buttons.Right);
            var hasSpeedMultiplier = input.IsButtonDown(Buttons.LeftShift);

            var axisZ = (moveForward ? 1f : 0) - (moveBackward ? 1f : 0);
            var axisX = (moveRight ? 1f : 0) - (moveLeft ? 1f : 0);
            var speedMultiplier = hasSpeedMultiplier ? 2f : 1f;

            var mouseDelta = input.MouseOffset * 8f * Time.Delta;

            foreach (var slice in Iterator)
            {
                ref var control = ref slice.Sibling<FreeControl>();
                ref var translation = ref slice.Sibling<Translation>();
                ref var rotation = ref slice.Sibling<Rotation>();

                control.Yaw += mouseDelta.X;
                control.Pitch += mouseDelta.Y;
                control.Pitch = Math.Min(Math.Max(control.Pitch, -89f), 89f);

                var yawPitchRoll = new Vector3(control.Yaw, control.Pitch, control.Roll).DegToRad();

                rotation.Value = Quaternion.CreateFromYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
                
                var speed = speedMultiplier * control.Speed * Time.Delta;

                var moveVector = (Vector3.UnitX * axisX + Vector3.UnitZ * axisZ) * speed;

                translation.Value += Vector3.Transform(moveVector, rotation.Value);
            }
        }
    }
}
