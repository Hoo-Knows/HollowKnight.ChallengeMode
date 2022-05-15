using System.Collections.Generic;
using Modding;
using SFCore;
using UnityEngine;
using Satchel;
using System.IO;

namespace ChallengeMode
{
	public class ChallengeMode : Mod
	{
		public Modifier[] modifiers;
		public Modifier[] modifiersU;
		public ModifierControl modifierControl;
		private GameObject modifierObject;

		private int numActiveModifiers;
		private int spaCount;
		private string iconPath;

		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		public override string GetVersion() => "0.4.2.1";

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
			modifiers = new Modifier[]
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
			modifiersU = new Modifier[]
			{
				modifierObject.AddComponent<Modifiers.NailmasterU>(),
				modifierObject.AddComponent<Modifiers.EphemeralOrdealU>(),
				modifierObject.AddComponent<Modifiers.SomethingWickedU>(),
				modifierObject.AddComponent<Modifiers.PaleWatchU>(),
				modifierObject.AddComponent<Modifiers.ForgottenLightU>()
			};

			//Test individual modifier
			//modifiers = new Modifier[]
			//{
			//	modifierObject.AddComponent<Modifiers.SalubrasCurse>()
			//};

			modifierControl = modifierObject.AddComponent<ModifierControl>();
			spaCount = 0;
			numActiveModifiers = 1;

			//Create achievements
			iconPath = Path.Combine(AssemblyUtils.getCurrentDirectory(), "Icons");
			IoUtils.EnsureDirectory(iconPath);
			for(int i = 0; i < modifiers.Length + modifiersU.Length; i++)
			{
				//Get modifier
				Modifier modifier;
				if(i < modifiers.Length) modifier = modifiers[i];
				else modifier = modifiersU[i - modifiers.Length];

				//Load texture
				Texture2D texture = TextureUtils.createTextureOfColor(64, 64, Color.clear);
				string path = Path.Combine(iconPath, "ChallengeMode_PlaceholderIcon.png");
				if(File.Exists(path))
				{
					texture = TextureUtils.LoadTextureFromFile(path);
				}

				//Add achievement
				AchievementHelper.AddAchievement(modifier.ToString(),
					Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64f),
					modifier.ToString(), "ChallengeMode_AchievementText", false);
			}
			UIManager.instance.RefreshAchievementsList();

			ModHooks.LanguageGetHook += LanguageGetHook;
			ModHooks.BeforeSceneLoadHook += BeforeSceneLoadHook;

			Log("Initialized");
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

		private string BeforeSceneLoadHook(string sceneName)
		{
			//Used to override scene
			//if(sceneName == "GG_Mage_Knight") sceneName = "GG_Radiance";

			if(modifierControl == null) modifierControl = modifierObject.AddComponent<ModifierControl>();
			modifierControl.Initialize(numActiveModifiers, sceneName);

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
				if(key == modifier.ToString()) return modifier.ToString().Split(new char[] { '_' })[1];
			}
			foreach(Modifier modifier in modifiersU)
			{
				if(key == modifier.ToString()) return modifier.ToString().Split(new char[] { '_' })[1];
			}
			if(key == "ChallengeMode_AchievementText") return "";
			return orig;
		}

		public void Unload()
		{
			if(modifierControl != null) modifierControl.Unload();

			ModHooks.LanguageGetHook -= LanguageGetHook;
			ModHooks.BeforeSceneLoadHook -= BeforeSceneLoadHook;
		}
	}
}
