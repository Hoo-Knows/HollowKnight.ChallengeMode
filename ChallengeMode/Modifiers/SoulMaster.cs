using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class SoulMaster : Modifier
	{
		private bool flag;

		public override void StartEffect()
		{
			flag = true;

			On.HealthManager.Hit += HealthManagerHit;

			StartCoroutine(HandleSoul());
		}

		private void HealthManagerHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
		{
			if(hitInstance.AttackType == AttackTypes.Nail || hitInstance.AttackType == AttackTypes.NailBeam)
			{
				hitInstance.DamageDealt = 1;
			}
			orig(self, hitInstance);
		}

		private IEnumerator HandleSoul()
		{
			while(flag)
			{
				HeroController.instance.AddMPChargeSpa(11);
				yield return new WaitForSeconds(3f);
			}
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			On.HealthManager.Hit -= HealthManagerHit;

			flag = false;
		}

		public override string ToString()
		{
			return "ChallengeMode_Soul Master";
		}

		public override List<string> GetCodeBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Soul Master",
				"ChallengeMode_Nail Only",
				"ChallengeMode_Nailmaster"
			};
		}
	}
}
