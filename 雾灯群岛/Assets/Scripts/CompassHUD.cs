using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Minimal compass that keeps objective direction available without a full map overlay.</summary>
    public sealed class CompassHUD : MonoBehaviour
    {
        private Transform _player;
        private GuidanceMarker _guidance;
        private GUIStyle _cardinalStyle;
        private GUIStyle _targetStyle;
        private GUIStyle _behindStyle;

        private void Start()
        {
            _player = GameObject.Find("Explorer")?.transform;
            _guidance = GetComponent<GuidanceMarker>();
        }

        private void OnGUI()
        {
            if (StartScreen.IsOpen || PauseMenu.IsPaused || MapOverlay.IsOpen || QuestJournalPanel.IsOpen || DialoguePanel.IsOpen || _player == null) return;
            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            _cardinalStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(.82f, .9f, .9f) }
            };
            _targetStyle ??= new GUIStyle(_cardinalStyle)
            {
                fontSize = 18,
                normal = { textColor = new Color(1f, .72f, .24f) }
            };
            _behindStyle ??= new GUIStyle(_cardinalStyle)
            {
                fontSize = 14,
                normal = { textColor = new Color(1f, .78f, .34f) }
            };

            float width = Mathf.Min(430f, bounds.width - 80f);
            float left = (bounds.width - width) * .5f;
            float top = bounds.width < 1000f ? 192f : 20f;
            if (bounds.width >= 1000f && left < 590f)
            {
                left = 590f; // Leave a gap after the 558 px objective HUD at x=16.
                width = Mathf.Min(width, bounds.width - left - 16f);
            }
            GUI.Box(new Rect(left, top, width, 38f), string.Empty);
            float heading = Mathf.Repeat(_player.eulerAngles.y, 360f);
            DrawCardinal(left, top, width, 0f - heading, "N");
            DrawCardinal(left, top, width, 90f - heading, "E");
            DrawCardinal(left, top, width, 180f - heading, "S");
            DrawCardinal(left, top, width, 270f - heading, "W");

            if (_guidance?.Target == null) return;
            Vector3 toTarget = _guidance.Target.position - _player.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < .01f) return;
            float objectiveAngle = Vector3.SignedAngle(_player.forward, toTarget.normalized, Vector3.up);
            float markerX = left + width * .5f + Mathf.Clamp(objectiveAngle, -90f, 90f) * (width / 190f);
            GUI.Label(new Rect(markerX - 16f, top + 8f, 32f, 26f), "◆", _targetStyle);
            if (Mathf.Abs(objectiveAngle) > 100f)
            {
                bool insetBehind = bounds.width < 1000f || left + width + 164f > bounds.width;
                float behindX = insetBehind ? left + width - 160f : left + width + 4f;
                float behindY = insetBehind ? top + 42f : top + 8f;
                if (insetBehind) GUI.Box(new Rect(behindX - 4f, behindY, 168f, 24f), string.Empty);
                GUI.Label(new Rect(behindX, behindY, 160f, 24f),
                    objectiveAngle < 0f ? "目标在左后方" : "目标在右后方", _behindStyle);
            }
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawCardinal(float left, float top, float width, float angle, string label)
        {
            float normalized = Mathf.DeltaAngle(0f, angle);
            if (Mathf.Abs(normalized) > 100f) return;
            float x = left + width * .5f + normalized * (width / 200f);
            GUI.Label(new Rect(x - 16f, top + 10f, 32f, 22f), label, _cardinalStyle);
        }
    }
}
