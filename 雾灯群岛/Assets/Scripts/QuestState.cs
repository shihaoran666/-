using System;
using UnityEngine;

namespace FoglightIsles
{
    public enum PrismQuestStage { NotAccepted, SearchTideflats, ReturnToHarbour, Complete }

    /// <summary>Explicit state machine for the first vertical-slice quest.</summary>
    public sealed class QuestState : MonoBehaviour
    {
        public PrismQuestStage Stage { get; private set; }
        public event Action<PrismQuestStage> Changed;

        private void Awake() => Stage = ProgressStore.LoadQuestStage();

        public string Objective => Stage switch
        {
            PrismQuestStage.NotAccepted => "查看港口告示牌，取得前往潮汐浅滩的线索。",
            PrismQuestStage.SearchTideflats => "航向潮汐浅滩，依次点亮三座潮汐信标，开启棱镜祭台。",
            PrismQuestStage.ReturnToHarbour => "带着棱镜零件返回晨湾港，交给港口管理员。",
            PrismQuestStage.Complete => "灯塔棱镜已送回晨湾港。航线修复工作即将开始。",
            _ => string.Empty
        };

        public void Accept()
        {
            if (Stage != PrismQuestStage.NotAccepted) return;
            SetStage(PrismQuestStage.SearchTideflats);
            PrototypeHUD.SetMessage("任务已接受：" + Objective);
        }

        public void CollectPrism()
        {
            if (Stage != PrismQuestStage.SearchTideflats)
            {
                PrototypeHUD.SetMessage("这块棱镜属于失修灯塔。先去港口告示牌查看委托。 ");
                return;
            }
            SetStage(PrismQuestStage.ReturnToHarbour);
            PrototypeHUD.SetMessage("取得灯塔棱镜零件：" + Objective);
        }

        public void HandIn()
        {
            if (Stage != PrismQuestStage.ReturnToHarbour)
            {
                PrototypeHUD.SetMessage(Stage == PrismQuestStage.Complete ? "委托已经完成。" : "管理员正在等候潮汐浅滩的灯塔线索。 ");
                return;
            }
            SetStage(PrismQuestStage.Complete);
            PrototypeHUD.SetMessage("委托完成：晨湾港灯塔已重新点亮。去找船匠乔恩领取勘测帆。 ");
        }

        private void SetStage(PrismQuestStage newStage)
        {
            Stage = newStage;
            ProgressStore.SaveQuestStage(Stage);
            Changed?.Invoke(Stage);
        }
    }
}
