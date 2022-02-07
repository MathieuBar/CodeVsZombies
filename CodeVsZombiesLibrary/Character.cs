using System;
namespace CodeVsZombiesLibrary
{
    public abstract class Character
    {
        public int Id {get; private set;}
        public Position Pos {get; private set;}
        public int Speed {get; protected set;}
        protected IStateChangedEventSender Owner { get; private set; }

        public Character(int id, int xPos, int yPos, IStateChangedEventSender owner = null)
        {
            this.Id = id;
            this.Pos = new Position(xPos, yPos);
            this.Owner = owner;

            if (owner != null)
            {
                owner.StateChanged += OnNewTurnStarted;
            }
        }

        public virtual void UpdatePosition(Position pos)
        {
            this.Pos = pos;
        }

        public void UpdatePosition(int x, int y)
            => this.UpdatePosition(new Position(x, y));

        public Position ComputeNextPos(Position targetPos)
        {
            return this.ComputeNextPos(this.Pos, targetPos);
        }

        public Position ComputeNextPos(Position sourcePos, Position targetPos)
        {
            if (this.Speed == 0) return sourcePos;

            if (sourcePos.X < 0 || sourcePos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.X < 0 || targetPos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPos), $"targetPos should not contains negative coordinates (values : {targetPos})");
            }
            
            if (sourcePos.Equals(targetPos)) return targetPos;

            double distToTarget = sourcePos.DistanceTo(targetPos);
            int deltaX = targetPos.X - sourcePos.X;
            int deltaY = targetPos.Y - sourcePos.Y;
            double realNextX = sourcePos.X + this.Speed*deltaX / distToTarget;
            double realNextY = sourcePos.Y + this.Speed*deltaY / distToTarget;

            Position result = new Position(realNextX, realNextY);

            // if target reached, return target pos
            if (Math.Abs(result.X - sourcePos.X) >= Math.Abs(deltaX)
                && Math.Abs(result.Y - sourcePos.Y) >= Math.Abs(deltaY))
            {
                return targetPos;
            }

            return result;
        }

        public int TurnsToBeInRange(Position targetPos, int range)
            => this.TurnsToBeInRange(this.Pos, targetPos, range);

        public int TurnsToBeInRange(Position sourcePos, Position targetPos, int range)
        {
            if (sourcePos.X < 0 || sourcePos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.X < 0 || targetPos.Y < 0)
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

        protected virtual void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            // Nothing to do in Character class. To be implemented in derived classes.
        }
    }
}
