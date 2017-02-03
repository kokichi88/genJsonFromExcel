using System;
using UnityEngine;

namespace Core.Pooling {
	public class SimplePoolItem : MonoBehaviour {
		private event Action dynamicCleanupProcedures;
		private event Action defaultCleanupProcedures;

		public void AddCleanupProcedure(Action procedure) {
			dynamicCleanupProcedures += procedure;
		}

		public void AddDefaultCleanupProcedures(Action procedure) {
			defaultCleanupProcedures += procedure;
		}

		public void OnBeforeReturn() {
			if (defaultCleanupProcedures != null) {
				defaultCleanupProcedures();
			}

			if (dynamicCleanupProcedures != null) {
				dynamicCleanupProcedures();
			}

			dynamicCleanupProcedures = null;
		}
	}
}