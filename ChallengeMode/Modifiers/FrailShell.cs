using System.Collections.Generic;
using Modding;

namespace ChallengeMode.Modifiers
{
	class FrailShell : Modifier
	{
		public override void StartEffect()
		{
			ModHooks.TakeDamageHook += TakeDamageHook;
		}

		private int TakeDamageHook(ref int hazardType, int damage)
		{
			return damage + 1;
		}

		public override void StopEffect()
		{
			ModHooks.TakeDamageHook -= TakeDamageHook;
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
