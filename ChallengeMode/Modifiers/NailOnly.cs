namespace ChallengeMode.Modifiers
{
	class NailOnly : Modifier
	{
		public override void StartEffect()
		{
			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			return false;
		}

		public override void StopEffect()
		{
			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
		}

		public override string ToString()
		{
			return "ChallengeMode_Nail Only";
		}
	}
}
