using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
#endif

public class GameClient : MonoBehaviour
{
    private Guid _sessionId;
    private Guid _userId;
    private SynchronizationContext _unityContext;
    private ArenaEntitySpawner _spawner;

#if !UNITY_WEBGL || UNITY_EDITOR
    private HubConnection _conn; // Solo para Standalone/Editor
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void StartSignalR(string hubUrl, string token);

    [DllImport("__Internal")]
    private static extern void StopSignalR();

    [DllImport("__Internal")]
    private static extern void InvokeServer(string methodName, string argsJson);
#endif

    private void Awake()
    {
        _unityContext = SynchronizationContext.Current;
        _spawner = FindFirstObjectByType<ArenaEntitySpawner>();
    }

    public async void Connect(Guid sessionId, Guid userId, string token, string hubUrl)
    {
        _sessionId = sessionId;
        _userId = userId;

#if UNITY_WEBGL && !UNITY_EDITOR
        StartSignalR(hubUrl, token); // JS maneja SignalR
#else
        await ConnectStandaloneCSharp(hubUrl, token); // Cliente C# para Standalone/Editor
#endif
    }

    // Este método será llamado desde JS cuando SignalR se conecte
    public void OnSignalRConnected(string _)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var joinData = new JoinGameData { sessionId = _sessionId.ToString() };
        string args = JsonUtility.ToJson(joinData);
        InvokeServer("JoinGame", args);
#endif
    }

    public void SpawnCard(Guid cardId, int x, int y)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var spawnData = new SpawnCardData
        {
            sessionId = _sessionId.ToString(),
            cardId = cardId.ToString(),
            x = x,
            y = y,
        };
        string args = JsonUtility.ToJson(spawnData);
        InvokeServer("SpawnCard", args);
#else
        _conn?.InvokeAsync("SpawnCard", _sessionId, cardId, x, y);
#endif
    }

    // Cliente C# Standalone
#if !UNITY_WEBGL || UNITY_EDITOR
    private async Task ConnectStandaloneCSharp(string hubUrl, string token)
    {
        _conn = new HubConnectionBuilder()
            .WithUrl(
                hubUrl,
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                }
            )
            .WithAutomaticReconnect()
            .Build();

        // Registro de eventos
        _conn.On<object>("JoinedToGame", JoinedToGame);
        _conn.On<object>("CardSpawned", CardSpawned);
        _conn.On<object>("RefreshHand", RefreshHand);
        _conn.On<object>("TroopMoved", TroopMoved);
        _conn.On<object>("UnitDamaged", UnitDamaged);
        _conn.On<object>("UnitKilled", UnitKilled);
        _conn.On<float>("NewElixir", NewElixir);
        _conn.On<object>("EndGame", EndGame);
        _conn.On<object>("Hand", Hand);
        _conn.On<string>("Error", OnServerError);

        await _conn.StartAsync();
        await _conn.InvokeAsync("JoinGame", _sessionId);
    }
