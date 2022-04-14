using SFCore.Utils;

namespace ChallengeMode.Modifiers
{
	class NailOnly : Modifier
	{
		private PlayMakerFSM spellFSM;

		public override void StartEffect()
		{
			spellFSM = HeroController.instance.spellControl;

			spellFSM.InsertMethod("Can Cast?", () =>
			{
				spellFSM.SendEvent("CANCEL");
			}, 0);

			spellFSM.InsertMethod("Can Cast? QC", () =>
			{
				spellFSM.SendEvent("CANCEL");
			}, 0);
		}

		public override void StopEffect()
		{
			spellFSM.RemoveAction("Can Cast?", 0);
			spellFSM.RemoveAction("Can Cast? QC", 0);
		}

		public override string ToString()
		{
			return "ChallengeMode_Nail Only";
		}
	}
}
