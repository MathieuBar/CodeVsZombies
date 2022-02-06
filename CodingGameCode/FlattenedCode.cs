using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Save humans, destroy zombies!
 **/
    class Program
    {
        static void Main()        
        {
            string[] inputs;
            Player player = null;

            Inputs allInputs = new Inputs();
            bool firstLoop = true;

            // game loop
            while (true)
            {
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
                }
                else
                {
                    firstLoop = false;
                    player.UpdateFromNewInputs(allInputs);
                }


                // get hero target choice from player
                Position target = player.GetNextHeroTarget();
                Console.WriteLine($"{target.X} {target.Y}"); // Your destination coordinates

            }
        }
    }

    public abstract class Character
    {
        public int Id {get; private set;}
        public Position Pos {get; private set;}
        public int Speed {get; protected set;}
        protected Player Owner { get; private set; }

        public Character(int id, int xPos, int yPos, Player owner = null)
        {
            this.Id = id;
            this.Pos = new Position(xPos, yPos);
            this.Owner = owner;

            if (owner != null)
            {
                owner.NewTurnStarted += OnNewTurnStarted;
            }
        }

        public void UpdatePosition(Position pos)
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

    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int ShootRange = 2000;
        private const int _heroSpeed = 1000;

        private Dictionary<int, int> _turnsToGetInRangeToHuman;

        public Hero(int id, int xPos, int yPos, Player owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Hero._heroSpeed;
            this._turnsToGetInRangeToHuman = new Dictionary<int, int>();
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
            this.UpdatePosition(Position.UndefinedPos);
        }
    }

    public class Human : Character
    {   public Human(int id, int xPos, int yPos, Player owner = null):
            base(id, xPos, yPos, owner)
        {
            this.Speed = 0;
        }

        public Human(HumanInputs hi, Player owner = null): this(hi.Id, hi.X, hi.Y, owner)
        {
            // nothing to add
        }

        public HumanInputs ToHumanInputs()
        {
            return new HumanInputs(this.Id, this.Pos.X, this.Pos.Y);
        }
    }

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


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos, Player owner = null): 
            base(id, xPos, yPos, owner)
        {
            this.Speed = Zombie._zombieSpeed;
            this.ClearComputedData();
            this._givenNextPosition = new Position(nextXPos, nextYPos);
        }

        public Zombie(int id, int xPos, int yPos, Player owner = null): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y, owner)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi, Player owner = null): 
            this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext, owner)
        {
            // nothing to add
        }

        public ZombieInputs ToZombieInputs()
        {
            return new ZombieInputs(
                this.Id, this.Pos.X, this.Pos.Y, this._givenNextPosition.X, this._givenNextPosition.Y);
        }


        public void UpdateFromNewInputs(ZombieInputs newTurnZombieInputs)
        {
            // update positions
            this.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
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
            this.UpdatePosition(Position.UndefinedPos);
            this._givenNextPosition = Position.UndefinedPos;
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

        public override string ToString()
        {
            return $"{Id} {X} {Y} {XNext} {YNext}";
        }
    }
