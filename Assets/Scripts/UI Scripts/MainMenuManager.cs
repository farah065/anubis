using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsMenuUI;
    [SerializeField] private MMF_Player buttonHoverFeedbacks;
    [SerializeField] private MMF_Player buttonClickFeedbacks;

    private void Start()
    {
        _settingsMenuUI.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("Cooldown Room");
    }

    public void OpenSettingsMenu()
    {
        BackgroundPanelController.Instance.ShowBackgroundPanel();
        _settingsMenuUI.SetActive(true);
    }

    public void CloseSettingsMenu()
    {
        BackgroundPanelController.Instance.HideBackgroundPanel();
        _settingsMenuUI.SetActive(false);
    }

    public void OnHoverButton()
    {
        buttonHoverFeedbacks?.PlayFeedbacks();
    }

    public void OnClickButton()
    {
        buttonClickFeedbacks?.PlayFeedbacks();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
