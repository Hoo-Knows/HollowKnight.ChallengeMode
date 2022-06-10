using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;

namespace ChallengeMode.Modifiers
{
	class ForgottenLightU : Modifier
	{
		private GameObject absradGO;
		private PlayMakerFSM absradAttackFSM;
		private PlayMakerFSM absradChoiceFSM;
		private Random random;

		private float beamsRotation;

		private List<GameObject> swordsCW;
		private List<GameObject> swordsCCW;
		private AudioClip swordsClip;

		private GameObject swordWall;

		private GameObject orbBeam;
		private AudioClip orbBeamAnticClip;
		private AudioClip orbBeamFireClip;

		private AudioSource audioSource;

		public override void StartEffect()
		{
			random = new Random();
			swordsCW = new List<GameObject>();
			swordsCCW = new List<GameObject>();

			StartCoroutine(WaitToActivate());
		}

		private IEnumerator WaitToActivate()
		{
			yield return new WaitUntil(() => GameObject.Find("Absolute Radiance") != null);
			absradGO = GameObject.Find("Absolute Radiance");
			absradAttackFSM = absradGO.LocateMyFSM("Attack Commands");
			absradChoiceFSM = absradGO.LocateMyFSM("Attack Choices");
			audioSource = absradGO.GetComponent<AudioSource>();

			ModifyEyeBeams();
			ModifySwordBurst();
			ModifySwordWall();
			ModifyOrbs();
			ModifyClimb();

			yield break;
		}

		private void ModifyEyeBeams()
		{
			//Pick random rotation direction
			absradAttackFSM.InsertMethod("NF Glow", () =>
			{
				if(absradChoiceFSM.FsmVariables.FindFsmInt("Arena").Value == 1) beamsRotation = 0.2f;
				else beamsRotation = 0.14f;
				beamsRotation *= random.Next(0, 2) == 0 ? -1f : 1f;
			}, 0);

			//Add rotation to eye beams
			string[] state = new string[] { "EB 1", "EB 2", "EB 3", "EB 7", "EB 8", "EB 9" };
			int[] index = new int[] { 6, 6, 6, 5, 5, 5 };
			int[] beamNum = new int[] { 1, 2, 3, 1, 2, 3 };
			for(int i = 0; i < 6; i++)
			{
				string stateLocal = state[i];
				int indexLocal = index[i];
				int beamNumLocal = beamNum[i];

				GameObject beams;
				if(i == 0 || i == 3)
				{
					beams = absradAttackFSM.GetAction<GetRotation>(stateLocal, indexLocal).gameObject.GameObject.Value;
				}
				else
				{
					beams = absradAttackFSM.GetAction<SetRotation>(stateLocal, indexLocal).gameObject.GameObject.Value;
				}
				absradAttackFSM.InsertMethod(stateLocal, () =>
				{
					StartCoroutine(EyeBeams(beams, beamNumLocal));
				}, indexLocal);
			}

			//Make eye beams last longer
			int[] audioIndex = new int[] { 2, 3, 3, 2, 2, 2 };
			int[] offset = new int[] { 0, 0, 0, 1, 1, 1 };
			for(int i = 0; i < 6; i++)
			{
				absradAttackFSM.GetAction<AudioPlayerOneShotSingle>(state[i], audioIndex[i]).delay = 0.5f;
				absradAttackFSM.GetAction<SendEventByName>(state[i], 9 - offset[i]).delay = 0.5f;
				absradAttackFSM.GetAction<SendEventByName>(state[i], 10 - offset[i]).delay = 2f;
				absradAttackFSM.GetAction<Wait>(state[i], 11 - offset[i]).time = 1.75f;
			}

			//Prevent last eye beam from disappearing early
			absradAttackFSM.GetAction<Wait>("EB 3", 11).time = 2f;
			absradAttackFSM.GetAction<Wait>("EB 9", 10).time = 2f;

			//Change light beam rotation to be consistently dodgeable
			state = new string[] { "EB 2", "EB 3", "EB 8", "EB 9" };
			for(int i = 0; i < 4; i++)
			{
				absradAttackFSM.GetAction<RandomFloat>(state[i], 0).min = 45f;
				absradAttackFSM.GetAction<RandomFloat>(state[i], 0).max = 45f;
			}
		}

		private IEnumerator EyeBeams(GameObject beams, int num)
		{
			for(int i = 0; i < 200; i++)
			{
				beams.transform.Rotate(0f, 0f, beamsRotation * (num % 2 == 0 ? 1f : -1f), Space.Self);
				yield return new WaitForSeconds(0.01f);
			}
			foreach(PlayMakerFSM fsm in beams.GetComponentsInChildren<PlayMakerFSM>())
			{
				fsm.SetState("End");
			}
			yield break;
		}

		private void ModifySwordBurst()
		{
			//Reset swords list
			absradAttackFSM.InsertMethod("Dir", () =>
			{
				swordsCW.Clear();
				swordsCCW.Clear();
			}, 0);

			//Always spawn clockwise because we only edit the clockwise FSM states
			absradAttackFSM.GetAction<SendRandomEvent>("Dir", 2).weights[1] = 0f;

			//Track swords
			absradAttackFSM.InsertMethod("CW Spawn", () =>
			{
				if(!absradAttackFSM.FsmVariables.FindFsmBool("Repeated").Value)
				{
					swordsCW.Add(absradAttackFSM.FsmVariables.FindFsmGameObject("Attack Obj").Value);
				}
				else
				{
					swordsCCW.Add(absradAttackFSM.FsmVariables.FindFsmGameObject("Attack Obj").Value);
				}
			}, 1);

			//Manually make swords fire
			swordsClip = absradAttackFSM.GetAction<AudioPlayerOneShotSingle>("CW Fire", 1).audioClip.Value as AudioClip;
			absradAttackFSM.RemoveAction("CW Fire", 1);
			absradAttackFSM.RemoveAction("CW Fire", 0);
			absradAttackFSM.InsertMethod("CW Fire", () =>
			{
				StartCoroutine(SwordBurst());
			}, 0);

			//Spawn second wave before firing
			absradAttackFSM.InsertMethod("CW Fire", () =>
			{
				if(!absradAttackFSM.FsmVariables.FindFsmBool("Repeated").Value)
				{
					absradAttackFSM.SetState("CW Restart");
					absradAttackFSM.FsmVariables.FindFsmBool("Repeated").Value = true;
				}
			}, 0);
		}

