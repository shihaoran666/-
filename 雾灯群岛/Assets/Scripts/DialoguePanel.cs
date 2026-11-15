using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    /// <summary>Lightweight in-world dialogue presentation for vertical-slice NPCs.</summary>
    public sealed class DialoguePanel : MonoBehaviour
    {
        private static readonly Queue<string> Lines = new();
        private static string _speaker;
        private static int _canAdvanceAfterFrame;
        private GUIStyle _speakerStyle;
        private GUIStyle _lineStyle;

        public static bool IsOpen => Lines.Count > 0;
        internal static string CurrentLine => IsOpen ? Lines.Peek() : string.Empty;

        public static void Show(string speaker, params string[] lines)
        {
            Lines.Clear();
            _speaker = speaker;
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line)) Lines.Enqueue(line);
            }
            _canAdvanceAfterFrame = Time.frameCount + 1;
        }

        private void Update()
        {
            if (!IsOpen || PauseMenu.IsPaused) return;
            bool advance = (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) ||
                           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (advance && Time.frameCount >= _canAdvanceAfterFrame) Lines.Dequeue();
        }

        private void OnGUI()
        {
            if (!IsOpen || PauseMenu.IsPaused) return;
            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            _speakerStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, .74f, .32f) }
            };
            _lineStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            float width = Mathf.Min(760f, bounds.width - 80f);
            float left = (bounds.width - width) * .5f;
            float top = Mathf.Max(16f, bounds.height - 235f);
            GUI.Box(new Rect(left - 16f, top - 16f, width + 32f, 145f), string.Empty);
            GUI.Label(new Rect(left, top, width, 30f), _speaker, _speakerStyle);
            GUI.Label(new Rect(left, top + 34f, width, 72f), Lines.Peek(), _lineStyle);
            GUI.Label(new Rect(left, top + 108f, width, 24f), InputPromptMode.UsingGamepad ? "A 继续" : "E / Space 继续", _lineStyle);
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void OnDestroy()
        {
            Lines.Clear();
            _speaker = string.Empty;
        }
    }
}
