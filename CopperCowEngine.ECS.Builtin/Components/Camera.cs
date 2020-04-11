﻿using System;
using System.Numerics;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct CameraSetup : IComponentData
    {
        public float NearClippingPlane;

        public float FarClippingPlane;

        public float Fov;

        public float AspectRatio;

        public static CameraSetup Default = new CameraSetup
        {
            NearClippingPlane = 0.001f,
            FarClippingPlane = 10000f,
            Fov = (float)(Math.PI * 0.5),
            AspectRatio = 1,
        };
    }

    public struct CameraProjection : IComponentData
    {
        public Matrix4x4 Value;
    }

    public struct CameraViewProjection : IComponentData
    {
        public Matrix4x4 View;

        public Matrix4x4 ViewProjection;
    }
}
