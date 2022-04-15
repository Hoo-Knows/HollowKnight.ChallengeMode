using UnityEngine;
using System.Collections.Generic;

namespace ChallengeMode
{
	public abstract class Modifier : MonoBehaviour
	{
		public abstract void StartEffect();

		public abstract void StopEffect();

		public abstract override string ToString();

		public virtual List<string> GetBlacklistedModifiers()
		{
			return new List<string>() { ToString() };
		}
	}
}
