using UnityEngine;

public class PersistentUI : Singleton<PersistentUI>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
