using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("DontDestroyOnLoad");

        foreach (GameObject obj in objs)
        {
            DontDestroyOnLoad(obj);
        }
    }
}