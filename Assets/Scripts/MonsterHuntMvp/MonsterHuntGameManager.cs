using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MonsterHuntGameManager : MonoBehaviour
{
    public static MonsterHuntGameManager Instance { get; private set; }

    public MonsterHuntPlayerController player;
    public Transform monsterRoot;
    public GameObject monsterPrefab;
    public GameObject bossPrefab;
    public HuntZone currentZone = HuntZone.Easy;
    public bool gameEnded;
    public bool bossKilled;
    public int remainingMonsters;

    private readonly List<MonsterHuntMonsterController> spawnedMonsters = new List<MonsterHuntMonsterController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (monsterRoot == null)
        {
            monsterRoot = new GameObject("Monsters").transform;
        }

        SpawnFromCsv();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (gameEnded && keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void SetCurrentZone(HuntZone zone)
    {
        currentZone = zone;
    }

    public void GrantMonsterReward(MonsterData data)
    {
        if (player == null || data == null)
        {
            return;
        }

        player.progress.AddReward(data.expReward, data.goldReward);
        remainingMonsters = Mathf.Max(0, remainingMonsters - 1);

        if (data.isBoss)
        {
            bossKilled = true;
            ClearGame();
        }
    }

    public void GameOver()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        if (player != null)
        {
            player.enabled = false;
            player.combat.enabled = false;
        }
    }

    public void ClearGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        if (player != null)
        {
            player.enabled = false;
            player.combat.enabled = false;
        }
    }

    private void SpawnFromCsv()
    {
        List<MonsterData> allData = MonsterCsvLoader.Load();
        remainingMonsters = 0;

        foreach (MonsterData data in allData)
        {
            for (int i = 0; i < data.spawnCount; i++)
            {
                SpawnMonster(data, i);
            }
        }
    }

    private void SpawnMonster(MonsterData data, int index)
    {
        GameObject prefab = data.isBoss && bossPrefab != null ? bossPrefab : monsterPrefab;
        if (prefab == null)
        {
            Debug.LogError("Monster prefab is not assigned.");
            return;
        }

        Vector3 center = GetZoneCenter(data.zone);
        float radius = data.isBoss ? 0f : 7f;
        float angle = index * 137.5f * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 1f, Mathf.Sin(angle) * radius);
        GameObject instance = Instantiate(prefab, center + offset, Quaternion.identity, monsterRoot);
        instance.name = data.name;
        MonsterHuntMonsterController monster = instance.GetComponent<MonsterHuntMonsterController>();
        monster.Initialize(data, player != null ? player.transform : null);
        spawnedMonsters.Add(monster);
        remainingMonsters++;
    }

    public static Vector3 GetZoneCenter(HuntZone zone)
    {
        switch (zone)
        {
            case HuntZone.Easy:
                return new Vector3(-24f, 0f, 12f);
            case HuntZone.Normal:
                return new Vector3(24f, 0f, 12f);
            case HuntZone.Hard:
                return new Vector3(-24f, 0f, -18f);
            case HuntZone.Boss:
                return new Vector3(24f, 0f, -18f);
            default:
                return Vector3.zero;
        }
    }

    private void OnGUI()
    {
        if (player == null)
        {
            return;
        }

        GUI.Box(new Rect(12, 12, 360, 150), "Monster Hunt MVP");
        GUI.Label(new Rect(24, 40, 330, 22), "Zone: " + currentZone);
        GUI.Label(new Rect(24, 62, 330, 22), "HP: " + player.health.CurrentHealth + " / " + player.health.MaxHealth);
        GUI.Label(new Rect(24, 84, 330, 22), "Level: " + player.progress.Level + "  EXP: " + player.progress.CurrentExp + " / " + player.progress.GetNextLevelExpLabel());
        GUI.Label(new Rect(24, 106, 330, 22), "Gold: " + player.progress.Gold + "  Kills: " + player.progress.KillCount);
        GUI.Label(new Rect(24, 128, 330, 22), "Gun Cooldown: " + Mathf.RoundToInt(player.combat.HeadbuttCooldownRatio * 100f) + "%");
        DrawCrosshair();

        if (gameEnded)
        {
            string title = bossKilled ? "CLEAR - Boss defeated" : "GAME OVER";
            GUI.Box(new Rect(Screen.width / 2 - 190, Screen.height / 2 - 80, 380, 160), title);
            GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 - 45, 320, 22), "Final Level: " + player.progress.Level);
            GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 - 20, 320, 22), "Gold: " + player.progress.Gold + "  Kills: " + player.progress.KillCount);
            GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 + 20, 320, 22), "Press R to restart");
        }
    }

    private void DrawCrosshair()
    {
        float x = Screen.width * 0.5f;
        float y = Screen.height * 0.5f;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x - 8f, y - 1f, 16f, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x - 1f, y - 8f, 2f, 16f), Texture2D.whiteTexture);
    }
}
