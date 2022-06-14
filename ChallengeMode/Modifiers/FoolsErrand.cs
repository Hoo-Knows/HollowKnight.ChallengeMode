using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class FoolsErrand : Modifier
	{
		private List<GameObject> spikes;
		private GameObject spikeGO;

		private GameObject groundSpikesGO;
		private PlayMakerFSM groundSpikesFSM;
		private AudioClip audioSpikeAntic;
		private AudioClip audioSpikeExpand;
		private AudioClip audioSpikeRetract;

		private List<GameObject> enemies;
		private Random random;
		private AudioSource audioSource;
		private bool waveFlag;
		private bool enemyFlag;
		private int numEnemies;
		private bool allowSpikes;

		private readonly List<string> spikeScenes = new List<string>()
		{
			"GG_Gruz_Mother", "GG_Gruz_Mother_V", "GG_False_Knight", "GG_Mega_Moss_Charger", "GG_Hornet_1", "GG_Dung_Defender",
			"GG_Mage_Knight", "GG_Mage_Knight_V", "GG_Brooding_Mawlek", "GG_Brooding_Mawlek_V", "GG_Nailmasters",
			"GG_Crystal_Guardian", "GG_Soul_Master", "GG_Oblobbles", "GG_Mantis_Lords", "GG_Mantis_Lords_V", "GG_Ghost_Marmu",
			"GG_Ghost_Marmu_V", "GG_Broken_Vessel", "GG_Galien", "GG_Painter", "GG_Hive_Knight", "GG_Ghost_Hu", "GG_Collector",
			"GG_Collector_V", "GG_God_Tamer", "GG_Grimm", "GG_Watcher_Knights", "GG_Nosk", "GG_Nosk_V", "GG_Sly", "GG_Hornet_2",
			"GG_Crystal_Guardian_2", "GG_Lost_Kin", "GG_Traitor_Lord", "GG_White_Defender", "GG_Soul_Tyrant", "GG_Grey_Prince_Zote",
			"GG_Failed_Champion", "GG_Hollow_Knight"
		};

		public override void StartEffect()
		{
			//Set spikes
			spikeGO = ChallengeMode.Preloads["Room_Colosseum_Bronze"]["Colosseum Manager/Ground Spikes/Colosseum Spike (19)"];
			spikes = new List<GameObject>();
			for(int i = 0; i < 30; i++)
			{
				GameObject spike = Instantiate(spikeGO,
					HeroController.instance.transform.position + new Vector3(i * 2f - 30f, -0.25f, 0f), Quaternion.identity);
				spike.GetComponent<DamageHero>().hazardType = 1;
				spike.LocateMyFSM("Enemy Hurt").GetAction<SetFsmInt>("Hurt", 0).setValue = 0;
				spike.SetActive(true);
				spikes.Add(spike);
			}

			//Spike audio
			groundSpikesGO = ChallengeMode.Preloads["Room_Colosseum_Bronze"]["Colosseum Manager/Ground Spikes"];
			groundSpikesFSM = groundSpikesGO.LocateMyFSM("Spike Audio");
			audioSpikeAntic = groundSpikesFSM.GetAction<AudioPlaySimple>("Antic", 1).oneShotClip.Value as AudioClip;
			audioSpikeExpand = groundSpikesFSM.GetAction<AudioPlaySimple>("Expand", 3).oneShotClip.Value as AudioClip;
			audioSpikeRetract = groundSpikesFSM.GetAction<AudioPlaySimple>("Retract", 2).oneShotClip.Value as AudioClip;

			//Set possible enemies
			enemies = new List<GameObject>();
			//Armored Squit
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 2/Colosseum Cage Small"]);
			//Battle Obble
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 37/Colosseum Cage Small (3)"]);
			//Shielded Fool
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 3/Colosseum Cage Large"]);
			//Sturdy Fool
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 50/Colosseum Cage Large"]);
			//Heavy Fool
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 1/Colosseum Cage Large"]);
			//Winged Fool
			enemies.Add(ChallengeMode.Preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 6/Colosseum Cage Large"]);

			waveFlag = true;
			enemyFlag = false;
			numEnemies = 1;
			random = new Random();
			audioSource = HeroController.instance.GetComponent<AudioSource>();
			allowSpikes = spikeScenes.Contains(GameManager.instance.sceneName);

			StartCoroutine(WaveControl());
		}

		private IEnumerator WaveControl()
		{
			yield return new WaitForSeconds(7.5f);
			while(waveFlag)
			{
				//Spikes
				if(allowSpikes)
				{
					yield return new WaitWhile(() => HeroController.instance.controlReqlinquished);
					PlayMakerFSM.BroadcastEvent("EXPAND");
					audioSource.PlayOneShot(audioSpikeAntic);
					yield return new WaitForSeconds(2f);
					audioSource.PlayOneShot(audioSpikeExpand);
					yield return new WaitForSeconds(5f);
					PlayMakerFSM.BroadcastEvent("RETRACT");
					audioSource.PlayOneShot(audioSpikeRetract);

					yield return new WaitForSeconds(random.Next(3, 5));
				}

				//Enemies
				enemyFlag = true;
				StartCoroutine(SpawnEnemies());
				yield return new WaitWhile(() => enemyFlag);

				yield return new WaitForSeconds(random.Next(15, 20));
			}
			yield break;
		}

		private IEnumerator SpawnEnemies()
		{
			for(int i = 0; i < numEnemies + random.Next(0, 2); i++)
			{
				int index = random.Next(0, enemies.Count);

				//Spawn cage
				Vector3 spawnPos = new Vector3(HeroController.instance.transform.position.x, 
					HeroController.instance.transform.position.y + 5f);
				GameObject cage = Instantiate(enemies[index], spawnPos, Quaternion.identity);
				PlayMakerFSM cageFSM = cage.LocateMyFSM("Spawn");
				cage.SetActive(true);
				cageFSM.SetState("Init");

				//Spawn enemy manually so we can keep track of when it dies
				GameObject enemy = null;
				if(index < 2) //Small cage
				{
					enemy = Instantiate(cageFSM.FsmVariables.FindFsmGameObject("Enemy Type").Value, spawnPos, Quaternion.identity);
					cageFSM.RemoveAction("Spawn", 0);
				}
				else //Large cage
				{
					enemy = Instantiate(cageFSM.FsmVariables.FindFsmGameObject("Corpse to Instantiate").Value, spawnPos, Quaternion.identity);
					cageFSM.RemoveAction("Spawn", 1);
				}
				//Scale hp to be balanced around low level nail
				HealthManager hm = enemy.GetComponent<HealthManager>();
				hm.hp *= PlayerData.instance.GetInt("nailDamage");
				hm.hp /= 21;

				//Prevent geo gain
				hm.SetGeoLarge(0);
				hm.SetGeoMedium(0);
				hm.SetGeoSmall(0);

				enemy.SetActive(false);

				//Set enemy active when cage comes up
				cageFSM.InsertMethod("Pause", () =>
				{
					enemy.SetActive(true);
				}, 0);

				//Start cage
				cageFSM.SendEvent("SPAWN");
				yield return new WaitWhile(() => enemy != null);
				Destroy(enemy);
				yield return new WaitForSeconds(0.5f);
			}

			//Increase number of enemies after one wave
			if(numEnemies == 1) numEnemies++;

			enemyFlag = false;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			foreach(GameObject spike in spikes)
			{
				Destroy(spike);
			}
			waveFlag = false;
			enemyFlag = false;
			numEnemies = 1;
		}

		public override string ToString()
		{
			return "ChallengeMode_A Fool's Errand";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress",
				"ChallengeMode_Ascension",
				"ChallengeMode_Nailmaster",
				"ChallengeMode_Past Regrets",
				"ChallengeMode_Unfriendly Fire",
				"ChallengeMode_Temporal Distortion"
			};
		}
	}
}
