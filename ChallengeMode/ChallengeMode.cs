using System;
using Modding;

namespace ChallengeMode
{
	public class ChallengeMode : Mod, ITogglableMod
	{
		Modifier[] modifiers = new Modifier[1];
		Modifier currentModifier;
		public ChallengeMode() : base("Challenge Mode") { }

		public override void Initialize()
		{
			Unload();

			modifiers[0] = GameManager.instance.gameObject.AddComponent<Modifiers.HighStress>();

			ModHooks.Instance.BeforeSceneLoadHook += BeforeSceneLoad;
		}

		private string BeforeSceneLoad(string sceneName)
		{
			if(currentModifier != null) currentModifier.StopEffect();
			if(sceneName == "GG_Gruz_Mother" || sceneName == "GG_Gruz_Mother_V")
			{
				var random = new Random();
				currentModifier = modifiers[random.Next(0, modifiers.Length - 1)];
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
