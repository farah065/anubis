using UnityEngine;

/// <summary>
/// Handles applying root motion from animations to the enemy's transform and navmesh agent.
/// Should be attached to the same game object that has the animator.
/// </summary>
public class AnimationController : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;

    private void Awake()
    {
        _enemy.Animator.applyRootMotion = true;
        _enemy.Agent.updatePosition = false;
        _enemy.Agent.updateRotation = true;
    }

    private void OnAnimatorMove()
    {
        // root motion delta (how far the animation moved this frame)
        Vector3 rootDelta = _enemy.Animator.deltaPosition;

        // apply root motion to the parent transform
        _enemy.transform.position += rootDelta;

        // keep agent height consistent with navmesh
        Vector3 pos = _enemy.transform.position;
        pos.y = _enemy.Agent.nextPosition.y;
        _enemy.transform.position = pos;

        // update agent position to match
        _enemy.Agent.nextPosition = pos;
    }
}
