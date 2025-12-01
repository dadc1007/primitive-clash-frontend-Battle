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

        if (!TryGetWorldPointFromMouse(out Vector3 hitPoint))
        {
            Debug.LogWarning("[CartaDrag] ⚠️ Raycast no golpeó ninguna superficie válida.");
            _rectTransform.anchoredPosition = _startPosition;
            return;
        }

        Vector2Int gridPos = ConvertUnityToBackend(hitPoint);

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
        // Usar las MISMAS coordenadas que ArenaEntitySpawner
        const int BACKEND_ROWS = 30;
        const int BACKEND_COLS = 18;
    
        // Coordenadas base (tropas)
        const float CORNER_0_0_X = -47.9f;
        const float CORNER_0_0_Z = 41.79f;
        const float CORNER_0_17_Z = 57.48f;
        const float CORNER_29_0_X = -14.98f;
    
        float cellSizeX = (CORNER_29_0_X - CORNER_0_0_X) / BACKEND_ROWS;
        float cellSizeZ = (CORNER_0_17_Z - CORNER_0_0_Z) / (BACKEND_COLS - 1);
    
        float deltaX = world.x - CORNER_0_0_X;
        float deltaZ = world.z - CORNER_0_0_Z;
        float filaFloat = deltaX / cellSizeX;
        float columnaFloat = deltaZ / cellSizeZ;
    
        // Conversión inversa
        int fila = Mathf.RoundToInt(filaFloat);
        int columna = Mathf.RoundToInt(columnaFloat);
        
        // Clamp para asegurar que esté dentro de los límites
        fila = Mathf.Clamp(fila, 0, BACKEND_ROWS - 1);
        columna = Mathf.Clamp(columna, 0, BACKEND_COLS - 1);
        
        return new Vector2Int(columna, fila);
    }

    // private bool PuedeJugarEn(Vector2Int gridPos)
    // {
    //     if (_gameClient == null)
    //         return false;
    //
    //     bool soyJugadorIzquierda = _gameClient.isLeftSide;
    //     // Lado izquierdo: filas 0–14 | Lado derecho: filas 15–29
    //     return soyJugadorIzquierda ? gridPos.x < 15 : gridPos.x >= 15;
    // }

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
    }
}
