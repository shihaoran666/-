#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FoglightIsles.Editor
{
    /// <summary>Ensures the Input System package used by the prototype is active for desktop builds.</summary>
    [InitializeOnLoad]
    public static class InputSystemProjectSetup
    {
        private const string ActiveInputHandler = "activeInputHandler";
        private const int InputSystemPackage = 1;
        private static bool _scheduled;

        static InputSystemProjectSetup()
        {
            if (_scheduled) return;
            _scheduled = true;
            EditorApplication.delayCall += EnableInputSystem;
        }

        private static void EnableInputSystem()
        {
            int activeHandler = PlayerSettings.GetPropertyInt(ActiveInputHandler, BuildTargetGroup.Standalone);
            if (activeHandler == InputSystemPackage || activeHandler == 2) return;

            PlayerSettings.SetPropertyInt(ActiveInputHandler, InputSystemPackage, BuildTargetGroup.Standalone);
            AssetDatabase.SaveAssets();
            Debug.Log("[FoglightIsles Setup] Enabled the Input System package for Standalone. Unity may ask to restart the editor.");
        }
    }
}
#endif
