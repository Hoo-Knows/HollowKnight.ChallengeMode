using System.Collections;
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
			yield return new WaitUntil(() => ChallengeMode.modifierControl.displayed);
			yield return new WaitForSeconds(3f);
			while(flag)
			{
				activeModifier = null;
				int loops = 0;
				while(activeModifier == null)
				{
					activeModifier = ChallengeMode.modifierControl.SelectModifier();
					loops++;
					if(loops > 100) break;
				}
				if(loops > 100) break;

				AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
				AchievementHandler.AchievementAwarded aa =
					ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");
				aa.Invoke(activeModifier.ToString());

				try
				{
					activeModifier.StartEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Chaos, Chaos - Failed to start " + activeModifier.ToString().
						Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
				}
				yield return new WaitForSeconds(15f);
				try
				{
					activeModifier.StopEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Chaos, Chaos - Failed to stop " + activeModifier.ToString().
						Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
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
				activeModifier.StopEffect();
			}
			catch
			{
				ChallengeMode.Instance.Log("Chaos, Chaos - Failed to stop " + activeModifier.ToString().
						Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_Chaos, Chaos";
		}
	}
}
