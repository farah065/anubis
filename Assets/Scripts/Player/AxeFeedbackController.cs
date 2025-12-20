using MoreMountains.Feedbacks;
using UnityEngine;

public class AxeFeedbackController : MonoBehaviour
{
    public MMF_Player AxeImpactFeedbacks;

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"Axe collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AxeImpactFeedbacks?.PlayFeedbacks();
        }
    }
}
