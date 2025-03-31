using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Economy.Interfaces;
using IDM.Core;
using IDM.Core.Events;

namespace IDM.Economy.Adapters
{
    /// <summary>
    /// Adapter to make the existing ResourceManager compatible with the IResourceManager interface
    /// while preserving existing functionality.
    /// </summary>
    [RequireComponent(typeof(ResourceManager))]
    public class ResourceManagerAdapter : MonoBehaviour, IResourceManager
    {
        private ResourceManager _resourceManager;

        // IResourceManager events (forward to original implementation)
        public event Action<ResourceType, int> OnResourceChanged;
        public event Action<ResourceType, int> OnGathererChanged;
        public event Action<Dictionary<ResourceType, int>> OnResourcesUpdated;

        private void Awake()
        {
            _resourceManager = GetComponent<ResourceManager>();

            if (_resourceManager == null)
            {
                Debug.LogError("ResourceManagerAdapter requires a ResourceManager component!");
                return;
            }

            // Register this adapter with the ServiceLocator
            ServiceLocator.Instance.RegisterService<IResourceManager>(this);
        }

        private void OnEnable()
        {
            // Subscribe to ResourceManager events
            if (_resourceManager != null)
            {
                _resourceManager.OnResourceChanged += HandleResourceChanged;
                _resourceManager.OnGathererChanged += HandleGathererChanged;
                _resourceManager.OnResourcesUpdated += HandleResourcesUpdated;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from ResourceManager events
            if (_resourceManager != null)
            {
                _resourceManager.OnResourceChanged -= HandleResourceChanged;
                _resourceManager.OnGathererChanged -= HandleGathererChanged;
                _resourceManager.OnResourcesUpdated -= HandleResourcesUpdated;
            }
        }

        // Event handlers that forward events and also publish typed events
        private void HandleResourceChanged(ResourceType resourceType, int newValue)
        {
            // Forward the event
            OnResourceChanged?.Invoke(resourceType, newValue);

            // Publish a typed event (we don't have old value, so using 0 for now)
            ResourceChangedEvent typedEvent = new ResourceChangedEvent(
                (int)resourceType,
                newValue,
                0
            );
            TypedEventBus.Instance.Publish(typedEvent);
        }

        private void HandleGathererChanged(ResourceType resourceType, int newCount)
        {
            // Forward the event
            OnGathererChanged?.Invoke(resourceType, newCount);

            // Publish a typed event
            GathererChangedEvent typedEvent = new GathererChangedEvent(
                (int)resourceType,
                newCount
            );
            TypedEventBus.Instance.Publish(typedEvent);
        }

        private void HandleResourcesUpdated(Dictionary<ResourceType, int> resources)
        {
            // Forward the event
            OnResourcesUpdated?.Invoke(resources);

            // Convert to int-based dictionary for cross-assembly compatibility
            Dictionary<int, int> resourcesById = new Dictionary<int, int>();
            foreach (var kvp in resources)
            {
                resourcesById[(int)kvp.Key] = kvp.Value;
            }

            // Publish a typed event
            AllResourcesUpdatedEvent typedEvent = new AllResourcesUpdatedEvent(resourcesById);
            TypedEventBus.Instance.Publish(typedEvent);
        }

        // IResourceManager methods (forward to ResourceManager)
        public int GetResource(ResourceType resourceType)
        {
            return _resourceManager.GetResource(resourceType);
        }

        public Dictionary<ResourceType, int> GetAllResources()
        {
            return _resourceManager.GetAllResources();
        }

        public bool AddResource(ResourceType resourceType, int amount)
        {
            return _resourceManager.AddResource(resourceType, amount);
        }

        public bool SpendResource(ResourceType resourceType, int amount)
        {
            return _resourceManager.SpendResource(resourceType, amount);
        }

        public bool HasEnoughResource(ResourceType resourceType, int amount)
        {
            return _resourceManager.HasEnoughResource(resourceType, amount);
        }

        public bool HasEnoughResources(Dictionary<ResourceType, int> resourceCosts)
        {
            return _resourceManager.HasEnoughResources(resourceCosts);
        }

        public bool SpendResources(Dictionary<ResourceType, int> resourceCosts)
        {
            return _resourceManager.SpendResources(resourceCosts);
        }

        public int GetResourceCapacity(ResourceType resourceType)
        {
            return _resourceManager.GetResourceCapacity(resourceType);
        }

        public void SetResourceCapacity(ResourceType resourceType, int capacity)
        {
            _resourceManager.SetResourceCapacity(resourceType, capacity);
        }

        public void SetGatherers(ResourceType resourceType, int count)
        {
            _resourceManager.SetGatherers(resourceType, count);
        }

        public int GetGatherers(ResourceType resourceType)
        {
            return _resourceManager.GetGatherers(resourceType);
        }

        public Dictionary<ResourceType, int> GetAllGatherers()
        {
            return _resourceManager.GetAllGatherers();
        }

        public void SetProductionModifier(ResourceType resourceType, float modifier)
        {
            _resourceManager.SetProductionModifier(resourceType, modifier);
        }

        public int GetProductionRate(ResourceType resourceType)
        {
            return _resourceManager.GetProductionRate(resourceType);
        }

        public void ProcessTimeAdvancement()
        {
            _resourceManager.ProcessTimeAdvancement();
        }

        public void ProcessTurnCompletion()
        {
            _resourceManager.ProcessTurnCompletion();
        }
    }
}