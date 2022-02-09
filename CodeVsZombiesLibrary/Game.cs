using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Game: IStateChangedEventSender
    {
        public event EventHandler StateChanged;

        public int Score { get; private set; }

        private Hero Ash { get; set; }
        private Dictionary<int, Human> Humans { get; set; }
        private Dictionary<int, Zombie> Zombies { get; set; }
        
        public Game(Inputs startInputs)
        {
            this.InitFromInputs(startInputs);
        }

        public void InitFromInputs(Inputs startInputs)
        {
            this.Ash = new Hero(startInputs, this);

            this.Humans = new Dictionary<int, Human>(startInputs.HumanCount);
            foreach (HumanInputs hi in startInputs.HumansInputs)
            {
                this.Humans.Add(hi.Id, new Human(hi, this));
            }

            this.Zombies = new Dictionary<int, Zombie>(startInputs.ZombieCount);
            foreach (ZombieInputs zi in startInputs.ZombieInputs)
            {
                this.Zombies.Add(zi.Id, new Zombie(zi, this));
            }

            this.Score = 0;
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this.StateChanged?.Invoke(this, EventArgs.Empty);

            this.Ash.UpdatePosition(newTurnInputs.X, newTurnInputs.Y);

            this.UpdateDeadHumans(newTurnInputs.HumanCount, newTurnInputs.HumansInputs);

            this.UpdateDeadZombies(newTurnInputs.ZombieCount, newTurnInputs.ZombieInputs);
            foreach(ZombieInputs zi in newTurnInputs.ZombieInputs)
            {
                this.Zombies[zi.Id].UpdateFromNewInputs(zi);
            }
        }

        /// <summary>
        /// Update this game by simulating next turn with given next hero target
        /// </summary>
        /// <param name="nextHeroTarget">next hero target</param>
        /// <return>Game end : true if all humans are dead or all zombies are dead after this turn</return>
        public bool UpdateByNewTurnSimulation(Position nextHeroTarget)
        {
            // update zombies positions (independant from next hero target)
            foreach(Zombie z in this.Zombies.Values)
            {
                z.UpdateByNewTurnSimulation(this.Ash, this.Humans.Values);
            }

            // update hero pos in inputs with given target (independant from new zombies positions)
            this.Ash.UpdatePosition(this.Ash.ComputeNextPos(nextHeroTarget));

            // send state changed event
            this.StateChanged?.Invoke(this, EventArgs.Empty);

            // kill zombies, update score and check if all zombies are dead
            this.Score += this.HeroKillZombies();
            if (this.Zombies.Count == 0)
            {
                return true;
            }

            // kill humans and check if all humans are dead
            this.ZombiesKillHumans();
            if (this.Humans.Count == 0)
            {
                this.Score = 0;
                return true;
            }

            // send state changed event
            this.StateChanged?.Invoke(this, EventArgs.Empty);

            return false;
        }

        /// Convert current Game to Inputs (mainly for unit tests purposes)
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

        public int[] GetHumansAliveIds() => this.Humans.Keys.ToArray();

        public int[] GetZombiesAliveIds() => this.Zombies.Keys.ToArray();

        public Position GetZombieNextPosition(int zombieId)
        {
            bool zombieAlive = this.Zombies.TryGetValue(zombieId, out Zombie zombie);
            return zombieAlive ? zombie.GetNextPosition(this.Ash, this.Humans.Values) : Position.UndefinedPos;
        }

        public bool IsZombieAlive(int zombieId) => this.Zombies.ContainsKey(zombieId);

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

        /// <summary>
        /// All zombies in hero shoot range are destroyed, giving some score
        /// </summary>
        /// <returns>the score generated by the kills</returns>
        private int HeroKillZombies()
        {
            int score = 0;

            int scoreBase = this.Humans.Count * this.Humans.Count * 10;
            (int fibA, int fibB) = (2, 3);
            foreach(int zombieId in this.Zombies.Keys)
            {
                if (this.Ash.Pos.DistanceTo(this.Zombies[zombieId].Pos) <= Hero.ShootRange)
                {
                    this.Zombies.Remove(zombieId);
                    score += scoreBase * fibB;
                    (fibA, fibB) = (fibB, fibA + fibB);
                }
            }

            return score;
        }

        /// <summary>
        /// Zombies at a human pos kill this human
        /// </summary>
        private void ZombiesKillHumans()
        {
            foreach(Zombie z in this.Zombies.Values)
            {
                foreach(int humanId in this.Humans.Keys)
                {
                    if (z.Pos.Equals(this.Humans[humanId].Pos))
                    {
                        this.Humans.Remove(humanId);
                    }
                }
            }
        }
    }
}