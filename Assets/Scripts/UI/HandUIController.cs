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
            if (img != null && (!img || img.sprite == null)) continue;

            RectTransform rt = img.GetComponent<RectTransform>();
            if (!rt) continue;
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

        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        RectTransform rt = target.GetComponent<RectTransform>();
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                Vector2.one * 0.5f
            );
            target.sprite = sprite;
            target.preserveAspect = true;
            target.color = Color.white;
            if (rt)
            {
                rt.sizeDelta = rt.sizeDelta;
            }
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

        int replacedIndex = ReemplazarCartaJugada(data.cardToPut);
        ActualizarIdsCartasDrag(replacedIndex);

        StartCoroutine(
            RenderHandAsync(
                new HandNotification { hand = _manoActual.ToArray(), nextCard = data.nextCard }
            )
        );

        if (elixirBar != null)
            elixirBar.SetElixir(data.elixir);

        LimpiarReferencias();
    }

    private int ReemplazarCartaJugada(PlayerCardNotification nuevaCarta)
    {
        if (ultimoIndiceJugado >= 0 && ultimoIndiceJugado < _manoActual.Count)
        {
            _manoActual[ultimoIndiceJugado] = nuevaCarta;
            return ultimoIndiceJugado;
        }

        int index = _manoActual.FindIndex(c => c.playerCardId == ultimaCartaUsadaId);
        if (index != -1)
        {
            _manoActual[index] = nuevaCarta;
            return index;
        }

        _manoActual.Add(nuevaCarta);
        return -1;
    }

    private void ActualizarIdsCartasDrag(int replacedIndex)
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
}
