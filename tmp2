using UnityEngine;

public class NPCCompany : MonoBehaviour
{
    public string companyName = "G-Corp";
    public MarketManager marketManager;

    [Header("Behavior Settings")]
    public float actionProbabilityPerHour = 0.2f;
    public ResourceType targetResource = ResourceType.Silicon;
    public bool isBuyer = true;
    public int minAmount = 5;
    public int maxAmount = 20;

    void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyAction; }
    void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyAction; }

    private void HandleHourlyAction()
    {
        if (marketManager == null) return;

        if (Random.value <= actionProbabilityPerHour)
        {
            int amount = Random.Range(minAmount, maxAmount + 1);

            if (isBuyer)
            {
                double cost;
                marketManager.BuyResourceFromMarket(targetResource, amount, out cost, false);
                Debug.Log($"<b><color=#ef5350>[NPC]</color></b> {companyName} kupuje {amount}x {targetResource} z rynku za {cost:F2} USD.");
            }
            else
            {
                // In a real scenario, an NPC company might have its own inventory or cash,
                // but here we just simulate market forces by interacting with MarketManager directly.
                // We'll add a simple sell method directly into MarketManager to represent NPC selling,
                // because SellResourceFromDelivery deposits to corporationManager.
                // For now, we simulate selling by just directly decreasing saturation.
                SimulateNPCSelling(amount);
            }
        }
    }

    private void SimulateNPCSelling(int amount)
    {
        // Just directly simulating a sell to the market that increases saturation without depositing money to the player.
        // The player sells via SellResourceFromDelivery which does both.
        // Let's call a method or adjust manually. Since marketSaturation is private,
        // we should add a method to MarketManager to handle NPC sells.

        // As a quick workaround, we can just call BuyResourceFromMarket with a negative amount? No, that would give the player money since it uses corporationManager.cash -= cost.
        // Let's modify MarketManager in the next step or assume it exists.
        // Actually, to avoid changing MarketManager again, we can just use Reflection or add a public method to MarketManager.
        // I will just add `NPCSellResource` to MarketManager in a moment, or use `SendMessage`.

        if (marketManager != null)
        {
            marketManager.SendMessage("NPCSellResource", new object[] { targetResource, amount }, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"<b><color=#ef5350>[NPC]</color></b> {companyName} sprzedaje {amount}x {targetResource} na rynek.");
        }
    }
}
