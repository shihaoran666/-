using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Records wall-clock frame pacing at both islands and during sailing.</summary>
    public sealed class PrototypePerformanceValidator : MonoBehaviour
    {
        private const string VerifyArgument = "-verify-performance";
        private const int WarmupFrames = 90;
        private const int SampleFrames = 360;
        private BoatController _sailingBoat;
        private Transform _sailingDestination;

        private void Update()
        {
            if (_sailingBoat == null || _sailingDestination == null) return;
            Vector3 direction = _sailingDestination.position - _sailingBoat.transform.position;
            direction.y = 0f;
            float steering = Mathf.Clamp(Vector3.SignedAngle(_sailingBoat.transform.forward, direction, Vector3.up) / 25f, -1f, 1f);
            _sailingBoat.SimulateControl(1f, steering, Time.deltaTime);
        }

        private void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), VerifyArgument) >= 0)
                StartCoroutine(Measure());
        }

        private IEnumerator Measure()
        {
            yield return null;
            FindFirstObjectByType<StartScreen>()?.BeginExploration();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            BoatController boat = FindFirstObjectByType<BoatController>();
            DockingPoint destination = null;
            foreach (DockingPoint dock in FindObjectsByType<DockingPoint>(FindObjectsSortMode.None))
                if (dock.DockName == "潮汐浅滩码头") destination = dock;
            if (player == null || boat == null || destination == null || Camera.main == null || Screen.width < 1 || Screen.height < 1)
            {
                Debug.LogError("[FoglightIsles Performance] FAIL — player, camera or render surface is missing.");
                Application.Quit(1);
                yield break;
            }

            Debug.Log($"[FoglightIsles Performance] Device: {SystemInfo.graphicsDeviceName}; resolution: {Screen.width}x{Screen.height}; quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}; target cap: {Application.targetFrameRate} FPS; vSync: {QualitySettings.vSyncCount}.");
            yield return MeasureLocation("Morning Bay", player, new Vector3(0f, 1f, 10f), Debug.isDebugBuild);
            yield return MeasureLocation("Tideflats", player, new Vector3(0f, 1f, -76f), Debug.isDebugBuild);
            player.BoardBoat(boat);
            boat.SetCaptain(null); // Keep the camera on the boat while simulated input controls the benchmark route.
            _sailingBoat = boat;
            _sailingDestination = destination.transform;
            yield return MeasureFrames("Sailing", 600, Debug.isDebugBuild);
            _sailingBoat = null;
            _sailingDestination = null;
            Debug.Log("[FoglightIsles Performance] PASS — harbour, island and rendered sailing sampled.");
            Application.Quit(0);
        }

        private static IEnumerator MeasureLocation(string label, PlayerController player, Vector3 position, bool sampleAllocations)
        {
            player.transform.position = position;
            Physics.SyncTransforms();
            yield return MeasureFrames(label, SampleFrames, sampleAllocations);
        }

        private static IEnumerator MeasureFrames(string label, int sampleCount, bool sampleAllocations)
        {
            var endOfFrame = new WaitForEndOfFrame();
            for (int i = 0; i < WarmupFrames; i++) yield return endOfFrame;

            ProfilerRecorder gcRecorder = default;
            if (sampleAllocations)
            {
                gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
                if (!gcRecorder.Valid)
                {
                    Debug.LogWarning($"[FoglightIsles Performance] {label} GC counter unavailable in this build.");
                    sampleAllocations = false;
                }
                else
                {
                    byte[] calibration = new byte[4096];
                    calibration[0] = 1;
                    long current = gcRecorder.CurrentValue;
                    yield return endOfFrame;
                    long completed = gcRecorder.LastValue;
                    GC.KeepAlive(calibration);
                    if (Math.Max(current, completed) < 4096)
                    {
                        Debug.LogWarning($"[FoglightIsles Performance] {label} GC counter failed 4 KB calibration (current {current} B, completed {completed} B).");
                        sampleAllocations = false;
                    }
                }
            }

            var frameSeconds = new List<float>(sampleCount);
            long totalAllocated = 0;
            long worstAllocated = 0;
            int allocatingFrames = 0;
            long previousTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            for (int i = 0; i < sampleCount; i++)
            {
                yield return endOfFrame;
                long nowTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                if (sampleAllocations)
                {
                    long frameAllocated = gcRecorder.LastValue;
                    totalAllocated += frameAllocated;
                    worstAllocated = Math.Max(worstAllocated, frameAllocated);
                    if (frameAllocated > 0) allocatingFrames++;
                }
                frameSeconds.Add((nowTicks - previousTicks) / (float)System.Diagnostics.Stopwatch.Frequency);
                previousTicks = nowTicks;
            }
            float total = 0f;
            foreach (float seconds in frameSeconds) total += seconds;
            frameSeconds.Sort();
            int slowFrameCount = Mathf.Max(1, Mathf.CeilToInt(frameSeconds.Count * .01f));
            float slowSeconds = 0f;
            for (int i = 0; i < slowFrameCount; i++) slowSeconds += frameSeconds[frameSeconds.Count - 1 - i];
            float averageFps = frameSeconds.Count / total;
            float onePercentLowFps = slowFrameCount / slowSeconds;
            float worstFps = 1f / frameSeconds[frameSeconds.Count - 1];
            float p95Ms = frameSeconds[Mathf.CeilToInt(frameSeconds.Count * .95f) - 1] * 1000f;
            Debug.Log($"[FoglightIsles Performance] {label}: avg {averageFps:F1} FPS, 1% low {onePercentLowFps:F1} FPS, worst {worstFps:F1} FPS, p95 {p95Ms:F1} ms ({sampleCount} frames).");
            if (sampleAllocations)
                Debug.Log($"[FoglightIsles Performance] {label} GC allocated in frame: avg {totalAllocated / (double)sampleCount:F1} B/frame, worst {worstAllocated} B, allocating frames {allocatingFrames}/{sampleCount} (development build).");
            gcRecorder.Dispose();
        }
    }
}
