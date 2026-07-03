using UnityEngine;

public class MonsterHuntPlayerHealth : MonoBehaviour
{
    public int baseMaxHealth = 8;
    public float invincibleDuration = 0.9f;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsInvincible { get { return invincibleTimer > 0f; } }

    private float invincibleTimer;

    private void Awake()
    {
        MaxHealth = baseMaxHealth;
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        invincibleTimer = Mathf.Max(0f, invincibleTimer - Time.deltaTime);
    }

    public void ApplyLevel(int level)
    {
        int oldMax = MaxHealth;
        MaxHealth = baseMaxHealth + Mathf.Max(0, level - 1) * 2;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + Mathf.Max(0, MaxHealth - oldMax));
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsInvincible)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invincibleTimer = invincibleDuration;

        if (CurrentHealth <= 0 && MonsterHuntGameManager.Instance != null)
        {
            MonsterHuntGameManager.Instance.GameOver();
        }
    }
}
