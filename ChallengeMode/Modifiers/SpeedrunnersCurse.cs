using Modding;

namespace ChallengeMode.Modifiers
{
	class SpeedrunnersCurse : Modifier
	{
		public override void StartEffect()
		{
			ModCommon.ModCommon.OnSpellHook += OnSpellHook;
			ModHooks.Instance.GetPlayerBoolHook += GetPlayerBoolHook;
		}

		private bool OnSpellHook(ModCommon.ModCommon.Spell s)
		{
			if(s == ModCommon.ModCommon.Spell.Quake)
			{
				HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 2);
				return false;
			}
			return true;
		}

		private bool GetPlayerBoolHook(string target)
		{
			//Unequip sbody, equip thorns
			if(target == "equippedCharm_14") return false;
			if(target == "equippedCharm_12") return true;
			return PlayerData.instance.GetBoolInternal(target);
		}

		public override void StopEffect()
		{
			ModCommon.ModCommon.OnSpellHook -= OnSpellHook;
			ModHooks.Instance.GetPlayerBoolHook -= GetPlayerBoolHook;
		}

		public override string ToString()
		{
			return "ChallengeMode_Speedrunner's Curse";
		}
	}
}
