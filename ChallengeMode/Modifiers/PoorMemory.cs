namespace ChallengeMode.Modifiers
{
	class PoorMemory : Modifier
	{
		public override void StartEffect()
		{
			GameCameras.instance.hudCanvas.gameObject.SetActive(false);
		}

		public override void StopEffect()
		{
			GameCameras.instance.hudCanvas.gameObject.SetActive(true);
		}

		public override string ToString()
		{
			return "ChallengeMode_Poor Memory";
		}
	}
}
