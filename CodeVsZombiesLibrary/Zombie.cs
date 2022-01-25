using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Zombie: Character
    {
        private const int ZombieSpeed = 400;

        public Position NextPosition {get; private set;}
        public Human NextNearestHuman {get; private set;}
        public int TurnsToNearestHuman {get; private set;}
        public bool NextTargetIsHero {get; private set;}
            

        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos): 
            base(id, xPos, yPos)
        {
            this.NextPosition = new Position(nextXPos, nextYPos);
            this.Speed = Zombie.ZombieSpeed;
        }

        public Zombie(ZombieInputs zi): this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext)
        {
            // nothing to add
        }

        public ZombieInputs ToZombieInputs()
        {
            return new ZombieInputs(
                this.Id, this.Pos.x, this.Pos.y, this.NextPosition.x, this.NextPosition.y);
        }

        public void UpdateBothPositions(int x, int y, int nextX, int nextY)
        {
            this.UpdatePosition(x, y);
            this.NextPosition = new Position(nextX, nextY);
        }

        public void UpdateTarget(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            double distToHero = hero.Pos.DistanceTo(this.NextPosition);

            Human nearestHuman = null;
            double distToNearestHuman = double.MaxValue;
            foreach (Human human in humans)
            {
                double distToHuman = human.Pos.DistanceTo(this.NextPosition);
                if (distToHuman < distToNearestHuman
                    || (distToHuman == distToNearestHuman && human.Id < nearestHuman.Id))
                {
                    distToNearestHuman = distToHuman;
                    nearestHuman = human;
                }
            }

            this.NextNearestHuman = nearestHuman;
            this.TurnsToNearestHuman = this.TurnsToBeInRange(this.NextPosition, nearestHuman.Pos, 0) + 1;
            this.NextTargetIsHero = distToHero < distToNearestHuman;
        }
    }
}