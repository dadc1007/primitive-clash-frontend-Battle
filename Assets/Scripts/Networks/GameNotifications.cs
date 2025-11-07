using System;

[Serializable]
public class CardSpawnedNotification
{
    public string unitId;
    public string userId;
    public string cardPlayedId;
    public int level;
    public int x;
    public int y;
    public string nextCardId;
}

[Serializable]
public class EndGameNotification
{
    public Guid winnerId;
    public Guid losserId;
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