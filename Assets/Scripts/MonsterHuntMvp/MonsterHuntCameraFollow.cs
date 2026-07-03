using UnityEngine;

public class MonsterHuntCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 shoulderOffset = new Vector3(0f, 2.15f, -3.25f);
    public float followSpeed = 18f;
    public float rotationSpeed = 20f;
    public float lookHeight = 1.45f;
    public bool snapToTarget = true;

    private Vector3 lastForward = Vector3.forward;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 planarForward = target.forward;
        planarForward.y = 0f;
        if (planarForward.sqrMagnitude > 0.01f)
        {
            lastForward = planarForward.normalized;
        }

        Quaternion yaw = Quaternion.LookRotation(lastForward, Vector3.up);
        Vector3 desiredPosition = target.position + yaw * shoulderOffset;
        transform.position = snapToTarget
            ? desiredPosition
            : Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = snapToTarget
            ? desiredRotation
            : Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
