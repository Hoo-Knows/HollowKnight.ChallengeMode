using Modding;
using UnityEngine;
using GlobalEnums;
using System.Collections;

namespace ChallengeMode.Modifiers
{
	class AspidRancher : Modifier
	{
		private GameObject aspidGO;
		private bool spawnFlag;

		public override void StartEffect()
		{
			aspidGO = ChallengeMode.Instance.preloadedObjects["Deepnest_East_04"]["Super Spitter"];
			Destroy(aspidGO.GetComponent<PersistentBoolItem>());
			HealthManager hm = aspidGO.GetComponent<HealthManager>();
			hm.hp = 1;
			//Disable soul gain
			ReflectionHelper.SetField(hm, "enemyType", 6);

			spawnFlag = true;

			ModHooks.AfterAttackHook += AfterAttackHook;
			ModHooks.SlashHitHook += SlashHitHook;
			ModHooks.HeroUpdateHook += HeroUpdateHook;
		}

		private void AfterAttackHook(AttackDirection dir)
		{
			StartCoroutine(SpawnAspid());
		}

		private IEnumerator SpawnAspid()
		{
			spawnFlag = true;
			yield return new WaitWhile(() => HeroController.instance.cState.attacking);

			if(spawnFlag)
			{
				GameObject aspid = Instantiate(aspidGO);
				foreach(PlayMakerFSM fsm in aspid.GetComponentsInChildren<PlayMakerFSM>())
				{
					fsm.SetState(fsm.Fsm.StartState);
				}
				aspid.transform.position = HeroController.instance.transform.position;
				aspid.transform.position += 4 * (HeroController.instance.cState.facingRight ? Vector3.left : Vector3.right);
				aspid.SetActive(true);
			}

			yield break;
		}

		private void SlashHitHook(Collider2D otherCollider, GameObject slash)
		{
			if(otherCollider.gameObject.GetComponent<HealthManager>() != null)
			{
				spawnFlag = false;
			}
		}

		private void HeroUpdateHook()
		{
			if(HeroController.instance.cState.bouncing || HeroController.instance.cState.nailCharging)
			{
				spawnFlag = false;
			}
		}

		public override void StopEffect()
		{
			spawnFlag = false;

			ModHooks.AfterAttackHook -= AfterAttackHook;
			ModHooks.SlashHitHook -= SlashHitHook;
			ModHooks.HeroUpdateHook += HeroUpdateHook;
			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Aspid Rancher";
		}
	}
}
