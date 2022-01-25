using System;
namespace CodeVsZombiesLibrary
{
    public abstract class Character
    {
        public int Id {get; private set;}
        public Position Pos {get; private set;}
        public int Speed {get; protected set;}

        public Character(int id, int xPos, int yPos)
        {
            this.Id = id;
            this.Pos = new Position(xPos, yPos);
        }

        public void UpdatePosition(Position pos)
        {
            this.Pos = pos;
        }

        public void UpdatePosition(int x, int y)
        {
            this.Pos = new Position(x, y);
        }

        public Position ComputeNextPos(Position targetPos)
        {
            return this.ComputeNextPos(this.Pos, targetPos);
        }

        public Position ComputeNextPos(Position sourcePos, Position targetPos)
        {
            if (this.Speed == 0) return sourcePos;

            if (sourcePos.x < 0 || sourcePos.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.x < 0 || targetPos.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPos), $"targetPos should not contains negative coordinates (values : {targetPos})");
            }
            
            if (sourcePos.Equals(targetPos)) return targetPos;

            double distToTarget = sourcePos.DistanceTo(targetPos);
            int deltaX = targetPos.x - sourcePos.x;
            int deltaY = targetPos.y - sourcePos.y;
            double realNextX = sourcePos.x + this.Speed*deltaX / distToTarget;
            double realNextY = sourcePos.y + this.Speed*deltaY / distToTarget;

            Position result = new Position(realNextX, realNextY);

            // if target reached, return target pos
            if (Math.Abs(result.x - sourcePos.x) >= Math.Abs(deltaX)
                && Math.Abs(result.y - sourcePos.y) >= Math.Abs(deltaY))
            {
                return targetPos;
            }

            return result;
        }

        public int TurnsToBeInRange(Position targetPos, int range)
            => this.TurnsToBeInRange(this.Pos, targetPos, range);

        public int TurnsToBeInRange(Position sourcePos, Position targetPos, int range)
        {
            if (sourcePos.x < 0 || sourcePos.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.x < 0 || targetPos.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPos), $"targetPos should not contains negative coordinates (values : {targetPos})");
            }

            if (this.Speed == 0) return -1;

            int result = 0;
            while (sourcePos.DistanceTo(targetPos) > range)
            {
                sourcePos = this.ComputeNextPos(sourcePos, targetPos);
                result += 1;
            }

            return result;
        }
    }
}
