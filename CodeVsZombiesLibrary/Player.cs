using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Player
    {
        private Hero Ash { get; set; }
        private Dictionary<int, Human> Humans { get; set; }
        private ISet<int> HumansAlive { get; set; }
        private Dictionary<int, Zombie> Zombies { get; set; }
        private ISet<int> ZombiesAlive { get; set; }
        private Position _nextZombiesBarycentre;
        public Position NextZombiesBarycentre => this.GetNextZombiesBarycentre();

        public Player(Inputs startInputs)
        {
            this.InitFromInputs(startInputs);
        }

        public void InitFromInputs(Inputs startInputs)
        {
            this.Ash = new Hero(startInputs);

            this.Humans = new Dictionary<int, Human>(startInputs.HumanCount);
            this.HumansAlive = new HashSet<int>(startInputs.HumanCount);
            foreach (HumanInputs hi in startInputs.HumansInputs)
            {
                this.AddHuman(hi);
                this.HumansAlive.Add(hi.Id);
            }

            this.Zombies = new Dictionary<int, Zombie>(startInputs.ZombieCount);
            this.ZombiesAlive = new HashSet<int>(startInputs.ZombieCount);
            foreach (ZombieInputs zi in startInputs.ZombieInputs)
            {
                this.AddZombie(zi);
                this.ZombiesAlive.Add(zi.Id);
            }
            this._nextZombiesBarycentre = Position.UndefinedPos;
            this.Ash.UpdateDistancesToHumans(this.Humans.Values);
            this.UpdateZombiesTargets();
            this.UpdateHumansThreats();
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this.UpdateAshPos(newTurnInputs.X, newTurnInputs.Y);

            this.UpdateDeadHumans(newTurnInputs.HumanCount, newTurnInputs.HumansInputs);

            this.UpdateDeadZombies(newTurnInputs.ZombieCount, newTurnInputs.ZombieInputs);
            foreach(ZombieInputs zi in newTurnInputs.ZombieInputs)
            {
                this.Zombies[zi.Id].UpdateFromNewInputs(zi);
            }
            this._nextZombiesBarycentre = Position.UndefinedPos;
            this.Ash.UpdateDistancesToHumans(this.Humans.Values);
            this.UpdateZombiesTargets();
            this.UpdateHumansThreats();
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
            if (!this.Humans.ContainsKey(humanId))
            {
                throw new ArgumentOutOfRangeException(nameof(humanId), $"No human with id {humanId} in humans alive.");
            }
            return this.Humans[humanId].Doomed;
        }

        public bool AllHumanDoomed()
        {
            return this.Humans.All(h => h.Value.Doomed);
        }

        public Position GetNextHeroTarget()
        {
            Position target = this.NextZombiesBarycentre;
            Inputs nextInputs = this.SimulateNextMove(target);
            Player playerNextTurn = new Player(nextInputs);
            playerNextTurn.UpdateZombiesTargets();
            
            if (playerNextTurn.AllHumanDoomed())
            {
                Human humanToProtect = this.Humans.Values.FirstOrDefault(
                    h => !h.Doomed && h.ThreateningZombiesCount > 0
                );
                if (humanToProtect != null)
                {
                    return humanToProtect.Pos;
                }
            }

            return target;
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
                result.AddZombieInputs(
                    z.Id, 
                    z.NextPosition.X,
                    z.NextPosition.Y,
                    Position.UndefinedPos.X,
                    Position.UndefinedPos.Y
                    );
            }

            return result;
        }

        private void AddHuman(HumanInputs hi)
        {
            this.Humans.Add(hi.Id, new Human(hi));
        }

        private void AddZombie(ZombieInputs zi)
        {
            this.Zombies.Add(zi.Id, new Zombie(zi));
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
                this.HumansAlive.Clear();
                this.HumansAlive.UnionWith(humansInputs.Select(hi => hi.Id));
                IEnumerable<int> deadHumans = this.Humans.Keys.Except(this.HumansAlive);
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
                this.ZombiesAlive.Clear();
                this.ZombiesAlive.UnionWith(zombiesInputs.Select(zi => zi.Id));
                IEnumerable<int> deadZombies = this.Zombies.Keys.Except(this.ZombiesAlive);
                foreach (int id in deadZombies)
                {
                    this.Zombies.Remove(id);
                }
            }
        }

        private void UpdateZombiesTargets()
        {
            foreach(Zombie zombie in this.Zombies.Values)
            {
                zombie.UpdateTarget(this.Ash, this.Humans.Values);
            }
        }

        private void UpdateHumansThreats()
        {
            foreach(Human human in this.Humans.Values)
            {
                human.ClearThreateningZombies();
            }

            foreach(Zombie zombie in this.Zombies.Values.Where(z => z.NextNearestHuman != null))
            {
                Human human = zombie.NextNearestHuman;
                int turnsToBeCoveredByHero = this.Ash.GetTurnsToGetInRangeToHuman(human.Id);
                human.AddThreateningZombie(zombie, turnsToBeCoveredByHero);
            }
        }

        /// <summary>
        /// Gets the barycentre of next positions of zombies
        /// </summary>
        /// <remarks>
        /// Lazy getter. 
        /// !!! 
        /// Private field <cref name="_zombiesBarycentre"/> must be set to 
        /// <cref name="Position.UndefinedPos"/> at each change in zombies
        /// number or positions
        /// !!!
        /// </remarks>
        /// <returns>the barycentre of next positions of zombies</returns>
        /// <exception cref="NotImplementedException"></exception>
        private Position GetNextZombiesBarycentre()
        {
            if (this._nextZombiesBarycentre.Equals(Position.UndefinedPos))
            {
                this._nextZombiesBarycentre = Position.FindBarycentre(
                    this.Zombies.Values.Select(z => z.NextPosition));
            }

            return this._nextZombiesBarycentre;
        }
    }
}