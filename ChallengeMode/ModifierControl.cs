using System.Collections;
using UnityEngine;
using System;

namespace ChallengeMode
{
	class ModifierControl : MonoBehaviour
	{
		private Modifier[] activeModifiers;

		public void Initialize(Modifier[] modifiers, int numActiveModifiers)
		{
			var random = new System.Random();
			activeModifiers = new Modifier[numActiveModifiers];

			//Select modifiers
			for(int i = 0; i < numActiveModifiers; i++)
			{
				Modifier modifier = modifiers[random.Next(0, modifiers.Length)];

				//Check if modifier has already been selected
				if(Array.IndexOf(activeModifiers, modifier) == -1)
				{
					//Soul Master and Nail Only are mutually exclusive
					if(modifier.ToString() == "ChallengeMode_Soul Master" 
					&& Array.IndexOf(activeModifiers, modifiers[6]) != -1)
					{
						i--; continue;
					}
					if(modifier.ToString() == "ChallengeMode_Nail Only" 
					&& Array.IndexOf(activeModifiers, modifiers[7]) != -1)
					{
						i--; continue;
					}

					activeModifiers[i] = modifier;
				}
				else i--;
			}

			On.HeroController.FinishedEnteringScene += FinishedEnteringScene;
		}

		private void FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
		{
			orig(self, setHazardMarker, preventRunBob);
			StartCoroutine(ActivateModifiers());
		}

		private IEnumerator ActivateModifiers()
		{
			//yield return null;
			Time.timeScale = 0.2f;
			foreach(Modifier modifier in activeModifiers)
			{
				GameManager.instance.AwardAchievement(modifier.ToString());
				modifier.StartEffect();
				yield return new WaitForSecondsRealtime(0.75f);
			}
			yield return new WaitForSecondsRealtime(2f);
			if(!GameManager.instance.isPaused) Time.timeScale = 1f;
			On.HeroController.FinishedEnteringScene -= FinishedEnteringScene;
			yield break;
		}

		public void Unload()
		{
			if(activeModifiers != null)
			{
				foreach(Modifier modifier in activeModifiers)
					modifier.StopEffect();
			}
			activeModifiers = null;

			On.HeroController.FinishedEnteringScene -= FinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}
