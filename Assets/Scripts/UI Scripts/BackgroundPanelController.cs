using UnityEngine;
using UnityEngine.UI;

public class BackgroundPanelController : Singleton<BackgroundPanelController>
{
    [SerializeField] private Image _backgroundPanelImage;

    private void Start()
    {
        _backgroundPanelImage.enabled = false;
    }

    public void ShowBackgroundPanel()
    {
        _backgroundPanelImage.enabled = true;
    }

    public void HideBackgroundPanel()
    {
        _backgroundPanelImage.enabled = false;
    }
}
