using System.Collections.Generic;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class UnfriendlyFire : Modifier
	{
		private PlayMakerFSM spawnFSM;
		private GameObject grimmchildGO;
		private PlayMakerFSM grimmchildFSM;
		private GameObject grimmballGO;
		private PlayMakerFSM grimmballFSM;

		public override void StartEffect()
		{
			spawnFSM = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
			if(spawnFSM.FsmVariables.FindFsmGameObject("Child").Value == null) spawnFSM.SetState("Spawn");

			grimmchildGO = spawnFSM.FsmVariables.FindFsmGameObject("Child").Value;
			grimmchildFSM = grimmchildGO.LocateMyFSM("Control");

			//Set level to 4
			grimmchildFSM.SendEvent("4");

			//Set damage to 0
			grimmchildFSM.GetAction<SetIntValue>("Level 4", 0).intValue = 0;

			//Increase time between attacks
			grimmchildFSM.GetAction<FloatSubtract>("Follow", 0).subtract = 0.4f;

			//Decrease flameball speed
			grimmchildFSM.FsmVariables.FindFsmFloat("Flameball Speed").Value = 10f;

			//Fly slower
			grimmchildFSM.GetAction<FloatClamp>("Follow", 10).minValue = 0.25f;
			grimmchildFSM.GetAction<FloatClamp>("Follow", 10).maxValue = 0.25f;

			//Stay farther away from player
			grimmchildFSM.GetAction<DistanceFlySmooth>("Follow", 11).targetRadius = 6f;

			//Make teleports less frequent
			grimmchildFSM.GetAction<FloatCompare>("Follow", 17).float2 = 25f;

			//Target player
			grimmchildFSM.InsertMethod("Check For Target", () =>
			{
				grimmchildFSM.FsmVariables.FindFsmGameObject("Target").Value = HeroController.instance.gameObject;
			}, 2);

			//Prevent shooting if player is stunned
			grimmchildFSM.InsertMethod("Check For Target", () =>
			{
				if(HeroController.instance.controlReqlinquished)
				{
					grimmchildFSM.SendEvent("NO TARGET");
				}
			}, 0);

			//Make grimmball do damage
			grimmchildFSM.InsertMethod("Shoot", () =>
			{
				grimmballGO = grimmchildFSM.FsmVariables.FindFsmGameObject("Flameball").Value;
				grimmballGO.layer = (int)PhysLayers.ENEMY_ATTACK;
				grimmballGO.AddComponent<DamageHero>();
				grimmballGO.GetComponent<DamageHero>().damageDealt = 1;
				grimmballGO.GetComponent<DamageHero>().hazardType = 1;

				//Prevent grimmball from doing daamge after impact
				grimmballFSM = grimmballGO.LocateMyFSM("Control");
				grimmballFSM.InsertMethod("Impact", () =>
				{
					grimmballGO.layer = (int)PhysLayers.CORPSE;
				}, 0);
			}, 10);
		}

		public override void StopEffect()
		{
			Destroy(grimmballGO);
			Destroy(grimmchildGO);
		}

		public override string ToString()
		{
			return "ChallengeMode_Unfriendly Fire";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Unfriendly Fire", "ChallengeMode_High Stress", "ChallengeMode_A Fool's Errand",
				"ChallengeMode_Ascension", "ChallengeMode_Past Regrets"
			};
		}
	}
}
