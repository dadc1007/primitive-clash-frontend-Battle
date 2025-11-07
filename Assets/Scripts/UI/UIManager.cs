using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField]
    private GameObject endGamePanel;

    [SerializeField]
    private TextMeshProUGUI resultadoTexto;

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
        endGamePanel?.SetActive(false); // Asegurar que el panel esté oculto al inicio
    }

    public void ShowEndGame(Guid winnerId)
    {
        if (endGamePanel == null || resultadoTexto == null)
        {
            Debug.LogWarning("[UIManager] ⚠️ No se asignaron referencias de UI.");
            return;
        }

        endGamePanel.SetActive(true);
        resultadoTexto.text = $"¡Partida terminada!\nGanador: {winnerId}";
        Debug.Log($"[UIManager] Panel de fin de juego mostrado → Ganador: {winnerId}");
    }

    public void HideEndGame()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);
    }
}
