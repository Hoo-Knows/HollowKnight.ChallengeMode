using System.Collections;
using GlobalEnums;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class AspidRancher : Modifier
	{
		private GameObject aspidGO;
		private bool spawnFlag;
		private bool waiting;

		public override void StartEffect()
		{
			aspidGO = ChallengeMode.instance.preloadedObjects["Deepnest_East_04"]["Super Spitter"];
			Destroy(aspidGO.GetComponent<PersistentBoolItem>());
			HealthManager hm = aspidGO.GetComponent<HealthManager>();
			hm.hp = 1;
			//Disable soul gain
			ReflectionHelper.SetField(hm, "enemyType", 6);

			spawnFlag = true;
			waiting = false;

			ModHooks.AfterAttackHook += AfterAttackHook;
			ModHooks.SlashHitHook += SlashHitHook;
		}

		private void AfterAttackHook(AttackDirection dir)
		{
			if(waiting)
			{
				StopAllCoroutines();
				TrySpawnAspid();
			}
			spawnFlag = true;
			StartCoroutine(WaitToSpawn());
		}

		private void SlashHitHook(Collider2D otherCollider, GameObject slash)
		{
			GameObject go = otherCollider.gameObject;
			if(go.GetComponent<HealthManager>() != null || go.GetComponent<DamageHero>() != null)
			{
				spawnFlag = false;
			}
		}

		private IEnumerator WaitToSpawn()
		{
			waiting = true;
			yield return new WaitWhile(() => HeroController.instance.cState.attacking);
			waiting = false;

			TrySpawnAspid();
			yield break;
		}

		private void TrySpawnAspid()
		{
			if(!spawnFlag || HeroController.instance.cState.nailCharging) return;

			GameObject aspid = Instantiate(aspidGO);
			foreach(PlayMakerFSM fsm in aspid.GetComponentsInChildren<PlayMakerFSM>())
			{
				fsm.SetState(fsm.Fsm.StartState);
			}
			aspid.transform.position = HeroController.instance.transform.position;
			aspid.transform.position += 4 * (HeroController.instance.cState.facingRight ? Vector3.left : Vector3.right);
			aspid.SetActive(true);
		}

		public override void StopEffect()
		{
			ModHooks.AfterAttackHook -= AfterAttackHook;
			ModHooks.SlashHitHook -= SlashHitHook;
			StopAllCoroutines();

			spawnFlag = false;
			waiting = false;
		}

		public override string ToString()
		{
			return "ChallengeMode_Aspid Rancher";
		}
	}
}
