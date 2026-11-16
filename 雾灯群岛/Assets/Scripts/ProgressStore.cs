using System;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Small versioned persistence boundary for the prototype's progression state.</summary>
    public static class ProgressStore
    {
        private const string QuestStageKey = "foglightisles.quest.prism.stage.v1";
        private const string SurveySailKey = "foglightisles.boat.survey-sail.v1";
        private const string TideBeaconProgressKey = "foglightisles.tide-beacons.v1";
        private const string SurveyChartStageKey = "foglightisles.quest.survey-chart.v1";
        private const string StormSignalStageKey = "foglightisles.quest.storm-signal.v1";
        internal const string StormVaneDirectionKey = "foglightisles.quest.storm-vane-direction.v1";
        private const string TideClueReadKey = "foglightisles.tide-clue-read.v1";
        internal const string SurveyClueReadKey = "foglightisles.quest.survey-chart.clue.v1";
        private const string HarbourProsperityKey = "foglightisles.harbour.prosperity.v1";
        private const string HarbourRouteArchiveKey = "foglightisles.harbour.route-archive.v1";
        private static readonly bool IsInteractiveReview = Array.IndexOf(Environment.GetCommandLineArgs(), "-interactive-review") >= 0;
        private static readonly bool IsPerformanceRun = Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-performance") >= 0;
        private static readonly bool IsValidationRun = Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-quest-loop") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-quest-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-discoveries") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-discoveries-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-new-save") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-title-new-save") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-look") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-voyage") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-menu") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-journey") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-island-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-first-quest") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-first-quest-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-chart-quest") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-chart-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-chart-old-save") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-storm-old-save") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-storm-quest") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-storm-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-free-roam") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-keyboard-free-roam-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-input-prompts") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-focus-pause") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-display-mode") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-display-resume") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-playtest-screenshot") >= 0 ||
                                                       Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-review") >= 0;
        private static readonly string StorageSuffix = IsValidationRun ? ".validation" : IsPerformanceRun ? ".performance" :
                                                       IsInteractiveReview ? ".playtest.v2" : string.Empty;

        internal static string ActiveScope => StorageSuffix.Length == 0 ? "regular" : StorageSuffix;
        internal static string StorageKey(string key) => StorageSuffix.Length == 0 ? key : key + StorageSuffix;

        public static PrismQuestStage LoadQuestStage()
        {
            int value = PlayerPrefs.GetInt(StorageKey(QuestStageKey), (int)PrismQuestStage.NotAccepted);
            return value >= (int)PrismQuestStage.NotAccepted && value <= (int)PrismQuestStage.Complete
                ? (PrismQuestStage)value
                : PrismQuestStage.NotAccepted;
        }

        public static void SaveQuestStage(PrismQuestStage stage)
        {
            PlayerPrefs.SetInt(StorageKey(QuestStageKey), (int)stage);
            PlayerPrefs.Save();
        }

        public static bool HasSurveySail => PlayerPrefs.GetInt(StorageKey(SurveySailKey), 0) == 1;

        public static void UnlockSurveySail()
        {
            PlayerPrefs.SetInt(StorageKey(SurveySailKey), 1);
            PlayerPrefs.Save();
        }

        public static int LoadTideBeaconProgress() => Mathf.Clamp(PlayerPrefs.GetInt(StorageKey(TideBeaconProgressKey), 0), 0, 3);

        public static void SaveTideBeaconProgress(int progress)
        {
            PlayerPrefs.SetInt(StorageKey(TideBeaconProgressKey), Mathf.Clamp(progress, 0, 3));
            PlayerPrefs.Save();
        }

        public static SurveyChartStage LoadSurveyChartStage()
        {
            int value = PlayerPrefs.GetInt(StorageKey(SurveyChartStageKey), (int)SurveyChartStage.Locked);
            return value >= (int)SurveyChartStage.Locked && value <= (int)SurveyChartStage.Complete
                ? (SurveyChartStage)value
                : SurveyChartStage.Locked;
        }

        public static void SaveSurveyChartStage(SurveyChartStage stage)
        {
            PlayerPrefs.SetInt(StorageKey(SurveyChartStageKey), (int)stage);
            PlayerPrefs.Save();
        }

        public static StormSignalStage LoadStormSignalStage()
        {
            int value = PlayerPrefs.GetInt(StorageKey(StormSignalStageKey), (int)StormSignalStage.Locked);
            return value >= (int)StormSignalStage.Locked && value <= (int)StormSignalStage.Complete
                ? (StormSignalStage)value
                : StormSignalStage.Locked;
        }

        public static void SaveStormSignalStage(StormSignalStage stage)
        {
            PlayerPrefs.SetInt(StorageKey(StormSignalStageKey), (int)stage);
            PlayerPrefs.Save();
        }

        // 0 = north / harbour. Missing keys start east so old search saves can solve the new step.
        public static int LoadStormVaneDirection() => Mathf.Clamp(PlayerPrefs.GetInt(StorageKey(StormVaneDirectionKey), 1), 0, 3);

        public static void SaveStormVaneDirection(int direction)
        {
            PlayerPrefs.SetInt(StorageKey(StormVaneDirectionKey), Mathf.Clamp(direction, 0, 3));
            PlayerPrefs.Save();
        }

        public static bool HasReadTideClue => PlayerPrefs.GetInt(StorageKey(TideClueReadKey), 0) == 1;

        public static void SaveTideClueRead()
        {
            PlayerPrefs.SetInt(StorageKey(TideClueReadKey), 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// The lost-chart quest's ruin inscription. Deliberately a separate key: the
        /// SurveyChartStage integers are a saved-game contract, so a new step must not
        /// be inserted into that enum. An old save without this key reads as "not read yet".
        /// </summary>
        public static bool HasReadSurveyClue => PlayerPrefs.GetInt(StorageKey(SurveyClueReadKey), 0) == 1;

        public static void SaveSurveyClueRead()
        {
            PlayerPrefs.SetInt(StorageKey(SurveyClueReadKey), 1);
            PlayerPrefs.Save();
        }

        /// <summary>How many commissions the harbour has actually been changed by (0-3).</summary>
        public static int HarbourProsperity => Mathf.Clamp(PlayerPrefs.GetInt(StorageKey(HarbourProsperityKey), 0), 0, 3);

        public static void SetHarbourProsperity(int value)
        {
            PlayerPrefs.SetInt(StorageKey(HarbourProsperityKey), Mathf.Clamp(value, 0, 3));
            PlayerPrefs.Save();
        }

        public static bool HasHarbourRouteArchive => PlayerPrefs.GetInt(StorageKey(HarbourRouteArchiveKey), 0) == 1;

        public static void UnlockHarbourRouteArchive()
        {
            PlayerPrefs.SetInt(StorageKey(HarbourRouteArchiveKey), 1);
            PlayerPrefs.Save();
        }

        public static void ClearHarbourRouteArchive()
        {
            PlayerPrefs.DeleteKey(StorageKey(HarbourRouteArchiveKey));
            PlayerPrefs.Save();
        }

        public static void ClearPrototypeProgress()
        {
            PlayerPrefs.DeleteKey(StorageKey(QuestStageKey));
            PlayerPrefs.DeleteKey(StorageKey(SurveySailKey));
            PlayerPrefs.DeleteKey(StorageKey(TideBeaconProgressKey));
            PlayerPrefs.DeleteKey(StorageKey(SurveyChartStageKey));
            PlayerPrefs.DeleteKey(StorageKey(StormSignalStageKey));
            PlayerPrefs.DeleteKey(StorageKey(StormVaneDirectionKey));
            PlayerPrefs.DeleteKey(StorageKey(TideClueReadKey));
            PlayerPrefs.DeleteKey(StorageKey(SurveyClueReadKey));
            PlayerPrefs.DeleteKey(StorageKey(HarbourProsperityKey));
            PlayerPrefs.DeleteKey(StorageKey(HarbourRouteArchiveKey));
            PlayerPrefs.Save();
        }
    }
}
