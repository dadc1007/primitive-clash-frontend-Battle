using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField]
    private Image fillImage;

    [Header("Configuración")]
    [SerializeField]
    private Vector3 offset = new(0f, 2f, 0f);

    private Transform _target;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (fillImage == null)
            Debug.LogWarning("[HealthBar] ⚠️ No se asignó la imagen de relleno.");
    }

    public void Initialize(Transform followTarget)
    {
        _target = followTarget;
    }

    private void LateUpdate()
    {
        if (_target == null || _mainCamera == null)
            return;

        // Seguir a la tropa
        transform.position = _target.position + offset;

        // Mirar siempre hacia la cámara (solo rota en eje Y si prefieres estilo 2.5D)
        transform.forward = _mainCamera.transform.forward;
    }

    public void SetHealth(float normalizedValue)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = Mathf.Clamp01(normalizedValue);
    }
}
