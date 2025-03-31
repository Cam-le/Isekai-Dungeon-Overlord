using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Service Locator provides centralized access to game services
    /// without direct dependencies between components
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ServiceLocator");
                    _instance = go.AddComponent<ServiceLocator>();
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

        /// <summary>
        /// Register a service implementation
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="service">Service implementation</param>
        public void RegisterService<T>(T service)
        {
            _services[typeof(T)] = service;
            Debug.Log($"Service registered: {typeof(T).Name}");
        }

        /// <summary>
        /// Get a registered service
        /// </summary>
        /// <typeparam name="T">Interface type to retrieve</typeparam>
        /// <returns>Service implementation or default if not found</returns>
        public T GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            Debug.LogWarning($"Service not found: {typeof(T).Name}");
            return default;
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Interface type to check</typeparam>
        /// <returns>True if service is registered</returns>
        public bool HasService<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregister a service
        /// </summary>
        /// <typeparam name="T">Interface type to unregister</typeparam>
        public void UnregisterService<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
                Debug.Log($"Service unregistered: {typeof(T).Name}");
            }
        }
    }
}