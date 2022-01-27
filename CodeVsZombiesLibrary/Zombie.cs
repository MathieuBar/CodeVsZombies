using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Zombie: Character
    {
        private const int _zombieSpeed = 400;

        public Position NextPosition => this.GetNextPosition();
        public Human NextNearestHuman {get; private set;}
        public int TurnsToNearestHuman {get; private set;}
        public bool NextTargetIsHero {get; private set;}
        
        private Position _givenNextPosition;
        private Position _targetPos;
        private Position _computedNextPosition;


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos): 
            base(id, xPos, yPos)
        {
            this._givenNextPosition = new Position(nextXPos, nextYPos);
            this.Speed = Zombie._zombieSpeed;
            this.ClearStatefullData();
        }

        public Zombie(int id, int xPos, int yPos): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi): this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext)
        {
            // nothing to add
        }

        public ZombieInputs ToZombieInputs()
        {
            return new ZombieInputs(
                this.Id, this.Pos.X, this.Pos.Y, this._givenNextPosition.X, this._givenNextPosition.Y);
        }


        public void UpdateFromNewInputs(ZombieInputs newTurnZombieInputs)
        {
            this.ClearStatefullData();
            
            // update positions
            this.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
        }

        public void UpdateTarget(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            double distToHero = hero.Pos.DistanceTo(this.Pos);

            Human nearestHuman = null;
            double distToNearestHuman = double.MaxValue;
            foreach (Human human in humans)
            {
                double distToHuman = human.Pos.DistanceTo(this.Pos);
                if (distToHuman < distToNearestHuman
                    || (distToHuman == distToNearestHuman && human.Id < nearestHuman.Id))
                {
                    distToNearestHuman = distToHuman;
                    nearestHuman = human;
                }
            }

            this.NextNearestHuman = nearestHuman;
            this.TurnsToNearestHuman = this.TurnsToBeInRange(this.Pos, nearestHuman.Pos, 0);
            this.NextTargetIsHero = distToHero < distToNearestHuman;
            this._targetPos = this.NextTargetIsHero ? hero.Pos : this.NextNearestHuman.Pos;
        }

        private void ClearStatefullData()
        {
            this._computedNextPosition = Position.UndefinedPos;
            this._targetPos = Position.UndefinedPos;
            this.NextNearestHuman = null;
            this.TurnsToNearestHuman = -1;
            this.NextTargetIsHero = false;
        }

        private Position GetNextPosition()
        {
            // Next position has been given => return given next position
            if (!this._givenNextPosition.Equals(Position.UndefinedPos))
            {
                return this._givenNextPosition;
            }   

            // No next position given => if next position already computed return it
            if (!this._computedNextPosition.Equals(Position.UndefinedPos))
            {
                return _computedNextPosition;
            }

            // No next position given nor computed => _targetPos must have been set to continue
            if (this._targetPos.Equals(Position.UndefinedPos))
            {
                throw new InvalidOperationException(
                    "Unable to give next position when the next position  " +
                    "was not given and target position is not set yet. " +
                    "Update targets first !");
            }

            // No next position given nor computed, but targetPos is known => compute next pos
            this._computedNextPosition = this.ComputeNextPos(this._targetPos);
            return this._computedNextPosition;
        }
    }
}