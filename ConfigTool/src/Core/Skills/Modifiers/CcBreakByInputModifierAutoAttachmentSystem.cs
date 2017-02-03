using Artemis;
using Artemis.System;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Combat.Utils;
using Core.Skills.Modifiers.Info;
using Gameplay.DungeonLogic;
using RSG;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class CcBreakByInputModifierAutoAttachmentSystem : EntityProcessingSystem {
		private static Aspect aspect = Aspect.All(
			typeof(JustCreatedModifiersComponent), typeof(SkillComponent), typeof(StatsComponent),
			typeof(EquippedSkillsComponent)
		);

		private readonly Environment environment;

		private CcBreakByInputModifierConfig modifierConfig;
		private Promise creationPromise;
		private CollectionOfInteractions modifierInteractionCollection;
		private CcBreakByInputModifierEventConfig ccBreakEventConfig;

		public CcBreakByInputModifierAutoAttachmentSystem(Environment environment) : base(aspect) {
			this.environment = environment;

			creationPromise = new Promise();
			ResourcePreload resourcePreload = new ResourcePreload();
			resourcePreload.LoadModifierInteractionConfig()
				.Then(interactions => {
					modifierInteractionCollection = interactions;
					return resourcePreload.LoadCcBreakByInputEventConfig();
				})
				.Then(config => {
					ccBreakEventConfig = config;
					modifierConfig = config.ConfigObj;
					creationPromise.Resolve();
				});
		}

		public Promise CreationPromise => creationPromise;

		public override void Process(Entity entity) {
			if (!ccBreakEventConfig.enable) return;
			StatsComponent statsComponent = entity.GetComponent<StatsComponent>();
			if (statsComponent.BasicStatsFromConfig.ShowRole() != EntityRole.Hero) return;

			JustCreatedModifiersComponent justCreatedModifiers = entity.GetComponent<JustCreatedModifiersComponent>();
			bool found = false;
			Modifier triggerModifier = null;
			foreach (Modifier modifier in justCreatedModifiers.modifiers) {
				if (!CcBreakByInputModifier.interested.Contains(modifier.Type())) continue;

				triggerModifier = modifier;
				found = true;
				break;
			}

			if (!found) return;

			EquippedSkillsComponent equippedSkills = entity.GetComponent<EquippedSkillsComponent>();
			if (!equippedSkills.EquippedSkills.IsPassiveRecovery2Equipped()) return;

			SkillComponent skillComponent = entity.GetComponent<SkillComponent>();
			CcBreakByInputInfo info = new CcBreakByInputInfo(Target.Target, modifierConfig, null);
			Character character = skillComponent.Character;
			character.AddModifier(new CcBreakByInputModifier(
				info, entity, entity, triggerModifier, ccBreakEventConfig, environment, modifierInteractionCollection
			));
		}
	}
}