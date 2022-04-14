using System.Collections.Generic;

namespace ChallengeMode.Modifiers
{
	class SalubrasCurse : Modifier
	{
		private List<int> charms;

		public override void StartEffect()
		{
			//Store current charms and unequip them
			charms = new List<int>();
			for(int i = 1; i < 41; i++)
			{
				if(PlayerData.instance.GetBool("equippedCharm_" + i))
				{
					charms.Add(i);
				}
				PlayerData.instance.SetBool("equippedCharm_" + i, false);
				GameManager.instance.UnequipCharm(i);
			}
			GameManager.instance.RefreshOvercharm();
		}

		public override void StopEffect()
		{
			foreach(int i in charms)
			{
				PlayerData.instance.SetBool("equippedCharm_" + i, true);
				GameManager.instance.EquipCharm(i);
			}
			GameManager.instance.RefreshOvercharm();
		}

		public override string ToString()
		{
			return "ChallengeMode_Salubra's Curse";
		}
	}
}
