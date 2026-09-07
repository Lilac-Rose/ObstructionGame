using UnityEditor;
using UnityEngine;

namespace MirrorDash.EditorTools
{
    // sets recommended player settings for a WebGL build.
    // run this after switching platform to WebGL in Build Profiles.
    public static class WebGLSetup
    {
        [MenuItem("Mirror Dash/Apply Recommended WebGL Settings")]
        public static void ApplyWebGLSettings()
        {
            // gzip + decompression fallback so it works on basically any static host
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            // fixed memory size was crashing after a while, let it grow instead
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.initialMemorySize = 32;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            PlayerSettings.WebGL.geometricMemoryGrowthStep = 0.2f;
            PlayerSettings.WebGL.memoryGeometricGrowthCap = 96;

            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;

            // don't need threads and it requires special server headers anyway
            PlayerSettings.WebGL.threadsSupport = false;

            Debug.Log("WebGL settings applied.");
        }
    }
}
