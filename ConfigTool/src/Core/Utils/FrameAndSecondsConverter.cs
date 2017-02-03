using System;

namespace Core.Utils {
	public class FrameAndSecondsConverter {
		private static FrameAndSecondsConverter _30FpsInstance = new FrameAndSecondsConverter(30);

		private int fps;

		public static FrameAndSecondsConverter _30Fps {
			get { return _30FpsInstance; }
		}

		public FrameAndSecondsConverter(int fps) {
			this.fps = fps;
		}

		public int SecondsToFrames(float seconds) {
			return (int) Math.Round(seconds * fps);
		}

		public float SecondsToFloatFrames(float seconds) {
			return (float) Math.Round(seconds * fps, 1);
		}

		public float FramesToSeconds(int frame) {
			return FloatFramesToSeconds(frame);
		}

		public float FloatFramesToSeconds(float frame) {
			return (float) Math.Round(frame / (float) fps, 3);
		}
	}
}