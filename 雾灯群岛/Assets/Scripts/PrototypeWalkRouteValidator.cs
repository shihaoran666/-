using System;
using System.Collections;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Checks harbour and island routes with the player's actual CharacterController collisions.</summary>
    public sealed class PrototypeWalkRouteValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-walk-route";
        private const float Step = .12f;

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(VerifyRoute());
        }

        private static IEnumerator VerifyRoute()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            DockingPoint dock = null;
            foreach (DockingPoint point in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (point.DockName == "潮汐浅滩码头") dock = point;
            if (player == null || dock == null)
            {
                Fail("player or Tideflats dock is missing");
                yield break;
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            (string name, Vector2 point, string expectedTitle)[] harbourRoute =
            {
                ("港口告示牌", new Vector2(-5.7f, 6.5f), "港口告示牌"),
                ("航线路标", new Vector2(-2.8f, 4.4f), "路标：潮汐浅滩航线"),
                ("主码头入口", new Vector2(0f, 2.5f), null),
                ("晨湾港码头", new Vector2(0f, -1.6f), "晨湾港码头")
            };
            foreach ((string name, Vector2 point, string expectedTitle) in harbourRoute)
            {
                if (!WalkTo(player, controller, point))
                {
                    Fail($"cannot walk from the harbour spawn to {name}");
                    yield break;
                }
                if (!HasGround(player.transform.position, true))
                {
                    Fail($"no solid harbour ground or pier below {name}");
                    yield break;
                }
                if (expectedTitle != null && player.FindNearbyInteractable()?.Title != expectedTitle)
                {
                    Fail($"cannot select {name} after walking to it");
                    yield break;
                }
            }
            controller.enabled = false;
            player.transform.position = dock.DisembarkPosition;
            controller.enabled = true;
            Physics.SyncTransforms();

            (string name, Vector2 point)[] route =
            {
                ("路标", new Vector2(3.5f, -72.2f)),
                ("石碑", new Vector2(-3f, -72.2f)),
                ("西侧信标", new Vector2(-13f, -77.5f)),
                ("棱镜", new Vector2(-8.2f, -86.8f)),
                ("中央信标", new Vector2(0f, -91.5f)),
                ("透镜", new Vector2(-4.6f, -89.8f)),
                ("遗迹南侧", new Vector2(3f, -79f)),
                ("航图", new Vector2(8.3f, -82.5f)),
                ("东侧信标", new Vector2(13f, -79.5f)),
                ("返航小径", new Vector2(6f, -70f)),
                ("浅滩码头", new Vector2(1.4f, -66.5f))
            };
            foreach ((string name, Vector2 point) in route)
            {
                if (!WalkTo(player, controller, point))
                {
                    Fail($"cannot walk to {name} from the preceding checkpoint");
                    yield break;
                }
                if (name == "路标")
                {
                    Interactable sign = player.FindNearbyInteractable();
                    if (sign?.Title != "路标：潮汐遗迹")
                    {
                        Fail("the Tideflats route sign cannot be selected from the path");
                        yield break;
                    }
                    sign.Interact(player);
                    if (!PrototypeHUD.CurrentMessage.Contains("蓝纹石碑") || !PrototypeHUD.CurrentMessage.Contains("遗迹入口"))
                    {
                        Fail("the Tideflats route sign does not explain the destinations");
                        yield break;
                    }
                }
                if (!HasGround(player.transform.position, false))
                {
                    Fail($"no solid island or pier below {name}");
                    yield break;
                }
            }
            if (player.FindNearbyInteractable()?.gameObject != dock.gameObject)
            {
                Fail("the player cannot select the dock after walking back");
                yield break;
            }
            Debug.Log("[FoglightIsles Walk Route] PASS — CharacterController reached harbour interactions, island objectives and both docks on solid ground.");
            Application.Quit(0);
        }

        private static bool WalkTo(PlayerController player, CharacterController controller, Vector2 destination)
        {
            for (int step = 0; step < 500; step++)
            {
                Vector2 current = new Vector2(player.transform.position.x, player.transform.position.z);
                Vector2 toDestination = destination - current;
                if (toDestination.magnitude < .55f) return true;
                Vector2 planarStep = Vector2.ClampMagnitude(toDestination, Step);
                controller.Move(new Vector3(planarStep.x, -.25f, planarStep.y));
                if (player.transform.position.y < -2.5f) return false;
            }
            return false;
        }

        private static bool HasGround(Vector3 position, bool harbour)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(position + Vector3.up * .4f, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore))
            {
                string name = hit.collider.gameObject.name;
                if (harbour && (name == "Morning Bay Shore" || name == "Main Dock")) return true;
                if (!harbour && (name == "Tideflats Island" || name == "Tideflats Dock")) return true;
            }
            return false;
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"[FoglightIsles Walk Route] FAIL — {reason}.");
            Application.Quit(1);
        }
    }
}
