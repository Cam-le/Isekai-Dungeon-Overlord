using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IDM.Core;
using TMPro;
namespace IDM.UI
{

    /// <summary>
    /// Manages the main game UI elements that visualize the turn structure
    /// </summary>
    public class GameLoopUI : MonoBehaviour
    {
        [Header("Time Period Indicators")]
        [SerializeField] private GameObject timePeriodPanel;
        [SerializeField] private Image morningIndicator;
        [SerializeField] private Image afternoonIndicator;
        [SerializeField] private Image eveningIndicator;
        [SerializeField] private Image nightIndicator;

        [Header("Turn Information")]
        [SerializeField] private TextMeshProUGUI turnCounterText;
        [SerializeField] private TextMeshProUGUI currentTimeText;
        [SerializeField] private TextMeshProUGUI currentStateText;

        [Header("Action Buttons")]
        [SerializeField] private Button dungeonManagementButton;
        [SerializeField] private Button factionNegotiationButton;
        [SerializeField] private Button buildConstructionButton;
        [SerializeField] private Button resourceGatheringButton;
        [SerializeField] private Button advanceTimeButton;
        [SerializeField] private Button endTurnButton;

        [Header("Action Panel")]
        [SerializeField] private GameObject actionButtonsPanel;
        [SerializeField] private GameObject returnToSelectionPanel;
        [SerializeField] private Button returnToSelectionButton;

        // Reference to the game manager
        private GameLoopManager _gameManager;

        // Color settings for time period indicators
        private Color _activeTimeColor = new Color(0.18f, 0.8f, 0.44f); // Green
        private Color _completedTimeColor = new Color(0.2f, 0.2f, 0.8f); // Blue
        private Color _inactiveTimeColor = new Color(0.6f, 0.6f, 0.6f); // Gray

        private void Start()
        {
            // Get reference to the game manager
            _gameManager = GameLoopManager.Instance;

            // Subscribe to events
            _gameManager.OnStateChanged += UpdateStateUI;
            _gameManager.OnTurnChanged += UpdateTurnUI;
            _gameManager.OnTimePeriodChanged += UpdateTimePeriodUI;

            // Set up button listeners for main actions
            dungeonManagementButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.DungeonManagement));
            factionNegotiationButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.FactionNegotiation));
            buildConstructionButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.BuildingConstruction));
            resourceGatheringButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.ResourceGathering));
            advanceTimeButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.AdvanceTime));
            endTurnButton.onClick.AddListener(() => OnActionButtonClicked(GameStateType.TurnEnd));

            // Set up return button
            returnToSelectionButton.onClick.AddListener(OnReturnToSelectionClicked);

            // Initial UI update
            UpdateTurnUI(_gameManager.CurrentTurn);
            UpdateTimePeriodUI(_gameManager.CurrentTimePeriod);
            UpdateStateUI(_gameManager.CurrentStateType);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_gameManager != null)
            {
                _gameManager.OnStateChanged -= UpdateStateUI;
                _gameManager.OnTurnChanged -= UpdateTurnUI;
                _gameManager.OnTimePeriodChanged -= UpdateTimePeriodUI;
            }
        }

        /// <summary>
        /// Updates UI elements based on the current turn
        /// </summary>
        private void UpdateTurnUI(int turnNumber)
        {
            turnCounterText.text = $"Turn {turnNumber}";
        }

        /// <summary>
        /// Updates UI elements based on the current time period
        /// </summary>
        private void UpdateTimePeriodUI(TimePeriod timePeriod)
        {
            // Update the time text
            currentTimeText.text = $"Time: {timePeriod}";

            // Reset all indicators to inactive
            morningIndicator.color = _inactiveTimeColor;
            afternoonIndicator.color = _inactiveTimeColor;
            eveningIndicator.color = _inactiveTimeColor;
            nightIndicator.color = _inactiveTimeColor;

            // Mark completed time periods
            switch (timePeriod)
            {
                case TimePeriod.Morning:
                    morningIndicator.color = _activeTimeColor;
                    break;
                case TimePeriod.Afternoon:
                    morningIndicator.color = _completedTimeColor;
                    afternoonIndicator.color = _activeTimeColor;
                    break;
                case TimePeriod.Evening:
                    morningIndicator.color = _completedTimeColor;
                    afternoonIndicator.color = _completedTimeColor;
                    eveningIndicator.color = _activeTimeColor;
                    break;
                case TimePeriod.Night:
                    morningIndicator.color = _completedTimeColor;
                    afternoonIndicator.color = _completedTimeColor;
                    eveningIndicator.color = _completedTimeColor;
                    nightIndicator.color = _activeTimeColor;
                    break;
            }
        }

        /// <summary>
        /// Updates UI elements based on the current state
        /// </summary>
        private void UpdateStateUI(GameStateType stateType)
        {
            // Update the state text
            currentStateText.text = $"State: {GetStateDisplayName(stateType)}";

            // Show/hide appropriate panels based on state
            bool isActionSelection = stateType == GameStateType.PlayerActionSelection;
            actionButtonsPanel.SetActive(isActionSelection);
            returnToSelectionPanel.SetActive(!isActionSelection &&
                                              stateType != GameStateType.TurnStart &&
                                              stateType != GameStateType.TurnEnd &&
                                              stateType != GameStateType.AdvanceTime);

            // Disable end turn button if it's not night yet
            if (isActionSelection)
            {
                TimePeriod currentTimePeriod = _gameManager.CurrentTimePeriod;
                endTurnButton.interactable = true; // Allow ending turn early
            }
        }

        /// <summary>
        /// Called when an action button is clicked
        /// </summary>
        private void OnActionButtonClicked(GameStateType actionState)
        {
            _gameManager.ChangeState(actionState);
        }

        /// <summary>
        /// Called when the return to selection button is clicked
        /// </summary>
        private void OnReturnToSelectionClicked()
        {
            _gameManager.CompleteActionAndReturnToSelection();
        }

        /// <summary>
        /// Returns a user-friendly display name for the given state type
        /// </summary>
        private string GetStateDisplayName(GameStateType stateType)
        {
            return stateType switch
            {
                GameStateType.TurnStart => "Turn Start",
                GameStateType.PlayerActionSelection => "Select Action",
                GameStateType.DungeonManagement => "Dungeon Management",
                GameStateType.FactionNegotiation => "Faction Negotiations",
                GameStateType.BuildingConstruction => "Construction",
                GameStateType.ResourceGathering => "Resource Gathering",
                GameStateType.EventInteraction => "Event",
                GameStateType.AdvanceTime => "Advancing Time",
                GameStateType.CombatDefense => "Combat",
                GameStateType.TurnEnd => "Turn End",
                _ => stateType.ToString()
            };
        }
    }
}