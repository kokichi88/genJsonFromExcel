using Core.Skills.Modifiers;
using Ssar.Combat.Skills.Events;

namespace Core.Skills.Modifiers {
    public interface BuffFactory {
        bool Create(BaseEvent ef, Character caster, Character target, ref Modifier modifier);
    }
}