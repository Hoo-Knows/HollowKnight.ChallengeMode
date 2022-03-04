namespace ChallengeMode.Modifiers
{
	class SpeedrunnersCurse : Modifier
	{
		private bool sbodyAlreadyEquipped;
		private bool thornsNotAlreadyEquipped;

		public override void StartEffect()
		{
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
			HeroController.instance.CharmUpdate();

			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			if(s == ModCommon.ModCommon.Spell.Quake)
			{
				HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 2);
				return false;
			}
			return true;
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
			HeroController.instance.CharmUpdate();

			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
		}

		public override string ToString()
		{
			return "ChallengeMode_Speedrunner's Curse";
		}
	}
}
