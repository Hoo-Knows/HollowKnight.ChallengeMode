using UnityEngine;

namespace ChallengeMode
{
	abstract class Modifier : MonoBehaviour
	{
		public abstract void StartEffect();

		public abstract void StopEffect();
	}
}
