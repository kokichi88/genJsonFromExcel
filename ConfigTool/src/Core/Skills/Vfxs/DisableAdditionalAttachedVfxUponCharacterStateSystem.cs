using System.Collections.Generic;
using Artemis;
using Artemis.System;
using Core.Skills.Modifiers;
using Ssar.Combat.Skills;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class DisableAdditionalAttachedVfxUponCharacterStateSystem : EntityProcessingSystem {
		private static Aspect aspect = Aspect.All(
			typeof(AdditionalAttachedVfxComponent), typeof(SkillComponent),
			typeof(JustCreatedModifiersComponent), typeof(CharacterStateTransitionComponent)
		);

		public DisableAdditionalAttachedVfxUponCharacterStateSystem() : base(aspect) {
		}

		public override void Process(Entity entity) {
			AdditionalAttachedVfxComponent aavc = entity.GetComponent<AdditionalAttachedVfxComponent>();
			JustCreatedModifiersComponent justCreatedModifiers = entity.GetComponent<JustCreatedModifiersComponent>();
			foreach (Modifier modifier in justCreatedModifiers.modifiers) {
				if (aavc.interestedModifierTypes.Contains(modifier.Type())) {
					foreach (GameObject gameObject in aavc.vfxs) {
						gameObject.SetActive(false);
					}
					break;
				}
			}

			CharacterStateTransitionComponent characterStateTransition =
				entity.GetComponent<CharacterStateTransitionComponent>();
			if (characterStateTransition.justChanged) {
				if (characterStateTransition.from != Character.CharacterState.Default) {
					if (characterStateTransition.to == Character.CharacterState.Default) {
						foreach (GameObject gameObject in aavc.vfxs) {
							gameObject.SetActive(true);
						}
					}
				}
			}
		}
	}
}