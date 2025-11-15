using UnityEngine;

namespace GEM
{
	public enum PowerupRarity
	{
		Common,
		Uncommon,
		Rare,
		Legendary
	}

	public enum PlayerProperty
	{
		MeleeAttackDamage,
		MeleeAttackKnockback,
		RangedAttackDamage,
		RangedAttackKnockback,
		MovementSpeed,
		MaxHealth,
	}

	[CreateAssetMenu(fileName = "PowerupData", menuName = "ScriptableObjects/Powerup", order = 0)]
	public class PowerupData : ScriptableObject
	{
		public PowerupRarity rarity;
		public PlayerProperty property;
		public float value;
	}
}