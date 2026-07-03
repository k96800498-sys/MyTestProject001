using UnityEngine;

public class MonsterHuntZoneTrigger : MonoBehaviour
{
    public HuntZone zone;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MonsterHuntPlayerController>() != null && MonsterHuntGameManager.Instance != null)
        {
            MonsterHuntGameManager.Instance.SetCurrentZone(zone);
        }
    }
}
