using Core.Utils.Extensions;

namespace Core.Utils {
	public class InputAcceptWindow {
		private AcceptWindow aw;
		private Input[] inputs;

		public InputAcceptWindow(AcceptWindow aw, Input[] inputs) {
			this.aw = aw;
			this.inputs = inputs;
		}

		public bool IsAccept(float time, Input input) {
			return aw.IsAccept(time) && inputs.Contains(input);
		}

		public enum Input {
			Run,
			Dash,
			Jump,
			ComboAttack,
			Skill
		}
	}
}