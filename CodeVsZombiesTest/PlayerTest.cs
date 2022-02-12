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
        public void GetNextHeroTarget_Reflexe_ScoreNotNull()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Reflexe);
            Player p = new Player(inputs);
            Game g = new Game(inputs);
            int maxTimeInMilliSeconds = 95;
            
            bool endGame = false;
            while (!endGame)
            {
                Position targetPos = p.GetNextHeroTarget(maxTimeInMilliSeconds);
                endGame = g.UpdateByNewTurnSimulation(targetPos);
            }

            Assert.IsTrue(p.BestSimulScore > 0);
        }


        [TestMethod]
        public void GetNextHeroTarget_Dilemme_FinalScoreBetterOrEqualThanFirstSimulation()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Dilemme);

            int maxTimeInMilliSeconds = 95;

            // simulate game with target history simulated at first turn 
            Player playerRef = new Player(inputs);
            playerRef.SimulateManyGamesWithRandomZombieStrat(maxTimeInMilliSeconds);
            IEnumerable<Position> bestSimulTargetHistory = playerRef.BestSimulTargetHistory;
            Game gameRef = new Game(inputs);
            foreach(Position targetPos in bestSimulTargetHistory)
            {
                bool gameRefEnd = gameRef.UpdateByNewTurnSimulation(targetPos);
                if (gameRefEnd) break;
            }
            int scoreRef = gameRef.Score;

            // simulate game with player GetNextHeroTarget (with simulation at each turn to try and improve score
            Game gameImproved = new Game(inputs);
            bool endGameImproved = false;
            while (!endGameImproved)
            {
                playerRef.UpdateFromNewInputs(gameImproved.ToInputs());
                endGameImproved =  gameImproved.UpdateByNewTurnSimulation(playerRef.GetNextHeroTarget(maxTimeInMilliSeconds));
            }
            int scoreImproved = gameImproved.Score;

            Assert.IsTrue(scoreImproved >= scoreRef);
        }

        [TestMethod]
        public void GetNextHeroTarget_Dilemme_PlayerBestSimulScoreEqualsActualScore()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Dilemme);
            Player p = new Player(inputs);
            Game g = new Game(inputs);
            int maxTimeInMilliSeconds = 95;
            
            bool endGame = false;
            while (!endGame)
            {
                Position targetPos = p.GetNextHeroTarget(maxTimeInMilliSeconds);
                endGame = g.UpdateByNewTurnSimulation(targetPos);
                if (!endGame)
                {
                    p.UpdateFromNewInputs(g.ToInputs());
                }
            }

            Assert.AreEqual(p.BestSimulScore, g.Score);
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
