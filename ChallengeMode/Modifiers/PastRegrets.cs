using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using UnityEngine;
using Modding;

namespace ChallengeMode.Modifiers
{
	class PastRegrets : Modifier
	{
		private GameObject shadeGO;
		private PlayMakerFSM shadeFSM;
		private PlayMakerFSM spellFSM;
		private bool usingFireball;
		private bool usingQuake;
		private bool usingScream;
		private bool warping;

		public override void StartEffect()
		{
			shadeGO = Instantiate(GameManager.instance.sm.hollowShadeObject);
			shadeGO.transform.position = HeroController.instance.transform.position + 3 * Vector3.left;
			shadeFSM = shadeGO.LocateMyFSM("Shade Control");
			shadeGO.SetActive(true);

			usingFireball = false;
			usingQuake = false;
			usingScream = false;
			warping = false;

			//Remove jingle
			shadeGO.LocateMyFSM("Play Audio").RemoveTransition("Pause", "FINISHED");
			shadeGO.transform.Find("Music Control").gameObject.LocateMyFSM("Music Control").RemoveTransition("Init", "FINISHED");

			//Remove dreamnail cheese
			shadeGO.LocateMyFSM("Dreamnail Kill").RemoveTransition("Idle", "DREAM IMPACT");

			//Remove spell limit, increase hp, and set spell levels to max
			shadeFSM.InsertMethod("Init", () =>
			{
				shadeFSM.FsmVariables.FindFsmInt("SP").Value = int.MaxValue;
				shadeGO.GetComponent<HealthManager>().hp = int.MaxValue;
				shadeFSM.FsmVariables.FindFsmInt("Fireball Level").Value = 2;
				shadeFSM.FsmVariables.FindFsmInt("Quake Level").Value = 2;
				shadeFSM.FsmVariables.FindFsmInt("Scream Level").Value = 2;

			}, 25);

			//Remove roam limit
			shadeFSM.FsmVariables.FindFsmFloat("Max Roam").Value = 999f;

			//Make shade unfriendly
			shadeFSM.RemoveAction("Friendly?", 2);

			//Decrease frequency of random attacks and make them slashes with a delay
			shadeFSM.GetAction<WaitRandom>("Fly", 5).timeMin = 5f;
			shadeFSM.GetAction<WaitRandom>("Fly", 5).timeMax = 5f;
			shadeFSM.InsertMethod("Quake?", () =>
			{
				shadeFSM.SetState("Slash Antic");
			}, 0);

			//Decrease flight speed
			shadeFSM.GetAction<ChaseObject>("Fly", 4).speedMax = 1.5f;
			shadeFSM.GetAction<ChaseObjectV2>("Fly", 6).speedMax = 1.5f;

			//Teleport to different location depending on spell
			shadeFSM.InsertMethod("Retreat Start", () =>
			{
				warping = true;
			}, 0);
			shadeFSM.InsertMethod("Retreat", () =>
			{
				Vector3 position = HeroController.instance.transform.position;
				if(usingFireball)
				{
					if(HeroController.instance.cState.facingRight) position += 3f * Vector3.left;
					else position += 2f * Vector3.right;
					position += Vector3.down;
				}
				else if(usingQuake)
				{
					position += 6f * Vector3.up;
				}
				else if(usingScream)
				{
					position += 4f * Vector3.down;
				}
				shadeFSM.FsmVariables.FindFsmVector3("Start Pos").Value = position;
			}, 1);
			shadeFSM.InsertMethod("Retreat Reset", () =>
			{
				warping = false;
			}, 3);

			//Make shade face player before using fireball
			shadeFSM.InsertAction("Cast Antic", shadeFSM.GetAction<FaceObject>("Fireball Pos", 3), 0);

			//Increase warp speed
			shadeFSM.GetAction<iTweenMoveTo>("Retreat", 2).time = 0.1f;

			//Increase attack speed
			shadeFSM.GetAction<Wait>("Cast Antic", 8).time = 0.1f;
			shadeFSM.GetAction<Wait>("Quake Antic", 7).time = 0.45f;
			shadeFSM.GetAction<Wait>("Scream Antic", 6).time = 0.1f;

			//Reset variables after using a spell
			shadeFSM.InsertMethod("Cast", () =>
			{
				usingFireball = false;
			}, 4);
			shadeFSM.InsertMethod("Collider On", () =>
			{
				usingQuake = false;
			}, 2);
			shadeFSM.InsertMethod("Scream Recover", () =>
			{
				usingScream = false;
			}, 2);

			//Detect when player uses a spell
			spellFSM = HeroController.instance.spellControl;
			spellFSM.InsertMethod("Wallside?", () =>
			{
				if(usingFireball || usingQuake || usingScream) return;
				usingFireball = true;
				StartCoroutine(SpellControl());
			}, 0);
			spellFSM.InsertMethod("On Ground?", () =>
			{
				if(usingFireball || usingQuake || usingScream) return;
				usingQuake = true;
				StartCoroutine(SpellControl());
			}, 0);
			spellFSM.InsertMethod("Scream Get?", () =>
			{
				if(usingFireball || usingQuake || usingScream) return;
				usingScream = true;
				StartCoroutine(SpellControl());
			}, 0);
		}

		private IEnumerator SpellControl()
		{
			shadeFSM.SendEvent("RETREAT");
			yield return new WaitWhile(() => warping);
			if(usingFireball) shadeFSM.SetState("Cast Antic");
			if(usingQuake) shadeFSM.SetState("Quake Antic");
			if(usingScream) shadeFSM.SetState("Scream Antic");
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			Destroy(shadeGO);
			usingFireball = false;
			usingQuake = false;
			usingScream = false;
			warping = false;

			spellFSM.RemoveAction("Wallside?", 0);
			spellFSM.RemoveAction("On Ground?", 0);
			spellFSM.RemoveAction("Scream Get?", 0);
		}

		public override string ToString()
		{
			return "ChallengeMode_Past Regrets";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress",
				"ChallengeMode_Nail Only",
				"ChallengeMode_Nailmaster",
				"ChallengeMode_A Fool's Errand",
				"ChallengeMode_Unfriendly Fire",
				"ChallengeMode_Ascension"
			};
		}
	}
}