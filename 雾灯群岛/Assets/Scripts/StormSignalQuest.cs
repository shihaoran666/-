using System;
using UnityEngine;

namespace FoglightIsles
{
    public enum StormSignalStage { Locked, SearchTideflats, ReturnToShipwright, Complete }

    /// <summary>Third vertical-slice task: recover a stormglass lens for the harbour signal mast.</summary>
    public sealed class StormSignalQuest : MonoBehaviour
    {
        private static readonly string[] Directions = { "北", "东", "南", "西" };
        private SurveyChartQuest _surveyQuest;
        public StormSignalStage Stage { get; private set; }
        public int VaneDirection => ProgressStore.LoadStormVaneDirection();
        public bool VaneAligned => VaneDirection == 0;
        public event Action<StormSignalStage> Changed;

        public string Objective => Stage switch
        {
            StormSignalStage.SearchTideflats => VaneAligned
                ? "风向标已指向晨湾港；取走坍塌灯塔旁的风暴透镜。"
                : "用海图辨认晨湾港方位，再到坍塌灯塔旁旋转风向标。",
            StormSignalStage.ReturnToShipwright => "把风暴透镜交给晨湾港的船匠乔恩。",
            _ => string.Empty
        };

        public void Configure(SurveyChartQuest surveyQuest)
        {
            _surveyQuest = surveyQuest;
            Stage = ProgressStore.LoadStormSignalStage();
        }

        public void TalkToJon()
        {
            if (_surveyQuest.Stage != SurveyChartStage.Complete)
            {
                PrototypeHUD.SetMessage("乔恩：等艾琳把航图理清，我们才能知道风暴信号该指向哪里。 ");
                return;
            }

            switch (Stage)
            {
                case StormSignalStage.Locked:
                    SetStage(StormSignalStage.SearchTideflats);
                    PrototypeHUD.SetMessage("新委托：用海图辨认晨湾港的方位，把浅滩坍塌灯塔旁的风向标转向港口，再取回透镜。 ");
                    break;
                case StormSignalStage.SearchTideflats:
                    PrototypeHUD.SetMessage("乔恩：在海图上比较晨湾港和浅滩的位置。把灯塔旁的风向标转向港口，才能卸下透镜。 ");
                    break;
                case StormSignalStage.ReturnToShipwright:
                    SetStage(StormSignalStage.Complete);
                    PrototypeHUD.SetMessage("试玩版主线完成：港口信号桅杆重新亮起。你仍可自由探索群岛。 ");
                    break;
                case StormSignalStage.Complete:
                    PrototypeHUD.SetMessage("乔恩：信号桅杆工作得很漂亮。晨鹱号已经准备好继续探索。 ");
                    break;
            }
        }

        public void CollectLens()
        {
            if (Stage != StormSignalStage.SearchTideflats)
            {
                PrototypeHUD.SetMessage("风暴透镜折射着微光。先向乔恩问清它的用途。 ");
                return;
            }
            if (!VaneAligned)
            {
                PrototypeHUD.SetMessage("透镜仍锁在石座上：先用海图判断晨湾港的方向，再校准风向标。 ");
                return;
            }
            SetStage(StormSignalStage.ReturnToShipwright);
            PrototypeHUD.SetMessage("取得风暴透镜：" + Objective);
        }

        public void RotateVane()
        {
            if (Stage != StormSignalStage.SearchTideflats || VaneAligned)
            {
                PrototypeHUD.SetMessage("风向标已经指向晨湾港，无须再转动。 ");
                return;
            }
            int direction = (VaneDirection + 1) % 4;
            ProgressStore.SaveStormVaneDirection(direction);
            PrototypeHUD.SetMessage(direction == 0
                ? "箭头指北，正对晨湾港。透镜石座的锁扣松开了。"
                : "风向标现在指向" + Directions[direction] + "方；对照海图确认港口的方位。 ");
            Changed?.Invoke(Stage);
        }

        private void SetStage(StormSignalStage stage)
        {
            Stage = stage;
            ProgressStore.SaveStormSignalStage(stage);
            Changed?.Invoke(stage);
        }
    }
}
