using UnityEngine;
using UnityEngine.InputSystem;

namespace FoglightIsles
{
    public sealed class BoatController : MonoBehaviour
    {
        [SerializeField] private float acceleration = 4f;
        [SerializeField] private float reverseAcceleration = 2f;
        [SerializeField] private float maxForwardSpeed = 11f;
        [SerializeField] private float maxReverseSpeed = 3f;
        [SerializeField] private float turnSpeed = 52f;
        [SerializeField] private float drag = 1.4f;

        private float _speed;
        private PlayerController _captain;
        private DockingPoint _nearbyDock;
        private bool _hasSurveySail;
        private DockingPoint[] _docks;
        private readonly Collider[] _shoreHits = new Collider[32];

        public bool HasCaptain => _captain != null;
        internal float Speed => _speed;
        internal DockingPoint NearbyDock => _nearbyDock;
        internal float MaxForwardSpeed => maxForwardSpeed;
        internal float TurnSpeed => turnSpeed;

        public void SetCaptain(PlayerController captain) => _captain = captain;

        public void ApplySurveySail()
        {
            if (_hasSurveySail) return;
            _hasSurveySail = true;
            maxForwardSpeed += 2.5f;
            turnSpeed += 8f;
        }

        private void Update()
        {
            if (StartScreen.IsOpen || PauseMenu.IsPaused || DialoguePanel.IsOpen || MapOverlay.IsOpen || QuestJournalPanel.IsOpen || !HasCaptain || (Keyboard.current == null && Gamepad.current == null)) return;
            float throttle = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) throttle += 1f;
                if (Keyboard.current.sKey.isPressed) throttle -= 1f;
            }
            float steering = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) steering -= 1f;
                if (Keyboard.current.dKey.isPressed) steering += 1f;
            }
            if (Mathf.Approximately(throttle, 0f) && Mathf.Approximately(steering, 0f) && Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude >= .18f * .18f)
                {
                    throttle = stick.y;
                    steering = stick.x;
                }
            }
            SimulateControl(throttle, steering, Time.deltaTime);
        }

        internal void SimulateControl(float throttle, float steering, float deltaTime)
        {
            throttle = Mathf.Clamp(throttle, -1f, 1f);
            steering = Mathf.Clamp(steering, -1f, 1f);
            if (throttle > 0f) _speed += acceleration * throttle * deltaTime;
            if (throttle < 0f) _speed += reverseAcceleration * throttle * deltaTime;
            if (Mathf.Abs(throttle) < .01f) _speed = Mathf.MoveTowards(_speed, 0f, drag * deltaTime);
            _speed = Mathf.Clamp(_speed, -maxReverseSpeed, maxForwardSpeed);
            float steeringStrength = Mathf.Lerp(.25f, 1f, Mathf.InverseLerp(0f, maxForwardSpeed, Mathf.Abs(_speed)));
            transform.Rotate(Vector3.up, steering * turnSpeed * steeringStrength * deltaTime);
            Vector3 nextPosition = transform.position + transform.forward * (_speed * deltaTime);
            if (Mathf.Abs(nextPosition.x) > 95f || Mathf.Abs(nextPosition.z) > 95f)
            {
                _speed = 0f;
                if (_captain != null) PrototypeHUD.SetMessage("前方已是雾海边界。请转向并沿航线返航。");
            }
            else if (TouchesShore(nextPosition))
            {
                _speed = 0f;
                if (_captain != null) PrototypeHUD.SetMessage("前方是浅滩。请转向码头航道。");
            }
            else transform.position = nextPosition;
            transform.position = new Vector3(transform.position.x, .38f + Mathf.Sin(Time.time * 1.4f) * .05f, transform.position.z);

            _nearbyDock = FindNearestDock();
            ApplyDockingAssist(deltaTime, throttle);
        }

        // Slow-speed manoeuvring is the hardest part for new players, so within docking range
        // we bleed speed faster and nudge the hull toward the pier instead of letting it drift past.
        private void ApplyDockingAssist(float deltaTime, float throttle)
        {
            if (_nearbyDock == null || Mathf.Abs(throttle) > .05f) return;
            float distance = Vector3.Distance(transform.position, _nearbyDock.transform.position);
            if (distance > DockingPoint.DockingRadius) return;
            float speed = Mathf.Abs(_speed);
            if (speed > 2.5f) return;
            _speed = Mathf.MoveTowards(_speed, 0f, speed * 3.2f * deltaTime);
            Vector3 toDock = _nearbyDock.transform.position - transform.position;
            toDock.y = 0f;
            if (toDock.sqrMagnitude > 1f)
            {
                Vector3 nextPosition = transform.position + toDock.normalized * Mathf.Min(distance, 1.6f * deltaTime);
                if (!TouchesShore(nextPosition)) transform.position = nextPosition;
            }
        }

        public void TryDisembark(PlayerController player)
        {
            _nearbyDock = FindNearestDock();
            if (_nearbyDock == null || Mathf.Abs(_speed) >= 1.2f)
            {
                PrototypeHUD.SetMessage("请减速并靠近码头后再下船。");
                return;
            }
            _speed = 0f;
            _captain = null;
            player.LeaveBoat(_nearbyDock.DisembarkPosition);
        }

        private DockingPoint FindNearestDock()
        {
            DockingPoint closest = null;
            float closestDistance = DockingPoint.DockingRadius;
            _docks ??= FindObjectsByType<DockingPoint>(FindObjectsSortMode.None);
            foreach (DockingPoint dock in _docks)
            {
                float distance = Vector3.Distance(transform.position, dock.transform.position);
                if (distance < closestDistance)
                {
                    closest = dock;
                    closestDistance = distance;
                }
            }
            return closest;
        }

        private bool TouchesShore(Vector3 position)
        {
            Vector3 sample = position + Vector3.down * .5f;
            int count = Physics.OverlapSphereNonAlloc(sample, .6f, _shoreHits, ~0, QueryTriggerInteraction.Ignore);
            Collider[] hits = _shoreHits;
            if (count == _shoreHits.Length)
            {
                hits = Physics.OverlapSphere(sample, .6f, ~0, QueryTriggerInteraction.Ignore);
                count = hits.Length;
            }
            for (int index = 0; index < count; index++)
            {
                string name = hits[index].gameObject.name;
                if (name == "Morning Bay Shore" || name == "Tideflats Island") return true;
            }
            return false;
        }
    }
}
