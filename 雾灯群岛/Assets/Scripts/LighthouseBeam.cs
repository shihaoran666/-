using UnityEngine;

namespace FoglightIsles
{
    /// <summary>
    /// Sweeps the restored harbour lighthouse beam around its vertical axis.
    /// The beam mesh itself is parented under the existing Lighthouse Beacon object, so the
    /// restored/never-restored state (which already drives <c>SetActive</c> on that object)
    /// also controls the beam. No new save flag, quest stage or persistence key is involved.
    /// </summary>
    public sealed class LighthouseBeam : MonoBehaviour
    {
        /// <summary>Seconds for one full sweep. The art plan asks for a 4–6 s period.</summary>
        public const float DefaultSweepSeconds = 5f;

        private float _degreesPerSecond = 360f / DefaultSweepSeconds;

        /// <summary>Sets the sweep period; values at or below zero fall back to the default.</summary>
        public void Configure(float sweepSeconds)
        {
            _degreesPerSecond = sweepSeconds > .01f ? 360f / sweepSeconds : 360f / DefaultSweepSeconds;
        }

        private void Update()
        {
            transform.Rotate(0f, _degreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }
    }
}
