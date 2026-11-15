using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Exercises prompt switching using real Input System events rather than a UI-only flag.</summary>
    public sealed class PrototypeInputPromptValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-input-prompts";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
            {
                Application.runInBackground = true;
                StartCoroutine(VerifyPrompts());
            }
        }

        private static IEnumerator VerifyPrompts()
        {
            yield return null;
            Gamepad pad = InputSystem.AddDevice<Gamepad>();
            pad.MakeCurrent();
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = Vector2.right });
            yield return null;
            if (!InputPromptMode.UsingGamepad || InputPromptMode.FormatInteractionHint("[E] 验证交互") != "[A] 验证交互")
            {
                Fail("left-stick input did not switch prompts and interaction label to gamepad");
                yield break;
            }

            InputSystem.QueueStateEvent(pad, new GamepadState());
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            if (InputPromptMode.UsingGamepad || InputPromptMode.FormatInteractionHint("[E] 验证交互") != "[E] 验证交互")
            {
                Fail("keyboard input did not switch prompts and interaction label back to keyboard and mouse");
                yield break;
            }

            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(pad);
            Debug.Log("[FoglightIsles Input Prompts] PASS — gamepad and keyboard input switch UI prompt mode.");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Input Prompts] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
