using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Keeps IMGUI screens within the platform safe area and readable on large displays.</summary>
    internal static class SafeGuiLayout
    {
        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;
        private const float MaximumScale = 1.5f;

        internal static Matrix4x4 Begin(out Rect logicalSafeArea)
        {
            Rect safeArea = Screen.safeArea;
            float scale = Mathf.Clamp(
                Mathf.Min(safeArea.width / ReferenceWidth, safeArea.height / ReferenceHeight),
                1f,
                MaximumScale);
            logicalSafeArea = new Rect(0f, 0f, safeArea.width / scale, safeArea.height / scale);
            Matrix4x4 previous = GUI.matrix;
            // Screen.safeArea uses a bottom-left origin; IMGUI uses a top-left origin.
            Vector3 topLeft = new Vector3(safeArea.xMin, Screen.height - safeArea.yMax, 0f);
            GUI.matrix = previous * Matrix4x4.TRS(topLeft, Quaternion.identity, new Vector3(scale, scale, 1f));
            return previous;
        }
    }
}
