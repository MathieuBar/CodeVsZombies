using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVsZombiesLibrary
{
    public class Player
    {
        private Game _curGame;

        public Player(Inputs startInputs)
        {
            this._curGame = new Game(startInputs);
        }

        public void UpdateFromNewInputs(Inputs newTurnInputs)
        {
            this._curGame.UpdateFromNewInputs(newTurnInputs);
        }

        public Position GetNextHeroTarget()
        {
            return new Position(0, 0);
        }
    }
}