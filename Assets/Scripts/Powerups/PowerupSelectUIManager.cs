using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GEM
{
    public class PowerupSelectUIManager : Singleton<PowerupSelectUIManager>
    {
        //[SerializeField] private UIDocument uiDocument;
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

            // Define rarity colors (semi-transparent)
            Color commonColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);        // Grey
            Color uncommonColor = new Color(0.4f, 0.8f, 0.4f, 0.7f);      // Light Green
            Color rareColor = new Color(0.3f, 0.5f, 1f, 0.7f);            // Blue
            Color legendaryColor = new Color(1f, 0.9f, 0.2f, 0.7f);       // Yellow

            for (int i = 0; i < powerups.Count && i < 3; i++)
            {
                PowerupData powerup = powerups[i];

                // Get the item button
                Button itemButton = buttons[i].GetComponent<Button>();
                if (itemButton == null) continue;

                int itemIndex = i;
                itemButton.onClick.AddListener(() => OnPowerupSelected(itemIndex));

                // Set background color based on rarity
                Color backgroundColor;
                switch (powerup.rarity)
                {
                    case PowerupRarity.Common:
                        backgroundColor = commonColor;
                        break;
                    case PowerupRarity.Uncommon:
                        backgroundColor = uncommonColor;
                        break;
                    case PowerupRarity.Rare:
                        backgroundColor = rareColor;
                        break;
                    case PowerupRarity.Legendary:
                        backgroundColor = legendaryColor;
                        break;
                    default:
                        backgroundColor = commonColor;
                        break;
                }
                itemButton.gameObject.GetComponent<Image>().color = backgroundColor;

                // Set title
                TMP_Text title = itemButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                if (title != null)
                {
                    string propertyName = FormatPropertyName(powerup.property);
                    string rarityName = powerup.rarity.ToString();
                    title.text = $"{propertyName} ({rarityName})";
                }

                // Set description
                TMP_Text description = itemButton.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
                if (description != null)
                {
                    string propertyName = FormatPropertyName(powerup.property);
                    description.text = $"Increases {propertyName} by {powerup.value}%";
                }
            }
        }


        private string FormatPropertyName(PlayerProperty property)
        {
            // Convert enum to string and add spaces before capital letters
            string propertyString = property.ToString();
            string formatted = "";

            for (int i = 0; i < propertyString.Length; i++)
            {
                if (i > 0 && char.IsUpper(propertyString[i]))
                {
                    formatted += " ";
                }

                formatted += propertyString[i];
            }

            return formatted;
        }
    }
}
