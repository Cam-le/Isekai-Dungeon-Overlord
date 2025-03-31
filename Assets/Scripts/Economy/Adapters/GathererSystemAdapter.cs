using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Core;
using IDM.Core.Events;

namespace IDM.Economy.Adapters
{
    /// <summary>
    /// Adapter for the GathererSystem that avoids cross-assembly type issues.
    /// </summary>
    [RequireComponent(typeof(GathererSystem))]
    public class GathererSystemAdapter : MonoBehaviour
    {
        private GathererSystem _gathererSystem;

        // Events using primitive types to avoid cross-assembly issues
        public event Action<int> OnAvailableGatherersChanged;
        public event Action<Dictionary<int, int>> OnAssignmentsChangedById;

        private void Awake()
        {
            _gathererSystem = GetComponent<GathererSystem>();

            if (_gathererSystem == null)
            {
                Debug.LogError("GathererSystemAdapter requires a GathererSystem component!");
                return;
            }

            // Register this adapter with the ServiceLocator
            ServiceLocator.Instance.RegisterService<GathererSystemAdapter>(this);
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
            // Convert ResourceType enum values to integers for cross-assembly compatibility
            Dictionary<int, int> assignmentsById = new Dictionary<int, int>();
            foreach (var kvp in assignments)
            {
                assignmentsById[(int)kvp.Key] = kvp.Value;
            }

            // Forward the event
            OnAssignmentsChangedById?.Invoke(assignmentsById);

            // Publish individual gatherer changed events for each assignment
            if (TypedEventBus.Instance != null)
            {
                foreach (var kvp in assignments)
                {
                    GathererChangedEvent typedEvent = new GathererChangedEvent((int)kvp.Key, kvp.Value);
                    TypedEventBus.Instance.Publish(typedEvent);
                }
            }
        }

        // Public methods that forward to GathererSystem using primitive types
        public bool AssignGatherers(int resourceTypeId, int count)
        {
            return _gathererSystem.AssignGatherers((ResourceType)resourceTypeId, count);
        }

        public int GetAssignedGatherers(int resourceTypeId)
        {
            return _gathererSystem.GetAssignedGatherers((ResourceType)resourceTypeId);
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