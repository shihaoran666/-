using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Nearby NPC identity in the world, without a quest target arrow.</summary>
    public sealed class NpcNameplate : MonoBehaviour
    {
        private const float MaxDistance = 11f;
        private readonly RaycastHit[] _occlusionHits = new RaycastHit[16];
        private string _label;
        private Transform _player;
        private Camera _camera;
        private GUIStyle _style;

        public void Configure(string label) => _label = label;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>()?.transform;
            _camera = Camera.main;
        }

        private void OnGUI()
        {
            if (_player == null || _camera == null || StartScreen.IsOpen || PauseMenu.IsPaused ||
                DialoguePanel.IsOpen || MapOverlay.IsOpen || QuestJournalPanel.IsOpen) return;

            Vector3 playerOffset = transform.position - _player.position;
            playerOffset.y = 0f;
            float distance = playerOffset.magnitude;
            if (distance < 2.3f || distance > MaxDistance) return;

            Vector3 anchor = transform.position + Vector3.up * 1.55f;
            Vector3 toAnchor = anchor - _camera.transform.position;
            int count = Physics.RaycastNonAlloc(_camera.transform.position, toAnchor.normalized,
                _occlusionHits, toAnchor.magnitude, ~0, QueryTriggerInteraction.Ignore);
            for (int index = 0; index < count; index++)
            {
                Transform hit = _occlusionHits[index].collider.transform;
                if (!hit.IsChildOf(transform) && !hit.IsChildOf(_player) &&
                    _occlusionHits[index].distance < toAnchor.magnitude - .2f) return;
            }

            Vector3 screen = _camera.WorldToScreenPoint(anchor);
            if (screen.z <= 0f) return;
            float left = screen.x - 78f;
            float top = Screen.height - screen.y - 36f;
            bool compact = Screen.width < 1000f;
            if (compact)
            {
                top = Mathf.Max(top, 234f); // Below the compact HUD and compass.
                float centre = Screen.width * .5f;
                if (left < centre + 50f && left + 156f > centre - 50f)
                    left = screen.x >= centre ? screen.x + 40f : screen.x - 196f;
            }
            float bottomMargin = compact ? 70f : 110f;
            if (left < 6f || left + 156f > Screen.width - 6f || top < 186f ||
                top + 30f > Screen.height - bottomMargin) return;

            _style ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Box(new Rect(left, top, 156f, 30f), _label, _style);
        }
    }
}
