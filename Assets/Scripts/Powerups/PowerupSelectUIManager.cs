using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GEM
{
    public class PowerupSelectUIManager : Singleton<PowerupSelectUIManager>
    {
        [SerializeField] private GameObject powerupSelectUI;
        [SerializeField] private GameObject[] buttons;
        [SerializeField] private PowerupPickup intiatingPickup;
        [SerializeField] private PowerupCardController[] powerupCardControllers;
        private List<PowerupData> currentPowerups; // Store the current powerup selection

        public void SetUIVisibility(bool isVisible)
        {
            if (isVisible)
            {
                powerupSelectUI.SetActive(isVisible);
            }
            else
            {
                StartCoroutine(HideUI());
            }
        }

        private IEnumerator HideUI()
        {
            foreach (var controller in powerupCardControllers)
            {
                controller.DisappearFeedbacks.PlayFeedbacks();
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.2f);

            powerupSelectUI.SetActive(false);
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
            PowerupDisplayManager.Instance.AddPowerupToDisplay(selectedPowerup);
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
