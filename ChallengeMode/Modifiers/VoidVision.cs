namespace ChallengeMode.Modifiers
{
	class VoidVision : Modifier
	{
		public override void StartEffect()
		{
			On.SceneManager.Update += SceneManagerUpdate;
		}

		private void SceneManagerUpdate(On.SceneManager.orig_Update orig, SceneManager self)
		{
			HeroController.instance.vignetteFSM.SetState("Dark 1");
			orig(self);
		}

		public override void StopEffect()
		{
			On.SceneManager.Update -= SceneManagerUpdate;

			HeroController.instance.vignetteFSM.SetState("Normal");
		}

		public override string ToString()
		{
			return "ChallengeMode_Void Vision";
		}
	}
}
