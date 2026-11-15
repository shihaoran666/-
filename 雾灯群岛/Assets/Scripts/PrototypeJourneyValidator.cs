using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Runs a non-interactive harbour-to-island-to-harbour check when launched with -verify-journey.</summary>
    public sealed class PrototypeJourneyValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-journey";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(VerifyJourney());
        }

        private static IEnumerator VerifyJourney()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();

            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            DockingPoint harbourDock = FindDock("晨湾港码头");
            DockingPoint tideflatsDock = FindDock("潮汐浅滩码头");
            if (player == null || boat == null || harbourDock == null || tideflatsDock == null)
            {
                Fail("player, boat, or docking point is missing");
                yield break;
            }

            if (!HasSolidLanding(tideflatsDock, "Tideflats Dock") || !HasSolidLanding(harbourDock, "Main Dock"))
            {
                Fail("a disembark point is not supported by its pier");
                yield break;
            }

            if (!TryBoardAtDock(player, boat, harbourDock))
            {
                Fail("the player cannot board from the Morning Bay pier interaction");
                yield break;
            }
            if (!SailToDock(boat, tideflatsDock))
            {
                Fail("the boat could not sail and brake at Tideflats");
                yield break;
            }
            boat.TryDisembark(player);
            if (boat.HasCaptain || Vector3.Distance(player.transform.position, tideflatsDock.DisembarkPosition) > .01f)
            {
                Fail("could not disembark at Tideflats");
                yield break;
            }

            if (!TryBoardAtDock(player, boat, tideflatsDock))
            {
                Fail("the player cannot reboard from the Tideflats pier interaction");
                yield break;
            }
            if (!TurnTowards(boat, harbourDock.transform.position) || !SailToDock(boat, harbourDock))
            {
                Fail("the boat could not turn and sail back to Morning Bay");
                yield break;
            }
            boat.TryDisembark(player);
            if (boat.HasCaptain || Vector3.Distance(player.transform.position, harbourDock.DisembarkPosition) > .01f)
            {
                Fail("could not return to Morning Bay");
                yield break;
            }
            if (!TryBoardAtDock(player, boat, harbourDock))
            {
                Fail("the player cannot reboard after returning to Morning Bay");
                yield break;
            }
            boat.TryDisembark(player);

            Collider seaCollider = GameObject.Find("Mist Sea")?.GetComponent<Collider>();
            player.transform.position = new Vector3(0f, -4f, 0f);
            if (seaCollider == null || seaCollider.enabled || !player.RecoverIfBelowSea() || player.transform.position.y < 0f)
            {
                Fail("sea collision or fall recovery is incorrect");
                yield break;
            }

            boat.transform.SetPositionAndRotation(new Vector3(0f, .38f, -2.8f), Quaternion.identity);
            Physics.SyncTransforms();
            boat.SimulateControl(1f, 0f, 1f);
            if (Mathf.Abs(boat.transform.position.z + 2.8f) > .01f)
            {
                Fail("the boat crossed the Morning Bay shore");
                yield break;
            }

            boat.transform.SetPositionAndRotation(new Vector3(0f, .38f, -63f), Quaternion.Euler(0f, 180f, 0f));
            Physics.SyncTransforms();
            boat.SimulateControl(1f, 0f, 1f);
            if (Mathf.Abs(boat.transform.position.z + 63f) > .01f)
            {
                Fail("the boat crossed the Tideflats shore");
                yield break;
            }

            boat.transform.SetPositionAndRotation(new Vector3(94.8f, .38f, -20f), Quaternion.Euler(0f, 90f, 0f));
            boat.SimulateControl(1f, 0f, 1f);
            if (boat.transform.position.x > 95f)
            {
                Fail("the boat sailed beyond the visible sea edge");
                yield break;
            }
            boat.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            boat.SimulateControl(1f, 0f, 1f);
            if (boat.transform.position.x >= 94.8f)
            {
                Fail("the boat could not steer back from the sea edge");
                yield break;
            }

            Debug.Log("[FoglightIsles Journey] PASS — powered sailing, docking, return loop, shore blocking, sea boundary and fall recovery validated.");
            Application.Quit(0);
        }

        private static bool TurnTowards(BoatController boat, Vector3 position)
        {
            Vector3 direction = position - boat.transform.position;
            direction.y = 0f;
            for (int step = 0; step < 1200; step++)
            {
                if (Vector3.Angle(boat.transform.forward, direction) < 2f) return true;
                float sign = Mathf.Sign(Vector3.SignedAngle(boat.transform.forward, direction, Vector3.up));
                boat.SimulateControl(0f, sign, 1f / 60f);
            }
            return false;
        }

        private static bool SailToDock(BoatController boat, DockingPoint dock)
        {
            for (int step = 0; step < 1800; step++)
            {
                Vector3 toDock = dock.transform.position - boat.transform.position;
                toDock.y = 0f;
                float distance = toDock.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f) return true;
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                float throttle = distance > stoppingDistance ? 1f : -1f;
                float steering = Mathf.Clamp(Vector3.SignedAngle(boat.transform.forward, toDock, Vector3.up) / 25f, -1f, 1f);
                boat.SimulateControl(throttle, steering, 1f / 60f);
            }
            return false;
        }

        private static DockingPoint FindDock(string name)
        {
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == name) return dock;
            return null;
        }

        private static bool HasSolidLanding(DockingPoint dock, string pierName)
        {
            return Physics.Raycast(dock.DisembarkPosition + Vector3.up, Vector3.down, out RaycastHit hit,
                       3f, ~0, QueryTriggerInteraction.Ignore) && hit.collider.gameObject.name == pierName;
        }

        private static bool TryBoardAtDock(PlayerController player, BoatController boat, DockingPoint dock)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = dock.DisembarkPosition;
            controller.enabled = true;
            Physics.SyncTransforms();
            Interactable nearby = player.FindNearbyInteractable();
            if (nearby == null || nearby.gameObject != dock.gameObject) return false;
            nearby.Interact(player);
            return player.IsOnBoat && boat.HasCaptain &&
                   Vector3.Angle(player.transform.forward, boat.transform.forward) < 1f;
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Journey] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
