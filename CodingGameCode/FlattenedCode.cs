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

        public Character(int id, int xPos, int yPos)
        {
            this.Id = id;
            this.Pos = new Position(xPos, yPos);
        }

        public void UpdatePosition(Position pos)
        {
            this.Pos = pos;
        }

        public void UpdatePosition(int x, int y)
        {
            this.Pos = new Position(x, y);
        }

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
    }

    public class Hero : Character
    {
        public const int DefaultHeroId = -1;
        public const int _shootRange = 2000;
        private const int _heroSpeed = 1000;
        
        private Dictionary<int, int> TurnsToGetInRangeToHuman { get; set; }

        public Hero(int id, int xPos, int yPos): base(id, xPos, yPos)
        {
            this.Speed = Hero._heroSpeed;
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
            return this.TurnsToBeInRange(targetPos, Hero._shootRange);
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

    public class Human : Character
    {
        public bool Doomed { get; set; }
        public int TurnsBeforeBeingCaught { get; private set; }
        public int ThreateningZombiesCount => this.ThreateningZombies.Count;
        private ISet<int> ThreateningZombies { get; set; }

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
        }
        
    }

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
        private const int _zombieSpeed = 400;

        public Position NextPosition => this.GetNextPosition();
        public Human NextNearestHuman {get; private set;}
        public int TurnsToNearestHuman {get; private set;}
        public bool NextTargetIsHero {get; private set;}
        
        private Position _givenNextPosition;
        private Position _targetPos;
        private Position _computedNextPosition;


        public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos): 
            base(id, xPos, yPos)
        {
            this._givenNextPosition = new Position(nextXPos, nextYPos);
            this.Speed = Zombie._zombieSpeed;
            this.ClearStatefullData();
        }

        public Zombie(int id, int xPos, int yPos): this(
            id, xPos, yPos, Position.UndefinedPos.X, Position.UndefinedPos.Y)
        {
            // nothing to add
        }

        public Zombie(ZombieInputs zi): this(zi.Id, zi.X, zi.Y, zi.XNext, zi.YNext)
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
            this.ClearStatefullData();
            
            // update positions
            this.UpdatePosition(newTurnZombieInputs.X, newTurnZombieInputs.Y);
            this._givenNextPosition = new Position(newTurnZombieInputs.XNext, newTurnZombieInputs.YNext);
        }

        public void UpdateTarget(Hero hero, IEnumerable<Human> humans)
        {
            if (hero is null) throw new ArgumentNullException(nameof(hero));
            if (humans is null) throw new ArgumentNullException(nameof(humans));
            if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

            double distToHero = hero.Pos.DistanceTo(this.Pos);

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

            this.NextNearestHuman = nearestHuman;
            this.TurnsToNearestHuman = this.TurnsToBeInRange(this.Pos, nearestHuman.Pos, 0);
            this.NextTargetIsHero = distToHero < distToNearestHuman;
            this._targetPos = this.NextTargetIsHero ? hero.Pos : this.NextNearestHuman.Pos;
        }

        private void ClearStatefullData()
        {
            this._computedNextPosition = Position.UndefinedPos;
            this._targetPos = Position.UndefinedPos;
            this.NextNearestHuman = null;
            this.TurnsToNearestHuman = -1;
            this.NextTargetIsHero = false;
        }

        private Position GetNextPosition()
        {
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

            // No next position given nor computed => _targetPos must have been set to continue
            if (this._targetPos.Equals(Position.UndefinedPos))
            {
                throw new InvalidOperationException(
                    "Unable to give next position when the next position  " +
                    "was not given and target position is not set yet. " +
                    "Update targets first !");
            }

            // No next position given nor computed, but targetPos is known => compute next pos
            this._computedNextPosition = this.ComputeNextPos(this._targetPos);
            return this._computedNextPosition;
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
