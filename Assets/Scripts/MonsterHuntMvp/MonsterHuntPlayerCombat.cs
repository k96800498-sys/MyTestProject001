using UnityEngine;
using UnityEngine.InputSystem;

public class MonsterHuntPlayerCombat : MonoBehaviour
{
    public int baseGunDamage = 22;
    public float fireCooldown = 0.28f;
    public float range = 40f;
    public float aimHeight = 1.45f;
    public float contactDamageInterval = 0.85f;
    public LayerMask shotMask = ~0;

    public bool IsHeadbutting { get { return false; } }
    public float HeadbuttCooldownRatio { get; private set; }

    private MonsterHuntPlayerProgress progress;
    private float fireTimer;
    private float nextContactDamageTime;

    private void Awake()
    {
        progress = GetComponent<MonsterHuntPlayerProgress>();
    }

    private void Update()
    {
        if (MonsterHuntGameManager.Instance != null && MonsterHuntGameManager.Instance.gameEnded)
        {
            return;
        }

        fireTimer = Mathf.Max(0f, fireTimer - Time.deltaTime);
        HeadbuttCooldownRatio = fireCooldown <= 0f ? 0f : fireTimer / fireCooldown;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool firePressed = (keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        if (firePressed)
        {
            TryFire();
        }
    }

    public void TryHeadbutt()
    {
        TryFire();
    }

    public void TryFire()
    {
        if (fireTimer > 0f)
        {
            return;
        }

        fireTimer = fireCooldown;
        Ray ray = GetAimRay();
        if (Physics.Raycast(ray, out RaycastHit hit, range, shotMask, QueryTriggerInteraction.Ignore))
        {
            MonsterHuntMonsterController monster = hit.collider.GetComponentInParent<MonsterHuntMonsterController>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(GetGunDamage());
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleMonsterCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryHandleMonsterCollision(collision);
    }

    private Ray GetAimRay()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            return new Ray(camera.transform.position, camera.transform.forward);
        }

        return new Ray(transform.position + Vector3.up * aimHeight, transform.forward);
    }

    private int GetGunDamage()
    {
        int levelBonus = progress != null ? (progress.Level - 1) * 5 : 0;
        return Mathf.Max(1, baseGunDamage + levelBonus);
    }

    private void TryHandleMonsterCollision(Collision collision)
    {
        MonsterHuntMonsterController monster = collision.collider.GetComponentInParent<MonsterHuntMonsterController>();
        if (monster == null || monster.IsDead || monster.Data == null)
        {
            return;
        }

        MonsterHuntPlayerHealth health = GetComponent<MonsterHuntPlayerHealth>();
        if (health != null && Time.time >= nextContactDamageTime)
        {
            nextContactDamageTime = Time.time + contactDamageInterval;
            health.TakeDamage(monster.Data.contactDamage);
        }
    }
}
