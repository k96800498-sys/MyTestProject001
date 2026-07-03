using UnityEngine;
using UnityEngine.InputSystem;

public class MonsterHuntPlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public LayerMask groundLayer = ~0;
    public MonsterHuntPlayerCombat combat;
    public MonsterHuntPlayerHealth health;
    public MonsterHuntPlayerProgress progress;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<MonsterHuntPlayerCombat>();
        health = GetComponent<MonsterHuntPlayerHealth>();
        progress = GetComponent<MonsterHuntPlayerProgress>();
    }

    private void Update()
    {
        if (MonsterHuntGameManager.Instance != null && MonsterHuntGameManager.Instance.gameEnded)
        {
            moveInput = Vector3.zero;
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        float x = 0f;
        float z = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
        moveInput = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);

        if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (combat != null && combat.IsHeadbutting)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        Vector3 desired = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(desired.x, velocity.y, desired.z);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveInput, Vector3.up);
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.85f, groundLayer, QueryTriggerInteraction.Ignore);
    }
}
