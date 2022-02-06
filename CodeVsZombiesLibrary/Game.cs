using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Game
    {
        public event EventHandler NewTurnStarted;

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
                this.AddHuman(hi);
            }

            this.Zombies = new Dictionary<int, Zombie>(startInputs.ZombieCount);
            foreach (ZombieInputs zi in startInputs.ZombieInputs)
            {
                this.AddZombie(zi);
            }
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
    }
}