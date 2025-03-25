using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IDM.Core;

namespace IDM.UI
{
    /// <summary>
    /// Manages the main game UI elements that visualize the time-based turn structure
    /// </summary>
    public class GameLoopUI : MonoBehaviour
    {
        #region UI References
        [Header("Layout Containers")]
        [SerializeField] private RectTransform headerPanel;
        [SerializeField] private RectTransform mainContentPanel;
        [SerializeField] private RectTransform actionBarPanel;

        [Header("Header UI Elements")]
        [SerializeField] private TextMeshProUGUI turnCounterText;
        [SerializeField] private TextMeshProUGUI currentTimeText;
        [SerializeField] private GameObject timeIndicatorsContainer;
        [SerializeField] private Image[] timeIndicators = new Image[4]; // Morning, Afternoon, Evening, Night

        [Header("Resource Display")]
        [SerializeField] private GameObject resourceDisplayContainer;
        [SerializeField] private TextMeshProUGUI dungeonPointsText;
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI stoneText;
        [SerializeField] private TextMeshProUGUI manaText;

        [Header("Action Bar Buttons")]
        [SerializeField] private GameObject actionSelectionButtons;
        [SerializeField] private Button dungeonManagementButton;
        [SerializeField] private Button factionNegotiationButton;
        [SerializeField] private Button buildConstructionButton;
        [SerializeField] private Button resourceGatheringButton;
        [SerializeField] private Button advanceTimeButton;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private GameObject returnButtonContainer;
        [SerializeField] private Button returnToSelectionButton;

        [Header("Main Content Panels")]
        [SerializeField] private GameObject dungeonOverviewPanel;
        [SerializeField] private GameObject dungeonManagementPanel;
        [SerializeField] private GameObject buildingConstructionPanel;
        [SerializeField] private GameObject resourceGatheringPanel;
        [SerializeField] private GameObject factionNegotiationPanel;
        #endregion

        #region Private Variables
        // Reference to the game manager
        private GameLoopManager _gameLoopManager;

        // Color settings for time period indicators
        private Color _activeTimeColor = new Color(0.5f, 0.2f, 0.8f); // Purple
        private Color _completedTimeColor = new Color(0.2f, 0.2f, 0.8f); // Blue
        private Color _inactiveTimeColor = new Color(0.3f, 0.3f, 0.3f); // Dark Gray
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Get reference to the game manager
            _gameLoopManager = GameLoopManager.Instance;

            if (_gameLoopManager == null)
            {
                _gameLoopManager = FindFirstObjectByType<GameLoopManager>();
            }

            // Subscribe to events
            _gameLoopManager.OnStateChanged += UpdateStateUI;
            _gameLoopManager.OnTurnChanged += UpdateTurnUI;
            _gameLoopManager.OnTimePeriodChanged += UpdateTimePeriodUI;

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
            UpdateTurnUI(_gameLoopManager.CurrentTurn);
            UpdateTimePeriodUI(_gameLoopManager.CurrentTimePeriod);
            UpdateStateUI(_gameLoopManager.CurrentStateType);

            // Connect resource system to game loop
            _gameLoopManager.ConnectResourceSystem();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_gameLoopManager != null)
            {
                _gameLoopManager.OnStateChanged -= UpdateStateUI;
                _gameLoopManager.OnTurnChanged -= UpdateTurnUI;
                _gameLoopManager.OnTimePeriodChanged -= UpdateTimePeriodUI;

                // Disconnect to prevent memory leaks
                _gameLoopManager.DisconnectResourceSystem();
            }
        }
        #endregion

        #region UI Update Methods
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
            currentTimeText.text = $"{timePeriod}";

            // Reset all indicators to inactive
            for (int i = 0; i < timeIndicators.Length; i++)
            {
                timeIndicators[i].color = _inactiveTimeColor;
            }

            // Mark completed and current time periods
            int currentIndex = (int)timePeriod;
            for (int i = 0; i < timeIndicators.Length; i++)
            {
                if (i < currentIndex)
                {
                    timeIndicators[i].color = _completedTimeColor;
                }
                else if (i == currentIndex)
                {
                    timeIndicators[i].color = _activeTimeColor;
                }
            }
        }

        /// <summary>
        /// Updates UI elements based on the current state
        /// </summary>
        private void UpdateStateUI(GameStateType stateType)
        {
            // Hide all content panels first
            dungeonOverviewPanel.SetActive(false);
            dungeonManagementPanel.SetActive(false);
            buildingConstructionPanel.SetActive(false);
            resourceGatheringPanel.SetActive(false);
            factionNegotiationPanel.SetActive(false);

            // Show/hide action buttons based on state
            bool isActionSelection = stateType == GameStateType.PlayerActionSelection;
            actionSelectionButtons.SetActive(isActionSelection);
            returnButtonContainer.SetActive(!isActionSelection);

            // Show the appropriate content panel based on state
            switch (stateType)
            {
                case GameStateType.PlayerActionSelection:
                    dungeonOverviewPanel.SetActive(true);
                    break;
                case GameStateType.DungeonManagement:
                    dungeonManagementPanel.SetActive(true);
                    break;
                case GameStateType.BuildingConstruction:
                    buildingConstructionPanel.SetActive(true);
                    break;
                case GameStateType.ResourceGathering:
                    resourceGatheringPanel.SetActive(true);
                    break;
                case GameStateType.FactionNegotiation:
                    factionNegotiationPanel.SetActive(true);
                    break;
                    // Add other states as needed
            }
        }


        #endregion

        #region Button Event Handlers
        /// <summary>
        /// Called when an action button is clicked
        /// </summary>
        private void OnActionButtonClicked(GameStateType actionState)
        {
            _gameLoopManager.ChangeState(actionState);
        }

        /// <summary>
        /// Called when the return to selection button is clicked
        /// </summary>
        private void OnReturnToSelectionClicked()
        {
            _gameLoopManager.CompleteActionAndReturnToSelection();
        }
        #endregion

        #region Content Panel Methods (Placeholders)
        /// <summary>
        /// Updates the dungeon overview panel with current game info
        /// </summary>
        public void UpdateDungeonOverview()
        {
            // This will be implemented in a future phase
            // Would show dungeon visualization, active projects, etc.
        }

        /// <summary>
        /// Updates the dungeon management panel
        /// </summary>
        public void UpdateDungeonManagement()
        {
            // This will be implemented in a future phase
            // Would display minion assignments, facilities, etc.
        }

        /// <summary>
        /// Updates the building construction panel
        /// </summary>
        public void UpdateBuildingConstruction()
        {
            // This will be implemented in a future phase
            // Would display available projects, current constructions, etc.
        }

        /// <summary>
        /// Updates the resource gathering panel
        /// </summary>
        public void UpdateResourceGathering()
        {
            // This will be implemented in a future phase
            // Would display gatherer assignments, collection rates, etc.
        }

        /// <summary>
        /// Updates the faction negotiation panel
        /// </summary>
        public void UpdateFactionNegotiation()
        {
            // This will be implemented in a future phase
            // Would display factions, relations, treaties, etc.
        }
        #endregion
    }
}