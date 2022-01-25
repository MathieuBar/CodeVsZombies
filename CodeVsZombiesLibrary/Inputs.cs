using System;
using System.Collections.Generic;
using System.Text;

namespace CodeVsZombiesLibrary
{
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
}