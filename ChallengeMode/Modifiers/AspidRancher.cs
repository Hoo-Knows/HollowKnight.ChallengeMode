using Modding;
using UnityEngine;
using GlobalEnums;
using System.Reflection;
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
			hm.hp = 13;
			//disable soul gain
			FieldInfo fi = ReflectionHelper.GetField(typeof(HealthManager), "enemyType");
			fi.SetValue(hm, 6);

			spawnFlag = true;

			ModHooks.Instance.AfterAttackHook += AfterAttackHook;
			On.HealthManager.Hit += HealthManagerHit;
			ModHooks.Instance.HeroUpdateHook += HeroUpdateHook;
		}

		private void AfterAttackHook(AttackDirection dir)
		{
			StartCoroutine(SpawnAspid());
		}

		private IEnumerator SpawnAspid()
		{
			yield return new WaitWhile(() => HeroController.instance.cState.attacking == true);

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
			spawnFlag = true;

			yield break;
		}

		private void HealthManagerHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
		{
			spawnFlag = false;
			orig(self, hitInstance);
		}

		private void HeroUpdateHook()
		{
			if(HeroController.instance.cState.bouncing == true) spawnFlag = false;
		}

		public override void StopEffect()
		{
			spawnFlag = false;

			ModHooks.Instance.AfterAttackHook -= AfterAttackHook;
			On.HealthManager.Hit -= HealthManagerHit;
			ModHooks.Instance.HeroUpdateHook += HeroUpdateHook;
			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Aspid Rancher";
		}
	}
}
