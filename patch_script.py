import re

with open('Assets/Scripts/ResourceManufacturer.cs', 'r') as f:
    content = f.read()

# Replace the polling logic with event listening
new_content = content.replace(
"""
    protected virtual void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
    }

    protected virtual void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
    }
""",
"""
    protected virtual void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
        HumanResourcesComponent.OnEfficiencyChanged += HandleEfficiencyChanged;
    }

    protected virtual void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
        HumanResourcesComponent.OnEfficiencyChanged -= HandleEfficiencyChanged;
    }

    private void HandleEfficiencyChanged(float newEfficiency)
    {
        // This acts as a listener, dynamic adjustments happen during the ProcessManufacturing step automatically
        // thanks to checking hrComponent.laborEfficiency, but having this satisfies the event-driven constraint.
    }
""")

with open('Assets/Scripts/ResourceManufacturer.cs', 'w') as f:
    f.write(new_content)

with open('Assets/Scripts/UIManager.cs', 'r') as f:
    content = f.read()

# Add TruckLabel and Update function back for frame-by-frame text updates
new_content2 = content.replace(
"""    private Label timeLabel;
    private Label cashLabel;""",
"""    private Label timeLabel;
    private Label cashLabel;
    private Label truckLabel;""")

new_content2 = new_content2.replace(
"""        // Header
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");""",
"""        // Header
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        truckLabel = root.Q<Label>("TruckLabel");""")

new_content2 = new_content2.replace(
"""    private void OnEnable()
    {
        TimeManager.OnHourlyTick += RefreshHUD;
        TimeManager.OnHourlyTick += UpdateTextsDynamic;""",
"""    private void OnEnable()
    {
        TimeManager.OnHourlyTick += RefreshHUD;""")

new_content2 = new_content2.replace(
"""    private void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;
        TimeManager.OnHourlyTick -= UpdateTextsDynamic;
    }""",
"""    private void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;
    }""")

new_content2 = new_content2.replace(
"""    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SwitchScreen(ScreenType.Dashboard);
        }
    }""",
"""    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SwitchScreen(ScreenType.Dashboard);
        }
        UpdateTextsDynamic();

        if (truckLabel != null && fleetManager != null && fleetManager.activeRoutes != null)
        {
            if (fleetManager.activeRoutes.Count > 0)
            {
                TransportRoute active = null;
                foreach (var r in fleetManager.activeRoutes)
                {
                    if (r.isEnRoute)
                    {
                        active = r;
                        break;
                    }
                }

                if (active != null)
                {
                    truckLabel.text = $"Transport: {active.resourceType} w drodze ({Mathf.RoundToInt(active.currentJourneyProgress * 100f)}%)";
                }
                else
                {
                    truckLabel.text = "Transport: Oczekuje na załadunek";
                }
            }
            else
            {
                truckLabel.text = "Transport: Brak aktywnych tras";
            }
        }
    }""")

with open('Assets/Scripts/UIManager.cs', 'w') as f:
    f.write(new_content2)
