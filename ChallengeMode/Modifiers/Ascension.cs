using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class Ascension : Modifier
	{
		private GameObject gorbGO;
		private PlayMakerFSM gorbMovementFSM;
		private PlayMakerFSM gorbAttackFSM;
		private PlayMakerFSM gorbDistanceAttackFSM;
		private bool isAttacking;

		public override void StartEffect()
		{
			gorbGO = Instantiate(ChallengeMode.Instance.preloadedObjects["GG_Ghost_Gorb"]["Warrior/Ghost Warrior Slug"]);
			gorbGO.transform.position = HeroController.instance.transform.position;
			Destroy(gorbGO.GetComponent<DamageHero>());
			Destroy(gorbGO.GetComponent<Collider2D>());
			gorbGO.SetActive(true);

			gorbMovementFSM = gorbGO.LocateMyFSM("Movement");
			gorbAttackFSM = gorbGO.LocateMyFSM("Attacking");
			gorbDistanceAttackFSM = gorbGO.LocateMyFSM("Distance Attack");
			isAttacking = false;

			//Make Gorb teleport out, wait, then teleport to player
			gorbMovementFSM.GetAction<Wait>("Warp Out", 2).time = 6f;
			gorbMovementFSM.RemoveAction("Return", 0);
			gorbMovementFSM.InsertAction("Return", gorbMovementFSM.GetAction<SetPosition>("Warp", 1), 0);
			gorbMovementFSM.InsertMethod("Return", () =>
			{
				gorbMovementFSM.FsmVariables.FindFsmVector3("Warp Pos").Value = HeroController.instance.transform.position;
			}, 0);

			//Make Gorb warp or attack when idle and player can move
			gorbMovementFSM.InsertMethod("Hover", () =>
			{
				if(!isAttacking || HeroController.instance.controlReqlinquished)
				{
					gorbMovementFSM.SendEvent("RETURN");
				}
				else
				{
					StartCoroutine(GorbAttack());
				}
			}, 0);

			//Set isAttacking after teleport
			gorbMovementFSM.InsertMethod("Return", () =>
			{
				isAttacking = true;
			}, 7);

			//Make sure Gorb is visible when attacking
			gorbAttackFSM.InsertAction("Antic", gorbMovementFSM.GetAction<SetMeshRenderer>("Return", 6), 1);

			//Reset isAttacking
			gorbAttackFSM.InsertMethod("End", () =>
			{
				isAttacking = false;
			}, 0);

			//Disable Gorb's distance attack
			gorbDistanceAttackFSM.InsertMethod("Away", () =>
			{
				gorbDistanceAttackFSM.SendEvent("CLOSE");
			}, 0);
		}

		private IEnumerator GorbAttack()
		{
			yield return new WaitForSeconds(0.3f);
			gorbAttackFSM.SetState("Antic");
			yield break;
		}

		public override void StopEffect()
		{
			StopAllCoroutines();

			Destroy(gorbGO);
		}

		public override string ToString()
		{
			return "ChallengeMode_Ascension";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress",
				"ChallengeMode_A Fool's Errand",
				"ChallengeMode_Past Regrets",
				"ChallengeMode_Unfriendly Fire"
			};
		}
	}
}
