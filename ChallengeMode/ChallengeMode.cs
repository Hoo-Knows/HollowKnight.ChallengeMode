﻿using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, ITogglableMod
	{
		public Modifier[] modifiers;
		public ModifierControl modifierControl;

		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		private int spaCount;
		private int numActiveModifiers;

		public override string GetVersion() => "0.3.1.4";

		public ChallengeMode() : base("ChallengeMode") { }

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Instance = this;
			this.preloadedObjects = preloadedObjects;

			//All modifiers
			modifiers = new Modifier[18];
			modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.HighStress>();
			modifiers[1] = GameManager.instance.gameObject.AddComponent<Modifiers.FrailShell>();
			modifiers[2] = GameManager.instance.gameObject.AddComponent<Modifiers.AdrenalineRush>();
			modifiers[3] = GameManager.instance.gameObject.AddComponent<Modifiers.AspidRancher>();
			modifiers[4] = GameManager.instance.gameObject.AddComponent<Modifiers.VoidVision>();
			modifiers[5] = GameManager.instance.gameObject.AddComponent<Modifiers.SpeedrunnersCurse>();
			modifiers[6] = GameManager.instance.gameObject.AddComponent<Modifiers.NailOnly>();
			modifiers[7] = GameManager.instance.gameObject.AddComponent<Modifiers.SoulMaster>();
			modifiers[8] = GameManager.instance.gameObject.AddComponent<Modifiers.HungryKnight>();
			modifiers[9] = GameManager.instance.gameObject.AddComponent<Modifiers.UnfriendlyFire>();
			modifiers[10] = GameManager.instance.gameObject.AddComponent<Modifiers.Ascension>();
			modifiers[11] = GameManager.instance.gameObject.AddComponent<Modifiers.SalubrasCurse>();
			modifiers[12] = GameManager.instance.gameObject.AddComponent<Modifiers.PastRegrets>();
			modifiers[13] = GameManager.instance.gameObject.AddComponent<Modifiers.InfectedWounds>();
			modifiers[14] = GameManager.instance.gameObject.AddComponent<Modifiers.ChaosChaos>();
			modifiers[15] = GameManager.instance.gameObject.AddComponent<Modifiers.TemporalDistortion>();
			modifiers[16] = GameManager.instance.gameObject.AddComponent<Modifiers.PoorMemory>();
			modifiers[17] = GameManager.instance.gameObject.AddComponent<Modifiers.AFoolsErrand>();

			//Test individual modifier
			//modifiers = new Modifier[1];
			//modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.AFoolsErrand>();

			modifierControl = GameManager.instance.gameObject.AddComponent<ModifierControl>();
			spaCount = 0;
			numActiveModifiers = 1;

			//Create achievements
			foreach(Modifier modifier in modifiers)
			{
				AchievementHelper.AddAchievement(modifier.ToString(),
					Sprite.Create(new Texture2D(80, 80), new Rect(0f, 0f, 80f, 80f), new Vector2(0f, 0f)),
					modifier.ToString(), "ChallengeMode_AchievementText", false);
			}
			UIManager.instance.RefreshAchievementsList();

			ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
			ModHooks.LanguageGetHook += LanguageGetHook;
		}

		public override List<(string, string)> GetPreloadNames()
		{
			return new List<(string, string)>
			{
				("Deepnest_East_04","Super Spitter"), //Primal Aspid
				("GG_Ghost_Gorb", "Warrior/Ghost Warrior Slug"), //Gorb
				("Abyss_19", "Parasite Balloon (1)"), //Infected Balloon
				("Room_Colosseum_Bronze", "Colosseum Manager/Ground Spikes"), //Colosseum Spikes (for audio)
				("Room_Colosseum_Bronze", "Colosseum Manager/Ground Spikes/Colosseum Spike (19)"), //Colosseum Spike
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 2/Colosseum Cage Small"), //Armored Squit Cage
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 37/Colosseum Cage Small (3)"), //Battle Obble Cage
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 3/Colosseum Cage Large"), //Shielded Fool Cage
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 50/Colosseum Cage Large"), //Sturdy Fool Cage
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 1/Colosseum Cage Large"), //Heavy Fool Cage
				("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 6/Colosseum Cage Large"), //Winged Fool Cage
			};
		}

		private string BeforeSceneLoad(string sceneName)
		{
			if(modifierControl == null) modifierControl = GameManager.instance.gameObject.AddComponent<ModifierControl>();
			modifierControl.Initialize(sceneName, numActiveModifiers);

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

		private string LanguageGetHook(string key, string sheetTitle, string orig)
		{
			foreach(Modifier modifier in modifiers)
			{
				if(key == modifier.ToString()) return modifier.ToString().Substring(14);
			}
			if(key == "ChallengeMode_AchievementText") return "";
			return orig;
		}

		public void Unload()
		{
			modifierControl.Unload();

			ModHooks.BeforeSceneLoadHook -= BeforeSceneLoad;
			ModHooks.LanguageGetHook -= LanguageGetHook;
		}
	}
}
