using System;
using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode
{
	public class ModifierControl : MonoBehaviour
	{
		private Modifier[] modifiers;
		private Modifier[] activeModifiers;

		private int spaCount;
		private int numActiveModifiers;
		private string currentScene;
		private Random random;

		private List<string> sceneBlacklist;
		private List<string> foolSceneBlacklist;

		public void Initialize()
		{
			Unload();

			modifiers = ChallengeMode.Instance.modifiers;
			spaCount = 0;
			numActiveModifiers = 1;
			currentScene = "none lol";
			activeModifiers = new Modifier[numActiveModifiers];
			random = new Random();

			sceneBlacklist = new List<string>()
			{
				"GG_Atrium", "GG_Atrium_Roof", "GG_Unlock_Wastes", "GG_Blue_Room", "GG_Workshop", "GG_Land_Of_Storms",
				"GG_Engine", "GG_Engine_Prime", "GG_Unn", "GG_Engine_Root", "GG_Wyrm", "GG_Spa"
			};
			foolSceneBlacklist = new List<string>()
			{
				"GG_Vengefly", "GG_Vengefly_V", "GG_Ghost_Gorb", "GG_Ghost_Gorb_V", "GG_Ghost_Xero", "GG_Ghost_Xero_V",
				"GG_Flukemarm", "GG_Uumuu", "GG_Uumuu_V", "GG_Nosk_Hornet", "GG_Ghost_No_Eyes_V", "GG_Ghost_Markoth_V",
				"GG_Grimm_Nightmare", "GG_Radiance"
			};

			ModHooks.BeforeSceneLoadHook += BeforeSceneLoadHook;
		}

		private string BeforeSceneLoadHook(string sceneName)
		{
			Stop();

			if(sceneName == "GG_Spa") spaCount++;
			if(sceneName == "GG_Atrium" || sceneName == "GG_Atrium_Roof" || sceneName == "GG_Workshop") spaCount = 0;
			//P1 section in P5
			if(spaCount == 0) numActiveModifiers = 1;
			//P2 and P3 sections in P5
			if(spaCount == 1) numActiveModifiers = 2;
			//P4 section in P5
			if(spaCount == 5) numActiveModifiers = 3;
			currentScene = sceneName;

			Start();

			return sceneName;
		}

		private void Start()
		{
			if(currentScene.Substring(0, 2) != "GG" || sceneBlacklist.Contains(currentScene)) return;

			//Select modifiers
			for(int i = 0; i < numActiveModifiers; i++)
			{
				Modifier modifier = modifiers[random.Next(0, modifiers.Length)];

				ChallengeMode.Instance.Log("Checking if " + modifier.ToString() + " is valid...");
				if(CheckValidModifier(modifier))
				{
					ChallengeMode.Instance.Log(modifier.ToString() + " is valid");
					activeModifiers[i] = modifier;
					//Frail Shell must appear before High Stress
					if(modifier.ToString() == "ChallengeMode_Frail Shell")
					{
						for(int j = 0; j < numActiveModifiers; j++)
						{
							if(activeModifiers[j] != null && activeModifiers[j].ToString() == "ChallengeMode_High Stress")
							{
								ChallengeMode.Instance.Log("Swapping Frail Shell and High Stress");
								(activeModifiers[i], activeModifiers[j]) = (activeModifiers[j], activeModifiers[i]);
								break;
							}
						}
					}
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
			//A Fool's Errand
			if(modifier.ToString() == "ChallengeMode_A Fool's Errand")
			{
				if(foolSceneBlacklist.Contains(currentScene))
				{
					return false;
				}
			}

			foreach(Modifier m in activeModifiers)
			{
				if(m != null && m.GetBlacklistedModifiers().Contains(modifier.ToString()))
				{
					return false;
				}
			}

			return true;
		}

		private void OnFinishedEnteringScene()
		{
			StartCoroutine(StartModifiers());
		}

		private IEnumerator StartModifiers()
		{
			ChallengeMode.Instance.Log("Starting modifiers");

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

		private void Stop()
		{
			if(activeModifiers != null && activeModifiers.Length != 0)
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
			activeModifiers = new Modifier[numActiveModifiers];
		}

		public void Unload()
		{
			Stop();

			ModHooks.BeforeSceneLoadHook -= BeforeSceneLoadHook;
			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}
