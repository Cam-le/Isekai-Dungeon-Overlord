using System;
using UnityEngine;

namespace IDM.Core.Interfaces
{
    /// <summary>
    /// Interface for pub/sub event system
    /// </summary>
    public interface IEventBus
    {
        void AddListener(string eventName, Action<object> listener);
        void RemoveListener(string eventName, Action<object> listener);
        void TriggerEvent(string eventName, object data = null);
        void ClearAllListeners();
    }
}

