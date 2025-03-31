using System;
using System.Collections.Generic;

namespace IDM.Core.Events
{
    /// <summary>
    /// Event triggered when a resource value changes
    /// </summary>
    public readonly struct ResourceChangedEvent
    {
        public readonly int ResourceTypeId;
        public readonly int NewValue;
        public readonly int OldValue;

        public ResourceChangedEvent(int resourceTypeId, int newValue, int oldValue)
        {
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    /// <summary>
    /// Event triggered when a gatherer assignment changes
    /// </summary>
    public readonly struct GathererChangedEvent
    {
        public readonly int ResourceTypeId;
        public readonly int NewCount;

        public GathererChangedEvent(int resourceTypeId, int count)
        {
            ResourceTypeId = resourceTypeId;
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
        public readonly string EventId;
        public readonly string EventTitle;

        public GameEventStartedEvent(string eventId, string eventTitle)
        {
            EventId = eventId;
            EventTitle = eventTitle;
        }
    }

    /// <summary>
    /// Event triggered when a game event completes
    /// </summary>
    public readonly struct GameEventCompletedEvent
    {
        public readonly string EventId;
        public readonly int ChoiceIndex;

        public GameEventCompletedEvent(string eventId, int choiceIndex)
        {
            EventId = eventId;
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
        public readonly IReadOnlyDictionary<int, int> Resources;

        public AllResourcesUpdatedEvent(IReadOnlyDictionary<int, int> resources)
        {
            Resources = resources;
        }
    }
}