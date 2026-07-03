using System.Globalization;
using UnityEngine;

public class MonsterHuntPlayerProgress : MonoBehaviour
{
    private static readonly int[] RequiredExpByLevel = { 0, 50, 120, 220, 360, 540, 760, 1020, 1320, 1660 };

    public int Level { get; private set; }
    public int CurrentExp { get; private set; }
    public int Gold { get; private set; }
    public int KillCount { get; private set; }

    private void Awake()
    {
        Level = 1;
    }

    public void AddReward(int exp, int gold)
    {
        CurrentExp += Mathf.Max(0, exp);
        Gold += Mathf.Max(0, gold);
        KillCount++;
        TryLevelUp();
    }

    private void TryLevelUp()
    {
        while (Level < 10 && CurrentExp >= RequiredExpByLevel[Level])
        {
            Level++;
            MonsterHuntPlayerHealth health = GetComponent<MonsterHuntPlayerHealth>();
            if (health != null)
            {
                health.ApplyLevel(Level);
            }
        }
    }

    public string GetNextLevelExpLabel()
    {
        if (Level >= 10)
        {
            return "MAX";
        }

        return RequiredExpByLevel[Level].ToString(CultureInfo.InvariantCulture);
    }
}
