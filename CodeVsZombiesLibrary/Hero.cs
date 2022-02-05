using System;
using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int ShootRange = 2000;
        private const int _heroSpeed = 1000;

        private Dictionary<int, int> _turnsToGetInRangeToHuman;

        public Hero(int id, int xPos, int yPos, Player owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Hero._heroSpeed;
            this._turnsToGetInRangeToHuman = new Dictionary<int, int>();
        }

        public Hero(int xPos, int yPos, Player owner = null): 
            this(DefaultHeroId, xPos, yPos, owner)
        {
            // nothing to add
        }

        public Hero(Inputs inputs, Player owner = null): this(inputs.X, inputs.Y, owner)
        {
            // nothing to add
        }

        public int TurnsToBeInShootRange(Position targetPos)
        {
            return this.TurnsToBeInRange(targetPos, Hero.ShootRange);
        }

        public int GetTurnsToGetInRangeToHuman(Human human)
        {
            bool hasValue = this._turnsToGetInRangeToHuman.TryGetValue(human.Id, out int result);
            if (!hasValue)
            {
                result = this.TurnsToBeInShootRange(human.Pos);
                this._turnsToGetInRangeToHuman[human.Id] = result;
            }
            return result;
        }

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this._turnsToGetInRangeToHuman.Clear();
            this.UpdatePosition(Position.UndefinedPos);
        }
    }
}