using Ssar.Combat.Skills.Events;

namespace Core.Skills.EventTriggers {
	public interface EventTrigger {
		void OnCreated(Skill s);
		void OnUpdate(float dt);
		void OnBeHit(Character byCaster);
		void OnProjectileLaunch(Skill bySkill);
		void OnEventFrameAdd(BaseEvent ef);
		void OnInterrupt();
	}
}