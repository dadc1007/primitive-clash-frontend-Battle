using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel principal de fin de juego")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Image background; // Fondo negro semitransparente

    [Header("Textos de UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI ownTowersText;
    [SerializeField] private TextMeshProUGUI rivalTowersText;

    private void Awake()
    {
        // Singleton seguro
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIManager] ⚠️ Se destruyó una instancia duplicada.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        endGamePanel?.SetActive(false); // Asegurar que el panel esté oculto al inicio
    }

    public void ShowEndGame(string winnerId, string localPlayerId, int ownTowers, int rivalTowers)
    {
        if (endGamePanel == null || titleText == null || resultText == null)
        {
            Debug.LogWarning("[UIManager] ⚠️ No se asignaron referencias de UI.");
            return;
        }

        endGamePanel.SetActive(true);

        bool hasWon = winnerId == localPlayerId;

        titleText.text = "PARTIDA FINALIZADA";

        Color winColor = new Color(0f, 1f, 0.55f); 
        Color loseColor = new Color(1f, 0.25f, 0.25f);
        Color textColor = hasWon ? winColor : loseColor;

        resultText.text = hasWon ? "¡GANASTE!" : "DERROTA";
        resultText.color = textColor;

        ownTowersText.text = $"Tus Torres: {ownTowers}";
        rivalTowersText.text = $"Torres Rivales: {rivalTowers}";

        if (background != null)
            background.color = new Color(0f, 0f, 0f, 0.7f);
    }

    public void HideEndGame()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);
    }
}
