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
    public GameObject guardianTower;
    public GameObject leaderTower;

    private readonly Dictionary<string, GameObject> _prefabs = new();
    private readonly Dictionary<string, float> _yOffsets = new()
    {
        { "6ce2da92-188d-4458-b9a3-8f4e7ae02864", 1.7f },
        { "1a4b313f-5dff-4163-8dc5-eb2bf2df0a41", 2.4f },
        { "8b76c8c0-614f-4783-84f5-a9965eb01093", 1.3f },
        { "fe53ae3f-9575-48e0-847e-ad675cb51e7b", 3.5f },
        { "14a6debd-dc5a-42c6-a46a-ba2f137c6a50", 4.4f },
        { "0a1ae662-f5a3-4826-ab65-42d37b997154", 1.0f },
        { "29a0c77e-54da-40ec-afbb-cdd5449fd40f", 3.4f },
        { "6b75ea3b-a3b5-4ec6-8a38-31ca352bee55", 1.1f },
        { "dc719076-4eea-4ec8-9d49-732d440cb27f", 0.8f },
        { "4e718199-d25b-4f0f-88e2-221a43eb5dc6", 0.8f },
    };

    // Offsets de pivot para cada tipo de entidad (X, Z)
    private readonly Dictionary<string, Vector2> _pivotOffsets = new()
    {
        { "6ce2da92-188d-4458-b9a3-8f4e7ae02864", new Vector2(0, 0) },
        { "1a4b313f-5dff-4163-8dc5-eb2bf2df0a41", new Vector2(0, 0) },
        { "8b76c8c0-614f-4783-84f5-a9965eb01093", new Vector2(0, 0) },
        { "fe53ae3f-9575-48e0-847e-ad675cb51e7b", new Vector2(0, 0) },
        { "14a6debd-dc5a-42c6-a46a-ba2f137c6a50", new Vector2(0, 0) },
        { "0a1ae662-f5a3-4826-ab65-42d37b997154", new Vector2(0, 0) },
        { "29a0c77e-54da-40ec-afbb-cdd5449fd40f", new Vector2(0, 0) },
        { "6b75ea3b-a3b5-4ec6-8a38-31ca352bee55", new Vector2(0, 0) },
        { "dc719076-4eea-4ec8-9d49-732d440cb27f", new Vector2(0.5f, 0.6f) },
        { "4e718199-d25b-4f0f-88e2-221a43eb5dc6", new Vector2(0.5f, 1.05f) },
    };

    // Dimensiones del backend
    private const int BACKEND_ROWS = 30;
    private const int BACKEND_COLS = 18;

    // Coordenadas base (usando las de las tropas)
    private const float CORNER_0_0_X = -47.9f;
    private const float CORNER_0_0_Z = 41.79f;
    private const float CORNER_0_17_X = -48.81f;
    private const float CORNER_0_17_Z = 57.48f;
    private const float CORNER_29_0_X = -14.98f;
    private const float CORNER_29_0_Z = 41.86f;

    private float CellSizeX => (CORNER_29_0_X - CORNER_0_0_X) / BACKEND_ROWS;
    private float CellSizeZ => (CORNER_0_17_Z - CORNER_0_0_Z) / (BACKEND_COLS - 1);

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
        _prefabs["dc719076-4eea-4ec8-9d49-732d440cb27f"] = guardianTower;
        _prefabs["4e718199-d25b-4f0f-88e2-221a43eb5dc6"] = leaderTower;
    }

    public GameObject SpawnEntity(string id, int fila, int columna)
    {
        if (!_prefabs.TryGetValue(id, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"⚠️ [SpawnEntity] Prefab no encontrado o nulo para ID={id}");
            return null;
        }

        Vector3 position = GridToWorld(id, fila, columna);
        GameObject instance = Instantiate(prefab, position, prefab.transform.rotation);

        if (_yOffsets.TryGetValue(id, out float yOffset))
            instance.transform.position += Vector3.up * yOffset;

        if (IsTower(id) && fila > BACKEND_ROWS / 2)
        {
            instance.transform.Rotate(0f, 180f, 0f);
        }

        return instance;
    }

    public Vector3 GridToWorld(string cardId, int fila, int columna)
    {
        float baseX = CORNER_0_0_X + (fila * CellSizeX);
        float baseZ = CORNER_0_0_Z + (columna * CellSizeZ);

        Vector2 pivotOffset = _pivotOffsets.GetValueOrDefault(cardId, Vector2.zero);
        float x = baseX + pivotOffset.x;
        float z = baseZ + pivotOffset.y;
        float y = _yOffsets.GetValueOrDefault(cardId, 0f);

        return new Vector3(x, y, z);
    }

    private static bool IsTower(string id)
    {
        return id == "dc719076-4eea-4ec8-9d49-732d440cb27f"
            || 
            id == "4e718199-d25b-4f0f-88e2-221a43eb5dc6"; 
    }
}
