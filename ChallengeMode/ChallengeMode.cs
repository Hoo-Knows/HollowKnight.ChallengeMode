﻿using System.Collections.Generic;
using Modding;
using UnityEngine;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, ITogglableMod
	{
		private Modifier[] modifiers;
		private Modifier currentModifier;
		public Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
		public static ChallengeMode Instance;

		public ChallengeMode() : base("ChallengeMode") { }

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			Instance = this;
			this.preloadedObjects = preloadedObjects;

			Unload();

			//modifiers = new Modifier[5];
			//modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.HighStress>();
			//modifiers[1] = GameManager.instance.gameObject.AddComponent<Modifiers.FrailShell>();
			//modifiers[2] = GameManager.instance.gameObject.AddComponent<Modifiers.AdrenalineRush>();
			//modifiers[3] = GameManager.instance.gameObject.AddComponent<Modifiers.AspidRancher>();
			//modifiers[4] = GameManager.instance.gameObject.AddComponent<Modifiers.VoidVision>();

			//Test individual modifier
			modifiers = new Modifier[1];
			modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.VoidVision>();

			ModHooks.Instance.BeforeSceneLoadHook += BeforeSceneLoad;
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
			if(currentModifier != null) currentModifier.StopEffect();
			if(sceneName == "GG_Gruz_Mother" || sceneName == "GG_Gruz_Mother_V")
			{
				var random = new System.Random();
				currentModifier = modifiers[random.Next(0, modifiers.Length)];
				Log(currentModifier.ToString());
				currentModifier.StartEffect();
			}
			return sceneName;
		}

		public void Unload()
		{
			ModHooks.Instance.BeforeSceneLoadHook -= BeforeSceneLoad;
		}
	}
}
