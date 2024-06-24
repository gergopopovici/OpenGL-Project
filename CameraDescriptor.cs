using Silk.NET.Maths;
using System;
using System.Numerics;

namespace Grafika_Projekt
{
    internal class CameraDescriptor
    {
        private Vector3D<float> originPoint = new Vector3D<float>(0, 0, 0);
        private float Distance = 40;
        private float yaw = (float)Math.PI / 180 * 90;
        private float pitch = 0;
        private const float AngleChangeStepSize = (float)Math.PI / 180;
        private const float angleClamp = (float)Math.PI / 180 * 25;
        private const float CameraSensitivity = 0.01f;
        internal float MovementSpeed = 7.0f;
        internal Vector3D<float> positionClamp = new Vector3D<float>(1750f, 810f, 1750f);
        private const float MinDistance = 10.0f;
        private const float MaxDistance = 60.0f;
        private CameraMode mode = CameraMode.ThirdPerson;
        private const float FirstPersonOffset = 2.0f;
        private const float SkyboxSize = 4000.0f;
        private const float FirstPersonOffsetZ = -2.0f;
        private const float FirstPersonOffsetY = 1.8f;

        public Vector3D<float> Position
        {
            get
            {
                if (mode == CameraMode.FirstPerson)
                {
                    return originPoint + new Vector3D<float>(0, FirstPersonOffsetY, FirstPersonOffsetZ);
                }
                else
                {
                    return originPoint - Distance * GetCameraFront();
                }
            }
        }

        public Vector3D<float> Origin { set => originPoint = value; }
        public Vector3D<float> UpVector => new Vector3D<float>(0, 1, 0);
        public Vector3D<float> Target => originPoint;

        public void ChangeAngle(Vector2 pos)
        {
            yaw += pos.X * AngleChangeStepSize * CameraSensitivity;
            pitch -= pos.Y * AngleChangeStepSize * CameraSensitivity;
            pitch = Math.Clamp(pitch, -angleClamp, angleClamp);
        }

        internal Vector3D<float> GetCameraFront()
        {
            Vector3D<float> front;
            front.X = MathF.Cos(yaw) * MathF.Cos(pitch);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Sin(yaw) * MathF.Cos(pitch);
            return Vector3D.Normalize(front);
        }

        internal Vector3D<float> GetCameraRight()
        {
            return Vector3D.Normalize(Vector3D.Cross(GetCameraFront(), UpVector));
        }

        public void Update(Vector3D<float> squirrelPosition, Vector2 movementDirection, float deltaTime)
        {
            if (mode == CameraMode.ThirdPerson)
            {
                if (!IsSquirrelVisible(squirrelPosition))
                {
                    originPoint = Vector3D.Lerp(originPoint, squirrelPosition, 1f * deltaTime);
                }

                Distance = Math.Clamp(Distance, MinDistance, MaxDistance);
                LockCameraOntoSquirrel(squirrelPosition);
                yaw = yaw % (2 * MathF.PI);
            }
            else
            {
                if (Math.Abs(squirrelPosition.X) < 200)
                {
                    Vector3D<float> targetPosition = squirrelPosition + new Vector3D<float>(0, FirstPersonOffsetY, FirstPersonOffsetZ);
                    Vector3D<float> newPosition = Vector3D.Lerp(originPoint, targetPosition, MovementSpeed * deltaTime);
                    originPoint = ClampToSkybox(newPosition);
                }

            }
         }

            private Vector3D<float> ClampToSkybox(Vector3D<float> position)
        {
            float halfSkyboxSize = SkyboxSize / 2;
            position.X = Math.Clamp(position.X, -halfSkyboxSize, halfSkyboxSize);
            position.Y = Math.Clamp(position.Y, 0, SkyboxSize);
            position.Z = Math.Clamp(position.Z, -halfSkyboxSize, halfSkyboxSize);
            return position;
        }

        private bool IsSquirrelVisible(Vector3D<float> squirrelPosition)
        {
            Vector3D<float> directionToSquirrel = Vector3D.Normalize(squirrelPosition - Position);
            float dotProduct = Vector3D.Dot(GetCameraFront(), directionToSquirrel);
            return dotProduct > 0.5f;
        }

        private void LockCameraOntoSquirrel(Vector3D<float> squirrelPosition)
        {
            Vector3D<float> direction = Vector3D.Normalize(squirrelPosition - Position);
            yaw = MathF.Atan2(direction.Z, direction.X);
            pitch = MathF.Asin(direction.Y);
        }

        public void ToggleCameraMode()
        {
            originPoint = new Vector3D<float>(0,0,0);
            mode = mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
        }
    }

    internal enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }
}
