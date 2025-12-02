using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerNames : MonoBehaviour
{
    [Header("Textos de UI")]
    [SerializeField] private TextMeshProUGUI leftPlayer;
    [SerializeField] private TextMeshProUGUI rightPlayer;

    [Header("Canvas Padres")]
    [SerializeField] private Canvas leftPlayerCanvas;
    [SerializeField] private Canvas rightPlayerCanvas;

    public void SetPlayerNames(string topPlayerName, string bottomPlayerName)
    {
        StartCoroutine(UpdatePlayerNames(topPlayerName, bottomPlayerName));
    }

    private IEnumerator UpdatePlayerNames(string topPlayerName, string bottomPlayerName)
    {
        if (leftPlayer != null)
        {
            leftPlayer.text = topPlayerName;
            leftPlayer.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(leftPlayer.rectTransform);

            // Forzar rebuild del padre que tiene Content Size Fitter
            RectTransform parent = leftPlayer.transform.parent as RectTransform;
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }
        }
        else
            Debug.LogWarning("⚠️ leftPlayer (Top) no está asignado");

        if (rightPlayer != null)
        {
            rightPlayer.text = bottomPlayerName;
            rightPlayer.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rightPlayer.rectTransform);

            // Forzar rebuild del padre que tiene Content Size Fitter
            RectTransform parent = rightPlayer.transform.parent as RectTransform;
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }
        }
        else
            Debug.LogWarning("⚠️ rightPlayer (Bottom) no está asignado");

        yield return null;

        // Segunda pasada para asegurar que todo se actualizó
        if (leftPlayer != null)
        {
            RectTransform parent = leftPlayer.transform.parent as RectTransform;
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }
        }

        if (rightPlayer != null)
        {
            RectTransform parent = rightPlayer.transform.parent as RectTransform;
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }
        }

        Canvas.ForceUpdateCanvases();
    }
}