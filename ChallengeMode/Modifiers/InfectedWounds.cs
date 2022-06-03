using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class InfectedWounds : Modifier
	{
		private GameObject balloonGO;

		public override void StartEffect()
		{
			balloonGO = ChallengeMode.Preloads["Abyss_19"]["Parasite Balloon (1)"];
			Destroy(balloonGO.GetComponent<PersistentBoolItem>());
			HealthManager hm = balloonGO.GetComponent<HealthManager>();
			hm.hp = 1;
			//Disable soul gain
			ReflectionHelper.SetField(hm, "enemyType", 6);

			ModHooks.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			StopAllCoroutines();

			On.HealthManager.Hit -= HealthManagerHit;
			StartCoroutine(NailControl());
			StartCoroutine(SoulControl());

			Random random = new Random();
			int numToSpawn = random.Next(1, 4);
			for(int i = 0; i < numToSpawn; i++)
			{
				GameObject balloon = Instantiate(balloonGO);
				balloon.transform.position = HeroController.instance.transform.position;
				balloon.transform.position += new Vector3(random.Next(-4, 4), random.Next(0, 4), 0);
				balloon.SetActive(true);
				balloon.LocateMyFSM("Control").SendEvent("SPAWN");
			}

			return damage;
		}

		private IEnumerator NailControl()
		{
			On.HealthManager.Hit += HealthManagerHit;
			yield return new WaitForSeconds(5f);
			On.HealthManager.Hit -= HealthManagerHit;
			yield break;
		}

		private void HealthManagerHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
		{
			if(hitInstance.AttackType == AttackTypes.Nail || hitInstance.AttackType == AttackTypes.NailBeam)
			{
				hitInstance.DamageDealt /= 2 + 1;
			}
			orig(self, hitInstance);
		}

		private IEnumerator SoulControl()
		{
			for(int i = 0; i < 33; i++)
			{
				HeroController.instance.TakeMPQuick(3);
				yield return new WaitForSeconds(0.3f);
			}
			yield break;
		}

		public override void StopEffect()
		{
			ModHooks.TakeHealthHook -= TakeHealthHook;
			On.HealthManager.Hit -= HealthManagerHit;

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Infected Wounds";
		}
	}
}
