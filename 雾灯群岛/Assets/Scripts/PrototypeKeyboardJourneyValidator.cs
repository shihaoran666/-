using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace FoglightIsles
{
    /// <summary>Runs the title, first quest, sailing and Tideflats puzzle flow through keyboard input.</summary>
    public sealed class PrototypeKeyboardJourneyValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-keyboard-journey";
        private const string ResumeArgument = "-verify-keyboard-island-resume";
        private const string FullQuestArgument = "-verify-keyboard-first-quest";
        private const string FullQuestResumeArgument = "-verify-keyboard-first-quest-resume";
        private static bool _prepared;

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, FullQuestResumeArgument) >= 0)
            {
                StartCoroutine(VerifyFullQuestResume());
                return;
            }
            if (Array.IndexOf(args, ResumeArgument) >= 0)
            {
                StartCoroutine(VerifyIslandResume());
                return;
            }
            bool fullQuest = Array.IndexOf(args, FullQuestArgument) >= 0;
            if (!fullQuest && Array.IndexOf(args, VerifyArgument) < 0) return;
            Application.runInBackground = true;
            if (!_prepared)
            {
                _prepared = true;
                ProgressStore.ClearPrototypeProgress();
                DiscoveryJournal.ClearProgress();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            StartCoroutine(VerifyJourney(fullQuest));
        }

        private static IEnumerator VerifyIslandResume()
        {
            yield return null;
            QuestState quest = FindFirstObjectByType<QuestState>();
            TideBeaconPuzzle puzzle = FindFirstObjectByType<TideBeaconPuzzle>();
            if (quest == null || puzzle == null ||
                quest.Stage != PrismQuestStage.ReturnToHarbour ||
                ProgressStore.LoadQuestStage() != PrismQuestStage.ReturnToHarbour ||
                !puzzle.IsComplete || ProgressStore.LoadTideBeaconProgress() != 3 ||
                GameObject.Find("灯塔棱镜零件") != null)
            {
                Fail("keyboard island progress did not restore after restart");
                yield break;
            }
            Debug.Log("[FoglightIsles Keyboard Island Resume] PASS — prism pickup and three lit beacons restored after restart.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyFullQuestResume()
        {
            yield return null;
            QuestState quest = FindFirstObjectByType<QuestState>();
            TideBeaconPuzzle puzzle = FindFirstObjectByType<TideBeaconPuzzle>();
            Light lighthouse = GameObject.Find("Lighthouse Light")?.GetComponent<Light>();
            if (quest == null || puzzle == null || quest.Stage != PrismQuestStage.Complete ||
                ProgressStore.LoadQuestStage() != PrismQuestStage.Complete ||
                !puzzle.IsComplete || ProgressStore.LoadTideBeaconProgress() != 3 ||
                GameObject.Find("Lighthouse Beacon") == null || lighthouse == null || !lighthouse.enabled ||
                !ProgressStore.HasSurveySail ||
                FindFirstObjectByType<BoatController>() is not { MaxForwardSpeed: 13.5f, TurnSpeed: 60f })
            {
                Fail("keyboard first-quest hand-in, lighthouse and survey sail did not restore after restart");
                yield break;
            }
            Debug.Log("[FoglightIsles Keyboard First Quest Resume] PASS — handed-in prism, lit lighthouse, survey sail and puzzle progress restored after restart.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyJourney(bool fullQuest)
        {
            yield return null;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            QuestState quest = FindFirstObjectByType<QuestState>();
            if (player == null || boat == null || quest == null || !StartScreen.IsOpen ||
                quest.Stage != PrismQuestStage.NotAccepted ||
                FindFirstObjectByType<DiscoveryJournal>()?.Count != 0)
            {
                Fail("new-game input baseline is missing");
                yield break;
            }

            bool captureFrames = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-keyboard-journey") >= 0;
            if (captureFrames && Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-small-review") >= 0)
            {
                Screen.SetResolution(640, 360, FullScreenMode.Windowed);
                yield return new WaitUntil(() => Screen.width == 640 && Screen.height == 360);
                Debug.Log($"[FoglightIsles Keyboard Capture] Review resolution {Screen.width}x{Screen.height}");
            }
            Keyboard virtualKeyboard = InputSystem.AddDevice<Keyboard>();
            virtualKeyboard.MakeCurrent();
            Vector3 spawn = player.transform.position;
            // Title keyboard activation has dedicated coverage. Batch-mode input timestamps can
            // consume Enter before StartScreen's next Update, so begin the input journey explicitly.
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            yield return null;
            if (StartScreen.IsOpen || Time.timeScale == 0f)
            {
                Fail("could not begin exploration before keyboard journey");
                yield break;
            }

            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            Interactable noticeBoard = null;
            foreach (Interactable interactable in FindObjectsByType<Interactable>(FindObjectsSortMode.None))
                if (interactable.Title == "港口告示牌") { noticeBoard = interactable; break; }
            if (noticeBoard == null)
            {
                Fail("notice board is missing before keyboard walking");
                yield break;
            }
            Vector3 toNoticeBoard = noticeBoard.transform.position - player.transform.position;
            toNoticeBoard.y = 0f;
            if (toNoticeBoard.sqrMagnitude > .01f) player.transform.rotation = Quaternion.LookRotation(toNoticeBoard);
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
            bool foundBoard = false;
            for (int frame = 0; frame < 240; frame++)
            {
                yield return null;
                if (player.FindNearbyInteractable()?.Title != "港口告示牌") continue;
                foundBoard = true;
                break;
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!foundBoard || Vector3.Distance(spawn, player.transform.position) < 2f)
            {
                Fail($"W did not walk from spawn to notice board (position={player.transform.position})");
                yield break;
            }

            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-board.png"));
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (quest.Stage != PrismQuestStage.SearchTideflats)
            {
                Fail("E did not accept the notice-board quest");
                yield break;
            }

            Vector2[] waypoints =
            {
                new Vector2(-2.8f, 4.4f),
                new Vector2(0f, 2.5f),
                new Vector2(0f, -1.6f)
            };
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
            foreach (Vector2 waypoint in waypoints)
            {
                bool arrived = false;
                for (int frame = 0; frame < 240; frame++)
                {
                    Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                    Vector2 direction = waypoint - current;
                    if (direction.magnitude < .65f)
                    {
                        arrived = true;
                        break;
                    }
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                    yield return null;
                }
                if (arrived) continue;
                Fail($"keyboard walk could not reach harbour waypoint {waypoint} from {player.transform.position}");
                yield break;
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (player.FindNearbyInteractable()?.Title != "晨湾港码头")
            {
                Fail("dock interaction was not selected after keyboard walking");
                yield break;
            }

            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!player.IsOnBoat || !boat.HasCaptain)
            {
                Fail("E did not board from the harbour dock");
                yield break;
            }

            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-boarded.png"));
            }
            float startingZ = boat.transform.position.z;
            float startingYaw = boat.transform.eulerAngles.y;
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W, Key.D));
            for (int frame = 0; frame < 60; frame++) yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            float turn = Mathf.Abs(Mathf.DeltaAngle(startingYaw, boat.transform.eulerAngles.y));
            if (boat.Speed < .5f || boat.transform.position.z >= startingZ - .25f || turn < 1f)
            {
                Fail($"W/D did not propel and steer the boat (speed={boat.Speed:0.00}, turn={turn:0.0})");
                yield break;
            }

            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-sailing.png"));
            }
            DockingPoint tideflatsDock = null;
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == "潮汐浅滩码头") tideflatsDock = dock;
            if (tideflatsDock == null)
            {
                Fail("Tideflats dock is missing");
                yield break;
            }

            bool reachedDock = false;
            bool capturedSea = false;
            for (int frame = 0; frame < 1800; frame++)
            {
                Vector3 toDock = tideflatsDock.transform.position - boat.transform.position;
                toDock.y = 0f;
                float distance = toDock.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f)
                {
                    reachedDock = true;
                    break;
                }
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                Key throttle = distance > stoppingDistance ? Key.W : Key.S;
                float heading = Vector3.SignedAngle(boat.transform.forward, toDock, Vector3.up);
                bool steer = Mathf.Abs(heading) > 3f &&
                             frame % 10 < Mathf.CeilToInt(Mathf.Clamp01(Mathf.Abs(heading) / 25f) * 10f);
                KeyboardState input = steer
                    ? new KeyboardState(throttle, heading > 0f ? Key.D : Key.A)
                    : new KeyboardState(throttle);
                QueueKeyboard(virtualKeyboard, input);
                yield return null;
                if (captureFrames && !capturedSea && boat.transform.position.z < -35f)
                {
                    capturedSea = true;
                    yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-sea.png"));
                }
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!reachedDock)
            {
                Fail($"keyboard sailing could not brake at Tideflats (position={boat.transform.position}, speed={boat.Speed:0.00})");
                yield break;
            }

            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-arrival.png"));
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            CharacterController landingController = player.GetComponent<CharacterController>();
            for (int frame = 0; frame < 60 && !landingController.isGrounded; frame++) yield return null;
            Vector3 tideflatsOffset = player.transform.position - tideflatsDock.DisembarkPosition;
            bool tideflatsHit = Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out RaycastHit landing,
                3f, ~0, QueryTriggerInteraction.Ignore);
            if (player.IsOnBoat || boat.HasCaptain || !landingController.isGrounded ||
                new Vector2(tideflatsOffset.x, tideflatsOffset.z).sqrMagnitude > .0225f ||
                !tideflatsHit || landing.collider.gameObject.name != "Tideflats Dock")
            {
                Fail($"E did not disembark onto the solid Tideflats pier (grounded={landingController.isGrounded}, offset={tideflatsOffset}, firstHit={landing.collider?.gameObject.name ?? "none"})");
                yield break;
            }

            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-landed.png"));
            }

            TideBeaconPuzzle puzzle = FindFirstObjectByType<TideBeaconPuzzle>();
            if (puzzle == null)
            {
                Fail("Tideflats beacon puzzle is missing");
                yield break;
            }
            (string name, Vector2 point, string interaction)[] islandRoute =
            {
                ("island sign", new Vector2(3.5f, -72.2f), null),
                ("clue stone", new Vector2(-3f, -72.2f), "潮汐箭纹石碑"),
                ("west beacon", new Vector2(-13f, -77.5f), "西侧潮汐信标"),
                ("prism approach", new Vector2(-8.2f, -86.8f), null),
                ("centre beacon", new Vector2(0f, -91.5f), "中央潮汐信标"),
                ("ruin south", new Vector2(3f, -79f), null),
                ("east beacon", new Vector2(13f, -79.5f), "东侧潮汐信标"),
                ("return path", new Vector2(3f, -79f), null),
                ("prism", new Vector2(-8.2f, -86.8f), "破损的灯塔棱镜")
            };
            int expectedBeaconProgress = 0;
            foreach ((string name, Vector2 point, string interaction) in islandRoute)
            {
                QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
                bool arrived = false;
                for (int frame = 0; frame < 420; frame++)
                {
                    Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                    Vector2 direction = point - current;
                    if (direction.magnitude < .65f &&
                        (interaction == null || player.FindNearbyInteractable()?.Title == interaction))
                    {
                        arrived = true;
                        break;
                    }
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                    yield return null;
                }
                QueueKeyboard(virtualKeyboard, new KeyboardState());
                yield return null;
                if (!arrived)
                {
                    Fail($"keyboard walk could not reach {name} at {point} from {player.transform.position}");
                    yield break;
                }
                if (interaction == null) continue;
                if (player.FindNearbyInteractable()?.Title != interaction)
                {
                    Fail($"keyboard route could not select {interaction} from {player.transform.position}");
                    yield break;
                }
                QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
                yield return null;
                QueueKeyboard(virtualKeyboard, new KeyboardState());
                yield return null;
                if (name == "clue stone")
                {
                    if (!PrototypeHUD.CurrentMessage.Contains("西侧信标") ||
                        !PrototypeHUD.CurrentMessage.Contains("东侧信标"))
                    {
                        Fail("E did not read the west-to-east clue");
                        yield break;
                    }
                    if (captureFrames)
                    {
                        yield return new WaitForEndOfFrame();
                        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-clue.png"));
                    }
                }
                else if (name == "prism")
                {
                    if (quest.Stage != PrismQuestStage.ReturnToHarbour)
                    {
                        Fail("E did not collect the prism after solving the beacons");
                        yield break;
                    }
                    if (captureFrames)
                    {
                        yield return new WaitForEndOfFrame();
                        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-prism.png"));
                    }
                }
                else if (ProgressStore.LoadTideBeaconProgress() != ++expectedBeaconProgress)
                {
                    Fail($"E did not light {name} in order");
                    yield break;
                }
            }
            if (!puzzle.IsComplete || quest.Stage != PrismQuestStage.ReturnToHarbour)
            {
                Fail("island quest state was not saved after keyboard exploration");
                yield break;
            }
            if (!fullQuest)
            {
                if (captureFrames) yield return new WaitForSecondsRealtime(2f);
                InputSystem.RemoveDevice(virtualKeyboard);
                Debug.Log($"[FoglightIsles Keyboard Journey] PASS — keyboard input reached Tideflats, read the clue, lit three beacons and collected the prism (initial turn={turn:0.0} degrees).");
                Application.Quit(0);
                yield break;
            }

            (string name, Vector2 point, string interaction)[] returnRoute =
            {
                ("ruin return path", new Vector2(3f, -79f), null),
                ("dock approach", new Vector2(0f, -71f), null),
                ("Tideflats pier", new Vector2(1.4f, -66.5f), "潮汐浅滩码头")
            };
            foreach ((string name, Vector2 point, string interaction) in returnRoute)
            {
                QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
                bool arrived = false;
                for (int frame = 0; frame < 420; frame++)
                {
                    Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                    Vector2 direction = point - current;
                    if (direction.magnitude < .65f &&
                        (interaction == null || player.FindNearbyInteractable()?.Title == interaction))
                    {
                        arrived = true;
                        break;
                    }
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                    yield return null;
                }
                QueueKeyboard(virtualKeyboard, new KeyboardState());
                yield return null;
                if (!arrived)
                {
                    Fail($"keyboard return walk could not reach {name} from {player.transform.position}");
                    yield break;
                }
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!player.IsOnBoat || !boat.HasCaptain)
            {
                Fail("E did not reboard from Tideflats pier");
                yield break;
            }

            DockingPoint harbourDock = null;
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == "晨湾港码头") harbourDock = dock;
            if (harbourDock == null)
            {
                Fail("Morning Bay dock is missing");
                yield break;
            }

            bool turnedToHarbour = false;
            for (int frame = 0; frame < 1200; frame++)
            {
                Vector3 toHarbour = harbourDock.transform.position - boat.transform.position;
                toHarbour.y = 0f;
                float heading = Vector3.SignedAngle(boat.transform.forward, toHarbour, Vector3.up);
                if (Mathf.Abs(heading) < 3f)
                {
                    turnedToHarbour = true;
                    break;
                }
                QueueKeyboard(virtualKeyboard,
                    new KeyboardState(heading > 0f ? Key.D : Key.A));
                yield return null;
            }
            if (!turnedToHarbour)
            {
                Fail($"keyboard could not turn boat toward Morning Bay (heading={boat.transform.eulerAngles.y:0.0})");
                yield break;
            }

            bool returnedToHarbour = false;
            for (int frame = 0; frame < 1800; frame++)
            {
                Vector3 toDock = harbourDock.transform.position - boat.transform.position;
                toDock.y = 0f;
                float distance = toDock.magnitude;
                if (distance < DockingPoint.DockingRadius - .3f && Mathf.Abs(boat.Speed) < 1.2f)
                {
                    returnedToHarbour = true;
                    break;
                }
                float stoppingDistance = boat.Speed * boat.Speed / 4f + 3.5f;
                Key throttle = distance > stoppingDistance ? Key.W : Key.S;
                float heading = Vector3.SignedAngle(boat.transform.forward, toDock, Vector3.up);
                bool steer = Mathf.Abs(heading) > 3f &&
                             frame % 10 < Mathf.CeilToInt(Mathf.Clamp01(Mathf.Abs(heading) / 25f) * 10f);
                QueueKeyboard(virtualKeyboard, steer
                    ? new KeyboardState(throttle, heading > 0f ? Key.D : Key.A)
                    : new KeyboardState(throttle));
                yield return null;
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!returnedToHarbour)
            {
                Fail($"keyboard could not brake at Morning Bay (position={boat.transform.position}, speed={boat.Speed:0.00})");
                yield break;
            }
            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-return.png"));
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            for (int frame = 0; frame < 60 && !landingController.isGrounded; frame++) yield return null;
            Vector3 harbourOffset = player.transform.position - harbourDock.DisembarkPosition;
            bool harbourHit = Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out RaycastHit harbourLanding,
                3f, ~0, QueryTriggerInteraction.Ignore);
            if (player.IsOnBoat || boat.HasCaptain || !landingController.isGrounded ||
                new Vector2(harbourOffset.x, harbourOffset.z).sqrMagnitude > .0225f ||
                !harbourHit || harbourLanding.collider.gameObject.name != "Main Dock")
            {
                Fail($"E did not land on the Morning Bay pier (grounded={landingController.isGrounded}, offset={harbourOffset}, firstHit={harbourLanding.collider?.gameObject.name ?? "none"})");
                yield break;
            }

            (string name, Vector2 point, string interaction)[] handInRoute =
            {
                ("main pier entrance", new Vector2(0f, 2.5f), null),
                ("harbour path", new Vector2(-3.5f, 5.5f), null),
                ("Eileen", new Vector2(-7.4f, 7.1f), "艾琳，港口管理员")
            };
            foreach ((string name, Vector2 point, string interaction) in handInRoute)
            {
                QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
                bool arrived = false;
                for (int frame = 0; frame < 420; frame++)
                {
                    Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                    Vector2 direction = point - current;
                    if (direction.magnitude < .65f &&
                        (interaction == null || player.FindNearbyInteractable()?.Title == interaction))
                    {
                        arrived = true;
                        break;
                    }
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                    yield return null;
                }
                QueueKeyboard(virtualKeyboard, new KeyboardState());
                yield return null;
                if (!arrived)
                {
                    Fail($"keyboard hand-in walk could not reach {name} from {player.transform.position}");
                    yield break;
                }
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            Light lighthouse = GameObject.Find("Lighthouse Light")?.GetComponent<Light>();
            if (quest.Stage != PrismQuestStage.Complete || !DialoguePanel.IsOpen ||
                GameObject.Find("Lighthouse Beacon") == null || lighthouse == null || !lighthouse.enabled ||
                ProgressStore.HasSurveySail)
            {
                Fail("E did not hand the prism to Eileen and light the harbour lighthouse");
                yield break;
            }
            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-hand-in.png"));
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (DialoguePanel.IsOpen)
            {
                Fail("E did not close Eileen's dialogue before walking to Jon");
                yield break;
            }

            (string name, Vector2 point, string interaction)[] upgradeRoute =
            {
                ("harbour square", new Vector2(-3.5f, 7.5f), null),
                ("shipwright approach", new Vector2(0f, 8.5f), null),
                ("Jon", new Vector2(2.4f, 8.5f), "乔恩，船匠")
            };
            foreach ((string name, Vector2 point, string interaction) in upgradeRoute)
            {
                QueueKeyboard(virtualKeyboard, new KeyboardState(Key.W));
                bool arrived = false;
                for (int frame = 0; frame < 420; frame++)
                {
                    Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                    Vector2 direction = point - current;
                    if (direction.magnitude < .65f &&
                        (interaction == null || player.FindNearbyInteractable()?.Title == interaction))
                    {
                        arrived = true;
                        break;
                    }
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));
                    yield return null;
                }
                QueueKeyboard(virtualKeyboard, new KeyboardState());
                yield return null;
                if (!arrived)
                {
                    Fail($"keyboard upgrade walk could not reach {name} from {player.transform.position}");
                    yield break;
                }
            }
            QueueKeyboard(virtualKeyboard, new KeyboardState(Key.E));
            yield return null;
            QueueKeyboard(virtualKeyboard, new KeyboardState());
            yield return null;
            if (!ProgressStore.HasSurveySail || boat.MaxForwardSpeed != 13.5f || boat.TurnSpeed != 60f ||
                !DialoguePanel.IsOpen)
            {
                Fail("E did not collect Jon's survey sail upgrade");
                yield break;
            }
            if (captureFrames)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../review-keyboard-upgrade.png"));
                yield return new WaitForSecondsRealtime(2f);
            }
            InputSystem.RemoveDevice(virtualKeyboard);
            Debug.Log("[FoglightIsles Keyboard First Quest] PASS — keyboard completed the prism quest, returned to Eileen and collected Jon's survey sail upgrade.");
            Application.Quit(0);
        }

        private static void QueueKeyboard(Keyboard keyboard, KeyboardState state)
        {
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, state);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Keyboard Journey] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
