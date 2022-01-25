using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.FileIO;

namespace CodeVsZombies
{
class Program
{
    static void Main()
    {
        string[] inputs;
        Game game = new Game();
        bool firstLoop = true;
        ISet<int> humansAlive = new HashSet<int>();
        ISet<int> zombiesAlive = new HashSet<int>();

        // game loop
        while (true)
        {
            // read Ash position
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            Console.Error.WriteLine($"Ash position : {x} {y}");
            game.UpdateAshPos(x, y);

            // read humans count and positions
            humansAlive.Clear();
            int humanCount = int.Parse(Console.ReadLine());
            Console.Error.WriteLine($"Human count : {humanCount}");
            for (int i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                Console.Error.WriteLine($"human n° {i} : {string.Join(' ', inputs)}");
                int humanId = int.Parse(inputs[0]);
                if (firstLoop)
                {
                    int humanX = int.Parse(inputs[1]);
                    int humanY = int.Parse(inputs[2]);
                    game.AddHuman(humanId, humanX, humanY);
                }
                humansAlive.Add(humanId);
            }
            game.UpdateDeadHumans(humansAlive);

            // read zombies count and positions
            zombiesAlive.Clear();
            int zombieCount = int.Parse(Console.ReadLine());
            Console.Error.WriteLine($"Zombie count : {zombieCount}");
            for (int i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                Console.Error.WriteLine($"zombie n° {i} : {string.Join(' ', inputs)}");
                int zombieId = int.Parse(inputs[0]);
                int zombieX = int.Parse(inputs[1]);
                int zombieY = int.Parse(inputs[2]);
                int zombieXNext = int.Parse(inputs[3]);
                int zombieYNext = int.Parse(inputs[4]);
                if (firstLoop)
                {
                    game.AddZombie(zombieId, zombieX, zombieY, zombieXNext, zombieYNext);
                }
                else
                {
                    game.UpdateNextZombiePos(i, zombieXNext, zombieYNext);
                }
                zombiesAlive.Add(zombieId);
            }
            game.UpdateDeadZombies(zombiesAlive);

            // update distances and zombies targets
            game.UpdateZombiesTargets();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Position target = game.GetNextHeroTarget();
            Console.WriteLine($"{target.x} {target.y}"); // Your destination coordinates

        }
    }
}

public class Game
{
    private Hero Ash { get; set; }
    private Dictionary<int, Human> Humans { get; set; }
    private Dictionary<int, Zombie> Zombies { get; set; }


    public Game()
    {
        this.Ash = new Hero(-1, -1, -1);
        this.Humans = new Dictionary<int, Human>();
        this.Zombies = new Dictionary<int, Zombie>();
    }

    public void UpdateAshPos(int x, int y)
    {
        this.Ash.UpdatePosition(x, y);
    }

    public void AddHuman(int id, int x, int y)
    {
        this.Humans.Add(id, new Human(id, x, y));
    }

    public void UpdateDeadHumans(ISet<int> humansAlive)
    {
        if (humansAlive.Count > this.Humans.Count)
        {
            throw new InvalidOperationException("More human alive in input than in internal State.");
        }

        if (humansAlive.Count < this.Humans.Count)
        {
            var deadHumans = this.Humans.Keys.Except(humansAlive);
            foreach (int id in deadHumans)
            {
                this.Humans.Remove(id);
            }
        }
    }

    public void AddZombie(int id, int x, int y, int nextX, int nextY)
    {
        this.Zombies.Add(id, new Zombie(id, x, y, nextX, nextY));
    }

    public void UpdateNextZombiePos(int idx, int x, int y)
    {
        this.Zombies[idx].UpdateNextPosition(x, y);
    }

    public void UpdateDeadZombies(ISet<int> zombiesAlive)
    {
        if (zombiesAlive.Count > this.Zombies.Count)
        {
            throw new InvalidOperationException("More zombies alive in input than in internal State.");
        }

        if (zombiesAlive.Count < this.Zombies.Count)
        {
            var deadZombies = this.Zombies.Keys.Except(zombiesAlive);
            foreach (int id in deadZombies)
            {
                this.Zombies.Remove(id);
            }
        }
    }

    public void UpdateZombiesTargets()
    {
        foreach(Zombie zombie in this.Zombies.Values)
        {
            zombie.UpdateTarget(this.Ash, this.Humans.Values);
        }
    }

    public Position GetNextHeroTarget()
    {
        return this.Humans[0].Pos;
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

    protected int DistanceInTurns(int distance)
    {
        if (this.Speed == 0)
        {
            return -1;
        }

        return distance / this.Speed;
    }
}

public class Hero: Character
{
    private const int HeroSpeed = 1000;
    public const int ShootRange = 2000;

    public Hero(int id, int xPos, int yPos): base(id, xPos, yPos)
    {
        this.Speed = Hero.HeroSpeed;
    }
}

public class Human: Character
{
    public Human(int id, int xPos, int yPos): base(id, xPos, yPos)
    {
        this.Speed = 0;
    }
}

public class Zombie: Character
{
    private const int ZombieSpeed = 400;

    public Position NextPosition {get; private set;}
    public Human NextNearestHuman {get; private set;}
    public int NextDistanceToNearestHuman {get; private set;}
    public int TurnsToNearestHuman {get; private set;}
    public bool NextTargetIsHero {get; private set;}
        

    public Zombie(int id, int xPos, int yPos, int nextXPos, int nextYPos): 
        base(id, xPos, yPos)
    {
        this.NextPosition = new Position(nextXPos, nextYPos);
        this.Speed = Zombie.ZombieSpeed;
    }

    public void UpdateNextPosition(int nextX, int nextY)
    {
        this.UpdatePosition(this.Pos);
        this.NextPosition = new Position(nextX, nextY);
    }

    public void UpdateTarget(Hero hero, IEnumerable<Human> humans)
    {
        if (hero is null) throw new ArgumentNullException(nameof(hero));
        if (humans is null) throw new ArgumentNullException(nameof(humans));
        if (!humans.Any()) throw new InvalidOperationException("There must be at least one human");

        int distToHero = hero.Pos.DistanceTo(this.NextPosition);

        Human nearestHuman = null;
        int distToNearestHuman = int.MaxValue;
        foreach (Human human in humans)
        {
            int distToHuman = human.Pos.DistanceTo(this.NextPosition);
            if (distToHuman < distToNearestHuman)
            {
                distToNearestHuman = distToHuman;
                nearestHuman = human;
            }
        }

        this.NextNearestHuman = nearestHuman;
        this.NextDistanceToNearestHuman = distToNearestHuman;
        this.TurnsToNearestHuman = this.DistanceInTurns(distToNearestHuman) + 1;
        this.NextTargetIsHero = distToHero < distToNearestHuman;
    }
}

public struct Position
{
    public int x;
    public int y;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int DistanceTo(Position pos)
    {
        // Position (-1, -1) means character is dead => return -1 as distance
        if (x == -1)
        {
            return -1;
        }

        return (int)Math.Floor(Math.Sqrt(
            Math.Pow(pos.x-this.x, 2) + Math.Pow(pos.y-this.y, 2)));
    }
}
}
