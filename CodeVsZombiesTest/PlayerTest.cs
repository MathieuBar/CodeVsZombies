using System;
using System.Collections.Generic;
using System.Linq;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        public void Create_ValidInputs_PlayerWellInitialized()
        {
            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1500, 0), // 2 turns to be in hero range
                    new HumanInputs(1, 7500, 0), // 1 turn to be in hero range
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 1100, 0, 1500, 0), // one turn from human 0
                    new ZombieInputs(1, 7900, 0, 7500, 0), // one turn from human 1                
                }
            );
            Player p = new Player(inputs);
            

            Assert.AreEqual(p.ToInputs().ToString(), inputs.ToString());
        }


        [TestMethod]
        public void Create_TwoZombies_GoodZombiesBarycentre()
        {
            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1500, 0), // 2 turns to be in hero range
                    new HumanInputs(1, 7500, 0), // 1 turn to be in hero range
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 1100, 0, 1500, 0), // one turn from human 0
                    new ZombieInputs(1, 7900, 0, 7500, 0), // one turn from human 1                
                }
            );
            double expectedX = (1500+7500) / 2.0; // mean of zombies next X positions
            double expectedY = 0; // mean of zombies next Y positions
            Position expectedBarycentre = new Position(expectedX, expectedY);

            Player p = new Player(inputs);

            Position res = p.NextZombiesBarycentre;
            Assert.AreEqual(expectedBarycentre.X, res.X);
            Assert.AreEqual(expectedBarycentre.Y, res.Y);
        }

        [TestMethod]
        public void UpdateFromNewInputs_MovedZombies_UpdatedBarycentre()
        {
            // Create initial inputs and init player
            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1500, 0),
                    new HumanInputs(1, 7500, 1000),
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 1100, 0, 1500, 0), // one turn from human 0
                    new ZombieInputs(1, 8000, 1000, 7600, 1000), // one turn from human 1                
                }
            );
            Player p = new Player(inputs);

            // update zombie inputs to move the two zombies
            inputs.ZombieInputs[0] = new ZombieInputs(0, 1500, 0, 1894, 65);
            inputs.ZombieInputs[1] = new ZombieInputs(1, 7600, 1000, 7500, 1000);

            // expected barycentre
            double expectedX = (7500 + 1894) / 2.0;
            double expectedY = (1000 + 65) / 2.0;
            Position expectedBarycentre = new Position(expectedX, expectedY);

            // begin of test case : update player with new inputs
            p.UpdateFromNewInputs(inputs);

            // check result
            Position res = p.NextZombiesBarycentre;
            Assert.AreEqual(expectedBarycentre.X, res.X);
            Assert.AreEqual(expectedBarycentre.Y, res.Y);
        }

        [TestMethod]
        public void GetNextHeroTarget_OneDoomedHuman_DoomedFlagSet()
        {
            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1500, 0), // 2 turns to be in hero range
                    new HumanInputs(1, 7500, 0), // 1 turn to be in hero range
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 1100, 0, 1500, 0), // one turn from human 0
                    new ZombieInputs(1, 7900, 0, 7500, 0), // one turn from human 1                
                }
            );
            Player p = new Player(inputs);

            p.GetNextHeroTarget();

            Assert.IsTrue(p.IsHumanDoomed(0));
            Assert.IsFalse(p.IsHumanDoomed(1));
        }
    }
}
