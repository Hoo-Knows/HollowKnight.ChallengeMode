using Modding;

namespace ChallengeMode.Modifiers
{
	class FrailShell : Modifier
	{
		public override void StartEffect()
		{
			ModHooks.Instance.TakeHealthHook += damageTaken;
		}

		private int damageTaken(int damage)
		{
			return damage *= 2;
		}

		public override void StopEffect()
		{
			ModHooks.Instance.TakeHealthHook -= damageTaken;
		}
	}
}
