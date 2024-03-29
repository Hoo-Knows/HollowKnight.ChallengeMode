﻿using System.Collections.Generic;
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

		public override void StartEffect()
		{
			spawnFSM = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
			if(spawnFSM.FsmVariables.FindFsmGameObject("Child").Value == null) spawnFSM.SetState("Spawn");

			grimmchildGO = spawnFSM.FsmVariables.FindFsmGameObject("Child").Value;
			grimmchildFSM = grimmchildGO.LocateMyFSM("Control");

			//Set level to 4
			grimmchildFSM.SetState("4");

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
			grimmchildFSM.GetAction<FloatCompare>("Follow", 17).float2 = 10f;

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
				GameObject grimmballGO = grimmchildFSM.FsmVariables.FindFsmGameObject("Flameball").Value;
				grimmballGO.layer = (int)PhysLayers.ENEMY_ATTACK;
				grimmballGO.AddComponent<DamageHero>();
				grimmballGO.GetComponent<DamageHero>().damageDealt = 1;
				grimmballGO.GetComponent<DamageHero>().hazardType = 1;
				grimmballGO.GetComponent<Rigidbody2D>().gravityScale = 0f;

				//Prevent grimmball from doing damage after impact
				PlayMakerFSM grimmballFSM = grimmballGO.LocateMyFSM("Control");
				grimmballFSM.InsertAction("Impact", grimmballFSM.GetAction<SetCircleCollider>("Shrink", 3), 0);
			}, 10);
		}

		public override void StopEffect()
		{
			Destroy(grimmchildGO);
		}

		public override string ToString()
		{
			return "ChallengeMode_Unfriendly Fire";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_High Stress",
				"ChallengeMode_A Fool's Errand",
				"ChallengeMode_Ascension",
				"ChallengeMode_Salubra's Curse",
				"ChallengeMode_Past Regrets"
			};
		}
	}
}
