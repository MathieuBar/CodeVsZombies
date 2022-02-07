using System;

namespace CodeVsZombiesLibrary
{
    public interface IStateChangedEventSender
    {
        public event EventHandler StateChanged;
    }
}