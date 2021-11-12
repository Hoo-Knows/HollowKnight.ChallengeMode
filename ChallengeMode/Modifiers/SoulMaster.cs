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
			StartCoroutine(HandleSoul());
		}

		private int GetPlayerIntHook(string target)
		{
			if(target == "nailDamage") return 1;
			return PlayerData.instance.GetIntInternal(target);
		}

		private IEnumerator HandleSoul()
		{
			while(flag)
			{
				HeroController.instance.AddMPChargeSpa(11);
				yield return new WaitForSecondsRealtime(1.5f);
			}
			yield break;
		}

		public override void StopEffect()
		{
			flag = false;

			ModHooks.Instance.GetPlayerIntHook -= GetPlayerIntHook;
			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Soul Master";
		}
	}
}
