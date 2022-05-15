using SFCore.Utils;

namespace ChallengeMode.Modifiers
{
	class VoidVision : Modifier
	{
		string[] states;
		public override void StartEffect()
		{
			states = new string[] { "Scene Reset", "Scene Reset 2", "Dark Lev Check" };
			foreach(string state in states)
			{
				HeroController.instance.vignetteFSM.InsertMethod(state, () =>
				{
					HeroController.instance.vignetteFSM.SetState("Lantern");
				}, 0);
			}
			HeroController.instance.vignetteFSM.SetState("Lantern");
		}

		public override void StopEffect()
		{
			foreach(string state in states)
			{
				HeroController.instance.vignetteFSM.RemoveAction(state, 0);
			}
			HeroController.instance.vignetteFSM.SetState("Normal");
		}

		public override string ToString()
		{
			return "ChallengeMode_Void Vision";
		}
	}
}
