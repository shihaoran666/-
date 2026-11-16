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
            PlayerPrefs.DeleteKey(key);
            ProgressStore.SaveStormSignalStage(StormSignalStage.SearchTideflats);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.SearchTideflats ||
                ProgressStore.LoadStormVaneDirection() != 1 ||
                (int)StormSignalStage.Locked != 0 || (int)StormSignalStage.SearchTideflats != 1 ||
                (int)StormSignalStage.ReturnToShipwright != 2 || (int)StormSignalStage.Complete != 3)
            {
                Fail("old search stage or default vane direction changed");
                yield break;
            }

            ProgressStore.SaveStormSignalStage(StormSignalStage.ReturnToShipwright);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.ReturnToShipwright)
            {
                Fail("old return stage changed");
                yield break;
            }
            ProgressStore.SaveStormSignalStage(StormSignalStage.Complete);
            if (ProgressStore.LoadStormSignalStage() != StormSignalStage.Complete)
            {
                Fail("old completed stage changed");
                yield break;
            }
            ProgressStore.SaveStormSignalStage(StormSignalStage.Locked);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log("[FoglightIsles Storm Old Save] PASS — pre-vane search, return and complete stages remain loadable.");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError("[FoglightIsles Storm Old Save] FAIL — " + reason);
            Application.Quit(1);
        }
    }
}
