using System;
using System.Collections.Generic;

namespace Core.Skills.Cooldowns {
    public class CooldownsCollection {
        private Dictionary<string, Cooldown> cooldownBySkillId = new Dictionary<string, Cooldown>();

        public void AddCooldown(string skillId, Cooldown cd) {
            if(skillId == null) throw new Exception("SkillId is null");
            if(cd == null) throw new Exception("Cooldown is null");

            cooldownBySkillId[skillId] = cd;
        }

        public bool TryGetCooldown(string skillId, ref Cooldown cd) {
            if (!cooldownBySkillId.ContainsKey(skillId)) return false;

            cd = cooldownBySkillId[skillId];
            return true;
        }
    }
}