using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDM.Core
{
    /// <summary>
    /// Main Game Manager that controls the time-based game loop
    /// </summary>
    public class GameLoopManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static GameLoopManager _instance;
        public static GameLoopManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameLoopManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameLoopManager");
                        _instance = go.AddComponent<GameLoopManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region State Variables
        // Current game state
        private GameStateBase _currentState;

        // All possible game states
        private TurnStartState _turnStartState;
        private PlayerActionSelectionState _playerActionSelectionState;
        private DungeonManagementState _dungeonManagementState;
        private FactionNegotiationState _factionNegotiationState;
        private BuildingConstructionState _buildingConstructionState;
        private ResourceGatheringState _resourceGatheringState;
        private EventInteractionState _eventInteractionState;
        private AdvanceTimeState _advanceTimeState;
        private CombatDefenseState _combatDefenseState;
        private TurnEndState _turnEndState;

        // Current turn and time period
        private int _currentTurn = 0;
        private TimePeriod _currentTimePeriod = TimePeriod.Morning;

        // Raid tracking (placeholder - to be expanded later)
        private int _turnsSinceLastRaid = 0;
        private int _raidInterval = 5; // Default raid every 5 turns
        #endregion

        #region Game State Properties
        // Public access to current game state
        public GameStateBase CurrentState => _currentState;
        public int CurrentTurn => _currentTurn;
        public TimePeriod CurrentTimePeriod => _currentTimePeriod;
        public GameStateType CurrentStateType => _currentState?.StateType ?? GameStateType.None;

        // Events
        public event Action<GameStateType> OnStateChanged;
        public event Action<int> OnTurnChanged;
        public event Action<TimePeriod> OnTimePeriodChanged;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeGameStates();
        }

        private void Start()
        {
            // Start the game at turn 1, morning
            _currentTurn = 1;
            _currentTimePeriod = TimePeriod.Morning;

            // Start with the TurnStart state
            ChangeState(GameStateType.TurnStart);

            // Notify listeners that turn 1 has started
            OnTurnChanged?.Invoke(_currentTurn);
            OnTimePeriodChanged?.Invoke(_currentTimePeriod);
        }
        #endregion

        #region State Management
        private void InitializeGameStates()
        {
            // Initialize all possible game states
            _turnStartState = new TurnStartState(this);
            _playerActionSelectionState = new PlayerActionSelectionState(this);
            _dungeonManagementState = new DungeonManagementState(this);
            _factionNegotiationState = new FactionNegotiationState(this);
            _buildingConstructionState = new BuildingConstructionState(this);
            _resourceGatheringState = new ResourceGatheringState(this);
            _eventInteractionState = new EventInteractionState(this);
            _advanceTimeState = new AdvanceTimeState(this);
            _combatDefenseState = new CombatDefenseState(this);
            _turnEndState = new TurnEndState(this);
        }

        /// <summary>
        /// Changes the current game state
        /// </summary>
        public void ChangeState(GameStateType newStateType)
        {
            // Exit the current state if one exists
            _currentState?.OnExit();

            // Find the new state
            _currentState = newStateType switch
            {
                GameStateType.TurnStart => _turnStartState,
                GameStateType.PlayerActionSelection => _playerActionSelectionState,
                GameStateType.DungeonManagement => _dungeonManagementState,
                GameStateType.FactionNegotiation => _factionNegotiationState,
                GameStateType.BuildingConstruction => _buildingConstructionState,
                GameStateType.ResourceGathering => _resourceGatheringState,
                GameStateType.EventInteraction => _eventInteractionState,
                GameStateType.AdvanceTime => _advanceTimeState,
                GameStateType.CombatDefense => _combatDefenseState,
                GameStateType.TurnEnd => _turnEndState,
                _ => throw new ArgumentOutOfRangeException(nameof(newStateType), $"Unexpected state type: {newStateType}")
            };

            // Enter the new state
            _currentState.OnEnter();

            // Notify listeners about the state change
            OnStateChanged?.Invoke(newStateType);

            Debug.Log($"Game State changed to: {newStateType}");
        }

        /// <summary>
        /// Advance to the next time period
        /// </summary>
        public void AdvanceTimePeriod()
        {
            // Get the next time period
            _currentTimePeriod = _currentTimePeriod switch
            {
                TimePeriod.Morning => TimePeriod.Afternoon,
                TimePeriod.Afternoon => TimePeriod.Evening,
                TimePeriod.Evening => TimePeriod.Night,
                TimePeriod.Night => TimePeriod.Morning, // Should not happen normally as night -> new turn
                _ => throw new ArgumentOutOfRangeException()
            };

            Debug.Log($"Advanced to {_currentTimePeriod}");
            OnTimePeriodChanged?.Invoke(_currentTimePeriod);

            // If we've advanced to morning, we need to start a new turn
            // (This should not happen through this method, but is a safeguard)
            if (_currentTimePeriod == TimePeriod.Morning)
            {
                AdvanceToNextTurn();
            }
        }

        /// <summary>
        /// Check if a raid should occur
        /// </summary>
        public bool ShouldRaidOccur()
        {
            // Placeholder logic - to be expanded later
            // For now, simple interval-based system
            _turnsSinceLastRaid++;

            if (_turnsSinceLastRaid >= _raidInterval)
            {
                _turnsSinceLastRaid = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Advances to the next turn
        /// </summary>
        public void AdvanceToNextTurn()
        {
            _currentTurn++;
            _currentTimePeriod = TimePeriod.Morning;

            OnTurnChanged?.Invoke(_currentTurn);
            OnTimePeriodChanged?.Invoke(_currentTimePeriod);

            Debug.Log($"Advancing to Turn {_currentTurn}, {_currentTimePeriod}");

            // Start the new turn
            ChangeState(GameStateType.TurnStart);
        }

        /// <summary>
        /// Completes the current free action and returns to action selection
        /// </summary>
        public void CompleteActionAndReturnToSelection()
        {
            ChangeState(GameStateType.PlayerActionSelection);
        }
    }

    /// <summary>
    /// Enum representing all possible game states
    /// </summary>
    public enum GameStateType
    {
        None,
        TurnStart,
        PlayerActionSelection,
        DungeonManagement,
        FactionNegotiation,
        BuildingConstruction,
        ResourceGathering,
        EventInteraction,
        AdvanceTime,
        CombatDefense,
        TurnEnd
    }

    /// <summary>
    /// Enum representing the time periods in a turn
    /// </summary>
    public enum TimePeriod
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }

    /// <summary>
    /// Base class for all game states
    /// </summary>
    public abstract class GameStateBase
    {
        protected GameLoopManager GameManager;

        public abstract GameStateType StateType { get; }

        protected GameStateBase(GameLoopManager gameManager)
        {
            GameManager = gameManager;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
    }

    #region State Implementations
    public class TurnStartState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.TurnStart;

        public TurnStartState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Turn Start: Initializing new turn...");

            // Perform start-of-turn calculations or setup
            // For example, reset action points, process ongoing effects, etc.

            // Automatically transition to the player action selection
            GameManager.ChangeState(GameStateType.PlayerActionSelection);
        }
    }

    public class PlayerActionSelectionState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.PlayerActionSelection;

        public PlayerActionSelectionState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log($"Player Action Selection: {GameManager.CurrentTimePeriod}");

            // This state waits for player to select their next action
            // UI should display available actions based on current time period
        }
    }

    public class DungeonManagementState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.DungeonManagement;

        public DungeonManagementState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Dungeon Management: Manage minions and facilities");

            // This is a free action, so player will return to action selection after completion
            // UI would display dungeon management options
        }
    }

    public class FactionNegotiationState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.FactionNegotiation;

        public FactionNegotiationState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Faction Negotiation: Engage in diplomacy");

            // This is a free action, so player will return to action selection after completion
            // UI would display faction negotiation options
        }
    }

    public class BuildingConstructionState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.BuildingConstruction;

        public BuildingConstructionState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Building Construction: Plan and start construction projects");

            // Starting construction doesn't advance time, but progress happens when time advances
            // UI would display construction options
        }
    }

    public class ResourceGatheringState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.ResourceGathering;

        public ResourceGatheringState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Resource Gathering: Assign minions to gathering tasks");

            // Assigning gatherers doesn't advance time, rewards come when time advances
            // UI would display resource gathering options
        }
    }

    public class EventInteractionState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.EventInteraction;

        public EventInteractionState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Event Interaction: Handle triggered events");

            // Events consume time when resolved
            // UI would display the event and choices
        }
    }

    public class AdvanceTimeState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.AdvanceTime;

        public AdvanceTimeState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log($"Advance Time: Moving from {GameManager.CurrentTimePeriod} to next period");

            // Process all ongoing activities
            ProcessOngoingActivities();

            // Check for random events
            if (ShouldTriggerEvent())
            {
                GameManager.ChangeState(GameStateType.EventInteraction);
                return;
            }

            // Check for raid
            if (GameManager.ShouldRaidOccur())
            {
                GameManager.ChangeState(GameStateType.CombatDefense);
                return;
            }

            // Advance the time period
            AdvanceToNextTimePeriod();
        }

        private void ProcessOngoingActivities()
        {
            // Process construction progress
            // Process resource gathering results
            // Process any other time-dependent activities

            Debug.Log("Processing time-dependent activities...");
        }

        private bool ShouldTriggerEvent()
        {
            // Placeholder - determine if a random event should occur
            // This would be based on game rules, current situation, etc.
            return UnityEngine.Random.value < 0.2f; // 20% chance for testing
        }

        private void AdvanceToNextTimePeriod()
        {
            TimePeriod currentPeriod = GameManager.CurrentTimePeriod;

            switch (currentPeriod)
            {
                case TimePeriod.Morning:
                    GameManager.AdvanceTimePeriod(); // To Afternoon
                    GameManager.ChangeState(GameStateType.PlayerActionSelection);
                    break;
                case TimePeriod.Afternoon:
                    GameManager.AdvanceTimePeriod(); // To Evening
                    GameManager.ChangeState(GameStateType.PlayerActionSelection);
                    break;
                case TimePeriod.Evening:
                    GameManager.AdvanceTimePeriod(); // To Night
                    GameManager.ChangeState(GameStateType.PlayerActionSelection);
                    break;
                case TimePeriod.Night:
                    // End of the day, move to turn end
                    GameManager.ChangeState(GameStateType.TurnEnd);
                    break;
            }
        }
    }

    public class CombatDefenseState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.CombatDefense;

        public CombatDefenseState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Combat Defense: Prepare for raid");

            // Initialize the combat system
            // This would transition to a turn-based tactical combat sequence
        }
    }

    public class TurnEndState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.TurnEnd;

        public TurnEndState(GameLoopManager gameManager) : base(gameManager) { }

        public override void OnEnter()
        {
            Debug.Log("Turn End: Finalizing turn and preparing for the next one");

            // Perform end-of-turn cleanup and calculations
            // For example, generate resources, apply ongoing effects, etc.

            // Save the game state
            SaveGameState();

            // Advance to the next turn automatically
            GameManager.AdvanceToNextTurn();
        }

        private void SaveGameState()
        {
            // Placeholder for save game functionality
            Debug.Log($"Game state saved at the end of turn {GameManager.CurrentTurn}");
        }
    }
    #endregion
}
#endregion