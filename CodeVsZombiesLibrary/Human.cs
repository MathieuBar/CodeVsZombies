using System;
using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Human : Character
    {
        public bool Doomed { get; set; }
        public int TurnsBeforeBeingCaught { get; private set; }
        public int ThreateningZombiesCount => this.ThreateningZombies.Count;
        private ISet<int> ThreateningZombies { get; set; }

        public Human(int id, int xPos, int yPos, Player owner = null):
            base(id, xPos, yPos, owner)
        {
            this.Speed = 0;
            this.Doomed = false;
            this.ThreateningZombies = new HashSet<int>();
            this.TurnsBeforeBeingCaught = int.MaxValue;
        }

        public Human(HumanInputs hi, Player owner = null): this(hi.Id, hi.X, hi.Y, owner)
        {
            // nothing to add
        }

        public HumanInputs ToHumanInputs()
        {
            return new HumanInputs(this.Id, this.Pos.X, this.Pos.Y);
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
            this.Doomed = false;
        }

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this.ClearThreateningZombies();
        }        
    }
}