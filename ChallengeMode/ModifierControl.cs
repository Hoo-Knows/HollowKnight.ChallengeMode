using System.Collections;
using UnityEngine;
using Random = System.Random;
using System;

namespace ChallengeMode
{
	public class ModifierControl : MonoBehaviour
	{
		private Modifier[] activeModifiers;
		private readonly string[] sceneBlacklist = 
		{
			"GG_Atrium", "GG_Atrium_Roof", "GG_Unlock_Wastes", "GG_Blue_Room", "GG_Workshop", "GG_Land_Of_Storms",
			"GG_Engine", "GG_Engine_Prime", "GG_Unn", "GG_Engine_Root", "GG_Wyrm", "GG_Spa"
		};
		private readonly string[] foolSceneBlacklist =
		{
			"GG_Flukemarm", "GG_Uumuu", "GG_Uumuu_V", "GG_Nosk_Hornet", "GG_Ghost_No_Eyes_V", "GG_Ghost_Markoth_V"
		};
		private string sceneName;

		public void Initialize(string sceneName, Modifier[] modifiers, int numActiveModifiers)
		{
			Unload();

			if(sceneName.Substring(0, 2) != "GG" || Array.IndexOf(sceneBlacklist, sceneName) != -1) return;

			Random random = new Random();
			activeModifiers = new Modifier[numActiveModifiers];
			this.sceneName = sceneName;

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

			//Chaos, Chaos cannot appear with A Fool's Errand
			if(modifier.ToString() == "ChallengeMode_Chaos, Chaos"
			&& Array.IndexOf(activeModifiers, modifiers[17]) != -1)
			{
				result = false;
			}

			//A Fool's Errand cannot appear with Chaos, Chaos or on a blacklisted scene
			if(modifier.ToString() == "ChallengeMode_A Fool's Errand"
			&& Array.IndexOf(activeModifiers, modifiers[14]) != -1 || Array.IndexOf(foolSceneBlacklist, sceneName) != -1)
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
			ChallengeMode.Instance.Log("Activating modifiers");
			Time.timeScale = 0.2f;
			foreach(Modifier modifier in activeModifiers)
			{
				GameManager.instance.AwardAchievement(modifier.ToString());
				ChallengeMode.Instance.Log("Starting " + modifier.ToString().Substring(14));
				try
				{
					modifier.StartEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Failed to start " + modifier.ToString().Substring(14));
				}
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
				{
					if(modifier != null)
					{
						ChallengeMode.Instance.Log("Stopping " + modifier.ToString().Substring(14));
						try
						{
							modifier.StopEffect();
						}
						catch
						{
							ChallengeMode.Instance.Log("Failed to stop " + modifier.ToString().Substring(14));
						}
					}
				}
			}
			activeModifiers = null;

			On.HeroController.FinishedEnteringScene -= FinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}
