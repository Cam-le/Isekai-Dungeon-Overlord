using System;
using UnityEngine;

namespace IDM.Events.Interfaces
{
    /// <summary>
    /// Interface for event management and handling
    /// </summary>
    public interface IEventManager
    {
        // Events
        event Action<GameEvent> OnEventStarted;
        event Action<GameEvent> OnEventCompleted;

        // Methods
        GameEvent GetRandomEvent();
        void SelectEventChoice(int choiceIndex);
    }
}