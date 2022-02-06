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
            g.NewTurnStarted += onNewTurnStarted;

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
            g.NewTurnStarted += onNewTurnStarted;

            g.UpdateFromNewInputs(inputs);

            Assert.IsTrue(eventReceived);
        }
    }
}