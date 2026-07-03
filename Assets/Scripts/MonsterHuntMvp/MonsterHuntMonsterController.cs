using System.Collections;
using UnityEngine;

public class MonsterHuntMonsterController : MonoBehaviour
{
    public MonsterData Data { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrentHp { get; private set; }
    public float healthBarHeight = 1.35f;
    public Vector2 healthBarSize = new Vector2(64f, 8f);

    private Transform target;
    private Rigidbody rb;
    private Renderer cachedRenderer;
    private Vector3 spawnPosition;
    private float patrolAngle;
    private GUIStyle healthLabelStyle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cachedRenderer = GetComponentInChildren<Renderer>();
        spawnPosition = transform.position;
    }

    public void Initialize(MonsterData data, Transform playerTarget)
    {
        Data = data;
        target = playerTarget;
        CurrentHp = data.maxHp;
        transform.localScale = data.isBoss ? new Vector3(3.0f, 3.0f, 3.0f) : Vector3.one * Mathf.Lerp(0.9f, 1.4f, data.maxHp / 150f);
        ApplyColor(data.zone, data.isBoss);
    }

    private void FixedUpdate()
    {
        MonsterHuntGameManager manager = MonsterHuntGameManager.Instance;
        if (IsDead || Data == null || target == null || manager == null || manager.gameEnded)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        Vector3 moveDirection;

        if (toTarget.magnitude <= Data.chaseRange)
        {
            moveDirection = toTarget.normalized;
        }
        else
        {
            patrolAngle += Time.fixedDeltaTime * 0.8f;
            Vector3 patrolPoint = spawnPosition + new Vector3(Mathf.Cos(patrolAngle), 0f, Mathf.Sin(patrolAngle)) * 2.5f;
            moveDirection = patrolPoint - transform.position;
            moveDirection.y = 0f;
            moveDirection = moveDirection.sqrMagnitude > 0.1f ? moveDirection.normalized : Vector3.zero;
        }

        rb.linearVelocity = new Vector3(moveDirection.x * Data.moveSpeed, rb.linearVelocity.y, moveDirection.z * Data.moveSpeed);
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
    }

    private void OnGUI()
    {
        if (IsDead || Data == null || CurrentHp <= 0)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector3 worldPosition = GetHealthBarWorldPosition();
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            return;
        }

        float ratio = Mathf.Clamp01((float)CurrentHp / Data.maxHp);
        float width = healthBarSize.x;
        float height = healthBarSize.y;
        Rect background = new Rect(screenPosition.x - width * 0.5f, Screen.height - screenPosition.y, width, height);
        Rect fill = new Rect(background.x + 1f, background.y + 1f, Mathf.Max(0f, (width - 2f) * ratio), height - 2f);

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(background, Texture2D.whiteTexture);
        GUI.color = ratio > 0.45f ? new Color(0.1f, 0.85f, 0.25f) : ratio > 0.2f ? new Color(1f, 0.75f, 0.15f) : new Color(0.95f, 0.15f, 0.12f);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (Data.isBoss)
        {
            healthLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(background.x - 24f, background.y - 16f, width + 48f, 16f), Data.name, healthLabelStyle);
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHp -= amount;
        StartCoroutine(FlashHit());

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        if (MonsterHuntGameManager.Instance != null)
        {
            MonsterHuntGameManager.Instance.GrantMonsterReward(Data);
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Destroy(gameObject, 0.35f);
    }

    private IEnumerator FlashHit()
    {
        if (cachedRenderer == null)
        {
            yield break;
        }

        Color original = cachedRenderer.material.color;
        cachedRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = original;
        }
    }

    private void ApplyColor(HuntZone zone, bool isBoss)
    {
        if (cachedRenderer == null)
        {
            return;
        }

        Color color;
        switch (zone)
        {
            case HuntZone.Easy:
                color = new Color(0.25f, 0.9f, 0.3f);
                break;
            case HuntZone.Normal:
                color = new Color(0.95f, 0.75f, 0.2f);
                break;
            case HuntZone.Hard:
                color = new Color(0.9f, 0.22f, 0.18f);
                break;
            case HuntZone.Boss:
                color = new Color(0.45f, 0.18f, 0.75f);
                break;
            default:
                color = Color.gray;
                break;
        }

        if (isBoss)
        {
            color = new Color(0.35f, 0.05f, 0.52f);
        }

        cachedRenderer.material.color = color;
    }

    private Vector3 GetHealthBarWorldPosition()
    {
        if (cachedRenderer != null)
        {
            Bounds bounds = cachedRenderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + healthBarHeight, bounds.center.z);
        }

        return transform.position + Vector3.up * healthBarHeight;
    }
}
