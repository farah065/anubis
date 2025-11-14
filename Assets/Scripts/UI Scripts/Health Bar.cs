using UnityEditor;
using UnityEngine;
using UnityEngine.UI;  // Needed for Slider + legacy Text

public class HealthBar : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider;  // The HP bar
    public Text healthText;      // The legacy UI text

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;

    void Start()
    {
        currentHealth = maxHealth;

        // Initialize slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateHealthUI();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(33);
        }
    }
    public void TakeDamage(int amount)
    {
        if (Time.timeScale == 0f)
        {
            
            return;
        }
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        // Update slider value
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Update text: "current/max"
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;

            float percent = (float)currentHealth / maxHealth;

            // Danger threshold = 30%
            if (percent < 0.3f)
                healthText.color = dangerColor;
            else
                healthText.color = normalColor;
        }
    }
}
