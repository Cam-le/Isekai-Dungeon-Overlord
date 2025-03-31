using System;
using UnityEngine;

namespace IDM.Core.Interfaces
{
    /// <summary>
    /// Interface for managing game states and turn progression
    /// </summary>
    public interface IGameStateManager
    {
        // Properties
        GameStateType CurrentStateType { get; }
        int CurrentTurn { get; }
        TimePeriod CurrentTimePeriod { get; }

        // Events
        event Action<GameStateType> OnStateChanged;
        event Action<int> OnTurnChanged;
        event Action<TimePeriod> OnTimePeriodChanged;

        // Methods
        void ChangeState(GameStateType newState);
        void AdvanceTimePeriod();
        void AdvanceToNextTurn();
        void CompleteActionAndReturnToSelection();
        bool ShouldRaidOccur();
    }
}

