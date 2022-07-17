using System;
using System.Collections;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class ChaosChaos : Modifier
	{
		private Modifier activeModifier;
		private AchievementHandler.AchievementAwarded aa;
		private bool flag;

		public override void StartEffect()
		{
			AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
			aa = ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");

			flag = true;
			StartCoroutine(BeginChaos());
		}

		private IEnumerator BeginChaos()
		{
			//Activate first modifier instantly, but wait until later to display
			SetActiveModifier();
			try
			{
				activeModifier.StartEffect();
			}
			catch(Exception e)
			{
				ChallengeMode.Instance.Log("Chaos, Chaos - Failed to start " + activeModifier.ToString().
					Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
				ChallengeMode.Instance.Log(e.ToString());
			}
			yield return new WaitUntil(() => ChallengeMode.modifierControl.displayed);
			yield return new WaitForSeconds(3f);
			aa.Invoke(activeModifier.ToString());

			//Modifiers after first one work like normal
			while(flag)
			{
				yield return new WaitForSeconds(15f);
				try
				{
					activeModifier.StopEffect();
				}
				catch(Exception e)
				{
					ChallengeMode.Instance.Log("Chaos, Chaos - Failed to stop " + activeModifier.ToString().
						Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
					ChallengeMode.Instance.Log(e.ToString());
				}

				SetActiveModifier();
				if(activeModifier == null) break;

				aa.Invoke(activeModifier.ToString());

				try
				{
					activeModifier.StartEffect();
				}
				catch(Exception e)
				{
					ChallengeMode.Instance.Log("Chaos, Chaos - Failed to start " + activeModifier.ToString().
						Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
					ChallengeMode.Instance.Log(e.ToString());
				}
			}
			yield break;
		}

		private void SetActiveModifier()
		{
			activeModifier = null;
			int loops = 0;
			while(activeModifier == null)
			{
				activeModifier = ChallengeMode.modifierControl.SelectModifier();
				loops++;
				if(loops > 100) break;
			}
		}

		public override void StopEffect()
		{
			flag = false;
			StopAllCoroutines();

			try
			{
				activeModifier.StopEffect();
			}
			catch(Exception e)
			{
				ChallengeMode.Instance.Log("Chaos, Chaos - Failed to stop " + activeModifier.ToString().
					Split(new char[] { '_' })[1] + " for " + GameManager.instance.sceneName);
				ChallengeMode.Instance.Log(e.ToString());
			}
		}

		public override string ToString()
		{
			return "ChallengeMode_Chaos, Chaos";
		}
	}
}
