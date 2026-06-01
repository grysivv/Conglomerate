import re

with open("Assets/Scripts/FleetManager.cs", "r") as f:
    content = f.read()

# Zmienić FieldInfo z lokalnej w Update na prywatną w klasie
replacement = """
    private SpriteRenderer spriteRenderer;
    private FieldInfo speedMultiplierField;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.color = Color.yellow;
        speedMultiplierField = typeof(TimeManager).GetField("currentSpeedMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
    }
"""

content = content.replace("""
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.color = Color.yellow;
    }
""", replacement)

update_logic = """
        if (isEnRoute)
        {
            // Pobranie currentSpeedMultiplier przez refleksję, by uwzględnić przyspieszenie gry
            float speedMultiplier = 1.0f;
            if (speedMultiplierField != null)
            {
                speedMultiplier = (float)speedMultiplierField.GetValue(timeManager);
            }
"""

content = content.replace("""
        if (isEnRoute)
        {
            // Pobranie currentSpeedMultiplier przez refleksję, by uwzględnić przyspieszenie gry
            float speedMultiplier = 1.0f;
            FieldInfo field = typeof(TimeManager).GetField("currentSpeedMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                speedMultiplier = (float)field.GetValue(timeManager);
            }
""", update_logic)

with open("Assets/Scripts/FleetManager.cs", "w") as f:
    f.write(content)
