using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Continues the saved demo with a real keyboard route through the storm lens quest.</summary>
    public sealed class PrototypeKeyboardStormValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-keyboard-storm-quest";
        private const string ResumeArgument = "-verify-keyboard-storm-resume";
        private bool _failed;

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, ResumeArgument) >= 0) StartCoroutine(VerifyResume());
            else if (Array.IndexOf(args, VerifyArgument) >= 0) StartCoroutine(VerifyQuest());
        }

        private IEnumerator VerifyResume()
        {
            yield return null;
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            Light signal = GameObject.Find("Harbour Signal Light")?.GetComponent<Light>();
            if (storm == null || boat == null || storm.Stage != StormSignalStage.Complete ||
                ProgressStore.LoadStormSignalStage() != StormSignalStage.Complete ||
                FindFirstObjectByType<SurveyChartQuest>()?.Stage != SurveyChartStage.Complete ||
                FindFirstObjectByType<QuestState>()?.Stage != PrismQuestStage.Complete ||
                !ProgressStore.HasSurveySail || boat.MaxForwardSpeed != 13.5f || boat.TurnSpeed != 60f ||
                GameObject.Find("Harbour Chart Display") == null ||
                GameObject.Find("Harbour Signal Lamp") == null || signal == null || !signal.enabled ||
                GameObject.Find("风暴透镜") != null || FindFirstObjectByType<GuidanceMarker>()?.Target != null)
            {
                Fail("storm lens hand-in, signal lamp or free exploration state did not restore after restart");
                yield break;
            }
            Debug.Log("[FoglightIsles Keyboard Storm Resume] PASS — storm lens completion, lit signal lamp and free exploration restored after restart.");
            Application.Quit(0);
        }

        private IEnumerator VerifyQuest()
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            if (player == null || boat == null || storm == null || !StartScreen.IsOpen ||
                FindFirstObjectByType<QuestState>()?.Stage != PrismQuestStage.Complete ||
                FindFirstObjectByType<SurveyChartQuest>()?.Stage != SurveyChartStage.Complete ||
                !ProgressStore.HasSurveySail || storm.Stage != StormSignalStage.Locked ||
                boat.MaxForwardSpeed != 13.5f || boat.TurnSpeed != 60f)
            {
                Fail("storm quest requires the saved chart quest; run -verify-keyboard-chart-quest first");
                yield break;
            }
            bool capture = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-keyboard-storm") >= 0;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (StartScreen.IsOpen) { Fail("Enter did not open the saved game"); yield break; }

            yield return Walk(player, keyboard, new Vector2(2.4f, 8.5f), "乔恩，船匠", "Jon");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (storm.Stage != StormSignalStage.SearchTideflats || !DialoguePanel.IsOpen)
            { Fail("E did not accept the storm lens quest"); yield break; }
            if (capture) yield return Capture("review-keyboard-storm-accepted.png");
            yield return PressE(keyboard);
            if (DialoguePanel.IsOpen) { Fail("E did not close Jon's storm lens dialogue"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 8.5f), null, "harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, -1.6f), "晨湾港码头", "Morning Bay pier");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!player.IsOnBoat || !boat.HasCaptain) { Fail("E did not board the upgraded boat"); yield break; }

            DockingPoint tideflats = FindDock("潮汐浅滩码头");
            DockingPoint harbour = FindDock("晨湾港码头");
            if (tideflats == null || harbour == null) { Fail("a quest dock is missing"); yield break; }
            yield return Sail(boat, keyboard, tideflats, "Tideflats");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-storm-arrival.png");
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain ||
                Vector3.Distance(player.transform.position, tideflats.DisembarkPosition) > .05f)
            { Fail("E did not land at Tideflats"); yield break; }
            GuidanceMarker guidance = FindFirstObjectByType<GuidanceMarker>();
            if (guidance?.Target?.name != "风向标")
            { Fail("the compass did not point to the storm vane after landing"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, -78f), null, "island path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3f, -85f), null, "collapsed tower approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-4.6f, -89.8f), "风暴透镜", "storm lens");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (storm.Stage != StormSignalStage.SearchTideflats || GameObject.Find("风暴透镜") == null)
            { Fail("the storm lens was removable before calibrating the vane"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-3f, -85f), null, "tower return path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-7f, -85.8f), "风向标", "storm vane");
            if (_failed) yield break;
            for (int turn = 0; turn < 3; turn++) yield return PressE(keyboard);
            if (!storm.VaneAligned || ProgressStore.LoadStormVaneDirection() != 0)
            { Fail("the storm vane did not align north after three turns"); yield break; }
            if (guidance.Target?.name != "风暴透镜")
            { Fail("the compass did not switch to the storm lens after calibration"); yield break; }
            yield return Walk(player, keyboard, new Vector2(-3f, -85f), null, "tower approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-4.6f, -89.8f), "风暴透镜", "storm lens");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-storm-found.png");
            yield return PressE(keyboard);
            if (storm.Stage != StormSignalStage.ReturnToShipwright || GameObject.Find("风暴透镜") != null)
            { Fail("E did not collect the storm lens"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, -78f), null, "island return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, -71f), null, "dock approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(1.4f, -66.5f), "潮汐浅滩码头", "Tideflats pier");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!player.IsOnBoat || !boat.HasCaptain) { Fail("E did not reboard at Tideflats"); yield break; }
            yield return TurnTo(boat, keyboard, harbour);
            if (_failed) yield break;
            yield return Sail(boat, keyboard, harbour, "Morning Bay");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-storm-return.png");
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain ||
                Vector3.Distance(player.transform.position, harbour.DisembarkPosition) > .05f)
            { Fail("E did not land at Morning Bay"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 8.5f), null, "harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(2.4f, 8.5f), "乔恩，船匠", "Jon hand-in");
            if (_failed) yield break;
            yield return PressE(keyboard);
            Light signal = GameObject.Find("Harbour Signal Light")?.GetComponent<Light>();
            if (storm.Stage != StormSignalStage.Complete || !DialoguePanel.IsOpen ||
                GameObject.Find("Harbour Signal Lamp") == null || signal == null || !signal.enabled ||
                FindFirstObjectByType<GuidanceMarker>()?.Target != null)
            { Fail("E did not hand the lens to Jon, light the signal mast and clear guidance"); yield break; }
            if (capture) yield return Capture("review-keyboard-storm-hand-in.png");
            yield return PressE(keyboard);
            if (!DialoguePanel.IsOpen) { Fail("completion dialogue lost its free exploration line"); yield break; }
            if (capture) yield return Capture("review-keyboard-storm-complete.png");
            yield return PressE(keyboard);
            if (DialoguePanel.IsOpen) { Fail("E did not close the completion dialogue"); yield break; }
            Vector3 beforeFreeWalk = player.transform.position;
            player.transform.rotation = Quaternion.LookRotation(Vector3.left);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            for (int frame = 0; frame < 25; frame++) yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (Vector3.Distance(beforeFreeWalk, player.transform.position) < .5f)
            { Fail("free exploration movement did not resume after completion"); yield break; }
            if (capture) { yield return Capture("review-keyboard-storm-free-roam.png"); yield return new WaitForSecondsRealtime(2f); }
            InputSystem.RemoveDevice(keyboard);
            Debug.Log("[FoglightIsles Keyboard Storm] PASS — accepted the lens quest, sailed to Tideflats, returned it to Jon, lit the signal mast and resumed free exploration by keyboard.");
            Application.Quit(0);
        }

        private IEnumerator Walk(PlayerController player, Keyboard keyboard, Vector2 point, string interaction, string label)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            for (int frame = 0; frame < 540; frame++)
            {
                Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                Vector2 direction = point - current;
                if (direction.magnitude < .65f &&
                    (interaction == null || player.FindNearbyInteractable()?.Title == interaction))
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    yield return null;
                    yield break;
                }
                player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                yield return null;
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            Fail($"keyboard walk could not reach {label} from {player.transform.position}");
        }

        private static IEnumerator PressE(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        private IEnumerator TurnTo(BoatController boat, Keyboard keyboard, DockingPoint target)
        {
            for (int frame = 0; frame < 1200; frame++)
            {
                Vector3 direction = target.transform.position - boat.transform.position;
                direction.y = 0f;
                float heading = Vector3.SignedAngle(boat.transform.forward, direction, Vector3.up);
                if (Mathf.Abs(heading) < 3f)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    yield return null;
                    yield break;
                }
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(heading > 0f ? Key.D : Key.A));
                yield return null;
            }
            Fail("keyboard could not turn the upgraded boat toward Morning Bay");
        }

        private IEnumerator Sail(BoatController boat, Keyboard keyboard, DockingPoint target, string label)
        {
            for (int frame = 0; frame < 1800; frame++)
            {
                Vector3 direction = target.transform.position - boat.transform.position;
                direction.y = 0f;
                float distance = direction.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    yield return null;
                    yield break;
                }
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                Key throttle = distance > stoppingDistance ? Key.W : Key.S;
                float heading = Vector3.SignedAngle(boat.transform.forward, direction, Vector3.up);
                bool steer = Mathf.Abs(heading) > 3f &&
                             frame % 10 < Mathf.CeilToInt(Mathf.Clamp01(Mathf.Abs(heading) / 25f) * 10f);
                InputSystem.QueueStateEvent(keyboard, steer
                    ? new KeyboardState(throttle, heading > 0f ? Key.D : Key.A)
                    : new KeyboardState(throttle));
                yield return null;
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            Fail($"keyboard could not brake the upgraded boat at {label} (position={boat.transform.position}, speed={boat.Speed:0.00})");
        }

        private static IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../" + name));
        }

        private static DockingPoint FindDock(string name)
        {
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == name) return dock;
            return null;
        }

        private void Fail(string reason)
        {
            _failed = true;
            Debug.LogError("[FoglightIsles Keyboard Storm] FAIL — " + reason + ".");
            Application.Quit(1);
        }
    }
}
