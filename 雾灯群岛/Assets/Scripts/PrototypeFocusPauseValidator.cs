using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Verifies that gameplay pauses when the application loses focus.</summary>
    public sealed class PrototypeFocusPauseValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-focus-pause";

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(VerifyFocusPause());
        }

        private static IEnumerator VerifyFocusPause()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            yield return null;
            PauseMenu pauseMenu = FindFirstObjectByType<PauseMenu>();
            if (pauseMenu == null)
            {
                Fail("pause menu is missing");
                yield break;
            }

            pauseMenu.PauseForFocusLossForValidation();
            yield return null;
            if (!PauseMenu.IsPaused || Time.timeScale != 0f || Cursor.lockState != CursorLockMode.None)
            {
                Fail($"focus loss did not pause gameplay (paused={PauseMenu.IsPaused}, timeScale={Time.timeScale:0.0}, cursor={Cursor.lockState})");
                yield break;
            }

            Debug.Log("[FoglightIsles Focus Pause] PASS — focus loss pauses gameplay and unlocks the cursor.");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Focus Pause] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
