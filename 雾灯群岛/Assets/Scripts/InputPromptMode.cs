using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    /// <summary>Tracks the latest meaningful input source so UI prompts stay appropriate for the player.</summary>
    public sealed class InputPromptMode : MonoBehaviour
    {
        private const float StickActivation = .18f;

        public static bool UsingGamepad { get; private set; }

        internal static string FormatInteractionHint(string hint)
        {
            if (string.IsNullOrEmpty(hint)) return string.Empty;
            return UsingGamepad ? hint.Replace("[E]", "[A]") : hint;
        }

        private void Awake()
        {
            UsingGamepad = Gamepad.current != null && Keyboard.current == null;
        }

        private void Update()
        {
            bool keyboardOrMouse = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                                   (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame ||
                                                              Mouse.current.rightButton.wasPressedThisFrame ||
                                                              Mouse.current.delta.ReadValue().sqrMagnitude > .01f));
            Gamepad gamepad = Gamepad.current;
            bool gamepadInput = gamepad != null &&
                (gamepad.buttonSouth.wasPressedThisFrame || gamepad.buttonEast.wasPressedThisFrame ||
                 gamepad.buttonNorth.wasPressedThisFrame || gamepad.buttonWest.wasPressedThisFrame ||
                 gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame ||
                 gamepad.leftStick.ReadValue().sqrMagnitude >= StickActivation * StickActivation ||
                 gamepad.rightStick.ReadValue().sqrMagnitude >= StickActivation * StickActivation ||
                 gamepad.dpad.ReadValue().sqrMagnitude > 0f);

            if (keyboardOrMouse) UsingGamepad = false;
            else if (gamepadInput) UsingGamepad = true;
        }
    }
}
