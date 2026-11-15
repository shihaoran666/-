using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace FoglightIsles
{
    /// <summary>Exercises the title, walking, boarding and voyage through actual gamepad bindings.</summary>
    public sealed class PrototypeGamepadVoyageValidator : MonoBehaviour
    {
        private static bool _prepared;
        private bool _failed;

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-voyage") < 0) return;
            Application.runInBackground = true;
            if (!_prepared)
            {
                _prepared = true;
                ProgressStore.ClearPrototypeProgress();
                DiscoveryJournal.ClearProgress();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            StartCoroutine(VerifyVoyage());
        }

        private IEnumerator VerifyVoyage()
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            QuestState quest = FindFirstObjectByType<QuestState>();
            if (player == null || boat == null || quest == null || !StartScreen.IsOpen ||
                quest.Stage != PrismQuestStage.NotAccepted ||
                FindFirstObjectByType<DiscoveryJournal>()?.Count != 0)
            { Fail("fresh title or controllers are missing"); yield break; }
            Gamepad pad = InputSystem.AddDevice<Gamepad>();
            yield return PressA(pad);
            if (StartScreen.IsOpen || Time.timeScale == 0f)
            { Fail("gamepad A did not open exploration"); yield break; }

            Vector3 spawn = player.transform.position;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(.1f, .1f) });
            for (int frame = 0; frame < 30; frame++) yield return null;
            Vector3 drift = player.transform.position - spawn;
            if (new Vector2(drift.x, drift.z).magnitude > .12f)
            { Fail("small left-stick drift moved the player"); yield break; }
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = Vector2.up });
            bool foundBoard = false;
            for (int frame = 0; frame < 240; frame++)
            {
                yield return null;
                if (player.FindNearbyInteractable()?.Title != "港口告示牌") continue;
                foundBoard = true;
                break;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            if (!foundBoard || Vector3.Distance(spawn, player.transform.position) < 2f)
            { Fail("left stick did not walk to the notice board"); yield break; }
            yield return PressA(pad);
            if (quest.Stage != PrismQuestStage.SearchTideflats)
            { Fail("gamepad A did not accept the first quest"); yield break; }

            yield return Walk(player, pad, new Vector2(-2.8f, 4.4f), "harbour path");
            if (_failed) yield break;
            yield return Walk(player, pad, new Vector2(0f, 2.5f), "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, pad, new Vector2(0f, -1.6f), "harbour boarding point");
            if (_failed) yield break;
            if (player.FindNearbyInteractable()?.Title != "晨湾港码头")
            { Fail("harbour dock was not selected after gamepad walking"); yield break; }
            yield return PressA(pad);
            if (!player.IsOnBoat || !boat.HasCaptain)
            { Fail("gamepad A did not board the boat"); yield break; }

            float startZ = boat.transform.position.z;
            float startYaw = boat.transform.eulerAngles.y;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(.55f, 1f) });
            for (int frame = 0; frame < 60; frame++) yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            if (boat.Speed < .5f || boat.transform.position.z >= startZ - .25f ||
                Mathf.Abs(Mathf.DeltaAngle(startYaw, boat.transform.eulerAngles.y)) < 1f)
            { Fail("left stick did not propel and steer the boat"); yield break; }
            DockingPoint tideflats = null;
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == "潮汐浅滩码头") tideflats = dock;
            if (tideflats == null) { Fail("Tideflats dock is missing"); yield break; }

            bool arrived = false;
            for (int frame = 0; frame < 1800; frame++)
            {
                Vector3 toDock = tideflats.transform.position - boat.transform.position;
                toDock.y = 0f;
                float distance = toDock.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f)
                { arrived = true; break; }
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                float throttle = distance > stoppingDistance ? 1f : -1f;
                float heading = Vector3.SignedAngle(boat.transform.forward, toDock, Vector3.up);
                bool steer = Mathf.Abs(heading) > 3f &&
                             frame % 10 < Mathf.CeilToInt(Mathf.Clamp01(Mathf.Abs(heading) / 25f) * 10f);
                InputSystem.QueueStateEvent(pad, new GamepadState
                {
                    leftStick = new Vector2(steer ? Mathf.Sign(heading) : 0f, throttle)
                });
                yield return null;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            if (!arrived)
            { Fail($"left stick could not brake at Tideflats (position={boat.transform.position}, speed={boat.Speed:0.00})"); yield break; }
            yield return PressA(pad);
            CharacterController controller = player.GetComponent<CharacterController>();
            for (int frame = 0; frame < 60 && !controller.isGrounded; frame++) yield return null;
            Vector3 tideflatsOffset = player.transform.position - tideflats.DisembarkPosition;
            bool tideflatsHit = Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out RaycastHit landing,
                3f, ~0, QueryTriggerInteraction.Ignore);
            if (player.IsOnBoat || boat.HasCaptain || !controller.isGrounded ||
                new Vector2(tideflatsOffset.x, tideflatsOffset.z).sqrMagnitude > .0225f ||
                !tideflatsHit || landing.collider.gameObject.name != "Tideflats Dock")
            { Fail($"gamepad A did not land on the solid Tideflats pier (onBoat={player.IsOnBoat}, captain={boat.HasCaptain}, grounded={controller.isGrounded}, offset={tideflatsOffset}, firstHit={landing.collider?.gameObject.name ?? "none"})"); yield break; }
            if (player.FindNearbyInteractable()?.Title != "潮汐浅滩码头")
            { Fail("Tideflats boarding point was not selected after landing"); yield break; }
            yield return PressA(pad);
            if (!player.IsOnBoat || !boat.HasCaptain)
            { Fail("gamepad A did not reboard at Tideflats"); yield break; }
            DockingPoint harbour = null;
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == "晨湾港码头") harbour = dock;
            if (harbour == null) { Fail("Morning Bay dock is missing"); yield break; }
            bool turned = false;
            for (int frame = 0; frame < 1200; frame++)
            {
                Vector3 toHarbour = harbour.transform.position - boat.transform.position;
                toHarbour.y = 0f;
                float heading = Vector3.SignedAngle(boat.transform.forward, toHarbour, Vector3.up);
                if (Mathf.Abs(heading) < 3f) { turned = true; break; }
                InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(Mathf.Sign(heading), 0f) });
                yield return null;
            }
            if (!turned) { Fail("left stick could not turn boat toward Morning Bay"); yield break; }
            bool returned = false;
            for (int frame = 0; frame < 1800; frame++)
            {
                Vector3 toDock = harbour.transform.position - boat.transform.position;
                toDock.y = 0f;
                float distance = toDock.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f)
                { returned = true; break; }
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                float throttle = distance > stoppingDistance ? 1f : -1f;
                float heading = Vector3.SignedAngle(boat.transform.forward, toDock, Vector3.up);
                bool steer = Mathf.Abs(heading) > 3f &&
                             frame % 10 < Mathf.CeilToInt(Mathf.Clamp01(Mathf.Abs(heading) / 25f) * 10f);
                InputSystem.QueueStateEvent(pad, new GamepadState
                {
                    leftStick = new Vector2(steer ? Mathf.Sign(heading) : 0f, throttle)
                });
                yield return null;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            if (!returned)
            { Fail($"left stick could not brake at Morning Bay (position={boat.transform.position}, speed={boat.Speed:0.00})"); yield break; }
            yield return PressA(pad);
            for (int frame = 0; frame < 60 && !controller.isGrounded; frame++) yield return null;
            Vector3 harbourOffset = player.transform.position - harbour.DisembarkPosition;
            bool harbourHit = Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out RaycastHit harbourLanding,
                3f, ~0, QueryTriggerInteraction.Ignore);
            if (player.IsOnBoat || boat.HasCaptain || !controller.isGrounded ||
                new Vector2(harbourOffset.x, harbourOffset.z).sqrMagnitude > .0225f ||
                !harbourHit || harbourLanding.collider.gameObject.name != "Main Dock")
            { Fail($"gamepad A did not land on the solid Morning Bay pier (onBoat={player.IsOnBoat}, captain={boat.HasCaptain}, grounded={controller.isGrounded}, offset={harbourOffset}, firstHit={harbourLanding.collider?.gameObject.name ?? "none"})"); yield break; }
            InputSystem.RemoveDevice(pad);
            Debug.Log("[FoglightIsles Gamepad Voyage] PASS — gamepad A and left stick accepted the quest, sailed to Tideflats, reboarded, returned and landed at Morning Bay.");
            Application.Quit(0);
        }

        private IEnumerator Walk(PlayerController player, Gamepad pad, Vector2 waypoint, string label)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = Vector2.up });
            for (int frame = 0; frame < 240; frame++)
            {
                Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                Vector2 direction = waypoint - current;
                if (direction.magnitude < .65f)
                {
                    InputSystem.QueueStateEvent(pad, new GamepadState());
                    yield return null;
                    yield break;
                }
                player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                yield return null;
            }
            Fail($"left stick could not reach {label} from {player.transform.position}");
        }

        private static IEnumerator PressA(Gamepad pad)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.South));
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
        }

        private void Fail(string reason)
        {
            _failed = true;
            Debug.LogError("[FoglightIsles Gamepad Voyage] FAIL — " + reason + ".");
            Application.Quit(1);
        }
    }
}
