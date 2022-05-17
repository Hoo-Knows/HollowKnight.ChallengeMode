using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class ChaosChaos : Modifier
	{
		private Modifier activeModifier;
		private bool flag;

		public override void StartEffect()
		{
			flag = true;

			StartCoroutine(BeginChaos());
		}

		private IEnumerator BeginChaos()
		{
			yield return new WaitForSeconds(2f);
			while(flag)
			{
				activeModifier = null;
				int loops = 0;
				while(activeModifier == null)
				{
					activeModifier = ChallengeMode.Instance.modifierControl.SelectModifier();
					loops++;
					if(loops > 1000) break;
				}
				if(loops > 1000) break;

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
				yield return new WaitForSeconds(15f);
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
