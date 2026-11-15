using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Shares the active objective with the compass and the opened sea chart.</summary>
    public sealed class GuidanceMarker : MonoBehaviour
    {
        public Transform Target { get; private set; }
        public string Label { get; private set; }

        public void SetTarget(Transform target, string label)
        {
            Target = target;
            Label = label;
        }
    }
}
