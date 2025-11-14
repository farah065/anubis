using MoreMountains.Feedbacks;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private Collider _attackCollider;

    public void EnableAttackCollider()
    {
        _attackCollider.enabled = true;
    }

    public void DisableAttackCollider()
    {
        _attackCollider.enabled = false;
    }
}
