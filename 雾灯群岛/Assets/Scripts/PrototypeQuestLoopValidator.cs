using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Exercises the complete prototype task chain when launched with -verify-quest-loop.</summary>
    public sealed class PrototypeQuestLoopValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-quest-loop";
        private const string ResumeArgument = "-verify-quest-resume";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(VerifyQuestLoop());
            else if (Array.IndexOf(Environment.GetCommandLineArgs(), ResumeArgument) >= 0)
                StartCoroutine(VerifyRestoredUpgrade());
        }

        private static IEnumerator VerifyRestoredUpgrade()
        {
            yield return null;
            BoatController boat = FindFirstObjectByType<BoatController>();
            QuestState prism = FindFirstObjectByType<QuestState>();
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            if (boat == null || prism == null || chart == null || storm == null ||
                !ProgressStore.HasSurveySail || !Mathf.Approximately(boat.MaxForwardSpeed, 13.5f) ||
                !Mathf.Approximately(boat.TurnSpeed, 60f) ||
                prism.Stage != PrismQuestStage.Complete || chart.Stage != SurveyChartStage.Complete ||
                storm.Stage != StormSignalStage.Complete ||
                FindFirstObjectByType<StartScreen>()?.HasSavedProgress != true ||
                GameObject.Find("Harbour Chart Display") == null || GameObject.Find("Harbour Signal Lamp") == null ||
                GameObject.Find("Harbour Signal Light")?.GetComponent<Light>().enabled != true)
            {
                Fail("saved quest stages or survey sail were not restored in a new process");
                yield break;
            }
            Debug.Log("[FoglightIsles Quest Resume] PASS — completed quests and upgraded boat restored after restart.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyQuestLoop()
        {
            yield return null;
            QuestState prism = FindFirstObjectByType<QuestState>();
            TideBeaconPuzzle puzzle = FindFirstObjectByType<TideBeaconPuzzle>();
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            GuidanceMarker guidance = FindFirstObjectByType<GuidanceMarker>();
            if (prism == null || puzzle == null || chart == null || storm == null || bootstrap == null || boat == null || player == null || guidance == null)
            {
                Fail("one or more quest, boat or guidance services are missing");
                yield break;
            }

            float baseSpeed = boat.MaxForwardSpeed;
            float baseTurn = boat.TurnSpeed;

            prism.Accept();
            MovePlayerTo(player, new Vector3(-3f, .05f, -72.2f));
            Interactable clue = player.FindNearbyInteractable();
            if (clue == null || clue.gameObject.name != "潮汐箭纹石碑")
            {
                Fail("the tide order clue cannot be read from the landing path");
                yield break;
            }
            clue.Interact(player);
            if (!PrototypeHUD.CurrentMessage.Contains("西侧信标") || !PrototypeHUD.CurrentMessage.Contains("东侧信标"))
            {
                Fail("the tide order clue did not explain the beacon sequence");
                yield break;
            }
            yield return null;
            if (guidance.Target == null || guidance.Target == puzzle.CurrentTarget ||
                !guidance.Label.Contains("探索三座潮汐信标"))
            {
                Fail("the compass revealed the next correct beacon instead of the search area");
                yield break;
            }
            puzzle.Activate(2); // Intentionally wrong: confirms the reset path before solving.
            if (ProgressStore.LoadTideBeaconProgress() != 0 || !LitBeaconCapsMatch(0))
            {
                Fail("incorrect tide beacon order did not reset saved state and visual caps");
                yield break;
            }

            puzzle.Activate(0);
            if (!LitBeaconCapsMatch(1)) { Fail("first beacon visual cap did not appear alone"); yield break; }
            puzzle.Activate(1);
            if (!LitBeaconCapsMatch(2)) { Fail("second beacon visual cap did not appear"); yield break; }
            puzzle.Activate(2);
            if (!puzzle.IsComplete || ProgressStore.LoadTideBeaconProgress() != 3 || !LitBeaconCapsMatch(3))
            {
                Fail("tide beacon sequence did not persist");
                yield break;
            }

            if (!TryCollectFromApproach(player, "灯塔棱镜零件", new Vector3(-8.2f, .05f, -86.8f)))
            {
                Fail("the prism cannot be selected from a ground-level approach");
                yield break;
            }
            MovePlayerTo(player, new Vector3(-7.4f, .05f, 7f));
            bootstrap.TalkToEileen();
            yield return null;
            if (prism.Stage != PrismQuestStage.Complete || ProgressStore.HasSurveySail || guidance.Target != GameObject.Find("船匠乔恩")?.transform)
            {
                Fail("prism hand-in did not guide the player to the sail upgrade");
                yield break;
            }

            bootstrap.TalkToJon();
            if (!ProgressStore.HasSurveySail || !Mathf.Approximately(boat.MaxForwardSpeed, baseSpeed + 2.5f) ||
                !Mathf.Approximately(boat.TurnSpeed, baseTurn + 8f))
            {
                Fail("the shipwright did not save and apply the survey sail upgrade");
                yield break;
            }
            bootstrap.TalkToJon();
            if (!Mathf.Approximately(boat.MaxForwardSpeed, baseSpeed + 2.5f) || storm.Stage != StormSignalStage.Locked)
            {
                Fail("repeated shipwright interaction stacked the upgrade or skipped the chart quest");
                yield break;
            }

            bootstrap.TalkToEileen();
            yield return null;
            if (chart.Stage != SurveyChartStage.SearchRuin || guidance.Target != boat.transform)
            {
                Fail("chart quest did not point the player to the boat");
                yield break;
            }
            if (!TryCollectFromApproach(player, "遗迹残碑", new Vector3(6.6f, .05f, -83.6f)) ||
                !ProgressStore.HasReadSurveyClue || chart.Stage != SurveyChartStage.SearchRuin)
            {
                Fail("the ruin inscription could not be read before collecting the chart");
                yield break;
            }
            if (!TryCollectFromApproach(player, "失落航图", new Vector3(8.3f, .05f, -82.5f)))
            {
                Fail("the chart cannot be selected from the ruin entrance");
                yield break;
            }
            if (chart.Stage != SurveyChartStage.ReturnToHarbour ||
                GameObject.Find("遗迹残碑")?.GetComponent<Interactable>()?.isActiveAndEnabled != false)
            {
                Fail("the chart clue stayed interactable after collection");
                yield break;
            }
            MovePlayerTo(player, new Vector3(-7.4f, .05f, 7f));
            bootstrap.TalkToEileen();
            if (GameObject.Find("Harbour Chart Display") == null || GameObject.Find("Harbour Signal Lamp") != null)
            {
                Fail("the chart hand-in did not change the harbour navigation table correctly");
                yield break;
            }
            bootstrap.TalkToJon();
            for (int turn = 0; turn < 3; turn++)
            {
                if (!TryCollectFromApproach(player, "风向标", new Vector3(-7f, .05f, -85.8f)))
                { Fail("the storm vane cannot be reached from the tower path"); yield break; }
            }
            if (!storm.VaneAligned)
            { Fail("the storm vane did not point north after three turns"); yield break; }
            if (!TryCollectFromApproach(player, "风暴透镜", new Vector3(-4.6f, .05f, -89.8f)))
            {
                Fail("the storm lens cannot be selected beside the collapsed lighthouse");
                yield break;
            }
            MovePlayerTo(player, new Vector3(2.4f, .05f, 8f));
            bootstrap.TalkToJon();
            yield return null;

            bool complete = prism.Stage == PrismQuestStage.Complete &&
                            chart.Stage == SurveyChartStage.Complete &&
                            storm.Stage == StormSignalStage.Complete &&
                            ProgressStore.LoadQuestStage() == PrismQuestStage.Complete &&
                            ProgressStore.HasSurveySail &&
                            ProgressStore.LoadSurveyChartStage() == SurveyChartStage.Complete &&
                            ProgressStore.LoadStormSignalStage() == StormSignalStage.Complete &&
                            GameObject.Find("Harbour Chart Display") != null &&
                            GameObject.Find("Harbour Signal Lamp") != null &&
                            GameObject.Find("Harbour Signal Light")?.GetComponent<Light>().enabled == true &&
                            guidance.Target == null;
            if (!complete)
            {
                Fail("one or more quest states failed to complete or save");
                yield break;
            }

            Debug.Log("[FoglightIsles Quest Loop] PASS — beacon puzzle, ground-level item interactions, three NPC quests, survey sail effect and saved stages validated.");
            Application.Quit(0);
        }

        private static bool TryCollectFromApproach(PlayerController player, string objectName, Vector3 position)
        {
            MovePlayerTo(player, position);
            Interactable nearby = player.FindNearbyInteractable();
            if (nearby == null || nearby.gameObject.name != objectName) return false;
            nearby.Interact(player);
            return true;
        }

        private static bool LitBeaconCapsMatch(int litCount)
        {
            string[] names = { "西侧潮汐信标点亮柱", "中央潮汐信标点亮柱", "东侧潮汐信标点亮柱" };
            for (int index = 0; index < names.Length; index++)
                if ((GameObject.Find(names[index]) != null) != (index < litCount)) return false;
            return true;
        }

        private static void MovePlayerTo(PlayerController player, Vector3 position)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Quest Loop] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
