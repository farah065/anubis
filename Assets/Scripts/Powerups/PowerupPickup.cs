
using System.Collections.Generic;
using GEM;
using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    public List<PowerupData> Powerups;
    public PowerupSelectUIManager PowerupSelectUI;

    private void Awake()
    {
        PowerupSelectUI = FindFirstObjectByType<PowerupSelectUIManager>();
    }

    private void Start()
    {
        PowerupSelectUI.SetUIVisibility(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SelectPowerups();
            PowerupSelectUI.SetInitiatingPickup(this);
            PowerupSelectUI.SetUIVisibility(true);
            PowerupSelectUI.PopulatePowerupUI(Powerups);
            Time.timeScale = 0;

        }
    }

    public void ClosePowerupUI()
    {
        Time.timeScale = 1;
        PowerupSelectUI.SetUIVisibility(false);
        GameManager.Instance.PowerupInScene = false;
        Destroy(gameObject);
    }

    private void SelectPowerups()
    {
        Debug.Log("Select Powerups");
        List<PowerupData> selectedPowerups = new List<PowerupData>();
        HashSet<string> selectedNames = new HashSet<string>(); // Track duplicates

        PowerupData[] allPowerups = Resources.LoadAll<PowerupData>("Powerups");

        if (allPowerups.Length == 0)
        {
            Debug.LogError("No powerups found in Resources/Powerups/");
        }

        for (int i = 0; i < 3; i++)
        {
            // Generate random number and determine rarity
            float roll = UnityEngine.Random.Range(0f, 100f);
            PowerupRarity targetRarity;

            if (roll < 50f)
                targetRarity = PowerupRarity.Common;
            else if (roll < 75f)
                targetRarity = PowerupRarity.Uncommon;
            else if (roll < 87.5f)
                targetRarity = PowerupRarity.Rare;
            else
                targetRarity = PowerupRarity.Legendary;

            // Find all powerups of the target rarity
            List<PowerupData> availablePowerups = new List<PowerupData>();
            foreach (PowerupData powerup in allPowerups)
            {
                if (powerup.rarity == targetRarity && !selectedNames.Contains(powerup.name))
                {
                    availablePowerups.Add(powerup);
                }
            }

            // If no powerups of this rarity available (or all are duplicates),
            // try to find any non-duplicate powerup as fallback
            if (availablePowerups.Count == 0)
            {
                foreach (PowerupData powerup in allPowerups)
                {
                    if (!selectedNames.Contains(powerup.name))
                    {
                        availablePowerups.Add(powerup);
                    }
                }
            }

            // Select random powerup from available ones
            if (availablePowerups.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, availablePowerups.Count);
                PowerupData selected = availablePowerups[randomIndex];
                selectedPowerups.Add(selected);
                selectedNames.Add(selected.name);
            }
            else
            {
                Debug.LogWarning($"Could not find unique powerup for slot {i + 1}");
            }
        }

        Powerups = selectedPowerups;
    }
}
