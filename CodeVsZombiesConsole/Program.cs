using System;
using System.Collections.Generic;
using CodeVsZombiesLibrary;

namespace CodeVsZombiesConsole
{
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
}
