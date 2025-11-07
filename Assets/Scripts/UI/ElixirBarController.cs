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

    private void Start()
    {
        SetElixir(elixirInicial);
    }

    public void SetElixir(float valor)
    {
        _elixirActual = Mathf.Clamp(valor, 0f, elixirMax);
        float porcentaje = _elixirActual / elixirMax;

        if (relleno)
            relleno.fillAmount = porcentaje;
        else
            Debug.LogWarning("[ElixirBarController] ⚠️ No se asignó el relleno (Image).");

        if (texto)
            texto.text = $"{_elixirActual:0}/{elixirMax:0}";
        else
            Debug.LogWarning("[ElixirBarController] ⚠️ No se asignó el texto (TMP).");
    }
}
