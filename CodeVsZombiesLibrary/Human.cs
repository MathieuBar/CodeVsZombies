using System;
using System.Collections.Generic;

namespace CodeVsZombiesLibrary
{
    public class Human : Character
    {   public Human(int id, int xPos, int yPos, Player owner = null):
            base(id, xPos, yPos, owner)
        {
            this.Speed = 0;
        }

        public Human(HumanInputs hi, Player owner = null): this(hi.Id, hi.X, hi.Y, owner)
        {
            // nothing to add
        }

        public HumanInputs ToHumanInputs()
        {
            return new HumanInputs(this.Id, this.Pos.X, this.Pos.Y);
        }
    }
}