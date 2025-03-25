// Move GameLoopManagerExtensions.cs to IDM.Core namespace
// (instead of having it in IDM.Economy and creating a dependency cycle)

using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Extension methods for GameLoopManager to handle resource events
    /// </summary>
    public static class GameLoopManagerExtensions
    {
        /// <summary>
        /// Attach this to GameLoopManager's initialization to connect resource system
        /// </summary>
        public static void ConnectResourceSystem(this GameLoopManager gameLoopManager)
        {
            if (gameLoopManager == null)
            {
                Debug.LogError("GameLoopManager is null! Cannot connect resource system.");
                return;
            }

            // Subscribe to time advancement events
            gameLoopManager.OnTimePeriodChanged += HandleTimePeriodChanged;
            gameLoopManager.OnTurnChanged += HandleTurnChanged;

            Debug.Log("Resource system connected to game loop");
        }

        /// <summary>
        /// Detach resource system from game loop to prevent memory leaks
        /// </summary>
        public static void DisconnectResourceSystem(this GameLoopManager gameLoopManager)
        {
            gameLoopManager.OnTimePeriodChanged -= HandleTimePeriodChanged;
            gameLoopManager.OnTurnChanged -= HandleTurnChanged;
        }

        // Handle time period changes by generating resources
        private static void HandleTimePeriodChanged(TimePeriod newTimePeriod)
        {
            // Use event bus pattern to broadcast without direct dependency
            EventBus.Instance.TriggerEvent("TimePeriodAdvanced", newTimePeriod);
            Debug.Log($"Resources collected for {newTimePeriod} time period");
        }

        // Handle turn changes by generating Dungeon Points
        private static void HandleTurnChanged(int newTurn)
        {
            // Use event bus pattern to broadcast without direct dependency
            EventBus.Instance.TriggerEvent("TurnCompleted", newTurn);
            Debug.Log($"Turn completed, Dungeon Points generated for turn {newTurn}");
        }
    }
}