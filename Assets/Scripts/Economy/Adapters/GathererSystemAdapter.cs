using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Economy.Interfaces;
using IDM.Core;
using IDM.Core.Events;

namespace IDM.Economy.Adapters
{
    /// <summary>
    /// Adapter to make the existing GathererSystem compatible with the IGathererSystem interface
    /// while preserving existing functionality.
    /// </summary>
    [RequireComponent(typeof(GathererSystem))]
    public class GathererSystemAdapter : MonoBehaviour, IGathererSystem
    {
        private GathererSystem _gathererSystem;

        // IGathererSystem events (forward to original implementation)
        public event Action<int> OnAvailableGatherersChanged;
        public event Action<Dictionary<ResourceType, int>> OnAssignmentsChanged;

        private void Awake()
        {
            _gathererSystem = GetComponent<GathererSystem>();

            if (_gathererSystem == null)
            {
                Debug.LogError("GathererSystemAdapter requires a GathererSystem component!");
                return;
            }

            // Register this adapter with the ServiceLocator
            ServiceLocator.Instance.RegisterService<IGathererSystem>(this);
        }

        private void OnEnable()
        {
            // Subscribe to GathererSystem events
            if (_gathererSystem != null)
            {
                _gathererSystem.OnAvailableGatherersChanged += HandleAvailableGatherersChanged;
                _gathererSystem.OnAssignmentsChanged += HandleAssignmentsChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from GathererSystem events
            if (_gathererSystem != null)
            {
                _gathererSystem.OnAvailableGatherersChanged -= HandleAvailableGatherersChanged;
                _gathererSystem.OnAssignmentsChanged -= HandleAssignmentsChanged;
            }
        }

        // Event handlers
        private void HandleAvailableGatherersChanged(int availableGatherers)
        {
            // Forward the event
            OnAvailableGatherersChanged?.Invoke(availableGatherers);
        }

        private void HandleAssignmentsChanged(Dictionary<ResourceType, int> assignments)
        {
            // Forward the event
            OnAssignmentsChanged?.Invoke(assignments);

            // Publish individual gatherer changed events for each assignment
            foreach (var kvp in assignments)
            {
                GathererChangedEvent typedEvent = new GathererChangedEvent(kvp.Key, kvp.Value);
                TypedEventBus.Instance.Publish(typedEvent);
            }
        }

        // IGathererSystem methods (forward to GathererSystem)
        public bool AssignGatherers(ResourceType resourceType, int count)
        {
            return _gathererSystem.AssignGatherers(resourceType, count);
        }

        public int GetAssignedGatherers(ResourceType resourceType)
        {
            return _gathererSystem.GetAssignedGatherers(resourceType);
        }

        public int GetTotalAssignedGatherers()
        {
            return _gathererSystem.GetTotalAssignedGatherers();
        }

        public int GetAvailableGatherers()
        {
            return _gathererSystem.GetAvailableGatherers();
        }

        public void AddGatherers(int count)
        {
            _gathererSystem.AddGatherers(count);
        }

        public void SetEfficiencyMultiplier(float multiplier)
        {
            _gathererSystem.SetEfficiencyMultiplier(multiplier);
        }

        public float GetEfficiencyMultiplier()
        {
            return _gathererSystem.GetEfficiencyMultiplier();
        }

        public void ResetAssignments()
        {
            _gathererSystem.ResetAssignments();
        }
    }
}