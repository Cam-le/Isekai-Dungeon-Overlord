using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Core;
using ResourceTypeEnum = IDM.Economy.ResourceType;
using EconomyResourceManager = IDM.Economy.ResourceManager;

namespace IDM.Economy
{
    /// <summary>
    /// Manages the assignment and efficiency of resource gatherers
    /// </summary>
    public class GathererSystem : MonoBehaviour
    {
        #region Singleton
        public static GathererSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        [Header("Gatherer Settings")]
        [SerializeField] private int _maxGatherers = 20;
        [SerializeField] private int _startingGatherers = 5;
        [SerializeField] private float _gatheringEfficiencyMultiplier = 1.0f;

        // Available gatherer pool (unassigned)
        private int _availableGatherers;

        // Current gatherer assignments
        private Dictionary<ResourceTypeEnum, int> _currentAssignments = new Dictionary<ResourceTypeEnum, int>();

        // Events
        public event Action<int> OnAvailableGatherersChanged;
        public event Action<Dictionary<ResourceTypeEnum, int>> OnAssignmentsChanged;

        private void Start()
        {
            // Initialize resource types in assignment dictionary
            foreach (ResourceTypeEnum resourceType in Enum.GetValues(typeof(ResourceTypeEnum)))
            {
                // Skip DP as it can't have gatherers
                if (resourceType != ResourceTypeEnum.DungeonPoints)
                {
                    _currentAssignments[resourceType] = 0;
                }
            }

            // Set starting values
            _availableGatherers = _startingGatherers;

            // Notify initial state
            OnAvailableGatherersChanged?.Invoke(_availableGatherers);
            OnAssignmentsChanged?.Invoke(_currentAssignments);

            // Connect to ResourceManager (if both exist)
            if (EconomyResourceManager.Instance != null)
            {
                // Sync any existing gatherer assignments to ResourceManager
                SyncAssignmentsToResourceManager();
            }
            else
            {
                Debug.LogError("ResourceManager not found! GathererSystem requires ResourceManager.");
            }
        }

        /// <summary>
        /// Assign gatherers to a specific resource
        /// </summary>
        public bool AssignGatherers(ResourceTypeEnum resourceType, int count)
        {
            // Ignore assignments for DP
            if (resourceType == ResourceTypeEnum.DungeonPoints)
                return false;

            // Validate requested count is positive
            if (count < 0)
                return false;

            // Calculate delta from current assignment
            int currentAssignment = _currentAssignments[resourceType];
            int delta = count - currentAssignment;

            // Check if we have enough available gatherers
            if (delta > 0 && delta > _availableGatherers)
                return false;

            // Update available count
            _availableGatherers -= delta;

            // Update assignment
            _currentAssignments[resourceType] = count;

            // Notify changes
            OnAvailableGatherersChanged?.Invoke(_availableGatherers);
            OnAssignmentsChanged?.Invoke(_currentAssignments);

            // Update ResourceManager
            if (EconomyResourceManager.Instance != null)
            {
                EconomyResourceManager.Instance.SetGatherers(resourceType, count);
            }

            return true;
        }

        /// <summary>
        /// Get number of gatherers assigned to a resource
        /// </summary>
        public int GetAssignedGatherers(ResourceTypeEnum resourceType)
        {
            if (resourceType == ResourceTypeEnum.DungeonPoints || !_currentAssignments.ContainsKey(resourceType))
                return 0;

            return _currentAssignments[resourceType];
        }

        /// <summary>
        /// Get total number of assigned gatherers
        /// </summary>
        public int GetTotalAssignedGatherers()
        {
            int total = 0;
            foreach (var kvp in _currentAssignments)
            {
                total += kvp.Value;
            }
            return total;
        }

        /// <summary>
        /// Get number of available (unassigned) gatherers
        /// </summary>
        public int GetAvailableGatherers()
        {
            return _availableGatherers;
        }

        /// <summary>
        /// Add new gatherers to the available pool
        /// </summary>
        public void AddGatherers(int count)
        {
            if (count <= 0)
                return;

            _availableGatherers += count;
            _maxGatherers += count; // Also increase max as these are new minions

            OnAvailableGatherersChanged?.Invoke(_availableGatherers);
        }

        /// <summary>
        /// Set the gathering efficiency multiplier (affected by upgrades, etc.)
        /// </summary>
        public void SetEfficiencyMultiplier(float multiplier)
        {
            _gatheringEfficiencyMultiplier = Mathf.Max(0.1f, multiplier);

            // Update ResourceManager with new efficiency
            SyncEfficiencyToResourceManager();
        }

        /// <summary>
        /// Get current gathering efficiency
        /// </summary>
        public float GetEfficiencyMultiplier()
        {
            return _gatheringEfficiencyMultiplier;
        }

        /// <summary>
        /// Reset all gatherer assignments
        /// </summary>
        public void ResetAssignments()
        {
            // Return all gatherers to available pool
            _availableGatherers = _startingGatherers;

            // Clear assignments
            foreach (ResourceType resourceType in _currentAssignments.Keys)
            {
                _currentAssignments[resourceType] = 0;

                // Update ResourceManager
                if (EconomyResourceManager.Instance != null)
                {
                    EconomyResourceManager.Instance.SetGatherers(resourceType, 0);
                }
            }

            // Notify changes
            OnAvailableGatherersChanged?.Invoke(_availableGatherers);
            OnAssignmentsChanged?.Invoke(_currentAssignments);
        }

        // Sync all gathering assignments to ResourceManager
        private void SyncAssignmentsToResourceManager()
        {
            if (EconomyResourceManager.Instance == null)
                return;

            foreach (var kvp in _currentAssignments)
            {
                EconomyResourceManager.Instance.SetGatherers(kvp.Key, kvp.Value);
            }

            // Also sync efficiency
            SyncEfficiencyToResourceManager();
        }

        // Sync efficiency to ResourceManager
        private void SyncEfficiencyToResourceManager()
        {
            if (EconomyResourceManager.Instance == null)
                return;

            foreach (ResourceTypeEnum resourceType in Enum.GetValues(typeof(ResourceTypeEnum)))
            {
                if (resourceType != ResourceTypeEnum.DungeonPoints)
                {
                    EconomyResourceManager.Instance.SetProductionModifier(resourceType, _gatheringEfficiencyMultiplier);
                }
            }
        }
    }
}