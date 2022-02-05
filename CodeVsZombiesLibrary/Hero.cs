using System;
using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int ShootRange = 2000;
        private const int _heroSpeed = 1000;
        
        private Dictionary<int, int> TurnsToGetInRangeToHuman { get; set; }

        public Hero(int id, int xPos, int yPos, Player owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Hero._heroSpeed;
            this.TurnsToGetInRangeToHuman = new Dictionary<int, int>();
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

        public int GetTurnsToGetInRangeToHuman(int humanId)
        {
            bool hasValue = this.TurnsToGetInRangeToHuman.TryGetValue(humanId, out int result);
            if (!hasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(humanId), $"Unknown distance in turns to human id {humanId}");
            }
            return(result);
        }

        public void UpdateDistancesToHumans(IEnumerable<Human> humans)
        {
            foreach (Human human in humans)
            {
                this.TurnsToGetInRangeToHuman[human.Id] = this.TurnsToBeInShootRange(human.Pos);
            }
        }

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this.TurnsToGetInRangeToHuman.Clear();
        }
    }
}