using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeVsZombiesLibrary
{
    public class Player
    {
        private const int UndefinedZombieTarget = -1;

        public int BestSimulScore { get; private set; }
        public IEnumerable<Position> BestSimulTargetHistory => this._bestSimulTargetHistory.ToArray();

        private readonly Game _curGame;
        private readonly Game _simulGame;
        private Inputs _lastInputs;
        private readonly Random _randomGen = new Random();
        private int _curZombieTarget = UndefinedZombieTarget;
        private Queue<Position> _lastSimulTargetHistory = new Queue<Position>(100);
        private Queue<Position> _bestSimulTargetHistory = new Queue<Position>(100);
        private Stopwatch _stopwatch = new Stopwatch();

        public Player(Inputs startInputs)
        {
            this._lastInputs = startInputs;
            this._curGame = new Game(startInputs);
            this._simulGame = new Game(startInputs);
            this.BestSimulScore = 0;
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this._lastInputs = newTurnInputs;
            this._curGame.UpdateFromNewInputs(newTurnInputs);
            this._curZombieTarget = UndefinedZombieTarget;
        }

        /// <summary>
        /// Gets next hero target to be given to coding game program, at each turn
        /// </summary>
        /// <returns>Next hero target for the "real" game</returns>
        public Position GetNextHeroTarget(int maxTimeInMilliSeconds)
        {
            this.SimulateManyGamesWithRandomZombieStrat(maxTimeInMilliSeconds);

            if (this._bestSimulTargetHistory.Count > 0)
            {
                return this._bestSimulTargetHistory.Dequeue();
            }

            if (this._lastSimulTargetHistory.Count > 0)
            {
                return this._lastSimulTargetHistory.Dequeue();
            }

            return new Position(0, 0);
        }

        public int SimulateManyGamesWithRandomZombieStrat(int maxTimeInMilliSeconds)
        {
            const int marginInMilliSeconds = 5;
            int numberOfGameSimulated = 0;

            this._stopwatch.Reset();
            this._stopwatch.Start();

            this._lastSimulTargetHistory.Clear();
            GameState curGameState = new GameState(this._lastInputs, this._curGame.Score);
            this._simulGame.ResetFromGameState(curGameState);

            while (this._stopwatch.ElapsedMilliseconds < maxTimeInMilliSeconds - marginInMilliSeconds)
            {
                Position targetPos = this.ComputeNextHeroTargetRandomZombieStrat();
                this._lastSimulTargetHistory.Enqueue(targetPos);
                bool endGame = this._simulGame.UpdateByNewTurnSimulation(targetPos);

                if (endGame)
                {
                    if (this._simulGame.Score > this.BestSimulScore)
                    {
                        this.BestSimulScore = this._simulGame.Score;
                        (this._bestSimulTargetHistory, this._lastSimulTargetHistory) = (this._lastSimulTargetHistory, this._bestSimulTargetHistory);
                    }

                    this._simulGame.ResetFromGameState(curGameState);
                    this._curZombieTarget = UndefinedZombieTarget;
                    this._lastSimulTargetHistory.Clear();
                    numberOfGameSimulated += 1;
                }
            }

            return numberOfGameSimulated;
        }

        /// <summary>
        /// Return a target for the hero, by selecting a random zombie still in 
        /// game, heading toward its next pos while he is not dead, and then 
        /// chosing another random zombie.
        /// </summary>
        /// <returns>The suggested target pos for hero for next turn</returns>
        private Position ComputeNextHeroTargetRandomZombieStrat()
        {
            if (this._curZombieTarget == UndefinedZombieTarget 
                || !this._simulGame.IsZombieAlive(this._curZombieTarget))
            {
                this._curZombieTarget = this.SelectRandomZombieAsTarget();
            }

            return this._simulGame.GetZombieNextPosition(this._curZombieTarget);
        }

        private int SelectRandomZombieAsTarget()
        {
            int[] zombiesId = this._simulGame.GetZombiesAliveIds();
            int rndIdx = _randomGen.Next(zombiesId.Length);
            return zombiesId[rndIdx];
        }
    }
}