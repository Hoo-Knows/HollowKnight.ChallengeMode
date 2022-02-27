using ModCommon.Util;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections;

namespace ChallengeMode.Modifiers
{
	class PastRegrets : Modifier
	{
		private GameObject shadeGO;
		private PlayMakerFSM shadeFSM;
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

			//Remove spell limit, increase hp, and set spell levels to max
			shadeFSM.InsertMethod("Init", 25, () =>
			{
				shadeFSM.FsmVariables.FindFsmInt("SP").Value = int.MaxValue;
				shadeGO.GetComponent<HealthManager>().hp = int.MaxValue;
				shadeFSM.FsmVariables.FindFsmInt("Fireball Level").Value = 2;
				shadeFSM.FsmVariables.FindFsmInt("Quake Level").Value = 2;
				shadeFSM.FsmVariables.FindFsmInt("Scream Level").Value = 2;

			});

			//Remove roam limit
			shadeFSM.FsmVariables.FindFsmFloat("Max Roam").Value = 999f;

			//Make shade unfriendly
			shadeFSM.RemoveAction("Friendly?", 2);

			//Decrease frequency of random attacks and make them all slashes
			shadeFSM.GetAction<WaitRandom>("Fly", 5).timeMin = 5f;
			shadeFSM.GetAction<WaitRandom>("Fly", 5).timeMax = 6f;
			shadeFSM.InsertMethod("Quake?", 0, () =>
			{
				shadeFSM.SetState("Slash Antic");
			});

			//Decrease flight speed
			shadeFSM.GetAction<ChaseObject>("Fly", 4).speedMax = 2f;
			shadeFSM.GetAction<ChaseObjectV2>("Fly", 6).speedMax = 2f;

			//Teleport to different location depending on spell
			shadeFSM.InsertMethod("Retreat Start", 0, () =>
			{
				warping = true;
			});
			shadeFSM.InsertMethod("Retreat", 1, () =>
			{
				Vector3 position = HeroController.instance.transform.position;
				if(usingFireball)
				{
					if(HeroController.instance.cState.facingRight) position += 3 * Vector3.left;
					else position += 3 * Vector3.right;
				}
				else if(usingQuake)
				{
					position += 3 * Vector3.up;
				}
				else if(usingScream)
				{
					position += 3 * Vector3.down;
				}
				shadeFSM.FsmVariables.FindFsmVector3("Start Pos").Value = position;
			});
			shadeFSM.InsertMethod("Retreat Reset", 3, () =>
			{
				warping = false;
			});

			//Make shade face player before using fireball
			shadeFSM.InsertAction("Cast Antic", shadeFSM.GetAction<FaceObject>("Fireball Pos", 3), 0);

			//Increase warp speed
			shadeFSM.GetAction<iTweenMoveTo>("Retreat", 2).time = 0.5f;

			//Increase attack speed
			shadeFSM.GetAction<Wait>("Cast Antic", 8).time = 0.2f;
			shadeFSM.GetAction<Wait>("Quake Antic", 7).time = 0.2f;
			shadeFSM.GetAction<Wait>("Scream Antic", 6).time = 0.2f;

			//Reset variables after using a spell
			shadeFSM.InsertMethod("Cast", 4, () =>
			{
				usingFireball = false;
			});
			shadeFSM.InsertMethod("Collider On", 2, () =>
			{
				usingQuake = false;
			});
			shadeFSM.InsertMethod("Scream Recover", 2, () =>
			{
				usingScream = false;
			});

			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			if(shadeFSM.ActiveStateName == "Fly")
			{
				if(s == ModCommon.ModCommon.Spell.Fireball) usingFireball = true;
				if(s == ModCommon.ModCommon.Spell.Quake) usingQuake = true;
				if(s == ModCommon.ModCommon.Spell.Scream) usingScream = true;
				StartCoroutine(SpellControl());
			}
			return true;
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
			shadeFSM.Recycle();
			shadeGO.Recycle();
			usingFireball = false;
			usingQuake = false;
			usingScream = false;
			warping = false;

			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Past Regrets";
		}
	}
}
