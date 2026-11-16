using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        private static string _message = "任务：查看港口告示牌，取得前往潮汐浅滩的线索。";
        private static string _interactionHint = string.Empty;
        private static float _messageSetAt;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _compactTitle;
        private GUIStyle _compactBody;
        private GUIStyle _measure;
        private QuestState _quest;
        private SurveyChartQuest _surveyQuest;
        private StormSignalQuest _stormSignalQuest;
        private PlayerController _player;
        private BoatController _boat;
        private GuidanceMarker _guidance;
        private DiscoveryJournal _discoveries;
        private string _regionLabel;
        private string _objectiveLabel;
        private string _discoveryLabel;
        private string _interactionLabel;
        private string _boatLabel;
        private string _lastRegion;
        private string _lastObjective;
        private string _lastInteractionHint;
        private int _lastDiscoveryCount = -1;
        private int _lastSpeedTenths = -1;
        private int _lastDockDistance = -1;
        private DockingPoint _lastNearbyDock;
        private bool _lastCanDisembark;
        private bool _lastInteractionGamepadPrompts;
        private bool _lastBoatGamepadPrompts;
        private int _lastRefreshFrame = -1;

        public static void SetMessage(string message)
        {
            _message = message;
            _messageSetAt = Time.unscaledTime;
        }
        internal static string CurrentMessage => _message;
        public static void SetInteractionHint(string hint) => _interactionHint = hint;

        private void Awake()
        {
            _quest = GetComponent<QuestState>();
            _surveyQuest = GetComponent<SurveyChartQuest>();
            _stormSignalQuest = GetComponent<StormSignalQuest>();
            _player = FindFirstObjectByType<PlayerController>();
            _boat = FindFirstObjectByType<BoatController>();
            _guidance = GetComponent<GuidanceMarker>();
            _discoveries = GetComponent<DiscoveryJournal>();
        }

        private void OnGUI()
        {
            if (StartScreen.IsOpen || PauseMenu.IsPaused || MapOverlay.IsOpen || QuestJournalPanel.IsOpen || DialoguePanel.IsOpen) return;
            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            if (_lastRefreshFrame != Time.frameCount)
            {
                _lastRefreshFrame = Time.frameCount;
                RefreshLabels();
            }
            _title ??= new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, .73f, .35f) } };
            _body ??= new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, normal = { textColor = Color.white } };
            if (bounds.height < 480) DrawCompactHeader(bounds);
            else
            {
                GUI.Box(new Rect(16f, 16f, 558f, 166f), string.Empty);
                GUI.Label(new Rect(28, 24, 450, 34), _regionLabel, _title);
                GUI.Label(new Rect(28, 58, 530, 50), _objectiveLabel, _body);
                GUI.Label(new Rect(28, 108, 530, 45), _message, _body);
                if (_discoveries != null)
                    GUI.Label(new Rect(28, 150, 280, 28), _discoveryLabel, _body);
            }
            if (!string.IsNullOrEmpty(_interactionLabel))
                GUI.Label(new Rect(bounds.width * .5f - 220f, bounds.height - 105f, 440f, 30f), _interactionLabel, _body);
            if (_player != null && _player.IsOnBoat && _boat != null)
            {
                GUI.Box(new Rect(16f, bounds.height - 82f, Mathf.Min(bounds.width - 32f, 620f), 32f), string.Empty);
                GUI.Label(new Rect(28f, bounds.height - 77f, Mathf.Min(bounds.width - 56f, 596f), 26f), _boatLabel, _body);
            }
            bool onBoat = _player != null && _player.IsOnBoat;
            bool gamepadPrompts = InputPromptMode.UsingGamepad;
            string controls = gamepadPrompts
                ? (onBoat ? "左摇杆航行/转向 · A 靠岸下船 · Y 日志 · View 海图" :
                            "左/右摇杆移动观察 · L3 奔跑 · A 互动 · Y 日志 · View 海图")
                : (onBoat ? "W/S 加减速 · A/D 转向 · E 靠岸下船 · J 日志 · M 海图" :
                            "WASD 移动 · 鼠标观察 · Shift 奔跑 · E 互动 · J 日志 · M 海图");
            float controlsWidth = Mathf.Min(bounds.width - 56f, 596f);
            GUIStyle measure = _measure;
            if (measure == null)
            {
                measure = new GUIStyle(_body) { wordWrap = false };
                _measure = measure;
            }
            if (measure.CalcSize(new GUIContent(controls)).x > controlsWidth)
                controls = onBoat
                    ? "W/S 加减速 · A/D 转向 · E 靠岸 · J 日志 · M 海图"
                    : "WASD 移动 · Shift 跑 · E 互动 · J 日志 · M 海图";
            GUI.Label(new Rect(28, bounds.height - 45, controlsWidth, 30), controls, _body);
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawCompactHeader(Rect bounds)
        {
            _compactTitle ??= new GUIStyle(_title) { fontSize = 19 };
            _compactBody ??= new GUIStyle(_body) { fontSize = 14 };
            GUI.Box(new Rect(8f, 8f, 272f, 94f), string.Empty);
            GUI.Label(new Rect(16f, 14f, 256f, 27f), _regionLabel, _compactTitle);
            string target = _guidance != null && _guidance.Target != null ? _guidance.Label : _objectiveLabel;
            GUI.Label(new Rect(16f, 42f, 256f, 38f), "目标：" + target, _compactBody);
            if (_discoveries != null)
                GUI.Label(new Rect(16f, 80f, 256f, 18f), _discoveryLabel, _compactBody);
            if (Time.unscaledTime - _messageSetAt <= 7f && !string.IsNullOrEmpty(_message))
            {
                GUI.Box(new Rect(8f, 108f, 272f, 70f), string.Empty);
                GUI.Label(new Rect(16f, 113f, 256f, 60f), _message, _compactBody);
            }
        }

        private void RefreshLabels()
        {
            float playerZ = _player == null ? 10f : _player.transform.position.z;
            string region = playerZ <= -62f ? "潮汐浅滩" : playerZ < -12f ? "晨湾航道" : "晨湾港";
            if (region != _lastRegion)
            {
                _lastRegion = region;
                _regionLabel = "雾灯群岛 · " + region;
            }

            string objective = _quest == null ? _message : _quest.Objective;
            if (_quest != null && _quest.Stage == PrismQuestStage.Complete && _surveyQuest != null && _stormSignalQuest != null)
            {
                if (!ProgressStore.HasSurveySail)
                    objective = "向船匠乔恩领取勘测帆。";
                else if (_surveyQuest.Stage == SurveyChartStage.Locked)
                    objective = "向港口管理员艾琳询问失落航图。";
                else if (_surveyQuest.Stage != SurveyChartStage.Complete)
                    objective = _surveyQuest.Stage == SurveyChartStage.SearchRuin && !_surveyQuest.ClueRead
                        ? _surveyQuest.RuinClueObjective
                        : _surveyQuest.Objective;
                else if (_stormSignalQuest.Stage == StormSignalStage.Locked)
                    objective = "向船匠乔恩询问风暴透镜。";
                else if (_stormSignalQuest.Stage != StormSignalStage.Complete)
                    objective = _stormSignalQuest.Objective;
                else
                    objective = "试玩版主线已完成，可自由探索群岛。";
            }
            if (objective != _lastObjective)
            {
                _lastObjective = objective;
                _objectiveLabel = "目标：" + objective;
            }

            if (_discoveries != null && _discoveries.Count != _lastDiscoveryCount)
            {
                _lastDiscoveryCount = _discoveries.Count;
                _discoveryLabel = $"探索发现：{_lastDiscoveryCount}/{_discoveries.Total}";
            }
            bool gamepadPrompts = InputPromptMode.UsingGamepad;
            if (_interactionHint != _lastInteractionHint || gamepadPrompts != _lastInteractionGamepadPrompts)
            {
                _lastInteractionHint = _interactionHint;
                _lastInteractionGamepadPrompts = gamepadPrompts;
                _interactionLabel = InputPromptMode.FormatInteractionHint(_interactionHint);
            }

            if (_player == null || !_player.IsOnBoat || _boat == null) return;
            float speed = Mathf.Abs(_boat.Speed);
            int speedTenths = Mathf.RoundToInt(speed * 10f);
            DockingPoint targetDock = _guidance?.Target?.GetComponent<DockingPoint>();
            int dockDistance = targetDock == null ? -1 : Mathf.RoundToInt(Vector3.Distance(_boat.transform.position, targetDock.transform.position));
            DockingPoint nearbyDock = _boat.NearbyDock;
            bool canDisembark = speed < 1.2f;
            if (speedTenths == _lastSpeedTenths && dockDistance == _lastDockDistance && nearbyDock == _lastNearbyDock &&
                canDisembark == _lastCanDisembark && gamepadPrompts == _lastBoatGamepadPrompts) return;
            _lastSpeedTenths = speedTenths;
            _lastDockDistance = dockDistance;
            _lastNearbyDock = nearbyDock;
            _lastCanDisembark = canDisembark;
            _lastBoatGamepadPrompts = gamepadPrompts;
            string interactKey = gamepadPrompts ? "A" : "E";
            string docking = nearbyDock == null ? "靠近码头并减速下船" :
                canDisembark ? $"可在{nearbyDock.DockName}按 {interactKey} 下船" :
                $"靠近{nearbyDock.DockName}，减速至 1.2 m/s";
            string distance = dockDistance < 0 ? string.Empty : $" · 距码头 {dockDistance} m";
            _boatLabel = $"航速 {speedTenths / 10f:0.0} m/s{distance} · {docking}";
        }
    }
}
