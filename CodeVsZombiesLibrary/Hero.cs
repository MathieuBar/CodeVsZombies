using System;
using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int ShootRange = 2000;
        private const int HeroSpeed = 1000;
        private Dictionary<int, int> TurnsToGetInRangeToHuman { get; set; }

        public Hero(int id, int xPos, int yPos): base(id, xPos, yPos)
        {
            this.Speed = Hero.HeroSpeed;
            this.TurnsToGetInRangeToHuman = new Dictionary<int, int>();
        }

        public Hero(int xPos, int yPos): this(DefaultHeroId, xPos, yPos)
        {
            // nothing to add
        }

        public Hero(Inputs inputs): this(inputs.X, inputs.Y)
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
    }
}