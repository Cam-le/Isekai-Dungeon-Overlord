using System;
using System.Collections.Generic;
using UnityEngine;
using IDM.Economy;
using ResourceTypeEnum = IDM.Economy.ResourceType;
using EconomyResourceManager = IDM.Economy.ResourceManager;

namespace IDM.Economy
{
    /// <summary>
    /// Component to attach to objects that have a resource cost
    /// </summary>
    public class ResourceCost : MonoBehaviour
    {
        [SerializeField] private ResourceRequirement[] _resourceRequirements;

        private Dictionary<ResourceTypeEnum, int> _costDictionary = new Dictionary<ResourceTypeEnum, int>();

        private void Awake()
        {
            // Convert array to dictionary for easier use
            foreach (var requirement in _resourceRequirements)
            {
                if (requirement.Amount > 0)
                {
                    _costDictionary[requirement.ResourceType] = requirement.Amount;
                }
            }
        }

        /// <summary>
        /// Get all resource requirements
        /// </summary>
        public IReadOnlyDictionary<ResourceTypeEnum, int> GetRequirements()
        {
            return _costDictionary;
        }

        /// <summary>
        /// Check if the player can afford this cost
        /// </summary>
        public bool CanAfford()
        {
            return EconomyResourceManager.Instance.HasEnoughResources(_costDictionary);
        }

        /// <summary>
        /// Try to spend resources for this cost
        /// </summary>
        public bool TrySpendResources()
        {
            return EconomyResourceManager.Instance.SpendResources(_costDictionary);
        }

        /// <summary>
        /// Get the cost of a specific resource
        /// </summary>
        public int GetCost(ResourceTypeEnum resourceType)
        {
            if (_costDictionary.TryGetValue(resourceType, out int amount))
                return amount;
            return 0;
        }
    }
}