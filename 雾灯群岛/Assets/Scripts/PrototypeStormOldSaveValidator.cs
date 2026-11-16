using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Exercises pre-vane saves in the isolated validation preference scope.</summary>
    public sealed class PrototypeStormOldSaveValidator : MonoBehaviour
    {
        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-storm-old-save") >= 0)
                StartCoroutine(Verify());
        }

        private static IEnumerator Verify()
        {
            yield return null;
            string key = ProgressStore.StorageKey(ProgressStore.StormVaneDirectionKey);
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            if (chart == null)
            {
                Fail("survey chart quest is missing");
                yield break;
            }
            var probeObject = new GameObject("Storm Old Save Probe");
            StormSignalQuest probe = probeObject.AddComponent<StormSignalQuest>();
            PlayerPrefs.DeleteKey(key);
            ProgressStore.SaveStormSignalStage(StormSignalStage.Locked);
            probe.Configure(chart);
            probe.RotateVane();
            if (!PrototypeHUD.CurrentMessage.Contains("先向乔恩接取") || probe.DisplayVaneDirection != 1)
            {
                Fail("unaccepted quest claims the vane is already aligned or rotates it");
                yield break;
            }
            ProgressStore.SaveStormSignalStage(StormSignalStage.SearchTideflats);
            probe.Configure(chart);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.SearchTideflats ||
                ProgressStore.LoadStormVaneDirection() != 1 ||
                probe.VaneAligned || probe.DisplayVaneDirection != 1 ||
                (int)StormSignalStage.Locked != 0 || (int)StormSignalStage.SearchTideflats != 1 ||
                (int)StormSignalStage.ReturnToShipwright != 2 || (int)StormSignalStage.Complete != 3)
            {
                Fail("old search stage or default vane direction changed");
                yield break;
            }

            ProgressStore.SaveStormSignalStage(StormSignalStage.ReturnToShipwright);
            probe.Configure(chart);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.ReturnToShipwright ||
                !probe.VaneAligned || probe.DisplayVaneDirection != 0)
            {
                Fail("old return stage changed");
                yield break;
            }
            ProgressStore.SaveStormSignalStage(StormSignalStage.Complete);
            probe.Configure(chart);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.Complete ||
                !probe.VaneAligned || probe.DisplayVaneDirection != 0)
            {
                Fail("old completed stage changed");
                yield break;
            }
            ProgressStore.SaveStormSignalStage(StormSignalStage.Locked);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            UnityEngine.Object.Destroy(probeObject);
            Debug.Log("[FoglightIsles Storm Old Save] PASS — pre-vane search remains solvable, late stages display north, and locked interaction gives the correct hint.");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError("[FoglightIsles Storm Old Save] FAIL — " + reason);
            Application.Quit(1);
        }
    }
}
