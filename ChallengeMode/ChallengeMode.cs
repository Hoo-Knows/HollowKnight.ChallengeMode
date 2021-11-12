using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, ITogglableMod
	{
		private Modifier[] modifiers;
		private Modifier[] activeModifiers;
		private int numActiveModifiers;
		private ModifierControl modifierControl;

		private HashSet<string> blacklistedScenes;
		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		public ChallengeMode() : base("ChallengeMode") { }

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Instance = this;
			this.preloadedObjects = preloadedObjects;
			blacklistedScenes = new HashSet<string>()
			{
				"GG_Atrium", "GG_Atrium_Roof", "GG_Engine", "GG_Engine_Root", "GG_Spa", "GG_Waterways", "GG_Workshop", "GG_Wyrm"
			};

			Unload();

			//All modifiers
			modifiers = new Modifier[9];
			modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.HighStress>();
			modifiers[1] = GameManager.instance.gameObject.AddComponent<Modifiers.FrailShell>();
			modifiers[2] = GameManager.instance.gameObject.AddComponent<Modifiers.AdrenalineRush>();
			modifiers[3] = GameManager.instance.gameObject.AddComponent<Modifiers.AspidRancher>();
			modifiers[4] = GameManager.instance.gameObject.AddComponent<Modifiers.VoidVision>();
			modifiers[5] = GameManager.instance.gameObject.AddComponent<Modifiers.SpeedrunnersCurse>();
			modifiers[6] = GameManager.instance.gameObject.AddComponent<Modifiers.NailOnly>();
			modifiers[7] = GameManager.instance.gameObject.AddComponent<Modifiers.SoulMaster>();
			modifiers[8] = GameManager.instance.gameObject.AddComponent<Modifiers.HungryKnight>();

			//Test individual modifier
			//modifiers = new Modifier[1];
			//modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.AdrenalineRush>();

			numActiveModifiers = 1;
			modifierControl = GameManager.instance.gameObject.AddComponent<ModifierControl>();

			AchievementHelper.Initialize();
			foreach(Modifier modifier in modifiers)
			{
				AchievementHelper.AddAchievement(modifier.ToString(), new Sprite(), modifier.ToString(), "ChallengeMode_AchievementText", false);
			}
			UIManager.instance.RefreshAchievementsList();

			ModHooks.Instance.BeforeSceneLoadHook += BeforeSceneLoad;
			ModHooks.Instance.LanguageGetHook += LanguageGetHook;
			ModHooks.Instance.BeforePlayerDeadHook += BeforePlayerDeadHook;
		}

		public override List<(string, string)> GetPreloadNames()
		{
			return new List<(string, string)>
			{
				("Deepnest_East_04","Super Spitter")
			};
		}

		private string BeforeSceneLoad(string sceneName)
		{
			modifierControl.Reset();
			if(activeModifiers != null)
			{
				foreach(Modifier modifier in activeModifiers)
					modifier.StopEffect();
			}
			activeModifiers = null;

			if(sceneName == "GG_Spa") numActiveModifiers++;
			//Log(numActiveModifiers);

			if(sceneName.Substring(0, 2) == "GG" && !blacklistedScenes.Contains(sceneName))
			{
				var random = new System.Random();
				activeModifiers = new Modifier[numActiveModifiers];
				for(int i = 0; i < numActiveModifiers; i++)
				{
					activeModifiers[i] = modifiers[random.Next(0, modifiers.Length)];
				}
				modifierControl.ActivateModifiers(activeModifiers);
			}
			return sceneName;
		}

		private string LanguageGetHook(string key, string sheetTitle)
		{
			foreach(Modifier modifier in modifiers)
			{
				if(key == modifier.ToString()) return modifier.ToString().Substring(14);
			}
			if(key == "ChallengeMode_AchievementText") return "";
			return Language.Language.GetInternal(key, sheetTitle);
		}

		private void BeforePlayerDeadHook()
		{
			numActiveModifiers = 1;
		}

		public void Unload()
		{
			ModHooks.Instance.BeforeSceneLoadHook -= BeforeSceneLoad;
			ModHooks.Instance.LanguageGetHook -= LanguageGetHook;
			ModHooks.Instance.BeforePlayerDeadHook -= BeforePlayerDeadHook;
		}
	}
}
