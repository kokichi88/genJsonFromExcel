#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;

namespace Core.Skills.Vfxs.Impacts {
	public class ImpactProxyConfigEditor : EditorWindow {
		public ImpactProxyConfig config;

		private EditorHelper.ScrollView.ScrollPosition scrollPosition = new EditorHelper.ScrollView.ScrollPosition();

		private void OnGUI() {
			if (config == null) return;

			config.editor = this;
			using (new EditorHelper.ScrollView(scrollPosition)) {
				config.OnGUI();
			}
		}
	}
}
#endif