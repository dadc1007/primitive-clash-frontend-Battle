using System;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class GameClient : MonoBehaviour
{
    private HubConnection _conn;
    private Guid _sessionId,
        _userId;
    private SynchronizationContext _unityContext;

    public event Action<string> OnHand;
    public event Action<string> OnError;

    public bool isLeftSide = true;

    private void Awake() => _unityContext = SynchronizationContext.Current;

    public async void Connect(Guid sessionId, Guid userId, string hubUrl)
    {
        _sessionId = sessionId;
        _userId = userId;

        _conn = new HubConnectionBuilder().WithUrl(hubUrl).WithAutomaticReconnect().Build();

        ArenaEntitySpawner spawner = FindFirstObjectByType<ArenaEntitySpawner>();

        _conn.On<object>(
            "JoinedToGame",
            obj =>
            {
                Debug.LogWarning("JoinedToGame recibido");

                if (obj == null)
                {
                    Debug.LogWarning("⚠️ JoinedToGame llegó vacío");
                    return;
                }

                string json = SerializeIndented(obj);

                try
                {
                    JoinedToGameNotification data = JsonUtility.FromJson<JoinedToGameNotification>(
                        json
                    );

                    RunOnMainThread(() =>
                    {
                        foreach (TowerNotification tower in data.arena.towers)
                            SpawnAndRegisterTower(spawner, tower);

                        foreach (CardSpawnedNotification entity in data.arena.entities)
                            SpawnAndRegisterEntity(spawner, entity);

                        var player = data.players.Find(p => p.id == _userId.ToString());
                        if (player != null)
                            FindFirstObjectByType<ElixirBarController>()
                                ?.SetElixir(player.currentElixir);
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando CardSpawned: {ex}");
                }
            }
        );

        _conn.On<object>(
            "CardSpawned",
            obj =>
            {
                if (obj == null)
                {
                    Debug.LogWarning("⚠️ CardSpawned llegó vacío");
                    return;
                }

                string json = SerializeIndented(obj);

                try
                {
                    CardSpawnedNotification data = JsonUtility.FromJson<CardSpawnedNotification>(
                        json
                    );

                    RunOnMainThread(() => SpawnAndRegisterEntity(spawner, data));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando CardSpawned: {ex}");
                }
            }
        );

        _conn.On<object>(
            "RefreshHand",
            obj =>
            {
                if (obj == null)
                {
                    Debug.LogWarning("⚠️ RefreshHand llegó vacío");
                    return;
                }

                string json = SerializeIndented(obj);

                try
                {
                    RefreshHandNotification data = JsonUtility.FromJson<RefreshHandNotification>(
                        json
                    );

                    RunOnMainThread(() =>
                    {
                        HandUIController handUI = FindFirstObjectByType<HandUIController>();
                        if (handUI == null)
                        {
                            Debug.LogError("❌ No se encontró un HandUIController activo");
                            return;
                        }
                        handUI.OnRefreshHandReceived(data);
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando RefreshHand: {ex}");
                }
            }
        );

        _conn.On<object>(
            "TroopMoved",
            obj =>
            {
                string json = SerializeIndented(obj);

                Debug.Log($"TroopMoved recibido: {obj}");

                try
                {
                    TroopMovedNotification data = JsonUtility.FromJson<TroopMovedNotification>(
                        json
                    );

                    RunOnMainThread(() =>
                    {
                        Vector3 pos = spawner.GridToWorld(data.cardId, data.y, data.x);
                        EntityManager.Instance.Move(data.troopId, pos);
                        Debug.Log(
                            $"🚶‍♂️ Tropa movida → troopId={data.troopId}, grid=({data.x},{data.y}), pos={pos}"
                        );
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando TroopMoved: {ex}");
                }
            }
        );

        _conn.On<object>(
            "UnitDamaged",
            obj =>
            {
                string json = SerializeIndented(obj);

                try
                {
                    UnitDamagedNotification data = JsonUtility.FromJson<UnitDamagedNotification>(
                        json
                    );
                    Debug.Log(
                        $"💥 Daño → target={data.targetId}, dmg={data.damage}, health={data.health}/{data.maxHealth}"
                    );

                    RunOnMainThread(() =>
                    {
                        GameObject go = EntityManager.Instance.Get(data.targetId);
                        if (go == null)
                        {
                            Debug.LogWarning($"⚠️ No se encontró unidad {data.targetId}");
                            return;
                        }

                        EntityManager.Instance.ShowHit(data.targetId);

                        if (
                            !EntityManager.Instance.TryGetHealthBar(data.targetId, out HealthBar hb)
                        )
                            return;
                        float normalized =
                            data.maxHealth > 0f ? (float)data.health / data.maxHealth : 0f;
                        hb.SetHealth(normalized);
                        Debug.Log(
                            $"🩸 HealthBar actualizada para {data.targetId} → {normalized:P0}"
                        );
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando UnitDamaged: {ex}");
                }
            }
        );

        _conn.On<object>(
            "UnitKilled",
            obj =>
            {
                string json = SerializeIndented(obj);

                try
                {
                    UnitKilledNotificacion data = JsonUtility.FromJson<UnitKilledNotificacion>(
                        json
                    );
                    RunOnMainThread(() => EntityManager.Instance.Remove(data.targetId));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥 Error procesando Unitkilled: {ex}");
                }
            }
        );

        _conn.On<float>(
            "NewElixir",
            e =>
            {
                Debug.Log("💧NewElixir del jugador recibida (desde SignalR):\n" + e);
                RunOnMainThread(() => FindFirstObjectByType<ElixirBarController>().SetElixir(e));
            }
        );

        _conn.On<object>(
            "EndGame",
            obj =>
            {
                string json = SerializeIndented(obj);

                Debug.Log("EndGame del jugador recibida (desde SignalR):\n" + json);

                EndGameNotification data = JsonUtility.FromJson<EndGameNotification>(json);

                RunOnMainThread(() =>
                {
                    int ownTowers,
                        rivalTowers;

                    if (data.winnerId == _userId.ToString())
                    {
                        ownTowers = data.towersWinner;
                        rivalTowers = data.towersLosser;
                    }
                    else
                    {
                        ownTowers = data.towersLosser;
                        rivalTowers = data.towersWinner;
                    }

                    UIManager.Instance.ShowEndGame(
                        data.winnerId,
                        _userId.ToString(),
                        ownTowers,
                        rivalTowers
                    );
                });
            }
        );

        _conn.On<string>("Error", msg => Debug.LogError("🔴 Error recibido → " + msg));

        _conn.On<object>(
            "Hand",
            handNotification =>
            {
                try
                {
                    string json = SerializeIndented(handNotification);

                    RunOnMainThread(() =>
                    {
                        var handUI = FindFirstObjectByType<HandUIController>();
                        if (handUI == null)
                        {
                            Debug.LogError("❌ No se encontró un HandUIController activo");
                            return;
                        }

                        Debug.Log("✅ HandUIController encontrado → actualizando UI");
                        handUI.OnHandReceived(json);
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"💥  Excepción dentro del callback 'Hand': {ex}");
                }
            }
        );

        await _conn.StartAsync();
        Debug.Log($"🔗  Conexión establecida. ConnectionId = {_conn.ConnectionId}");

        await System.Threading.Tasks.Task.Delay(500);
        await _conn.InvokeAsync("JoinGame", _sessionId, _userId);
        Debug.Log("✅ Conectado al GameHub");
    }

    public async void SpawnCard(Guid cardId, int x, int y)
    {
        Debug.Log($"🚀 Enviando SpawnCard → cardId={cardId}, pos=({x},{y})");

        try
        {
            await _conn.InvokeAsync("SpawnCard", _sessionId, _userId, cardId, x, y);
            Debug.Log("✅ SpawnCard enviado correctamente");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al enviar spawn: {ex.Message}");
        }
    }

    private static string SerializeIndented(object obj) =>
        System.Text.Json.JsonSerializer.Serialize(
            obj,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );

    private void RunOnMainThread(Action action) => _unityContext.Post(_ => action?.Invoke(), null);

    [System.Serializable]
    public class CardSpawnedDto
    {
        public string name;
        public int x;
        public int y;
    }

    private void SpawnAndRegisterTower(ArenaEntitySpawner spawner, TowerNotification tower)
    {
        GameObject go = spawner?.SpawnEntity(tower.towerTemplateId, tower.y, tower.x);
        if (go == null)
        {
            Debug.LogError($"❌ No se pudo instanciar prefab para TowerId={tower.id}");
            return;
        }

        EntityManager.Instance.Register(tower.id, go, tower.health, tower.maxHealth);
        Debug.Log($"✅ Torre registrada: TowerId={tower.id} ({go.name}) en ({tower.x},{tower.y})");
    }

    private void SpawnAndRegisterEntity(ArenaEntitySpawner spawner, CardSpawnedNotification entity)
    {
        GameObject go = spawner?.SpawnEntity(entity.cardPlayedId, entity.x, entity.y);
        if (go == null)
        {
            Debug.LogError(
                $"❌ No se pudo instanciar prefab para cardPlayedId={entity.cardPlayedId}"
            );
            return;
        }

        EntityManager.Instance.Register(entity.unitId, go, entity.health, entity.maxHealth);
        Debug.Log(
            $"✅ Tropa registrada: unitId={entity.unitId} ({go.name}) en ({entity.x},{entity.y})"
        );
    }
}
