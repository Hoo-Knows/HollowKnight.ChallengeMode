using UnityEngine;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using GlobalEnums;

namespace ChallengeMode.Modifiers
{
	class PaleWatchU : Modifier
	{
		private GameObject[] kingsmoulds;
		private PlayMakerFSM pvFSM;
		private bool attacking;

		public override void StartEffect()
		{
			kingsmoulds = new GameObject[2];
			kingsmoulds[0] = Instantiate(ChallengeMode.Preloads["White_Palace_02"]["Battle Scene/Royal Gaurd"],
				new Vector3(31.5f, 8.6f, 0f), Quaternion.identity);
			kingsmoulds[1] = Instantiate(ChallengeMode.Preloads["White_Palace_02"]["Battle Scene/Royal Gaurd"],
				new Vector3(59.5f, 8.6f, 0f), Quaternion.identity);

			for(int i = 0; i < 2; i++)
			{
				PlayMakerFSM kingsmouldFSM = kingsmoulds[i].LocateMyFSM("Guard");
				
				//Remove contact damage and increase hp
				Destroy(kingsmoulds[i].GetComponent<DamageHero>());
				kingsmoulds[i].GetComponent<HealthManager>().hp = int.MaxValue;
				kingsmoulds[i].layer = (int)PhysLayers.CORPSE;

				//Prevent waking up when player gets close
				kingsmouldFSM.RemoveAction("Rest", 0);

				//Make Kingsmould face player
				kingsmouldFSM.InsertAction("Rest", kingsmouldFSM.GetAction<FaceObject>("Chase", 1), 0);

				//Prevent automatic attacks
				kingsmouldFSM.RemoveTransition("Idle", "EVADE");
				kingsmouldFSM.RemoveTransition("Idle", "THROW");
				kingsmouldFSM.RemoveTransition("Idle", "SLASH");
				kingsmouldFSM.RemoveTransition("Idle", "CHASE");

				//Set attacking var to false when idle
				kingsmouldFSM.InsertMethod("Idle", () =>
				{
					attacking = false;
				}, 0);

				//Increase delay after attack
				kingsmouldFSM.GetAction<WaitRandom>("Wait", 2).timeMin = 3f;
				kingsmouldFSM.GetAction<WaitRandom>("Wait", 2).timeMax = 3f;

				kingsmoulds[i].SetActive(true);
			}

			attacking = false;
			StartCoroutine(WaitToActivate());
		}

		private IEnumerator WaitToActivate()
		{
			yield return new WaitUntil(() => GameObject.Find("HK Prime") != null);

			pvFSM = GameObject.Find("HK Prime").LocateMyFSM("Control");

			//Activate Kingsmoulds during opening animation
			pvFSM.InsertMethod("Intro 5", () =>
			{
				kingsmoulds[0].LocateMyFSM("Guard").SendEvent("WAKE");
				kingsmoulds[1].LocateMyFSM("Guard").SendEvent("WAKE");
			}, 0);

			//Make Kingsmoulds attack if PV is idle and not already attacking
			pvFSM.InsertMethod("Idle", () =>
			{
				if(!attacking)
				{
					kingsmoulds[0].LocateMyFSM("Guard").SetState("Throw Start");
					kingsmoulds[1].LocateMyFSM("Guard").SetState("Throw Start");
					attacking = true;
				}
			}, 0);

			yield break;
		}

		public override void StopEffect()
		{
			pvFSM.RemoveAction("Intro 5", 0);
			pvFSM.RemoveAction("Idle", 0);

			Destroy(kingsmoulds[0]);
			Destroy(kingsmoulds[1]);
		}

		public override string ToString()
		{
			return "ChallengeMode_Pale Watch";
		}
	}
}
