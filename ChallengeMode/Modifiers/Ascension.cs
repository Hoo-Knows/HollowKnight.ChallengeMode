using UnityEngine;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;

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
			gorbGO.layer = (int)PhysLayers.CORPSE;
			gorbGO.SetActive(true);

			gorbMovementFSM = gorbGO.LocateMyFSM("Movement");
			gorbAttackFSM = gorbGO.LocateMyFSM("Attacking");
			gorbDistanceAttackFSM = gorbGO.LocateMyFSM("Distance Attack");
			isAttacking = false;

			//Make Gorb teleport out, wait, then teleport to player
			gorbMovementFSM.GetAction<Wait>("Warp Out", 2).time = 5f;
			gorbMovementFSM.RemoveAction("Return", 0);
			gorbMovementFSM.InsertAction("Return", gorbMovementFSM.GetAction<SetPosition>("Warp", 1), 0);
			gorbMovementFSM.InsertMethod("Return", () =>
			{
				gorbMovementFSM.FsmVariables.FindFsmVector3("Warp Pos").Value = HeroController.instance.transform.position;
			}, 0);

			//Make Gorb warp or attack when idle
			gorbMovementFSM.InsertMethod("Hover", () =>
			{
				if(!isAttacking) gorbMovementFSM.SendEvent("RETURN");
				else gorbAttackFSM.SetState("Antic");
			}, 0);

			//Set isAttacking after teleport
			gorbMovementFSM.InsertMethod("Return", () =>
			{
				isAttacking = true;
			}, 7);

			//Prevent attacking if something else is happening
			gorbAttackFSM.InsertMethod("Antic", () =>
			{
				if(!isAttacking) gorbAttackFSM.SetState("End");
			}, 0);

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

		public override void StopEffect()
		{
			gorbMovementFSM.Recycle();
			gorbAttackFSM.Recycle();
			gorbDistanceAttackFSM.Recycle();
			gorbGO.Recycle();
		}

		public override string ToString()
		{
			return "ChallengeMode_Ascension";
		}
	}
}
