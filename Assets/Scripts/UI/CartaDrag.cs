using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CartaDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas _canvas;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private GameClient _gameClient;
    private HandUIController _handUI;

    private Vector2 _startPosition;

    [HideInInspector]
    public System.Guid cardId;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();

        if (!TryGetComponent(out _canvasGroup))
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _gameClient = FindFirstObjectByType<GameClient>();
        _handUI = FindFirstObjectByType<HandUIController>();

        Debug.Log(
            $"[CartaDrag] 🧩 Inicializado para carta {name} (cardId={cardId}). Canvas: {_canvas != null}, GameClient: {_gameClient != null}"
        );
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = _rectTransform.anchoredPosition;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        Debug.Log($"[CartaDrag] 🛑 Fin de drag de '{name}'. Realizando raycast...");

        if (!TryGetWorldPointFromMouse(out Vector3 hitPoint))
        {
            Debug.LogWarning("[CartaDrag] ⚠️ Raycast no golpeó ninguna superficie válida.");
            _rectTransform.anchoredPosition = _startPosition;
            return;
        }

        Debug.Log($"[CartaDrag] ☄️ Raycast hit en {hitPoint}");
        Vector2Int gridPos = ConvertUnityToBackend(hitPoint);
        Debug.Log($"[CartaDrag] 🧮 Coordenadas backend = ({gridPos.x},{gridPos.y})");

        if (!PuedeJugarEn(gridPos))
        {
            Debug.LogWarning(
                $"[CartaDrag] ❌ No puedes jugar en ({gridPos.x},{gridPos.y}), está fuera de tu lado."
            );
            _rectTransform.anchoredPosition = _startPosition;
            return;
        }

        Debug.Log(
            $"[CartaDrag] ✅ Posición válida. Enviando SpawnCard({cardId}, {gridPos.x}, {gridPos.y})..."
        );

        // Guarda la carta usada antes de enviar
        RegistrarCartaJugada();

        _gameClient.SpawnCard(cardId, gridPos.x, gridPos.y);
        _rectTransform.anchoredPosition = _startPosition;
    }

    private static bool TryGetWorldPointFromMouse(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        Ray ray = Camera.main!.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out RaycastHit hit) && (hitPoint = hit.point) != Vector3.zero;
    }

    private static Vector2Int ConvertUnityToBackend(Vector3 world)
    {
        const float offsetX = 48f,
            offsetZ = 41f;
        const float cellSizeX = 1.1f,
            cellSizeZ = 0.94f; // 30x18 grid

        int fila = Mathf.Clamp(Mathf.RoundToInt((world.x + offsetX) / cellSizeX), 0, 29);
        int columna = Mathf.Clamp(Mathf.RoundToInt((world.z - offsetZ) / cellSizeZ), 0, 17);
        return new Vector2Int(fila, columna);
    }

    private bool PuedeJugarEn(Vector2Int gridPos)
    {
        if (_gameClient == null)
            return false;

        bool soyJugadorIzquierda = _gameClient.isLeftSide;
        // Lado izquierdo: filas 0–14 | Lado derecho: filas 15–29
        return soyJugadorIzquierda ? gridPos.x < 15 : gridPos.x >= 15;
    }

    private void RegistrarCartaJugada()
    {
        if (_handUI == null)
        {
            Debug.LogWarning("[CartaDrag] ⚠️ No hay referencia a HandUIController.");
            return;
        }

        int index = _handUI.GetCardIndexById(cardId);
        if (index == -1)
        {
            Debug.LogWarning(
                $"[CartaDrag] ⚠️ No se encontró la carta con id={cardId} en la mano actual."
            );
            return;
        }

        _handUI.ultimoIndiceJugado = index;
        _handUI.ultimaCartaUsadaId = cardId.ToString();
        Debug.Log($"[CartaDrag] 💾 Guardado índice jugado={index}, id={cardId}");
    }
}
