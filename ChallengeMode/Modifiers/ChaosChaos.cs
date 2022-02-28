using System.Collections;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class ChaosChaos : Modifier
	{
		private Modifier[] modifiers;
		private Modifier activeModifier;
		private bool flag;

		public override void StartEffect()
		{
			//Reflection to get list of modifiers and active modifiers
			modifiers = ChallengeMode.Instance.modifiers;

			flag = true;
			StartCoroutine(BeginChaos());
		}

		private IEnumerator BeginChaos()
		{
			yield return new WaitForSeconds(2f);
			while(flag)
			{
				activeModifier = PickNewModifier();
				GameManager.instance.AwardAchievement(activeModifier.ToString());
				activeModifier.StartEffect();
				yield return new WaitForSeconds(15f);
				activeModifier.StopEffect();
			}
			yield break;
		}

		private Modifier PickNewModifier()
		{
			Random random = new Random();
			Modifier modifier = null;
			bool foundModifier = false;

			while(!foundModifier)
			{
				modifier = modifiers[random.Next(0, modifiers.Length)];

				foundModifier = ChallengeMode.Instance.modifierControl.CheckValidModifier(modifier);
			}
			return modifier;
		}

		public override void StopEffect()
		{
			flag = false;
			activeModifier.StopEffect();

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Chaos, Chaos";
		}
	}
}
