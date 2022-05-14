using System;
using System.Collections;
using System.Collections.Generic;
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
			PlayMakerFSM.BroadcastEvent("HERO DAMAGED");
			yield return new WaitForSeconds(5f);
			HeroController.instance.AddHealth(Math.Min(health, health + healthBlue - damage) - 1);
			for(int i = 0; i < Math.Max(healthBlue - damage, 0); i++)
			{
				PlayMakerFSM.BroadcastEvent("ADD BLUE HEALTH");
			}
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
				{
					PlayMakerFSM.BroadcastEvent("ADD BLUE HEALTH");
				}
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_High Stress";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress", "ChallengeMode_Unfriendly Fire", "ChallengeMode_Ascension",
				"ChallengeMode_Past Regrets", "ChallengeMode_A Fool's Errand", "ChallengeMode_Poor Memory"
			};
		}
	}
}
