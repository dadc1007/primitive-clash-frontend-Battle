using System;
using System.Collections.Generic;

[Serializable]
public class CardSpawnedNotification
{
    public string unitId;
    public string userId;
    public string cardPlayedId;
    public int level;
    public int x;
    public int y;
    public int health;
    public int maxHealth;
}

[Serializable]
public class EndGameNotification
{
    public string winnerId;
    public string losserId;
    public int towersWinner;
    public int towersLosser;
}

[Serializable]
public class TroopMovedNotification
{
    public string troopId;
    public string playerId;
    public string cardId;
    public int x;
    public int y;
    public string state;
}

[Serializable]
public class UnitDamagedNotification
{
    public string attackerId;
    public string targetId;
    public int damage;
    public int health;
    public int maxHealth;
}

[Serializable]
public class UnitKilledNotificacion
{
    public string attackerId;
    public string targetId;
}

[Serializable]
public class PlayerCardNotification
{
    public string playerId;
    public string playerCardId;
    public string cardId;
    public int elixir;
    public string imageUrl;
}

[Serializable]
public class HandNotification
{
    public PlayerCardNotification[] hand;
    public PlayerCardNotification nextCard;
}

[Serializable]
public class RefreshHandNotification
{
    public PlayerCardNotification cardToPut;
    public PlayerCardNotification nextCard;
    public float elixir;
}

[Serializable]
public class JoinedToGameNotification
{
    public string gameId;
    public GameState state;
    public List<PlayerStateNotification> players;
    public ArenaNotification arena;
}

public enum GameState
{
    InProgress,
    Finished,
}

[Serializable]
public class PlayerStateNotification
{
    public string id;
    public bool isConnected;
    public string connectionId;
    public float currentElixir;
}

[Serializable]
public class ArenaNotification
{
    public Guid Id;
    public ArenaTemplate arenaTemplate;
    public List<TowerNotification> towers;
    public List<CardSpawnedNotification> entities;
}

[Serializable]
public class ArenaTemplate
{
    public string id;
    public string name;
    public int requiredTrophies;
}

[Serializable]
public class TowerNotification
{
    public string id;
    public string towerTemplateId;
    public string userId;
    public TowerType type;
    public int health;
    public int maxHealth;
    public int x;
    public int y;
}

public enum TowerType
{
    Leader,
    Guardian,
}

[Serializable]
public class JoinGameData
{
    public string sessionId;
    public string userId;
}

[Serializable]
public class SpawnCardData
{
    public string sessionId;
    public string userId;
    public string cardId;
    public int x;
    public int y;
}