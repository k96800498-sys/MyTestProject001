using System;

public enum HuntZone
{
    Easy,
    Normal,
    Hard,
    Boss
}

[Serializable]
public class MonsterData
{
    public string monsterId;
    public string name;
    public HuntZone zone;
    public int maxHp;
    public int contactDamage;
    public float moveSpeed;
    public float chaseRange;
    public float headbuttDamageMultiplier;
    public float stompDamageMultiplier;
    public int expReward;
    public int goldReward;
    public int spawnCount;
    public bool isBoss;
}
