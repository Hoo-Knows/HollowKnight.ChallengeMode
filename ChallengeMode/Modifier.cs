using UnityEngine;
using System.Collections.Generic;

namespace ChallengeMode
{
	public abstract class Modifier : MonoBehaviour
	{
		public abstract void StartEffect();

		public abstract void StopEffect();

		public abstract override string ToString();

		//Modifiers that are fundamentally incompatible
		public virtual List<string> GetCodeBlacklist() => new List<string>();

		//Modifiers that are too difficult/too easy when they appear together
		public virtual List<string> GetBalanceBlacklist() => new List<string>();
	}
}
