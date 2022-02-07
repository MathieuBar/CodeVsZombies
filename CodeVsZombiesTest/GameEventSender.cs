using System;
using CodeVsZombiesLibrary;

namespace CodeVsZombiesTest
{
    public class GameEventSender: IStateChangedEventSender
    {
        public event EventHandler StateChanged;

        public void SendStateChangedEvent()
        {
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}