using System;
using UnityEngine;
using IDM.Core;
using IDM.Core.Interfaces;
using IDM.Core.Events;
using IDM.Events.Interfaces;

namespace IDM.Events.Adapters
{
    /// <summary>
    /// Adapter to make the existing EventManager compatible with the ICoreEventManager interface
    /// while preserving existing functionality. This avoids cross-assembly type issues.
    /// </summary>
    [RequireComponent(typeof(EventManager))]
    public class EventManagerAdapter : MonoBehaviour, ICoreEventManager, IEventManager
    {
        private EventManager _eventManager;
        private GameEvent _currentEvent;

        // ICoreEventManager events (for cross-assembly use)
        public event Action<string> OnEventStartedById;
        public event Action<string, int> OnEventCompletedWithChoice;

        // IEventManager events (for same-assembly use)
        public event Action<GameEvent> OnEventStarted;
        public event Action<GameEvent> OnEventCompleted;

        private void Awake()
        {
            _eventManager = GetComponent<EventManager>();

            if (_eventManager == null)
            {
                Debug.LogError("EventManagerAdapter requires an EventManager component!");
                return;
            }

            // Register this adapter with the ServiceLocator as both interfaces
            ServiceLocator.Instance.RegisterService<ICoreEventManager>(this);
            ServiceLocator.Instance.RegisterService<IEventManager>(this);
        }

        private void OnEnable()
        {
            // Subscribe to EventManager events
            if (_eventManager != null)
            {
                _eventManager.OnEventStarted += HandleEventStarted;
                _eventManager.OnEventCompleted += HandleEventCompleted;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from EventManager events
            if (_eventManager != null)
            {
                _eventManager.OnEventStarted -= HandleEventStarted;
                _eventManager.OnEventCompleted -= HandleEventCompleted;
            }
        }

        // Event handlers
        private void HandleEventStarted(GameEvent gameEvent)
        {
            // Store current event
            _currentEvent = gameEvent;

            // Forward the event to same-assembly subscribers
            OnEventStarted?.Invoke(gameEvent);

            // Forward to cross-assembly subscribers using only the ID
            OnEventStartedById?.Invoke(gameEvent.id);

            // Publish a typed event for TypedEventBus users
            if (TypedEventBus.Instance != null)
            {
                GameEventStartedEvent typedEvent = new GameEventStartedEvent(
                    gameEvent.id,
                    gameEvent.title
                );
                TypedEventBus.Instance.Publish(typedEvent);
            }
        }

        private void HandleEventCompleted(GameEvent gameEvent)
        {
            // Forward the event to same-assembly subscribers
            OnEventCompleted?.Invoke(gameEvent);

            // We don't know the choice index here, so we use -1
            // Forward to cross-assembly subscribers
            OnEventCompletedWithChoice?.Invoke(gameEvent.id, -1);

            // Publish a typed event for TypedEventBus users
            if (TypedEventBus.Instance != null)
            {
                GameEventCompletedEvent typedEvent = new GameEventCompletedEvent(
                    gameEvent.id,
                    -1
                );
                TypedEventBus.Instance.Publish(typedEvent);
            }

            // Clear current event
            _currentEvent = null;
        }

        // ICoreEventManager methods
        public void SelectEventChoice(int choiceIndex)
        {
            _eventManager.SelectEventChoice(choiceIndex);

            // If we still have the current event reference, we can publish a complete event with the choice
            if (_currentEvent != null)
            {
                OnEventCompletedWithChoice?.Invoke(_currentEvent.id, choiceIndex);

                // Publish a typed event
                if (TypedEventBus.Instance != null)
                {
                    GameEventCompletedEvent typedEvent = new GameEventCompletedEvent(
                        _currentEvent.id,
                        choiceIndex
                    );
                    TypedEventBus.Instance.Publish(typedEvent);
                }
            }
        }

        // IEventManager methods
        public GameEvent GetRandomEvent()
        {
            return _eventManager.GetRandomEvent();
        }
    }
}