using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Reports missing vertical-slice runtime objects in the Unity console after the bootstrap completes.</summary>
    public sealed class PrototypeRuntimeValidator : MonoBehaviour
    {
        private static readonly string[] RequiredObjects =
        {
            "Explorer", "Main Camera", "Skiff - Dawn Petrel", "Morning Bay Shore", "Tideflats Island",
            "晨湾港码头", "潮汐浅滩码头", "港口管理员艾琳", "船匠乔恩", "Harbour Notice Board",
            "西侧潮汐信标", "中央潮汐信标", "东侧潮汐信标", "潮汐箭纹石碑", "Morning Bay Lighthouse", "Harbour Navigation Table", "Harbour Signal Mast"
        };

        private void Start()
        {
            Debug.Log($"[FoglightIsles Save] Scope: {ProgressStore.ActiveScope}; prism stage: {GetComponent<QuestState>()?.Stage}.");

            int failures = 0;
            foreach (string objectName in RequiredObjects)
            {
                if (GameObject.Find(objectName) == null)
                {
                    Debug.LogError($"[FoglightIsles Validation] Missing required runtime object: {objectName}");
                    failures++;
                }
            }

            if (GetComponent<QuestState>() == null || GetComponent<TideBeaconPuzzle>() == null ||
                GetComponent<SurveyChartQuest>() == null || GetComponent<StormSignalQuest>() == null ||
                GetComponent<PauseMenu>() == null || GetComponent<GuidanceMarker>() == null ||
                GetComponent<QuestJournalPanel>() == null)
            {
                Debug.LogError("[FoglightIsles Validation] One or more core gameplay services are not attached to Game Bootstrap.");
                failures++;
            }

            if (failures == 0)
                Debug.Log("[FoglightIsles Validation] PASS — greybox harbour, tideflats and vertical-slice services are present.");
            else
                Debug.LogError($"[FoglightIsles Validation] FAIL — {failures} issue(s) found.");
        }
    }
}
