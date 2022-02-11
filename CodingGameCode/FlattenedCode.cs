using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

    class Program
    {
        const long maxResponseDelayInMilliSeconds = 100;

        static void Main()        
        {
            string[] inputs;
            Player player = null;

            Inputs allInputs = new Inputs();
            bool firstLoop = true;

            Stopwatch stopwatch = new Stopwatch();

            // game loop
            while (true)
            {
                stopwatch.Reset();
                allInputs.Reset();

                // read Ash position
                inputs = Console.ReadLine().Split(' ');
                allInputs.X = int.Parse(inputs[0]);
                allInputs.Y = int.Parse(inputs[1]);

                // read humans count and positions
                allInputs.HumanCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < allInputs.HumanCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int humanId = int.Parse(inputs[0]);
                    int humanX = int.Parse(inputs[1]);
                    int humanY = int.Parse(inputs[2]);
                    allInputs.AddHumanInputs(humanId, humanX, humanY);
                }

                // read zombies count and positions
                allInputs.ZombieCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < allInputs.ZombieCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int zombieId = int.Parse(inputs[0]);
                    int zombieX = int.Parse(inputs[1]);
                    int zombieY = int.Parse(inputs[2]);
                    int zombieXNext = int.Parse(inputs[3]);
                    int zombieYNext = int.Parse(inputs[4]);
                    allInputs.AddZombieInputs(zombieId, zombieX, zombieY, zombieXNext, zombieYNext);
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Console.Error.WriteLine(allInputs);

                // create or update player infos
                if (firstLoop)
                {
                    player = new Player(allInputs);
                    firstLoop = false;
                }
                else
                {
                    player.UpdateFromNewInputs(allInputs);
                }


                // get hero target choice from player
                int maxDelay = (int)(maxResponseDelayInMilliSeconds - stopwatch.ElapsedMilliseconds - 1);
                Position target = player.GetNextHeroTarget(maxDelay);
                Console.WriteLine($"{target.X} {target.Y}"); // Your destination coordinates

            }
        }
    }

    public abstract class Character
    {
        public int Id {get; private set;}
        public Position Pos {get; private set;}
        public int Speed {get; protected set;}
        protected IStateChangedEventSender Owner { get; private set; }

        public Character(int id, int xPos, int yPos, IStateChangedEventSender owner = null)
        {
            this.Id = id;
            this.Pos = new Position(xPos, yPos);
            this.Owner = owner;

            if (owner != null)
            {
                owner.StateChanged += OnNewTurnStarted;
            }
        }

        public virtual void UpdatePosition(Position pos)
        {
            this.Pos = pos;
        }

        public void UpdatePosition(int x, int y)
            => this.UpdatePosition(new Position(x, y));

        public Position ComputeNextPos(Position targetPos)
        {
            return this.ComputeNextPos(this.Pos, targetPos);
        }

        public Position ComputeNextPos(Position sourcePos, Position targetPos)
        {
            if (this.Speed == 0) return sourcePos;

            if (sourcePos.X < 0 || sourcePos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.X < 0 || targetPos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPos), $"targetPos should not contains negative coordinates (values : {targetPos})");
            }
            
            if (sourcePos.Equals(targetPos)) return targetPos;

            double distToTarget = sourcePos.DistanceTo(targetPos);
            int deltaX = targetPos.X - sourcePos.X;
            int deltaY = targetPos.Y - sourcePos.Y;
            double realNextX = sourcePos.X + this.Speed*deltaX / distToTarget;
            double realNextY = sourcePos.Y + this.Speed*deltaY / distToTarget;

            Position result = new Position(realNextX, realNextY);

            // if target reached, return target pos
            if (Math.Abs(result.X - sourcePos.X) >= Math.Abs(deltaX)
                && Math.Abs(result.Y - sourcePos.Y) >= Math.Abs(deltaY))
            {
                return targetPos;
            }

            return result;
        }

        public int TurnsToBeInRange(Position targetPos, int range)
            => this.TurnsToBeInRange(this.Pos, targetPos, range);

        public int TurnsToBeInRange(Position sourcePos, Position targetPos, int range)
        {
            if (sourcePos.X < 0 || sourcePos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePos), $"sourcePos should not contains negative coordinates (values : {targetPos})");
            }
            if (targetPos.X < 0 || targetPos.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPos), $"targetPos should not contains negative coordinates (values : {targetPos})");
            }

            if (this.Speed == 0) return -1;

            int result = 0;
            while (sourcePos.DistanceTo(targetPos) > range)
            {
                sourcePos = this.ComputeNextPos(sourcePos, targetPos);
                result += 1;
            }

            return result;
        }

        protected virtual void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            // Nothing to do in Character class. To be implemented in derived classes.
        }
    }


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
            (int fibA, int fibB) = (1, 1);
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

    public class GameState
    {
        public Inputs Inputs { get; }
        public int Score { get; }

        public bool Equals(GameState other)
        {
            return this.Score == other.Score && this.Inputs.Equals(other.Inputs);

        }

        public GameState(Inputs inputs, int score)
        {
            this.Inputs = inputs;
            this.Score = score;
        }

        public override string ToString()
        {
            return this.Inputs.ToString() + $"Score : {this.Score}{Environment.NewLine})";
        }
    }

    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int ShootRange = 2000;
        private const int _heroSpeed = 1000;

        private Dictionary<int, int> _turnsToGetInRangeToHuman;

        public Hero(int id, int xPos, int yPos, IStateChangedEventSender owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Hero._heroSpeed;
            this._turnsToGetInRangeToHuman = new Dictionary<int, int>();
        }

        public Hero(int xPos, int yPos, IStateChangedEventSender owner = null): 
            this(DefaultHeroId, xPos, yPos, owner)
        {
            // nothing to add
        }

        public Hero(Inputs inputs, IStateChangedEventSender owner = null): this(inputs.X, inputs.Y, owner)
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
        }
    }


    public class Human : Character
    {   public Human(int id, int xPos, int yPos, IStateChangedEventSender owner = null):
            base(id, xPos, yPos, owner)
        {
            this.Speed = 0;
        }

        public Human(HumanInputs hi, IStateChangedEventSender owner = null): this(hi.Id, hi.X, hi.Y, owner)
        {
            // nothing to add
        }

        public HumanInputs ToHumanInputs()
        {
            return new HumanInputs(this.Id, this.Pos.X, this.Pos.Y);
        }
    }

    public struct HumanInputs
    {
        public int Id { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public HumanInputs(int id, int x, int y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
        }

        public bool Equals(HumanInputs other)
        {
            return this.Id == other.Id && this.X == other.X && this.Y == other.Y;
        }

        public override string ToString()
        {
            return $"{Id} {X} {Y}";
        }
    }

    public class Inputs
    {
        public int X {get; set;}
        public int Y { get; set; }
        public int HumanCount { get; set; }
        public IList<HumanInputs> HumansInputs { get; private set; }

        public int ZombieCount { get; set; }
        public IList<ZombieInputs> ZombieInputs { get; private set; }

        public Inputs()
        {
            this.HumansInputs = new List<HumanInputs>();
            this.ZombieInputs = new List<ZombieInputs>();
            this.Reset();
        }

        public Inputs(int x, int y, IList<HumanInputs> humanInputs, IList<ZombieInputs> zombieInputs)
        {
            this.X = x;
            this.Y = y;
            this.HumanCount = humanInputs.Count;
            this.HumansInputs = humanInputs;
            this.ZombieCount = zombieInputs.Count;
            this.ZombieInputs = zombieInputs;
        }

        public void Reset()
        {
            this.X = -1;
            this.Y = -1;
            this.HumanCount = 0;
            this.HumansInputs.Clear();
            this.ZombieCount = 0;
            this.ZombieInputs.Clear();
        }

        public void AddHumanInputs(int id, int x, int y)
        {
            this.HumansInputs.Add(new HumanInputs(id, x, y));
        }

        public void AddZombieInputs(int id, int x, int y, int nextX, int nextY)
        {
            this.ZombieInputs.Add(new ZombieInputs(id, x, y, nextX, nextY));
        }

        public bool Equals(Inputs other)
        {
            bool result = true;

            if (this.X != other.X 
                || this.Y != other.Y 
                || this.HumanCount != other.HumanCount 
                || this.ZombieCount != other.ZombieCount
                || this.HumansInputs.Count != other.HumansInputs.Count
                || this.ZombieInputs.Count != other.ZombieInputs.Count)
            {
                return false;
            }

            foreach((HumanInputs thisHi, HumanInputs otherHi) in
                this.HumansInputs.OrderBy(hi => hi.Id).Zip(other.HumansInputs.OrderBy(hi => hi.Id)))
            {
                if (!thisHi.Equals(otherHi))
                {
                    return false;
                }
            }
            
            foreach((ZombieInputs thisZi, ZombieInputs otherZi) in
                this.ZombieInputs.OrderBy(hi => hi.Id).Zip(other.ZombieInputs.OrderBy(hi => hi.Id)))
            {
                if (!thisZi.Equals(otherZi))
                {
                    return false;
                }
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Ash position : {this.X} {this.Y}{Environment.NewLine}");
            sb.Append($"Human count : {this.HumanCount}{Environment.NewLine}");
            for(int i = 0; i < this.HumansInputs.Count; i++)
            {
                sb.Append($"human n° {i} : {this.HumansInputs[i]}{Environment.NewLine}");
            }
            sb.Append($"Zombie count : {this.ZombieCount}{Environment.NewLine}");
            for(int i = 0; i < this.ZombieInputs.Count; i++)
            {
                sb.Append($"zombie n° {i} : {this.ZombieInputs[i]}{Environment.NewLine}");
            }

            return sb.ToString();
        }
    }

    public interface IStateChangedEventSender
    {
        public event EventHandler StateChanged;
    }

    public class Player
    {
        private const int UndefinedZombieTarget = -1;

        public int BestSimulScore { get; private set; }
        public IEnumerable<Position> BestSimulTargetHistory => this._bestSimulTargetHistory.ToArray();

        private Game _curGame;
        private Inputs _lastInputs;
        private readonly Random _randomGen = new Random();
        private int _curZombieTarget = UndefinedZombieTarget;
        private Queue<Position> _lastSimulTargetHistory = new Queue<Position>(100);
        private Queue<Position> _bestSimulTargetHistory = new Queue<Position>(100);
        private Stopwatch _stopwatch = new Stopwatch();

        public Player(Inputs startInputs)
        {
            this._lastInputs = startInputs;
            this._curGame = new Game(startInputs);
            this.BestSimulScore = 0;
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this._lastInputs = newTurnInputs;
            this._curGame.UpdateFromNewInputs(newTurnInputs);
            this._curZombieTarget = UndefinedZombieTarget;
        }

        /// <summary>
        /// Gets next hero target to be given to coding game program, at each turn
        /// </summary>
        /// <returns>Next hero target for the "real" game</returns>
        public Position GetNextHeroTarget(int maxTimeInMilliSeconds)
        {
            if (this.BestSimulScore == 0)
            {
                this.SimulateManyGamesWithRandomZombieStrat(maxTimeInMilliSeconds);
            }

            if (this._bestSimulTargetHistory.Count > 0)
            {
                return this._bestSimulTargetHistory.Dequeue();
            }

            if (this._lastSimulTargetHistory.Count > 0)
            {
                return this._lastSimulTargetHistory.Dequeue();
            }

            return new Position(0, 0);
        }

        public int SimulateManyGamesWithRandomZombieStrat(int maxTimeInMilliSeconds)
        {
            const int marginInMilliSeconds = 5;
            int numberOfGameSimulated = 0;

            this._stopwatch.Reset();
            this._stopwatch.Start();

            this._lastSimulTargetHistory.Clear();

            while (this._stopwatch.ElapsedMilliseconds < maxTimeInMilliSeconds - marginInMilliSeconds)
            {
                Position targetPos = this.ComputeNextHeroTargetRandomZombieStrat();
                this._lastSimulTargetHistory.Enqueue(targetPos);
                bool endGame = this._curGame.UpdateByNewTurnSimulation(targetPos);

                if (endGame)
                {
                    if (this._curGame.Score > this.BestSimulScore)
                    {
                        this.BestSimulScore = this._curGame.Score;
                        (this._bestSimulTargetHistory, this._lastSimulTargetHistory) = (this._lastSimulTargetHistory, this._bestSimulTargetHistory);
                    }

                    this._curGame.InitFromInputs(this._lastInputs);
                    this._curZombieTarget = UndefinedZombieTarget;
                    this._lastSimulTargetHistory.Clear();
                    numberOfGameSimulated += 1;
                }
            }

            return numberOfGameSimulated;
        }

        /// <summary>
        /// Return a target for the hero, by selecting a random zombie still in 
        /// game, heading toward its next pos while he is not dead, and then 
        /// chosing another random zombie.
        /// </summary>
        /// <returns>The suggested target pos for hero for next turn</returns>
        private Position ComputeNextHeroTargetRandomZombieStrat()
        {
            if (this._curZombieTarget == UndefinedZombieTarget 
                || !this._curGame.IsZombieAlive(this._curZombieTarget))
            {
                this._curZombieTarget = this.SelectRandomZombieAsTarget();
            }

            return this._curGame.GetZombieNextPosition(this._curZombieTarget);
        }

        private int SelectRandomZombieAsTarget()
        {
            int[] zombiesId = this._curGame.GetZombiesAliveIds();
            int rndIdx = _randomGen.Next(zombiesId.Length);
            return zombiesId[rndIdx];
        }
    }


    public struct Position
    {
        public int X;
        public int Y;

        public static Position UndefinedPos => new Position(-1, -1);
        
        public static Position FindBarycentre(IEnumerable<Position> positions)
        {
            if (positions is null) throw new ArgumentNullException(nameof(positions));
            if (!positions.Any()) 
            {
                throw new ArgumentOutOfRangeException(nameof(positions), $"No position given to compute barycentre");
            }

            int count = 0;
            double x = 0;
            double y = 0;

            foreach(Position pos in positions)
            {
                count += 1;
                x += pos.X;
                y += pos.Y;
            }

            return new Position(x/count, y/count);
        }

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Position(double x, double y)
        {
            this.X = Position.RoundCoordinate(x);
            this.Y = Position.RoundCoordinate(y);            
        }

        public double DistanceTo(Position pos)
        {
            // Position (-1, -1) means character is dead => return float.NaN as distance
            if (X < 0 || Y < 0 || pos.X < 0 || pos.Y < 0)
            {
                return double.NaN;
            }

            return Math.Sqrt(
                Math.Pow(pos.X-this.X, 2) + Math.Pow(pos.Y-this.Y, 2));
        }

        public bool Equals(Position pos)
        {
            return this.X == pos.X && this.Y == pos.Y;
        }

        private static int RoundCoordinate(double realCoordinate) 
            => (int)Math.Floor(realCoordinate);
    }


    public class Zombie: Character
    {
        public const int UndefinedTurnsToNearestHuman = -1;
        private const int _zombieSpeed = 400;

        private Human _nearestHuman;
        private double _distToNearestHuman;
        private int _turnsToNearestHuman;
        private bool? _nextTargetIsHero;
        private Position _givenNextPosition;
        private Position _computedNextPosition;


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos, IStateChangedEventSender owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Zombie._zombieSpeed;
            this.ClearComputedData();
            this._givenNextPosition = new Position(nextXPos, nextYPos);
        }

        public Zombie(int id, int xPos, int yPos, IStateChangedEventSender owner = null): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y, owner)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi, IStateChangedEventSender owner = null): 
            this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext, owner)
        {
            // nothing to add
        }

        public ZombieInputs ToZombieInputs()
        {
            return new ZombieInputs(
                this.Id, this.Pos.X, this.Pos.Y, this._givenNextPosition.X, this._givenNextPosition.Y);
        }

        public override void UpdatePosition(Position pos)
        {
            base.UpdatePosition(pos);
            this._givenNextPosition = Position.UndefinedPos;
        }

        public void UpdateFromNewInputs(ZombieInputs newTurnZombieInputs)
        {
            // update positions
            base.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
        }

        /// <summary>
        /// Update this zombie position by simulating next move for a new turn.
        /// </summary>
        /// <param name="hero">hero of the game</param>
        /// <param name="humans">humans of the game</param>
        public void UpdateByNewTurnSimulation(Hero hero, IEnumerable<Human> humans)
        {
            this.UpdatePosition(this.GetNextPosition(hero, humans));
        }

        public Human GetNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._nearestHuman != null)
            {
                return this._nearestHuman;
            }

            Human nearestHuman = null;
            double distToNearestHuman = double.MaxValue;
            foreach (Human human in humans)
            {
                double distToHuman = human.Pos.DistanceTo(this.Pos);
                if (distToHuman < distToNearestHuman
                    || (distToHuman == distToNearestHuman && human.Id < nearestHuman.Id))
                {
                    distToNearestHuman = distToHuman;
                    nearestHuman = human;
                }
            }

            this._distToNearestHuman = distToNearestHuman;
            this._nearestHuman = nearestHuman;
            return this._nearestHuman;
        }

        public double GetDistToNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (!double.IsNaN(this._distToNearestHuman))
            {
                return this._distToNearestHuman;
            }

            Human nearestHuman = this.GetNearestHuman(humans);
            this._distToNearestHuman = nearestHuman.Pos.DistanceTo(this.Pos);
            return this._distToNearestHuman;
        }

        public int GetTurnsToNearestHuman(IEnumerable<Human> humans)
        {
            if (humans is null) throw new ArgumentNullException(nameof(humans));            
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._turnsToNearestHuman != UndefinedTurnsToNearestHuman)
            {
                return this._turnsToNearestHuman;
            }

            Human nearestHuman = this.GetNearestHuman(humans);
            this._turnsToNearestHuman = this.TurnsToBeInRange(nearestHuman.Pos, 0);
            return this._turnsToNearestHuman;
        }

        public bool GetNextTargetIsHero(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            if (this._nextTargetIsHero.HasValue)
            {
                return this._nextTargetIsHero.Value;
            }

            double distToHero = hero.Pos.DistanceTo(this.Pos);
            double distToNearestHuman = this.GetDistToNearestHuman(humans);

            this._nextTargetIsHero = distToHero < distToNearestHuman;
            return this._nextTargetIsHero.Value;
        }

        public Position GetNextPosition(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            // Next position has been given => return given next position
            if (!this._givenNextPosition.Equals(Position.UndefinedPos))
            {
                return this._givenNextPosition;
            }   

            // No next position given => if next position already computed return it
            if (!this._computedNextPosition.Equals(Position.UndefinedPos))
            {
                return _computedNextPosition;
            }

            // No next position given nor computed => compute next pos
            bool nextTargetIsHero = this.GetNextTargetIsHero(hero, humans);
            Position targetPos = nextTargetIsHero ? hero.Pos : this.GetNearestHuman(humans).Pos;
            this._computedNextPosition = this.ComputeNextPos(targetPos);
            return this._computedNextPosition;
        }

        protected override void OnNewTurnStarted(object sender, EventArgs eventArgs)
        {
            this.ClearComputedData();
        }

        private void ClearComputedData()
        {
            this._nearestHuman = null;
            this._distToNearestHuman = double.NaN;
            this._turnsToNearestHuman = UndefinedTurnsToNearestHuman;
            this._nextTargetIsHero = null;
            this._computedNextPosition = Position.UndefinedPos;
        }
    }

    public struct ZombieInputs
    {
        public int Id { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int XNext { get; private set; }
        public int YNext { get; private set; }

        public ZombieInputs(int id, int x, int y, int xNext, int yNext)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.XNext = xNext;
            this.YNext = yNext;
        }

        public bool Equals(ZombieInputs other)
        {
            return
                this.Id == other.Id
                && this.X == other.X
                && this.Y == other.Y
                && (this.XNext == -1 || other.XNext == -1 || this.XNext == other.XNext)
                && (this.YNext == -1 || other.YNext == -1 || this.YNext == other.YNext);
        }

        public override string ToString()
        {
            return $"{Id} {X} {Y} {XNext} {YNext}";
        }
    }
