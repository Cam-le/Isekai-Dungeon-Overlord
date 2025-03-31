using System;
using UnityEngine;
using IDM.Core;
using IDM.Core.Interfaces;

namespace IDM.Core.Adapters
{
    /// <summary>
    /// Adapter to make the existing EventBus compatible with the IEventBus interface
    /// while preserving existing functionality.
    /// </summary>
    [RequireComponent(typeof(EventBus))]
    public class EventBusAdapter : MonoBehaviour, IEventBus
    {
        private EventBus _eventBus;

        private void Awake()
        {
            _eventBus = GetComponent<EventBus>();

            if (_eventBus == null)
            {
                Debug.LogError("EventBusAdapter requires an EventBus component!");
                return;
            }

            // Register this adapter with the ServiceLocator
            ServiceLocator.Instance.RegisterService<IEventBus>(this);
        }

        // IEventBus methods (forward to original EventBus)
        public void AddListener(string eventName, Action<object> listener)
        {
            _eventBus.AddListener(eventName, listener);
        }

        public void RemoveListener(string eventName, Action<object> listener)
        {
            _eventBus.RemoveListener(eventName, listener);
        }

        public void TriggerEvent(string eventName, object data = null)
        {
            _eventBus.TriggerEvent(eventName, data);
        }

        public void ClearAllListeners()
        {
            _eventBus.ClearAllListeners();
        }
    }
}