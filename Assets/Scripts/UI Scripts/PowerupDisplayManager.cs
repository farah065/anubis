using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using GEM;
using UnityEngine.UI;

public class PowerupDisplayManager : Singleton<PowerupDisplayManager>
{
    [SerializeField] private RectTransform _obelisk;
    [SerializeField] private Image[] _powerupImages;

    [Header("Powerup Sprites")]
    [SerializeField] private Sprite _maxHealthSprite;
    [SerializeField] private Sprite _meleeAttackDamageSprite;
    [SerializeField] private Sprite _meleeAttackKnockbackSprite;
    [SerializeField] private Sprite _rangedAttackDamageSprite;
    [SerializeField] private Sprite _rangedAttackKnockbackSprite;
    [SerializeField] private Sprite _movementSpeedSprite;

    private static List<PlayerProperty> _displayedProperties = new List<PlayerProperty>();
    private int _numOfPowerups = 0;

    private void Start()
    {
        _obelisk.anchoredPosition = new Vector2(-32, -924);
        foreach (PlayerProperty property in _displayedProperties)
        {
            _numOfPowerups++;
            SnapObeliskToPosition();
            SetPowerupIcon(property);
        }
    }

    public void AddPowerupToDisplay(PowerupData powerupData)
    {
        // if powerupData.property is in _displayedProperties, return
        foreach (PlayerProperty property in _displayedProperties)
        {
            if (property == powerupData.property)
            {
                return;
            }
        }

        _numOfPowerups++;
        _displayedProperties.Add(powerupData.property);
        MoveObelisk();
        SetPowerupIcon(powerupData.property);
    }

    public void ResetDisplayedPowerups()
    {
        _displayedProperties = new List<PlayerProperty>();
        _numOfPowerups = 0;
        _obelisk.anchoredPosition = new Vector2(-32, -924);
    }

    private void MoveObelisk()
    {
        if (_numOfPowerups == 1)
        {
            _obelisk.DOLocalMoveY(-730, 0.5f).SetEase(Ease.InCubic);
        }
        else if (_numOfPowerups == _powerupImages.Length)
        {
            _obelisk.DOLocalMoveY(-312, 0.5f).SetEase(Ease.InCubic);
        }
        else
        {
            _obelisk.DOLocalMoveY(_obelisk.localPosition.y + 80, 0.5f).SetEase(Ease.InCubic);
        }
    }

    private void SnapObeliskToPosition()
    {
        if (_numOfPowerups == 1)
        {
            _obelisk.anchoredPosition = new Vector2(-32, -730);
        }
        else if (_numOfPowerups == _powerupImages.Length)
        {
            _obelisk.anchoredPosition = new Vector2(-32, -312);
        }
        else
        {
            _obelisk.anchoredPosition = new Vector2(-32, _obelisk.anchoredPosition.y + 80);
        }
    }

    private void SetPowerupIcon(PlayerProperty powerupProperty)
    {
        if (powerupProperty == PlayerProperty.MaxHealth)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _maxHealthSprite;
        }
        else if (powerupProperty == PlayerProperty.MeleeAttackDamage)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _meleeAttackDamageSprite;
        }
        else if (powerupProperty == PlayerProperty.MeleeAttackKnockback)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _meleeAttackKnockbackSprite;
        }
        else if (powerupProperty == PlayerProperty.RangedAttackDamage)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _rangedAttackDamageSprite;
        }
        else if (powerupProperty == PlayerProperty.RangedAttackKnockback)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _rangedAttackKnockbackSprite;
        }
        else if (powerupProperty == PlayerProperty.MovementSpeed)
        {
            _powerupImages[_numOfPowerups - 1].sprite = _movementSpeedSprite;
        }
    }
}
