using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Unity.VisualScripting;
using Image = UnityEngine.UI.Image;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel; 
    public GameObject dimPanel;
    public GameObject settingsWindow;
    public GameObject controlsWindow;
    private Image dimImage;
    private void Start()
    {
        optionsPanel.SetActive(false);
        dimPanel.SetActive(false);
        dimImage= dimPanel.GetComponent<Image>();
        settingsWindow.SetActive(false);
        controlsWindow.SetActive(false);
    }
    public void Play()
    {
        // TODO: Load the first scene here.
        // Example:
        // SceneManager.LoadScene("GameScene");

        Debug.Log("Play button pressed");
    }
    public void Settings()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(true);
        SetDimAlpha(0.8f);
        Debug.Log("Settings window opened.");
    }
    public void CloseSettings()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);
        SetDimAlpha(0.6f);
        Debug.Log("Settings window closed.");
    }
    public void Controls()
    {
        if (controlsWindow != null)
            controlsWindow.SetActive(true);
        SetDimAlpha(0.8f);
        Debug.Log("Controls window opened.");
    }
    public void CloseControls()
    {
        if (controlsWindow != null)
            controlsWindow.SetActive(false);
        SetDimAlpha(0.6f);
        Debug.Log("Controls window closed.");
    }
    public void Options()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
        if (dimPanel != null)
        {
            SetDimAlpha(0.6f);
            dimPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Pause game
        Debug.Log("Options menu opened.");
    }
    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (dimPanel != null)
            dimPanel.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log("Options closed, game resumed.");
    }
    private void SetDimAlpha(float alpha)
    {
        if (dimImage == null) return;

        Color c = dimImage.color;
        c.a = alpha;
        dimImage.color = c;
    }
    public void Quit()
    {
        Debug.Log("Quit button pressed. Quitting application...");

        Application.Quit();
    }
}
