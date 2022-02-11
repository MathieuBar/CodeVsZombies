using System;
using System.Collections.Generic;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class GameTest
    {
        [TestMethod]
        public void Create_ValidInputs_WellInitialized()
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
            Game g = new Game(inputs);
            
            Assert.AreEqual(inputs.ToString(), g.ToInputs().ToString());
        }
        
        [TestMethod]
        public void InitFromInputs_NewTurnStarted_EventNotSent()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Simple);
            Game g = new Game(inputs);
            bool eventReceived = false;
            EventHandler onNewTurnStarted = (sender, eventArgs) =>
                eventReceived = true;
            g.StateChanged += onNewTurnStarted;

            g.InitFromInputs(inputs);

            Assert.IsFalse(eventReceived);
        }

        [TestMethod]
        public void UpdateFromNewInputs_NewTurnStarted_EventSent()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Simple);
            Game g = new Game(inputs);
            bool eventReceived = false;
            EventHandler onNewTurnStarted = (sender, eventArgs) =>
                eventReceived = true;
            g.StateChanged += onNewTurnStarted;

            g.UpdateFromNewInputs(inputs);

            Assert.IsTrue(eventReceived);
        }

        [TestMethod]
        public void UpdateByNewTurnSimulation_ManyZombiesKilled_GoodScore()
        {
            Inputs inputs = new Inputs(
                0, 0,
                new List<HumanInputs>(){
                    // 3 humans alive => 3^2 * 10 = 90 base score for each zombie killed
                    new HumanInputs(0, 10000, 10000),
                    new HumanInputs(1, 9000, 10000),
                    new HumanInputs(2, 10000, 9000),
                },
                new List<ZombieInputs>(){
                    // first zombie goes toward human and is killed at second turn 
                    // => 90*1 = 90 points
                    new ZombieInputs(0, 0, 2400, 0, 2000),
                    // 4 following zombies go toward human and will all be killed at third turn
                    // => 90*1 + 90*2 + 90*3 + 90*5 = 990 points added to the 90 points of the previous kill
                    // => 1080 total score
                    new ZombieInputs(1, 0, 2800, 0, 2400),
                    new ZombieInputs(2, 0, 2800, 0, 2400),
                    new ZombieInputs(3, 0, 2800, 0, 2400),
                    new ZombieInputs(4, 0, 2800, 0, 2400),
                }
            );
            Game g = new Game(inputs);
            Position heroTargetPos = new Position(0, 0); // hero won't move : zombies will come to it

            g.UpdateByNewTurnSimulation(heroTargetPos);
            Assert.AreEqual(90, g.Score);

            g.UpdateByNewTurnSimulation(heroTargetPos);
            Assert.AreEqual(1080, g.Score);
        }

        [TestMethod]
        public void UpdateByNewTurnSimulation_HumanKilled_ReturnFalseAndGoodHumansAlive()
        {
            Inputs inputs = new Inputs(
                9000, 9000,
                new List<HumanInputs>(){
                    new HumanInputs(0, 10000, 9000),
                    new HumanInputs(1, 0, 0),
                    new HumanInputs(2, 9000, 10000),
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 0, 400, 0, 0),
                }
            );
            Game g = new Game(inputs);
            Position heroTargetPos = new Position(9000, 9000); // hero won't move
            int[] expectedHumansAliveIds = { 0, 2 };

            bool gameEnd = g.UpdateByNewTurnSimulation(heroTargetPos);
            int[] humansAliveIds = g.GetHumansAliveIds();

            Assert.IsFalse(gameEnd);
            CollectionAssert.AreEquivalent(expectedHumansAliveIds, humansAliveIds);
        }

        [TestMethod]
        public void UpdateByNewTurnSimulation_AllZombiesKilled_ReturnTrue()
        {
            Inputs inputs = new Inputs(
                0, 0,
                new List<HumanInputs>(){new HumanInputs(0, 0, 0)},
                new List<ZombieInputs>(){new ZombieInputs(0, 2400, 0, 2000, 0)}
            );
            Game g = new Game(inputs);
            Position heroTargetPos = new Position(0, 0); // hero won't move
            int expectedScore = 10;

            bool result = g.UpdateByNewTurnSimulation(heroTargetPos);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedScore, g.Score);
        }


        [TestMethod]
        public void UpdateByNewTurnSimulation_AllHumanKilled_ReturnTrueScore0()
        {
            Inputs inputs = new Inputs(
                0, 0,
                new List<HumanInputs>(){new HumanInputs(
                    0, 5000, 0),
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 5800, 0, 5400, 0), // this zombie will kill the human after 2nd turn
                    new ZombieInputs(1, 1000, 0, 600, 0), // this zombie will be killed after 1st turn
                }
            );
            Game g = new Game(inputs);
            Position heroTargetPos = new Position(0, 0); // hero won't move

            bool result = g.UpdateByNewTurnSimulation(heroTargetPos);
            Assert.IsFalse(result);
            Assert.AreEqual(10, g.Score);

            result = g.UpdateByNewTurnSimulation(heroTargetPos);
            Assert.IsTrue(result);
            Assert.AreEqual(0, g.Score);
        }

        [TestMethod]
        public void UpdateByNewTurnSimulation_TwoZombiesRemix_SameResultsAsCodingGame()
        {
            IList<(GameState state, Position target)> realGameHistory = 
                InputsGenerator.GenerateRealCaseInputHistoryForTwoZombiesRemix();
            Game g = new Game(realGameHistory[0].state.Inputs);
            
            for (int i = 1; i < realGameHistory.Count; i++)
            {
                Position targetPos = realGameHistory[i - 1].target;
                g.UpdateByNewTurnSimulation(targetPos);
                GameState computedState = g.ToState();
                GameState expectedState = realGameHistory[i].state;

                Assert.IsTrue(computedState.Equals(expectedState));
            }
        }

        [TestMethod]
        public void GetZombieNextPosition_UnknownZombie_UndefinedPos()
        {
            Inputs inputs = new Inputs(
                0, 0,
                new List<HumanInputs>(){new HumanInputs(0, 0, 0)},
                new List<ZombieInputs>(){new ZombieInputs(0, 2400, 0, 2000, 0)}
            );
            Game g = new Game(inputs);

            Position result = g.GetZombieNextPosition(1);

            Assert.AreEqual(Position.UndefinedPos.X, result.X);
            Assert.AreEqual(Position.UndefinedPos.Y, result.Y);
        }


        [TestMethod]
        public void GetZombieNextPosition_KnownZombie_ZombiePos()
        {
            Inputs inputs = new Inputs(
                0, 0,
                new List<HumanInputs>(){new HumanInputs(0, 0, 1000)},
                new List<ZombieInputs>(){new ZombieInputs(0, 2400, 1000, 2000, 1000)}
            );
            Game g = new Game(inputs);

            Position result = g.GetZombieNextPosition(0);

            Assert.AreEqual(2000, result.X);
            Assert.AreEqual(1000, result.Y);
        }
     }
}