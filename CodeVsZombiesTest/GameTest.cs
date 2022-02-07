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
                    new HumanInputs(2, 1000, 9000),
                },
                new List<ZombieInputs>(){
                    // first zombie goes toward human and is killed at second turn 
                    // => 90*3 = 270 points
                    new ZombieInputs(0, 0, 2400, 0, 2000),
                    // three following zombies go toward human and will all be killed at third turn
                    // => 90*3 + 90*5 + 90*8 = 1440 points added to the 270 points of the previous kill
                    // => 1710 total score
                    new ZombieInputs(1, 0, 2800, 0, 2400),
                    new ZombieInputs(2, 0, 2800, 0, 2400),
                    new ZombieInputs(3, 0, 2800, 0, 2400),
                }
            );
            Game g = new Game(inputs);
            Position heroTargetPos = new Position(0, 0); // hero won't move : zombies will come to it

            g.UpdateByNewTurnSimulation(heroTargetPos);
            
            Assert.AreEqual(270, g.Score);

            g.UpdateByNewTurnSimulation(heroTargetPos);
            Assert.AreEqual(1710, g.Score);
        }
    }
}