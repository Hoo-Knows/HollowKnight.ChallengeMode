using System.Collections;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class HungryKnight : Modifier
	{
		private bool flag;

		public override void StartEffect()
		{
			flag = true;

			HeroController.instance.AddMPChargeSpa(99);
			StartCoroutine(HandleSoul());
		}

		private IEnumerator HandleSoul()
		{
			while(flag)
			{
				if(!HeroController.instance.controlReqlinquished)
				{
					if(BossSequenceController.BoundSoul)
					{
						HeroController.instance.TakeMP(5);
					}
					else
					{
						HeroController.instance.TakeMP(11);
					}
				}
				yield return new WaitForSeconds(4f);
				if(PlayerData.instance.GetInt("MPCharge") == 0)
				{
					if(!HeroController.instance.controlReqlinquished)
					{
						HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 1);
					}
					yield return new WaitForSeconds(6f);
				}
			}
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			flag = false;
		}

		public override string ToString()
		{
			return "ChallengeMode_Hungry Knight";
		}
	}
}
