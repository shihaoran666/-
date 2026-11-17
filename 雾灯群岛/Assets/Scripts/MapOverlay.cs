using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    /// <summary>Lightweight schematic map that keeps the exploratory journey readable without revealing every collectible.</summary>
    public sealed class MapOverlay : MonoBehaviour
    {
        private PlayerController _player;
        private BoatController _boat;
        private GuidanceMarker _guidance;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _marker;
        private Texture2D _islandMask;

        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerController>();
            _boat = FindFirstObjectByType<BoatController>();
            _guidance = GetComponent<GuidanceMarker>();
        }

        private void Update()
        {
            if (StartScreen.IsOpen || PauseMenu.IsPaused || DialoguePanel.IsOpen || QuestJournalPanel.IsOpen) return;
            bool toggle = (Keyboard.current != null && (Keyboard.current.mKey.wasPressedThisFrame ||
                                                        (IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame))) ||
                          (Gamepad.current != null && (Gamepad.current.selectButton.wasPressedThisFrame ||
                                                        (IsOpen && Gamepad.current.buttonEast.wasPressedThisFrame)));
            if (toggle) SetOpen(!IsOpen);
        }

        private void OnGUI()
        {
            if (!IsOpen) return;
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, .74f, .32f) }
            };
            _body ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            _marker ??= new GUIStyle(_body)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, .76f, .26f) }
            };
            _islandMask ??= CreateIslandMask();

            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            float scale = Mathf.Min(1f, bounds.width / 720f, bounds.height / 600f);
            _body.fontSize = Mathf.Max(15, Mathf.CeilToInt(12f / scale));
            GUI.matrix *= Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            const float width = 680f;
            const float height = 560f;
            float left = (bounds.width / scale - width) * .5f;
            float top = (bounds.height / scale - height) * .5f;
            var map = new Rect(left + 34f, top + 75f, width - 68f, height - 145f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, .58f);
            GUI.DrawTexture(new Rect(0f, 0f, bounds.width / scale, bounds.height / scale), Texture2D.whiteTexture);
            GUI.color = new Color(.04f, .07f, .09f, .94f);
            GUI.DrawTexture(new Rect(left, top, width, height), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Box(new Rect(left, top, width, height), string.Empty);
            GUI.Label(new Rect(left, top + 16f, width, 42f), "航海图 · 晨湾海域", _title);
            if (GUI.Button(new Rect(left + width - 86f, top + 20f, 62f, 30f), "关闭")) SetOpen(false);
            GUI.Box(map, string.Empty);
            DrawTinted(new Rect(map.x + 2f, map.y + 2f, map.width - 4f, map.height - 4f), Texture2D.whiteTexture, new Color(.05f, .18f, .24f));
            GUI.Label(new Rect(map.xMax - 58f, map.y + 10f, 50f, 26f), "N ↑", _body);
            Vector2 harbour = ToMap(map, new Vector3(0f, 0f, 12f));
            Vector2 tideflats = ToMap(map, new Vector3(0f, 0f, -82f));
            DrawTinted(new Rect(harbour.x - 105f, harbour.y - 45f, 210f, 90f), _islandMask, new Color(.28f, .38f, .33f));
            DrawTinted(new Rect(tideflats.x - 95f, tideflats.y - 52f, 190f, 104f), _islandMask, new Color(.33f, .39f, .32f));
            for (float y = harbour.y + 57f; y < tideflats.y - 58f; y += 22f)
                DrawTinted(new Rect(harbour.x - 2f, y, 4f, 11f), Texture2D.whiteTexture, new Color(.9f, .72f, .36f, .7f));
            DrawPlace(map, new Vector3(0f, 0f, 12f), "晨湾港");
            DrawPlace(map, new Vector3(0f, 0f, -82f), "潮汐浅滩");
            DrawMarker(map, _player != null ? _player.transform.position : Vector3.zero, "●", new Color(.4f, .85f, 1f));
            if (_boat != null) DrawMarker(map, _boat.transform.position, "▲", new Color(.92f, .9f, .76f));
            if (_guidance != null && _guidance.Target != null) DrawMarker(map, _guidance.Target.position, "◆", new Color(1f, .7f, .2f));
            GUI.Label(new Rect(left + 30f, top + height - 52f, width - 60f, 26f),
                _guidance?.Target != null ? "● 你的位置   ▲ 晨鹱号   ◆ 当前目标" : "● 你的位置   ▲ 晨鹱号   自由探索中", _body);
            GUI.Label(new Rect(left + 30f, top + height - 28f, width - 60f, 22f), "M / View、Esc / 手柄 B 关闭地图", _body);
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void SetOpen(bool open)
        {
            IsOpen = open;
            Time.timeScale = open ? 0f : 1f;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        internal void OpenForReview() => SetOpen(true);
        internal void CloseForPause() { if (IsOpen) SetOpen(false); }

        private void DrawPlace(Rect map, Vector3 position, string label)
        {
            Vector2 point = ToMap(map, position);
            GUI.Label(new Rect(point.x + 46f, point.y - 12f, 140f, 24f), label, _body);
        }

        private static void DrawTinted(Rect rect, Texture texture, Color tint)
        {
            Color previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect, texture);
            GUI.color = previous;
        }

        private static Texture2D CreateIslandMask()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + .5f - size * .5f) / (size * .5f);
                float dy = (y + .5f - size * .5f) / (size * .5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((1f - dx * dx - dy * dy) * 7f));
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void DrawMarker(Rect map, Vector3 position, string symbol, Color color)
        {
            Vector2 point = ToMap(map, position);
            Color previous = _marker.normal.textColor;
            _marker.normal.textColor = color;
            GUI.Label(new Rect(point.x - 14f, point.y - 14f, 28f, 28f), symbol, _marker);
            _marker.normal.textColor = previous;
        }

        private static Vector2 ToMap(Rect map, Vector3 world)
        {
            float x = Mathf.InverseLerp(-25f, 25f, world.x);
            float y = 1f - Mathf.InverseLerp(-110f, 28f, world.z);
            return new Vector2(Mathf.Lerp(map.xMin + 14f, map.xMax - 14f, x), Mathf.Lerp(map.yMin + 14f, map.yMax - 14f, y));
        }

        private void OnDestroy()
        {
            if (IsOpen) Time.timeScale = 1f;
            IsOpen = false;
            if (_islandMask != null) Destroy(_islandMask);
        }
    }
}
