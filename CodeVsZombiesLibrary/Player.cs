using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Player
    {
        public event EventHandler NewTurnStarted;

        private Hero Ash { get; set; }
        private Dictionary<int, Human> Humans { get; set; }
        private Dictionary<int, Zombie> Zombies { get; set; }
        
        private Position _nextZombiesBarycentre;
        private ISet<int> _humansDoomed;
        private bool _humansDoomedIsSet;

        public Player(Inputs startInputs)
        {
            this.InitFromInputs(startInputs);
        }

        public void InitFromInputs(Inputs startInputs)
        {
            this.Ash = new Hero(startInputs, this);

            this.Humans = new Dictionary<int, Human>(startInputs.HumanCount);
            foreach (HumanInputs hi in startInputs.HumansInputs)
            {
                this.AddHuman(hi);
            }

            this.Zombies = new Dictionary<int, Zombie>(startInputs.ZombieCount);
            foreach (ZombieInputs zi in startInputs.ZombieInputs)
            {
                this.AddZombie(zi);
            }

            this._nextZombiesBarycentre = Position.UndefinedPos;
            this._humansDoomed = new HashSet<int>(startInputs.HumanCount);
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this.NewTurnStarted?.Invoke(this, EventArgs.Empty);

            this.UpdateAshPos(newTurnInputs.X, newTurnInputs.Y);

            this.UpdateDeadHumans(newTurnInputs.HumanCount, newTurnInputs.HumansInputs);

            this.UpdateDeadZombies(newTurnInputs.ZombieCount, newTurnInputs.ZombieInputs);
            foreach(ZombieInputs zi in newTurnInputs.ZombieInputs)
            {
                this.Zombies[zi.Id].UpdateFromNewInputs(zi);
            }

            this._nextZombiesBarycentre = Position.UndefinedPos;
            this._humansDoomed.Clear();
            this._humansDoomedIsSet = false;
        }

        /// Convert current Player to Inputs (mainly for unit tests purposes)
        public Inputs ToInputs()
        {
            Inputs result = new Inputs(
                this.Ash.Pos.X,
                this.Ash.Pos.Y,
                this.Humans.Values.Select(h => h.ToHumanInputs()).ToList(),
                this.Zombies.Values.Select(z => z.ToZombieInputs()).ToList()
            );

            return result;
        }

        public bool IsHumanDoomed(int humanId)
        {
            if (!this._humansDoomedIsSet)
            {
                this.SetHumansDoomed();
            }

            return this._humansDoomed.Contains(humanId);
        }

        public bool AllHumanDoomed()
        {
            return this.Humans.Keys.All(humanId => this.IsHumanDoomed(humanId));
        }

        public Position GetNextHeroTarget()
        {
            Position target = this.GetNextZombiesBarycentre();
            Inputs nextInputs = this.SimulateNextMove(target);
            Player playerNextTurn = new Player(nextInputs);
            
            if (playerNextTurn.AllHumanDoomed())
            {
                Human humanToProtect = this.Humans.Values
                    .Where(h => !this.IsHumanDoomed(h.Id))
                    .OrderBy(h => this.Ash.GetTurnsToGetInRangeToHuman(h))
                    .FirstOrDefault();
                if (humanToProtect != null)
                {
                    return humanToProtect.Pos;
                }
            }

            return target;
        }

        /// <summary>
        /// Gets the barycentre of next positions of zombies
        /// </summary>
        /// <remarks>
        /// Lazy getter. 
        /// !!! 
        /// Private field _nextZombiesBarycentre must be set to 
        /// Position.UndefinedPos at each new turn
        /// !!!
        /// </remarks>
        /// <returns>the barycentre of next positions of zombies</returns>
        public Position GetNextZombiesBarycentre()
        {
            if (this._nextZombiesBarycentre.Equals(Position.UndefinedPos))
            {
                this._nextZombiesBarycentre = Position.FindBarycentre(
                    this.Zombies.Values.Select(z => z.GetNextPosition(this.Ash, this.Humans.Values)));
            }

            return this._nextZombiesBarycentre;
        }

        private Inputs SimulateNextMove(Position target)
        {
            // inputs from present states
            Inputs result = this.ToInputs();

            // update hero pos in inputs with given target
            Position nextHeroPos = this.Ash.ComputeNextPos(target);
            result.X = nextHeroPos.X;
            result.Y = nextHeroPos.Y;

            // update zombies positions
            result.ZombieInputs.Clear();
            result.ZombieCount = 0;
            foreach(Zombie z in this.Zombies.Values)
            {
                Position nextZombiePos = z.GetNextPosition(this.Ash, this.Humans.Values);
                result.AddZombieInputs(
                    z.Id, 
                    nextZombiePos.X,
                    nextZombiePos.Y,
                    Position.UndefinedPos.X,
                    Position.UndefinedPos.Y
                    );
            }

            return result;
        }

        private void AddHuman(HumanInputs hi)
        {
            this.Humans.Add(hi.Id, new Human(hi, this));
        }

        private void AddZombie(ZombieInputs zi)
        {
            this.Zombies.Add(zi.Id, new Zombie(zi, this));
        }

        private void UpdateAshPos(int x, int y)
        {
            this.Ash.UpdatePosition(x, y);
        }

        private void UpdateDeadHumans(int newHumanCount, IList<HumanInputs> humansInputs)
        {
            if (newHumanCount > this.Humans.Count)
            {
                throw new InvalidOperationException("More human alive in input than in internal State.");
            }

            if (newHumanCount < this.Humans.Count)
            {
                IEnumerable<int> deadHumans = this.Humans.Keys.Except(humansInputs.Select(hi => hi.Id));
                foreach (int id in deadHumans)
                {
                    this.Humans.Remove(id);
                }
            }
        }

        private void UpdateDeadZombies(int newZombieCount, IList<ZombieInputs> zombiesInputs)
        {
            if (newZombieCount > this.Zombies.Count)
            {
                throw new InvalidOperationException("More zombies alive in input than in internal State.");
            }

            if (newZombieCount < this.Zombies.Count)
            {
                IEnumerable<int> deadZombies = this.Zombies.Keys.Except(zombiesInputs.Select(zi => zi.Id));
                foreach (int id in deadZombies)
                {
                    this.Zombies.Remove(id);
                }
            }
        }

        private void SetHumansDoomed()
        {
            this._humansDoomed.Clear();

            foreach(Zombie zombie in this.Zombies.Values
                .Where(z => !z.GetNextTargetIsHero(this.Ash, this.Humans.Values)))
            {
                Human targetedHuman = zombie.GetNearestHuman(this.Humans.Values);
                int turnsToReachTarget = zombie.GetTurnsToNearestHuman(this.Humans.Values);
                int turnsToBeCoveredByHero = this.Ash.GetTurnsToGetInRangeToHuman(targetedHuman);

                if (turnsToReachTarget < turnsToBeCoveredByHero)
                {
                    this._humansDoomed.Add(targetedHuman.Id);
                }
            }

            this._humansDoomedIsSet = true;
        }
    }
}