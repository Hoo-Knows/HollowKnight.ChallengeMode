using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class SoulMaster : Modifier
	{
		private bool flag;
		private int nailDamage;

		public override void StartEffect()
		{
			flag = true;
			nailDamage = PlayerData.instance.nailDamage;
			PlayerData.instance.nailDamage = 1;
			PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");

			StartCoroutine(HandleSoul());
		}

		private IEnumerator HandleSoul()
		{
			while(flag)
			{
				HeroController.instance.AddMPChargeSpa(11);
				yield return new WaitForSeconds(2f);
			}
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			flag = false;
			PlayerData.instance.SetInt("nailDamage", nailDamage);
			PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
		}

		public override string ToString()
		{
			return "ChallengeMode_Soul Master";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Soul Master", "ChallengeMode_Nail Only"
			};
		}
	}
}
