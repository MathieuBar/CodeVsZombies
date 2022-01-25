using System;
using System.Collections.Generic;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class PlayerTest
    {
        public static Inputs GenerateInputs(PlayerTestCase playerTestCase) => playerTestCase switch
        {
            // Human 0 doomed, human 1 not doomed
            PlayerTestCase.TwoHumansFirstDoomed => new Inputs(
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
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(playerTestCase), $"Not expected player test name value: {playerTestCase}"),
        };


        [TestMethod]
        public void Create_ValidInputs_PlayerWellInitialized()
        {
            Inputs inputs = PlayerTest.GenerateInputs(PlayerTestCase.TwoHumansFirstDoomed);

            Player p = new Player(inputs);

            Assert.AreEqual(p.ToInputs().ToString(), inputs.ToString());
        }

        [TestMethod]
        public void GetNextHeroTarget_OneDoomedHuman_DoomedFlagSet()
        {
            Inputs inputs = PlayerTest.GenerateInputs(PlayerTestCase.TwoHumansFirstDoomed);
            Player p = new Player(inputs);

            p.GetNextHeroTarget();

            Assert.IsTrue(p.IsHumanDoomed(0));
            Assert.IsFalse(p.IsHumanDoomed(1));
        }
    }
}
