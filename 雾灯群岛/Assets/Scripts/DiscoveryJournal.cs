using System.Collections.Generic;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Persistent optional discoveries that reward free-roam exploration without gating quests.</summary>
    public sealed class DiscoveryJournal : MonoBehaviour
    {
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string Title;
            public readonly string Region;
            public readonly string Note;

            public Entry(string id, string title, string region, string note)
            {
                Id = id;
                Title = title;
                Region = region;
                Note = note;
            }
        }

        private const string Prefix = "foglightisles.discovery.";
        private static readonly Entry[] Entries =
        {
            new("harbour-log", "潮湿的港口日志", "晨湾港", "最后一页记录着雾中灯塔的交接日期，字迹已被海水晕开。"),
            new("old-compass", "褪色的旧罗盘", "晨湾港", "指针总偏向潮汐浅滩，和旧航图上的航线相合。"),
            new("tide-shell", "会回响的潮汐贝壳", "潮汐浅滩", "贴近耳边时，壳内会传出三次潮鸣。"),
            new("watchers-charm", "守望者护符", "潮汐浅滩", "护符背面刻着一艘从晨湾港驶出的旧船。")
        };
        private readonly HashSet<string> _discovered = new HashSet<string>();

        public int Count => _discovered.Count;
        public int Total => Entries.Length;
        public static IReadOnlyList<Entry> Catalog => Entries;

        private void Awake()
        {
            foreach (Entry entry in Entries)
                if (PlayerPrefs.GetInt(ProgressStore.StorageKey(Prefix + entry.Id), 0) == 1) _discovered.Add(entry.Id);
        }

        public bool IsDiscovered(string id) => _discovered.Contains(id);

        public bool Discover(string id, string title)
        {
            if (!_discovered.Add(id)) return false;
            PlayerPrefs.SetInt(ProgressStore.StorageKey(Prefix + id), 1);
            PlayerPrefs.Save();
            PrototypeHUD.SetMessage(Count == Total
                ? $"四件探索物已集齐（{Count}/{Total}）。返港找艾琳整理旧航线。按 J / 手柄 Y 查看笔记。"
                : $"探索发现：{title}（{Count}/{Total}）。按 J / 手柄 Y 查看探索笔记。");
            return true;
        }

        public static void ClearProgress()
        {
            foreach (Entry entry in Entries) PlayerPrefs.DeleteKey(ProgressStore.StorageKey(Prefix + entry.Id));
            ProgressStore.ClearHarbourRouteArchive();
            PlayerPrefs.Save();
        }
    }
}
