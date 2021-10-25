using Modding;

namespace ChallengeMode.Modifiers
{
	class FrailShell : Modifier
	{
		public override void StartEffect()
		{
			ModHooks.Instance.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			return damage *= 2;
		}

		public override void StopEffect()
		{
			ModHooks.Instance.TakeHealthHook -= TakeHealthHook;
		}
	}
}
