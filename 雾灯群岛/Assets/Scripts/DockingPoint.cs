using UnityEngine;

namespace FoglightIsles
{
    public sealed class DockingPoint : MonoBehaviour
    {
        public const float DockingRadius = 6f;
        [SerializeField] private Vector3 disembarkOffset = new Vector3(2f, 1f, 0f);
        private BoatController _boat;
        public string DockName = "码头";

        public Vector3 DisembarkPosition => transform.position + transform.TransformDirection(disembarkOffset);

        public void Configure(string dockName, Vector3 exitOffset)
        {
            DockName = dockName;
            disembarkOffset = exitOffset;
        }

        public void BindBoat(BoatController boat)
        {
            _boat = boat;
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.radius = .8f;
            trigger.isTrigger = true;
            var boarding = gameObject.AddComponent<Interactable>();
            boarding.Title = DockName;
            boarding.Prompt = "晨鹱号靠岸后登船";
            boarding.OnInteracted = player =>
            {
                if (_boat != null && !_boat.HasCaptain &&
                    Vector3.Distance(_boat.transform.position, transform.position) < DockingRadius)
                    player.BoardBoat(_boat);
                else
                    PrototypeHUD.SetMessage("晨鹱号尚未靠近这座码头。沿航线寻找小船。");
            };
        }
    }
}
