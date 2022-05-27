using UnityEngine;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class SomethingWickedU : Modifier
	{
		private GameObject[] grimmkins;
		private PlayMakerFSM nkgFSM;

		public override void StartEffect()
		{
			GameObject spawnGO = ChallengeMode.Instance.preloadedObjects["Hive_03"]["Flamebearer Spawn"];
			grimmkins = new GameObject[2];

			for(int i = 0; i < 2; i++)
			{
				grimmkins[i] = Instantiate(spawnGO.LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value);
				grimmkins[i].SetActive(false);
				Destroy(grimmkins[i].GetComponent<DamageHero>());
				Destroy(grimmkins[i].GetComponent<Collider2D>());
				grimmkins[i].transform.position = HeroController.instance.transform.position;

				//FSM is located in sharedassets192.assets
				PlayMakerFSM grimmkinFSM = grimmkins[i].LocateMyFSM("Control");

				//Set delay between attacks
				grimmkinFSM.GetAction<WaitRandom>("Attack Pause", 0).timeMin = 5f;
				grimmkinFSM.GetAction<WaitRandom>("Attack Pause", 0).timeMax = 7.5f;

				//Make Grimmkin after jump to Attack Pause after Appear
				grimmkinFSM.InsertMethod("Init", () =>
				{
					grimmkinFSM.SendEvent("START");
				}, 23);
				grimmkinFSM.InsertMethod("Appear", () =>
				{
					grimmkinFSM.SetState("Attack Pause");
				}, 7);

				//Remove screenshake
				grimmkinFSM.RemoveAction("Appear", 2);

				//Make Grimmkin dash at player after returning from teleport
				grimmkinFSM.RemoveTransition("Attack Pause", "FINISHED");
				grimmkinFSM.AddTransition("Attack Pause", "FINISHED", "Tele In");
				grimmkinFSM.InsertMethod("Tele In", () =>
				{
					Random random = new Random();
					float x;
					if(HeroController.instance.transform.position.x > 90f) x = random.Next(68, 85);
					else if(HeroController.instance.transform.position.x < 80f) x = random.Next(85, 103);
					else x = random.Next(0, 2) == 0 ? 68f : 102f;

					grimmkinFSM.gameObject.transform.position = new Vector3(x, random.Next(7, 10));
					grimmkinFSM.SetState("Dash Antic");
				}, 8);
				grimmkinFSM.RemoveAction("Tele In", 9);

				//Make Grimmkin attack after dashing
				grimmkinFSM.RemoveTransition("Dash End", "FINISHED");
				grimmkinFSM.AddTransition("Dash End", "FINISHED", "Pillar Antic");

				//Set position of flame pillar
				grimmkinFSM.InsertMethod("Spawn Pillar", () =>
				{
					grimmkinFSM.FsmVariables.FindFsmVector2("Ray Hit Pos").Value =
						new Vector2(grimmkinFSM.gameObject.transform.position.x, 5.1f);
				}, 0);

				//Reduce number of pillars
				grimmkinFSM.GetAction<SetIntValue>("Pillar Antic", 7).intValue = 1;
			}

			nkgFSM = GameObject.Find("Nightmare Grimm Boss").LocateMyFSM("Control");

			//Decrease delay/add more Grimmkins each pufferfish
			nkgFSM.InsertMethod("Set Balloon 1", () =>
			{
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMin = 2.5f;
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMax = 5f;
			}, 0);
			nkgFSM.InsertMethod("Set Balloon 2", () =>
			{
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMin = 5f;
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMax = 7.5f;
				grimmkins[1].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMin = 5f;
				grimmkins[1].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMax = 7.5f;
				grimmkins[1].SetActive(true);
			}, 0);
			nkgFSM.InsertMethod("Set Balloon 3", () =>
			{
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMin = 2.5f;
				grimmkins[0].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMax = 5f;
				grimmkins[1].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMin = 2.5f;
				grimmkins[1].LocateMyFSM("Control").GetAction<WaitRandom>("Attack Pause", 0).timeMax = 5f;
			}, 0);

			StartCoroutine(WaitToActivate());
		}

		private IEnumerator WaitToActivate()
		{
			yield return new WaitForSeconds(7.5f);
			grimmkins[0].SetActive(true);
			yield break;
		}

		public override void StopEffect()
		{
			Destroy(grimmkins[0]);
			Destroy(grimmkins[1]);
		}

		public override string ToString()
		{
			return "ChallengeMode_Something Wicked";
		}
	}
}
