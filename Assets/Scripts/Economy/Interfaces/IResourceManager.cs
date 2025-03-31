using System;
using System.Collections.Generic;

namespace IDM.Economy.Interfaces
{
    /// <summary>
    /// Interface for resource management operations
    /// </summary>
    public interface IResourceManager
    {
        // Events
        event Action<ResourceType, int> OnResourceChanged;
        event Action<ResourceType, int> OnGathererChanged;
        event Action<Dictionary<ResourceType, int>> OnResourcesUpdated;

        // Resource methods
        int GetResource(ResourceType resourceType);
        Dictionary<ResourceType, int> GetAllResources();
        bool AddResource(ResourceType resourceType, int amount);
        bool SpendResource(ResourceType resourceType, int amount);
        bool HasEnoughResource(ResourceType resourceType, int amount);
        bool HasEnoughResources(Dictionary<ResourceType, int> resourceCosts);
        bool SpendResources(Dictionary<ResourceType, int> resourceCosts);

        // Capacity methods
        int GetResourceCapacity(ResourceType resourceType);
        void SetResourceCapacity(ResourceType resourceType, int capacity);

        // Gatherer methods
        void SetGatherers(ResourceType resourceType, int count);
        int GetGatherers(ResourceType resourceType);
        Dictionary<ResourceType, int> GetAllGatherers();

        // Production methods
        void SetProductionModifier(ResourceType resourceType, float modifier);
        int GetProductionRate(ResourceType resourceType);

        // Process methods
        void ProcessTimeAdvancement();
        void ProcessTurnCompletion();
    }
}