#if UNITY_EDITOR
using UnityEditor;

namespace Verpha.HierarchyDesigner
{
    public static class HierarchyDesigner_Manager_Editor
    {
        #region Properties
        #region State
        public static bool IsPlaying { get; private set; }
        #endregion
        #region Theme
        public static bool IsProSkin { get; private set; }
        #endregion
        #endregion

        #region Initialization Methods
        public static void LoadCaches()
        {
            UpdateEditorThemeCache();
        }

        public static void Initialize()
        {
            SubscribeToEvents();
            UpdatePlayingState();
        }

        private static void SubscribeToEvents()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region State Methods
        private static void UpdatePlayingState()
        {
            IsPlaying = EditorApplication.isPlaying;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            UpdatePlayingState();
        }
        #endregion

        #region Theme Methods
        private static void UpdateEditorThemeCache()
        {
            IsProSkin = EditorGUIUtility.isProSkin;
        }
        #endregion
    }
}
#endif