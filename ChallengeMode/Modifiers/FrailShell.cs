using System.Collections.Generic;
using Modding;

namespace ChallengeMode.Modifiers
{
	class FrailShell : Modifier
	{
		public override void StartEffect()
		{
			ModHooks.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			return damage + 1;
		}

		public override void StopEffect()
		{
			ModHooks.TakeHealthHook -= TakeHealthHook;
		}

		public override string ToString()
		{
			return "ChallengeMode_Frail Shell";
		}

		public override List<string> GetCodeBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress"
			};
		}
	}
}
