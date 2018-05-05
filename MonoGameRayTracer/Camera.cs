using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;
using System;

namespace MonoGameRayTracer
{
    public class Camera
    {
        private Vector3 m_Origin;
        private Vector3 m_Rotation;
        private Vector3 m_LowerLeftCorner;
        private Vector3 m_Horizontal;
        private Vector3 m_Vertical;
        private Vector3 m_LookAt;
        private Vector3 m_UpVector;
        private float m_FieldOfView;
        private float m_Aspect;

        public Vector3 Position => m_Origin;
        public Vector3 LowerLeftCorner => m_LowerLeftCorner;
        public Vector3 Horizontal => m_Horizontal;
        public Vector3 Vertical => m_Vertical;

        private Matrix m_RotationMatrix;

        public Camera()
        {
            m_Origin = new Vector3(0.0f, 0.0f, 0.0f);
            m_RotationMatrix = Matrix.CreateFromYawPitchRoll(m_Rotation.Y, m_Rotation.X, m_Rotation.Z);
            m_LookAt = m_Origin + Vector3.Transform(Vector3.Forward, m_RotationMatrix);
            m_UpVector = Vector3.Up;
            m_FieldOfView = 75.0f * MathHelper.Pi / 180.0f;
            m_Aspect = (float)640 / (float)480;

            ComputeMatrix();
        }

        public Camera(Vector3 origin, Vector3 rotation, Vector3 upVector, float fov, float aspect)
        {
            m_Origin = origin;
            m_RotationMatrix = Matrix.CreateFromYawPitchRoll(m_Rotation.Y, m_Rotation.X, m_Rotation.Z);
            m_LookAt = m_Origin + Vector3.Transform(Vector3.Forward, m_RotationMatrix);
            m_UpVector = upVector;
            m_FieldOfView = fov;
            m_Aspect = aspect;

            ComputeMatrix();
        }

        private void ComputeMatrix()
        {
            var theta = m_FieldOfView * MathHelper.Pi / 180.0f;
            var halfHeight = Mathf.Tan(theta / 2.0f);
            var halfWidth = m_Aspect * halfHeight;

            var w = Mathf.UnitVector(m_Origin - m_LookAt);
            var u = Mathf.UnitVector(Vector3.Cross(m_UpVector, w));
            var v = Vector3.Cross(w, u);

            m_LowerLeftCorner.X = -halfWidth;
            m_LowerLeftCorner.Y = -halfHeight;
            m_LowerLeftCorner.Z = -1.0f;

            m_LowerLeftCorner = m_Origin - halfWidth * u - halfHeight * v - w;
            m_Horizontal = 2.0f * halfWidth * u;
            m_Vertical = 2.0f * halfHeight * v;
        }

        public Ray GetRay(ref float u, ref float v) => new Ray(m_Origin, m_LowerLeftCorner + u * m_Horizontal + v * m_Vertical - m_Origin);

        public void Move(float x, float y, float z)
        {
            m_Origin.X += x;
            m_Origin.Y += y;
            m_Origin.Z += z;

            m_Origin += Vector3.Transform(new Vector3(x, y, z), m_RotationMatrix);
            m_LookAt = m_Origin + Vector3.Transform(Vector3.Forward, m_RotationMatrix);

            ComputeMatrix();
        }

        public void Rotate(float x, float y, float z)
        {
            m_Rotation.X += x;
            m_Rotation.Y += y;
            m_Rotation.Z += z;
            m_RotationMatrix = Matrix.CreateFromYawPitchRoll(m_Rotation.Y, m_Rotation.X, m_Rotation.Z);

            m_LookAt = m_Origin + Vector3.Transform(Vector3.Forward, m_RotationMatrix);

            ComputeMatrix();
        }
    }
}
