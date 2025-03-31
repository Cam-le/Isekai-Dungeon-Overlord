using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Service locator pattern implementation to reduce direct dependencies between systems
    /// and replace the excessive singleton usage throughout the codebase.
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
        /// Register a service implementation for a specific type
        /// </summary>
        public void RegisterService<T>(T service)
        {
            _services[typeof(T)] = service;
            Debug.Log($"Service registered: {typeof(T).Name}");
        }

        /// <summary>
        /// Register a service implementation for an interface type
        /// </summary>
        public void RegisterService<TInterface, TImplementation>(TImplementation service) where TImplementation : TInterface
        {
            _services[typeof(TInterface)] = service;
            Debug.Log($"Service registered: {typeof(TInterface).Name} (Implementation: {typeof(TImplementation).Name})");
        }

        /// <summary>
        /// Get a registered service by type
        /// </summary>
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
        public bool IsServiceRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Remove a registered service
        /// </summary>
        public void UnregisterService<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
                Debug.Log($"Service unregistered: {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Log all registered services using reflection
        /// </summary>
        public void LogAllRegisteredServices()
        {
            Debug.Log($"Currently registered services: {_services.Count}");

            foreach (var serviceEntry in _services)
            {
                Type serviceType = serviceEntry.Key;
                object serviceInstance = serviceEntry.Value;

                Debug.Log($"- {serviceType.FullName} (Implementation: {serviceInstance.GetType().Name})");
            }
        }
    }
}