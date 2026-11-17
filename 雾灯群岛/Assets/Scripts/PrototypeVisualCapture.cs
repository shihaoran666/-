using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FoglightIsles
{
    /// <summary>Captures a review frame from a player build when launched with -capture-review.</summary>
    public sealed class PrototypeVisualCapture : MonoBehaviour
    {
        private bool _interactiveReview;
        private bool _verifyScreenshot;
        private int _reviewFrame;
        private const string VerifyScreenshotArgument = "-verify-playtest-screenshot";

        private void Awake()
        {
            string[] args = Environment.GetCommandLineArgs();
            _verifyScreenshot = Array.IndexOf(args, VerifyScreenshotArgument) >= 0;
            _interactiveReview = _verifyScreenshot || Array.IndexOf(args, "-interactive-review") >= 0;
        }

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-review") >= 0)
                StartCoroutine(Capture());
            if (_verifyScreenshot) StartCoroutine(VerifyScreenshotKey());
        }

        private void Update()
        {
            if (_interactiveReview && Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
                StartCoroutine(SaveInteractiveFrame(_verifyScreenshot));
        }

        private IEnumerator VerifyScreenshotKey()
        {
            yield return null;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F12));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            for (int frame = 0; frame < 180; frame++)
            {
                if (_reviewFrame > 0) yield break;
                yield return null;
            }
            Debug.LogError("[FoglightIsles Screenshot] FAIL — F12 did not start screenshot capture.");
            Application.Quit(1);
        }

        private IEnumerator SaveInteractiveFrame(bool verify)
        {
            string path;
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "Screenshots");
                Directory.CreateDirectory(directory);
                path = Path.Combine(directory, $"Foglight-{DateTime.Now:yyyyMMdd-HHmmss-fff}-{++_reviewFrame:000}.png");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FoglightIsles Screenshot] FAIL — cannot prepare screenshot path: {exception}");
                if (verify) Application.Quit(1);
                yield break;
            }

            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!File.Exists(path) && Time.realtimeSinceStartup < deadline) yield return null;
            if (!File.Exists(path))
            {
                Debug.LogError($"[FoglightIsles Screenshot] FAIL — screenshot was not written to {path}");
                if (verify) Application.Quit(1);
                yield break;
            }
            Debug.Log($"[FoglightIsles Screenshot] PASS — saved to {path}");
            if (verify) Application.Quit(0);
        }

        private static IEnumerator Capture()
        {
            bool captureSmall = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-small-review") >= 0;
            bool captureLarge = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-large-review") >= 0;
            if (captureSmall || captureLarge)
            {
                int width = captureLarge ? 1920 : 640;
                int height = captureLarge ? 1080 : 360;
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                yield return new WaitUntil(() => Screen.width == width && Screen.height == height);
                Debug.Log($"[FoglightIsles Capture] Review resolution {Screen.width}x{Screen.height}");
            }
            yield return null;
            bool captureTitle = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-title-review") >= 0;
            if (!captureTitle) FindFirstObjectByType<StartScreen>()?.BeginExploration();
            bool capturePauseMenu = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-pause-review") >= 0;
            bool captureDialogue = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-dialogue-review") >= 0;
            bool captureMap = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-map-review") >= 0;
            bool captureBoat = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-boat-review") >= 0;
            bool captureShipwright = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-shipwright-review") >= 0;
            bool captureChart = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-chart-review") >= 0;
            bool captureLens = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-lens-review") >= 0;
            bool captureClue = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-clue-review") >= 0;
            bool captureJournal = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-journal-review") >= 0;
            bool captureBehind = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-behind-review") >= 0;
            bool captureDiscoveries = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-discoveries-review") >= 0;
            bool captureChartDisplay = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-chart-display-review") >= 0;
            bool captureSignalLamp = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-signal-lamp-review") >= 0;
            bool captureLighthouse = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-lighthouse-review") >= 0;
            bool captureLighthouseUnlit = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-lighthouse-unlit-review") >= 0;
            bool captureRouteArchive = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-route-archive-review") >= 0;
            bool captureHarbourDock = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-harbour-dock-review") >= 0;
            bool captureTideflatsDock = Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-tideflats-dock-review") >= 0;
            if (captureRouteArchive)
            {
                var discoveries = FindFirstObjectByType<DiscoveryJournal>();
                foreach (DiscoveryJournal.Entry entry in DiscoveryJournal.Catalog)
                    discoveries.Discover(entry.Id, entry.Title);
                FindFirstObjectByType<GameBootstrap>().TalkToEileen();
                DialoguePanel.Show(string.Empty);
                var player = FindFirstObjectByType<PlayerController>();
                player.transform.position = new Vector3(-7.2f, 1f, 9.1f);
                player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
            if (captureChartDisplay || captureSignalLamp)
            {
                var prism = FindFirstObjectByType<QuestState>();
                var chart = FindFirstObjectByType<SurveyChartQuest>();
                var storm = FindFirstObjectByType<StormSignalQuest>();
                prism.Accept();
                prism.CollectPrism();
                prism.HandIn();
                chart.TalkToEileen();
                chart.CollectChart();
                chart.TalkToEileen();
                if (captureSignalLamp)
                {
                    storm.TalkToJon();
                    storm.CollectLens();
                    storm.TalkToJon();
                }
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = captureChartDisplay ? new Vector3(-7f, 1f, 3f) : new Vector3(0f, 1f, -8f);
                    player.transform.rotation = Quaternion.Euler(0f, captureChartDisplay ? -45f : 30f, 0f);
                }
            }
            if (captureLighthouse)
            {
                var prism = FindFirstObjectByType<QuestState>();
                prism.Accept();
                prism.CollectPrism();
                prism.HandIn();
                var player = FindFirstObjectByType<PlayerController>();
                player.transform.position = new Vector3(0f, 1f, -79f);
                player.transform.rotation = Quaternion.identity;
                player.enabled = false;
                var camera = Camera.main;
                if (camera != null)
                {
                    Vector3 eye = new Vector3(0f, 3.5f, -83f);
                    Vector3 target = new Vector3(10f, 8.5f, 18f);
                    camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                }
                if (captureLighthouseUnlit)
                {
                    var beacon = GameObject.Find("Lighthouse Beacon");
                    var light = GameObject.Find("Lighthouse Light")?.GetComponent<Light>();
                    if (beacon != null) beacon.SetActive(false);
                    if (light != null) light.enabled = false;
                }
            }
            if (captureBoat)
            {
                FindFirstObjectByType<QuestState>()?.Accept();
                var player = FindFirstObjectByType<PlayerController>();
                var boat = FindFirstObjectByType<BoatController>();
                if (player != null && boat != null)
                {
                    player.BoardBoat(boat);
                    boat.SimulateControl(1f, 0f, 1f);
                }
            }
            if (captureShipwright)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = new Vector3(1f, 1f, 4f);
                    player.transform.rotation = Quaternion.identity;
                }
            }
            if (captureHarbourDock || captureTideflatsDock)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) player.enabled = false;
                var camera = Camera.main;
                if (camera != null)
                {
                    Vector3 eye = captureHarbourDock ? new Vector3(0f, 2f, -18f) : new Vector3(0f, 2f, -46f);
                    Vector3 target = captureHarbourDock ? new Vector3(0f, .6f, -3.2f) : new Vector3(0f, .6f, -63f);
                    camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                }
            }
            if (captureChart || captureLens)
            {
                var player = FindFirstObjectByType<PlayerController>();
                var prismQuest = FindFirstObjectByType<QuestState>();
                var chartQuest = FindFirstObjectByType<SurveyChartQuest>();
                prismQuest.Accept();
                prismQuest.CollectPrism();
                prismQuest.HandIn();
                ProgressStore.UnlockSurveySail();
                chartQuest.TalkToEileen();
                if (captureLens)
                {
                    chartQuest.ReadRuinClue();
                    chartQuest.CollectChart();
                    chartQuest.TalkToEileen();
                    FindFirstObjectByType<StormSignalQuest>().TalkToJon();
                }
                if (player != null)
                {
                    player.transform.position = captureChart ? new Vector3(8.3f, .05f, -79f) : new Vector3(-4.6f, .05f, -86f);
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
            if (captureClue)
            {
                FindFirstObjectByType<QuestState>()?.Accept();
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = new Vector3(-3f, .05f, -69f);
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
            if (captureBehind)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) player.transform.rotation = Quaternion.Euler(0f, 70f, 0f);
            }
            if (captureDialogue) DialoguePanel.Show("乔恩，船匠",
                "试玩版主线已完成。你可以继续自由探索晨湾港与潮汐浅滩，寻找尚未发现的线索。感谢试玩《雾灯群岛》！");
            if (capturePauseMenu) FindFirstObjectByType<PauseMenu>()?.OpenForReview();
            else if (captureMap) FindFirstObjectByType<MapOverlay>()?.OpenForReview();
            else if (captureJournal) FindFirstObjectByType<QuestJournalPanel>()?.OpenForReview();
            else if (captureDiscoveries)
            {
                var discoveries = FindFirstObjectByType<DiscoveryJournal>();
                discoveries?.Discover(DiscoveryJournal.Catalog[0].Id, DiscoveryJournal.Catalog[0].Title);
                discoveries?.Discover(DiscoveryJournal.Catalog[2].Id, DiscoveryJournal.Catalog[2].Title);
                FindFirstObjectByType<QuestJournalPanel>()?.OpenDiscoveriesForReview();
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(Application.dataPath,
                captureTitle ? "../review-title.png" : capturePauseMenu ? "../review-pause.png" : captureDialogue ? "../review-dialogue.png" : captureMap ? "../review-map.png" :
                captureBoat ? "../review-boat.png" : captureShipwright ? "../review-shipwright.png" :
                captureHarbourDock ? "../review-harbour-dock.png" : captureTideflatsDock ? "../review-tideflats-dock.png" : captureChart ? "../review-chart.png" :
                captureLens ? "../review-lens.png" : captureClue ? "../review-clue.png" :
                captureJournal ? "../review-journal.png" : captureBehind ? "../review-behind.png" :
                captureDiscoveries ? "../review-discoveries.png" :
                captureChartDisplay ? "../review-chart-display.png" : captureSignalLamp ? "../review-signal-lamp.png" :
                captureRouteArchive ? "../review-route-archive.png" :
                captureLighthouseUnlit ? "../review-lighthouse-unlit.png" : captureLighthouse ? "../review-lighthouse-lit.png" : "../review-harbour.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[FoglightIsles Capture] Saved review frame to {path}");
            if (captureLighthouse && Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-lighthouse-sweep-review") >= 0)
            {
                for (int step = 1; step <= 3; step++)
                {
                    yield return new WaitForSecondsRealtime(1.25f);
                    yield return new WaitForEndOfFrame();
                    string sweepPath = Path.Combine(Application.dataPath, $"../review-lighthouse-sweep-{step}.png");
                    ScreenCapture.CaptureScreenshot(sweepPath);
                    Debug.Log($"[FoglightIsles Capture] Saved review frame to {sweepPath}");
                }
            }
            yield return new WaitForSecondsRealtime(2f);
            Application.Quit(0);
        }
    }
}