#endif

    // Métodos llamados desde JS o C#
    public void JoinedToGame(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ JoinedToGame llegó vacío");
            return;
        }

        string json = obj.ToString();
        Debug.Log($"📦 JoinedToGame objeto recibido: {json}");

        try
        {
            JoinedToGameNotification data = JsonUtility.FromJson<JoinedToGameNotification>(json);

            RunOnMainThread(() =>
            {
                foreach (TowerNotification tower in data.arena.towers)
                    SpawnAndRegisterTower(_spawner, tower);

                foreach (CardSpawnedNotification entity in data.arena.entities)
                    SpawnAndRegisterEntity(_spawner, entity);

                PlayerStateNotification player = data.players.Find(p => p.id == _userId.ToString());
                if (player != null)
                {
                    Debug.Log($"💎 Jugador encontrado, elixir inicial: {player.currentElixir}");
                    FindFirstObjectByType<ElixirBarController>()?.SetElixir(player.currentElixir);
                }
                else
                {
                    Debug.LogWarning($"⚠️ No se encontró jugador con ID: {_userId}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥 Error procesando CardSpawned: {ex}");
        }
    }

    public void CardSpawned(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ CardSpawned llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            CardSpawnedNotification data = JsonUtility.FromJson<CardSpawnedNotification>(json);

            RunOnMainThread(() => SpawnAndRegisterEntity(_spawner, data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥 Error procesando CardSpawned: {ex}");
        }
    }

    public void RefreshHand(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ RefreshHand llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            RefreshHandNotification data = JsonUtility.FromJson<RefreshHandNotification>(json);

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

    public void TroopMoved(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ TroopMoved llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            TroopMovedNotification data = JsonUtility.FromJson<TroopMovedNotification>(json);

            RunOnMainThread(() =>
            {
                Vector3 pos = _spawner.GridToWorld(data.cardId, data.y, data.x);
                EntityManager.Instance.Move(data.troopId, pos);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥 Error procesando TroopMoved: {ex}");
        }
    }

    public void UnitDamaged(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ UnitDamaged llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            UnitDamagedNotification data = JsonUtility.FromJson<UnitDamagedNotification>(json);

            RunOnMainThread(() =>
            {
                GameObject go = EntityManager.Instance.Get(data.targetId);
                if (go == null)
                {
                    Debug.LogWarning($"⚠️ No se encontró unidad {data.targetId}");
                    return;
                }

                EntityManager.Instance.ShowHit(data.targetId);

                if (!EntityManager.Instance.TryGetHealthBar(data.targetId, out HealthBar hb))
                    return;
                float normalized = data.maxHealth > 0f ? (float)data.health / data.maxHealth : 0f;
                hb.SetHealth(normalized);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥 Error procesando UnitDamaged: {ex}");
        }
    }

    public void UnitKilled(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ UnitKilled llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            UnitKilledNotificacion data = JsonUtility.FromJson<UnitKilledNotificacion>(json);
            RunOnMainThread(() => EntityManager.Instance.Remove(data.targetId));
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥 Error procesando Unitkilled: {ex}");
        }
    }

    public void NewElixir(float value)
    {
        RunOnMainThread(() => FindFirstObjectByType<ElixirBarController>().SetElixir(value));
    }

    public void NewElixir(string valueStr)
    {
        float value = float.Parse(valueStr);
        Debug.Log($"⚡ NewElixir recibido: {value}");
        RunOnMainThread(() => FindFirstObjectByType<ElixirBarController>().SetElixir(value));
    }

    public void EndGame(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ EndGame llegó vacío");
            return;
        }

        string json = obj.ToString();

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

#if UNITY_WEBGL && !UNITY_EDITOR
            StopSignalR();
#endif
        });
    }

    public static void OnServerError(string message)
    {
        Debug.LogError("🔴 Error recibido desde SignalR → " + message);
    }

    public void Hand(object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ Hand llegó vacío");
            return;
        }

        string json = obj.ToString();

        try
        {
            RunOnMainThread(() =>
            {
                HandUIController handUI = FindFirstObjectByType<HandUIController>();
                if (handUI == null)
                {
                    Debug.LogError("❌ No se encontró un HandUIController activo");
                    return;
                }

                handUI.OnHandReceived(json);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"💥  Excepción dentro del callback 'Hand': {ex}");
        }
    }

    private void RunOnMainThread(Action action)
    {
        if (SynchronizationContext.Current == _unityContext)
            action?.Invoke();
        else
            _unityContext.Post(_ => action?.Invoke(), null);
    }

    [System.Serializable]
    public class CardSpawnedDto
    {
        public string name;
        public int x;
        public int y;
    }

    private static void SpawnAndRegisterTower(ArenaEntitySpawner spawner, TowerNotification tower)
    {
        GameObject go = spawner?.SpawnEntity(tower.towerTemplateId, tower.y, tower.x);
        if (go == null)
        {
            Debug.LogError($"❌ No se pudo instanciar prefab para TowerId={tower.id}");
            return;
        }

        EntityManager.Instance.Register(tower.id, go, tower.health, tower.maxHealth);
    }

    private static void SpawnAndRegisterEntity(
        ArenaEntitySpawner spawner,
        CardSpawnedNotification entity
    )
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
    }
}
