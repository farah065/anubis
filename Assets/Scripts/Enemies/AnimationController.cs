using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;

    private void Awake()
    {
        _enemy._animator.applyRootMotion = true;
        _enemy._agent.updatePosition = false;
        _enemy._agent.updateRotation = true;
    }

    private void OnAnimatorMove()
    {
        // Root motion delta (how far the animation moved this frame)
        Vector3 rootDelta = _enemy._animator.deltaPosition;

        // Apply root motion to the parent transform
        _enemy.transform.position += rootDelta;

        // Keep agent height consistent with navmesh
        Vector3 pos = _enemy.transform.position;
        pos.y = _enemy._agent.nextPosition.y;
        _enemy.transform.position = pos;

        // update agent position to match
        _enemy._agent.nextPosition = _enemy.transform.position;
    }
}
