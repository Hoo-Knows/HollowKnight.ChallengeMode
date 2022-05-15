﻿using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode
{
	public class ModifierControl : MonoBehaviour
	{
		private ChallengeMode cm;
		private Modifier[] activeModifiers;
		private Random random;

		private string currentScene;

		private readonly List<string> sceneBlacklist = new List<string>()
		{
			"GG_Atrium", "GG_Atrium_Roof", "GG_Unlock_Wastes", "GG_Blue_Room", "GG_Workshop", "GG_Land_Of_Storms",
			"GG_Engine", "GG_Engine_Prime", "GG_Unn", "GG_Engine_Root", "GG_Wyrm", "GG_Spa", "GG_Boss_Door_Entrance"
		};
		private readonly List<string> foolSceneBlacklist = new List<string>()
		{
			"GG_Vengefly", "GG_Vengefly_V", "GG_Ghost_Gorb", "GG_Ghost_Gorb_V", "GG_Ghost_Xero", "GG_Ghost_Xero_V",
			"GG_Flukemarm", "GG_Uumuu", "GG_Uumuu_V", "GG_Nosk_Hornet", "GG_Ghost_No_Eyes_V", "GG_Ghost_Markoth_V",
			"GG_Grimm_Nightmare", "GG_Radiance"
		};
		private readonly List<string> sceneUniqueList = new List<string>()
		{
			"GG_Nailmasters", "GG_Painter", "GG_Sly", "GG_Grey_Prince_Zote", "GG_Grimm_Nightmare",
			"GG_Hollow_Knight", "GG_Radiance"
		};

		public void Initialize(int numActiveModifiers, string currentScene)
		{
			Unload();

			this.currentScene = currentScene;

			if(currentScene.Substring(0, 2) != "GG" || sceneBlacklist.Contains(currentScene)) return;

			cm = ChallengeMode.Instance;
			activeModifiers = new Modifier[numActiveModifiers];
			random = new Random();

			//Select modifiers
			if(sceneUniqueList.Contains(currentScene))
			{
				int i = sceneUniqueList.IndexOf(currentScene) - 2;
				if(i < 0) i = 0;
				activeModifiers[0] = cm.modifiersU[i];

				//Nailmasters can have extra modifiers
				if(currentScene == "GG_Nailmasters" || currentScene == "GG_Painter" || currentScene == "GG_Sly")
				{
					for(int j = 1; j < numActiveModifiers; j++)
					{
						Modifier modifier = SelectModifier();
						if(modifier != null)
						{
							activeModifiers[j] = modifier;
						}
						else
						{
							j--;
						}
					}
				}
				//Absrad has High Stress
				if(currentScene == "GG_Radiance" && numActiveModifiers > 1)
				{
					activeModifiers[1] = cm.modifiers[0];
				}
			}
			else
			{
				for(int i = 0; i < numActiveModifiers; i++)
				{
					Modifier modifier = SelectModifier();

					if(modifier != null)
					{
						activeModifiers[i] = modifier;
						//Frail Shell must appear before High Stress and Poor Memory
						if(modifier.ToString() == "ChallengeMode_Frail Shell")
						{
							for(int j = 0; j < numActiveModifiers; j++)
							{
								if(activeModifiers[j].ToString() == "ChallengeMode_High Stress" ||
									activeModifiers[j].ToString() == "ChallengeMode_Poor Memory")
								{
									(activeModifiers[i], activeModifiers[j]) = (activeModifiers[j], activeModifiers[i]);
									break;
								}
							}
						}
					}
					else i--;
				}
			}
			GameManager.instance.OnFinishedEnteringScene += OnFinishedEnteringScene;
		}

		public Modifier SelectModifier()
		{
			Modifier modifier = cm.modifiers[random.Next(0, cm.modifiers.Length)];

			if(CheckValidModifier(modifier))
			{
				cm.Log(modifier.ToString() + " is valid");
				return modifier;
			}
			cm.Log(modifier.ToString() + " is not valid");
			return null;
		}

		private bool CheckValidModifier(Modifier modifier)
		{
			//A Fool's Errand
			if(modifier.ToString() == "ChallengeMode_A Fool's Errand")
			{
				if(foolSceneBlacklist.Contains(currentScene)) return false;
			}

			foreach(Modifier m in activeModifiers)
			{
				if(m != null && m.GetBlacklistedModifiers().Contains(modifier.ToString())) return false;
			}
			return true;
		}

		private void OnFinishedEnteringScene()
		{
			StartCoroutine(StartModifiers());
		}

		private IEnumerator StartModifiers()
		{
			cm.Log("Starting modifiers for " + currentScene);

			//Reflection magic to award achievements
			AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
			AchievementHandler.AchievementAwarded aa =
				ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");

			Time.timeScale = 0.2f;

			foreach(Modifier modifier in activeModifiers)
			{
				cm.Log("Starting " + modifier.ToString().Substring(14));

				aa.Invoke(modifier.ToString());
				try
				{
					modifier.StartEffect();
				}
				catch
				{
					cm.Log("Failed to start " + modifier.ToString().Substring(14));
				}
				yield return new WaitForSecondsRealtime(0.75f);
			}
			yield return new WaitForSecondsRealtime(2f);
			if(!GameManager.instance.isPaused) Time.timeScale = 1f;
			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			yield break;
		}

		private void StopModifiers()
		{
			if(activeModifiers != null)
			{
				cm.Log("Stopping modifiers for " + currentScene);
				foreach(Modifier modifier in activeModifiers)
				{
					if(modifier != null)
					{
						cm.Log("Stopping " + modifier.ToString().Substring(14));
						try
						{
							modifier.StopEffect();
						}
						catch
						{
							cm.Log("Failed to stop " + modifier.ToString().Substring(14));
						}
					}
				}
			}
			activeModifiers = null;
		}

		public void Unload()
		{
			StopModifiers();

			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}