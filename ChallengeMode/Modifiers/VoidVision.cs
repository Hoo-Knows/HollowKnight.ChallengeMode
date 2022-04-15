namespace ChallengeMode.Modifiers
{
	class VoidVision : Modifier
	{
		public override void StartEffect()
		{
			HeroController.instance.vignetteFSM.SetState("Lantern");
		}

		public override void StopEffect()
		{
			HeroController.instance.vignetteFSM.SetState("Normal");
		}

		public override string ToString()
		{
			return "ChallengeMode_Void Vision";
		}
	}
}
