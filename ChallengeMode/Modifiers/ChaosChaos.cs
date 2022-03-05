using System.Collections;
using Modding;
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

				AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
				AchievementHandler.AchievementAwarded aa =
					ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");
				aa.Invoke(activeModifier.ToString());

				ChallengeMode.Instance.Log("Starting " + activeModifier.ToString().Substring(14));
				try
				{
					activeModifier.StartEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Failed to start " + activeModifier.ToString().Substring(14));
				}
				yield return new WaitForSeconds(20f);
				try
				{
					ChallengeMode.Instance.Log("Stopping " + activeModifier.ToString().Substring(14));
					activeModifier.StopEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Failed to stop " + activeModifier.ToString().Substring(14));
				}
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

				foundModifier = ChallengeMode.Instance.modifierControl.CheckValidModifier(modifier) && modifier != activeModifier;
			}
			return modifier;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			flag = false;
			try
			{
				ChallengeMode.Instance.Log("Stopping " + activeModifier.ToString().Substring(14));
				activeModifier.StopEffect();
			}
			catch
			{
				ChallengeMode.Instance.Log("Failed to stop " + activeModifier.ToString().Substring(14));
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_Chaos, Chaos";
		}
	}
}
