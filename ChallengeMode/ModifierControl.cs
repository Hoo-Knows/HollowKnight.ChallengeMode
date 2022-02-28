using System.Collections;
using UnityEngine;
using Random = System.Random;
using System;

namespace ChallengeMode
{
	public class ModifierControl : MonoBehaviour
	{
		private Modifier[] activeModifiers;

		public void Initialize()
		{
			Modifier[] modifiers = ChallengeMode.Instance.modifiers;
			int numActiveModifiers = ChallengeMode.Instance.numActiveModifiers;

			Random random = new Random();
			activeModifiers = new Modifier[numActiveModifiers];

			//Select modifiers
			for(int i = 0; i < numActiveModifiers; i++)
			{
				Modifier modifier = modifiers[random.Next(0, modifiers.Length)];

				if(CheckValidModifier(modifier)) activeModifiers[i] = modifier;
				else i--;
			}

			On.HeroController.FinishedEnteringScene += FinishedEnteringScene;
		}

		public bool CheckValidModifier(Modifier modifier)
		{
			Modifier[] modifiers = ChallengeMode.Instance.modifiers;
			bool result = false;

			//Check if modifier hasn't been selected
			if(Array.IndexOf(activeModifiers, modifier) == -1)
			{
				result = true;
			}

			//Nail Only cannot appear with Soul Master or Past Regrets
			if(modifier.ToString() == "ChallengeMode_Nail Only"
			&& (Array.IndexOf(activeModifiers, modifiers[6]) != -1 || Array.IndexOf(activeModifiers, modifiers[12]) != -1))
			{
				result = false;
			}

			//Soul Master cannot appear with Nail Only
			if(modifier.ToString() == "ChallengeMode_Soul Master"
			&& Array.IndexOf(activeModifiers, modifiers[6]) != -1)
			{
				result = false;
			}

			//Past Regrets cannot appear with Nail Only
			if(modifier.ToString() == "ChallengeMode_Past Regrets"
			&& Array.IndexOf(activeModifiers, modifiers[6]) != -1)
			{
				result = false;
			}

			return result;
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
