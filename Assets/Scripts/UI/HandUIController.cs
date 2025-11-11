using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField]
    private Image[] slotImages; // Las 4 cartas visibles

    [SerializeField]
    private Image nextCardImage; // La carta “siguiente”

    [SerializeField]
    private ElixirBarController elixirBar;

    private readonly List<PlayerCardNotification> _manoActual = new();
    private static readonly Dictionary<string, Sprite> spriteCache = new();
    public int ultimoIndiceJugado = -1;
    public string ultimaCartaUsadaId = null;

    private void Start()
    {
        SetHandVisibility(false);
    }

    public void OnHandReceived(string json)
    {
        HandNotification data = JsonUtility.FromJson<HandNotification>(json);
        if (data?.hand == null)
        {
            Debug.LogWarning("[HandUIController] ⚠️ Datos de mano inválidos o vacíos.");
            return;
        }

        _manoActual.Clear();
        _manoActual.AddRange(data.hand);

        AsignarIdsACartasDrag();
        StartCoroutine(RenderHandAsync(data));
    }

    private IEnumerator RenderHandAsync(HandNotification data)
    {
        SetHandVisibility(false);

        List<IEnumerator> loaders = new List<IEnumerator>();
        List<Image> targets = new List<Image>();

        for (int i = 0; i < slotImages.Length && i < data.hand.Length; i++)
        {
            loaders.Add(CargarImagen(data.hand[i].imageUrl, slotImages[i]));
            targets.Add(slotImages[i]);

            SlotCartaUI slotUI = slotImages[i].GetComponent<SlotCartaUI>();
            if (slotUI)
                slotUI.SetElixirCost(data.hand[i].elixir);
            else
                Debug.LogWarning($"⚠️ Slot {slotImages[i].name} no tiene SlotCartaUI asignado.");
        }

        if (data.nextCard != null && nextCardImage != null)
        {
            loaders.Add(CargarImagen(data.nextCard.imageUrl, nextCardImage));
            targets.Add(nextCardImage);
        }

        foreach (IEnumerator loader in loaders)
            StartCoroutine(loader);

        bool allDone = false;
        while (!allDone)
        {
            allDone = targets.All(img => img.sprite != null);
            yield return null;
        }

        AjustarTamañoCartas(targets);
        SetHandVisibility(true);
    }

    private static void AjustarTamañoCartas(List<Image> images)
    {
        if (images.Count == 0)
            return;

        foreach (Image img in images)
        {
            if (img && (!img || !img.sprite))
                continue;

            RectTransform rt = img.GetComponent<RectTransform>();
            if (!rt)
                continue;
            rt.localScale = Vector3.one;
            img.preserveAspect = true;
        }
    }

    private void SetHandVisibility(bool visible)
    {
        foreach (Image img in slotImages)
            if (img)
                img.gameObject.SetActive(visible);

        if (nextCardImage)
            nextCardImage.gameObject.SetActive(visible);
    }

    private static IEnumerator CargarImagen(string url, Image target)
    {
        if (string.IsNullOrWhiteSpace(url) || !target)
            yield break;

        // Si ya está en caché, usarlo directamente
        if (spriteCache.TryGetValue(url, out Sprite cachedSprite))
        {
            target.sprite = cachedSprite;
            target.preserveAspect = true;
            target.color = Color.white;
            yield break;
        }

        // Si no está en caché, descárgalo
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                Vector2.one * 0.5f
            );
            spriteCache[url] = sprite;
            target.sprite = sprite;
            target.preserveAspect = true;
            target.color = Color.white;
        }
        else
        {
            Debug.LogError($"❌ Error al cargar imagen ({url}): {www.error}");
        }
    }

    private void AsignarIdsACartasDrag()
    {
        for (int i = 0; i < slotImages.Length && i < _manoActual.Count; i++)
        {
            if (!Guid.TryParse(_manoActual[i].playerCardId, out Guid realId))
                continue;
            CartaDrag drag = slotImages[i].GetComponent<CartaDrag>();
            if (drag != null)
                drag.cardId = realId;
        }
    }

    public void OnRefreshHandReceived(RefreshHandNotification data)
    {
        if (data?.cardToPut == null || data.nextCard == null)
            return;

        ReemplazarCartaJugada(data.cardToPut);
        ActualizarIdsCartasDrag();

        StartCoroutine(
            RenderHandAsync(
                new HandNotification { hand = _manoActual.ToArray(), nextCard = data.nextCard }
            )
        );

        if (elixirBar != null)
            elixirBar.SetElixir(data.elixir);

        LimpiarReferencias();
    }

    private void ReemplazarCartaJugada(PlayerCardNotification nuevaCarta)
    {
        if (ultimoIndiceJugado >= 0 && ultimoIndiceJugado < _manoActual.Count)
        {
            _manoActual[ultimoIndiceJugado] = nuevaCarta;
            return;
        }

        int index = _manoActual.FindIndex(c => c.playerCardId == ultimaCartaUsadaId);
        if (index != -1)
        {
            _manoActual[index] = nuevaCarta;
            return;
        }

        _manoActual.Add(nuevaCarta);
    }

    private void ActualizarIdsCartasDrag()
    {
        for (int i = 0; i < slotImages.Length && i < _manoActual.Count; i++)
        {
            if (!Guid.TryParse(_manoActual[i].playerCardId, out Guid realId))
                continue;
            CartaDrag drag = slotImages[i].GetComponent<CartaDrag>();
            if (drag != null)
                drag.cardId = realId;
        }
    }

    private void LimpiarReferencias()
    {
        ultimoIndiceJugado = -1;
        ultimaCartaUsadaId = null;
    }

    public int GetCardIndexById(Guid id)
    {
        for (int i = 0; i < _manoActual.Count; i++)
        {
            if (Guid.TryParse(_manoActual[i].playerCardId, out Guid parsed) && parsed == id)
                return i;
        }
        return -1;
    }

    private void ActualizarCostoElixir(Transform slotTransform, int elixirCost)
    {
        // Buscar el objeto "Elixir/Quantity" dentro del slot
        Transform elixirTransform = slotTransform.Find("Elixir/Quantity");
        if (!elixirTransform)
        {
            Debug.LogWarning($"⚠️ No se encontró 'Elixir/Quantity' en {slotTransform.name}");
            return;
        }

        // Puede ser Text o TextMeshProUGUI
        Text text = elixirTransform.GetComponent<Text>();
        if (text)
        {
            text.text = elixirCost.ToString();
            return;
        }

        TMPro.TextMeshProUGUI tmp = elixirTransform.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp)
        {
            tmp.text = elixirCost.ToString();
        }
    }
}
