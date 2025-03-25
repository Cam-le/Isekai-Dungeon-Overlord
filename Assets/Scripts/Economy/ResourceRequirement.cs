using System;
using UnityEngine;
using ResourceTypeEnum = IDM.Economy.ResourceType;

namespace IDM.Economy
{
    /// <summary>
    /// Represents a requirement for a specific amount of a resource type
    /// </summary>
    [Serializable]
    public struct ResourceRequirement
    {
        public ResourceTypeEnum ResourceType;
        public int Amount;

        public ResourceRequirement(ResourceTypeEnum resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = Mathf.Max(0, amount);
        }
    }
}