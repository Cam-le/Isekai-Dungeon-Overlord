using System;
using UnityEngine;
namespace IDM.Core.Interfaces
{
    /// <summary>
    /// Abstract interface for event management that doesn't depend on concrete types
    /// </summary>
    public interface ICoreEventManager
    {
        // Generic events using basic types to avoid cross-assembly issues
        event Action<string> OnEventStartedById;
        event Action<string, int> OnEventCompletedWithChoice;

        // Methods
        void SelectEventChoice(int choiceIndex);
    }
}

