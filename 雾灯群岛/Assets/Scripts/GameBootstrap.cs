using System;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Creates the first-week harbour greybox so no hand-authored assets are required yet.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        // Art plan A4: the restored lighthouse needs a body of light readable from the tideflats
        // (~94 m away). A point light fades under the fog long before that distance, so the beam
        // carries the state change instead of the light.
        private const float LighthouseBeamLength = 58f;
        private const float LighthouseBeamRadius = .9f;

        private Material _wood;
        private Material _stone;
        private Material _water;
        private Material _warmLight;
        private QuestState _quest;
        private TideBeaconPuzzle _tidePuzzle;
        private SurveyChartQuest _surveyQuest;
        private StormSignalQuest _stormSignalQuest;
        private DiscoveryJournal _discoveries;
        private InteractionCueAudio _cueAudio;
        private BoatController _boat;
        private GameObject _prism;
        private GameObject _surveyChart;
        private GameObject _stormLens;
        private Transform _stormVane;
        private GameObject _lighthouseBeacon;
        private Light _lighthouseLight;
        private GameObject _harbourChartDisplay;
        private GameObject _harbourRouteArchive;
        private GameObject _signalLamp;
        private Light _signalLight;
        private GuidanceMarker _guidance;
        private Transform _noticeBoard;
        private Transform _harbourManager;
        private Transform _shipwright;
        private Transform _lighthouseTarget;
        private PlayerController _player;
        private DockingPoint _morningBayDock;
        private DockingPoint _tideflatsDock;
        private Transform _tideClueTablet;
        private Transform _surveyClueTablet;
        private Transform _tideBeaconSearchArea;
        private bool _tideClueRead;
        private readonly GameObject[] HarbourBeaconPylons = new GameObject[3];

        internal bool IsHarbourRouteArchiveVisible => _harbourRouteArchive != null && _harbourRouteArchive.activeSelf;

        private void Awake()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-quest-loop") >= 0 ||
                Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-beacon-partial-prepare") >= 0)
                ProgressStore.ClearPrototypeProgress();
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verify-discoveries") >= 0)
                DiscoveryJournal.ClearProgress();
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capture-review") >= 0)
            {
                ProgressStore.ClearPrototypeProgress();
                DiscoveryJournal.ClearProgress();
            }
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            _quest = gameObject.AddComponent<QuestState>();
            _tidePuzzle = gameObject.AddComponent<TideBeaconPuzzle>();
            _tidePuzzle.Configure(_quest);
            _surveyQuest = gameObject.AddComponent<SurveyChartQuest>();
            _surveyQuest.Configure(_quest);
            _stormSignalQuest = gameObject.AddComponent<StormSignalQuest>();
            _stormSignalQuest.Configure(_surveyQuest);
            _discoveries = gameObject.AddComponent<DiscoveryJournal>();
            _cueAudio = gameObject.AddComponent<InteractionCueAudio>();
            _tidePuzzle.Activated += success => _cueAudio.Play(success ? InteractionCue.BeaconSuccess : InteractionCue.BeaconFailure);
            BuildMaterials();
            BuildLighting();
            BuildSoundscape();
            BuildSea();
            BuildHarbour();
            BuildTideflats();
            BuildDiscoveries();
            BuildBoat();
            BuildPlayer();
            _guidance = gameObject.AddComponent<GuidanceMarker>();
            _quest.Changed += RefreshWorldForQuest;
            _tidePuzzle.Changed += () => RefreshWorldForQuest(_quest.Stage);
            _surveyQuest.Changed += _ => RefreshWorldForQuest(_quest.Stage);
            _stormSignalQuest.Changed += _ => RefreshWorldForQuest(_quest.Stage);
            RefreshWorldForQuest(_quest.Stage);
            gameObject.AddComponent<InputPromptMode>();
            gameObject.AddComponent<StartScreen>();
            gameObject.AddComponent<PrototypeHUD>();
            gameObject.AddComponent<PauseMenu>();
            gameObject.AddComponent<PrototypeFocusPauseValidator>();
            gameObject.AddComponent<PrototypeInputPromptValidator>();
            gameObject.AddComponent<DialoguePanel>();
            gameObject.AddComponent<CompassHUD>();
            gameObject.AddComponent<MapOverlay>();
            gameObject.AddComponent<QuestJournalPanel>();
            gameObject.AddComponent<PrototypeRuntimeValidator>();
            gameObject.AddComponent<PrototypeJourneyValidator>();
            gameObject.AddComponent<PrototypeGamepadLookValidator>();
            gameObject.AddComponent<PrototypeGamepadVoyageValidator>();
            gameObject.AddComponent<PrototypeKeyboardJourneyValidator>();
            gameObject.AddComponent<PrototypeKeyboardChartValidator>();
            gameObject.AddComponent<PrototypeKeyboardStormValidator>();
            gameObject.AddComponent<PrototypeKeyboardFreeRoamValidator>();
            gameObject.AddComponent<PrototypeWalkRouteValidator>();
            gameObject.AddComponent<PrototypeQuestLoopValidator>();
            gameObject.AddComponent<PrototypeDiscoveryValidator>();
            gameObject.AddComponent<PrototypeNewSaveValidator>();
            gameObject.AddComponent<PrototypeChartOldSaveValidator>();
            gameObject.AddComponent<PrototypeStormOldSaveValidator>();
            gameObject.AddComponent<PrototypePerformanceValidator>();
            gameObject.AddComponent<PrototypeVisualCapture>();
        }

        private void Update()
        {
            RefreshJourneyGuidance();
        }

        private void BuildMaterials()
        {
            _wood = CreateMaterial(new Color(0.22f, 0.13f, 0.07f));
            _stone = CreateMaterial(new Color(0.34f, 0.37f, 0.36f));
            _water = CreateMaterial(new Color(0.08f, 0.28f, 0.36f));
            _warmLight = CreateMaterial(new Color(1f, 0.52f, 0.16f), true);
        }

        private static Material CreateMaterial(Color color, bool emission = false)
        {
            // The resource material keeps this shader included in player builds. Clone it so every
            // greybox object can still receive its own color at runtime.
            Material template = Resources.Load<Material>("FoglightRuntimeMaterial");
            if (template == null)
            {
                Debug.LogError("[FoglightIsles] Runtime material resource is missing from this build.");
                return null;
            }
            var material = new Material(template) { color = color };
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.5f);
            }
            return material;
        }

        private void BuildLighting()
        {
            RenderSettings.ambientLight = new Color(.2f, .25f, .31f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.34f, .44f, .49f);
            RenderSettings.fogDensity = .009f;
            var sun = new GameObject("Evening Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.67f, 0.45f);
            sun.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
        }

        private void BuildSoundscape()
        {
            gameObject.AddComponent<AmbientSoundscape>();
        }

        private void BuildSea()
        {
            var sea = Primitive(PrimitiveType.Plane, "Mist Sea", new Vector3(0, -0.12f, 0), new Vector3(20, 1, 20), _water);
            sea.layer = LayerMask.NameToLayer("Default");
            sea.GetComponent<Collider>().enabled = false;
        }

        private void BuildHarbour()
        {
            var land = Primitive(PrimitiveType.Cube, "Morning Bay Shore", new Vector3(0, -0.6f, 12f), new Vector3(34f, 1f, 28f), _stone);
            land.transform.rotation = Quaternion.Euler(0f, -8f, 0f);

            BuildDock(new Vector3(0f, 0.15f, 3f), new Vector3(4f, 0.3f, 13f), "Main Dock");
            BuildDock(new Vector3(-4.4f, 0.15f, -1.1f), new Vector3(5f, 0.3f, 4f), "West Pier");
            BuildDock(new Vector3(4.4f, 0.15f, -1.1f), new Vector3(5f, 0.3f, 4f), "East Pier");

            BuildHouse(new Vector3(-9f, 1.2f, 13f), new Vector3(4f, 2.4f, 3.5f), "Harbour Office");
            BuildHouse(new Vector3(-3.5f, 1.15f, 15f), new Vector3(3.5f, 2.3f, 3f), "Boathouse");
            BuildHouse(new Vector3(4.8f, 1.35f, 13.5f), new Vector3(5.5f, 2.7f, 4f), "Shipwright");
            BuildLighthouse(new Vector3(10f, 0f, 18f));
            BuildHarbourRewards();
            BuildHarbourRouteArchive();
            BuildTaskBoard();
            BuildLantern(new Vector3(-2f, 1f, 7f));
            BuildLantern(new Vector3(2f, 1f, 7f));
            BuildLantern(new Vector3(-7f, 1f, 10.5f));
            BuildSignpost(new Vector3(-1.8f, 1.2f, 5.8f), "潮汐浅滩航线", "从主码头登上晨鹱号，向南航行到潮汐浅滩码头。靠岸时减速，再按 E 下船。");
            BuildSignpost(new Vector3(7.2f, 1.2f, 8.5f), "灯塔栈桥", "晨湾港灯塔在东侧。带回棱镜交给艾琳，才能重新点亮灯塔。");
            _morningBayDock = BuildDockingPoint(new Vector3(0f, .25f, -3.2f), "晨湾港码头", new Vector3(-1.4f, 1f, 0f));
            BuildDockGuide(_morningBayDock.transform.position, "晨湾港");
        }

        private void BuildTideflats()
        {
            var island = Primitive(PrimitiveType.Cube, "Tideflats Island", new Vector3(0f, -.55f, -82f), new Vector3(38f, 1f, 32f), _stone);
            island.transform.rotation = Quaternion.Euler(0f, 11f, 0f);
            BuildDock(new Vector3(0f, .15f, -67f), new Vector3(4f, .3f, 9f), "Tideflats Dock");
            _tideflatsDock = BuildDockingPoint(new Vector3(0f, .25f, -66.5f), "潮汐浅滩码头", new Vector3(1.4f, 1f, 0f));
            // The tideflats trigger is 3.5 m behind the sea-facing end; mark the end so the boat need not cross the deck.
            BuildDockGuide(new Vector3(0f, .25f, -63f), "潮汐浅滩");
            _tideBeaconSearchArea = new GameObject("Tide Beacon Search Area").transform;
            _tideBeaconSearchArea.position = new Vector3(0f, 1f, -84f);

            Primitive(PrimitiveType.Cylinder, "Collapsed Tide Lighthouse", new Vector3(-8f, 2f, -91f), new Vector3(2.6f, 2f, 2.6f), _stone);
            Primitive(PrimitiveType.Cube, "Collapsed Beacon", new Vector3(-6.4f, 4.2f, -90f), new Vector3(1.2f, 1.2f, 1.2f), _warmLight);
            Primitive(PrimitiveType.Cube, "Tideflats Ruin", new Vector3(8f, 1.4f, -87f), new Vector3(6f, 2.8f, 4f), _wood);
            BuildTideBeacon(0, new Vector3(-13f, .85f, -79f), "西侧潮汐信标");
            BuildTideBeacon(1, new Vector3(0f, .85f, -93f), "中央潮汐信标");
            BuildTideBeacon(2, new Vector3(13f, .85f, -81f), "东侧潮汐信标");
            BuildSignpost(new Vector3(3.5f, 1.15f, -73.5f), "潮汐遗迹", "码头西侧的蓝纹石碑记着信标顺序；遗迹入口在岛屿东侧，坍塌灯塔在西南侧。");
            BuildTideClue();
            BuildRock(new Vector3(-17f, .1f, -75f), new Vector3(2.1f, .8f, 1.6f));
            BuildRock(new Vector3(-10f, .1f, -72f), new Vector3(1.1f, .5f, .9f));
            BuildRock(new Vector3(10f, .1f, -74f), new Vector3(1.5f, .65f, 1.1f));
            BuildRock(new Vector3(17f, .1f, -85f), new Vector3(2.3f, 1f, 1.7f));
            BuildRock(new Vector3(-16f, .1f, -91f), new Vector3(1.7f, .75f, 1.2f));
            BuildRock(new Vector3(4.5f, .1f, -96f), new Vector3(1.2f, .5f, 1.5f));
            _prism = Primitive(PrimitiveType.Sphere, "灯塔棱镜零件", new Vector3(-8.2f, 1.25f, -88.2f), Vector3.one * .7f, _warmLight);
            var prismInteraction = _prism.AddComponent<Interactable>();
            prismInteraction.Title = "破损的灯塔棱镜";
            prismInteraction.OnInteracted = _ =>
            {
                _quest.CollectPrism();
                if (_quest.Stage == PrismQuestStage.ReturnToHarbour) _prism.SetActive(false);
            };
            Primitive(PrimitiveType.Cube, "遗迹航图石台", new Vector3(8.3f, .25f, -83.8f), new Vector3(1.2f, .5f, .8f), _stone);
            _surveyChart = Primitive(PrimitiveType.Cube, "失落航图", new Vector3(8.3f, 1f, -83.8f), new Vector3(1f, .7f, .08f), CreateMaterial(new Color(.86f, .76f, .51f)));
            var chartInk = CreateMaterial(new Color(.12f, .31f, .36f));
            var upperRoute = Primitive(PrimitiveType.Cube, "航图北线", new Vector3(8.3f, 1.1f, -83.74f), new Vector3(.72f, .035f, .02f), chartInk);
            upperRoute.transform.SetParent(_surveyChart.transform, true);
            Destroy(upperRoute.GetComponent<Collider>());
            var lowerRoute = Primitive(PrimitiveType.Cube, "航图南线", new Vector3(8.3f, .88f, -83.74f), new Vector3(.55f, .035f, .02f), chartInk);
            lowerRoute.transform.SetParent(_surveyChart.transform, true);
            Destroy(lowerRoute.GetComponent<Collider>());
            var chartInteraction = _surveyChart.AddComponent<Interactable>();
            chartInteraction.Title = "失落航图";
            chartInteraction.Prompt = "拾取航图";
            chartInteraction.OnInteracted = _ =>
            {
                _surveyQuest.CollectChart();
                if (_surveyQuest.Stage == SurveyChartStage.ReturnToHarbour) _surveyChart.SetActive(false);
            };
            BuildSurveyChartClue();
            Primitive(PrimitiveType.Cylinder, "风暴透镜石座", new Vector3(-4.6f, .3f, -91.5f), new Vector3(.8f, .3f, .8f), _stone);
            _stormLens = Primitive(PrimitiveType.Sphere, "风暴透镜", new Vector3(-4.6f, .85f, -91.5f), Vector3.one * .7f,
                CreateMaterial(new Color(.35f, .8f, .95f), true));
            var lensInteraction = _stormLens.AddComponent<Interactable>();
            lensInteraction.Title = "风暴透镜";
            lensInteraction.Prompt = "拾取透镜";
            lensInteraction.OnInteracted = _ =>
            {
                _stormSignalQuest.CollectLens();
                if (_stormSignalQuest.Stage == StormSignalStage.ReturnToShipwright) _stormLens.SetActive(false);
            };
            BuildStormVane();
            BuildLantern(new Vector3(2f, 1f, -68f));
        }

        private void BuildStormVane()
        {
            var vane = Primitive(PrimitiveType.Cylinder, "风向标", new Vector3(-7f, 1.25f, -87.4f),
                new Vector3(.42f, 2.3f, .42f), _stone);
            var accent = CreateMaterial(new Color(.14f, .72f, .83f), true);
            var baseRing = Primitive(PrimitiveType.Cylinder, "风向标蓝纹底座", new Vector3(-7f, .12f, -87.4f),
                new Vector3(1.05f, .2f, 1.05f), accent);
            baseRing.transform.SetParent(vane.transform, true);
            Destroy(baseRing.GetComponent<Collider>());
            var topCap = Primitive(PrimitiveType.Cube, "风向标顶盖", vane.transform.position + Vector3.up * 1.18f,
                new Vector3(.72f, .14f, .72f), accent);
            topCap.transform.SetParent(vane.transform, true);
            Destroy(topCap.GetComponent<Collider>());
            var arrowMaterial = CreateMaterial(new Color(.95f, .75f, .26f), true);
            var pointer = Primitive(PrimitiveType.Cube, "风向标箭头", vane.transform.position + new Vector3(0f, 1.3f, .48f),
                new Vector3(.18f, .16f, 1.45f), arrowMaterial);
            pointer.transform.SetParent(vane.transform, true);
            Destroy(pointer.GetComponent<Collider>());
            var arrowHead = Primitive(PrimitiveType.Cube, "风向标箭头尖端", vane.transform.position + new Vector3(0f, 1.3f, 1.15f),
                new Vector3(.48f, .16f, .48f), arrowMaterial);
            arrowHead.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            arrowHead.transform.SetParent(vane.transform, true);
            Destroy(arrowHead.GetComponent<Collider>());
            var interaction = vane.AddComponent<Interactable>();
            interaction.Title = "风向标";
            interaction.Prompt = "旋转箭头至晨湾港";
            interaction.OnInteracted = _ =>
            {
                _stormSignalQuest.RotateVane();
                vane.transform.rotation = Quaternion.Euler(0f, _stormSignalQuest.DisplayVaneDirection * 90f, 0f);
            };
            vane.transform.rotation = Quaternion.Euler(0f, _stormSignalQuest.DisplayVaneDirection * 90f, 0f);
            _stormVane = vane.transform;
        }

        private void BuildDiscoveries()
        {
            BuildDiscovery(DiscoveryJournal.Catalog[0], new Vector3(-10.2f, 1.15f, 10.8f));
            BuildDiscovery(DiscoveryJournal.Catalog[1], new Vector3(6.5f, .28f, 9.5f));
            var tidePool = Primitive(PrimitiveType.Cylinder, "贝壳浅水洼", new Vector3(-15.2f, -.04f, -83.4f),
                new Vector3(2.4f, .035f, 1.8f), CreateMaterial(new Color(.13f, .48f, .53f)));
            tidePool.GetComponent<Collider>().enabled = false;
            BuildDiscovery(DiscoveryJournal.Catalog[2], new Vector3(-15.2f, .30f, -83.4f));
            var cairn = Primitive(PrimitiveType.Cylinder, "守望者旧石台", new Vector3(14.5f, .05f, -88.7f),
                new Vector3(2.3f, .13f, 2.3f), CreateMaterial(new Color(.17f, .27f, .33f)));
            cairn.GetComponent<Collider>().enabled = false;
            BuildDiscovery(DiscoveryJournal.Catalog[3], new Vector3(14.5f, .52f, -88.7f));
        }

        private void BuildDiscovery(DiscoveryJournal.Entry entry, Vector3 position)
        {
            PrimitiveType shape = entry.Id switch
            {
                "old-compass" => PrimitiveType.Cylinder,
                "tide-shell" => PrimitiveType.Sphere,
                _ => PrimitiveType.Cube
            };
            Vector3 scale = entry.Id switch
            {
                "harbour-log" => new Vector3(.6f, .14f, .42f),
                "old-compass" => new Vector3(.78f, .12f, .78f),
                "tide-shell" => new Vector3(.8f, .5f, .62f),
                _ => new Vector3(.55f, .62f, .14f)
            };
            Color color = entry.Id switch
            {
                "harbour-log" => new Color(.74f, .48f, .22f),
                "old-compass" => new Color(.83f, .68f, .27f),
                "tide-shell" => new Color(.43f, .82f, .86f),
                _ => new Color(.59f, .76f, .95f)
            };
            var item = Primitive(shape, "发现物 - " + entry.Title, position, scale, CreateMaterial(color, true));
            if (entry.Id == "harbour-log")
                AddDiscoveryAccent(item, PrimitiveType.Cube, "日志纸页", position + Vector3.up * .09f,
                    new Vector3(.48f, .025f, .32f), CreateMaterial(new Color(.92f, .85f, .66f)));
            else if (entry.Id == "old-compass")
                AddDiscoveryAccent(item, PrimitiveType.Cube, "罗盘指针", position + Vector3.up * .12f,
                    new Vector3(.5f, .025f, .06f), CreateMaterial(new Color(.2f, .36f, .48f)));
            else if (entry.Id == "tide-shell")
                AddDiscoveryAccent(item, PrimitiveType.Sphere, "贝壳珍珠", position + new Vector3(0f, .14f, -.25f),
                    Vector3.one * .16f, CreateMaterial(new Color(.9f, .96f, .94f), true));
            else
            {
                item.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
                AddDiscoveryAccent(item, PrimitiveType.Cube, "护符刻印", position + new Vector3(0f, 0f, -.065f),
                    new Vector3(.22f, .22f, .025f), CreateMaterial(new Color(.17f, .32f, .43f)));
            }
            var pickup = item.AddComponent<DiscoveryPickup>();
            pickup.Configure(_discoveries, entry.Id, entry.Title);
            var interaction = item.AddComponent<Interactable>();
            interaction.Title = entry.Title;
            interaction.Prompt = "记录发现";
            interaction.OnInteracted = _ =>
            {
                if (pickup.Collect()) _cueAudio.Play(InteractionCue.Discovery);
            };
        }

        private static void AddDiscoveryAccent(GameObject item, PrimitiveType shape, string name,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject accent = Primitive(shape, name, position, scale, material);
            accent.transform.SetParent(item.transform, true);
            UnityEngine.Object.Destroy(accent.GetComponent<Collider>());
        }

        private void BuildTideBeacon(int index, Vector3 position, string name)
        {
            Primitive(PrimitiveType.Cylinder, name + " 石座", position - Vector3.up * .5f, new Vector3(1.3f, .5f, 1.3f), _stone);
            var beacon = Primitive(PrimitiveType.Sphere, name, position + Vector3.up * .65f, Vector3.one * .65f, _warmLight);
            var glow = beacon.AddComponent<Light>();
            glow.range = 7f;
            glow.intensity = 2.6f;
            glow.color = new Color(1f, .44f, .08f);
            glow.enabled = false;
            var raisedCap = Primitive(PrimitiveType.Cylinder, name + "点亮柱",
                position + Vector3.up * 1.4f, new Vector3(.18f, .5f, .18f), _warmLight);
            raisedCap.transform.SetParent(beacon.transform, true);
            var capCollider = raisedCap.GetComponent<Collider>();
            capCollider.enabled = false;
            Destroy(capCollider);
            _tidePuzzle.RegisterBeacon(index, beacon.transform, raisedCap);
            var interaction = beacon.AddComponent<Interactable>();
            interaction.Title = name;
            interaction.Prompt = "点亮潮汐信标";
            interaction.OnInteracted = _ => _tidePuzzle.Activate(index);
        }

        private void BuildTideClue()
        {
            var tablet = Primitive(PrimitiveType.Cube, "潮汐箭纹石碑", new Vector3(-3f, .9f, -74f),
                new Vector3(1.5f, 1.8f, .28f), CreateMaterial(new Color(.1f, .24f, .28f)));
            var carving = CreateMaterial(new Color(.35f, .8f, .95f), true);
            var crown = Primitive(PrimitiveType.Cube, "石碑发光冠饰", tablet.transform.position + Vector3.up * .95f,
                new Vector3(1.65f, .12f, .35f), carving);
            crown.transform.SetParent(tablet.transform, true);
            Destroy(crown.GetComponent<Collider>());
            for (int index = 0; index < 3; index++)
            {
                var mark = Primitive(PrimitiveType.Cube, "石碑箭纹 " + (index + 1),
                    tablet.transform.position + new Vector3((index - 1) * .43f, .2f, .16f),
                    new Vector3(.22f, .2f, .035f), carving);
                mark.transform.SetParent(tablet.transform, true);
                Destroy(mark.GetComponent<Collider>());
            }
            var interaction = tablet.AddComponent<Interactable>();
            interaction.Title = "潮汐箭纹石碑";
            interaction.Prompt = "查看信标顺序";
            interaction.OnInteracted = _ =>
            {
                _tideClueRead = true;
                ProgressStore.SaveTideClueRead();
                PrototypeHUD.SetMessage(
                    "石碑上的灯纹从西向东排列：西侧信标 → 中央信标 → 东侧信标。");
            };
            _tideClueTablet = tablet.transform;
        }

        // Second quest's gating step: the chart on the table cannot be taken until this
        // inscription has been read. Mirrors BuildTideClue so both clues persist the same way.
        private void BuildSurveyChartClue()
        {
            var tablet = Primitive(PrimitiveType.Cube, "遗迹残碑", new Vector3(6.6f, .9f, -84.6f),
                new Vector3(1.1f, 1.4f, .24f), CreateMaterial(new Color(.16f, .2f, .26f)));
            var carving = CreateMaterial(new Color(.72f, .63f, .42f), true);
            var crown = Primitive(PrimitiveType.Cube, "残碑发光冠饰", tablet.transform.position + Vector3.up * .74f,
                new Vector3(1.22f, .1f, .3f), carving);
            crown.transform.SetParent(tablet.transform, true);
            Destroy(crown.GetComponent<Collider>());
            for (int index = 0; index < 2; index++)
            {
                var mark = Primitive(PrimitiveType.Cube, "残碑航纹 " + (index + 1),
                    tablet.transform.position + new Vector3((index - .5f) * .38f, .16f, .13f),
                    new Vector3(.3f, .05f, .03f), carving);
                mark.transform.SetParent(tablet.transform, true);
                Destroy(mark.GetComponent<Collider>());
            }
            var interaction = tablet.AddComponent<Interactable>();
            interaction.Title = "遗迹残碑";
            interaction.Prompt = "辨认航图残记";
            interaction.OnInteracted = _ => _surveyQuest.ReadRuinClue();
            _surveyClueTablet = tablet.transform;
        }

        private void BuildBoat()
        {
            var boat = new GameObject("Skiff - Dawn Petrel");
            boat.transform.position = new Vector3(0f, .38f, -5.4f);
            boat.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _boat = boat.AddComponent<BoatController>();
            _morningBayDock.BindBoat(_boat);
            _tideflatsDock.BindBoat(_boat);
            if (ProgressStore.HasSurveySail) _boat.ApplySurveySail();
            var board = boat.AddComponent<Interactable>();
            board.Title = "晨鹱号";
            board.Prompt = "登上小船";
            board.OnInteracted = player => player.BoardBoat(_boat);

            var hull = Primitive(PrimitiveType.Cube, "Boat Hull", boat.transform.position, new Vector3(1.8f, .55f, 4.2f), _wood);
            hull.transform.SetParent(boat.transform, true);
            var deck = Primitive(PrimitiveType.Cube, "Boat Deck", boat.transform.position + Vector3.up * .35f, new Vector3(1.45f, .18f, 2.3f), _stone);
            deck.transform.SetParent(boat.transform, true);
            var mast = Primitive(PrimitiveType.Cylinder, "Boat Mast", boat.transform.position + Vector3.up * 1.55f, new Vector3(.12f, 1.55f, .12f), _wood);
            mast.transform.SetParent(boat.transform, true);
            var sail = Primitive(PrimitiveType.Cube, "Boat Sail", boat.transform.position + new Vector3(1.05f, 2.7f, .2f), new Vector3(.05f, .9f, 1.1f), _warmLight);
            sail.transform.SetParent(boat.transform, true);
        }

        private static DockingPoint BuildDockingPoint(Vector3 position, string dockName, Vector3 disembarkOffset)
        {
            var dock = new GameObject(dockName);
            dock.transform.position = position;
            var dockingPoint = dock.AddComponent<DockingPoint>();
            dockingPoint.Configure(dockName, disembarkOffset);
            return dockingPoint;
        }

        private void BuildDock(Vector3 position, Vector3 scale, string name)
        {
            Primitive(PrimitiveType.Cube, name, position, scale, _wood);
            for (var z = -scale.z / 2f + 1f; z < scale.z / 2f; z += 3f)
            {
                Primitive(PrimitiveType.Cylinder, name + " Post", position + new Vector3(-scale.x / 2f + .4f, .75f, z), new Vector3(.25f, 1.5f, .25f), _wood);
                Primitive(PrimitiveType.Cylinder, name + " Post", position + new Vector3(scale.x / 2f - .4f, .75f, z), new Vector3(.25f, 1.5f, .25f), _wood);
            }
        }

        private void BuildDockGuide(Vector3 dockingPosition, string name)
        {
            // Three collision-free warm marks show the usable docking side before the 6 m docking radius is reached.
            for (int side = -1; side <= 1; side += 2)
            {
                var post = Primitive(PrimitiveType.Cube, name + "靠岸边柱",
                    dockingPosition + new Vector3(side * 1.55f, .5f, 0f),
                    new Vector3(.18f, .9f, .18f), _warmLight);
                post.GetComponent<Collider>().enabled = false;
                Destroy(post.GetComponent<Collider>());
            }
            var edge = Primitive(PrimitiveType.Cube, name + "靠岸边线",
                dockingPosition + new Vector3(0f, .09f, 0f),
                new Vector3(3.25f, .06f, .18f), _warmLight);
            edge.GetComponent<Collider>().enabled = false;
            Destroy(edge.GetComponent<Collider>());
        }

        private void BuildRock(Vector3 position, Vector3 scale)
        {
            var rock = Primitive(PrimitiveType.Sphere, "潮蚀岩", position, scale, _stone);
            rock.transform.rotation = Quaternion.Euler(0f, position.x * 13f, position.z * 4f);
        }

        private void BuildSignpost(Vector3 position, string name, string directions)
        {
            var post = Primitive(PrimitiveType.Cylinder, "路标柱 - " + name, position, new Vector3(.1f, 1.25f, .1f), _wood);
            var board = Primitive(PrimitiveType.Cube, "路标 - " + name, position + new Vector3(0f, .95f, 0f), new Vector3(1.1f, .28f, .12f), _warmLight);
            board.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            var sign = post.AddComponent<Interactable>();
            sign.Title = "路标：" + name;
            sign.Prompt = "查看路线";
            sign.CompleteMessage = directions;
        }

        private void BuildHouse(Vector3 position, Vector3 scale, string name)
        {
            Primitive(PrimitiveType.Cube, name, position, scale, _wood);
            var roof = Primitive(PrimitiveType.Cube, name + " Roof", position + Vector3.up * (scale.y * .62f), new Vector3(scale.x + .45f, .35f, scale.z + .45f), _stone);
            roof.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        private void BuildLighthouse(Vector3 position)
        {
            var tower = Primitive(PrimitiveType.Cylinder, "Morning Bay Lighthouse", position + Vector3.up * 4f, new Vector3(2.2f, 4f, 2.2f), _stone);
            _lighthouseTarget = tower.transform;
            _lighthouseBeacon = Primitive(PrimitiveType.Cylinder, "Lighthouse Beacon", position + Vector3.up * 8.2f, new Vector3(1.25f, .45f, 1.25f), _warmLight);
            var flare = Primitive(PrimitiveType.Sphere, "Lighthouse Beacon Flare", position + Vector3.up * 9.1f,
                Vector3.one * 2.5f, _warmLight);
            flare.transform.SetParent(_lighthouseBeacon.transform, true);
            Destroy(flare.GetComponent<Collider>());
            var column = Primitive(PrimitiveType.Cylinder, "Lighthouse Light Column", position + Vector3.up * 15f,
                new Vector3(1.6f, 6f, 1.6f), CreateMaterial(new Color(1f, .08f, 0f), true));
            column.transform.SetParent(_lighthouseBeacon.transform, true);
            Destroy(column.GetComponent<Collider>());

            // A4 light beam: a thin emissive cylinder parented under the beacon. Because the beacon
            // object is already toggled by RefreshWorldForQuest, the beam inherits the dark/restored
            // state and needs no save flag, quest stage or persistence change.
            var beamPivot = new GameObject("Lighthouse Beam Pivot");
            beamPivot.transform.SetParent(_lighthouseBeacon.transform, false);
            beamPivot.transform.localPosition = Vector3.zero;
            // The beacon cylinder is scaled; cancel that scale so the beam keeps its intended world length and radius.
            Vector3 beaconScale = _lighthouseBeacon.transform.lossyScale;
            beamPivot.transform.localScale = new Vector3(1f / beaconScale.x, 1f / beaconScale.y, 1f / beaconScale.z);
            var beamMaterial = CreateMaterial(new Color(2f, .95f, .12f), true);
            if (beamMaterial != null && beamMaterial.HasProperty("_EmissionColor"))
                beamMaterial.SetColor("_EmissionColor", new Color(1f, .48f, .08f) * 4.2f);
            var beam = Primitive(PrimitiveType.Cylinder, "Lighthouse Sweep Beam", position + Vector3.up * 8.2f,
                new Vector3(LighthouseBeamRadius * 2f, LighthouseBeamLength * .5f, LighthouseBeamRadius * 2f),
                beamMaterial);
            beam.transform.SetParent(beamPivot.transform, false);
            beam.transform.localPosition = new Vector3(0f, 0f, LighthouseBeamLength * .5f);
            beam.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(beam.GetComponent<Collider>());
            beamPivot.AddComponent<LighthouseBeam>().Configure(LighthouseBeam.DefaultSweepSeconds);
            _lighthouseLight = new GameObject("Lighthouse Light").AddComponent<Light>();
            _lighthouseLight.type = LightType.Point;
            _lighthouseLight.range = 120f;
            _lighthouseLight.intensity = 7f;
            _lighthouseLight.color = new Color(1f, .55f, .22f);
            _lighthouseLight.transform.position = position + Vector3.up * 8.5f;
        }

        private void BuildHarbourRewards()
        {
            Primitive(PrimitiveType.Cube, "Harbour Navigation Table", new Vector3(-11f, .65f, 7f),
                new Vector3(2f, .25f, 1.4f), _wood);
            foreach (float x in new[] { -11.8f, -10.2f })
            foreach (float z in new[] { 6.5f, 7.5f })
                Primitive(PrimitiveType.Cube, "Harbour Navigation Table Leg", new Vector3(x, .25f, z),
                    new Vector3(.14f, .75f, .14f), _wood);
            _harbourChartDisplay = Primitive(PrimitiveType.Cube, "Harbour Chart Display", new Vector3(-11f, .81f, 7f),
                new Vector3(1.55f, .05f, 1f), CreateMaterial(new Color(.87f, .76f, .53f)));
            var ink = CreateMaterial(new Color(.12f, .3f, .36f));
            for (int index = 0; index < 3; index++)
            {
                var route = Primitive(PrimitiveType.Cube, "Harbour Chart Route", new Vector3(-11.4f + index * .35f, .85f, 7f),
                    new Vector3(.28f, .01f, .04f), ink);
                route.transform.SetParent(_harbourChartDisplay.transform, true);
                Destroy(route.GetComponent<Collider>());
            }

            Primitive(PrimitiveType.Cylinder, "Harbour Signal Mast", new Vector3(8f, 3f, 6f),
                new Vector3(.18f, 3f, .18f), _wood);
            _signalLamp = Primitive(PrimitiveType.Sphere, "Harbour Signal Lamp", new Vector3(8f, 6.3f, 6f),
                Vector3.one * .85f, CreateMaterial(new Color(.35f, .8f, .95f), true));
            _signalLight = new GameObject("Harbour Signal Light").AddComponent<Light>();
            _signalLight.type = LightType.Point;
            _signalLight.range = 20f;
            _signalLight.intensity = 5f;
            _signalLight.color = new Color(.35f, .8f, .95f);
            _signalLight.transform.position = _signalLamp.transform.position;
        }

        // Optional discoveries culminate in a permanent place the player can revisit.
        // It is deliberately separate from the main quest chain: curiosity earns a readable reward, never a gate.
        private void BuildHarbourRouteArchive()
        {
            _harbourRouteArchive = new GameObject("Harbour Route Archive");
            Vector3 position = new Vector3(-11f, .65f, 10.2f);
            var table = Primitive(PrimitiveType.Cube, "Route Archive Table", position,
                new Vector3(2.15f, .25f, 1.15f), _wood);
            table.transform.SetParent(_harbourRouteArchive.transform, true);
            foreach (float x in new[] { -11.85f, -10.15f })
            foreach (float z in new[] { 9.78f, 10.62f })
            {
                var leg = Primitive(PrimitiveType.Cube, "Route Archive Table Leg", new Vector3(x, .25f, z),
                    new Vector3(.14f, .75f, .14f), _wood);
                leg.transform.SetParent(_harbourRouteArchive.transform, true);
            }
            var chart = Primitive(PrimitiveType.Cube, "Recovered Route Chart", position + Vector3.up * .16f,
                new Vector3(1.7f, .04f, .8f), CreateMaterial(new Color(.78f, .84f, .7f)));
            chart.transform.SetParent(_harbourRouteArchive.transform, true);
            var routeInk = CreateMaterial(new Color(.2f, .62f, .78f), true);
            for (int index = 0; index < 4; index++)
            {
                var route = Primitive(PrimitiveType.Cube, "Recovered Route Mark " + (index + 1),
                    position + new Vector3(-.52f + index * .34f, .2f, -.08f + (index % 2) * .18f),
                    new Vector3(.26f, .012f, .035f), routeInk);
                route.transform.rotation = Quaternion.Euler(0f, index % 2 == 0 ? 22f : -18f, 0f);
                route.transform.SetParent(_harbourRouteArchive.transform, true);
                Destroy(route.GetComponent<Collider>());
            }
            var archiveLight = new GameObject("Route Archive Lantern").AddComponent<Light>();
            archiveLight.type = LightType.Point;
            archiveLight.range = 5.5f;
            archiveLight.intensity = 1.35f;
            archiveLight.color = new Color(.35f, .78f, .92f);
            archiveLight.transform.position = position + new Vector3(0f, 1.25f, 0f);
            archiveLight.transform.SetParent(_harbourRouteArchive.transform, true);
            var interactable = table.AddComponent<Interactable>();
            interactable.Title = "旧航线陈列";
            interactable.Prompt = "阅读旧航线";
            interactable.OnInteracted = _ => DialoguePanel.Show("晨湾港旧航线",
                "日志上的交接日期、旧罗盘的偏角、贝壳的潮鸣与护符的刻痕拼成了一条航线。",
                "它越过潮汐浅滩，消失在浓雾更深处。下一座岛仍等待被发现。");
            _harbourRouteArchive.SetActive(ProgressStore.HasHarbourRouteArchive);
        }

        private void BuildTaskBoard()
        {
            var board = Primitive(PrimitiveType.Cube, "Harbour Notice Board", new Vector3(-5.7f, 1.3f, 8f), new Vector3(1.5f, 2.1f, .18f), _wood);
            board.transform.rotation = Quaternion.Euler(0f, 70f, 0f);
            var notice = Primitive(PrimitiveType.Cube, "Harbour Notice Sheet",
                board.transform.position + board.transform.forward * .11f + Vector3.up * .15f,
                new Vector3(1.18f, 1.45f, .035f), CreateMaterial(new Color(.82f, .75f, .58f)));
            notice.transform.rotation = board.transform.rotation;
            notice.transform.SetParent(board.transform, true);
            Destroy(notice.GetComponent<Collider>());
            _noticeBoard = board.transform;
            var interactable = board.AddComponent<Interactable>();
            interactable.Title = "港口告示牌";
            interactable.Prompt = "接取灯塔委托";
            interactable.OnInteracted = _ => _quest.Accept();

            var manager = Primitive(PrimitiveType.Capsule, "港口管理员艾琳", new Vector3(-7.4f, 1f, 8.8f), new Vector3(.7f, 1f, .7f), CreateMaterial(new Color(.12f, .23f, .29f)));
            _harbourManager = manager.transform;
            AttachNpcDecoration(manager, PrimitiveType.Sphere, "艾琳的灯塔徽记", new Vector3(0f, .42f, -.38f), Vector3.one * .3f, _warmLight);
            AttachNpcDecoration(manager, PrimitiveType.Cylinder, "艾琳的帽檐", new Vector3(0f, 1.03f, 0f), new Vector3(.76f, .055f, .64f), _stone);
            AttachNpcDecoration(manager, PrimitiveType.Cylinder, "艾琳的高帽", new Vector3(0f, 1.22f, 0f), new Vector3(.42f, .19f, .42f), _wood);
            AttachNpcDecoration(manager, PrimitiveType.Cube, "艾琳的航海卷册", new Vector3(-.48f, .05f, -.12f), new Vector3(.17f, .48f, .21f), CreateMaterial(new Color(.72f, .7f, .56f)));
            var managerInteraction = manager.AddComponent<Interactable>();
            managerInteraction.Title = "艾琳，港口管理员";
            manager.AddComponent<NpcNameplate>().Configure("艾琳 · 港口管理员");
            managerInteraction.OnInteracted = _ => TalkToEileen();

            var shipwright = Primitive(PrimitiveType.Capsule, "船匠乔恩", new Vector3(2.4f, 1f, 10.2f), new Vector3(.7f, 1f, .7f), CreateMaterial(new Color(.61f, .67f, .63f)));
            _shipwright = shipwright.transform;
            AttachNpcDecoration(shipwright, PrimitiveType.Cylinder, "乔恩的修船工具", new Vector3(.55f, .05f, 0f), new Vector3(.08f, .72f, .08f), _stone);
            AttachNpcDecoration(shipwright, PrimitiveType.Cube, "乔恩的锤头", new Vector3(.55f, .4f, 0f), new Vector3(.42f, .16f, .18f), _stone);
            AttachNpcDecoration(shipwright, PrimitiveType.Cube, "乔恩的扁帽", new Vector3(0f, 1.02f, -.09f), new Vector3(.7f, .12f, .65f), _wood);
            AttachNpcDecoration(shipwright, PrimitiveType.Sphere, "乔恩的腰灯", new Vector3(-.42f, -.18f, -.24f), Vector3.one * .22f, _warmLight);
            var shipwrightInteraction = shipwright.AddComponent<Interactable>();
            shipwrightInteraction.Title = "乔恩，船匠";
            shipwright.AddComponent<NpcNameplate>().Configure("乔恩 · 船匠");
            shipwrightInteraction.OnInteracted = _ => TalkToJon();
            BuildHarbourBeaconPylons();
        }

        // Three harbour pylons, revealed one per finished commission. Output: permanent, walk-around proof
        // that returning to port changed the place, not just a message.
        private void BuildHarbourBeaconPylons()
        {
            Vector3[] spots =
            {
                new Vector3(-12.5f, 1.05f, 4.2f),
                new Vector3(-12.5f, 1.05f, 0.6f),
                new Vector3(-12.5f, 1.05f, -3.0f)
            };
            for (int index = 0; index < spots.Length; index++)
            {
                var pylon = Primitive(PrimitiveType.Cylinder, "Harbour Beacon Pylon " + (index + 1),
                    spots[index], new Vector3(.16f, 1.05f, .16f), _wood);
                var flame = Primitive(PrimitiveType.Sphere, "Harbour Beacon Flame " + (index + 1),
                    spots[index] + Vector3.up * 1.25f, Vector3.one * .36f, _warmLight);
                flame.transform.SetParent(pylon.transform, true);
                Destroy(flame.GetComponent<Collider>());
                var light = flame.AddComponent<Light>();
                light.range = 9f;
                light.intensity = 2.2f;
                light.color = new Color(1f, .58f, .26f);
                pylon.SetActive(false);
                HarbourBeaconPylons[index] = pylon;
            }
        }

        internal void TalkToJon()
        {
            string line = _quest.Stage == PrismQuestStage.Complete && !ProgressStore.HasSurveySail
                ? "棱镜送回来了！我现在给晨鹱号换上勘测帆，航速和转向都会更灵活。"
                : _stormSignalQuest.Stage switch
            {
                StormSignalStage.SearchTideflats => "在海图上比较港口与浅滩方位；把坍塌灯塔旁的风向标转向港口，再取透镜。",
                StormSignalStage.ReturnToShipwright => "你回来了？让我看看那枚透镜。港口的信号桅杆终于能再亮起来了。",
                StormSignalStage.Complete => _discoveries.Count == _discoveries.Total
                    ? "你把四件旧物都找齐了。罗盘与护符上的刻痕相合，这片海还有未讲完的故事。"
                    : _discoveries.IsDiscovered("watchers-charm")
                    ? "信号桅杆工作得很漂亮。晨鹱号已经准备好继续探索。"
                    : "信号桅杆亮了。浅滩东南侧旧路边，据说有人遗失过守望者护符。",
                _ => ProgressStore.HarbourProsperity >= 2
                    ? "码头这几晚热闹多了。船一多，我就得连夜补帆——这才像话。"
                    : _discoveries.IsDiscovered("old-compass")
                    ? "海雾不会永远散去，但一艘好船能让你走得更远。"
                    : "修船屋门前有只褪色罗盘，指针总偏向潮汐浅滩。"
            };
            if (_stormSignalQuest.Stage == StormSignalStage.ReturnToShipwright)
                DialoguePanel.Show("乔恩，船匠", line,
                    "试玩版主线已完成。你可以继续自由探索晨湾港与潮汐浅滩，寻找尚未发现的线索。感谢试玩《雾灯群岛》！");
            else
                DialoguePanel.Show("乔恩，船匠", line);
            if (_quest.Stage != PrismQuestStage.Complete)
            {
                TryUnlockSurveySail();
                return;
            }
            if (!ProgressStore.HasSurveySail)
            {
                TryUnlockSurveySail();
                return;
            }
            bool handingInLens = _stormSignalQuest.Stage == StormSignalStage.ReturnToShipwright;
            _stormSignalQuest.TalkToJon();
            if (handingInLens && _stormSignalQuest.Stage == StormSignalStage.Complete)
                _cueAudio.Play(InteractionCue.QuestHandIn);
        }

        internal void TalkToEileen()
        {
            bool archiveUnlocked = TryUnlockHarbourRouteArchive();
            if (archiveUnlocked)
                DialoguePanel.Show("艾琳，港口管理员", EileenLine(),
                    "你找齐的四件旧物拼出了航线。我把它们整理到导航台旁的旧航线陈列；去看看吧。");
            else
                DialoguePanel.Show("艾琳，港口管理员", EileenLine());
            bool handingInPrism = _quest.Stage == PrismQuestStage.ReturnToHarbour;
            bool handingInChart = _surveyQuest.Stage == SurveyChartStage.ReturnToHarbour;
            if (handingInPrism) _quest.HandIn();
            else _surveyQuest.TalkToEileen();
            if ((handingInPrism && _quest.Stage == PrismQuestStage.Complete) ||
                (handingInChart && _surveyQuest.Stage == SurveyChartStage.Complete))
                _cueAudio.Play(InteractionCue.QuestHandIn);
        }

        private bool TryUnlockHarbourRouteArchive()
        {
            if (_discoveries == null || _discoveries.Count != _discoveries.Total || ProgressStore.HasHarbourRouteArchive) return false;
            ProgressStore.UnlockHarbourRouteArchive();
            if (_harbourRouteArchive != null) _harbourRouteArchive.SetActive(true);
            PrototypeHUD.SetMessage("探索回报：晨湾港的旧航线陈列已整理完成。");
            return true;
        }

        // Harbour prosperity is what the player changed, so it gets its own opening line
        // instead of being buried in the quest-stage switch below.
        private string EileenLine()
        {
            int prosperity = ProgressStore.HarbourProsperity;
            if (prosperity > 0 && _stormSignalQuest.Stage != StormSignalStage.Complete)
            {
                if (prosperity >= 2)
                    return "码头又亮起一盏灯，晚归的渔船认得回港的路了。晨湾港正在醒来。";
                return "你带回来的东西让码头亮了一盏灯。看，远处有船正在朝这里驶来。";
            }
            return _quest.Stage switch
            {
                PrismQuestStage.NotAccepted => "你愿意替港口跑一趟吗？潮汐浅滩的灯塔棱镜失踪了。",
                PrismQuestStage.SearchTideflats => "潮雾涨得很快。石碑上的箭纹会带你找到信标。",
                PrismQuestStage.ReturnToHarbour => "那道光……你真的找到了棱镜。把它交给我，晨湾港就有希望重新点亮。",
                _ => _surveyQuest.Stage switch
                {
                    SurveyChartStage.Locked => _discoveries.IsDiscovered("harbour-log")
                        ? "灯塔亮起来后，旧航图上的航线也许终于能重新辨认。"
                        : "灯塔亮起来后，旧航图也许能辨认。港务办公室门前还有一页潮湿日志。",
                    SurveyChartStage.SearchRuin => "遗迹里的航图被潮水浸透了，但它仍标着一条通往未知海域的线。",
                    SurveyChartStage.ReturnToHarbour => "让我看看那张航图。它会告诉我们迷雾外还有什么。",
                    _ => _discoveries.Count == _discoveries.Total
                        ? "日志、罗盘、贝壳和护符都在你的笔记里了。旧航线的线索终于能留在晨湾港。"
                        : _discoveries.IsDiscovered("tide-shell")
                        ? "有了航图，晨湾港不再只是雾里的一个孤岛。"
                        : "有了航图，晨湾港不再孤立。浅滩西侧海边似乎传来三次潮鸣。"
                }
            };
        }

        // Returning to harbour has to change something the player can see. Each finished
        // commission leaves a permanent, walk-around pylon on the quay; prosperity is derived
        // from completed commissions so it can never drift out of sync with the quest state.
        private void RefreshHarbourProsperity()
        {
            int prosperity = (int)_quest.Stage switch
            {
                >= (int)PrismQuestStage.Complete => 1,
                _ => 0
            };
            if (_surveyQuest.Stage == SurveyChartStage.Complete) prosperity++;
            if (_stormSignalQuest.Stage == StormSignalStage.Complete) prosperity++;
            int previous = ProgressStore.HarbourProsperity;
            if (previous != prosperity) ProgressStore.SetHarbourProsperity(prosperity);
            for (int index = 0; index < HarbourBeaconPylons.Length; index++)
            {
                if (HarbourBeaconPylons[index] != null)
                    HarbourBeaconPylons[index].SetActive(index < prosperity);
            }
            if (prosperity > previous)
                PrototypeHUD.SetMessage($"晨湾港又亮起一盏引航灯。（港口灯火 {prosperity}/3）");
        }

        private void TryUnlockSurveySail()
        {
            if (_quest.Stage != PrismQuestStage.Complete)
            {
                PrototypeHUD.SetMessage("乔恩：先带回潮汐浅滩的灯塔棱镜，我会给晨鹱号换上一套勘测帆。 ");
                return;
            }
            if (ProgressStore.HasSurveySail)
            {
                PrototypeHUD.SetMessage("乔恩：勘测帆已经装好。它能提高航速和转向响应。 ");
                return;
            }
            ProgressStore.UnlockSurveySail();
            _boat.ApplySurveySail();
            PrototypeHUD.SetMessage("船只升级完成：勘测帆已安装（最高航速 +2.5，转向速度 +8）。");
        }

        private void RefreshWorldForQuest(PrismQuestStage stage)
        {
            if (_prism != null) _prism.SetActive(stage == PrismQuestStage.SearchTideflats && _tidePuzzle.IsComplete);
            if (_surveyChart != null) _surveyChart.SetActive(_surveyQuest.Stage == SurveyChartStage.SearchRuin);
            if (_surveyClueTablet != null && _surveyClueTablet.TryGetComponent<Interactable>(out var surveyClueInteraction))
                surveyClueInteraction.enabled = _surveyQuest.Stage == SurveyChartStage.SearchRuin;
            if (_stormLens != null) _stormLens.SetActive(_stormSignalQuest.Stage == StormSignalStage.SearchTideflats);
            if (_stormVane != null && _stormVane.TryGetComponent<Interactable>(out var stormVaneInteraction))
                stormVaneInteraction.enabled = _stormSignalQuest.Stage == StormSignalStage.SearchTideflats;
            bool lighthouseRestored = stage == PrismQuestStage.Complete;
            if (_lighthouseBeacon != null) _lighthouseBeacon.SetActive(lighthouseRestored);
            if (_lighthouseLight != null) _lighthouseLight.enabled = lighthouseRestored;
            if (_harbourChartDisplay != null) _harbourChartDisplay.SetActive(_surveyQuest.Stage == SurveyChartStage.Complete);
            bool signalRestored = _stormSignalQuest.Stage == StormSignalStage.Complete;
            if (_signalLamp != null) _signalLamp.SetActive(signalRestored);
            if (_signalLight != null) _signalLight.enabled = signalRestored;
            RefreshHarbourProsperity();
            if (_guidance == null) return;
            if (stage == PrismQuestStage.Complete && _stormSignalQuest.Stage is not (StormSignalStage.Locked or StormSignalStage.Complete))
            {
                if (_stormSignalQuest.Stage == StormSignalStage.SearchTideflats)
                    _guidance.SetTarget(_stormSignalQuest.VaneAligned || _stormVane == null ? _stormLens.transform : _stormVane,
                        _stormSignalQuest.VaneAligned ? "风暴透镜" : "校准风向标");
                else
                    _guidance.SetTarget(_shipwright, "返回船匠乔恩处");
                return;
            }
            if (stage == PrismQuestStage.Complete && _surveyQuest.Stage != SurveyChartStage.Locked && _surveyQuest.Stage != SurveyChartStage.Complete)
            {
                if (_surveyQuest.Stage == SurveyChartStage.SearchRuin)
                {
                    // Until the inscription is read the guidance points at it, not the chart,
                    // so the compass never sends the player to an object they cannot take.
                    if (_surveyQuest.ClueRead || _surveyClueTablet == null)
                        _guidance.SetTarget(_surveyChart.transform, "失落航图");
                    else
                        _guidance.SetTarget(_surveyClueTablet, "遗迹残碑");
                }
                else
                    _guidance.SetTarget(_harbourManager, "返回晨湾港");
                return;
            }
            switch (stage)
            {
                case PrismQuestStage.NotAccepted:
                    _guidance.SetTarget(_noticeBoard, "查看告示牌");
                    break;
                case PrismQuestStage.SearchTideflats:
                    if (_tidePuzzle.IsComplete)
                        _guidance.SetTarget(_prism.transform, "灯塔棱镜");
                    else
                        SetTidePuzzleGuidance();
                    break;
                case PrismQuestStage.ReturnToHarbour:
                    _guidance.SetTarget(_harbourManager, "返回晨湾港");
                    break;
                case PrismQuestStage.Complete:
                    _guidance.SetTarget(_lighthouseTarget, "已修复的港口灯塔");
                    break;
            }
        }

        private void BuildLantern(Vector3 position)
        {
            Primitive(PrimitiveType.Cylinder, "Dock Lantern", position, new Vector3(.12f, 1f, .12f), _wood);
            var flame = Primitive(PrimitiveType.Sphere, "Lantern Glow", position + Vector3.up * 1.2f, Vector3.one * .32f, _warmLight);
            var light = flame.AddComponent<Light>();
            light.range = 6f;
            light.intensity = 2.1f;
            light.color = new Color(1f, .48f, .18f);
            flame.AddComponent<LanternFlicker>();
        }

        private void BuildPlayer()
        {
            var player = new GameObject("Explorer");
            player.transform.position = new Vector3(0f, 1f, 10f);
            player.transform.rotation = Quaternion.Euler(0f, -110f, 0f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = .35f;
            controller.stepOffset = .5f;
            controller.center = new Vector3(0f, .9f, 0f);
            _player = player.AddComponent<PlayerController>();

            var body = Primitive(PrimitiveType.Capsule, "Explorer Body", player.transform.position + Vector3.up * .9f, new Vector3(.65f, .9f, .65f), _warmLight);
            body.transform.SetParent(player.transform);
            body.transform.localPosition = new Vector3(0f, .9f, 0f);
            Destroy(body.GetComponent<Collider>());

            var cameraRoot = new GameObject("Camera Pivot");
            cameraRoot.transform.SetParent(player.transform);
            cameraRoot.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            cameraRoot.transform.localRotation = Quaternion.identity;
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            camera.backgroundColor = new Color(.24f, .34f, .39f);
            camera.transform.SetParent(cameraRoot.transform);
            camera.transform.localPosition = new Vector3(0f, 2.5f, -5.5f);
            camera.transform.localRotation = Quaternion.Euler(16f, 0f, 0f);
            _player.AssignCamera(camera.transform, cameraRoot.transform);
        }

        // The stone tablet is the only place the beacon order lives, so before it is read we
        // point at the tablet instead of leaking the next beacon through the compass.
        private void SetTidePuzzleGuidance()
        {
            _tideClueRead = ProgressStore.HasReadTideClue;
            if (!_tideClueRead && _tideClueTablet != null)
                _guidance.SetTarget(_tideClueTablet, "查看潮汐箭纹石碑");
            else if (_tideBeaconSearchArea != null)
                _guidance.SetTarget(_tideBeaconSearchArea, "按石碑箭纹探索三座潮汐信标");
            else
                _guidance.SetTarget(_tideflatsDock.transform, "潮汐浅滩码头");
        }

        private void RefreshJourneyGuidance()
        {
            if (_guidance == null || _player == null || _boat == null || _morningBayDock == null || _tideflatsDock == null) return;

            // Guide the player through the physical journey, not just to its distant quest item.
            // z = -45 separates Morning Bay from the Tideflats without adding invisible triggers.
            bool atTideflats = _player.transform.position.z < -45f;
            if (_quest.Stage == PrismQuestStage.SearchTideflats)
            {
                if (_player.IsOnBoat)
                    _guidance.SetTarget(_tideflatsDock.transform, "驶向潮汐浅滩码头");
                else if (!atTideflats)
                    _guidance.SetTarget(_boat.transform, "登上晨鹱号");
                else if (_tidePuzzle.IsComplete)
                    _guidance.SetTarget(_prism.transform, "灯塔棱镜");
                else
                    SetTidePuzzleGuidance();
            }
            else if (_quest.Stage == PrismQuestStage.ReturnToHarbour)
            {
                if (_player.IsOnBoat)
                    _guidance.SetTarget(_morningBayDock.transform, "驶向晨湾港码头");
                else if (atTideflats)
                    _guidance.SetTarget(_boat.transform, "登上晨鹱号返航");
                else
                    _guidance.SetTarget(_harbourManager, "交付棱镜给艾琳");
            }
            else if (_quest.Stage == PrismQuestStage.Complete)
            {
                if (!ProgressStore.HasSurveySail)
                    _guidance.SetTarget(_shipwright, "向乔恩领取勘测帆");
                else if (_surveyQuest.Stage == SurveyChartStage.Locked)
                    _guidance.SetTarget(_harbourManager, "向艾琳询问失落航图");
                else if (_surveyQuest.Stage == SurveyChartStage.SearchRuin)
                {
                    Transform islandTarget = _surveyQuest.ClueRead || _surveyClueTablet == null
                        ? _surveyChart.transform : _surveyClueTablet;
                    _guidance.SetTarget(_player.IsOnBoat ? _tideflatsDock.transform : atTideflats ? islandTarget : _boat.transform,
                        _player.IsOnBoat ? "驶向潮汐浅滩码头" : atTideflats ?
                            _surveyQuest.ClueRead ? "寻找失落航图" : "辨认遗迹残碑" : "登上晨鹱号");
                }
                else if (_surveyQuest.Stage == SurveyChartStage.ReturnToHarbour)
                    _guidance.SetTarget(_player.IsOnBoat ? _morningBayDock.transform : atTideflats ? _boat.transform : _harbourManager,
                        _player.IsOnBoat ? "驶向晨湾港码头" : atTideflats ? "登上晨鹱号返航" : "交付航图给艾琳");
                else if (_stormSignalQuest.Stage == StormSignalStage.Locked)
                    _guidance.SetTarget(_shipwright, "向乔恩询问风暴透镜");
                else if (_stormSignalQuest.Stage == StormSignalStage.SearchTideflats)
                {
                    Transform islandTarget = _stormSignalQuest.VaneAligned || _stormVane == null
                        ? _stormLens.transform : _stormVane;
                    string islandLabel = _stormSignalQuest.VaneAligned ? "寻找风暴透镜" : "校准风向标";
                    _guidance.SetTarget(_player.IsOnBoat ? _tideflatsDock.transform : atTideflats ? islandTarget : _boat.transform,
                        _player.IsOnBoat ? "驶向潮汐浅滩码头" : atTideflats ? islandLabel : "登上晨鹱号");
                }
                else if (_stormSignalQuest.Stage == StormSignalStage.ReturnToShipwright)
                    _guidance.SetTarget(_player.IsOnBoat ? _morningBayDock.transform : atTideflats ? _boat.transform : _shipwright,
                        _player.IsOnBoat ? "驶向晨湾港码头" : atTideflats ? "登上晨鹱号返航" : "交付透镜给乔恩");
                else
                    _guidance.SetTarget(null, string.Empty);
            }
        }

        private static void AttachNpcDecoration(GameObject npc, PrimitiveType type, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var part = Primitive(type, name, npc.transform.position + localPosition, scale, material);
            part.transform.SetParent(npc.transform, true);
            var collider = part.GetComponent<Collider>();
            collider.enabled = false;
            UnityEngine.Object.Destroy(collider);
        }

        private static GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().material = material;
            return item;
        }
    }
}
