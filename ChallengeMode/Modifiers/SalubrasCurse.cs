using System.Collections.Generic;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SalubrasCurse : Modifier
	{
		private int[] charms;

		public override void StartEffect()
		{
			//Store current charms and unequip them
			charms = PlayerData.instance.GetVariable<List<int>>(nameof(PlayerData.equippedCharms)).ToArray();

			foreach(int num in charms)
			{
				GameManager.instance.UnequipCharm(num);
				PlayerData.instance.SetBool("equippedCharm_" + num, false);
			}

			//Extra stuff to make sure
			PlayerData.instance.CalculateNotchesUsed();
			GameManager.instance.RefreshOvercharm();

			CharmUpdate();
			PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
		}

		public override void StopEffect()
		{
			foreach(int num in charms)
			{
				GameManager.instance.EquipCharm(num);
				PlayerData.instance.SetBool("equippedCharm_" + num, true);
			}

			PlayerData.instance.CalculateNotchesUsed();
			GameManager.instance.RefreshOvercharm();

			CharmUpdate();
			PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
		}

		private void CharmUpdate()
		{
			//Custom charm update method to prevent healing the player
			HeroController hc = HeroController.instance;
			if(hc.playerData.GetBool("equippedCharm_26"))
			{
				ReflectionHelper.SetField(hc, "nailChargeTime", hc.NAIL_CHARGE_TIME_CHARM);
			}
			else
			{
				ReflectionHelper.SetField(hc, "nailChargeTime", hc.NAIL_CHARGE_TIME_DEFAULT);
			}
			if(hc.playerData.GetBool("equippedCharm_23") && !hc.playerData.GetBool("brokenCharm_23"))
			{
				hc.playerData.SetInt("maxHealth", hc.playerData.GetInt("maxHealthBase") + 2);
			}
			else
			{
				hc.playerData.SetInt("maxHealth", hc.playerData.GetInt("maxHealthBase"));
			}
			if(hc.playerData.GetBool("equippedCharm_27"))
			{
				hc.playerData.SetInt("joniHealthBlue", (int)(hc.playerData.GetInt("maxHealth") * 1.4f));
				hc.playerData.SetInt("maxHealth", 1);
				ReflectionHelper.SetField(hc, "joniBeam", true);
			}
			else
			{
				hc.playerData.SetInt("joniHealthBlue", 0);
			}
			if(hc.playerData.GetBool("equippedCharm_40") && hc.playerData.GetInt("grimmChildLevel") == 5)
			{
				hc.carefreeShieldEquipped = true;
			}
			else
			{
				hc.carefreeShieldEquipped = false;
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_Salubra's Curse";
		}

		public override List<string> GetCodeBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Speedrunner's Curse",
			};
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Unfriendly Fire",
				"ChallengeMode_Ascension"
			};
		}
	}
}
