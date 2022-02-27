using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, ITogglableMod
	{
		private Modifier[] modifiers;
		private int numActiveModifiers;
		private ModifierControl modifierControl;

		private int spaCount;
		private HashSet<string> blacklistedScenes;
		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		public ChallengeMode() : base("ChallengeMode") { }

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Instance = this;
			this.preloadedObjects = preloadedObjects;
			spaCount = 0;
			blacklistedScenes = new HashSet<string>()
			{
				"GG_Atrium", "GG_Atrium_Roof", "GG_Unlock_Wastes", "GG_Blue_Room", "GG_Workshop", "GG_Land_Of_Storms", 
				"GG_Engine", "GG_Engine_Prime", "GG_Unn", "GG_Engine_Root", "GG_Wyrm", "GG_Spa"
			};

			//All modifiers
			//modifiers = new Modifier[12];
			//modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.HighStress>();
			//modifiers[1] = GameManager.instance.gameObject.AddComponent<Modifiers.FrailShell>();
			//modifiers[2] = GameManager.instance.gameObject.AddComponent<Modifiers.AdrenalineRush>();
			//modifiers[3] = GameManager.instance.gameObject.AddComponent<Modifiers.AspidRancher>();
			//modifiers[4] = GameManager.instance.gameObject.AddComponent<Modifiers.VoidVision>();
			//modifiers[5] = GameManager.instance.gameObject.AddComponent<Modifiers.SpeedrunnersCurse>();
			//modifiers[6] = GameManager.instance.gameObject.AddComponent<Modifiers.NailOnly>();
			//modifiers[7] = GameManager.instance.gameObject.AddComponent<Modifiers.SoulMaster>();
			//modifiers[8] = GameManager.instance.gameObject.AddComponent<Modifiers.HungryKnight>();
			//modifiers[9] = GameManager.instance.gameObject.AddComponent<Modifiers.UnfriendlyFire>();
			//modifiers[10] = GameManager.instance.gameObject.AddComponent<Modifiers.Ascension>();
			//modifiers[11] = GameManager.instance.gameObject.AddComponent<Modifiers.SleepyKnight>();

			//Test individual modifier
			modifiers = new Modifier[1];
			modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.Ascension>();

			numActiveModifiers = 1;
			modifierControl = GameManager.instance.gameObject.AddComponent<ModifierControl>();

			//Create achievements
			AchievementHelper.Initialize();
			foreach(Modifier modifier in modifiers)
			{
				AchievementHelper.AddAchievement(modifier.ToString(), new Sprite(), modifier.ToString(), "ChallengeMode_AchievementText", false);
			}
			UIManager.instance.RefreshAchievementsList();

			ModHooks.Instance.BeforeSceneLoadHook += BeforeSceneLoad;
			ModHooks.Instance.LanguageGetHook += LanguageGetHook;
		}

		public override List<(string, string)> GetPreloadNames()
		{
			return new List<(string, string)>
			{
				("Deepnest_East_04","Super Spitter"),
				("GG_Ghost_Gorb", "Warrior/Ghost Warrior Slug")
			};
		}

		private string BeforeSceneLoad(string sceneName)
		{
			modifierControl.Unload();

			if(sceneName.Substring(0, 2) == "GG" && !blacklistedScenes.Contains(sceneName))
			{
				modifierControl.Initialize(modifiers, numActiveModifiers);
			}

			if(sceneName == "GG_Spa") spaCount++;
			if(sceneName == "GG_Atrium" || sceneName == "GG_Atrium_Roof" || sceneName == "GG_Workshop") spaCount = 0;
			//P1 section in P5
			if(spaCount == 0) numActiveModifiers = 1;
			//P2 and P3 sections in P5
			if(spaCount == 1) numActiveModifiers = 2;
			//P4 section in P5
			if(spaCount == 5) numActiveModifiers = 3;

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

		public void Unload()
		{
			modifierControl.Unload();

			ModHooks.Instance.BeforeSceneLoadHook -= BeforeSceneLoad;
			ModHooks.Instance.LanguageGetHook -= LanguageGetHook;
		}
	}
}
