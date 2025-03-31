using System;
using System.Collections.Generic;

namespace IDM.Core.Events
{
    /// <summary>
    /// Event triggered when a resource value changes
    /// </summary>
    public readonly struct ResourceChangedEvent
    {
        public readonly IDM.Economy.ResourceType ResourceType;
        public readonly int NewValue;
        public readonly int OldValue;

        public ResourceChangedEvent(IDM.Economy.ResourceType type, int newValue, int oldValue)
        {
            ResourceType = type;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    /// <summary>
    /// Event triggered when a gatherer assignment changes
    /// </summary>
    public readonly struct GathererChangedEvent
    {
        public readonly IDM.Economy.ResourceType ResourceType;
        public readonly int NewCount;

        public GathererChangedEvent(IDM.Economy.ResourceType type, int count)
        {
            ResourceType = type;
            NewCount = count;
        }
    }

    /// <summary>
    /// Event triggered when the time period changes
    /// </summary>
    public readonly struct TimePeriodChangedEvent
    {
        public readonly TimePeriod NewTimePeriod;
        public readonly TimePeriod PreviousTimePeriod;

        public TimePeriodChangedEvent(TimePeriod newPeriod, TimePeriod prevPeriod)
        {
            NewTimePeriod = newPeriod;
            PreviousTimePeriod = prevPeriod;
        }
    }

    /// <summary>
    /// Event triggered when a turn changes
    /// </summary>
    public readonly struct TurnChangedEvent
    {
        public readonly int NewTurn;
        public readonly int PreviousTurn;

        public TurnChangedEvent(int newTurn, int prevTurn)
        {
            NewTurn = newTurn;
            PreviousTurn = prevTurn;
        }
    }

    /// <summary>
    /// Event triggered when the game state changes
    /// </summary>
    public readonly struct GameStateChangedEvent
    {
        public readonly GameStateType NewState;
        public readonly GameStateType PreviousState;

        public GameStateChangedEvent(GameStateType newState, GameStateType prevState)
        {
            NewState = newState;
            PreviousState = prevState;
        }
    }

    /// <summary>
    /// Event triggered when a game event starts
    /// </summary>
    public readonly struct GameEventStartedEvent
    {
        public readonly IDM.Events.GameEvent GameEvent;

        public GameEventStartedEvent(IDM.Events.GameEvent gameEvent)
        {
            GameEvent = gameEvent;
        }
    }

    /// <summary>
    /// Event triggered when a game event completes
    /// </summary>
    public readonly struct GameEventCompletedEvent
    {
        public readonly IDM.Events.GameEvent GameEvent;
        public readonly int ChoiceIndex;

        public GameEventCompletedEvent(IDM.Events.GameEvent gameEvent, int choiceIndex)
        {
            GameEvent = gameEvent;
            ChoiceIndex = choiceIndex;
        }
    }

    /// <summary>
    /// Event triggered when a raid occurs
    /// </summary>
    public readonly struct RaidEvent
    {
        public readonly int RaidStrength;
        public readonly string RaidSource;

        public RaidEvent(int strength, string source)
        {
            RaidStrength = strength;
            RaidSource = source;
        }
    }

    /// <summary>
    /// Event triggered when all resources are updated (usually at the start of the game)
    /// </summary>
    public readonly struct AllResourcesUpdatedEvent
    {
        public readonly IReadOnlyDictionary<IDM.Economy.ResourceType, int> Resources;

        public AllResourcesUpdatedEvent(IReadOnlyDictionary<IDM.Economy.ResourceType, int> resources)
        {
            Resources = resources;
        }
    }
}