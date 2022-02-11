using System;

namespace CodeVsZombiesLibrary
{
    public class GameState
    {
        public Inputs Inputs { get; }
        public int Score { get; }

        public bool Equals(GameState other)
        {
            return this.Score == other.Score && this.Inputs.Equals(other.Inputs);

        }

        public GameState(Inputs inputs, int score)
        {
            this.Inputs = inputs;
            this.Score = score;
        }

        public override string ToString()
        {
            return this.Inputs.ToString() + $"Score : {this.Score}{Environment.NewLine})";
        }
    }
}