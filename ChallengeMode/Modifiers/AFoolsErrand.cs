using System.Collections;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class AFoolsErrand : Modifier
	{
		private GameObject[] spikes;
		private GameObject spikeGO;

		private GameObject groundSpikesGO;
		private PlayMakerFSM groundSpikesFSM;
		private AudioClip audioSpikeAntic;
		private AudioClip audioSpikeExpand;
		private AudioClip audioSpikeRetract;

		private GameObject[] enemies;
		private Random random;
		private AudioSource audioSource;
		private bool waveFlag;
		private bool enemyFlag;
		private int numEnemies;

		public override void StartEffect()
		{
			//Set spikes
			spikeGO = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Bronze"]["Colosseum Manager/Ground Spikes/Colosseum Spike (19)"];
			spikes = new GameObject[31];
			for(int i = -15; i < 16; i++)
			{
				GameObject spike = Instantiate(spikeGO,
					HeroController.instance.transform.position + new Vector3(i * 2f, -0.25f, 0f), Quaternion.identity);
				spike.GetComponent<DamageHero>().hazardType = 1;
				spike.LocateMyFSM("Enemy Hurt").GetAction<SetFsmInt>("Hurt", 0).setValue = 0;
				spike.SetActive(true);
				spikes[i + 15] = spike;
			}

			//Spike audio
			groundSpikesGO = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Bronze"]["Colosseum Manager/Ground Spikes"];
			groundSpikesFSM = groundSpikesGO.LocateMyFSM("Spike Audio");
			audioSpikeAntic = groundSpikesFSM.GetAction<AudioPlaySimple>("Antic", 1).oneShotClip.Value as AudioClip;
			audioSpikeExpand = groundSpikesFSM.GetAction<AudioPlaySimple>("Expand", 3).oneShotClip.Value as AudioClip;
			audioSpikeRetract = groundSpikesFSM.GetAction<AudioPlaySimple>("Retract", 2).oneShotClip.Value as AudioClip;

			//Set possible enemies
			enemies = new GameObject[5];
			//Armored Squit
			enemies[0] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 2/Colosseum Cage Small"];
			//Battle Obble
			enemies[1] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 37/Colosseum Cage Small (3)"];
			//Death Loodle
			enemies[2] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 10/Colosseum Cage Small (5)"];
			//Primal Aspid
			enemies[3] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 17/Colosseum Cage Small (2)"];
			//Winged Fool
			enemies[4] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 6/Colosseum Cage Large"];
			
			waveFlag = true;
			enemyFlag = false;
			numEnemies = 1;
			random = new Random();
			audioSource = HeroController.instance.GetComponent<AudioSource>();

			StartCoroutine(WaveControl());
			StartCoroutine(IncreaseNumEnemies());
		}

		private IEnumerator WaveControl()
		{
			yield return new WaitForSeconds(7.5f);
			while(waveFlag)
			{
				Vector3 spawnPos = new Vector3(HeroController.instance.transform.position.x, spikes[0].transform.position.y + 6f);

				//Expand spikes
				PlayMakerFSM.BroadcastEvent("EXPAND");
				audioSource.PlayOneShot(audioSpikeAntic);
				yield return new WaitForSeconds(1f);

				//Spawn Enemies
				enemyFlag = true;
				StartCoroutine(SpawnEnemies(spawnPos));

				//Expanded
				yield return new WaitForSeconds(1f);
				audioSource.PlayOneShot(audioSpikeExpand);

				//Retract spikes after enemies are dead
				yield return new WaitWhile(() => enemyFlag);
				yield return new WaitForSeconds(1f);
				PlayMakerFSM.BroadcastEvent("RETRACT");
				audioSource.PlayOneShot(audioSpikeRetract);
				yield return new WaitForSeconds(random.Next(10, 15));
			}
			yield break;
		}

		private IEnumerator SpawnEnemies(Vector3 spawnPos)
		{
			GameObject enemy = null;

			for(int i = 0; i < numEnemies + random.Next(0, 2); i++)
			{
				int index = random.Next(0, enemies.Length);

				//Spawn cage
				GameObject cage = Instantiate(enemies[index], spawnPos, Quaternion.identity);
				PlayMakerFSM cageFSM = cage.LocateMyFSM("Spawn");

				//Spawn enemy manually
				if(index != 4) //Small cage
				{
					enemy = Instantiate(cageFSM.Fsm.GetFsmGameObject("Enemy Type").Value, spawnPos, Quaternion.identity);
				}
				else //Large cage
				{
					enemy = Instantiate(cageFSM.Fsm.GetFsmGameObject("Corpse to Instantiate").Value, spawnPos, Quaternion.identity);
				}
				enemy.SetActive(false);
				cageFSM.InsertMethod("Pause", 0, () =>
				{
					enemy.SetActive(true);
				});
				cage.SetActive(true);
				cageFSM.SetState("Anim");
				yield return new WaitWhile(() => enemy != null);
			}
			enemyFlag = false;
		}

		private IEnumerator IncreaseNumEnemies()
		{
			yield return new WaitForSeconds(20f);
			numEnemies = 2;
			yield return new WaitForSeconds(30f);
			numEnemies = 3;
			yield break;
		}

		public override void StopEffect()
		{
			foreach(GameObject spike in spikes)
			{
				spike.Recycle();
			}
			waveFlag = false;
			enemyFlag = false;
			numEnemies = 1;

			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_A Fool's Errand";
		}
	}
}
