using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FoglightIsles
{
    /// <summary>Prototype pause and accessibility settings panel, usable from keyboard and gamepad.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        private const string VolumeKey = "foglightisles.settings.volume.v1";
        private const string FovKey = "foglightisles.settings.fov.v1";
        private const string LookSensitivityKey = "foglightisles.settings.look-sensitivity.v1";
        private const string FullscreenKey = "foglightisles.settings.fullscreen.v1";
        private const string WindowWidthKey = "foglightisles.settings.window-width.v1";
        private const string WindowHeightKey = "foglightisles.settings.window-height.v1";
        private const string VSyncKey = "foglightisles.settings.vsync.v1";
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _selectionStyle;
        private bool _paused;
        private float _volume;
        private float _fieldOfView;
        private float _lookSensitivity;
        private bool _confirmClear;
        private bool _fullscreen;
        private bool _vSync;
        private int _windowWidth;
        private int _windowHeight;
        private int _resolutionIndex;
        private int _selectedOption;
        private bool _gamepadNavigation;

        private const int OptionCount = 9;

        private static readonly Vector2Int[] ResolutionSizes =
        {
            new Vector2Int(1280, 720), new Vector2Int(1366, 768), new Vector2Int(1440, 900), new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080), new Vector2Int(1920, 1200), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160)
        };

        public static bool IsPaused { get; private set; }

        private static bool IsAutomationRun
        {
            get
            {
                foreach (string argument in Environment.GetCommandLineArgs())
                    if (argument.StartsWith("-verify-", StringComparison.Ordinal) ||
                        argument.StartsWith("-capture-", StringComparison.Ordinal)) return true;
                return false;
            }
        }

        private void Awake()
        {
            _volume = PlayerPrefs.GetFloat(ProgressStore.StorageKey(VolumeKey), 0.8f);
            _fieldOfView = PlayerPrefs.GetFloat(ProgressStore.StorageKey(FovKey), 65f);
            _lookSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(ProgressStore.StorageKey(LookSensitivityKey), 1f), .5f, 2f);
            _fullscreen = PlayerPrefs.HasKey(ProgressStore.StorageKey(FullscreenKey))
                ? PlayerPrefs.GetInt(ProgressStore.StorageKey(FullscreenKey)) == 1
                : Screen.fullScreenMode != FullScreenMode.Windowed;
            _windowWidth = PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowWidthKey), 1280);
            _windowHeight = PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowHeightKey), 720);
            _resolutionIndex = NearestPresetIndex(_windowWidth, _windowHeight);
            _vSync = PlayerPrefs.GetInt(ProgressStore.StorageKey(VSyncKey), 0) == 1;
            ApplyAudioAndCameraSettings();
            ApplyFramePacing();
            if (PlayerPrefs.HasKey(ProgressStore.StorageKey(FullscreenKey))) ApplyDisplayMode();
        }

        private void Update()
        {
            if (StartScreen.IsOpen || MapOverlay.IsOpen || QuestJournalPanel.IsOpen) return;
            bool toggle = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                          (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
            if (toggle)
            {
                SetPaused(!_paused);
                return;
            }
            if (!_paused || Gamepad.current == null) return;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) _gamepadNavigation = false;
            Gamepad gamepad = Gamepad.current;
            if (gamepad.dpad.up.wasPressedThisFrame) MoveSelection(-1);
            if (gamepad.dpad.down.wasPressedThisFrame) MoveSelection(1);
            if (gamepad.dpad.left.wasPressedThisFrame) AdjustSelection(-1);
            if (gamepad.dpad.right.wasPressedThisFrame) AdjustSelection(1);
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                if (_confirmClear) CancelClear();
                else SetPaused(false);
            }
            else if (gamepad.buttonSouth.wasPressedThisFrame) ActivateSelection();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) PauseForFocusLoss(false);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) PauseForFocusLoss(false);
        }

        private void PauseForFocusLoss(bool force)
        {
            if ((!force && IsAutomationRun) || StartScreen.IsOpen || _paused || MapOverlay.IsOpen || QuestJournalPanel.IsOpen) return;
            SetPaused(true);
            PrototypeHUD.SetMessage("游戏已暂停：窗口失去焦点。按 Esc / Menu 继续。");
        }

        internal void PauseForFocusLossForValidation() => PauseForFocusLoss(true);

        private void SetPaused(bool paused)
        {
            _paused = paused;
            IsPaused = paused;
            if (paused)
            {
                _selectedOption = 0;
                _confirmClear = false;
                _gamepadNavigation = Gamepad.current != null;
            }
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        internal void OpenForReview() => SetPaused(true);
        internal void ToggleDisplayModeForReview() => ToggleDisplayMode();

        private void MoveSelection(int direction)
        {
            _gamepadNavigation = true;
            int count = _confirmClear ? 2 : OptionCount;
            _selectedOption = (_selectedOption + direction + count) % count;
        }

        private void AdjustSelection(int direction)
        {
            _gamepadNavigation = true;
            if (_confirmClear) return;
            if (_selectedOption == 1) _volume = Mathf.Clamp01(_volume + direction * .05f);
            else if (_selectedOption == 2) _fieldOfView = Mathf.Clamp(_fieldOfView + direction * 2f, 55f, 85f);
            else if (_selectedOption == 3) _lookSensitivity = Mathf.Clamp(_lookSensitivity + direction * .1f, .5f, 2f);
            else if (_selectedOption == 4)
            {
                ToggleDisplayMode();
                return;
            }
            else if (_selectedOption == 5)
            {
                CycleResolution(direction);
                return;
            }
            else if (_selectedOption == 6)
            {
                ToggleFramePacing();
                return;
            }
            else return;
            ApplyAudioAndCameraSettings();
            SaveSettings();
        }

        private void CycleResolution(int direction)
        {
            _resolutionIndex = (_resolutionIndex + direction + ResolutionSizes.Length) % ResolutionSizes.Length;
            _windowWidth = ResolutionSizes[_resolutionIndex].x;
            _windowHeight = ResolutionSizes[_resolutionIndex].y;
            if (!_fullscreen) ApplyDisplayMode();
            SaveSettings();
        }

        private string ResolutionLabel => $"{_windowWidth} × {_windowHeight}";

        private static int NearestPresetIndex(int width, int height)
        {
            int best = 0;
            long bestDelta = long.MaxValue;
            for (int i = 0; i < ResolutionSizes.Length; i++)
            {
                long delta = (long)(ResolutionSizes[i].x - width) * (ResolutionSizes[i].x - width)
                           + (long)(ResolutionSizes[i].y - height) * (ResolutionSizes[i].y - height);
                if (delta < bestDelta) { bestDelta = delta; best = i; }
            }
            return best;
        }

        private void ActivateSelection()
        {
            _gamepadNavigation = true;
            if (_confirmClear)
            {
                if (_selectedOption == 0) StartNewSave();
                else CancelClear();
                return;
            }
            switch (_selectedOption)
            {
                case 0: SetPaused(false); break;
                case 4: ToggleDisplayMode(); break;
                case 5: CycleResolution(1); break;
                case 6: ToggleFramePacing(); break;
                case 7: BeginClear(); break;
                case 8: Application.Quit(); break;
            }
        }

        private void BeginClear()
        {
            _confirmClear = true;
            _selectedOption = 0;
        }

        private void CancelClear()
        {
            _confirmClear = false;
            _selectedOption = 6;
        }

        private void OnGUI()
        {
            if (StartScreen.IsOpen || !_paused) return;
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, .74f, .32f) }
            };
            _body ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            _selectionStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                normal = { textColor = new Color(1f, .74f, .28f) }
            };

            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            float scale = Mathf.Min(1f, bounds.width / 540f, bounds.height / 700f);
            GUI.matrix *= Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = 470f;
            float left = (bounds.width / scale - width) * .5f;
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, .58f);
            GUI.DrawTexture(new Rect(0f, 0f, bounds.width / scale, bounds.height / scale), Texture2D.whiteTexture);
            GUI.color = new Color(.04f, .07f, .09f, .94f);
            GUI.DrawTexture(new Rect(left - 16f, 82f, width + 32f, 600f), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Box(new Rect(left - 16f, 82f, width + 32f, 600f), string.Empty);
            GUI.Label(new Rect(left, 100f, width, 42f), "暂停", _title);

            if (GUI.Button(new Rect(left + 100f, 158f, 270f, 38f), "继续游戏 / Resume")) SetPaused(false);
            GUI.Label(new Rect(left + 28f, 222f, 120f, 28f), "总音量", _body);
            float previousVolume = _volume;
            _volume = GUI.HorizontalSlider(new Rect(left + 145f, 230f, 230f, 20f), _volume, 0f, 1f);
            GUI.Label(new Rect(left + 385f, 222f, 52f, 28f), $"{_volume:P0}", _body);

            GUI.Label(new Rect(left + 28f, 270f, 120f, 28f), "视野角", _body);
            float previousFieldOfView = _fieldOfView;
            _fieldOfView = GUI.HorizontalSlider(new Rect(left + 145f, 278f, 230f, 20f), _fieldOfView, 55f, 85f);
            GUI.Label(new Rect(left + 385f, 270f, 52f, 28f), $"{_fieldOfView:0}°", _body);
            GUI.Label(new Rect(left + 28f, 316f, 120f, 28f), "镜头速度", _body);
            float previousLookSensitivity = _lookSensitivity;
            _lookSensitivity = GUI.HorizontalSlider(new Rect(left + 145f, 324f, 230f, 20f), _lookSensitivity, .5f, 2f);
            GUI.Label(new Rect(left + 385f, 316f, 60f, 28f), $"{_lookSensitivity:P0}", _body);

            GUI.Label(new Rect(left + 28f, 358f, 120f, 30f), "显示模式", _body);
            if (GUI.Button(new Rect(left + 145f, 354f, 230f, 34f), _fullscreen ? "全屏 / Fullscreen" : "窗口 / Windowed"))
                ToggleDisplayMode();
            GUI.Label(new Rect(left + 28f, 398f, 120f, 30f), "窗口尺寸", _body);
            if (GUI.Button(new Rect(left + 145f, 394f, 230f, 34f), ResolutionLabel))
                CycleResolution(1);
            GUI.Label(new Rect(left + 28f, 438f, 120f, 30f), "帧率模式", _body);
            if (GUI.Button(new Rect(left + 145f, 434f, 230f, 34f), _vSync ? "垂直同步 / VSync" : "60 FPS 限制"))
                ToggleFramePacing();
            if (!Mathf.Approximately(previousVolume, _volume) || !Mathf.Approximately(previousFieldOfView, _fieldOfView) ||
                !Mathf.Approximately(previousLookSensitivity, _lookSensitivity))
            {
                ApplyAudioAndCameraSettings();
                SaveSettings();
            }

            GUI.Label(new Rect(left + 28f, 480f, 410f, 26f), "键鼠：WASD 移动、鼠标观察、Shift 奔跑、E 交互", _body);
            GUI.Label(new Rect(left + 28f, 506f, 410f, 26f), "手柄：左/右摇杆移动与观察、按下左摇杆奔跑、A 交互", _body);
            GUI.Label(new Rect(left + 28f, 532f, 410f, 26f), "Esc / Menu：打开或关闭此菜单", _body);
            if (!_confirmClear)
                GUI.Label(new Rect(left + 28f, 557f, 410f, 24f), "手柄：十字键选择/调整 · A 确认 · B 返回", _body);

            if (!_confirmClear)
            {
                if (GUI.Button(new Rect(left + 100f, 586f, 270f, 34f), "新档试玩 / Start New Save")) BeginClear();
            }
            else
            {
                GUI.Label(new Rect(left + 28f, 557f, 410f, 26f), "将清除任务、升级和探索发现，确定吗？", _body);
                if (GUI.Button(new Rect(left + 58f, 588f, 165f, 34f), "确认清除")) StartNewSave();
                if (GUI.Button(new Rect(left + 247f, 588f, 165f, 34f), "取消")) CancelClear();
            }

            if (GUI.Button(new Rect(left + 100f, 636f, 270f, 34f), "退出游戏 / Quit")) Application.Quit();
            if (_gamepadNavigation)
            {
                float x = _confirmClear ? left + (_selectedOption == 0 ? 32f : 221f) : left + (_selectedOption is 0 or 7 or 8 ? 72f : 4f);
                float y = _confirmClear ? 590f : _selectedOption switch { 0 => 160f, 1 => 221f, 2 => 269f, 3 => 315f, 4 => 357f, 5 => 397f, 6 => 437f, 7 => 589f, _ => 639f };
                GUI.Label(new Rect(x, y, 28f, 28f), "▶", _selectionStyle);
            }
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void ApplyAudioAndCameraSettings()
        {
            UnityEngine.AudioListener.volume = _volume;
            PlayerController.LookSensitivityMultiplier = _lookSensitivity;
            if (Camera.main != null) Camera.main.fieldOfView = _fieldOfView;
        }

        private void ToggleDisplayMode()
        {
            if (!_fullscreen)
            {
                _windowWidth = Screen.width;
                _windowHeight = Screen.height;
            }
            _fullscreen = !_fullscreen;
            ApplyDisplayMode();
            SaveSettings();
        }

        private void ApplyDisplayMode()
        {
            if (_fullscreen)
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            else
                Screen.SetResolution(
                    Mathf.Clamp(_windowWidth, 640, Mathf.Max(640, Screen.currentResolution.width - 80)),
                    Mathf.Clamp(_windowHeight, 360, Mathf.Max(360, Screen.currentResolution.height - 80)),
                    FullScreenMode.Windowed);
        }

        private void ToggleFramePacing()
        {
            _vSync = !_vSync;
            ApplyFramePacing();
            SaveSettings();
        }

        private void ApplyFramePacing()
        {
            QualitySettings.vSyncCount = _vSync ? 1 : 0;
            Application.targetFrameRate = _vSync ? -1 : 60;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(VolumeKey), _volume);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(FovKey), _fieldOfView);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(LookSensitivityKey), _lookSensitivity);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(FullscreenKey), _fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(WindowWidthKey), _windowWidth);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(WindowHeightKey), _windowHeight);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(VSyncKey), _vSync ? 1 : 0);
            PlayerPrefs.Save();
        }

        internal void StartNewSave()
        {
            ProgressStore.ClearPrototypeProgress();
            DiscoveryJournal.ClearProgress();
            SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            IsPaused = false;
        }
    }
}
