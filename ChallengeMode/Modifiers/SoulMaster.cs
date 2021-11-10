using System.Collections;
using UnityEngine;
using Modding;

namespace ChallengeMode.Modifiers
{
	class SoulMaster : Modifier
	{
		private bool flag;

		public override void StartEffect()
		{
			flag = true;

			ModHooks.Instance.GetPlayerIntHook += GetPlayerIntHook;
			StartCoroutine(SoulHandler());
		}

		private int GetPlayerIntHook(string target)
		{
			if(target == "nailDamage") return 1;
			return PlayerData.instance.GetIntInternal(target);
		}

		private IEnumerator SoulHandler()
		{
			while(flag)
			{
				HeroController.instance.AddMPCharge(11);
				yield return new WaitForSecondsRealtime(1.5f);
			}
		}

		public override void StopEffect()
		{
			flag = false;

			ModHooks.Instance.GetPlayerIntHook -= GetPlayerIntHook;
			StopCoroutine(SoulHandler());
		}
	}
}
