using System.Collections.Generic;

[System.Serializable]
public class BulletCreateData
{
    public string ID;
    public string PlayerID;
    public int X;
    public int Y;
    public string Direction;
    public long Timestamp;
}

[System.Serializable]
public class BulletDestroyData
{
    public string ID;
    public string PlayerID;
    public int HitX;
    public int HitY;
}

[System.Serializable]
public class PlayerHitData
{
    public string PlayerID;
    public int HP;
    public string HitByID;
}

[System.Serializable]
public class GameStateData
{
    public Dictionary<string, PlayerData> Players;
    public string State;
}

[System.Serializable]
public class PlayerData
{
    public string ID;
    public int X;
    public int Y;
    public string Direction;
    public int HP;
}

[System.Serializable]
public class PlayerMoveData
{
    public string PlayerID;
    public int X;
    public int Y;
    public string Direction;
    public long Timestamp;
}
