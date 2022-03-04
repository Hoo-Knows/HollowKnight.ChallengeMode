using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class SalubrasCurse : Modifier
	{
		private bool[] originalCharms;
		private Random random;
		private bool[] charms;
		
		public override void StartEffect()
		{
			//Store current charms
			originalCharms = new bool[40];
			for(int i = 1; i < 41; i++)
			{
				originalCharms[i - 1] = PlayerData.instance.GetBool("equippedCharm_" + i);
			}

			//Set random charms
			random = new Random();
			charms = new bool[40];
			for(int i = 0; i < 4; i++)
			{
				int id = random.Next(1, 41);
				if(!charms[id - 1]) charms[id - 1] = true;
				else i--;
			}

			SetCharms(charms);
		}

		private void SetCharms(bool[] charms)
		{
			for(int i = 1; i < 41; i++)
			{
				//Unequip charm by default
				if(PlayerData.instance.GetBool("equippedCharm_" + i))
				{
					PlayerData.instance.SetBool("equippedCharm_" + i, false);
					GameManager.instance.UnequipCharm(i);
				}
				//Check if charm should be equipped
				PlayerData.instance.SetBool("equippedCharm_" + i, charms[i - 1]);
				if(charms[i - 1]) GameManager.instance.EquipCharm(i);
			}
			HeroController.instance.CharmUpdate();
			GameManager.instance.RefreshOvercharm();
			PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");
		}

		public override void StopEffect()
		{
			SetCharms(originalCharms);
		}

		public override string ToString()
		{
			return "ChallengeMode_Salubra's Curse";
		}
	}
}
