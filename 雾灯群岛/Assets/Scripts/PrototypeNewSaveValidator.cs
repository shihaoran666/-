using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Checks a scene reload through the actual New Save action using isolated validation preferences.</summary>
    public sealed class PrototypeNewSaveValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-new-save";
        private const string VerifyTitleArgument = "-verify-title-new-save";
        private const string VerifyDisplayArgument = "-verify-display-mode";
        private const string VerifyDisplayResumeArgument = "-verify-display-resume";
        private const string FullscreenKey = "foglightisles.settings.fullscreen.v1";
        private const string WindowWidthKey = "foglightisles.settings.window-width.v1";
        private const string WindowHeightKey = "foglightisles.settings.window-height.v1";
        private const string VolumeKey = "foglightisles.settings.volume.v1";
        private const string LookSensitivityKey = "foglightisles.settings.look-sensitivity.v1";
        private const string VSyncKey = "foglightisles.settings.vsync.v1";
        private static bool _awaitingReload;
        private static bool _awaitingTitleReload;
        private static Gamepad _titlePad;

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, VerifyDisplayArgument) >= 0)
                StartCoroutine(VerifyDisplaySwitch());
            else if (Array.IndexOf(args, VerifyDisplayResumeArgument) >= 0)
                StartCoroutine(VerifyDisplayResume());
            else if (Array.IndexOf(args, VerifyArgument) >= 0)
                StartCoroutine(_awaitingReload ? VerifyAfterReload() : TriggerNewSave());
            else if (Array.IndexOf(args, VerifyTitleArgument) >= 0)
                StartCoroutine(_awaitingTitleReload ? VerifyTitleAfterReload() : TriggerTitleNewSave());
        }

        private static IEnumerator TriggerNewSave()
        {
            yield return null;
            PauseMenu pause = FindFirstObjectByType<PauseMenu>();
            if (pause == null)
            {
                Fail("pause service is missing");
                yield break;
            }

            ProgressStore.SaveQuestStage(PrismQuestStage.SearchTideflats);
            ProgressStore.UnlockSurveySail();
            ProgressStore.SaveTideBeaconProgress(2);
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.SearchRuin);
            ProgressStore.SaveSurveyClueRead();
            ProgressStore.SaveStormSignalStage(StormSignalStage.ReturnToShipwright);
            ProgressStore.SaveStormVaneDirection(0);
            PlayerPrefs.SetInt(ProgressStore.StorageKey("foglightisles.discovery.harbour-log"), 1);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(VolumeKey), .42f);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(LookSensitivityKey), 1.5f);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(VSyncKey), 1);
            PlayerPrefs.Save();
            DialoguePanel.Show("乔恩", "这段对白不应留在新档中。");
            if (ProgressStore.LoadQuestStage() != PrismQuestStage.SearchTideflats || !DialoguePanel.IsOpen)
            {
                Fail("validation setup did not create an active quest and dialogue");
                yield break;
            }

            _awaitingReload = true;
            pause.StartNewSave();
        }

        private static IEnumerator VerifyAfterReload()
        {
            yield return null;
            QuestState quest = FindFirstObjectByType<QuestState>();
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            bool valid = quest != null && quest.Stage == PrismQuestStage.NotAccepted &&
                         ProgressStore.LoadQuestStage() == PrismQuestStage.NotAccepted &&
                         !ProgressStore.HasSurveySail && ProgressStore.LoadTideBeaconProgress() == 0 &&
                         ProgressStore.LoadSurveyChartStage() == SurveyChartStage.Locked &&
                         !ProgressStore.HasReadSurveyClue &&
                         ProgressStore.LoadStormSignalStage() == StormSignalStage.Locked &&
                         ProgressStore.LoadStormVaneDirection() == 1 &&
                         journal != null && journal.Count == 0 &&
                         boat != null && Mathf.Approximately(boat.MaxForwardSpeed, 11f) &&
                         !DialoguePanel.IsOpen && StartScreen.IsOpen &&
                         Mathf.Approximately(AudioListener.volume, .42f) &&
                         Mathf.Approximately(PlayerController.LookSensitivityMultiplier, 1.5f) &&
                         QualitySettings.vSyncCount == 1 && Application.targetFrameRate == -1;
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(VolumeKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(LookSensitivityKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(VSyncKey));
            PlayerPrefs.Save();
            if (!valid)
            {
                Fail("new save retained progress/dialogue state (including the survey clue) or discarded audio settings");
                yield break;
            }

            Debug.Log("[FoglightIsles New Save] PASS — progress, dialogue and the survey ruin clue reset; audio, look sensitivity and frame pacing settings retained.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyDisplaySwitch()
        {
            yield return new WaitForSecondsRealtime(.5f);
            PauseMenu pause = FindFirstObjectByType<PauseMenu>();
            if (pause == null)
            {
                FailDisplay("pause service is missing");
                yield break;
            }

            if (Screen.fullScreenMode != FullScreenMode.Windowed)
            {
                pause.ToggleDisplayModeForReview();
                yield return new WaitForSecondsRealtime(.5f);
            }
            int width = Screen.width;
            int height = Screen.height;
            if (Screen.fullScreenMode != FullScreenMode.Windowed || width < 640 || height < 360)
            {
                FailDisplay("windowed baseline was not established");
                yield break;
            }

            pause.ToggleDisplayModeForReview();
            yield return new WaitForSecondsRealtime(.5f);
            if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow ||
                PlayerPrefs.GetInt(ProgressStore.StorageKey(FullscreenKey), -1) != 1 ||
                PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowWidthKey), -1) != width ||
                PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowHeightKey), -1) != height)
            {
                FailDisplay("fullscreen switch did not save its mode and window dimensions");
                yield break;
            }

            pause.ToggleDisplayModeForReview();
            yield return new WaitForSecondsRealtime(.5f);
            if (Screen.fullScreenMode != FullScreenMode.Windowed || Screen.width != width || Screen.height != height)
            {
                FailDisplay("return to windowed did not restore the previous dimensions");
                yield break;
            }

            pause.ToggleDisplayModeForReview();
            yield return new WaitForSecondsRealtime(.5f);
            if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                FailDisplay("fullscreen mode was not active before restart");
                yield break;
            }
            Debug.Log($"[FoglightIsles Display] PASS — switched both ways and saved {width}x{height} for restart.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyDisplayResume()
        {
            yield return new WaitForSecondsRealtime(.5f);
            PauseMenu pause = FindFirstObjectByType<PauseMenu>();
            int width = PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowWidthKey), -1);
            int height = PlayerPrefs.GetInt(ProgressStore.StorageKey(WindowHeightKey), -1);
            if (pause == null || Screen.fullScreenMode != FullScreenMode.FullScreenWindow ||
                PlayerPrefs.GetInt(ProgressStore.StorageKey(FullscreenKey), -1) != 1 ||
                width < 640 || height < 360)
            {
                FailDisplay("saved fullscreen mode was not restored on restart");
                yield break;
            }

            pause.ToggleDisplayModeForReview();
            yield return new WaitForSecondsRealtime(.5f);
            bool restored = Screen.fullScreenMode == FullScreenMode.Windowed &&
                            Screen.width == width && Screen.height == height;
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(FullscreenKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(WindowWidthKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(WindowHeightKey));
            PlayerPrefs.Save();
            if (!restored)
            {
                FailDisplay("saved window dimensions were not restored after restart");
                yield break;
            }
            Debug.Log($"[FoglightIsles Display Resume] PASS — restarted fullscreen and restored {width}x{height} window.");
            Application.Quit(0);
        }

        private static IEnumerator TriggerTitleNewSave()
        {
            yield return null;
            StartScreen title = FindFirstObjectByType<StartScreen>();
            if (title == null || !StartScreen.IsOpen)
            { Fail("title screen is missing for new-save validation"); yield break; }

            ProgressStore.SaveQuestStage(PrismQuestStage.SearchTideflats);
            ProgressStore.UnlockSurveySail();
            ProgressStore.SaveTideBeaconProgress(2);
            ProgressStore.SaveSurveyChartStage(SurveyChartStage.SearchRuin);
            ProgressStore.SaveStormSignalStage(StormSignalStage.ReturnToShipwright);
            ProgressStore.SaveStormVaneDirection(0);
            string discoveryKey = ProgressStore.StorageKey("foglightisles.discovery.harbour-log");
            PlayerPrefs.SetInt(discoveryKey, 1);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(VolumeKey), .42f);
            PlayerPrefs.SetFloat(ProgressStore.StorageKey(LookSensitivityKey), 1.5f);
            PlayerPrefs.SetInt(ProgressStore.StorageKey(VSyncKey), 1);
            PlayerPrefs.Save();

            _titlePad = InputSystem.AddDevice<Gamepad>();
            yield return Press(_titlePad, GamepadButton.North);
            if (!title.IsConfirmingNewSave)
            { Fail("gamepad Y did not open title new-save confirmation"); yield break; }
            yield return Press(_titlePad, GamepadButton.East);
            if (title.IsConfirmingNewSave || ProgressStore.LoadQuestStage() != PrismQuestStage.SearchTideflats ||
                PlayerPrefs.GetInt(discoveryKey, 0) != 1)
            { Fail("gamepad B did not cancel without clearing progress"); yield break; }
            yield return Press(_titlePad, GamepadButton.North);
            if (!title.IsConfirmingNewSave)
            { Fail("title new-save confirmation did not reopen"); yield break; }
            _awaitingTitleReload = true;
            InputSystem.QueueStateEvent(_titlePad, new GamepadState().WithButton(GamepadButton.South));
            yield return null;
            for (int frame = 0; frame < 4; frame++) yield return null;
            if (title != null) Fail("gamepad A did not reload a new save from the title");
        }

        private static IEnumerator VerifyTitleAfterReload()
        {
            yield return null;
            if (_titlePad != null)
            {
                InputSystem.RemoveDevice(_titlePad);
                _titlePad = null;
            }
            QuestState quest = FindFirstObjectByType<QuestState>();
            DiscoveryJournal journal = FindFirstObjectByType<DiscoveryJournal>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            bool valid = quest != null && quest.Stage == PrismQuestStage.NotAccepted &&
                         ProgressStore.LoadQuestStage() == PrismQuestStage.NotAccepted &&
                         !ProgressStore.HasSurveySail && ProgressStore.LoadTideBeaconProgress() == 0 &&
                         ProgressStore.LoadSurveyChartStage() == SurveyChartStage.Locked &&
                         ProgressStore.LoadStormSignalStage() == StormSignalStage.Locked &&
                         ProgressStore.LoadStormVaneDirection() == 1 &&
                         journal != null && journal.Count == 0 &&
                         boat != null && Mathf.Approximately(boat.MaxForwardSpeed, 11f) &&
                         StartScreen.IsOpen && !FindFirstObjectByType<StartScreen>().IsConfirmingNewSave &&
                         !FindFirstObjectByType<StartScreen>().HasSavedProgress &&
                         Mathf.Approximately(AudioListener.volume, .42f) &&
                         Mathf.Approximately(PlayerController.LookSensitivityMultiplier, 1.5f) &&
                         QualitySettings.vSyncCount == 1 && Application.targetFrameRate == -1;
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(VolumeKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(LookSensitivityKey));
            PlayerPrefs.DeleteKey(ProgressStore.StorageKey(VSyncKey));
            PlayerPrefs.Save();
            if (!valid)
            { Fail("title new save retained progress or discarded audio/frame settings"); yield break; }
            Debug.Log("[FoglightIsles Title New Save] PASS — Y opened confirmation, B canceled, A reset progress while preserving settings.");
            Application.Quit(0);
        }

        private static IEnumerator Press(Gamepad pad, GamepadButton button)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(button));
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
        }

        private static void FailDisplay(string reason)
        {
            Debug.LogError($"[FoglightIsles Display] FAIL — {reason}.");
            Application.Quit(1);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles New Save] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
