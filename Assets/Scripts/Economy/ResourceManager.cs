using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Core;

namespace IDM.Economy
{
    /// <summary>
    /// Manages all resource operations for the game
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region Singleton
        public static ResourceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
        }
        #endregion

        #region Events
        public event Action<ResourceType, int> OnResourceChanged;
        public event Action<ResourceType, int> OnGathererChanged;
        public event Action<Dictionary<ResourceType, int>> OnResourcesUpdated;
        #endregion

        #region Resource Data
        [SerializeField] private int _startingDungeonPoints = 100;
        [SerializeField] private int _startingWood = 30;
        [SerializeField] private int _startingStone = 20;
        [SerializeField] private int _startingMetal = 10;
        [SerializeField] private int _startingFood = 50;
        [SerializeField] private int _baseResourcePerGatherer = 5;
        [SerializeField] private int _dpGainPerTurn = 15;

        // Current resources
        private Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

        // Gatherer assignments
        private Dictionary<ResourceType, int> _gatherers = new Dictionary<ResourceType, int>();

        // Resource capacity
        private Dictionary<ResourceType, int> _resourceCapacity = new Dictionary<ResourceType, int>();

        // Production modifiers (can be affected by upgrades, events, etc.)
        private Dictionary<ResourceType, float> _productionModifiers = new Dictionary<ResourceType, float>();
        #endregion

        private void InitializeResources()
        {
            // Set up default values for all resource types
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                _resources[resourceType] = 0;
                _gatherers[resourceType] = 0;
                _resourceCapacity[resourceType] = int.MaxValue; // Default to unlimited
                _productionModifiers[resourceType] = 1.0f; // Default modifier (1.0 = 100%)
            }

            // Set initial resource values
            _resources[ResourceType.DungeonPoints] = _startingDungeonPoints;
            _resources[ResourceType.Wood] = _startingWood;
            _resources[ResourceType.Stone] = _startingStone;
            _resources[ResourceType.Metal] = _startingMetal;
            _resources[ResourceType.Food] = _startingFood;

            // Set default capacity for resources (adjust as needed)
            _resourceCapacity[ResourceType.Wood] = 100;
            _resourceCapacity[ResourceType.Stone] = 100;
            _resourceCapacity[ResourceType.Metal] = 50;
            _resourceCapacity[ResourceType.Food] = 200;
            _resourceCapacity[ResourceType.Knowledge] = 100;

            // Dungeon points have no capacity limit
            _resourceCapacity[ResourceType.DungeonPoints] = int.MaxValue;

            // Notify UI of initial resource values
            OnResourcesUpdated?.Invoke(_resources);
        }

        private void Start()
        {
            // Subscribe to game loop events through event bus (no direct dependency)
            IDM.Core.EventBus.Instance.AddListener("TimePeriodAdvanced", OnTimePeriodAdvanced);
            IDM.Core.EventBus.Instance.AddListener("TurnCompleted", OnTurnCompleted);
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            IDM.Core.EventBus.Instance.RemoveListener("TimePeriodAdvanced", OnTimePeriodAdvanced);
            IDM.Core.EventBus.Instance.RemoveListener("TurnCompleted", OnTurnCompleted);
        }

        // Event handlers
        private void OnTimePeriodAdvanced(object data)
        {
            // Process resource collection when time advances
            ProcessTimeAdvancement();
        }

        private void OnTurnCompleted(object data)
        {
            // Generate DP when turn completes
            ProcessTurnCompletion();
        }
        #region Public API

        /// <summary>
        /// Get the current amount of a resource
        /// </summary>
        public int GetResource(ResourceType resourceType)
        {
            return _resources[resourceType];
        }

        /// <summary>
        /// Get all current resource values
        /// </summary>
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_resources);
        }

        /// <summary>
        /// Add an amount of a resource, respecting capacity limits
        /// </summary>
        public bool AddResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return false;

            int newValue = Mathf.Min(_resources[resourceType] + amount, _resourceCapacity[resourceType]);
            int actualAmountAdded = newValue - _resources[resourceType];

            if (actualAmountAdded <= 0) return false;

            _resources[resourceType] = newValue;
            OnResourceChanged?.Invoke(resourceType, _resources[resourceType]);
            return true;
        }

        /// <summary>
        /// Spend resources if they're available
        /// </summary>
        public bool SpendResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0 || _resources[resourceType] < amount) return false;

            _resources[resourceType] -= amount;
            OnResourceChanged?.Invoke(resourceType, _resources[resourceType]);
            return true;
        }

        /// <summary>
        /// Check if player has enough of a resource
        /// </summary>
        public bool HasEnoughResource(ResourceType resourceType, int amount)
        {
            return _resources[resourceType] >= amount;
        }

        /// <summary>
        /// Check if player has enough of multiple resources
        /// </summary>
        public bool HasEnoughResources(Dictionary<ResourceType, int> resourceCosts)
        {
            foreach (var cost in resourceCosts)
            {
                if (_resources[cost.Key] < cost.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Spend multiple resources at once (all or nothing)
        /// </summary>
        public bool SpendResources(Dictionary<ResourceType, int> resourceCosts)
        {
            if (!HasEnoughResources(resourceCosts))
                return false;

            foreach (var cost in resourceCosts)
            {
                _resources[cost.Key] -= cost.Value;
                OnResourceChanged?.Invoke(cost.Key, _resources[cost.Key]);
            }

            OnResourcesUpdated?.Invoke(_resources);
            return true;
        }

        /// <summary>
        /// Set gatherer count for a resource
        /// </summary>
        public void SetGatherers(ResourceType resourceType, int count)
        {
            // Ensure we're not setting gatherers for Dungeon Points
            if (resourceType == ResourceType.DungeonPoints)
                return;

            // Validate count is non-negative
            count = Mathf.Max(0, count);

            // Apply the change
            _gatherers[resourceType] = count;
            OnGathererChanged?.Invoke(resourceType, count);
        }

        /// <summary>
        /// Get current gatherer count for a resource
        /// </summary>
        public int GetGatherers(ResourceType resourceType)
        {
            return _gatherers[resourceType];
        }

        /// <summary>
        /// Get all gatherer assignments
        /// </summary>
        public Dictionary<ResourceType, int> GetAllGatherers()
        {
            return new Dictionary<ResourceType, int>(_gatherers);
        }

        /// <summary>
        /// Set a production modifier for a resource type
        /// </summary>
        public void SetProductionModifier(ResourceType resourceType, float modifier)
        {
            _productionModifiers[resourceType] = Mathf.Max(0, modifier);
        }

        /// <summary>
        /// Get current production rate per time period for a resource
        /// </summary>
        public int GetProductionRate(ResourceType resourceType)
        {
            // Dungeon Points have fixed generation per turn
            if (resourceType == ResourceType.DungeonPoints)
                return 0; // DP isn't generated per time period

            return Mathf.RoundToInt(_gatherers[resourceType] * _baseResourcePerGatherer * _productionModifiers[resourceType]);
        }

        /// <summary>
        /// Get maximum capacity for a resource
        /// </summary>
        public int GetResourceCapacity(ResourceType resourceType)
        {
            return _resourceCapacity[resourceType];
        }

        /// <summary>
        /// Set new capacity for a resource
        /// </summary>
        public void SetResourceCapacity(ResourceType resourceType, int capacity)
        {
            _resourceCapacity[resourceType] = Mathf.Max(1, capacity);

            // Ensure current resources don't exceed capacity
            if (_resources[resourceType] > _resourceCapacity[resourceType])
            {
                _resources[resourceType] = _resourceCapacity[resourceType];
                OnResourceChanged?.Invoke(resourceType, _resources[resourceType]);
            }
        }

        /// <summary>
        /// Process resource generation for a time period
        /// </summary>
        public void ProcessTimeAdvancement()
        {
            // Generate resources based on gatherers
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                // Skip DP as it's generated per turn, not per time period
                if (resourceType == ResourceType.DungeonPoints)
                    continue;

                int amountToAdd = GetProductionRate(resourceType);
                if (amountToAdd > 0)
                {
                    AddResource(resourceType, amountToAdd);
                }
            }

            // Notify all listeners of updated resources
            OnResourcesUpdated?.Invoke(_resources);
        }

        /// <summary>
        /// Process turn completion (generates DP)
        /// </summary>
        public void ProcessTurnCompletion()
        {
            // Generate Dungeon Points at the end of a turn
            AddResource(ResourceType.DungeonPoints, _dpGainPerTurn);

            // Notify all listeners of updated resources
            OnResourcesUpdated?.Invoke(_resources);
        }
        #endregion
    }
}