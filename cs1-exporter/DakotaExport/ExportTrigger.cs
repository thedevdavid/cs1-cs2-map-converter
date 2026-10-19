using System;
using ICities;

namespace DakotaExport
{
    /// <summary>
    /// Arms the export when a map or game finishes loading.
    /// </summary>
    public sealed class ExportLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            bool supported = mode == LoadMode.LoadMap || mode == LoadMode.NewMap ||
                             mode == LoadMode.LoadGame || mode == LoadMode.NewGame;
            if (!supported)
            {
                ExportLog.Unity("level loaded in mode " + mode + "; export not armed.");
                return;
            }

            ExportTrigger.Arm(mode.ToString());
        }

        public override void OnLevelUnloading()
        {
            ExportTrigger.Disarm();
            base.OnLevelUnloading();
        }
    }

    /// <summary>
    /// Runs the export on the main thread a few frames after load, once per level.
    /// </summary>
    public sealed class ExportTrigger : ThreadingExtensionBase
    {
        private const int DelayFrames = 10;

        private static bool s_armed;
        private static int s_framesLeft;
        private static string s_loadMode;

        internal static void Arm(string loadMode)
        {
            s_loadMode = loadMode;
            s_framesLeft = DelayFrames;
            s_armed = true;
            ExportLog.Unity("armed for load mode " + loadMode + "; exporting in " + DelayFrames + " frames.");
        }

        internal static void Disarm()
        {
            s_armed = false;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (!s_armed) return;
            if (--s_framesLeft > 0) return;
            s_armed = false;

            try
            {
                MapExporter.Run(s_loadMode);
            }
            catch (Exception ex)
            {
                ExportLog.Unity("export FAILED: " + ex);
            }
        }
    }
}