		private IEnumerator SwordBurst()
		{
			//FSM located in sharedassets407.assets
			yield return new WaitForSeconds(0.2f);
			audioSource.PlayOneShot(swordsClip);
			yield return new WaitForSeconds(0.3f);

			//Change sword speed and curve
			foreach(GameObject sword in swordsCW)
			{
				sword.LocateMyFSM("Control").GetAction<SetVelocityAsAngle>("Fire CW", 0).speed = 12f;
				sword.LocateMyFSM("Control").GetAction<FloatAdd>("Fire CW", 2).add = 30f;
				sword.LocateMyFSM("Control").SendEvent("FAN ATTACK CW");
			}
			foreach(GameObject sword in swordsCCW)
			{
				sword.LocateMyFSM("Control").GetAction<SetVelocityAsAngle>("Fire CCW", 0).speed = 12f;
				sword.LocateMyFSM("Control").GetAction<FloatAdd>("Fire CCW", 2).add = -30f;
				sword.LocateMyFSM("Control").SendEvent("FAN ATTACK CCW");
			}
			yield break;
		}

		private void ModifySwordWall()
		{
			//Find swords and change ease type
			string[] state = new string[] { "Comb Top", "Comb L", "Comb R", "Comb Top 2", "Comb L 2", "Comb R 2" };
			for(int i = 0; i < 6; i++)
			{
				string stateLocal = state[i];
				absradAttackFSM.InsertMethod(stateLocal, () =>
				{
					swordWall = absradAttackFSM.FsmVariables.FindFsmGameObject("Attack Obj").Value;
					//FSM located in sharedassets407.assets
					swordWall.LocateMyFSM("Control").GetAction<iTweenMoveBy>("Tween", 0).easeType = iTween.EaseType.easeInOutQuad;
					swordWall.LocateMyFSM("Control").GetAction<iTweenMoveBy>("Tween", 0).speed = 16f;
				}, 3);
			}
		}

		private void ModifyOrbs()
		{
			//Find orbs and edit FSM
			absradAttackFSM.InsertMethod("Spawn Fireball", () =>
			{
				GameObject orb = absradAttackFSM.FsmVariables.FindFsmGameObject("Projectile").Value;
				orbBeam = absradAttackFSM.FsmVariables.FindFsmGameObject("Ascend Beam").Value;
				//FSM located in sharedassets407.assets
				string[] fsm = new string[] { "Orb Control", "Orb Control", "Final Control" };
				string[] state = new string[] { "Impact", "Stop Particles", "Recycle" };
				for(int i = 0; i < 3; i++)
				{
					string fsmLocal = fsm[i];
					string stateLocal = state[i];
					orb.LocateMyFSM(fsmLocal).InsertMethod(stateLocal, () =>
					{
						StartCoroutine(Orbs(orb.transform.position));
					}, 0);
				}
			}, 2);

			//Get audio clips
			orbBeamAnticClip = absradAttackFSM.GetAction<AudioPlayerOneShotSingle>("Aim", 1).audioClip.Value as AudioClip;
			orbBeamFireClip = absradAttackFSM.GetAction<AudioPlayerOneShotSingle>("Aim", 3).audioClip.Value as AudioClip;
		}

		private IEnumerator Orbs(Vector3 pos)
		{
			//Create beam
			GameObject beam = Instantiate(orbBeam, pos, Quaternion.identity);
			beam.transform.Rotate(0f, 0f, -90f, Space.Self);
			beam.transform.position += new Vector3(0f, 20f, 0f);
			PlayMakerFSM beamFSM = beam.LocateMyFSM("Control");
			beam.SetActive(true);

			//Fire beam
			beamFSM.SendEvent("ANTIC");
			AudioSource.PlayClipAtPoint(orbBeamAnticClip, pos, 1f);
			yield return new WaitForSeconds(0.5f);
			beamFSM.SendEvent("FIRE");
			AudioSource.PlayClipAtPoint(orbBeamFireClip, pos, 1f);
			yield return new WaitForSeconds(0.5f);
			beamFSM.SendEvent("END");
			yield return new WaitForSeconds(1f);
			Destroy(beam);
			yield break;
		}

		private void ModifyClimb()
		{
			//Make accuracy perfect
			absradAttackFSM.GetAction<RandomFloat>("Aim", 4).min = 0f;
			absradAttackFSM.GetAction<RandomFloat>("Aim", 4).max = 0f;

			//Decrease fire delay
			absradAttackFSM.GetAction<SendEventByName>("Aim", 9).delay = 0.25f;
			absradAttackFSM.GetAction<AudioPlayerOneShotSingle>("Aim", 3).delay = 0.25f;
		}

		public override void StopEffect() { }

		public override string ToString()
		{
			return "ChallengeMode_Forgotten Light";
		}

		public override List<string> GetBalanceBlacklist()
		{
			return new List<string>()
			{
				"ChallengeMode_Nail Only",
				"ChallengeMode_Speedrunner's Curse"
			};
		}
	}
}
