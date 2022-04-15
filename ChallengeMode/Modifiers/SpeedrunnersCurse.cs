using System.Collections.Generic;
using SFCore.Utils;

namespace ChallengeMode.Modifiers
{
	class SpeedrunnersCurse : Modifier
	{
		private bool sbodyAlreadyEquipped;
		private bool thornsNotAlreadyEquipped;
		private PlayMakerFSM spellFSM;

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
			GameManager.instance.RefreshOvercharm();

			//Take hazard damage when using DDark
			spellFSM = HeroController.instance.spellControl;
			spellFSM.InsertMethod("Level Check 2", () =>
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
			GameManager.instance.RefreshOvercharm();

			spellFSM.RemoveAction("Level Check 2", 0);
		}

		public override string ToString()
		{
			return "ChallengeMode_Speedrunner's Curse";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Speedrunner's Curse", "ChallengeMode_Salubra's Curse"
			};
		}
	}
}
