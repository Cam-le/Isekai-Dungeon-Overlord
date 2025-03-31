using System;
using UnityEngine;
using IDM.Core.Interfaces;
using IDM.Core.Events;

namespace IDM.Core.Adapters
{
    /// <summary>
    /// Adapter to make the existing GameLoopManager compatible with the IGameStateManager interface
    /// while preserving existing functionality.
    /// </summary>
    [RequireComponent(typeof(GameLoopManager))]
    public class GameLoopManagerAdapter : MonoBehaviour, IGameStateManager
    {
        private GameLoopManager _gameLoopManager;
        private GameStateType _previousState;
        private TimePeriod _previousTimePeriod;
        private int _previousTurn;

        // IGameStateManager properties
        public GameStateType CurrentStateType => _gameLoopManager.CurrentStateType;
        public int CurrentTurn => _gameLoopManager.CurrentTurn;
        public TimePeriod CurrentTimePeriod => _gameLoopManager.CurrentTimePeriod;

        // IGameStateManager events (pass-through to existing GameLoopManager events)
        public event Action<GameStateType> OnStateChanged;
        public event Action<int> OnTurnChanged;
        public event Action<TimePeriod> OnTimePeriodChanged;

        private void Awake()
        {
            _gameLoopManager = GetComponent<GameLoopManager>();

            if (_gameLoopManager == null)
            {
                Debug.LogError("GameLoopManagerAdapter requires a GameLoopManager component!");
                return;
            }

            // Store initial state
            _previousState = _gameLoopManager.CurrentStateType;
            _previousTimePeriod = _gameLoopManager.CurrentTimePeriod;
            _previousTurn = _gameLoopManager.CurrentTurn;

            // Register this adapter with the ServiceLocator
            ServiceLocator.Instance.RegisterService<IGameStateManager>(this);
        }

        private void OnEnable()
        {
            // Subscribe to GameLoopManager events
            if (_gameLoopManager != null)
            {
                _gameLoopManager.OnStateChanged += HandleStateChanged;
                _gameLoopManager.OnTurnChanged += HandleTurnChanged;
                _gameLoopManager.OnTimePeriodChanged += HandleTimePeriodChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from GameLoopManager events
            if (_gameLoopManager != null)
            {
                _gameLoopManager.OnStateChanged -= HandleStateChanged;
                _gameLoopManager.OnTurnChanged -= HandleTurnChanged;
                _gameLoopManager.OnTimePeriodChanged -= HandleTimePeriodChanged;
            }
        }

        // Event handlers that forward events and also publish typed events
        private void HandleStateChanged(GameStateType newState)
        {
            // Forward the event
            OnStateChanged?.Invoke(newState);

            // Publish a typed event
            GameStateChangedEvent typedEvent = new GameStateChangedEvent(newState, _previousState);
            TypedEventBus.Instance.Publish(typedEvent);

            // Update previous state
            _previousState = newState;
        }

        private void HandleTurnChanged(int newTurn)
        {
            // Forward the event
            OnTurnChanged?.Invoke(newTurn);

            // Publish a typed event
            TurnChangedEvent typedEvent = new TurnChangedEvent(newTurn, _previousTurn);
            TypedEventBus.Instance.Publish(typedEvent);

            // Update previous turn
            _previousTurn = newTurn;
        }

        private void HandleTimePeriodChanged(TimePeriod newTimePeriod)
        {
            // Forward the event
            OnTimePeriodChanged?.Invoke(newTimePeriod);

            // Publish a typed event
            TimePeriodChangedEvent typedEvent = new TimePeriodChangedEvent(newTimePeriod, _previousTimePeriod);
            TypedEventBus.Instance.Publish(typedEvent);

            // Update previous time period
            _previousTimePeriod = newTimePeriod;
        }

        // IGameStateManager methods (forward to GameLoopManager)
        public void ChangeState(GameStateType newState)
        {
            _gameLoopManager.ChangeState(newState);
        }

        public void AdvanceTimePeriod()
        {
            _gameLoopManager.AdvanceTimePeriod();
        }

        public void AdvanceToNextTurn()
        {
            _gameLoopManager.AdvanceToNextTurn();
        }

        public void CompleteActionAndReturnToSelection()
        {
            _gameLoopManager.CompleteActionAndReturnToSelection();
        }

        public bool ShouldRaidOccur()
        {
            return _gameLoopManager.ShouldRaidOccur();
        }
    }
}