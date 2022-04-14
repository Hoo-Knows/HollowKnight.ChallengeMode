using System;
using System.Collections;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class HighStress : Modifier
	{
		private int health;
		private int healthBlue;
		private int damage;
		private bool flag;

		public override void StartEffect()
		{
			ModHooks.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			this.damage = damage;
			health = PlayerData.instance.GetInt("health");
			healthBlue = PlayerData.instance.GetInt("healthBlue");

			StopCoroutine(HandleHealth());
			if(health + healthBlue > damage)
			{
				StartCoroutine(HandleHealth());
				return 0;
			}
			return damage;
		}

		private IEnumerator HandleHealth()
		{
			flag = true;
			PlayerData.instance.SetInt("health", 1);
			PlayerData.instance.SetInt("healthBlue", 0);
			EventRegister.SendEvent("HERO DAMAGED");
			yield return new WaitForSeconds(5f);
			HeroController.instance.AddHealth(Math.Min(health, health + healthBlue - damage) - 1);
			for(int i = 0; i < Math.Max(healthBlue - damage, 0); i++)
				EventRegister.SendEvent("ADD BLUE HEALTH");
			flag = false;
			yield break;
		}

		public override void StopEffect()
		{
			ModHooks.TakeHealthHook -= TakeHealthHook;
			StopAllCoroutines();
			if(flag)
			{
				HeroController.instance.AddHealth(Math.Min(health, health + healthBlue - damage) - 1);
				for(int i = 0; i < Math.Max(healthBlue - damage, 0); i++)
					EventRegister.SendEvent("ADD BLUE HEALTH");
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_High Stress";
		}
	}
}
