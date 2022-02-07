using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Zombie: Character
    {
        public const int UndefinedTurnsToNearestHuman = -1;
        private const int _zombieSpeed = 400;

        private Human _nearestHuman;
        private double _distToNearestHuman;
        private int _turnsToNearestHuman;
        private bool? _nextTargetIsHero;
        private Position _givenNextPosition;
        private Position _computedNextPosition;


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos, IStateChangedEventSender owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Zombie._zombieSpeed;
            this.ClearComputedData();
            this._givenNextPosition = new Position(nextXPos, nextYPos);
        }

        public Zombie(int id, int xPos, int yPos, IStateChangedEventSender owner = null): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y, owner)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi, IStateChangedEventSender owner = null): 
            this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext, owner)
        {
            // nothing to add
        }

        public ZombieInputs ToZombieInputs()
        {
            return new ZombieInputs(
                this.Id, this.Pos.X, this.Pos.Y, this._givenNextPosition.X, this._givenNextPosition.Y);
        }

        public override void UpdatePosition(Position pos)
        {
            base.UpdatePosition(pos);
            this._givenNextPosition = Position.UndefinedPos;
        }

        public void UpdateFromNewInputs(ZombieInputs newTurnZombieInputs)
        {
            // update positions
            base.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
        }

        /// <summary>
        /// Update this zombie position by simulating next move for a new turn.
        /// </summary>
        /// <param name="hero">hero of the game</param>
        /// <param name="humans">humans of the game</param>
        public void UpdateByNewTurnSimulation(Hero hero, IEnumerable<Human> humans)
        {
            this.UpdatePosition(this.GetNextPosition(hero, humans));
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

            this._distToNearestHuman = distToNearestHuman;
            this._nearestHuman = nearestHuman;
            return this._nearestHuman;
        }

        public double GetDistToNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (!double.IsNaN(this._distToNearestHuman))
            {
                return this._distToNearestHuman;
            }

            Human nearestHuman = this.GetNearestHuman(humans);
            this._distToNearestHuman = nearestHuman.Pos.DistanceTo(this.Pos);
            return this._distToNearestHuman;
        }

        public int GetTurnsToNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._turnsToNearestHuman != UndefinedTurnsToNearestHuman)
            {
                return this._turnsToNearestHuman;
            }

            Human nearestHuman = this.GetNearestHuman(humans);
            this._turnsToNearestHuman = this.TurnsToBeInRange(nearestHuman.Pos, 0);
            return this._turnsToNearestHuman;
        }

        public bool GetNextTargetIsHero(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._nextTargetIsHero.HasValue)
            {
                return this._nextTargetIsHero.Value;
            }

            double distToHero = hero.Pos.DistanceTo(this.Pos);
            double distToNearestHuman = this.GetDistToNearestHuman(humans);

            this._nextTargetIsHero = distToHero < distToNearestHuman;
            return this._nextTargetIsHero.Value;
        }

        public Position GetNextPosition(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

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

            // No next position given nor computed => compute next pos
            bool nextTargetIsHero = this.GetNextTargetIsHero(hero, humans);
            Position targetPos = nextTargetIsHero ? hero.Pos : this.GetNearestHuman(humans).Pos;
            this._computedNextPosition = this.ComputeNextPos(targetPos);
            return this._computedNextPosition;
        }

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this.ClearComputedData();
        }

        private void ClearComputedData()
        {
            this._nearestHuman = null;
            this._distToNearestHuman = double.NaN;
            this._turnsToNearestHuman = UndefinedTurnsToNearestHuman;
            this._nextTargetIsHero = null;
            this._computedNextPosition = Position.UndefinedPos;
        }
    }
}