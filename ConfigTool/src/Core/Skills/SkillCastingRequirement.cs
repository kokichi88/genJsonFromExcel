using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Skills.Conditions;
using Core.Skills.Cooldowns;
using Core.Utils.Extensions;
using Ssar.Combat.Skills.Resources;
using UnityEngine;

namespace Core.Skills {
	public class SkillCastingRequirement {
		private Resource[] resources;
		private Condition[] conditions;

		public static SkillCastingRequirement None() {
			return new SkillCastingRequirement(new Resource[0], new Condition[0]);
		}

		public SkillCastingRequirement(Resource[] resources, Condition[] conditions) {
			if (resources == null) {
				throw new NullReferenceException("Cooldown param is null");
			}
			if (conditions == null) {
				throw new NullReferenceException("Conditions param is null");
			}

			this.resources = resources;
			this.conditions = conditions;
		}

		public void Update(float dt) {
			foreach (Condition condition in conditions) {
				condition.Update(dt);
			}

			for (int kIndex = 0; kIndex < resources.Length; kIndex++) {
				resources[kIndex].Update(dt);
			}
		}

		public bool IsCastable() {
			bool isAllConditionsMeet = IsAllConditionsMeet();

			bool areAllResourcesAvailable = true;
			for (int kIndex = 0; kIndex < resources.Length; kIndex++) {
				if (!resources[kIndex].IsAvailable()) {
					areAllResourcesAvailable = false;
					break;
				}
			}

			return areAllResourcesAvailable && isAllConditionsMeet;
		}

		public bool IsAllConditionsMeet() {
			bool isAllConditionsMeet = true;
			foreach (Condition condition in conditions) {
				if (!condition.IsMeet()) {
					isAllConditionsMeet = false;
					break;
				}
			}

			return isAllConditionsMeet;
		}

		public void Consume() {
			for (int kIndex = 0; kIndex < resources.Length; kIndex++) {
				resources[kIndex].Consume();
			}
		}

		public void Consume(params Resource.Name[] names) {
			for (int kIndex = 0; kIndex < resources.Length; kIndex++) {
				Resource r = resources[kIndex];
				if (!names.Contains(r.ShowName())) continue;

				r.Consume();
			}
		}

		public string Reasons() {
			for (int kIndex = 0; kIndex < resources.Length; kIndex++) {
				if (!resources[kIndex].IsAvailable()) {
					return resources[kIndex].ShowReasonForUnavailability();
				}
			}

			for (int i = 0; i < conditions.Length; i++) {
				if (!conditions[i].IsMeet()) {
					return conditions[i].Reason();
				}
			}

			return "Unknown";
		}

		public Resource[] Resources {
			get { return resources; }
		}

		public int GetCurrentCharge()
		{
			for (int i = 0; i <resources.Length; i++)
			{
				try
				{
					if (resources[i].GetType() == typeof(NonRecoverableChargeResource))
					{
						return (resources[i] as NonRecoverableChargeResource).GetCurrentCharge();
					}

					if (resources[i].GetType() == typeof(RecoverableChargeResource))
					{
						return (resources[i] as RecoverableChargeResource).GetCurrentCharge();					
					}
				}
				catch (Exception e)
				{
					
				}
				
			}
			return -1;
		}
		public float GetCurrentChargeRemainingPercentage()
		{
			for (int i = 0; i <resources.Length; i++)
			{
				try
				{

					if (resources[i].GetType() == typeof(RecoverableChargeResource))
					{
						return (resources[i] as RecoverableChargeResource).RemainingPercentage();
					}
				}
				catch (Exception e)
				{
					
				}
				
			}
			return -1;
		}
		public AetherResource GetAetherResource()
		{
			for (int i = 0; i < resources.Length; i++)
			{
				if (resources[i].GetType() == typeof(AetherResource))
				{
					return resources[i] as AetherResource;
				}
			}

			return null;
		}

		public int GetAetherThreshold()
		{
			AetherResource aetherResource = GetAetherResource();
			return aetherResource?.RequiredAether ?? -1;
		}

		public int GetCurrentAether()
		{
			AetherResource aetherResource = GetAetherResource();
			return aetherResource?.CurrentAether ?? 0;
		}

		public TimeCooldownResource GetTimeCooldownResource()
		{
			foreach (Resource resource in resources)
			{
				if (resource.GetType() == typeof(TimeCooldownResource))
				{
					return resource as TimeCooldownResource;
				}
			}
			return null;
		}

		public void GetCooldownInfo(out bool hasCooldown, out float remainingPercentage,out float duration)
		{
			hasCooldown = false;
			remainingPercentage = 0;
			duration = 0;
			for (int i = 0; i < resources.Length; i++)
			{
				if (resources[i].GetType()== typeof(RecoverableChargeResource))
				{
					RecoverableChargeResource recoverable = resources[i] as RecoverableChargeResource;
					hasCooldown = recoverable.Duration()>0;
					duration = recoverable.Duration();
					remainingPercentage = hasCooldown ? recoverable.RemainingPercentage(): 0;
					break;
				}

				if (resources[i].GetType()== typeof(NonRecoverableChargeResource))
				{
					hasCooldown = false;
					remainingPercentage = 0;
					duration = 0;
					break;
				}

				if (resources[i].GetType()==typeof(TimeCooldownResource))
				{
					TimeCooldownResource timeCooldown = resources[i] as TimeCooldownResource;
					hasCooldown = timeCooldown.Duration() > 0;
					remainingPercentage = hasCooldown ? timeCooldown.RemainingPercentage() : 0;
					duration = timeCooldown.Duration();
				}
			}
			
		}

		public void SetResources(Resource[] resources)
		{
			this.resources = resources;
		}
	}
}
