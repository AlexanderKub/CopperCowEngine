using System;
using System.Numerics;

namespace CopperCowEngine.Rendering.Geometry
{ 
    public static class MatrixUtils
    {
        public static Matrix4x4 PerspectiveFovLeftHand(
            float fieldOfView,
            float aspectRatio,
            float nearPlaneDistance,
            float farPlaneDistance)
        {
            var num1 = (float) (1.0 / Math.Tan(fieldOfView * 0.5));
            var num2 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
            Matrix4x4 matrix;
            matrix.M11 = num1 / aspectRatio;
            matrix.M12 = matrix.M13 = matrix.M14 = 0.0f;
            matrix.M22 = num1;
            matrix.M21 = matrix.M23 = matrix.M24 = 0.0f;
            matrix.M33 = num2;
            matrix.M34 = 1f;
            matrix.M31 = matrix.M32 = 0.0f;
            matrix.M41 = matrix.M42 = matrix.M44 = 0.0f;
            matrix.M43 = -num2 * nearPlaneDistance;
            return matrix;
        }
    }
}
