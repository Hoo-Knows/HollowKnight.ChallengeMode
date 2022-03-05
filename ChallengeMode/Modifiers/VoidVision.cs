namespace ChallengeMode.Modifiers
{
	class VoidVision : Modifier
	{
		public override void StartEffect()
		{
			HeroController.instance.vignetteFSM.SetState("Dark 1");
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
