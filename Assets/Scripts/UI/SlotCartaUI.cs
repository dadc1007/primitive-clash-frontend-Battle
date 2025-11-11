using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotCartaUI : MonoBehaviour
{
    [Header("Elementos del Elixir")]
    public Image background; // El círculo
    public TMP_Text quantityText; // El texto del costo de elixir

    public void SetElixirCost(int cost)
    {
        if (quantityText != null)
            quantityText.text = cost.ToString();
    }
}
