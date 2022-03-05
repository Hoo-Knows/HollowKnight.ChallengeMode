using System.Collections;
using UnityEngine;
using Random = System.Random;
using System;
using Modding;

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
			"GG_Vengefly", "GG_Vengefly_V", "GG_Ghost_Gorb", "GG_Ghost_Gorb_V", "GG_Ghost_Xero", "GG_Ghost_Xero_V", 
			"GG_Flukemarm", "GG_Uumuu", "GG_Uumuu_V", "GG_Nosk_Hornet", "GG_Ghost_No_Eyes_V", "GG_Ghost_Markoth_V", 
			"GG_Grimm_Nightmare", "GG_Radiance"
		};
		private string sceneName;

		public void Initialize(string sceneName, int numActiveModifiers)
		{
			Unload();

			if(sceneName.Substring(0, 2) != "GG" || Array.IndexOf(sceneBlacklist, sceneName) != -1) return;

			Random random = new Random();
			this.sceneName = sceneName;
			activeModifiers = new Modifier[numActiveModifiers];
			Modifier[] modifiers = ChallengeMode.Instance.modifiers;

			//Select modifiers
			for(int i = 0; i < numActiveModifiers; i++)
			{
				Modifier modifier = modifiers[random.Next(0, modifiers.Length)];

				ChallengeMode.Instance.Log("Checking if " + modifier.ToString() + " is valid...");
				if(CheckValidModifier(modifier))
				{
					ChallengeMode.Instance.Log(modifier.ToString() + " is valid");
					activeModifiers[i] = modifier;
				}
				else
				{
					ChallengeMode.Instance.Log(modifier.ToString() + " is not valid");
					i--;
				}
			}
			GameManager.instance.OnFinishedEnteringScene += OnFinishedEnteringScene;
		}

		public bool CheckValidModifier(Modifier modifier)
		{
			Modifier[] modifiers = ChallengeMode.Instance.modifiers;
			bool result = true;

			//Check if modifier hasn't been selected
			if(ContainsModifier(modifier, activeModifiers)) result = false;

			//Nail Only cannot appear with Soul Master or Past Regrets
			if(modifier.ToString() == "ChallengeMode_Nail Only")
			{
				result = result && !ContainsModifier(modifiers[7], activeModifiers) && !ContainsModifier(modifiers[12], activeModifiers);
			}

			//Soul Master cannot appear with Nail Only
			if(modifier.ToString() == "ChallengeMode_Soul Master")
			{
				result = result && !ContainsModifier(modifiers[6], activeModifiers);
			}

			//Past Regrets cannot appear with Nail Only
			if(modifier.ToString() == "ChallengeMode_Past Regrets")
			{
				result = result && !ContainsModifier(modifiers[6], activeModifiers);
			}

			//Chaos, Chaos cannot appear with A Fool's Errand
			if(modifier.ToString() == "ChallengeMode_Chaos, Chaos")
			{
				result = result && !ContainsModifier(modifiers[17], activeModifiers);
			}

			//A Fool's Errand cannot appear with Chaos, Chaos or on a blacklisted scene
			if(modifier.ToString() == "ChallengeMode_A Fool's Errand")
			{
				result = result && !ContainsModifier(modifiers[14], activeModifiers) && Array.IndexOf(foolSceneBlacklist, sceneName) != -1;
			}

			return result;
		}

		private bool ContainsModifier(Modifier modifier, Modifier[] modifiers)
		{
			for(int i = 0; i < modifiers.Length; i++)
			{
				if(modifiers[i] != null)
				{
					if(modifier.ToString() == modifiers[i].ToString()) return true;
				}
			}
			return false;
		}

		private void OnFinishedEnteringScene()
		{
			StartCoroutine(ActivateModifiers());
		}

		private IEnumerator ActivateModifiers()
		{
			ChallengeMode.Instance.Log("Activating modifiers");

			//Reflection magic to award achievements
			AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
			AchievementHandler.AchievementAwarded aa =
				ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");

			Time.timeScale = 0.2f;

			foreach(Modifier modifier in activeModifiers)
			{
				ChallengeMode.Instance.Log("Starting " + modifier.ToString().Substring(14));

				aa.Invoke(modifier.ToString());
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
			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			yield break;
		}

		public void Unload()
		{
			if(activeModifiers != null)
			{
				ChallengeMode.Instance.Log("Stopping modifiers");
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

			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}
