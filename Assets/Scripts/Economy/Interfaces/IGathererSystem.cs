using System;
using System.Collections.Generic;

namespace IDM.Economy.Interfaces
{
    /// <summary>
    /// Interface for gatherer management
    /// </summary>
    public interface IGathererSystem
    {
        // Events
        event Action<int> OnAvailableGatherersChanged;
        event Action<Dictionary<ResourceType, int>> OnAssignmentsChanged;

        // Methods
        bool AssignGatherers(ResourceType resourceType, int count);
        int GetAssignedGatherers(ResourceType resourceType);
        int GetTotalAssignedGatherers();
        int GetAvailableGatherers();
        void AddGatherers(int count);
        void SetEfficiencyMultiplier(float multiplier);
        float GetEfficiencyMultiplier();
        void ResetAssignments();
    }
}