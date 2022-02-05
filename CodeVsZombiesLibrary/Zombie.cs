using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Zombie: Character
    {
        public const int UndefinedTurnsToNearestHuman = -1;
        private const int _zombieSpeed = 400;

        public int TurnsToNearestHuman {get; private set;}
        public bool NextTargetIsHero {get; private set;}
        
        private Human _nearestHuman;
        private int _turnsToNearestHuman;
        private bool? _nextTargetIsHero;
        private Position _targetPos;
        private Position _givenNextPosition;
        private Position _computedNextPosition;


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos, Player owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Zombie._zombieSpeed;
            this.ClearComputedData();
            this._givenNextPosition = new Position(nextXPos, nextYPos);
        }

        public Zombie(int id, int xPos, int yPos, Player owner = null): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y, owner)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi, Player owner = null): 
            this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext, owner)
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
            // update positions
            this.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
        }

        public Human GetNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._nearestHuman != null)
            {
                return this._nearestHuman;
            }

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

            this._nearestHuman = nearestHuman;
            return this._nearestHuman;
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

            this._nearestHuman = nearestHuman;
            this.TurnsToNearestHuman = this.TurnsToBeInRange(this.Pos, nearestHuman.Pos, 0);
            this.NextTargetIsHero = distToHero < distToNearestHuman;
            this._targetPos = this.NextTargetIsHero ? hero.Pos : this._nearestHuman.Pos;
        }

        public Position GetNextPosition()
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

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this.ClearComputedData();
            this.UpdatePosition(Position.UndefinedPos);
            this._givenNextPosition = Position.UndefinedPos;
        }

        private void ClearComputedData()
        {
            this._nearestHuman = null;
            this._turnsToNearestHuman = UndefinedTurnsToNearestHuman;
            this._nextTargetIsHero = null;
            this._targetPos = Position.UndefinedPos;
            this._computedNextPosition = Position.UndefinedPos;
        }
    }
}