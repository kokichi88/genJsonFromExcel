#if UNITY_EDITOR

using UnityEditor;
using Utils.Editor;

namespace Core.Skills.Vfxs.DropItem
{
    public class DropItemConfigEditor : EditorWindow
    {
        public DropItemConfig config;
        
        private EditorHelper.ScrollView.ScrollPosition scrollPosition = new EditorHelper.ScrollView.ScrollPosition();

        private void OnGUI() {
            if (config == null) return;

            // config.editor = this;
            using (new EditorHelper.ScrollView(scrollPosition)) {
                config.OnGUI();
            }
        }
    }
}
#endif