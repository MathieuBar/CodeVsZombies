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

        /// <summary>
        /// Reset data to match <paramref cref="gameState"/>.
        /// </summary>
        /// <param name="gameState"></param>
        public void ResetFromGameState(GameState gameState)
        {
            this.Ash = new Hero(gameState.Inputs);

            this.Humans.Clear();
            foreach (HumanInputs hi in gameState.Inputs.HumansInputs)
            {
                this.Humans.Add(hi.Id, new Human(hi, this));
            }

            this.Zombies.Clear();
            foreach (ZombieInputs zi in gameState.Inputs.ZombieInputs)
            {
                this.Zombies.Add(zi.Id, new Zombie(zi, this));
            }

            this.Score = gameState.Score;
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this.StateChanged?.Invoke(this, EventArgs.Empty);

            this.UpdateScore(this.Zombies.Count - newTurnInputs.ZombieCount, this.Humans.Count);

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

        public GameState ToState()
        {
            return new GameState(this.ToInputs(), this.Score);
        }

        public int[] GetHumansAliveIds() => this.Humans.Keys.ToArray();

        public int[] GetZombiesAliveIds() => this.Zombies.Keys.ToArray();

        public Position GetZombieNextPosition(int zombieId)
        {
            bool zombieAlive = this.Zombies.TryGetValue(zombieId, out Zombie zombie);
            return zombieAlive ? zombie.GetNextPosition(this.Ash, this.Humans.Values) : Position.UndefinedPos;
        }

        public bool IsZombieAlive(int zombieId) => this.Zombies.ContainsKey(zombieId);

        /// <summary>
        /// Return the score earned at one turn given the number of zombies killed this turn 
        /// and the number of humans alive when those zombies have been killed.
        /// </summary>
        /// <param name="killedZombiesCount">Number of zombies killed during the turn</param>
        /// <param name="humansAliveCount">Number of humans alive **when the zombies were killed**</param>
        /// <returns>the score earned by those zombies kills</returns>
        private static int ComputeTurnScore(int killedZombiesCount, int humansAliveCount)
        {
            int score = 0;
            int scoreBase = humansAliveCount * humansAliveCount * 10;
            (int fibA, int fibB) = (1, 1);
            for (int i = 0; i < killedZombiesCount; i++)
            {
                score += scoreBase * fibB;
                (fibA, fibB) = (fibB, fibA + fibB);
            }

            return score;
        }

        private void UpdateScore(int killedZombiesCount, int previousHumanCount)
        {
            this.Score += Game.ComputeTurnScore(killedZombiesCount, previousHumanCount);
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

        /// <summary>
        /// All zombies in hero shoot range are destroyed, giving some score
        /// </summary>
        /// <returns>the score generated by the kills</returns>
        private int HeroKillZombies()
        {
            int killedZombiesCount = 0;
            foreach(int zombieId in this.Zombies.Keys)
            {
                if (this.Ash.Pos.DistanceTo(this.Zombies[zombieId].Pos) <= Hero.ShootRange)
                {
                    this.Zombies.Remove(zombieId);
                    killedZombiesCount++;
                }
            }

            return Game.ComputeTurnScore(killedZombiesCount, this.Humans.Count);
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