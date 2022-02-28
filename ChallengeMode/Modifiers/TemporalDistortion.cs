using Random = System.Random;
using System.Collections;
using UnityEngine;
using ModCommon.Util;
using HutongGames.PlayMaker.Actions;

namespace ChallengeMode.Modifiers
{
	class TemporalDistortion : Modifier
	{
		private Random random;
		private bool flag;
		private PlayMakerFSM dreamnailFSM;
		private GameObject flash;

		public override void StartEffect()
		{
			random = new Random();
			flag = true;

			dreamnailFSM = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
			flash = dreamnailFSM.GetAction<SpawnObjectFromGlobalPool>("Set", 12).gameObject.Value;

			StartCoroutine(TeleportControl());
		}

		private IEnumerator TeleportControl()
		{
			while(flag)
			{
				yield return new WaitForSeconds(random.Next(10, 13));

				Vector3 position = HeroController.instance.transform.position;
				GameObject dreamgate = Instantiate(GameManager.instance.sm.dreamgateObject, position, Quaternion.identity);
				Instantiate(flash, position, Quaternion.identity);

				yield return new WaitForSeconds(3f);

				HeroController.instance.transform.position = position;
				dreamgate.Recycle();
				Instantiate(flash, position, Quaternion.identity);
			}
			yield break;
		}

		public override void StopEffect()
		{
			flag = false;

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Temporal Distortion";
		}
	}
}
