using System;
using UnityEngine;

namespace FoglightIsles
{
    public enum SurveyChartStage { Locked, SearchRuin, ReturnToHarbour, Complete }

    /// <summary>Second vertical-slice quest, unlocked after the lighthouse prism hand-in.</summary>
    public sealed class SurveyChartQuest : MonoBehaviour
    {
        private QuestState _prismQuest;
        public SurveyChartStage Stage { get; private set; }
        public event Action<SurveyChartStage> Changed;

        public string Objective => Stage switch
        {
            SurveyChartStage.SearchRuin => "前往潮汐浅滩的遗迹，找回失落的航图。",
            SurveyChartStage.ReturnToHarbour => "把失落航图交给晨湾港管理员艾琳。",
            _ => string.Empty
        };

        public void Configure(QuestState prismQuest)
        {
            _prismQuest = prismQuest;
            Stage = ProgressStore.LoadSurveyChartStage();
        }

        public void TalkToEileen()
        {
            if (_prismQuest.Stage != PrismQuestStage.Complete)
            {
                PrototypeHUD.SetMessage("艾琳：先完成灯塔棱镜的委托，晨湾港才有能力重绘航线。 ");
                return;
            }

            switch (Stage)
            {
                case SurveyChartStage.Locked:
                    SetStage(SurveyChartStage.SearchRuin);
                    PrototypeHUD.SetMessage("新委托：遗迹里遗失了一张旧航图。前往潮汐浅滩，把它带回来。 ");
                    break;
                case SurveyChartStage.SearchRuin:
                    PrototypeHUD.SetMessage("艾琳：航图应该还在潮汐浅滩遗迹的石台旁。 ");
                    break;
                case SurveyChartStage.ReturnToHarbour:
                    SetStage(SurveyChartStage.Complete);
                    PrototypeHUD.SetMessage("委托完成：艾琳将航图挂上港口导航台，未知海域的轮廓显现了。 ");
                    break;
                case SurveyChartStage.Complete:
                    PrototypeHUD.SetMessage("艾琳：多亏这张航图，我们终于能看清下一片海域。 ");
                    break;
            }
        }

        public void CollectChart()
        {
            if (Stage != SurveyChartStage.SearchRuin)
            {
                PrototypeHUD.SetMessage("一张被潮水浸湿的航图。也许艾琳会知道它的用途。 ");
                return;
            }
            SetStage(SurveyChartStage.ReturnToHarbour);
            PrototypeHUD.SetMessage("取得失落航图：" + Objective);
        }

        private void SetStage(SurveyChartStage stage)
        {
            Stage = stage;
            ProgressStore.SaveSurveyChartStage(stage);
            Changed?.Invoke(stage);
        }
    }
}
