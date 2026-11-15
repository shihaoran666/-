#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FoglightIsles.Editor
{
    public static class PrototypeTools
    {
        [MenuItem("Foglight Isles/Clear Prototype Progress")]
        public static void ClearProgress()
        {
            if (!EditorUtility.DisplayDialog("清除雾灯群岛原型进度", "将清除任务、谜题、船只升级和探索发现存档。此操作不可恢复。", "清除", "取消"))
                return;

            ProgressStore.ClearPrototypeProgress();
            DiscoveryJournal.ClearProgress();
            Debug.Log("[FoglightIsles Tools] Prototype progress cleared. Restart Play Mode to begin from a new save.");
        }
    }
}
#endif
