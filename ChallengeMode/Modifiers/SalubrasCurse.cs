using System.Collections.Generic;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SalubrasCurse : Modifier
	{
		private List<int> charms;

		public override void StartEffect()
		{
			//Store current charms and unequip them
			charms = new List<int>();
			for(int i = 1; i < 41; i++)
			{
				if(PlayerData.instance.GetBool("equippedCharm_" + i))
				{
					charms.Add(i);
				}
				PlayerData.instance.SetBool("equippedCharm_" + i, false);
				GameManager.instance.UnequipCharm(i);
			}
			CharmUpdate();
		}

		public override void StopEffect()
		{
			foreach(int i in charms)
			{
				PlayerData.instance.SetBool("equippedCharm_" + i, true);
				GameManager.instance.EquipCharm(i);
			}
			CharmUpdate();
		}

		private void CharmUpdate()
		{
			//Custom charm update method to prevent healing
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
				hc.playerData.SetInt("joniHealthBlue", (int)((float)hc.playerData.GetInt("maxHealth") * 1.4f));
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

			GameManager.instance.RefreshOvercharm();
			PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
		}

		public override string ToString()
		{
			return "ChallengeMode_Salubra's Curse";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Speedrunner's Curse",
				"ChallengeMode_Unfriendly Fire"
			};
		}
	}
}
