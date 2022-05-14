using Random = System.Random;
using Modding;
using SFCore.Utils;
using System.Collections.Generic;

namespace ChallengeMode.Modifiers
{
	class PoorMemory : Modifier
	{
		private PlayerData pd;

		private int health;
		private int healthMax;
		private int healthBlue;
		private int healthBlueMax;
		private Random random;

		private PlayMakerFSM furyFSM;
		private int healthDiff;
		private int healthBlueDiff;

		public override void StartEffect()
		{
			pd = PlayerData.instance;

			if(!pd.GetBool("equippedCharm_27") && !pd.GetBool("equippedCharm_29"))
			{
				health = pd.GetInt("health");
				healthMax = pd.CurrentMaxHealth;
				healthBlue = pd.GetInt("healthBlue");
				healthBlueMax = pd.GetInt("healthBlue");
				random = new Random();

				//Fix Fury
				furyFSM = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury");
				furyFSM.InsertMethod("Check HP", () =>
				{
					furyFSM.FsmVariables.FindFsmInt("HP").Value = health;
				}, 6);
				furyFSM.InsertMethod("Recheck", () =>
				{
					furyFSM.FsmVariables.FindFsmInt("HP").Value = health;
				}, 1);

				//Fix Elegy
				ModHooks.AttackHook += AttackHook;
				ModHooks.AfterAttackHook += AfterAttackHook;

				ModHooks.TakeHealthHook += TakeHealthHook;
				On.HeroController.AddHealth += HeroControllerAddHealth;
				On.HeroController.CanFocus += HeroControllerCanFocus;
			}
		}

		private int TakeHealthHook(int damage)
		{
			RandomizeHealth();
			int damageBlue = System.Math.Min(healthBlue, damage);
			healthBlue -= damageBlue;
			damage -= damageBlue;
			if(damage > 0)
			{
				health -= damage;
			}
			if(health <= 0)
			{
				pd.SetInt("health", 0);
				PlayMakerFSM.BroadcastEvent("HERO DAMAGED");
			}
			ChallengeMode.Instance.Log("health: " + health);
			ChallengeMode.Instance.Log("healthBlue: " + healthBlue);
			return 0;
		}

		private void HeroControllerAddHealth(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
		{
			RandomizeHealth();
			if(health + amount <= healthMax)
			{
				health += amount;
			}
			ChallengeMode.Instance.Log("health: " + health);
			ChallengeMode.Instance.Log("healthBlue: " + healthBlue);
		}

		private bool HeroControllerCanFocus(On.HeroController.orig_CanFocus orig, HeroController self)
		{
			if(health == healthMax) return false;
			return orig(self);
		}

		private void AttackHook(GlobalEnums.AttackDirection obj)
		{
			healthDiff = health - pd.GetInt("health");
			healthBlueDiff = healthBlue - pd.GetInt("healthBlue");
			pd.SetInt("health", pd.GetInt("health") + healthDiff);
			pd.SetInt("healthBlue", pd.GetInt("healthBlue") + healthBlueDiff);
		}

		private void AfterAttackHook(GlobalEnums.AttackDirection obj)
		{
			pd.SetInt("health", pd.GetInt("health") - healthDiff);
			pd.SetInt("healthBlue", pd.GetInt("healthBlue") - healthBlueDiff);
		}

		private void RandomizeHealth()
		{
			//Health
			int healthFake = random.Next(1, healthMax + 1);
			bool healthFlag = healthFake > pd.GetInt("health");
			pd.SetInt("health", healthFake);

			//Lifeblood
			int healthBlueFake = random.Next(0, healthBlueMax + 1);
			bool healthBlueFlag = healthBlueFake > pd.GetInt("healthBlue");
			if(healthBlueFlag)
			{
				for(int i = 0; i < healthBlueFake - pd.GetInt("healthBlue"); i++)
				{
					PlayMakerFSM.BroadcastEvent("ADD BLUE HEALTH");
				}
			}
			else
			{
				pd.SetInt("healthBlue", healthBlueFake);
				PlayMakerFSM.BroadcastEvent("HERO DAMAGED");
			}

			if(healthFlag) PlayMakerFSM.BroadcastEvent("HERO HEALED");
		}

		public override void StopEffect()
		{
			if(!pd.GetBool("equippedCharm_27") && !pd.GetBool("equippedCharm_29"))
			{
				ModHooks.TakeHealthHook -= TakeHealthHook;
				On.HeroController.AddHealth -= HeroControllerAddHealth;
				On.HeroController.CanFocus -= HeroControllerCanFocus;
				ModHooks.AttackHook -= AttackHook;
				ModHooks.AfterAttackHook -= AfterAttackHook;

				furyFSM.RemoveAction("Check HP", 6);
				furyFSM.RemoveAction("Recheck", 1);

				pd.SetInt("health", health);
				pd.SetInt("healthBlue", healthBlue);

				GameCameras.instance.hudCanvas.gameObject.SetActive(false);
				GameCameras.instance.hudCanvas.gameObject.SetActive(true);
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_Poor Memory";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Poor Memory", "ChallengeMode_High Stress"
			};
		}
	}
}
