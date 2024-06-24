using Silk.NET.Maths;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Grafika_Projekt
{
    internal class SquirrelArrangementModel
    {
        public Vector3 Position
        {
            get { return new Vector3((float)X, (float)Y, (float)Z); }
        }
        private const double skyboxSize = 4000;
        public float RotationAngle { get; private set; } = 0.0f;

        public double X { get; private set; } = 0;
        public double Y { get; private set; } = 0;
        public double Z { get; private set; } = 0;
        public Vector3 PreviousPosition { get; private set; }

        public double Scale { get; private set; } =0.07 ;

        public float Speed { get; set; } = 1f;

        internal void IncreaseScale(double amount)
        {
            Scale += amount;
        }

        internal void DecreaseScale(double amount)
        {
            Scale -= amount;
        }


        internal void MoveLeft(double distance)
        {
            PreviousPosition = Position;
            double newX = X + distance*Speed;
            if (Math.Abs(newX) <= skyboxSize / 2 - Scale) 
            {
                X = newX;
            }
        }  
        internal void MoveRight(double distance)
        {
             PreviousPosition = Position;
            double newX = X - distance*Speed;
            if (Math.Abs(newX) <= skyboxSize / 2 - Scale)
            {
                X = newX;
            }
        }

        internal void MoveForward(double distance)
        {
            PreviousPosition = Position;
            double newY = Y - distance * Speed;
            if (Math.Abs(newY) <= skyboxSize / 2 -Scale)
            {
                Y = newY;
            }
            RotationAngle = 0.0f;
        }
        internal void MoveBackward(double distance)
        {
            PreviousPosition = Position;
            double newY = Y + distance * Speed;
            if (Math.Abs(newY) <= skyboxSize / 2 -Scale)
            {
                Y = newY;
            }
            RotationAngle = 180.0f;
        }
        public Vector3 GetDirection()
        {
            return Vector3.Normalize(Position - PreviousPosition);
        }

        public bool CollidesWith(AppleArrangementModel apple)
        {
            // Calculate the distance between the squirrel and the apple
            double distance = Math.Sqrt(Math.Pow(X - apple.X, 2) + Math.Pow(X - apple.Y, 2) + Math.Pow(Z - apple.Z, 2));

            // If the distance is less than the sum of their scales, they are colliding
            return distance < Scale / 2 + apple.Scale/2 ;
        }

    }
}
