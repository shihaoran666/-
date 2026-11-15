using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Checks optional discovery collection and persistence when launched with -verify-discoveries.</summary>
    public sealed class PrototypeDiscoveryValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-discoveries";
        private const string ResumeArgument = "-verify-discoveries-resume";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(VerifyDiscoveries());
            else if (Array.IndexOf(Environment.GetCommandLineArgs(), ResumeArgument) >= 0)
                StartCoroutine(VerifyRestoredDiscoveries());
        }

        private static IEnumerator VerifyRestoredDiscoveries()
        {
            yield return null;
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            if (journal == null || journal.Count != journal.Total ||
                FindFirstObjectByType<StartScreen>()?.HasSavedProgress != true)
            {
                Fail("saved discovery count was not restored");
                yield break;
            }
            foreach (DiscoveryJournal.Entry entry in DiscoveryJournal.Catalog)
            {
                if (!journal.IsDiscovered(entry.Id) || string.IsNullOrWhiteSpace(entry.Note))
                {
                    Fail("a saved discovery or its journal note is missing");
                    yield break;
                }
            }
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (!ProgressStore.HasHarbourRouteArchive || bootstrap == null || !bootstrap.IsHarbourRouteArchiveVisible)
            {
                Fail("the unlocked route archive was not restored in a new process");
                yield break;
            }
            Debug.Log("[FoglightIsles Discoveries Resume] PASS — discovery notes and harbour route archive restored in a new process.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyDiscoveries()
        {
            yield return null;
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            DiscoveryPickup[] pickups = FindObjectsByType<DiscoveryPickup>(FindObjectsSortMode.None);
            if (journal == null || pickups.Length != 4)
            {
                Fail("discovery journal or expected pickups are missing");
                yield break;
            }

            foreach (DiscoveryPickup pickup in pickups) pickup.Collect();
            if (journal.Count != journal.Total || journal.Total != pickups.Length)
            {
                Fail("discoveries did not save their collection state");
                yield break;
            }

            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null || ProgressStore.HasHarbourRouteArchive || bootstrap.IsHarbourRouteArchiveVisible)
            {
                Fail("the route archive was visible before the player returned to Eileen");
                yield break;
            }
            bootstrap.TalkToEileen();
            if (!ProgressStore.HasHarbourRouteArchive || !bootstrap.IsHarbourRouteArchiveVisible)
            {
                Fail("the completed discoveries did not unlock the harbour route archive");
                yield break;
            }

            Debug.Log("[FoglightIsles Discoveries] PASS — optional discoveries persist and unlock a visible harbour route archive.");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Discoveries] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
