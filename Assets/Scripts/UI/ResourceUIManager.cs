using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IDM.Economy;
using ResourceTypeEnum = IDM.Economy.ResourceType;
using EconomyResourceManager = IDM.Economy.ResourceManager;

namespace IDM.UI
{
    /// <summary>
    /// Manages the UI display of resources and gathering operations
    /// </summary>
    public class ResourceUIManager : MonoBehaviour
    {
        [Header("Resource Displays")]
        [SerializeField] private ResourceDisplay _dungeonPointsDisplay;
        [SerializeField] private ResourceDisplay _woodDisplay;
        [SerializeField] private ResourceDisplay _stoneDisplay;
        [SerializeField] private ResourceDisplay _metalDisplay;
        [SerializeField] private ResourceDisplay _foodDisplay;
        [SerializeField] private ResourceDisplay _knowledgeDisplay;

        [Header("Gatherer Controls")]
        [SerializeField] private GathererControl _woodGathererControl;
        [SerializeField] private GathererControl _stoneGathererControl;
        [SerializeField] private GathererControl _metalGathererControl;
        [SerializeField] private GathererControl _foodGathererControl;
        [SerializeField] private GathererControl _knowledgeGathererControl;

        [Header("Production Summary")]
        [SerializeField] private TextMeshProUGUI _totalGatherersText;
        [SerializeField] private TextMeshProUGUI _totalProductionText;

        private Dictionary<ResourceTypeEnum, ResourceDisplay> _resourceDisplays = new Dictionary<ResourceTypeEnum, ResourceDisplay>();
        private Dictionary<ResourceTypeEnum, GathererControl> _gathererControls = new Dictionary<ResourceTypeEnum, GathererControl>();

        private void Start()
        {
            // Map resource types to UI displays
            _resourceDisplays[ResourceTypeEnum.DungeonPoints] = _dungeonPointsDisplay;
            _resourceDisplays[ResourceTypeEnum.Wood] = _woodDisplay;
            _resourceDisplays[ResourceTypeEnum.Stone] = _stoneDisplay;
            _resourceDisplays[ResourceTypeEnum.Metal] = _metalDisplay;
            _resourceDisplays[ResourceTypeEnum.Food] = _foodDisplay;
            _resourceDisplays[ResourceTypeEnum.Knowledge] = _knowledgeDisplay;

            // Map resource types to gatherer controls
            _gathererControls[ResourceTypeEnum.Wood] = _woodGathererControl;
            _gathererControls[ResourceTypeEnum.Stone] = _stoneGathererControl;
            _gathererControls[ResourceTypeEnum.Metal] = _metalGathererControl;
            _gathererControls[ResourceTypeEnum.Food] = _foodGathererControl;
            _gathererControls[ResourceTypeEnum.Knowledge] = _knowledgeGathererControl;

            // Initialize UI with current values
            UpdateAllResourceDisplays();
            UpdateAllGathererControls();

            // Subscribe to resource events
            EconomyResourceManager resourceManager = EconomyResourceManager.Instance;
            if (resourceManager != null)
            {
                resourceManager.OnResourceChanged += HandleResourceChanged;
                resourceManager.OnGathererChanged += HandleGathererChanged;
                resourceManager.OnResourcesUpdated += HandleAllResourcesUpdated;
            }
            else
            {
                Debug.LogError("ResourceManager not found in scene!");
            }

            // Set up gatherer controls
            InitializeGathererControls();
        }

        private void OnDestroy()
        {
            // Unsubscribe from resource events
            if (EconomyResourceManager.Instance != null)
            {
                EconomyResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
                EconomyResourceManager.Instance.OnGathererChanged -= HandleGathererChanged;
                EconomyResourceManager.Instance.OnResourcesUpdated -= HandleAllResourcesUpdated;
            }
        }

        private void InitializeGathererControls()
        {
            foreach (var control in _gathererControls)
            {
                ResourceType resourceType = control.Key;
                GathererControl gathererControl = control.Value;

                // Set initial values
                gathererControl.ResourceType = resourceType;
                gathererControl.Initialize(
                    EconomyResourceManager.Instance.GetGatherers(resourceType),
                    (int count) => {
                        EconomyResourceManager.Instance.SetGatherers(resourceType, count);
                        UpdateProductionSummary();
                    }
                );

                gathererControl.UpdateProductionRate(EconomyResourceManager.Instance.GetProductionRate(resourceType));
            }

            UpdateProductionSummary();
        }

        // Handle changes to a single resource
        private void HandleResourceChanged(ResourceTypeEnum resourceType, int newValue)
        {
            if (_resourceDisplays.TryGetValue(resourceType, out ResourceDisplay display))
            {
                display.UpdateValue(newValue, EconomyResourceManager.Instance.GetResourceCapacity(resourceType));
            }
        }

