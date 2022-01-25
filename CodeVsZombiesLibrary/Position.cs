using System;

namespace CodeVsZombiesLibrary
{
    public struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Position(double x, double y)
        {
            this.x = Position.RoundCoordinate(x);
            this.y = Position.RoundCoordinate(y);            
        }

        public double DistanceTo(Position pos)
        {
            // Position (-1, -1) means character is dead => return float.NaN as distance
            if (x < 0 || y < 0 || pos.x < 0 || pos.y < 0)
            {
                return double.NaN;
            }

            return Math.Sqrt(
                Math.Pow(pos.x-this.x, 2) + Math.Pow(pos.y-this.y, 2));
        }

        public bool Equals(Position pos)
        {
            return this.x == pos.x && this.y == pos.y;
        }

        private static int RoundCoordinate(double realCoordinate) 
            => (int)Math.Floor(realCoordinate);
    }
}