﻿using ModCommon.Util;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using GlobalEnums;

namespace ChallengeMode.Modifiers
{
	class UnfriendlyFire : Modifier
	{
		private PlayMakerFSM spawnFSM;
		private GameObject grimmchildGO;
		private PlayMakerFSM grimmchildFSM;
		private GameObject grimmballGO;

		public override void StartEffect()
		{
			spawnFSM = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
			if(spawnFSM.FsmVariables.GetFsmGameObject("Child").Value == null) spawnFSM.SetState("Spawn");

			grimmchildGO = spawnFSM.FsmVariables.GetFsmGameObject("Child").Value;
			grimmchildFSM = grimmchildGO.LocateMyFSM("Control");

			//Set level to 4
			grimmchildFSM.SendEvent("4");

			//Set damage to 0
			grimmchildFSM.GetAction<SetIntValue>("Level 4", 0).intValue = 0;

			//Increase time between attacks
			grimmchildFSM.GetAction<FloatSubtract>("Follow", 0).subtract = 0.5f;

			//Decrease flameball speed
			grimmchildFSM.FsmVariables.FindFsmFloat("Flameball Speed").Value = 7.5f;

			//Stay farther away from player
			grimmchildFSM.GetAction<DistanceFlySmooth>("Follow", 11).targetRadius = 8f;

			//Target player
			grimmchildFSM.InsertMethod("Check For Target", 2, () =>
			{
				grimmchildFSM.FsmVariables.FindFsmGameObject("Target").Value = HeroController.instance.gameObject;
			});

			//Get flameball GameObject
			grimmchildFSM.InsertMethod("Shoot", 10, () =>
			{
				grimmballGO = grimmchildFSM.FsmVariables.GetFsmGameObject("Flameball").Value;
				grimmballGO.layer = (int) PhysLayers.ENEMY_ATTACK;
				grimmballGO.AddComponent<DamageHero>();
				grimmballGO.GetComponent<DamageHero>().damageDealt = 1;
				grimmballGO.GetComponent<DamageHero>().hazardType = 1;
			});
		}

		public override void StopEffect()
		{
			grimmchildFSM.Recycle();
			grimmballGO.Recycle();
			grimmchildGO.Recycle();
		}

		public override string ToString()
		{
			return "ChallengeMode_Unfriendly Fire";
		}
	}
}
