using System;

namespace CodeVsZombiesLibrary
{
    public struct Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Position(double x, double y)
        {
            this.X = Position.RoundCoordinate(x);
            this.Y = Position.RoundCoordinate(y);            
        }

        public double DistanceTo(Position pos)
        {
            // Position (-1, -1) means character is dead => return float.NaN as distance
            if (X < 0 || Y < 0 || pos.X < 0 || pos.Y < 0)
            {
                return double.NaN;
            }

            return Math.Sqrt(
                Math.Pow(pos.X-this.X, 2) + Math.Pow(pos.Y-this.Y, 2));
        }

        public bool Equals(Position pos)
        {
            return this.X == pos.X && this.Y == pos.Y;
        }

        private static int RoundCoordinate(double realCoordinate) 
            => (int)Math.Floor(realCoordinate);
    }
}