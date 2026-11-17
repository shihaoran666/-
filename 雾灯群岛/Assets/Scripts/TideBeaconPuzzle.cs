using System;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Persistent three-step exploration puzzle. The order is west, centre, east.</summary>
    public sealed class TideBeaconPuzzle : MonoBehaviour
    {
        private const int BeaconCount = 3;
        private readonly Transform[] _beacons = new Transform[BeaconCount];
        private QuestState _quest;
        private int _progress;

        public event Action Changed;
        public event Action<bool> Activated;
        public bool IsComplete => _progress >= BeaconCount;
        public Transform CurrentTarget => IsComplete ? null : _beacons[_progress];
        public string CurrentLabel => IsComplete ? "棱镜祭台已开启" : $"潮汐信标 {_progress + 1}/{BeaconCount}";

        public void Configure(QuestState quest)
        {
            _quest = quest;
            _progress = ProgressStore.LoadTideBeaconProgress();
        }

        public void RegisterBeacon(int index, Transform beacon)
        {
            _beacons[index] = beacon;
            RefreshBeaconVisual(index);
        }

        public void Activate(int index)
        {
            if (_quest.Stage != PrismQuestStage.SearchTideflats)
            {
                PrototypeHUD.SetMessage("信标仍被海雾封住。先从晨湾港告示牌接取灯塔委托。 ");
                return;
            }
            if (IsComplete)
            {
                PrototypeHUD.SetMessage("三座潮汐信标已经共鸣，棱镜祭台已经开启。 ");
                return;
            }
            if (index != _progress)
            {
                _progress = 0;
                ProgressStore.SaveTideBeaconProgress(_progress);
                RefreshAllVisuals();
                Changed?.Invoke();
                Activated?.Invoke(false);
                PrototypeHUD.SetMessage("潮音消散，已点亮的信标全部熄灭。石碑箭纹从西向东——西、中、东，从西侧重新来。 ");
                return;
            }

            _progress++;
            ProgressStore.SaveTideBeaconProgress(_progress);
            RefreshAllVisuals();
            Changed?.Invoke();
            Activated?.Invoke(true);
            PrototypeHUD.SetMessage(IsComplete
                ? "三座潮汐信标回应了彼此。棱镜祭台已开启。"
                : $"信标已点亮：{_progress}/{BeaconCount}。沿着箭纹继续寻找下一座。");
        }

        private void RefreshAllVisuals()
        {
            for (int index = 0; index < BeaconCount; index++) RefreshBeaconVisual(index);
        }

        private void RefreshBeaconVisual(int index)
        {
            if (_beacons[index] == null) return;
            bool lit = index < _progress;
            var renderer = _beacons[index].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", lit ? new Color(1f, .44f, .08f) * 3f : Color.black);
            }
            var light = _beacons[index].GetComponentInChildren<Light>();
            if (light != null) light.enabled = lit;
        }
    }
}
