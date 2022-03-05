using System.Collections;
using Modding;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class InfectedWounds : Modifier
	{
		private GameObject balloonGO;
		private int nailDamage;

		public override void StartEffect()
		{
			balloonGO = ChallengeMode.Instance.preloadedObjects["Abyss_19"]["Parasite Balloon (1)"];
			Destroy(balloonGO.GetComponent<PersistentBoolItem>());
			HealthManager hm = balloonGO.GetComponent<HealthManager>();
			hm.hp = 1;
			//Disable soul gain
			ReflectionHelper.SetField(hm, "enemyType", 6);

			nailDamage = PlayerData.instance.nailDamage;

			ModHooks.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			StopAllCoroutines();
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
			PlayerData.instance.nailDamage = 1;
			PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
			yield return new WaitForSeconds(7.5f);
			PlayerData.instance.SetInt("nailDamage", nailDamage);
			PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
			yield break;
		}

		private IEnumerator SoulControl()
		{
			for(int i = 0; i < 9; i++)
			{
				HeroController.instance.TakeMPQuick(11);
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}

		public override void StopEffect()
		{
			ModHooks.TakeHealthHook -= TakeHealthHook;
			StopAllCoroutines();

			PlayerData.instance.SetInt("nailDamage", nailDamage);
			PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
		}

		public override string ToString()
		{
			return "ChallengeMode_Infected Wounds";
		}
	}
}
