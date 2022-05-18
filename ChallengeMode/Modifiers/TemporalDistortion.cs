using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class TemporalDistortion : Modifier
	{
		private Random random;
		private bool flag;
		private PlayMakerFSM dreamnailFSM;
		private GameObject flash;

		private AudioClip audioSet;
		private AudioClip audioWarp;
		private AudioSource audioSource;

		public override void StartEffect()
		{
			random = new Random();
			flag = true;

			dreamnailFSM = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
			flash = dreamnailFSM.GetAction<SpawnObjectFromGlobalPool>("Set", 12).gameObject.Value;

			audioSet = dreamnailFSM.GetAction<AudioPlayerOneShotSingle>("Set", 11).audioClip.Value as AudioClip;
			audioWarp = dreamnailFSM.GetAction<AudioPlayerOneShotSingle>("Warp End", 9).audioClip.Value as AudioClip;
			audioSource = HeroController.instance.GetComponent<AudioSource>();

			StartCoroutine(TeleportControl());
		}

		private IEnumerator TeleportControl()
		{
			yield return new WaitForSeconds(5f);
			while(flag)
			{
				//Set gate
				Vector3 position = HeroController.instance.transform.position;
				GameObject dreamgate = Instantiate(GameManager.instance.sm.dreamgateObject, position, Quaternion.identity);
				audioSource.PlayOneShot(audioSet);

				//Telegraph
				for(int i = 0; i < 3; i++)
				{
					Instantiate(flash, position, Quaternion.identity);
					yield return new WaitForSeconds(1f);
				}

				//Warp to gate
				HeroController.instance.parryInvulnTimer = 0.6f;
				HeroController.instance.transform.position = position;
				Destroy(dreamgate);
				Instantiate(flash, position, Quaternion.identity);
				audioSource.PlayOneShot(audioWarp);

				yield return new WaitForSeconds(random.Next(10, 13));
			}
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			flag = false;
		}

		public override string ToString()
		{
			return "ChallengeMode_Temporal Distortion";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_A Fool's Errand"
			};
		}
	}
}
