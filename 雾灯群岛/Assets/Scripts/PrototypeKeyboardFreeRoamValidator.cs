using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace FoglightIsles
{
    /// <summary>Checks that all optional discoveries are reachable after the main story.</summary>
    public sealed class PrototypeKeyboardFreeRoamValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-keyboard-free-roam";
        private const string ResumeArgument = "-verify-keyboard-free-roam-resume";
        private static bool _prepared;
        private bool _failed;

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, ResumeArgument) >= 0) { StartCoroutine(VerifyResume()); return; }
            if (Array.IndexOf(args, VerifyArgument) < 0) return;
            if (!_prepared)
            {
                _prepared = true;
                DiscoveryJournal.ClearProgress();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            StartCoroutine(VerifyJourney());
        }

        private IEnumerator VerifyResume()
        {
            yield return null;
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            if (journal == null || journal.Count != journal.Total || journal.Total != 4 ||
                FindFirstObjectByType<StormSignalQuest>()?.Stage != StormSignalStage.Complete ||
                FindFirstObjectByType<GuidanceMarker>()?.Target != null)
            { Fail("free-roam discoveries or completed quest state did not restore after restart"); yield break; }
            foreach (DiscoveryJournal.Entry entry in DiscoveryJournal.Catalog)
                if (!journal.IsDiscovered(entry.Id) || GameObject.Find("发现物 - " + entry.Title) != null)
                { Fail("a discovered item is visible again after restart"); yield break; }
            Debug.Log("[FoglightIsles Keyboard Free Roam Resume] PASS — four discovery notes restored and world pickups stayed hidden after restart.");
            Application.Quit(0);
        }

        private IEnumerator VerifyJourney()
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            if (player == null || boat == null || journal == null || !StartScreen.IsOpen ||
                journal.Count != 0 || journal.Total != 4 ||
                FindFirstObjectByType<StormSignalQuest>()?.Stage != StormSignalStage.Complete ||
                FindFirstObjectByType<SurveyChartQuest>()?.Stage != SurveyChartStage.Complete ||
                FindFirstObjectByType<GuidanceMarker>()?.Target != null)
            { Fail("free-roam run requires the saved completed main story and four fresh discoveries"); yield break; }
            bool capture = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-keyboard-free-roam") >= 0;
            if (capture && Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-small-review") >= 0)
            {
                Screen.SetResolution(640, 360, FullScreenMode.Windowed);
                yield return new WaitUntil(() => Screen.width == 640 && Screen.height == 360);
                Debug.Log($"[FoglightIsles Keyboard Free Roam] Capture resolution {Screen.width}x{Screen.height}.");
            }
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            // Title input is covered separately. A queued Enter can be consumed before
            // StartScreen.Update on some player frames, so isolate this journey from it.
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (StartScreen.IsOpen || !PrototypeHUD.CurrentMessage.Contains("自由探索"))
            { Fail("could not resume free exploration with saved-story welcome text"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 5f), null, "south harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "west harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-7.4f, 7.1f), null, "Eileen path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-10.2f, 9.2f), "潮湿的港口日志", "harbour log");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-roam-harbour-log.png");
            yield return PressE(keyboard);
            if (journal.Count != 1 || GameObject.Find("发现物 - 潮湿的港口日志") != null)
            { Fail("E did not record the harbour log"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-7.4f, 7.1f), null, "west harbour return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "harbour square");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 7.5f), null, "east harbour path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(5f, 9.5f), "褪色的旧罗盘", "old compass");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-roam-old-compass.png");
            yield return PressE(keyboard);
            if (journal.Count != 2 || GameObject.Find("发现物 - 褪色的旧罗盘") != null)
            { Fail("E did not record the old compass"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 8.5f), null, "main pier approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier entrance");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, -1.6f), "晨湾港码头", "Morning Bay dock");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!player.IsOnBoat || !boat.HasCaptain) { Fail("E did not board for free exploration"); yield break; }
            DockingPoint tideflats = FindDock("潮汐浅滩码头");
            DockingPoint harbour = FindDock("晨湾港码头");
            if (tideflats == null || harbour == null) { Fail("free-roam docks are missing"); yield break; }
            yield return Sail(boat, keyboard, tideflats, "Tideflats");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain) { Fail("E did not land for island free exploration"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-3f, -72.2f), null, "clue path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-10f, -77f), null, "west island path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-13f, -82f), null, "west shell path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-13.8f, -83.4f), "会回响的潮汐贝壳", "tide shell");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-roam-tide-shell.png");
            yield return PressE(keyboard);
            if (journal.Count != 3 || GameObject.Find("发现物 - 会回响的潮汐贝壳") != null)
            { Fail("E did not record the tide shell"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-10f, -81f), null, "west island return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, -79f), null, "centre island path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(5f, -78f), null, "east ruin path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(12f, -82f), null, "east island path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(14.5f, -87.2f), "守望者护符", "watchers charm");
            if (_failed) yield break;
            if (capture) yield return Capture("review-keyboard-roam-watchers-charm.png");
            yield return PressE(keyboard);
            if (journal.Count != 4 || GameObject.Find("发现物 - 守望者护符") != null)
            { Fail("E did not record the watchers charm"); yield break; }
            if (capture) yield return Capture("review-keyboard-roam-complete.png");

            yield return Walk(player, keyboard, new Vector2(12f, -82f), null, "east island return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(5f, -78f), null, "ruin return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, -71f), null, "dock approach");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(1.4f, -66.5f), "潮汐浅滩码头", "Tideflats dock");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!player.IsOnBoat || !boat.HasCaptain) { Fail("E did not reboard after island discoveries"); yield break; }
            yield return TurnTo(boat, keyboard, harbour);
            if (_failed) yield break;
            yield return Sail(boat, keyboard, harbour, "Morning Bay");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (player.IsOnBoat || boat.HasCaptain) { Fail("E did not return to Morning Bay"); yield break; }

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (!QuestJournalPanel.IsOpen || Time.timeScale != 0f)
            { Fail("J did not open and pause the journal"); yield break; }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (capture) yield return Capture("review-keyboard-roam-journal.png");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (QuestJournalPanel.IsOpen || Time.timeScale == 0f)
            { Fail("J did not close the journal and resume time"); yield break; }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.M));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (!MapOverlay.IsOpen || Time.timeScale != 0f)
            { Fail("M did not open and pause the chart"); yield break; }
            if (capture) yield return Capture("review-keyboard-roam-map.png");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.M));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            if (MapOverlay.IsOpen || Time.timeScale == 0f)
            { Fail("M did not close the chart and resume time"); yield break; }
            foreach (DiscoveryJournal.Entry entry in DiscoveryJournal.Catalog)
                if (!journal.IsDiscovered(entry.Id)) { Fail("a free-roam note was not recorded"); yield break; }

            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "harbour pier return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "harbour square return");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(-7.4f, 7.1f), "艾琳，港口管理员", "Eileen after four discoveries");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!DialoguePanel.IsOpen || !DialoguePanel.CurrentLine.Contains("日志、罗盘、贝壳和护符"))
            { Fail("Eileen did not acknowledge four discoveries"); yield break; }
            if (capture) yield return Capture("review-keyboard-roam-eileen-complete.png");
            yield return CloseDialogue(keyboard);
            if (DialoguePanel.IsOpen) { Fail("Eileen discovery dialogue did not close"); yield break; }

            yield return Walk(player, keyboard, new Vector2(-3.5f, 5.5f), null, "harbour square eastbound");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 2.5f), null, "main pier eastbound");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(0f, 7.5f), null, "shipwright path");
            if (_failed) yield break;
            yield return Walk(player, keyboard, new Vector2(1.2f, 9.2f), "乔恩，船匠", "Jon after four discoveries");
            if (_failed) yield break;
            yield return PressE(keyboard);
            if (!DialoguePanel.IsOpen || !DialoguePanel.CurrentLine.Contains("四件旧物"))
            { Fail("Jon did not acknowledge four discoveries"); yield break; }
            if (capture) yield return Capture("review-keyboard-roam-jon-complete.png");
            yield return CloseDialogue(keyboard);
            if (DialoguePanel.IsOpen) { Fail("Jon discovery dialogue did not close"); yield break; }

            InputSystem.RemoveDevice(keyboard);
            if (capture) yield return new WaitForSecondsRealtime(2f);
            Debug.Log("[FoglightIsles Keyboard Free Roam] PASS — collected four optional discoveries by walking and sailing, opened journal and map, then heard both harbour NPC responses.");
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

        /// <summary>Advances dialogue until it closes; NPCs may answer with more than one line.</summary>
        private static IEnumerator CloseDialogue(Keyboard keyboard)
        {
            for (int attempt = 0; attempt < 4 && DialoguePanel.IsOpen; attempt++)
                yield return PressE(keyboard);
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
            Debug.LogError("[FoglightIsles Keyboard Free Roam] FAIL — " + reason + ".");
            Application.Quit(1);
        }
    }
}
