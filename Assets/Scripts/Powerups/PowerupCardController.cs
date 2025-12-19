using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GEM;
using MoreMountains.Feedbacks;

public class PowerupCardController : MonoBehaviour
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private TMP_Text _powerupName;
    [SerializeField] private TMP_Text _powerupDescription;
    [SerializeField] private TMP_Text _powerupRarity;

    [SerializeField] private Sprite[] _raritySprites; // 0: Common, 1: Uncommon, 2: Rare, 3: Legendary
    [SerializeField] private MMF_Player _flipFeedbacks;
    public MMF_Player DisappearFeedbacks;

    private PowerupData _powerupData;
    private bool _isFlipped = false;

    public void SetPowerupData(PowerupData powerupData)
    {
        _powerupData = powerupData;
        UpdateCardUI();
    }

    public void FlipCard()
    {
        if (_isFlipped) { return; }

        _isFlipped = true;
        _flipFeedbacks.PlayFeedbacks();
    }

    private void UpdateCardUI()
    {
        if (_powerupData == null) { return; }

        // Set card image based on rarity
        _cardImage.sprite = _raritySprites[(int)_powerupData.rarity];

        // Set powerup name
        _powerupName.text = FormatPropertyName(_powerupData.property);

        // Set powerup description
        _powerupDescription.text = $"Increases {FormatPropertyName(_powerupData.property)} by {_powerupData.value}%";

        // Set powerup rarity text
        _powerupRarity.text = "(" + _powerupData.rarity.ToString() + ")";
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
