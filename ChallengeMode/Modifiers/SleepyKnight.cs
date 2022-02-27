using System.Collections;
using UnityEngine;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SleepyKnight : Modifier
	{
		private int sleepCounter;

		public override void StartEffect()
		{
			sleepCounter = 0;

			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
			ModHooks.Instance.AfterAttackHook += AfterAttackHook;
			On.HeroAnimationController.PlayIdle += PlayIdle;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			StopAllCoroutines();
			sleepCounter += 5;
			if(sleepCounter >= 20) StartCoroutine(Sleep());
			return true;
		}

		private void AfterAttackHook(GlobalEnums.AttackDirection dir)
		{
			StopAllCoroutines();
			sleepCounter += 1;
			if(sleepCounter >= 20) StartCoroutine(Sleep());
		}

		private void PlayIdle(On.HeroAnimationController.orig_PlayIdle orig, HeroAnimationController self)
		{
			self.animator.Play("Idle Hurt");
		}

		private IEnumerator Sleep()
		{
			//from HollowTwitch mod
			sleepCounter = 0;
			HeroAnimationController animControl = HeroController.instance.GetComponent<HeroAnimationController>();
			animControl.PlayClip("Wake Up Ground");
			HeroController.instance.StopAnimationControl();
			HeroController.instance.RelinquishControl();
			yield return new WaitForSeconds(animControl.GetClipDuration("Wake Up Ground"));
			HeroController.instance.StartAnimationControl();
			HeroController.instance.RegainControl();
			yield break;
		}

		public override void StopEffect()
		{
			sleepCounter = 0;

			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
			ModHooks.Instance.AfterAttackHook -= AfterAttackHook;
			On.HeroAnimationController.PlayIdle -= PlayIdle;

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Sleepy Knight";
		}
	}
}
