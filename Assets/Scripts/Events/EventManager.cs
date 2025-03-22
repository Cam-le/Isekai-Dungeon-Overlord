using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDM.Core;

namespace IDM.Events
{
    /// <summary>
    /// Manages events and interactions during the Event Interaction State
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static EventManager _instance;
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<EventManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EventManager");
                        _instance = go.AddComponent<EventManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        // Reference to the game manager
        private GameLoopManager _gameManager;

        // List of potential events
        [SerializeField] private List<GameEvent> potentialEvents = new List<GameEvent>();

        // Currently active event
        private GameEvent _currentEvent;

        // Event related flags
        private bool _isEventActive = false;

        // Events
        public event Action<GameEvent> OnEventStarted;
        public event Action<GameEvent> OnEventCompleted;

        private void Start()
        {
            // Get reference to the game manager
            _gameManager = GameLoopManager.Instance;

            // Subscribe to state change events
            _gameManager.OnStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_gameManager != null)
            {
                _gameManager.OnStateChanged -= HandleStateChange;
            }
        }

        /// <summary>
        /// Handles state changes to trigger events when appropriate
        /// </summary>
        private void HandleStateChange(GameStateType newState)
        {
            if (newState == GameStateType.EventInteraction)
            {
                // When entering the Event state, trigger an event
                TriggerRandomEvent();
            }
        }

        /// <summary>
        /// Returns a random event that's appropriate for the current time period and game state
        /// </summary>
        public GameEvent GetRandomEvent()
        {
            // This would filter events based on current time period, turn number, etc.
            // For now, just return a random event

            if (potentialEvents.Count == 0)
            {
                Debug.LogWarning("No events available to trigger.");
                return null;
            }

            int eventIndex = UnityEngine.Random.Range(0, potentialEvents.Count);
            return potentialEvents[eventIndex];
        }

        /// <summary>
        /// Triggers a random event
        /// </summary>
        private void TriggerRandomEvent()
        {
            _currentEvent = GetRandomEvent();

            if (_currentEvent == null)
            {
                // If no suitable event, move back to time advance
                _gameManager.ChangeState(GameStateType.AdvanceTime);
                return;
            }

            // Set flag and notify listeners
            _isEventActive = true;
            OnEventStarted?.Invoke(_currentEvent);

            Debug.Log($"Event triggered: {_currentEvent.title} during {_gameManager.CurrentTimePeriod}");
        }

        /// <summary>
        /// Called when the player selects a choice for the current event
        /// </summary>
        public void SelectEventChoice(int choiceIndex)
        {
            if (!_isEventActive || _currentEvent == null)
            {
                Debug.LogWarning("Attempted to select a choice when no event is active.");
                return;
            }

            // Ensure the choice index is valid
            if (choiceIndex < 0 || choiceIndex >= _currentEvent.choices.Count)
            {
                Debug.LogError($"Invalid choice index: {choiceIndex}");
                return;
            }

            // Get the selected choice
            EventChoice choice = _currentEvent.choices[choiceIndex];

            // Apply the choice's effects
            ApplyEventChoiceEffects(choice);

            // Mark the event as completed
            _isEventActive = false;
            OnEventCompleted?.Invoke(_currentEvent);

            Debug.Log($"Event '{_currentEvent.title}' completed with choice: {choice.text}");

            // Continue time advancement
            _gameManager.ChangeState(GameStateType.AdvanceTime);
        }

        /// <summary>
        /// Applies the effects of an event choice
        /// </summary>
        private void ApplyEventChoiceEffects(EventChoice choice)
        {
            // This would apply the actual game effects
            // For example, modifying resources, relationships, etc.

            Debug.Log($"Applied event effects: {choice.effectDescription}");
        }
    }

    /// <summary>
    /// Represents a game event with multiple choices
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        public string id;
        public string title;
        [TextArea(3, 6)]
        public string description;
        public List<EventChoice> choices = new List<EventChoice>();

        // Time period constraints (when this event can occur)
        public bool canOccurInMorning = true;
        public bool canOccurInAfternoon = true;
        public bool canOccurInEvening = true;
        public bool canOccurInNight = true;

        // Additional properties could include:
        // - Requirements for the event to trigger
        // - Visual elements (character portraits, backgrounds)
        // - Audio elements

        /// <summary>
        /// Determines if this event can occur during the specified time period
        /// </summary>
        public bool CanOccurDuring(TimePeriod timePeriod)
        {
            return timePeriod switch
            {
                TimePeriod.Morning => canOccurInMorning,
                TimePeriod.Afternoon => canOccurInAfternoon,
                TimePeriod.Evening => canOccurInEvening,
                TimePeriod.Night => canOccurInNight,
                _ => false
            };
        }
    }

    /// <summary>
    /// Represents a choice in a game event
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        public string text;
        [TextArea(2, 4)]
        public string effectDescription;

        // Additional properties could include:
        // - Resource effects (DP cost/gain, etc.)
        // - Relationship effects with factions
        // - Unlocks or triggers for other events
    }
}