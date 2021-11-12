using System.Collections;
using UnityEngine;
using Modding;
using SFCore;

namespace ChallengeMode
{
	class ModifierControl : MonoBehaviour
	{
		private Modifier[] modifiers;

		public void ActivateModifiers(Modifier[] modifiers)
		{
			this.modifiers = modifiers;

			On.HeroController.FinishedEnteringScene += FinishedEnteringScene;
		}

		private void FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
		{
			orig(self, setHazardMarker, preventRunBob);
			StartCoroutine(ActivateModifiers());
		}

		private IEnumerator ActivateModifiers()
		{
			Time.timeScale = 0.1f;
			foreach(Modifier modifier in modifiers)
			{
				GameManager.instance.AwardAchievement(modifier.ToString());
				modifier.StartEffect();
				yield return new WaitForSecondsRealtime(0.75f);
			}
			yield return new WaitForSecondsRealtime(2f);
			Time.timeScale = 1f;
			Reset();
			yield break;
		}

		public void Reset()
		{
			modifiers = null;

			On.HeroController.FinishedEnteringScene -= FinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}
