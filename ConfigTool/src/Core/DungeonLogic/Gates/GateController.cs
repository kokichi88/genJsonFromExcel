using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;

namespace Core.DungeonLogic.Gates {
	public abstract class GateController {
		private bool sealed_;

		public void Open() {
			if (sealed_) {
				throw new Exception("Gate is sealed, cannot open");
			}

			OnOpen();
		}

		public void Close() {
			if (sealed_) {
				throw new Exception("Gate is sealed, cannot close");
			}

			OnClose();
		}

		public abstract bool IsOpened();

		public abstract bool IsClosed();

		public abstract bool IsOpening();

		public abstract bool IsClosing();

		public bool IsSealed() {
			return sealed_;
		}

		public void Seal() {
			sealed_ = true;
		}

		public abstract void Update(float dt);

		protected abstract void OnOpen();

		protected abstract void OnClose();
	}
}
