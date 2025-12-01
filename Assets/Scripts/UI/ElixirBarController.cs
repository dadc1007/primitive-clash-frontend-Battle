using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElixirBarController : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image relleno;
    public TextMeshProUGUI texto;

    [Header("Configuración")]
    [SerializeField]
    private float elixirMax = 10f;

    [SerializeField]
    private float elixirInicial = 5f;

    private float _elixirActual;
    private int _ultimoElixirEntero = -1;

    private void Start()
    {
        // Si ya se actualizó el elixir (por ejemplo, desde la red antes de Start), no sobrescribir con el inicial
        if (_ultimoElixirEntero == -1)
        {
            SetElixir(elixirInicial);
        }
    }

    public void SetElixir(float valor)
    {
        _elixirActual = Mathf.Clamp(valor, 0f, elixirMax);
        int elixirEntero = Mathf.FloorToInt(_elixirActual);
        Debug.Log($"⚡ SetElixir llamado: valor={valor}, clamped={_elixirActual}, entero={elixirEntero}");

        // Solo actualizar si el número entero cambió
        if (elixirEntero == _ultimoElixirEntero)
        {
            Debug.Log($"⏭️ Elixir sin cambios ({elixirEntero}), saltando actualización UI");
            return;
        }

        _ultimoElixirEntero = elixirEntero;
        float porcentaje = _elixirActual / elixirMax;
        Debug.Log($"📊 Actualizando UI: porcentaje={porcentaje:F2}, texto='{elixirEntero}/{(int)elixirMax}'");

        if (relleno)
            relleno.fillAmount = porcentaje;
        else
            Debug.LogWarning("[ElixirBarController] ⚠️ No se asignó el relleno (Image).");

        if (texto)
            texto.text = $"{elixirEntero}/{(int)elixirMax}";
        else
            Debug.LogWarning("[ElixirBarController] ⚠️ No se asignó el texto (TMP).");
    }
}
