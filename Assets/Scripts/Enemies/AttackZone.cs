using UnityEngine;

public class AttackZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player got hit");
        }
    }
}
