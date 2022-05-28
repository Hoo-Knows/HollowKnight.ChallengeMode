using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;
using Satchel;
using System.IO;
using Satchel.BetterMenus;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
	{
		public List<Modifier> modifiers;
		public List<Modifier> modifiersU;
		public ModifierControl modifierControl;
		private GameObject modifierObject;

		private int spaCount;
		private int numModifiers;

		public bool ToggleButtonInsideMenu => false;
		private Menu MenuRef;
		private string[] modifierNames;

		public static GlobalSettings Settings { get; set; } = new GlobalSettings();
		public void OnLoadGlobal(GlobalSettings s) => Settings = s;
		public GlobalSettings OnSaveGlobal() => Settings;

		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		public override string GetVersion() => "0.5.0.4";

		public ChallengeMode() : base("ChallengeMode") { }

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Log("Initializing");

			Instance = this;
			this.preloadedObjects = preloadedObjects;

			//Modifier Object, all Monobehaviours are attached to this
			modifierObject = new GameObject("Modifier Object");
			Object.DontDestroyOnLoad(modifierObject);

			//Modifiers
			modifiers = new List<Modifier>()
			{
				modifierObject.AddComponent<Modifiers.HighStress>(),
				modifierObject.AddComponent<Modifiers.FrailShell>(),
				modifierObject.AddComponent<Modifiers.AdrenalineRush>(),
				modifierObject.AddComponent<Modifiers.AspidRancher>(),
				modifierObject.AddComponent<Modifiers.VoidVision>(),
				modifierObject.AddComponent<Modifiers.SpeedrunnersCurse>(),
				modifierObject.AddComponent<Modifiers.NailOnly>(),
				modifierObject.AddComponent<Modifiers.SoulMaster>(),
				modifierObject.AddComponent<Modifiers.HungryKnight>(),
				modifierObject.AddComponent<Modifiers.UnfriendlyFire>(),
				modifierObject.AddComponent<Modifiers.Ascension>(),
				modifierObject.AddComponent<Modifiers.SalubrasCurse>(),
				modifierObject.AddComponent<Modifiers.PastRegrets>(),
				modifierObject.AddComponent<Modifiers.InfectedWounds>(),
				modifierObject.AddComponent<Modifiers.ChaosChaos>(),
				modifierObject.AddComponent<Modifiers.TemporalDistortion>(),
				modifierObject.AddComponent<Modifiers.PoorMemory>(),
				modifierObject.AddComponent<Modifiers.FoolsErrand>()
			};

			//Unique modifiers
			modifiersU = new List<Modifier>()
			{
				modifierObject.AddComponent<Modifiers.NailmasterU>(),
				modifierObject.AddComponent<Modifiers.EphemeralOrdealU>(),
				modifierObject.AddComponent<Modifiers.SomethingWickedU>(),
				modifierObject.AddComponent<Modifiers.PaleWatchU>(),
				modifierObject.AddComponent<Modifiers.ForgottenLightU>()
			};

			modifierControl = modifierObject.AddComponent<ModifierControl>();
			spaCount = 0;

			//Set names for menu
			modifierNames = new string[modifiers.Count];
			for(int i = 0; i < modifiers.Count; i++)
			{
				modifierNames[i] = modifiers[i].ToString().Split(new char[] { '_' })[1];
			}

			//Create achievements
			for(int i = 0; i < modifiers.Count + modifiersU.Count; i++)
			{
				//Get modifier
				Modifier modifier;
				if(i < modifiers.Count) modifier = modifiers[i];
				else modifier = modifiersU[i - modifiers.Count];

				CreateAchievement(modifier);
			}

			ModHooks.BeforeSceneLoadHook += BeforeSceneLoadHook;
			ModHooks.LanguageGetHook += LanguageGetHook;

			Log("Initialized");
		}

		private string BeforeSceneLoadHook(string sceneName)
		{
			//Used to override scene
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

		public void CreateAchievement(Modifier modifier)
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
							maxValue = 3
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
						modifierNames,
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

						new MenuButton("Reset settings", "",
						(_) =>
						{
							Settings.numModifiers = 1;
							Settings.incModifiers = 1;
							Settings.modifierOption = false;
							Settings.modifierValue = 0;
							Settings.logicOption = true;
							string[] Ids = new string[] { "NumSlider", "IncSlider", "ModifierOption", "ModifierValue", "LogicOption" };
							foreach(string Id in Ids)
							{
								MenuRef.Find(Id).Update();
							}
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
