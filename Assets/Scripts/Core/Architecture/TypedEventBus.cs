using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Strongly-typed event bus for decoupled communication between systems.
    /// Replaces the string-based event system with a type-safe implementation.
    /// </summary>
    public class TypedEventBus : MonoBehaviour
    {
        #region Singleton Pattern
        private static TypedEventBus _instance;

        public static TypedEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("TypedEventBus");
                    _instance = go.AddComponent<TypedEventBus>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        private Dictionary<Type, List<Delegate>> _eventHandlers =
            new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe to events of type T
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_eventHandlers.ContainsKey(type))
            {
                _eventHandlers[type] = new List<Delegate>();
            }

            _eventHandlers[type].Add(handler);
            Debug.Log($"Subscribed to event: {type.Name}");
        }

        /// <summary>
        /// Publish an event of type T
        /// </summary>
        public void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_eventHandlers.ContainsKey(type)) return;

            // Create a copy of the handlers list to avoid modification during iteration
            var handlers = new List<Delegate>(_eventHandlers[type]);

            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Action<T> typedHandler)
                    {
                        typedHandler(eventData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error handling event {type.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Unsubscribe from events of type T
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_eventHandlers.ContainsKey(type)) return;

            _eventHandlers[type].Remove(handler);

            // Remove the list if it's empty
            if (_eventHandlers[type].Count == 0)
            {
                _eventHandlers.Remove(type);
            }
        }

        /// <summary>
        /// Clear all event handlers
        /// </summary>
        public void ClearAllHandlers()
        {
            _eventHandlers.Clear();
            Debug.Log("All event handlers cleared");
        }

        /// <summary>
        /// Get the number of subscribers for an event type
        /// </summary>
        public int GetSubscriberCount<T>() where T : struct
        {
            var type = typeof(T);
            if (!_eventHandlers.ContainsKey(type)) return 0;

            return _eventHandlers[type].Count;
        }
    }
}