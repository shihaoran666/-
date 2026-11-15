using System;
using UnityEngine;

namespace FoglightIsles
{
    public enum StormSignalStage { Locked, SearchTideflats, ReturnToShipwright, Complete }

    /// <summary>Third vertical-slice task: recover a stormglass lens for the harbour signal mast.</summary>
    public sealed class StormSignalQuest : MonoBehaviour
    {
        private SurveyChartQuest _surveyQuest;
        public StormSignalStage Stage { get; private set; }
        public event Action<StormSignalStage> Changed;

        public string Objective => Stage switch
        {
            StormSignalStage.SearchTideflats => "在潮汐浅滩的坍塌灯塔旁寻找风暴透镜。",
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
                    PrototypeHUD.SetMessage("新委托：帮乔恩从潮汐浅滩的坍塌灯塔取回风暴透镜，修好港口信号桅杆。 ");
                    break;
                case StormSignalStage.SearchTideflats:
                    PrototypeHUD.SetMessage("乔恩：风暴透镜是块蓝白色晶片，就在坍塌灯塔旁的碎石间。 ");
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
            SetStage(StormSignalStage.ReturnToShipwright);
            PrototypeHUD.SetMessage("取得风暴透镜：" + Objective);
        }

        private void SetStage(StormSignalStage stage)
        {
            Stage = stage;
            ProgressStore.SaveStormSignalStage(stage);
            Changed?.Invoke(stage);
        }
    }
}
