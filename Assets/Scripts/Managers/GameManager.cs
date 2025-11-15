using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private List<SceneAsset> levelScenes;
    [SerializeField] private SceneAsset cooldownRoom;
#endif
    [SerializeField] private List<string> levelSceneNames;
    private string lastLevelName;
    private int levelIndex = 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-populate scene names from SceneAsset references
        levelSceneNames = new List<string>();
        foreach (var sceneAsset in levelScenes)
        {
            if (sceneAsset != null)
            {
                levelSceneNames.Add(sceneAsset.name);
            }
        }
    }
#endif

    public void LoadRandomLevel()
    {
        if (levelIndex < 5)
        {
            if (levelSceneNames.Count == 0)
            {
                Debug.LogError("No level scenes assigned!");
                return;
            }

            string chosen;
            do
            {
                chosen = levelSceneNames[Random.Range(0, levelSceneNames.Count)];
            }
            while (chosen == lastLevelName && levelSceneNames.Count > 1);

            lastLevelName = chosen;
            SceneManager.LoadScene(chosen);

            levelIndex++;
        }
        else
        {
            ReturnToCooldownRoom();
        }
    }

    public void ReturnToCooldownRoom()
    {
        SceneManager.LoadScene(cooldownRoom.name);
    }
}