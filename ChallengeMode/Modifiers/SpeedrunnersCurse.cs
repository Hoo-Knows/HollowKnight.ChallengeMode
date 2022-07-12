using System.Collections.Generic;
using SFCore.Utils;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SpeedrunnersCurse : Modifier
	{
		private bool sbodyAlreadyEquipped;
		private bool thornsNotAlreadyEquipped;

		public override void StartEffect()
		{
			//Unequip SBody, equip Thorns of Agony
			sbodyAlreadyEquipped = PlayerData.instance.GetBool("equippedCharm_14");
			thornsNotAlreadyEquipped = !PlayerData.instance.GetBool("equippedCharm_12");

			if(sbodyAlreadyEquipped)
			{
				PlayerData.instance.SetBool("equippedCharm_14", false);
				GameManager.instance.UnequipCharm(14);
			}
			if(thornsNotAlreadyEquipped)
			{
				PlayerData.instance.SetBool("equippedCharm_12", true);
				GameManager.instance.EquipCharm(12);
			}
			CharmUpdate();

			//Take hazard damage when using DDark
			HeroController.instance.spellControl.InsertMethod("Level Check 2", () =>
			{
				HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 2);
			}, 0);
		}

		public override void StopEffect()
		{
			if(sbodyAlreadyEquipped)
			{
				PlayerData.instance.SetBool("equippedCharm_14", true);
				GameManager.instance.EquipCharm(14);
			}
			if(thornsNotAlreadyEquipped)
			{
				PlayerData.instance.SetBool("equippedCharm_12", false);
				GameManager.instance.UnequipCharm(12);
			}

			ChallengeMode.Instance.Log("Speedrunner's Curse - before CharmUpdate " + GameManager.instance.sceneName);
			CharmUpdate();

			ChallengeMode.Instance.Log("Speedrunner's Curse - before FSM edit " + GameManager.instance.sceneName);
			HeroController.instance.spellControl.RemoveAction("Level Check 2", 0);

			ChallengeMode.Instance.Log("Speedrunner's Curse - end of StopEffect " + GameManager.instance.sceneName);
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
			ChallengeMode.Instance.Log("Speedrunner's Curse - before Charm Equip Check broadcast " + GameManager.instance.sceneName);
			PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
		}

		public override string ToString()
		{
			return "ChallengeMode_Speedrunner's Curse";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Salubra's Curse",
				"ChallengeMode_Nail Only",
				"ChallengeMode_Nailmaster"
			};
		}
	}
}
