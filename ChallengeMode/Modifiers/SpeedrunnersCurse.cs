using System.Collections.Generic;
using SFCore.Utils;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SpeedrunnersCurse : Modifier
	{
		private List<int> charms;

		public override void StartEffect()
		{
			//Store current charms and unequip them
			charms = new List<int>(PlayerData.instance.GetVariable<List<int>>(nameof(PlayerData.equippedCharms)).ToArray());

			PlayerData.instance.SetBool(nameof(PlayerData.equippedCharm_12), true);
			PlayerData.instance.SetBool(nameof(PlayerData.equippedCharm_14), false);
			PlayerData.instance.SetBool(nameof(PlayerData.equippedCharm_32), false);

			if(!charms.Contains(12)) GameManager.instance.EquipCharm(12);
			if(charms.Contains(14)) GameManager.instance.UnequipCharm(14);
			if(charms.Contains(32)) GameManager.instance.UnequipCharm(32);

			PlayerData.instance.CalculateNotchesUsed();
			GameManager.instance.RefreshOvercharm();

			CharmUpdate();
			PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");

			//Take hazard damage when using DDark
			HeroController.instance.spellControl.InsertMethod("Level Check 2", () =>
			{
				HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 2);
			}, 0);
		}

		public override void StopEffect()
		{
			PlayerData.instance.GetVariable<List<int>>(nameof(PlayerData.equippedCharms)).Clear();

			for(int num = 1; num <= 40; num++)
			{
				PlayerData.instance.SetBool("equippedCharm_" + num, false);
			}
			foreach(int num in charms)
			{
				GameManager.instance.EquipCharm(num);
				PlayerData.instance.SetBool("equippedCharm_" + num, true);
			}

			PlayerData.instance.CalculateNotchesUsed();
			GameManager.instance.RefreshOvercharm();

			ChallengeMode.Instance.Log("Speedrunner's Curse - before CharmUpdate " + GameManager.instance.sceneName);
			CharmUpdate();
			ChallengeMode.Instance.Log("Speedrunner's Curse - before Charm Equip Check broadcast " + GameManager.instance.sceneName);
			PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
			ChallengeMode.Instance.Log("Speedrunner's Curse - before Charm Indicator Check broadcast " + GameManager.instance.sceneName);
			PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");

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
			return "ChallengeMode_Speedrunner's Curse";
		}

		public override List<string> GetCodeBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Salubra's Curse"
			};
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Nail Only",
				"ChallengeMode_Nailmaster",
				"ChallengeMode_Ascension"
			};
		}
	}
}
