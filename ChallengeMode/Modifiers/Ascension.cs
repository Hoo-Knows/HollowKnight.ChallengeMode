using System.Collections.Generic;
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

		public override void StartEffect()
		{
			gorbGO = Instantiate(ChallengeMode.Preloads["GG_Ghost_Gorb"]["Warrior/Ghost Warrior Slug"]);
			gorbGO.transform.position = HeroController.instance.transform.position;
			GameObject.Destroy(gorbGO.GetComponent<DamageHero>());
			GameObject.Destroy(gorbGO.GetComponent<Collider2D>());
			gorbGO.SetActive(true);

			gorbMovementFSM = gorbGO.LocateMyFSM("Movement");
			gorbAttackFSM = gorbGO.LocateMyFSM("Attacking");
			gorbDistanceAttackFSM = gorbGO.LocateMyFSM("Distance Attack");

			//Make Gorb teleport out, wait, then teleport to player
			gorbMovementFSM.GetAction<Wait>("Warp Out", 2).time = 7.5f;
			gorbMovementFSM.RemoveAction("Return", 4);
			gorbMovementFSM.RemoveAction("Return", 0);
			gorbMovementFSM.InsertAction("Return", gorbMovementFSM.GetAction<SetPosition>("Warp", 1), 0);
			gorbMovementFSM.InsertMethod("Return", () =>
			{
				gorbMovementFSM.FsmVariables.FindFsmVector3("Warp Pos").Value = HeroController.instance.transform.position;
			}, 0);

			//Prevent movement FSM from leaving Return state
			gorbMovementFSM.RemoveTransition("Return", "FINISHED");

			//Reset isAttacking
			gorbAttackFSM.InsertMethod("Recover", () =>
			{
				gorbMovementFSM.SendEvent("RETURN");
			}, 0);

			//Set attack timer
			gorbAttackFSM.GetAction<WaitRandom>("Wait", 0).timeMin.Value = 0.75f;
			gorbAttackFSM.GetAction<WaitRandom>("Wait", 0).timeMax.Value = 0.75f;

			//Make attack FSM skip check for player
			gorbAttackFSM.ChangeTransition("Wait", "FINISHED", "Antic");

			//Wait until player has control
			gorbAttackFSM.InsertMethod("Antic", () =>
			{
				if(HeroController.instance.controlReqlinquished)
				{
					gorbAttackFSM.SendEvent("ATTACK OK");
				}
			}, 0);

			//Disable Gorb's distance attack
			gorbDistanceAttackFSM.RemoveTransition("Init", "FINISHED");
		}

		public override void StopEffect()
		{
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
				"ChallengeMode_Unfriendly Fire",
				"ChallengeMode_Salubra's Curse",
				"ChallengeMode_Speedrunner's Curse"
			};
		}
	}
}
