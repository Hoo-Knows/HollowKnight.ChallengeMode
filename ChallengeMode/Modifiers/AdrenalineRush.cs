﻿using System.Collections;
using Modding;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
	class AdrenalineRush : Modifier
	{
		private float customTimeScale;

		public override void StartEffect()
		{
			customTimeScale = 1f;

			ModHooks.TakeHealthHook += TakeHealthHook;
			On.GameManager.FreezeMoment_float_float_float_float += FreezeMoment;
		}

		private int TakeHealthHook(int damage)
		{
			customTimeScale = Mathf.Min(customTimeScale + 0.015f, 1.15f);
			return damage;
		}

		private IEnumerator FreezeMoment(On.GameManager.orig_FreezeMoment_float_float_float_float orig, GameManager self,
			float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		{
			yield return StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
			for(float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			yield return StartCoroutine(SetTimeScale(customTimeScale, rampUpTime));
			yield break;
		}

		public IEnumerator SetTimeScale(float newTimeScale, float duration)
		{
			float lastTimeScale = Time.timeScale;
			for(float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
			{
				float val = Mathf.Clamp01(timer / duration);
				SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
				yield return null;
			}
			SetTimeScale(newTimeScale);
			yield break;
		}

		private void SetTimeScale(float newTimeScale)
		{
			Time.timeScale = newTimeScale <= 0.01f ? 0f : newTimeScale;
		}

		public override void StopEffect()
		{
			customTimeScale = 1f;
			Time.timeScale = 1f;

			ModHooks.TakeHealthHook -= TakeHealthHook;
			On.GameManager.FreezeMoment_float_float_float_float -= FreezeMoment;
			StopAllCoroutines();
		}

		public override string ToString()
		{
			return "ChallengeMode_Adrenaline Rush";
		}
	}
}
