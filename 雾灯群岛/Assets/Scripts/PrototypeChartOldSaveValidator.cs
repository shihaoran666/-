using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>
    /// Covers the two saved-game cases the ruin-clue step introduces, in read-only fashion:
    /// a save written before the clue key existed must still load its stage and behave sanely,
    /// and a stage already past the search must never be pushed back into it.
    /// Run with <c>-verify-chart-old-save</c>. Writes and restores only the validation scope.
    /// </summary>
    public sealed class PrototypeChartOldSaveValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-chart-old-save";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(Verify());
        }

        private IEnumerator Verify()
        {
            yield return null;

            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            if (chart == null)
            {
                Fail("survey chart quest is missing");
                yield break;
            }

            // ---- Case 1: pre-change save sitting in SearchRuin with no clue key at all ----
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(ProgressStore.SurveyClueReadKey));
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.SearchRuin);
            PlayerPrefs.Save();

            if (ProgressStore.LoadSurveyChartStage() != SurveyChartStage.SearchRuin)
            {
                Fail("a pre-change SearchRuin save no longer loads its stage after the layout change");
                yield break;
            }

            if (ProgressStore.HasReadSurveyClue)
            {
                Fail("a save without the clue key reported the clue as already read");
                yield break;
            }

            // The enum integers are the saved-game contract: prove they did not move.
            if ((int)SurveyChartStage.Locked != 0 || (int)SurveyChartStage.SearchRuin != 1 ||
                (int)SurveyChartStage.ReturnToHarbour != 2 || (int)SurveyChartStage.Complete != 3)
            {
                Fail("SurveyChartStage integer values changed; existing saves would be misread");
                yield break;
            }

            // ---- Case 2: a save already past the search must not re-enter it ----
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.ReturnToHarbour);
            ProgressStore.SaveSurveyClueRead();
            if (ProgressStore.LoadSurveyChartStage() != SurveyChartStage.ReturnToHarbour)
            {
                Fail("a ReturnToHarbour save did not keep its stage");
                yield break;
            }

            // ---- Case 3: completing a save that never read the clue is allowed ----
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.Complete);
            if (ProgressStore.LoadSurveyChartStage() != SurveyChartStage.Complete)
            {
                Fail("a Complete save did not keep its stage");
                yield break;
            }

            // ---- Restore the validation scope so the next run starts clean ----
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(ProgressStore.SurveyClueReadKey));
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.Locked);
            PlayerPrefs.Save();

            Debug.Log("[FoglightIsles Chart Old Save] PASS — pre-change saves keep their stage, the missing clue key reads as unread, " +
                      "stage integers are unchanged, and later stages never fall back to SearchRuin.");
            Application.Quit(0);
        }

        private void Fail(string reason)
        {
            Debug.LogError("[FoglightIsles Chart Old Save] FAIL — " + reason);
            Application.Quit(1);
        }
    }
}
