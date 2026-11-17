#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FoglightIsles.Editor
{
    public static class BuildWindowsDemo
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string DevelopmentOutputPath = "Builds/Windows/FoglightIslesDemo.exe";
        private const string ReleaseOutputPath = "Builds/ReleaseCandidate/FoglightIslesDemo.exe";
        private const string RuntimeMaterialPath = "Assets/Resources/FoglightRuntimeMaterial.mat";

        [MenuItem("Foglight Isles/Build Windows Demo (Development)")]
        public static void BuildDevelopment()
        {
            Build(DevelopmentOutputPath, true);
        }

        [MenuItem("Foglight Isles/Build Windows Demo (Release Candidate)")]
        public static void BuildReleaseCandidate()
        {
            Build(ReleaseOutputPath, false);
        }

        // Kept as a command-line entry point for automated development-build validation.
        public static void Build()
        {
            Build(DevelopmentOutputPath, true);
        }

        private static void Build(string outputPath, bool development)
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogError($"[FoglightIsles Build] Required scene is missing: {ScenePath}");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            EnsureRuntimeMaterialAsset();
            PlayerSettings.productName = "Foglight Isles Demo";
            PlayerSettings.companyName = "Foglight Isles";
            PlayerSettings.bundleVersion = "0.2.6";
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = development ? BuildOptions.Development : BuildOptions.None
            });

            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[FoglightIsles Build] PASS — {(development ? "development" : "release candidate")} Windows demo created at {outputPath} ({report.summary.totalSize / 1024f / 1024f:0.0} MB).");
            else
                Debug.LogError($"[FoglightIsles Build] FAIL — {report.summary.result}. Check the Console for details.");
        }

        private static void EnsureRuntimeMaterialAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath) != null)
            {
                return;
            }

            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogError("[FoglightIsles Build] Required shader was not found: Unlit/Color");
                return;
            }

            Directory.CreateDirectory("Assets/Resources");
            var material = new Material(shader) { name = "FoglightRuntimeMaterial" };
            AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[FoglightIsles Build] Created runtime material resource.");
        }
    }
}
#endif