        // Handle changes to a gatherer assignment
        private void HandleGathererChanged(ResourceTypeEnum resourceType, int newCount)
        {
            if (_gathererControls.TryGetValue(resourceType, out GathererControl control))
            {
                control.UpdateCount(newCount);
                control.UpdateProductionRate(EconomyResourceManager.Instance.GetProductionRate(resourceType));
            }

            UpdateProductionSummary();
        }

        // Handle updates to all resources
        private void HandleAllResourcesUpdated(Dictionary<ResourceTypeEnum, int> resources)
        {
            UpdateAllResourceDisplays();
        }

        // Update all resource displays
        private void UpdateAllResourceDisplays()
        {
            foreach (ResourceTypeEnum resourceType in _resourceDisplays.Keys)
            {
                if (_resourceDisplays[resourceType] != null)
                {
                    _resourceDisplays[resourceType].UpdateValue(
                        EconomyResourceManager.Instance.GetResource(resourceType),
                        EconomyResourceManager.Instance.GetResourceCapacity(resourceType)
                    );
                }
            }
        }

        // Update all gatherer controls
        private void UpdateAllGathererControls()
        {
            foreach (ResourceTypeEnum resourceType in _gathererControls.Keys)
            {
                if (_gathererControls[resourceType] != null)
                {
                    _gathererControls[resourceType].UpdateCount(EconomyResourceManager.Instance.GetGatherers(resourceType));
                    _gathererControls[resourceType].UpdateProductionRate(EconomyResourceManager.Instance.GetProductionRate(resourceType));
                }
            }

            UpdateProductionSummary();
        }

        // Update the production summary
        private void UpdateProductionSummary()
        {
            int totalGatherers = 0;
            int totalProduction = 0;

            foreach (ResourceTypeEnum resourceType in _gathererControls.Keys)
            {
                totalGatherers += EconomyResourceManager.Instance.GetGatherers(resourceType);
                totalProduction += EconomyResourceManager.Instance.GetProductionRate(resourceType);
            }

            if (_totalGatherersText != null)
                _totalGatherersText.text = totalGatherers.ToString();

            if (_totalProductionText != null)
                _totalProductionText.text = totalProduction.ToString();
        }
    }

    /// <summary>
    /// UI component that displays a resource value and optional capacity
    /// </summary>
    [System.Serializable]
    public class ResourceDisplay
    {
        public TextMeshProUGUI ValueText;
        public TextMeshProUGUI CapacityText;
        public Image FillBar;

        public void UpdateValue(int value, int capacity)
        {
            if (ValueText != null)
                ValueText.text = value.ToString();

            if (capacity < int.MaxValue)
            {
                // Show capacity for resources with limits
                if (CapacityText != null)
                    CapacityText.text = $"/{capacity}";

                if (FillBar != null)
                    FillBar.fillAmount = (float)value / capacity;
            }
            else
            {
                // Hide capacity display for unlimited resources (like DP)
                if (CapacityText != null)
                    CapacityText.text = "";

                if (FillBar != null)
                    FillBar.fillAmount = 0;
            }
        }
    }

    /// <summary>
    /// UI component for adjusting gatherer counts
    /// </summary>
    [System.Serializable]
    public class GathererControl
    {
        public Button DecreaseButton;
        public Button IncreaseButton;
        public TextMeshProUGUI CountText;
        public TextMeshProUGUI ProductionRateText;

        [HideInInspector]
        public ResourceTypeEnum ResourceType;

        private System.Action<int> _onValueChanged;
        private int _currentCount;

        public void Initialize(int startingCount, System.Action<int> onValueChanged)
        {
            _currentCount = startingCount;
            _onValueChanged = onValueChanged;

            UpdateCount(_currentCount);

            if (DecreaseButton != null)
                DecreaseButton.onClick.AddListener(OnDecreaseClicked);

            if (IncreaseButton != null)
                IncreaseButton.onClick.AddListener(OnIncreaseClicked);
        }

        public void UpdateCount(int newCount)
        {
            _currentCount = newCount;

            if (CountText != null)
                CountText.text = _currentCount.ToString();

            // Disable decrease button if count is zero
            if (DecreaseButton != null)
                DecreaseButton.interactable = _currentCount > 0;
        }

        public void UpdateProductionRate(int rate)
        {
            if (ProductionRateText != null)
                ProductionRateText.text = $"+{rate}/period";
        }

        private void OnDecreaseClicked()
        {
            if (_currentCount > 0)
            {
                _currentCount--;
                _onValueChanged?.Invoke(_currentCount);
            }
        }

        private void OnIncreaseClicked()
        {
            _currentCount++;
            _onValueChanged?.Invoke(_currentCount);
        }
    }
}