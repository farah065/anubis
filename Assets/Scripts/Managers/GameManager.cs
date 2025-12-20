using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using GEM;
using MoreMountains.Tools;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : Singleton<GameManager>
{
#if UNITY_EDITOR
    [SerializeField] private List<SceneAsset> levelScenes;
    [SerializeField] private SceneAsset cooldownRoom;
#endif
    [SerializeField] private List<string> levelSceneNames;
    [SerializeField] private MMSMPlaylist gameplayMusicPlaylist;
    private string lastLevelName;
    private int levelIndex = 0;

    [SerializeField] private GameObject powerupPrefab;
    public bool PowerupInScene = false;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeMusic();
    }

    void InitializeMusic()
    {
        MMSMPlaylistManager.Instance.PlayOnStart = false;
        MMSMPlaylistManager.Instance.Playlist = gameplayMusicPlaylist;
        MMSMPlaylistManager.Instance.FadeIn = true;
        MMSMPlaylistManager.Instance.FadeOut = true;
        MMSMPlaylistManager.Instance.Play();
        PlayIdleMusic();
    }

    void PlayIdleMusic()
    {
        if (MMSMPlaylistManager.Instance.CurrentSongIndex == 0) return;
        MMSMPlaylistManager.Instance.PlaySongAt(0);
    }

    void PlayBattleMusic()
    {
        if (MMSMPlaylistManager.Instance.CurrentSongIndex == 1) return;
        MMSMPlaylistManager.Instance.PlaySongAt(1);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(TeleportNextFrame());
    }

    IEnumerator TeleportNextFrame()
    {
        yield return null; // wait 1 frame
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        player.transform.position = spawnPoint.transform.position; 
        player.transform.rotation = spawnPoint.transform.rotation;
    }

    public void LoadRandomLevel()
    {
        if (PowerupInScene) { return; }

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Player.Instance.health = 100;
        Player.Instance.maxHealth = 100;
        PlayerStateMachine.Instance.ResetPowerups();
        PowerupDisplayManager.Instance.ResetDisplayedPowerups();
    }

    public void LevelCleared()
    {
        PlayIdleMusic();
        SpawnPowerup();
    }

    public void EnterBattle()
    {
        PlayBattleMusic();
    }

    private void SpawnPowerup()
    {
        PowerupInScene = true;
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        Instantiate(powerupPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
    }
}