using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Continues the saved demo with a real keyboard route through the chart quest.</summary>
    public sealed class PrototypeKeyboardChartValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-keyboard-chart-quest";
        private const string ResumeArgument = "-verify-keyboard-chart-resume";
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
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            if (chart == null || boat == null || chart.Stage != SurveyChartStage.Complete ||
                ProgressStore.LoadSurveyChartStage() != SurveyChartStage.Complete ||
                FindFirstObjectByType<QuestState>()?.Stage != PrismQuestStage.Complete ||
                !ProgressStore.HasSurveySail || boat.MaxForwardSpeed != 13.5f || boat.TurnSpeed != 60f ||
                GameObject.Find("Harbour Chart Display") == null || GameObject.Find("失落航图") != null)
            {
                Fail("chart hand-in, harbour display or boat upgrade did not restore after restart");
                yield break;
            }
            Debug.Log("[FoglightIsles Keyboard Chart Resume] PASS — chart completion, harbour display and upgraded boat restored after restart.");
            Application.Quit(0);
        }

        private IEnumerator VerifyQuest()
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            if (player == null || boat == null || chart == null || !StartScreen.IsOpen ||
                FindFirstObjectByType<QuestState>()?.Stage != PrismQuestStage.Complete ||
                !ProgressStore.HasSurveySail || chart.Stage != SurveyChartStage.Locked ||
                boat.MaxForwardSpeed != 13.5f || boat.TurnSpeed != 60f)
            {
                Fail("chart quest requires the saved first quest and survey sail; run -verify-keyboard-first-quest first");
                yield break;
            }
            bool capture = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-keyboard-chart") >= 0;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (StartScreen.IsOpen) { Fail("Enter did not open the saved game"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 5f), null, "south harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "Eileen approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-7.4f, 7.1f), "艾琳，港口管理员", "Eileen");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (chart.Stage != SurveyChartStage.SearchRuin || !DialoguePanel.IsOpen)
            { Fail("E did not accept the lost chart quest"); yield break; }
            if (capture) yield return Capture("review-keyboard-chart-accepted.png");
            yield return PressE(keyboard);
            if (DialoguePanel.IsOpen) { Fail("E did not close Eileen's chart dialogue"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "harbour path");
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
            if (capture) yield return Capture("review-keyboard-chart-arrival.png");
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain ||
                Vector3.Distance(player.transform.position, tideflats.DisembarkPosition) > .05f)
            { Fail("E did not land at Tideflats"); yield break; }

            yield return Walk(player, keyboard, new Vector2(3.5f, -72.2f), null, "island sign");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(5f, -78f), null, "ruin entrance");
            if (_failed) yield break;
            // The chart is gated on the ruin inscription: pressing E before reading it must
            // change nothing and must not remove the object from the table.
            yield return Walk(player, keyboard, new Vector2(8.3f, -82.3f), "失落航图", "lost chart");
            if (_failed) yield break;
            if (ProgressStore.HasReadSurveyClue)
            { Fail("ruin clue read before the chart was reached; the gate cannot be exercised"); yield break; }
            yield return PressE(keyboard);
            if (chart.Stage != SurveyChartStage.SearchRuin || GameObject.Find("失落航图") == null)
            { Fail("E collected the lost chart before the ruin inscription was read"); yield break; }
            if (capture) yield return Capture("review-keyboard-chart-blocked.png");

            yield return Walk(player, keyboard, new Vector2(6.6f, -83.6f), "遗迹残碑", "ruin inscription");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!ProgressStore.HasReadSurveyClue)
            { Fail("E did not read the ruin inscription"); yield break; }

            yield return Walk(player, keyboard, new Vector2(8.3f, -82.3f), "失落航图", "lost chart");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-chart-found.png");
            yield return PressE(keyboard);
            if (chart.Stage != SurveyChartStage.ReturnToHarbour || GameObject.Find("失落航图") != null)
            { Fail("E did not collect the lost chart after reading the inscription"); yield break; }

            yield return Walk(player, keyboard, new Vector2(5f, -78f), null, "ruin return");
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
            if (capture) yield return Capture("review-keyboard-chart-return.png");
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain ||
                Vector3.Distance(player.transform.position, harbour.DisembarkPosition) > .05f)
            { Fail("E did not land at Morning Bay"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-7.4f, 7.1f), "艾琳，港口管理员", "Eileen hand-in");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (chart.Stage != SurveyChartStage.Complete || !DialoguePanel.IsOpen ||
                GameObject.Find("Harbour Chart Display") == null)
            { Fail("E did not hand the chart to Eileen and display it in the harbour"); yield break; }
            if (capture) { yield return Capture("review-keyboard-chart-hand-in.png"); yield return new WaitForSecondsRealtime(2f); }
            InputSystem.RemoveDevice(keyboard);
            Debug.Log("[FoglightIsles Keyboard Chart] PASS — accepted the chart quest, sailed with the upgraded boat, collected the chart and returned it to Eileen by keyboard.");
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
            Debug.LogError("[FoglightIsles Keyboard Chart] FAIL — " + reason + ".");
            Application.Quit(1);
        }
    }
}
