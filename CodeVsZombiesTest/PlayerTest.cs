using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
            
            Assert.AreEqual(inputs.ToString(), p.ToInputs().ToString());
        }

        [TestMethod]
        public void InitFromInputs_NewTurnStarted_EventNotSent()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Simple);
            Player p = new Player(inputs);
            bool eventReceived = false;
            EventHandler onNewTurnStarted = (sender, eventArgs) =>
                eventReceived = true;
            p.NewTurnStarted += onNewTurnStarted;

            p.InitFromInputs(inputs);

            Assert.IsFalse(eventReceived);
        }

        [TestMethod]
        public void UpdateFromNewInputs_NewTurnStarted_EventSent()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Simple);
            Player p = new Player(inputs);
            bool eventReceived = false;
            EventHandler onNewTurnStarted = (sender, eventArgs) =>
                eventReceived = true;
            p.NewTurnStarted += onNewTurnStarted;

            p.UpdateFromNewInputs(inputs);

            Assert.IsTrue(eventReceived);
        }

        [TestMethod]
        public void IsHumanDoomed_OneDoomedOneNot_FirstDoomedSecondNot()
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

            bool result1 = p.IsHumanDoomed(0);
            bool result2 = p.IsHumanDoomed(1);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void IsHumanDoomed_UpdatedPositions_BothHumansDoomed()
        {
            // init first turn
            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 0, 0), // 3 turns to be in hero range
                    new HumanInputs(1, 9000, 0), // 2 turns to be in hero range
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 800, 0, 400, 0), // 2 turns from human 0
                    new ZombieInputs(1, 9800, 0, 9400, 0), // 2 turns from human 1                
                }
            );
            Player p = new Player(inputs);

            // check that result before update is good
            Assert.IsTrue(p.IsHumanDoomed(0));
            Assert.IsFalse(p.IsHumanDoomed(1));

            // update positions for new turn, hero did not move so both humans are now doomed
            inputs.ZombieInputs.Clear();
            inputs.AddZombieInputs(0, 400, 0, 0, 0);
            inputs.AddZombieInputs(1, 9400, 0, 9000, 0);

            // compute IsDoomed for both humans
            bool result1 = p.IsHumanDoomed(0);
            bool result2 = p.IsHumanDoomed(1);

            // check result
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }


        [TestMethod]
        public void IsHumanDoomed_TwoZombiesOnSameHuman_Doomed()
        {
            Inputs inputs = new Inputs(
                4000,
                0,
                new List<HumanInputs>(){
                    new HumanInputs(0, 0, 0), // 2 turns to get under hero protection
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 800, 0, 400, 0), // 2 turns from human 0
                    new ZombieInputs(1, 400, 0, 0, 0), // 1 turns from human 0                
                }
            );
            Player p = new Player(inputs);

            bool result = p.IsHumanDoomed(0);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsHumanDoomed_ZombieTargetIsHero_NotDoomed()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie1 = new Zombie(0, 5400, 0, 5000, 0);
            Hero hero = new Hero(5000, 0);

            Inputs inputs = new Inputs(
                5000,
                0,
                new List<HumanInputs>(){
                    new HumanInputs(0, 0, 0),
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 5400, 0, 5000, 0),
                }
            );
            Player p = new Player(inputs);

            bool result = p.IsHumanDoomed(0);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AllHumanDoomed_AllHumanDoomed_True()
        {
            Inputs inputs = new Inputs(
                10000,
                10000,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1500, 0), // many turns to be in hero range
                    new HumanInputs(1, 7500, 0), // many turns to be in hero range
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 1100, 0, 1500, 0), // one turn from human 0
                    new ZombieInputs(1, 7900, 0, 7500, 0), // one turn from human 1                
                }
            );
            Player p = new Player(inputs);

            bool res = p.AllHumanDoomed();

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void AllHumanDoomed_NotAllHumanDoomed_False()
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

            bool res = p.AllHumanDoomed();

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void GetNextZombiesBarycentre_TwoZombiesInit_GoodZombiesBarycentre()
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

            Position res = p.GetNextZombiesBarycentre();

            Assert.AreEqual(expectedBarycentre.X, res.X);
            Assert.AreEqual(expectedBarycentre.Y, res.Y);
        }

        [TestMethod]
        public void GetNextZombiesBarycentre_MovedZombies_UpdatedBarycentre()
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

            // Compute NextZombiesBarycentre before new turn
            p.GetNextZombiesBarycentre();

            // update zombie inputs to move the two zombies
            inputs.ZombieInputs[0] = new ZombieInputs(0, 1500, 0, 1894, 65);
            inputs.ZombieInputs[1] = new ZombieInputs(1, 7600, 1000, 7500, 1000);

            // update player with new inputs
            p.UpdateFromNewInputs(inputs);

            // expected barycentre
            double expectedX = (7500 + 1894) / 2.0;
            double expectedY = (1000 + 65) / 2.0;
            Position expectedBarycentre = new Position(expectedX, expectedY);

            // test
            Position res = p.GetNextZombiesBarycentre();

            // check result
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

        [TestMethod]
        public void GetNextHeroTarget_SaveAtLeastOneHuman_HumanSaved()
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
            // expected target : human 1 position
            Position expected = new Position(7500, 0);
            Player p = new Player(inputs);

            Position res = p.GetNextHeroTarget();

            Assert.AreEqual(expected.X, res.X);
            Assert.AreEqual(expected.Y, res.Y);
        }


        [TestMethod]
        public void GetNextHeroTarget_OneHumanSafe_ZombiesBarycentre()
        {
            // hero between zombies and human, zombies go toward hero
            Inputs inputs = new Inputs(
                5000,
                5000,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 0, 0),
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 10000, 5000, 9400, 5000),
                    new ZombieInputs(1, 5000, 10000, 5000, 9400),               
                }
            );
            // expected target : next zombies barycentre
            Position expected = new Position(7200, 7200);
            Player p = new Player(inputs);

            Position res = p.GetNextHeroTarget();

            Assert.AreEqual(expected.X, res.X);
            Assert.AreEqual(expected.Y, res.Y);
        }
    }
}
