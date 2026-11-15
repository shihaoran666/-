using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Subtle deterministic flicker keeps harbour lights readable without noisy random jumps.</summary>
    public sealed class LanternFlicker : MonoBehaviour
    {
        private Light _light;
        private float _baseIntensity;
        private float _phase;

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_light != null) _baseIntensity = _light.intensity;
            _phase = transform.position.x * 1.7f + transform.position.z * .43f;
        }

        private void Update()
        {
            if (_light == null) return;
            _light.intensity = _baseIntensity * (0.86f + Mathf.Sin(Time.time * 5.2f + _phase) * .14f);
        }
    }
}
