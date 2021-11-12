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
				HeroController.instance.TakeMP(11);
				yield return new WaitForSecondsRealtime(1f);
				if(PlayerData.instance.GetInt("MPCharge") == 0)
				{
					HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 1);
					yield return new WaitForSecondsRealtime(4f);
				}
			}
			yield break;
		}

		public override void StopEffect()
		{
			flag = false;

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Hungry Knight";
		}
	}
}
