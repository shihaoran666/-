using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 4.2f;
        [SerializeField] private float sprintSpeed = 6.4f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float lookSensitivity = .12f;

        private CharacterController _controller;
        private Transform _cameraTransform;
        private Transform _cameraPivot;
        private Vector3 _cameraRestOffset;
        private readonly RaycastHit[] _cameraHits = new RaycastHit[32];
        private readonly Collider[] _interactionHits = new Collider[64];
        private float _verticalVelocity;
        private float _pitch = 16f;
        private BoatController _boat;
        private Interactable _hintTarget;
        private string _hintTitle;
        private string _hintPrompt;

        public bool IsOnBoat => _boat != null;
        internal static float LookSensitivityMultiplier { get; set; } = 1f;

        public void AssignCamera(Transform cameraTransform, Transform cameraPivot)
        {
            _cameraTransform = cameraTransform;
            _cameraPivot = cameraPivot;
            _cameraRestOffset = cameraTransform.localPosition;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            PrototypeHUD.SetInteractionHint(string.Empty);
            if (!StartScreen.IsOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            RecoverIfBelowSea();
            if (Keyboard.current == null && Gamepad.current == null) return;
            if (StartScreen.IsOpen || PauseMenu.IsPaused || DialoguePanel.IsOpen || MapOverlay.IsOpen || QuestJournalPanel.IsOpen)
            {
                UpdateInteractionHint(null);
                return;
            }
            UpdateCursor();
            UpdateLook();
            bool interactPressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                                  (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (_boat != null)
            {
                UpdateInteractionHint(null);
                if (interactPressed) _boat.TryDisembark(this);
                return;
            }
            UpdateMovement();
            Interactable nearby = FindNearbyInteractable();
            UpdateInteractionHint(nearby);
            if (interactPressed && nearby != null) nearby.Interact(this);
        }

        private void UpdateInteractionHint(Interactable nearby)
        {
            if (ReferenceEquals(nearby, _hintTarget) &&
                (nearby == null || (nearby.Title == _hintTitle && nearby.Prompt == _hintPrompt))) return;
            _hintTarget = nearby;
            _hintTitle = nearby == null ? null : nearby.Title;
            _hintPrompt = nearby == null ? null : nearby.Prompt;
            PrototypeHUD.SetInteractionHint(nearby == null ? string.Empty : $"[E] {_hintTitle} · {_hintPrompt}");
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null || _cameraPivot == null) return;
            Vector3 ray = _cameraPivot.TransformVector(_cameraRestOffset);
            float restDistance = ray.magnitude;
            if (restDistance < .01f) return;
            float visibleDistance = restDistance;
            Vector3 direction = ray / restDistance;
            int count = Physics.SphereCastNonAlloc(_cameraPivot.position, .25f, direction,
                _cameraHits, restDistance, ~0, QueryTriggerInteraction.Ignore);
            RaycastHit[] hits = _cameraHits;
            if (count == _cameraHits.Length)
            {
                hits = Physics.SphereCastAll(_cameraPivot.position, .25f, direction,
                    restDistance, ~0, QueryTriggerInteraction.Ignore);
                count = hits.Length;
            }
            for (int index = 0; index < count; index++)
            {
                RaycastHit hit = hits[index];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform) ||
                    (_boat != null && hit.collider.transform.IsChildOf(_boat.transform))) continue;
                visibleDistance = Mathf.Min(visibleDistance, Mathf.Max(.5f, hit.distance - .15f));
            }
            _cameraTransform.localPosition = _cameraRestOffset * (visibleDistance / restDistance);
        }

        public void BoardBoat(BoatController boat)
        {
            if (_boat != null) return;
            _boat = boat;
            UpdateInteractionHint(null);
            _controller.enabled = false;
            transform.SetParent(boat.transform, true);
            transform.localPosition = new Vector3(0f, .75f, 0f);
            transform.localRotation = Quaternion.identity;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            boat.SetCaptain(this);
            PrototypeHUD.SetMessage(InputPromptMode.UsingGamepad
                ? "已登船：左摇杆加减速和转向；靠岸减速后按 A 下船。"
                : "已登船：W/S 加减速，A/D 转向；靠岸减速后按 E 下船。");
        }

        public void LeaveBoat(Vector3 position)
        {
            if (_boat == null) return;
            _boat = null;
            transform.SetParent(null, true);
            transform.position = position;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = true;
            _controller.enabled = true;
            PrototypeHUD.SetMessage(LandingMessage(position));
        }

        private static string LandingMessage(Vector3 position)
        {
            QuestState prism = FindFirstObjectByType<QuestState>();
            SurveyChartQuest chart = FindFirstObjectByType<SurveyChartQuest>();
            StormSignalQuest storm = FindFirstObjectByType<StormSignalQuest>();
            if (position.z < -45f)
            {
                if (prism?.Stage == PrismQuestStage.SearchTideflats)
                    return "已到潮汐浅滩：先查看码头附近的箭纹石碑，再依序点亮信标。";
                if (chart?.Stage == SurveyChartStage.SearchRuin)
                    return "已到潮汐浅滩：前往遗迹入口石台，找回失落航图。";
                if (storm?.Stage == StormSignalStage.SearchTideflats)
                    return storm.VaneAligned
                        ? "已到潮汐浅滩：到坍塌灯塔旁取走蓝白色风暴透镜。"
                        : "已到潮汐浅滩：对照海图，将坍塌灯塔旁的风向标转向晨湾港。";
                return "已到潮汐浅滩：继续自由探索。按 E 或手柄 A 与附近物件交互。";
            }
            if (prism?.Stage == PrismQuestStage.ReturnToHarbour)
                return "已返晨湾港：把棱镜交给港口管理员艾琳。";
            if (chart?.Stage == SurveyChartStage.ReturnToHarbour)
                return "已返晨湾港：把航图交给港口管理员艾琳。";
            if (storm?.Stage == StormSignalStage.ReturnToShipwright)
                return "已返晨湾港：把风暴透镜交给船匠乔恩。";
            return "已返晨湾港：继续探索，或向港口居民询问新的线索。";
        }

        private void UpdateCursor()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void UpdateLook()
        {
            Vector2 delta = Cursor.lockState == CursorLockMode.Locked && Mouse.current != null
                ? Mouse.current.delta.ReadValue() * lookSensitivity * LookSensitivityMultiplier : Vector2.zero;
            if (Gamepad.current != null && delta.sqrMagnitude < .0001f)
            {
                Vector2 stick = Gamepad.current.rightStick.ReadValue();
                if (stick.sqrMagnitude >= .18f * .18f)
                    delta = stick * (120f * LookSensitivityMultiplier * Time.deltaTime);
            }
            if (delta.sqrMagnitude < .0001f) return;
            _pitch = Mathf.Clamp(_pitch - delta.y, -15f, 55f);
            transform.Rotate(Vector3.up * delta.x);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
            }
            if (input == Vector2.zero && Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude >= .18f * .18f) input = stick;
            }
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 movement = transform.forward * input.y + transform.right * input.x;
            bool sprinting = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
                             (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
            var speed = sprinting ? sprintSpeed : walkSpeed;
            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = movement * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        internal bool RecoverIfBelowSea()
        {
            if (_boat != null || transform.position.y >= -2.5f) return false;
            bool nearTideflats = transform.position.z < -45f;
            _controller.enabled = false;
            transform.position = nearTideflats ? new Vector3(0f, 1f, -78f) : new Vector3(0f, 1f, 10f);
            _verticalVelocity = 0f;
            _controller.enabled = true;
            PrototypeHUD.SetMessage(nearTideflats
                ? "海浪把你送回了潮汐浅滩。先找到码头，再登上晨鹱号。"
                : "海浪把你送回了晨湾港。码头的小船仍在等你。 ");
            return true;
        }

        internal Interactable FindNearbyInteractable()
        {
            Interactable closest = null;
            float closestDistance = 2.25f;
            int count = Physics.OverlapSphereNonAlloc(transform.position, 2.25f, _interactionHits);
            Collider[] hits = _interactionHits;
            if (count == _interactionHits.Length)
            {
                hits = Physics.OverlapSphere(transform.position, 2.25f);
                count = hits.Length;
            }
            for (int index = 0; index < count; index++)
            {
                Collider hit = hits[index];
                if (hit.GetComponentInParent<Interactable>() is { } interactable && interactable.isActiveAndEnabled)
                {
                    float distance = Vector3.Distance(transform.position, interactable.transform.position);
                    if (distance < closestDistance)
                    {
                        closest = interactable;
                        closestDistance = distance;
                    }
                }
            }
            return closest;
        }
    }
}
