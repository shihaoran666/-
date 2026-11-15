using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FoglightIsles
{
    /// <summary>Minimal title screen so the demo has an intentional entry point.</summary>
    public sealed class StartScreen : MonoBehaviour
    {
        private GUIStyle _title;
        private GUIStyle _subtitle;
        private GUIStyle _buildStyle;
        private GUIStyle _body;
        private string _buildLabel;
        private bool _confirmNewSave;
        private bool _hasSavedProgress;

        internal bool IsConfirmingNewSave => _confirmNewSave;
        internal bool HasSavedProgress => _hasSavedProgress;

        public static bool IsOpen { get; private set; } = true;

        private void Awake()
        {
            IsOpen = true;
            _buildLabel = ReadBuildLabel();
            _hasSavedProgress = ProgressStore.LoadQuestStage() != PrismQuestStage.NotAccepted ||
                                ProgressStore.LoadSurveyChartStage() != SurveyChartStage.Locked ||
                                ProgressStore.LoadStormSignalStage() != StormSignalStage.Locked ||
                                ProgressStore.HasSurveySail || ProgressStore.LoadTideBeaconProgress() > 0 ||
                                FindFirstObjectByType<DiscoveryJournal>()?.Count > 0;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!IsOpen) return;
            bool confirm = (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (_confirmNewSave)
            {
                if (confirm) StartNewSave();
                else if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                         (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
                    _confirmNewSave = false;
                return;
            }
            if ((Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame))
                _confirmNewSave = true;
            else if (confirm) BeginExploration();
        }

        private void OnGUI()
        {
            if (!IsOpen) return;
            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, .75f, .34f) }
            };
            _subtitle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(.75f, .86f, .9f) }
            };
            _buildStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(.7f, .82f, .86f) }
            };
            _body ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            GUI.Box(new Rect(0f, 0f, bounds.width, bounds.height), string.Empty);
            float width = Mathf.Min(620f, bounds.width - 60f);
            float left = (bounds.width - width) * .5f;
            float top = Mathf.Min(bounds.height * .26f, Mathf.Max(4f, bounds.height - 358f));
            GUI.Label(new Rect(left, top, width, 72f), "雾灯群岛", _title);
            GUI.Label(new Rect(left, top + 70f, width, 30f), "Foglight Isles · Prototype Demo", _subtitle);
            GUI.Label(new Rect(left, top + 101f, width, 24f), _buildLabel, _buildStyle);
            if (_confirmNewSave)
            {
                GUI.Label(new Rect(left, top + 126f, width, 44f),
                    "清除任务、船只升级和探索发现？\n音量与画面设置会保留。", _body);
                if (GUI.Button(new Rect(left + 10f, top + 180f, (width - 30f) * .5f, 42f), "确认新档 / Confirm")) StartNewSave();
                if (GUI.Button(new Rect(left + (width + 10f) * .5f, top + 180f, (width - 30f) * .5f, 42f), "取消 / Cancel"))
                    _confirmNewSave = false;
                GUI.Label(new Rect(left, top + 236f, width, 26f), "Enter / 手柄 A：确认   ·   Esc / 手柄 B：取消", _body);
                return;
            }

            GUI.Label(new Rect(left, top + 126f, width, 44f), "穿过迷雾，修复灯塔，把一条条失落的航线带回晨湾港。", _body);
            if (GUI.Button(new Rect(left + 10f, top + 180f, (width - 30f) * .5f, 42f), (_hasSavedProgress ? "继续探索 / Continue" : "开始探索 / Start"))) BeginExploration();
            if (GUI.Button(new Rect(left + (width + 10f) * .5f, top + 180f, (width - 30f) * .5f, 42f), "新档试玩 / Start New Save"))
                _confirmNewSave = true;
            string controlHelp = InputPromptMode.UsingGamepad
                ? "手柄：双摇杆移动/观察 · A 确认/互动 · View 海图 · Y 新档试玩"
                : "键鼠：WASD + 鼠标 + E · J 日志 · M 海图 · N 新档试玩";
            GUI.Label(new Rect(left, top + 228f, width, 46f), controlHelp, _body);
            if (GUI.Button(new Rect(left + 190f, top + 280f, width - 380f, 30f), "退出 / Quit")) Application.Quit();
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private static string ReadBuildLabel()
        {
            string path = Path.Combine(Application.dataPath, "../BUILD_INFO.txt");
            try
            {
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadLines(path))
                        if (line.StartsWith("Build ID: ", StringComparison.Ordinal))
                            return "构建编号：" + line.Substring("Build ID: ".Length);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return "开发构建 · " + Application.version;
        }

        internal void StartNewSave()
        {
            if (!IsOpen || !_confirmNewSave) return;
            ProgressStore.ClearPrototypeProgress();
            DiscoveryJournal.ClearProgress();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void BeginExploration()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            QuestState prism = FindFirstObjectByType<QuestState>();
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            string welcome = prism == null || prism.Stage == PrismQuestStage.NotAccepted
                ? "欢迎来到晨湾港。查看告示牌，开始你的第一次探索。"
                : storm != null && storm.Stage == StormSignalStage.Complete
                    ? "欢迎回来。试玩版主线已完成，可继续自由探索群岛；按 J 查看探索笔记。"
                    : "欢迎回来。按 J 查看任务日志，按 M 打开海图，继续当前探索。";
            PrototypeHUD.SetMessage(welcome);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            IsOpen = false;
        }
    }
}
