using UnityEngine;
using SFCore.Utils;
using Random = System.Random;
using System.Collections;
using HutongGames.PlayMaker.Actions;

namespace ChallengeMode.Modifiers
{
	class EphemeralOrdealU : Modifier
	{
		private GameObject battleCtrl;
		private GameObject[] zotelings;
		private PlayMakerFSM gpzFSM;
		private PlayMakerFSM zotelingFSM;
		private int numFat;

		public override void StartEffect()
		{
			//Ordeal Control
			battleCtrl = ChallengeMode.Instance.preloadedObjects["GG_Mighty_Zote"]["Battle Control"];

			//Set Zotelings
			zotelings = new GameObject[7];
			//Regular Zoteling
			zotelings[0] = battleCtrl.transform.Find("Dormant Warriors").gameObject.transform.Find("Zote Crew Normal (1)").gameObject;
			//Winged/Hopping Zoteling
			zotelings[1] = battleCtrl.transform.Find("Zotelings").gameObject.transform.Find("Ordeal Zoteling").gameObject;
			//Lanky Zoteling
			zotelings[2] = battleCtrl.transform.Find("Tall Zotes").gameObject.transform.Find("Zote Crew Tall").gameObject;
			//Heavy Zoteling
			zotelings[3] = battleCtrl.transform.Find("Fat Zotes").gameObject.transform.Find("Zote Crew Fat (1)").gameObject;
			//Fluke Zoteling
			zotelings[4] = battleCtrl.transform.Find("Zote Fluke").gameObject;
			//Salubra Zoteling
			zotelings[5] = battleCtrl.transform.Find("Zote Salubra").gameObject;

			gpzFSM = GameObject.Find("Grey Prince").LocateMyFSM("Control");

			//Clear spit states
			for(int i = 0; i < 7; i++)
			{
				gpzFSM.RemoveAction("Spit L", 0);
				gpzFSM.RemoveAction("Spit R", 0);
			}

			//Custom spit method
			gpzFSM.InsertMethod("Spit L", () => StartCoroutine(Spawn(true)), 0);
			gpzFSM.InsertMethod("Spit R", () => StartCoroutine(Spawn(false)), 0);

			//Increase max spits
			//gpzFSM.GetAction<SetIntValue>("Level 3", 13).intValue = 2;
			//gpzFSM.GetAction<SetIntValue>("Level 3", 14).intValue = 3;

			numFat = 0;
		}

		private IEnumerator Spawn(bool left)
		{
			Random random = new Random();
			int index = random.Next(0, 5);

			while(index == 3 && numFat >= 1)
			{
				index = random.Next(0, 5);
			}

			Vector3 pos = gpzFSM.FsmVariables.FindFsmGameObject("Spit Point").Value.transform.position;
			Vector2 velocity = new Vector2(left ? random.Next(-20, -5) : random.Next(5, 20), random.Next(20, 30));
			GameObject zoteling = Instantiate(zotelings[index], pos, Quaternion.identity);
			zoteling.GetComponent<Rigidbody2D>().velocity = velocity;
			Destroy(zoteling.GetComponent<PersistentBoolItem>());
			Destroy(zoteling.GetComponent<ConstrainPosition>());

			//Switch statement of doom
			zotelingFSM = zoteling.LocateMyFSM("Control");
			switch(index)
			{
				case 0: //Regular Zoteling
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Multiply"), 0);
					zotelingFSM.InsertMethod("Spawn Antic", () => zotelingFSM.SendEvent("FINISHED"), 8);
					zotelingFSM.RemoveAction("Spawn Antic", 7);
					zotelingFSM.RemoveAction("Spawn Antic", 4);
					zotelingFSM.RemoveAction("Spawn Antic", 1);
					zotelingFSM.RemoveAction("Tumble Out", 2);
					zotelingFSM.RemoveTransition("Death Reset", "FINISHED");
					break;
				case 1: //Winged/Hopping Zoteling
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Ball"), 0);
					zotelingFSM.RemoveAction("Ball", 2);
					zotelingFSM.RemoveTransition("Reset", "FINISHED");
					break;
				case 2: //Lanky Zoteling
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Multiply"), 0);
					zotelingFSM.InsertMethod("Spawn Antic", () => zotelingFSM.SendEvent("FINISHED"), 8);
					zotelingFSM.RemoveAction("Spawn Antic", 7);
					zotelingFSM.RemoveAction("Spawn Antic", 4);
					zotelingFSM.RemoveAction("Spawn Antic", 1);
					zotelingFSM.RemoveAction("Tumble Out", 2);
					zotelingFSM.RemoveTransition("Death Reset", "FINISHED");
					break;
				case 3: //Heavy Zoteling
					numFat++;
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Multiply"), 0);
					zotelingFSM.InsertMethod("Spawn Antic", () => zotelingFSM.SendEvent("FINISHED"), 8);
					zotelingFSM.RemoveAction("Spawn Antic", 7);
					zotelingFSM.RemoveAction("Spawn Antic", 4);
					zotelingFSM.RemoveAction("Spawn Antic", 1);
					zotelingFSM.RemoveAction("Tumble Out", 2);
					zotelingFSM.InsertMethod("Dr", () =>
					{
						if(zotelingFSM.transform.position.x < HeroController.instance.transform.position.x)
						{
							zotelingFSM.SendEvent("R");
						}
						else
						{
							zotelingFSM.SendEvent("L");
						}
					}, 2);
					zotelingFSM.RemoveAction("Dr", 1);
					zotelingFSM.InsertMethod("Death Reset", () =>
					{
						numFat--;
					}, 3);
					zotelingFSM.RemoveTransition("Death Reset", "FINISHED");
					break;
				case 4: //Fluke Zoteling
					zotelingFSM.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Pos"), 0);
					zotelingFSM.RemoveAction("Pos", 3);
					zotelingFSM.RemoveAction("Antic", 3);
					zotelingFSM.GetAction<FloatCompare>("Climb", 3).float2 = 40f;
					zotelingFSM.RemoveAction("Sleeping", 1);
					zotelingFSM.RemoveTransition("Sleeping", "GO");
					break;
				case 5: //Salubra Zoteling
					zotelingFSM.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
					zotelingFSM.InsertMethod("Dormant", () => zotelingFSM.SetState("Appear"), 0);
					zotelingFSM.RemoveAction("Appear", 6);
					zotelingFSM.RemoveAction("Appear", 3);
					zotelingFSM.RemoveAction("Idle", 1);
					zotelingFSM.RemoveAction("Idle", 0);
					zotelingFSM.GetAction<GhostMovement>("Sucking", 8).xPosMin.Value = 9f;
					zotelingFSM.GetAction<GhostMovement>("Sucking", 8).xPosMax.Value = 44f;
					zotelingFSM.RemoveAction("Dead", 1);
					zotelingFSM.RemoveTransition("Dead", "FINISHED");
					break;
			}
			zoteling.SetActive(true);
			yield break;
		}

		public override void StopEffect() { }

		public override string ToString()
		{
			return "ChallengeMode_The Ephemeral Ordeal";
		}
	}
}
