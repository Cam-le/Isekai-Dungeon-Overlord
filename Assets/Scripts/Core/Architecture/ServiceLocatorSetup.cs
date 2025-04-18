using UnityEngine;
using IDM.Core;
using IDM.Core.Interfaces;

namespace IDM.Core
{
    /// <summary>
    /// Initializes the ServiceLocator system and ensures all needed components are present.
    /// This should be placed on a GameObject in your initial scene.
    /// </summary>
    public class ServiceLocatorSetup : MonoBehaviour
    {
        [SerializeField] private GameObject serviceLocatorPrefab;
        [SerializeField] private GameObject typedEventBusPrefab;

        private void Awake()
        {
            SetupServiceLocator();
            SetupEventBus();
        }

        private void SetupServiceLocator()
        {
            if (ServiceLocator.Instance == null)
            {
                if (serviceLocatorPrefab != null)
                {
                    Instantiate(serviceLocatorPrefab);
                }
                else
                {
                    Debug.LogWarning("ServiceLocator prefab not set, creating default instance");
                    // ServiceLocator instance is automatically created by accessing Instance
                    var instance = ServiceLocator.Instance;
                }
            }
        }

        private void SetupEventBus()
        {
            if (TypedEventBus.Instance == null)
            {
                if (typedEventBusPrefab != null)
                {
                    Instantiate(typedEventBusPrefab);
                }
                else
                {
                    Debug.LogWarning("TypedEventBus prefab not set, creating default instance");
                    // TypedEventBus instance is automatically created by accessing Instance
                    var instance = TypedEventBus.Instance;
                }
            }
        }

        private void Start()
        {
            // Log the registered services for debugging
            Debug.Log("Checking registered services...");

            // Only check Core services that we can reference directly
            CheckService<IGameStateManager>("GameStateManager");
            CheckService<ICoreEventManager>("EventManager");
            CheckService<IEventBus>("EventBus");

            // Log all registered services without needing direct type references
            LogAllRegisteredServices();
        }

        private void CheckService<T>(string serviceName)
        {
            bool isRegistered = ServiceLocator.Instance.IsServiceRegistered<T>();
            Debug.Log($"Service {serviceName} ({typeof(T).Name}) is {(isRegistered ? "registered" : "NOT registered")}");
        }

        private void LogAllRegisteredServices()
        {
            // This will use reflection to get all registered services without needing direct type references
            ServiceLocator.Instance.LogAllRegisteredServices();
        }
    }
}