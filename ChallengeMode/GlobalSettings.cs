using Modding;

namespace ChallengeMode
{
	public class GlobalSettings : ModHooksGlobalSettings
	{
		public int numModifiers { get; set; } = 1;
		public int incModifiers { get; set; } = 1;
		public bool modifierOption { get; set; } = false;
		public int modifierValue { get; set; } = 0;
		public bool logicOption { get; set; } = true;
		public bool slowdownOption { get; set; } = true;
		public bool highStressOption { get; set; } = true;
		public bool uniqueOption { get; set; } = false;
		public bool everywhereOption { get; set; } = false;
	}
}
