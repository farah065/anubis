using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorDebugger : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        int layer = 0;
        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(layer);
        AnimatorTransitionInfo transition = anim.GetAnimatorTransitionInfo(layer);

        string currentName = GetStateName(current);
        string nextName = anim.IsInTransition(layer) ? GetTransitionTargetName(layer) : "(none)";

        Debug.Log(
            $"[Animator] Current: {currentName} " +
            $"({current.normalizedTime:F2}) | InTransition: {anim.IsInTransition(layer)} " +
            $"→ Next: {nextName}"
        );

        // Show parameters
        foreach (var p in anim.parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Bool:
                    Debug.Log($"  Bool {p.name}: {anim.GetBool(p.name)}");
                    break;
                case AnimatorControllerParameterType.Int:
                    Debug.Log($"  Int {p.name}: {anim.GetInteger(p.name)}");
                    break;
                case AnimatorControllerParameterType.Float:
                    Debug.Log($"  Float {p.name}: {anim.GetFloat(p.name):F2}");
                    break;
                case AnimatorControllerParameterType.Trigger:
                    Debug.Log($"  Trigger {p.name}");
                    break;
            }
        }
    }

    // Helper: try to map hash back to readable name
    private string GetStateName(AnimatorStateInfo info)
    {
        // fullPathHash encodes the name within the controller’s path.
        // We can’t get the literal string from the hash at runtime,
        // but AnimatorStateInfo.IsName lets us guess the readable label:
        string[] candidates = {
            "Attack0", "Attack1", "Attack2",
            "Idle", "Walk", "Run"
        };

        foreach (string name in candidates)
        {
            if (info.IsName(name) || info.IsName("Base Layer." + name))
                return name;
        }

        return $"<unknown hash:{info.shortNameHash}>";
    }

    // Helper: get the target state name during a transition
    private string GetTransitionTargetName(int layer)
    {
        AnimatorTransitionInfo t = anim.GetAnimatorTransitionInfo(layer);
        // same trick: we can only guess from known state names
        string[] candidates = {
            "Attack0", "Attack1", "Attack2",
            "Idle", "Walk", "Run"
        };
        foreach (string name in candidates)
        {
            if (t.IsName(name) || t.IsName("Base Layer." + name))
                return name;
        }
        return "<unknown>";
    }
}
