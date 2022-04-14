using System.Collections;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
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
		private int numWaves;
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
			enemies = new GameObject[6];
			//Armored Squit
			enemies[0] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 2/Colosseum Cage Small"];
			//Battle Obble
			enemies[1] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 37/Colosseum Cage Small (3)"];
			//Shielded Fool
			enemies[2] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 3/Colosseum Cage Large"];
			//Sturdy Fool
			enemies[3] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 50/Colosseum Cage Large"];
			//Heavy Fool
			enemies[4] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 1/Colosseum Cage Large"];
			//Winged Fool
			enemies[5] = ChallengeMode.Instance.preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 6/Colosseum Cage Large"];
			
			waveFlag = true;
			enemyFlag = false;
			numWaves = 0;
			numEnemies = 1;
			random = new Random();
			audioSource = HeroController.instance.GetComponent<AudioSource>();

			StartCoroutine(WaveControl());
		}

		private IEnumerator WaveControl()
		{
			yield return new WaitForSeconds(7.5f);
			while(waveFlag)
			{
				Vector3 spawnPos = new Vector3(HeroController.instance.transform.position.x, spikes[0].transform.position.y + 6f);

				//Spikes
				yield return new WaitWhile(() => HeroController.instance.controlReqlinquished);
				PlayMakerFSM.BroadcastEvent("EXPAND");
				audioSource.PlayOneShot(audioSpikeAntic);
				yield return new WaitForSeconds(2f);
				audioSource.PlayOneShot(audioSpikeExpand);
				yield return new WaitForSeconds(5f);
				PlayMakerFSM.BroadcastEvent("RETRACT");
				audioSource.PlayOneShot(audioSpikeRetract);

				yield return new WaitForSeconds(random.Next(5, 10));

				//Enemies
				enemyFlag = true;
				StartCoroutine(SpawnEnemies(spawnPos));
				yield return new WaitWhile(() => enemyFlag);

				yield return new WaitForSeconds(random.Next(5, 10));
			}
			yield break;
		}

		private IEnumerator SpawnEnemies(Vector3 spawnPos)
		{
			for(int i = 0; i < numEnemies + random.Next(0, 2); i++)
			{
				int index = random.Next(0, enemies.Length);

				//Spawn cage
				GameObject cage = Instantiate(enemies[index], spawnPos, Quaternion.identity);
				PlayMakerFSM cageFSM = cage.LocateMyFSM("Spawn");
				cage.SetActive(true);
				cageFSM.SetState("Init");

				//Spawn enemy manually so we can keep track of when it dies
				GameObject enemy = null;
				if(index < 2) //Small cage
				{
					enemy = Instantiate(cageFSM.Fsm.GetFsmGameObject("Enemy Type").Value, spawnPos, Quaternion.identity);
					cageFSM.RemoveAction("Spawn", 0);
				}
				else //Large cage
				{
					enemy = Instantiate(cageFSM.Fsm.GetFsmGameObject("Corpse to Instantiate").Value, spawnPos, Quaternion.identity);
					cageFSM.RemoveAction("Spawn", 1);
				}
				enemy.SetActive(false);

				//Set enemy active when cage comes up
				cageFSM.InsertMethod("Pause", () =>
				{
					enemy.SetActive(true);
				}, 0);

				//Start cage
				cageFSM.SendEvent("SPAWN");
				yield return new WaitWhile(() => enemy != null);
			}

			//Update number of waves/enemies (increase minimum number of enemies to 2 after 2 waves)
			numWaves++;
			if(numWaves == 2) numEnemies = 2;

			enemyFlag = false;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			foreach(GameObject spike in spikes)
			{
				spike.Recycle();
			}
			waveFlag = false;
			enemyFlag = false;
			numEnemies = 1;
		}

		public override string ToString()
		{
			return "ChallengeMode_A Fool's Errand";
		}
	}
}
