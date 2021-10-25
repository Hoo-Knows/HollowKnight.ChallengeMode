using System;
using System.Collections;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class HighStress : Modifier
	{
		public override void StartEffect()
		{
			ModHooks.Instance.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			int health = PlayerData.instance.GetInt("health");
			int healthBlue = PlayerData.instance.GetInt("healthBlue");
			StopCoroutine(HandleHealth(damage, health, healthBlue));
			if(health + healthBlue > damage)
			{
				StartCoroutine(HandleHealth(damage, health, healthBlue));
				return 0;
			}
			return damage;
		}

		private IEnumerator HandleHealth(int damage, int health, int healthBlue)
		{
			PlayerData.instance.health = 1;
			PlayerData.instance.healthBlue = 0;
			EventRegister.SendEvent("HERO DAMAGED");
			yield return new WaitForSecondsRealtime(5f);
			HeroController.instance.AddHealth(Math.Min(health, health + healthBlue - damage) - 1);
			for(int i = 0; i < Math.Max(healthBlue - damage, 0); i++)
				EventRegister.SendEvent("ADD BLUE HEALTH");
			yield break;
		}

		public override void StopEffect()
		{
			ModHooks.Instance.TakeHealthHook -= TakeHealthHook;
		}
	}
}
