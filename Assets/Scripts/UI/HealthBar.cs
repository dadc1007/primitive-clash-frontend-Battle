using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField]
    private Image fillImage;

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
        if (!_target || !_mainCamera)
            return;

        // Mirar siempre hacia la cámara
        transform.forward = _mainCamera.transform.forward;
    }

    public void SetHealth(float normalizedValue)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = Mathf.Clamp01(normalizedValue);
    }
}
