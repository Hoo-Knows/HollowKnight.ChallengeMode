using System.Collections;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class AdrenalineRush : Modifier
	{
		private float customTimeScale;

		public override void StartEffect()
		{
			customTimeScale = 1f;

			ModHooks.TakeHealthHook += TakeHealthHook;
		}

		private int TakeHealthHook(int damage)
		{
			customTimeScale = Mathf.Min(customTimeScale + 0.025f, 1.5f);
			StartCoroutine(WaitAfterRecoil());
			return damage;
		}

		private IEnumerator WaitAfterRecoil()
		{
			//recoilDuration is 0.5 seconds
			yield return new WaitForSeconds(0.5f);
			Time.timeScale = customTimeScale;
			yield break;
		}

		public override void StopEffect()
		{
			ModHooks.TakeHealthHook -= TakeHealthHook;
			StopAllCoroutines();

			customTimeScale = 1f;
			Time.timeScale = 1f;
		}

		public override string ToString()
		{
			return "ChallengeMode_Adrenaline Rush";
		}
	}
}
