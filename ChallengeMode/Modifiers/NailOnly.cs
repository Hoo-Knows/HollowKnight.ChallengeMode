namespace ChallengeMode.Modifiers
{
	class NailOnly : Modifier
	{
		public override void StartEffect()
		{
			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
			On.HeroController.CanFocus += CanFocus;
		}

		private bool CanFocus(On.HeroController.orig_CanFocus orig, HeroController self)
		{
			orig(self);
			return false;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			return false;
		}

		public override void StopEffect()
		{
			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
			On.HeroController.CanFocus -= CanFocus;
		}
	}
}
