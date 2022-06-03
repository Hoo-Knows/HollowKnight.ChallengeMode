using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;
using Satchel;
using Satchel.BetterMenus;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
	{
		public static List<Modifier> modifiers;
		public static List<Modifier> modifiersU;
		public static List<string> scenesU;
		public static ModifierControl modifierControl;
		private static GameObject modifierObject;

		private int spaCount;
		private int numModifiers;

		public bool ToggleButtonInsideMenu => false;
		private Menu MenuRef;
		private static List<string> modifierNames;

		public static GlobalSettings Settings { get; set; } = new GlobalSettings();
		public void OnLoadGlobal(GlobalSettings s) => Settings = s;
		public GlobalSettings OnSaveGlobal() => Settings;

		public static Dictionary<string, Dictionary<string, GameObject>> Preloads;
		public static ChallengeMode Instance;

		public override string GetVersion() => "0.6.0.0";

		public ChallengeMode() : base("ChallengeMode")
		{
			Instance = this;
		}

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Log("Initializing");

			Preloads = preloadedObjects;

			//Modifier Object, all Monobehaviours are attached to this
			modifierObject = new GameObject("Modifier Object");
			Object.DontDestroyOnLoad(modifierObject);

			modifiers = new List<Modifier>();
			modifiersU = new List<Modifier>();
			scenesU = new List<string>();
			modifierControl = modifierObject.AddComponent<ModifierControl>();
			modifierNames = new List<string>();

			//Modifiers
			AddModifier<Modifiers.HighStress>();
			AddModifier<Modifiers.FrailShell>();
			AddModifier<Modifiers.AdrenalineRush>();
			AddModifier<Modifiers.AspidRancher>();
			AddModifier<Modifiers.VoidVision>();
			AddModifier<Modifiers.SpeedrunnersCurse>();
			AddModifier<Modifiers.NailOnly>();
			AddModifier<Modifiers.SoulMaster>();
			AddModifier<Modifiers.HungryKnight>();
			AddModifier<Modifiers.UnfriendlyFire>();
			AddModifier<Modifiers.Ascension>();
			AddModifier<Modifiers.SalubrasCurse>();
			AddModifier<Modifiers.PastRegrets>();
			AddModifier<Modifiers.InfectedWounds>();
			AddModifier<Modifiers.ChaosChaos>();
			AddModifier<Modifiers.TemporalDistortion>();
			AddModifier<Modifiers.PoorMemory>();
			AddModifier<Modifiers.FoolsErrand>();

			//Unique modifiers
			AddModifierU<Modifiers.NailmasterU>("GG_Nailmasters");
			AddModifierU<Modifiers.NailmasterU>("GG_Painter");
			AddModifierU<Modifiers.NailmasterU>("GG_Sly");
			AddModifierU<Modifiers.EphemeralOrdealU>("GG_Grey_Prince_Zote");
			AddModifierU<Modifiers.SomethingWickedU>("GG_Grimm_Nightmare");
			AddModifierU<Modifiers.PaleWatchU>("GG_Hollow_Knight");
			AddModifierU<Modifiers.ForgottenLightU>("GG_Radiance");

			spaCount = 0;

			ModHooks.BeforeSceneLoadHook += BeforeSceneLoadHook;
			ModHooks.LanguageGetHook += LanguageGetHook;

			Log("Initialized");
		}

		private string BeforeSceneLoadHook(string sceneName)
		{
			//Used to override scene for testing
			//if(sceneName == "GG_Mage_Knight") sceneName = "GG_Radiance";

			if(sceneName == "GG_Spa") spaCount++;
			if(sceneName == "GG_Atrium" || sceneName == "GG_Atrium_Roof" || sceneName == "GG_Workshop") spaCount = 0;
			//P1 section in P5
			if(spaCount >= 0) numModifiers = Settings.numModifiers;
			//P2 and P3 sections in P5
			if(spaCount >= 1) numModifiers += Settings.incModifiers;
			//P4 section in P5
			if(spaCount >= 5) numModifiers += Settings.incModifiers;

			if(modifierControl == null) modifierControl = modifierObject.AddComponent<ModifierControl>();
			modifierControl.Initialize(numModifiers, sceneName);

			return sceneName;
		}

		private string LanguageGetHook(string key, string sheetTitle, string orig)
		{
			foreach(Modifier modifier in modifiers)
			{
				if(key == modifier.ToString()) return modifier.ToString().Split(new char[] { '_' })[1];
			}
			foreach(Modifier modifier in modifiersU)
			{
				if(key == modifier.ToString()) return modifier.ToString().Split(new char[] { '_' })[1];
			}
			if(key == "ChallengeMode_AchievementText") return "";
			return orig;
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
				("GG_Mighty_Zote", "Battle Control"), //Eternal Ordeal
				("Hive_03", "Flamebearer Spawn"), //Grimmkin Nightmare
				("White_Palace_02", "Battle Scene/Royal Gaurd") //Kingsmould
			};
		}

		public static void AddModifier<T>() where T : Modifier
		{
			modifiers.Add(modifierObject.AddComponent<T>());
			modifierNames.Add(modifiers[modifiers.Count - 1].ToString().Split(new char[] { '_' })[1]);
			CreateAchievement(modifiers[modifiers.Count - 1]);
		}

		public static void AddModifierU<T>(string sceneName) where T : Modifier
		{
			modifiersU.Add(modifierObject.AddComponent<T>());
			scenesU.Add(sceneName);
			CreateAchievement(modifiersU[modifiersU.Count - 1]);
		}

		public static void CreateAchievement(Modifier modifier)
		{
			Texture2D texture = TextureUtils.createTextureOfColor(64, 64, Color.clear);

			//Add achievement
			AchievementHelper.AddAchievement(modifier.ToString(),
				Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64f),
				modifier.ToString(), "ChallengeMode_AchievementText", false);

			UIManager.instance.RefreshAchievementsList();
		}

		public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
		{
			if(MenuRef == null)
			{
				MenuRef = new Menu("Challenge Mode",
					new Element[]
					{
						new CustomSlider("Number of modifiers",
						(f) => Settings.numModifiers = (int)f,
						() => Settings.numModifiers, Id: "NumSlider")
						{
							wholeNumbers = true,
							minValue = 1,
							maxValue = 5
						},

						new CustomSlider("Increment modifiers",
						(f) => Settings.incModifiers = (int)f,
						() => Settings.incModifiers, Id: "IncSlider")
						{
							wholeNumbers = true,
							minValue = 0,
							maxValue = 2
						},

						new HorizontalOption("Guarantee modifier",
						"Guarantee that a modifier will appear",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0)
							{
								Settings.modifierOption = true;
								MenuRef.Find("ModifierValue").Show();
								Settings.modifierValue = 0;
							}
							if(i == 1)
							{
								Settings.modifierOption = false;
								MenuRef.Find("ModifierValue").Hide();
							}
						},
						() => Settings.modifierOption ? 0 : 1, Id: "ModifierOption"),

						new HorizontalOption("", "",
						modifierNames.ToArray(),
						(i) => Settings.modifierValue = i,
						() => Settings.modifierValue, Id: "ModifierValue")
						{
							isVisible = Settings.modifierOption,
						},

						new HorizontalOption("Use logic",
						"Prevent easy or unfair modifier combinations",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0) Settings.logicOption = true;
							if(i == 1) Settings.logicOption = false;
						},
						() => Settings.logicOption ? 0 : 1, Id: "LogicOption"),

						new HorizontalOption("Use slowdown",
						"Slow down the game when displaying modifiers",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0) Settings.slowdownOption = true;
							if(i == 1) Settings.slowdownOption = false;
						},
						() => Settings.slowdownOption ? 0 : 1, Id: "SlowdownOption"),

						new HorizontalOption("Allow High Stress",
						"Turn off if you don't want to go from max health to dead in 5 seconds",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0) Settings.highStressOption = true;
							if(i == 1) Settings.highStressOption = false;
						},
						() => Settings.highStressOption ? 0 : 1, Id: "HighStressOption"),

						new HorizontalOption("Allow non-unique modifiers",
						"Allow regular modifiers on bosses that have unique modifiers",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0) Settings.uniqueOption = true;
							if(i == 1) Settings.uniqueOption = false;
						},
						() => Settings.uniqueOption ? 0 : 1, Id: "UniqueOption"),

						new HorizontalOption("Allow modifiers everywhere",
						"Allow modifiers in any room, even outside Godhome",
						new string[] { "True", "False" },
						(i) =>
						{
							if(i == 0) Settings.everywhereOption = true;
							if(i == 1) Settings.everywhereOption = false;
						},
						() => Settings.everywhereOption ? 0 : 1, Id: "EverywhereOption"),

						new MenuButton("Reset settings", "",
						(_) =>
						{
							Settings.numModifiers = 1;
							Settings.incModifiers = 1;
							Settings.modifierOption = false;
							Settings.modifierValue = 0;
							Settings.logicOption = true;
							Settings.slowdownOption = true;
							Settings.highStressOption = true;
							Settings.uniqueOption = false;
							Settings.everywhereOption = false;
							string[] IDs = new string[]
							{
								"NumSlider",
								"IncSlider",
								"ModifierOption",
								"ModifierValue",
								"LogicOption",
								"SlowdownOption",
								"HighStressOption",
								"UniqueOption",
								"EverywhereOption"
							};
							foreach(string ID in IDs)
							{
								MenuRef.Find(ID).Update();
							}
							MenuRef.Find("ModifierValue").Hide();
						}, Id: "ResetButton")
					}
				);
			}
			return MenuRef.GetMenuScreen(modListMenu);
		}

		public void Unload()
		{
			if(modifierControl != null) modifierControl.Unload();

			ModHooks.BeforeSceneLoadHook -= BeforeSceneLoadHook;
			ModHooks.LanguageGetHook -= LanguageGetHook;
		}
	}
}
