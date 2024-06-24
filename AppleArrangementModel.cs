using System;
using System.Numerics;

namespace Grafika_Projekt
{
    internal class AppleArrangementModel
    {
        private static Random random = new Random();
        public Vector3 Direction;
        private int updatesSinceLastDirectionChange = 0;
        private const double skyboxSize = 4000;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; } = 0;
        public float Speed { get; set; } = 0.5f;
        public double Scale { get; private set; } = 1;
        public GlObject GlObject { get; internal set; }
        public bool IsEaten { get; internal set; }

        public AppleArrangementModel()
        {
            Spawn();
        }

        public void Spawn()
        {
            X = -10;
            Y = random.NextDouble() %(skyboxSize - skyboxSize / 2);
            Z = 0;
            Direction = new Vector3((float)(random.NextDouble() - 0.5), (float)(random.NextDouble() - 0.5), 0);
            Direction = Vector3.Normalize(Direction); 
        }

        public void Move()
        {
            // Move the apple
            if (updatesSinceLastDirectionChange >= 100)
            {
                Direction = new Vector3((float)(random.NextDouble() - 0.5), (float)(random.NextDouble() - 0.5), 0);
                Direction = Vector3.Normalize(Direction); 
                updatesSinceLastDirectionChange = 0;
            }

             double newX = X + Direction.X * Speed; 
             double newY = Y + Direction.Y * Speed;

             bool withinXBounds = Math.Abs(newX) <= skyboxSize / 2 - Scale;
             bool withinYBounds = Math.Abs(newY) <= skyboxSize / 2 - Scale;

             if (withinXBounds)
             {
                 X = newX;
             }
             else
             {
                 // Reflect direction when hitting X bounds
                 Direction = new Vector3(-Direction.X, Direction.Y, Direction.Z);
             }

             if (withinYBounds)
             {
                 Y = newY;
             }
             else
             {
                 // Reflect direction when hitting Y bounds
                 Direction = new Vector3(Direction.X, -Direction.Y, Direction.Z);
             }


             updatesSinceLastDirectionChange++;
        }

    }

}
