using System;
using UnityEngine;
using IDM.Core.Interfaces;
using IDM.Core.Events;
using IDM.Events;
using IDM.Core;

namespace IDM.Events.Adapters
{
    /// <summary>
    /// Adapter to make the existing EventManager compatible with the IEventManager interface
    /// while preserving existing functionality.
    /// </summary>
    [RequireComponent(typeof(EventManager))]
    public class EventManagerAdapter : MonoBehaviour, IEventManager
    {
        private EventManager _eventManager;

        // IEventManager events (forward to original implementation)
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

            // Register this adapter with the ServiceLocator
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

        // Event handlers that forward events and also publish typed events
        private void HandleEventStarted(GameEvent gameEvent)
        {
            // Forward the event
            OnEventStarted?.Invoke(gameEvent);

            // Publish a typed event
            GameEventStartedEvent typedEvent = new GameEventStartedEvent(gameEvent);
            TypedEventBus.Instance.Publish(typedEvent);
        }

        private void HandleEventCompleted(GameEvent gameEvent)
        {
            // Forward the event
            OnEventCompleted?.Invoke(gameEvent);

            // Publish a typed event - we don't have choice index here, so we use -1
            // In a complete implementation, we would track the choice index
            GameEventCompletedEvent typedEvent = new GameEventCompletedEvent(gameEvent, -1);
            TypedEventBus.Instance.Publish(typedEvent);
        }

        // IEventManager methods (forward to EventManager)
        public GameEvent GetRandomEvent()
        {
            return _eventManager.GetRandomEvent();
        }

        public void SelectEventChoice(int choiceIndex)
        {
            _eventManager.SelectEventChoice(choiceIndex);

            // Note: Ideally we would have access to the current event here to publish a complete event
            // This is a limitation of the current architecture that would be addressed in a more thorough refactoring
        }
    }
}