using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    /// <summary>Readable, pause-safe record of the demo's three quest lines.</summary>
    public sealed class QuestJournalPanel : MonoBehaviour
    {
        private QuestState _prism;
        private SurveyChartQuest _chart;
        private StormSignalQuest _storm;
        private DiscoveryJournal _discoveries;
        private GUIStyle _title;
        private GUIStyle _heading;
        private GUIStyle _body;
        private bool _showDiscoveries;

        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            IsOpen = false;
            _prism = GetComponent<QuestState>();
            _chart = GetComponent<SurveyChartQuest>();
            _storm = GetComponent<StormSignalQuest>();
            _discoveries = GetComponent<DiscoveryJournal>();
        }

        private void Update()
        {
            if (StartScreen.IsOpen || PauseMenu.IsPaused || MapOverlay.IsOpen || DialoguePanel.IsOpen) return;
            bool journalKey = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) ||
                              (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);
            bool closeKey = IsOpen && ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                                       (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame));
            if (journalKey || closeKey)
            {
                SetOpen(!IsOpen);
                return;
            }
            if (IsOpen && ((Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)))
                _showDiscoveries = !_showDiscoveries;
        }

        internal void OpenForReview() => SetOpen(true);
        internal void CloseForPause() { if (IsOpen) SetOpen(false); }
        internal void OpenDiscoveriesForReview()
        {
            SetOpen(true);
            _showDiscoveries = true;
        }

        private void SetOpen(bool open)
        {
            IsOpen = open;
            if (open) _showDiscoveries = false;
            Time.timeScale = open ? 0f : 1f;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        private void OnGUI()
        {
            if (!IsOpen) return;
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 31, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, .74f, .32f) }
            };
            _heading ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 21, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(.58f, .84f, .94f) }
            };
            _body ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, wordWrap = true,
                normal = { textColor = Color.white }
            };

            Matrix4x4 previousMatrix = SafeGuiLayout.Begin(out Rect bounds);
            try
            {
            float scale = Mathf.Min(1f, bounds.width / 760f, bounds.height / 620f);
            _body.fontSize = Mathf.Max(16, Mathf.CeilToInt(12f / scale));
            GUI.matrix *= Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            const float width = 720f;
            const float height = 580f;
            float left = (bounds.width / scale - width) * .5f;
            float top = (bounds.height / scale - height) * .5f;
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, .62f);
            GUI.DrawTexture(new Rect(0f, 0f, bounds.width / scale, bounds.height / scale), Texture2D.whiteTexture);
            GUI.color = new Color(.04f, .07f, .09f, .95f);
            GUI.DrawTexture(new Rect(left, top, width, height), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Box(new Rect(left, top, width, height), string.Empty);
            GUI.Label(new Rect(left + 20f, top + 16f, width - 40f, 44f),
                _showDiscoveries ? "探索笔记 · 晨湾海域" : "任务日志 · 晨湾海域", _title);
            if (GUI.Button(new Rect(left + 34f, top + 68f, 150f, 34f), "任务")) _showDiscoveries = false;
            if (GUI.Button(new Rect(left + 194f, top + 68f, 150f, 34f), "探索发现")) _showDiscoveries = true;

            if (_showDiscoveries)
                DrawDiscoveries(left, top);
            else
            {
                DrawQuest(left, top + 120f, "灯塔棱镜", PrismStatus(), PrismDetail());
                DrawQuest(left, top + 240f, "失落航图", ChartStatus(), ChartDetail());
                DrawQuest(left, top + 360f, "风暴透镜", StormStatus(), StormDetail());
            }

            GUI.Label(new Rect(left + 28f, top + 503f, width - 56f, 27f),
                $"探索发现：{_discoveries?.Count ?? 0}/{_discoveries?.Total ?? 0}   ·   Tab / 手柄 RB 切换页面", _body);
            GUI.Label(new Rect(left + 28f, top + 537f, width - 56f, 27f),
                "J / 手柄 Y、Esc / 手柄 B 关闭日志", _body);
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawDiscoveries(float left, float top)
        {
            for (int index = 0; index < DiscoveryJournal.Catalog.Count; index++)
            {
                DiscoveryJournal.Entry entry = DiscoveryJournal.Catalog[index];
                bool found = _discoveries != null && _discoveries.IsDiscovered(entry.Id);
                float row = top + 120f + index * 92f;
                GUI.Label(new Rect(left + 28f, row, 650f, 30f),
                    found ? entry.Title + "  ·  " + entry.Region : "未发现的线索  ·  " + entry.Region, _heading);
                GUI.Label(new Rect(left + 34f, row + 36f, 650f, 48f),
                    found ? entry.Note : entry.Hint, _body);
            }
        }

        private void DrawQuest(float left, float top, string title, string status, string detail)
        {
            GUI.Label(new Rect(left + 28f, top, 640f, 32f), $"{title}  ·  {status}", _heading);
            GUI.Label(new Rect(left + 34f, top + 38f, 650f, 67f), detail, _body);
        }

        private string PrismStatus() => _prism?.Stage switch
        {
            PrismQuestStage.SearchTideflats => "进行中",
            PrismQuestStage.ReturnToHarbour => "待交付",
            PrismQuestStage.Complete => "已完成",
            _ => "未接取"
        };

        private string PrismDetail() => _prism?.Stage switch
        {
            PrismQuestStage.SearchTideflats => "前往潮汐浅滩，查看码头旁的箭纹石碑，点亮三座信标并取得棱镜。",
            PrismQuestStage.ReturnToHarbour => "携带棱镜回到晨湾港，交给管理员艾琳。",
            PrismQuestStage.Complete => ProgressStore.HasSurveySail
                ? "棱镜已交付，灯塔重新点亮；晨鹱号已换上勘测帆。"
                : "棱镜已交付，港口灯塔重新点亮。向船匠乔恩领取勘测帆。",
            _ => "到晨湾港告示牌查看灯塔委托。"
        };

        private string ChartStatus() => _chart?.Stage switch
        {
            SurveyChartStage.SearchRuin => "进行中",
            SurveyChartStage.ReturnToHarbour => "待交付",
            SurveyChartStage.Complete => "已完成",
            _ => "未解锁"
        };

        private string ChartDetail() => _chart?.Stage switch
        {
            SurveyChartStage.SearchRuin when _chart != null && !_chart.ClueRead => "先辨认遗迹残碑上的旧航线，再取走石台上的航图。",
            SurveyChartStage.SearchRuin => "到潮汐浅滩遗迹入口石台取回失落航图。",
            SurveyChartStage.ReturnToHarbour => "将航图带回晨湾港交给艾琳。",
            SurveyChartStage.Complete => "航图已交付，新的航线轮廓显现。",
            _ => "修复港口灯塔后，向艾琳询问旧航图。"
        };

        private string StormStatus() => _storm?.Stage switch
        {
            StormSignalStage.SearchTideflats => "进行中",
            StormSignalStage.ReturnToShipwright => "待交付",
            StormSignalStage.Complete => "已完成",
            _ => "未解锁"
        };

        private string StormDetail() => _storm?.Stage switch
        {
            StormSignalStage.SearchTideflats => _storm.VaneAligned
                ? "风向标已指北；到坍塌灯塔旁取走蓝白色风暴透镜。"
                : "在海图上比较晨湾港与浅滩的位置；旋转坍塌灯塔旁的风向标，使箭头指向港口。",
            StormSignalStage.ReturnToShipwright => "将透镜带回晨湾港交给船匠乔恩。",
            StormSignalStage.Complete => "信号桅杆已修复。试玩版主线完成，可以继续自由探索。",
            _ => "完成航图任务后，向乔恩询问风暴透镜。"
        };

        private void OnDestroy()
        {
            if (IsOpen) Time.timeScale = 1f;
            IsOpen = false;
        }
    }
}
