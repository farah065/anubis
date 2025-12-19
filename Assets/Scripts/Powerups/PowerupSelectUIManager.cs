using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GEM
{
    public class PowerupSelectUIManager : Singleton<PowerupSelectUIManager>
    {
        [SerializeField] private GameObject powerupSelectUI;
        [SerializeField] private GameObject[] buttons;
        [SerializeField] private PowerupPickup intiatingPickup;
        private List<PowerupData> currentPowerups; // Store the current powerup selection

        public void SetUIVisibility(bool isVisible)
        {
            powerupSelectUI.SetActive(isVisible);
        }

        private void OnPowerupSelected(int index)
        {
            if (currentPowerups == null || index < 0 || index >= currentPowerups.Count)
            {
                Debug.LogError("Invalid powerup selection");
                return;
            }
            PowerupData selectedPowerup = currentPowerups[index];

            Debug.Log($"Index: {index}");
            Debug.Log($"Selected: {selectedPowerup.name}");
            Debug.Log($"Rarity: {selectedPowerup.rarity}");
            Debug.Log($"Property: {selectedPowerup.property}");
            Debug.Log($"Value: {selectedPowerup.value}");

            Player.Instance.ApplyPowerup(selectedPowerup);
            intiatingPickup.ClosePowerupUI();
            intiatingPickup = null;
        }

        public void SetInitiatingPickup(PowerupPickup pickup)
        {
            intiatingPickup = pickup;
        }

        public void PopulatePowerupUI(List<PowerupData> powerups)
        {
            // Store powerups for later reference
            currentPowerups = powerups;

            for (int i = 0; i < powerups.Count && i < 3; i++)
            {
                PowerupData powerup = powerups[i];

                // Get the item button
                Button itemButton = buttons[i].GetComponent<Button>();
                if (itemButton == null) continue;

                int itemIndex = i;
                itemButton.onClick.AddListener(() => OnPowerupSelected(itemIndex));

                itemButton.gameObject.GetComponent<PowerupCardController>().SetPowerupData(powerup);
            }
        }
    }
}
