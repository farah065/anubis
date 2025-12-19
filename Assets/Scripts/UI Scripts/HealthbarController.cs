using UnityEngine;
using UnityEngine.UI;
using GEM;
using TMPro;

public class HealthbarController : Singleton<HealthbarController>
{
    [SerializeField] private Slider _healthbarSlider;
    [SerializeField] private TMP_Text _healthbarText;

    private void Start()
    {
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        float currentHealth = Player.Instance.health;
        float maxHealth = Player.Instance.maxHealth;

        if (_healthbarSlider != null)
        {
            _healthbarSlider.maxValue = maxHealth;
            _healthbarSlider.value = currentHealth;
        }
        if (_healthbarText != null)
        {
            _healthbarText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }
}
