using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        public void GetNextHeroTarget_Dilemme_HeroTargetsAreBestSimulatedTargetHistory()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Dilemme);
            Player p = new Player(inputs);

            int maxTimeInMilliSeconds = 95;
            p.SimulateManyGamesWithRandomZombieStrat(maxTimeInMilliSeconds);
            IEnumerable<Position> bestSimulTargetHistory = p.BestSimulTargetHistory;

            bool samePos = true;
            foreach(Position pos in bestSimulTargetHistory)
            {
                Position givenTargetPos = p.GetNextHeroTarget(maxTimeInMilliSeconds);
                if (pos.X != givenTargetPos.X || pos.Y != givenTargetPos.Y)
                {
                    samePos = false;
                }
            }

            Assert.IsTrue(samePos);
        }

        [TestMethod]
        public void SimulateManyGamesWithRandomZombieStrat_Reflexe_ScoreNotNull()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Reflexe);
            Player p = new Player(inputs);

            p.SimulateManyGamesWithRandomZombieStrat(95);

            Assert.IsTrue(p.BestSimulScore > 0);
        }

        [TestMethod]
        public void SimulateManyGamesWithRandomZombieStrat_Dilemme_AtLeast50SimulDone()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Dilemme);
            Player p = new Player(inputs);

            int maxTimeInMilliSeconds = 95;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int numberOfGameSimulated = p.SimulateManyGamesWithRandomZombieStrat(maxTimeInMilliSeconds);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds <= maxTimeInMilliSeconds);
            Assert.IsTrue(numberOfGameSimulated >= 50);
        }
    }
}
