using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Sounds {
	public class AudioClipPlayback : Loopable {
		private readonly BaseEvent ef;
		private readonly Environment environment;

		private SoundAction soundAction;
		private bool isInterrupted;
		private AudioClip audioClip;
		private float elapsed;

		public AudioClipPlayback(BaseEvent ef, Environment environment) {
			this.ef = ef;
			this.environment = environment;
			soundAction = (SoundAction) ef.ShowAction();
			Play();
		}

		public void Update(float dt) {
			elapsed += dt;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			if (audioClip != null) {
				environment.StopSfx(audioClip);
			}
		}

		public bool IsFinished() {
			if (audioClip == null) return false;

			return elapsed >= audioClip.length;
		}

		private void Play() {
			environment.PlaySfx(
				soundAction.audioPath, soundAction.volume, soundAction.loop, soundAction.pitch,
				(path, audioClip) => {
					this.audioClip = audioClip;
					if (isInterrupted) {
						environment.StopSfx(audioClip);
					}
				}
			);
		}
	}
}