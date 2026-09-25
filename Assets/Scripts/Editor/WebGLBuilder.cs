using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HelloTap.Editor
{
    public static class WebGLBuilder
    {
        private const string BuildOutputDir = "Builds/WebGL";

        [MenuItem("HelloTap/Build WebGL (Release)", false, 100)]
        public static void BuildWebGLMenu()
        {
            PerformBuild(false);
        }

        [MenuItem("HelloTap/Build WebGL (Development)", false, 101)]
        public static void BuildWebGLDevMenu()
        {
            PerformBuild(true);
        }

        public static void BuildWebGL()
        {
            // Batchmode entry point
            bool success = PerformBuild(false);
            if (!success)
            {
                EditorApplication.Exit(1);
            }
        }

        public static bool PerformBuild(bool isDevelopment)
        {
            Debug.Log("[WebGLBuilder] Starting WebGL Build pipeline...");

            // 1. Ensure WebGL target is active
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.Log("[WebGLBuilder] Switching active build target to WebGL...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // 2. Configure Player & WebGL Settings
            PlayerSettings.WebGL.template = "PROJECT:MobilePortrait";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // Uncompressed for universal compatibility without custom headers
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;
            PlayerSettings.runInBackground = true;

            // 3. Collect scenes
            string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

            // 4. Ensure output directory exists and is clean
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullOutputPath = Path.Combine(projectRoot, BuildOutputDir);
            
            if (Directory.Exists(fullOutputPath))
            {
                try
                {
                    Directory.Delete(fullOutputPath, true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[WebGLBuilder] Could not completely delete previous build folder: {e.Message}");
                }
            }
            Directory.CreateDirectory(fullOutputPath);

            // 5. Build Options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullOutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = isDevelopment ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None
            };

            Debug.Log($"[WebGLBuilder] Building to {fullOutputPath} (Dev: {isDevelopment})...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] WebGL Build Succeeded! Total size: {summary.totalSize / 1024 / 1024:F2} MB, Time: {summary.totalTime.TotalSeconds:F1}s");
                return true;
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] WebGL Build Failed! Result: {summary.result}, Errors: {summary.totalErrors}");
                return false;
            }
        }
    }
}
