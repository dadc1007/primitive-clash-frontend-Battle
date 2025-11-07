using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    private readonly Dictionary<string, GameObject> _entities = new();
    private readonly Dictionary<string, HealthBar> _healthBars = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EntityManager] ⚠️ Instancia duplicada eliminada.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(string id, GameObject obj)
    {
        if (string.IsNullOrEmpty(id) || obj == null)
        {
            Debug.LogWarning(
                $"⚠️ [EntityManager.Register] ID inválido o objeto nulo. Obj: {obj?.name}"
            );
            return;
        }

        if (!_entities.TryAdd(id, obj))
        {
            Debug.LogWarning(
                $"⚠️ [EntityManager.Register] Entidad con ID {id} ya registrada. Se ignorará duplicado."
            );
            return;
        }

        Debug.Log(
            $"🧱 [EntityManager.Register] Registrada entidad id={id}, name={obj.name}, pos={obj.transform.position}. Total={_entities.Count}"
        );

        // Inicializar HealthBar si existe
        HealthBar hb = obj.GetComponentInChildren<HealthBar>();
        if (hb == null)
            return;
        hb.Initialize(obj.transform);
        hb.SetHealth(1f);
        _healthBars[id] = hb;
        Debug.Log($"🩸 [EntityManager.Register] HealthBar vinculada a id={id}");
    }

    public GameObject Get(string id) =>
        string.IsNullOrEmpty(id) ? null : _entities.GetValueOrDefault(id);

    public bool TryGetHealthBar(string id, out HealthBar hb)
    {
        hb = null;
        return !string.IsNullOrEmpty(id) && _healthBars.TryGetValue(id, out hb);
    }

    public void Move(string id, Vector3 targetPos)
    {
        if (!_entities.TryGetValue(id, out GameObject obj))
            return;
        StartCoroutine(SmoothMove(obj, targetPos, 1f));
    }

    private static IEnumerator SmoothMove(GameObject obj, Vector3 targetPos, float duration)
    {
        if (!obj)
            yield break;

        Vector3 start = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!obj)
                yield break;

            Vector3 newPos = Vector3.Lerp(start, targetPos, elapsed / duration);
            Vector3 direction = (targetPos - newPos).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                obj.transform.rotation = Quaternion.Slerp(
                    obj.transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
            obj.transform.position = newPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj)
            obj.transform.position = targetPos;
    }

    public void ShowHit(string id)
    {
        if (!_entities.TryGetValue(id, out GameObject obj))
            return;

        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer == null)
            return;
        renderer.material.color = Color.red;
        StartCoroutine(RestoreColor(renderer));
        Debug.Log($"💥 [EntityManager.ShowHit] Efecto de daño aplicado en id={id}");
    }

    private static IEnumerator RestoreColor(Renderer renderer)
    {
        yield return new WaitForSeconds(0.2f);
        if (renderer)
            renderer.material.color = Color.white;
    }

    public void Remove(string id)
    {
        if (!_entities.TryGetValue(id, out GameObject obj))
        {
            Debug.LogWarning($"⚠️ [EntityManager.Remove] Tropas no encontrada: {id}");
            return;
        }

        Debug.Log(
            $"🗑️ [EntityManager.Remove] Eliminando tropa id={id}, nombre={obj.name}, pos={obj.transform.position}"
        );

        // HealthBar
        if (_healthBars.TryGetValue(id, out var hb))
        {
            if (hb != null)
                Destroy(hb.gameObject);
            _healthBars.Remove(id);
            Debug.Log($"🩸 [EntityManager.Remove] HealthBar eliminada para id={id}");
        }

        Destroy(obj);
        _entities.Remove(id);

        Debug.Log(
            $"✅ [EntityManager.Remove] Tropa eliminada correctamente. Total restante: {_entities.Count}"
        );
    }

    public void ClearAll()
    {
        foreach (GameObject obj in _entities.Values.Where(obj => obj != null))
        {
            Destroy(obj);
        }
        _entities.Clear();

        foreach (HealthBar hb in _healthBars.Values.Where(hb => hb != null))
        {
            Destroy(hb.gameObject);
        }
        _healthBars.Clear();

        Debug.Log("[EntityManager.ClearAll] Todas las entidades y healthbars eliminadas.");
    }
}
