using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Teleports {
	public class TeleportAroundTeamMate : TeleportAroundTargetLogic {
		public TeleportAroundTeamMate(TeleportAction.AroundTargetMode info, Character caster,
		                              Environment environment, Skill skill) : base(info, caster, environment, skill) {
		}

		protected override Character FindTarget(Character caster, Environment environment) {
			return environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {FindingFilter.ExcludeEnemies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe}
			)[0];
		}
	}
}