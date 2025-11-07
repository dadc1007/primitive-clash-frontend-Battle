using System.Collections.Generic;
using UnityEngine;

public class ArenaEntitySpawner : MonoBehaviour
{
    [Header("Prefabs de Unidades")]
    public GameObject cavemanPrefab;
    public GameObject lancerPrefab;
    public GameObject miniDinoPrefab;
    public GameObject miniDragonPrefab;
    public GameObject prehistoricDragonPrefab;
    public GameObject rockGolemPrefab;
    public GameObject pterodactylPrefab;
    public GameObject warriorCavemanPrefab;

    private readonly Dictionary<string, GameObject> _prefabs = new();
    private readonly Dictionary<string, float> _yOffsets = new()
    {
        {"6ce2da92-188d-4458-b9a3-8f4e7ae02864", 1.7f},
        {"1a4b313f-5dff-4163-8dc5-eb2bf2df0a41", 2.4f},
        {"8b76c8c0-614f-4783-84f5-a9965eb01093", 0.9f},
        {"fe53ae3f-9575-48e0-847e-ad675cb51e7b", 2.0f},
        {"14a6debd-dc5a-42c6-a46a-ba2f137c6a50", 4.4f},
        {"0a1ae662-f5a3-4826-ab65-42d37b997154", 1.0f},
        {"29a0c77e-54da-40ec-afbb-cdd5449fd40f", 3.4f},
        {"6b75ea3b-a3b5-4ec6-8a38-31ca352bee55", 1.1f}
    };
    
    
    private const float OffsetX = -48f;
    private const float OffsetZ = 41f;
    private const float CellSizeX = 1.1f;
    private const float CellSizeZ = 0.94f;
    

    private void Awake()
    {
        _prefabs["6ce2da92-188d-4458-b9a3-8f4e7ae02864"] = cavemanPrefab;
        _prefabs["1a4b313f-5dff-4163-8dc5-eb2bf2df0a41"] = lancerPrefab;
        _prefabs["8b76c8c0-614f-4783-84f5-a9965eb01093"] = miniDinoPrefab;
        _prefabs["fe53ae3f-9575-48e0-847e-ad675cb51e7b"] = miniDragonPrefab;
        _prefabs["14a6debd-dc5a-42c6-a46a-ba2f137c6a50"] = prehistoricDragonPrefab;
        _prefabs["0a1ae662-f5a3-4826-ab65-42d37b997154"] = rockGolemPrefab;
        _prefabs["29a0c77e-54da-40ec-afbb-cdd5449fd40f"] = pterodactylPrefab;
        _prefabs["6b75ea3b-a3b5-4ec6-8a38-31ca352bee55"] = warriorCavemanPrefab;
    }

    public GameObject SpawnEntity(string id, int fila, int columna)
    {
        if (!_prefabs.TryGetValue(id, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"⚠️ [SpawnEntity] Prefab no encontrado o nulo para ID={id}");
            return null;
        }

        Vector3 position = GridToWorld(id, fila, columna);
        GameObject instance = Instantiate(prefab, position, prefab.transform.rotation);

        if (_yOffsets.TryGetValue(id, out float yOffset))
            instance.transform.position += Vector3.up * yOffset;

        Debug.Log($"✅ Spawned '{prefab.name}' (ID={id}) en {instance.transform.position}");
        return instance;
    }

    public Vector3 GridToWorld(string cardId, int fila, int columna)
    {
        float x = OffsetX + (fila * CellSizeX);
        float z = OffsetZ + (columna * CellSizeZ);
        float y = _yOffsets.GetValueOrDefault(cardId, 0f);
        return new Vector3(x, y, z);
    }
}