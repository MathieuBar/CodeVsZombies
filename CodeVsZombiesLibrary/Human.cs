using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Human : Character
    {
        public bool Doomed { get; set; }
        public int TurnsBeforeBeingCaught { get; private set; }
        private ISet<int> ThreateningZombies { get; set; }
        public int ThreateningZombiesCount => this.ThreateningZombies.Count;

        public Human(int id, int xPos, int yPos): base(id, xPos, yPos)
        {
            this.Speed = 0;
            this.Doomed = false;
            this.ThreateningZombies = new HashSet<int>();
            this.TurnsBeforeBeingCaught = int.MaxValue;
        }

        public Human(HumanInputs hi): this(hi.Id, hi.X, hi.Y)
        {
            // nothing to add
        }

        public HumanInputs ToHumanInputs()
        {
            return new HumanInputs(this.Id, this.Pos.x, this.Pos.y);
        }

        public bool AddThreateningZombie(Zombie zombie, int turnsToBeCoveredByHero)
        {
            if (zombie.NextTargetIsHero 
                || zombie.NextNearestHuman != this
                || this.ThreateningZombies.Contains(zombie.Id))
            {
                return false;
            }

            this.ThreateningZombies.Add(zombie.Id);
            if (zombie.TurnsToNearestHuman < this.TurnsBeforeBeingCaught)
            {
                this.TurnsBeforeBeingCaught = zombie.TurnsToNearestHuman;
            }

            if (zombie.TurnsToNearestHuman < turnsToBeCoveredByHero)
            {
                this.Doomed = true;
            }

            return true;
        }

        public void ClearThreateningZombies()
        {
            this.ThreateningZombies.Clear();
            this.TurnsBeforeBeingCaught = int.MaxValue;
        }
        
    }
}