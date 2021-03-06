﻿using System;
using System.Numerics;

namespace CopperCowEngine.Core.Utils
{
    public static class NumericsExtensions
    {
        public static Vector3 DegToRad (this Vector3 degrees)
        {
            return MathF.PI / 180f * degrees;
        }
    }
}
