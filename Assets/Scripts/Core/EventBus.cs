using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Event bus for decoupled communication between systems
    /// </summary>
    public class EventBus : MonoBehaviour
    {
        #region Singleton
        private static EventBus _instance;

        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventBus");
                    _instance = go.AddComponent<EventBus>();
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

        private Dictionary<string, List<Action<object>>> _eventListeners =
            new Dictionary<string, List<Action<object>>>();

        /// <summary>
        /// Register to listen for an event
        /// </summary>
        public void AddListener(string eventName, Action<object> listener)
        {
            if (!_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName] = new List<Action<object>>();
            }

            _eventListeners[eventName].Add(listener);
        }

        /// <summary>
        /// Unregister from an event
        /// </summary>
        public void RemoveListener(string eventName, Action<object> listener)
        {
            if (_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName].Remove(listener);
            }
        }

        /// <summary>
        /// Trigger an event with optional data
        /// </summary>
        public void TriggerEvent(string eventName, object data = null)
        {
            if (_eventListeners.ContainsKey(eventName))
            {
                foreach (var listener in _eventListeners[eventName])
                {
                    try
                    {
                        listener?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error handling event {eventName}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Remove all listeners for cleanup
        /// </summary>
        public void ClearAllListeners()
        {
            _eventListeners.Clear();
        }
    }
}