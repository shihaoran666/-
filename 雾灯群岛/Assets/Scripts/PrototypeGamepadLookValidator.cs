using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Checks the real right-stick input path while the mouse cursor is unlocked.</summary>
    public sealed class PrototypeGamepadLookValidator : MonoBehaviour
    {
        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-look") >= 0)
                StartCoroutine(VerifyLook());
            else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-menu") >= 0)
            {
                Application.runInBackground = true;
                StartCoroutine(VerifyMenu());
            }
            else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-gamepad-panel-flow") >= 0)
            {
                Application.runInBackground = true;
                StartCoroutine(VerifyPanelFlow());
            }
        }

        private static IEnumerator VerifyLook()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Fail("player controller is missing");
                yield break;
            }

            Gamepad virtualPad = InputSystem.AddDevice<Gamepad>();
            Cursor.lockState = CursorLockMode.None;
            float initialYaw = player.transform.eulerAngles.y;
            InputSystem.QueueStateEvent(virtualPad, new GamepadState { rightStick = Vector2.right });
            InputSystem.Update();
            for (int frame = 0; frame < 8; frame++) yield return null;
            float turn = Mathf.Abs(Mathf.DeltaAngle(initialYaw, player.transform.eulerAngles.y));
            bool cursorStayedUnlocked = Cursor.lockState == CursorLockMode.None;
            InputSystem.RemoveDevice(virtualPad);
            if (!cursorStayedUnlocked || turn < 1f)
            {
                Fail($"right stick did not turn the camera with unlocked mouse (turn={turn:0.00}, cursor={Cursor.lockState})");
                yield break;
            }

            Debug.Log($"[FoglightIsles Gamepad Look] PASS — virtual right stick turned {turn:0.0} degrees with unlocked mouse.");
            Application.Quit(0);
        }

        private static IEnumerator VerifyMenu()
        {
            yield return null;
            Gamepad pad = InputSystem.AddDevice<Gamepad>();
            yield return Press(pad, GamepadButton.South);
            if (StartScreen.IsOpen) { Fail("gamepad A did not leave title screen"); yield break; }
            const string interactionMessage = "手柄交互输入回归标记";
            PrototypeHUD.SetMessage(interactionMessage);
            yield return Press(pad, GamepadButton.South);
            if (PrototypeHUD.CurrentMessage != interactionMessage)
            { Fail("gamepad A reopened the title welcome during play"); yield break; }
            yield return Press(pad, GamepadButton.Start);
            if (!PauseMenu.IsPaused || Time.timeScale != 0f)
            { Fail("gamepad Menu did not pause"); yield break; }
            for (int index = 0; index < 3; index++) yield return Press(pad, GamepadButton.DpadDown);
            string lookKey = ProgressStore.StorageKey("foglightisles.settings.look-sensitivity.v1");
            float lookBefore = PlayerPrefs.GetFloat(lookKey, 1f);
            bool moveLeft = lookBefore > 1.9f;
            yield return Press(pad, moveLeft ? GamepadButton.DpadLeft : GamepadButton.DpadRight);
            float lookAfter = PlayerPrefs.GetFloat(lookKey, -1f);
            if (Mathf.Abs(lookAfter - lookBefore) < .05f ||
                !Mathf.Approximately(PlayerController.LookSensitivityMultiplier, lookAfter))
            { Fail("gamepad D-pad did not adjust, apply and save look sensitivity"); yield break; }
            yield return Press(pad, moveLeft ? GamepadButton.DpadRight : GamepadButton.DpadLeft);
            if (Mathf.Abs(PlayerPrefs.GetFloat(lookKey, -1f) - lookBefore) > .01f)
            { Fail("gamepad D-pad did not restore look sensitivity"); yield break; }
            for (int index = 0; index < 2; index++) yield return Press(pad, GamepadButton.DpadDown);
            string widthKey = ProgressStore.StorageKey("foglightisles.settings.window-width.v1");
            string heightKey = ProgressStore.StorageKey("foglightisles.settings.window-height.v1");
            int widthBefore = PlayerPrefs.GetInt(widthKey, 1280);
            int heightBefore = PlayerPrefs.GetInt(heightKey, 720);
            yield return Press(pad, GamepadButton.DpadRight);
            int widthAfter = PlayerPrefs.GetInt(widthKey, -1);
            int heightAfter = PlayerPrefs.GetInt(heightKey, -1);
            if (widthAfter == widthBefore && heightAfter == heightBefore)
            { Fail("gamepad D-pad did not change and save window size"); yield break; }
            yield return Press(pad, GamepadButton.DpadLeft);
            if (PlayerPrefs.GetInt(widthKey, -1) == widthAfter && PlayerPrefs.GetInt(heightKey, -1) == heightAfter)
            { Fail("gamepad D-pad did not cycle window size back"); yield break; }
            yield return Press(pad, GamepadButton.DpadDown);
            const string vSyncKey = "foglightisles.settings.vsync.v1";
            string savedKey = ProgressStore.StorageKey(vSyncKey);
            int before = PlayerPrefs.GetInt(savedKey, 0);
            yield return Press(pad, GamepadButton.DpadRight);
            int after = PlayerPrefs.GetInt(savedKey, -1);
            if (after != 1 - before)
            { Fail("gamepad D-pad did not toggle and save frame pacing"); yield break; }
            yield return Press(pad, GamepadButton.DpadLeft);
            if (PlayerPrefs.GetInt(savedKey, -1) != before)
            { Fail("gamepad D-pad did not restore frame pacing"); yield break; }
            yield return Press(pad, GamepadButton.South);
            if (PlayerPrefs.GetInt(savedKey, -1) != 1 - before)
            { Fail("gamepad A did not confirm the selected frame pacing option"); yield break; }
            if (Time.timeScale != 0f)
            { Fail("gamepad A resumed time while pause menu stayed open"); yield break; }
            yield return Press(pad, GamepadButton.South);
            if (PlayerPrefs.GetInt(savedKey, -1) != before)
            { Fail("gamepad A did not restore the selected frame pacing option"); yield break; }
            if (Time.timeScale != 0f)
            { Fail("gamepad A resumed time while pause menu stayed open"); yield break; }
            yield return Press(pad, GamepadButton.East);
            if (PauseMenu.IsPaused || Time.timeScale == 0f)
            { Fail("gamepad B did not close pause menu and resume play"); yield break; }
            InputSystem.RemoveDevice(pad);
            Debug.Log("[FoglightIsles Gamepad Menu] PASS — A started, Menu paused, D-pad adjusted and saved look sensitivity, D-pad cycled window size, A changed frame pacing, B resumed play.");
            Application.Quit(0);
        }

        private static IEnumerator Press(Gamepad pad, GamepadButton button)
        {
            pad.MakeCurrent();
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(button));
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
        }

        private static IEnumerator VerifyPanelFlow()
        {
            yield return null;
            Gamepad pad = InputSystem.AddDevice<Gamepad>();
            yield return Press(pad, GamepadButton.South);
            if (StartScreen.IsOpen) { FailPanel("A did not leave the title screen"); yield break; }

            yield return Press(pad, GamepadButton.Select);
            if (!MapOverlay.IsOpen || Time.timeScale != 0f)
            { FailPanel("View did not open the map"); yield break; }
            yield return Press(pad, GamepadButton.Start);
            if (MapOverlay.IsOpen || !PauseMenu.IsPaused || Time.timeScale != 0f)
            { FailPanel("Menu did not switch from map to pause"); yield break; }
            yield return Press(pad, GamepadButton.East);
            if (PauseMenu.IsPaused || MapOverlay.IsOpen || Time.timeScale != 1f)
            { FailPanel("B did not resume play after map-to-pause"); yield break; }

            yield return Press(pad, GamepadButton.North);
            if (!QuestJournalPanel.IsOpen || Time.timeScale != 0f)
            { FailPanel("Y did not open the journal"); yield break; }
            yield return Press(pad, GamepadButton.Start);
            if (QuestJournalPanel.IsOpen || !PauseMenu.IsPaused || Time.timeScale != 0f)
            { FailPanel("Menu did not switch from journal to pause"); yield break; }
            yield return Press(pad, GamepadButton.East);
            if (PauseMenu.IsPaused || QuestJournalPanel.IsOpen || Time.timeScale != 1f)
            { FailPanel("B did not resume play after journal-to-pause"); yield break; }

            DialoguePanel.Show("艾琳", "测试对话的暂停与恢复。");
            yield return Press(pad, GamepadButton.Start);
            if (!PauseMenu.IsPaused || !DialoguePanel.IsOpen || Time.timeScale != 0f)
            { FailPanel("Menu did not pause an open dialogue"); yield break; }
            yield return Press(pad, GamepadButton.East);
            if (PauseMenu.IsPaused || !DialoguePanel.IsOpen || Time.timeScale != 1f)
            { FailPanel("B did not restore the open dialogue"); yield break; }
            yield return Press(pad, GamepadButton.South);
            if (DialoguePanel.IsOpen)
            { FailPanel("A did not advance the restored dialogue"); yield break; }

            DialoguePanel.Show("艾琳", "第一句必须保留。", "第二句等待确认。");
            yield return Press(pad, GamepadButton.Start);
            if (!PauseMenu.IsPaused) { FailPanel("Menu did not pause the second dialogue"); yield break; }
            yield return Press(pad, GamepadButton.South);
            if (PauseMenu.IsPaused || DialoguePanel.CurrentLine != "第一句必须保留。")
            { FailPanel("A resumed play and also advanced dialogue in the same frame"); yield break; }
            yield return Press(pad, GamepadButton.South);
            if (DialoguePanel.CurrentLine != "第二句等待确认。")
            { FailPanel("A did not advance dialogue on a separate press"); yield break; }

            InputSystem.RemoveDevice(pad);
            Debug.Log("[FoglightIsles Gamepad Panel Flow] PASS — Menu switched map and journal to pause; B and A resumed play without skipping dialogue.");
            Application.Quit(0);
        }

        private static void FailPanel(string reason)
        {
            Debug.LogError($"[FoglightIsles Gamepad Panel Flow] FAIL — {reason}.");
            Application.Quit(1);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Gamepad Look] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
