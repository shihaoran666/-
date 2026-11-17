using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Checks that a landing persists across processes and restores a boardable boat.</summary>
    public sealed class PrototypeSafeDockValidator : MonoBehaviour
    {
        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-verify-safe-dock-prepare") >= 0)
                StartCoroutine(Prepare());
            else if (Array.IndexOf(args, "-verify-safe-dock-resume") >= 0)
                StartCoroutine(Resume());
        }

        private static IEnumerator Prepare()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            DockingPoint dock = FindTideflatsDock();
            if (player == null || boat == null || dock == null || ProgressStore.HasSafeDock)
            {
                Fail("clean starting state or scene objects missing");
                yield break;
            }
            player.BoardBoat(boat);
            boat.transform.SetPositionAndRotation(new Vector3(3.2f, .38f, -62.1f), Quaternion.identity);
            Physics.SyncTransforms();
            boat.TryDisembark(player);
            if (boat.HasCaptain || !ProgressStore.ResumeAtTideflats ||
                Vector3.Distance(player.transform.position, dock.DisembarkPosition) > .01f)
            {
                Fail("Tideflats landing did not save a safe dock");
                yield break;
            }
            Debug.Log("[FoglightIsles Safe Dock Prepare] PASS — Tideflats landing persisted in validation scope.");
            Application.Quit(0);
        }

        private static IEnumerator Resume()
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            DockingPoint dock = FindTideflatsDock();
            Collider pier = GameObject.Find("Tideflats Dock")?.GetComponent<Collider>();
            Collider hull = GameObject.Find("Boat Hull")?.GetComponent<Collider>();
            StartScreen title = FindFirstObjectByType<StartScreen>();
            if (player == null || boat == null || dock == null || title == null || pier == null || hull == null ||
                !ProgressStore.ResumeAtTideflats || !title.HasSavedProgress ||
                Vector3.Distance(player.transform.position, dock.DisembarkPosition) > .01f ||
                Vector3.Distance(boat.transform.position, dock.transform.position) >= DockingPoint.DockingRadius ||
                pier.bounds.Intersects(hull.bounds))
            {
                Fail("reopened game did not restore player and boat together at Tideflats");
                yield break;
            }
            title.BeginExploration();
            dock.GetComponent<Interactable>()?.Interact(player);
            if (!boat.HasCaptain || !player.IsOnBoat)
            {
                Fail("restored boat could not be boarded from the pier");
                yield break;
            }
            Debug.Log("[FoglightIsles Safe Dock Resume] PASS — player and boardable boat restored at Tideflats across processes.");
            Application.Quit(0);
        }

        private static DockingPoint FindTideflatsDock()
        {
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.transform.position.z < -45f) return dock;
            return null;
        }

        private static void Fail(string reason)
        {
            Debug.LogError("[FoglightIsles Safe Dock] FAIL — " + reason);
            Application.Quit(1);
        }
    }
}
