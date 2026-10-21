using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// File-driven camera + screenshot command, map editor only. Polls %LOCALAPPDATA%\DakotaExport\camera.json;
    /// for each shot sets CameraController target and current position/angle/size (no smoothing), optionally
    /// hides the UI, waits a few frames, captures with Application.CaptureScreenshot, waits for the PNG, then
    /// writes camera.done.json (via .tmp + rename) and deletes camera.json. Only the camera and UI visibility
    /// change; nothing is saved.
    /// </summary>
    internal static class CameraCommand
    {
        public const string Schema = "dakota-export/cs1/camera/v1";
        private const float PollInterval = 0.25f;
        private const float FileTimeout = 20f;

        private enum Phase { Idle, Settle, WaitFile, Next }

        private sealed class Shot
        {
            public int Index;
            public double X, Z, Yaw, Pitch, Distance;
            public double? Y;
            public string Out;
            public bool HideUi;
            public int Frames = 3;
            public int SuperSize = 1;
            public string Error;
            public long Bytes;
            public bool Ok;
            public string Actual;   // JSON object text
        }

        private static bool s_enabled;
        private static Phase s_phase = Phase.Idle;
        private static float s_pollTimer;
        private static List<Shot> s_shots;
        private static int s_current;
        private static int s_framesLeft;
        private static float s_waitTime;
        private static long s_lastSize = -1;
        private static bool s_uiWasVisible = true;
        private static bool s_uiHidden;
        private static string s_requestText;
        private static string s_requestError;
        private static DateTime s_startedUtc;
        private static CameraController s_cc;

        public static string Dir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DakotaExport"); }
        }

        public static string RequestPath { get { return Path.Combine(Dir, "camera.json"); } }
        public static string DonePath { get { return Path.Combine(Dir, "camera.done.json"); } }

        public static void Enable()
        {
            s_enabled = true;
            s_phase = Phase.Idle;
            s_cc = null;
            ExportLog.Unity("camera command enabled: watching " + RequestPath);
        }

        public static void Disable()
        {
            RestoreUi();
            s_enabled = false;
            s_phase = Phase.Idle;
            s_shots = null;
            s_cc = null;
        }

        /// <summary>Called every frame on the main thread.</summary>
        public static void Update(float realTimeDelta)
        {
            if (!s_enabled) return;
            try
            {
                Step(realTimeDelta);
            }
            catch (Exception ex)
            {
                ExportLog.Unity("camera command FAILED: " + ex);
                RestoreUi();
                if (s_shots != null && s_current < s_shots.Count) s_shots[s_current].Error = ex.GetType().Name + ": " + ex.Message;
                Finish();
            }
        }

        private static void Step(float dt)
        {
            switch (s_phase)
            {
                case Phase.Idle:
                    s_pollTimer -= dt;
                    if (s_pollTimer > 0f) return;
                    s_pollTimer = PollInterval;
                    if (!File.Exists(RequestPath)) return;
                    if (!Load()) return; // file still being written; retry next poll
                    s_current = 0;
                    if (s_shots.Count == 0) { Finish(); return; }
                    Begin();
                    return;

                case Phase.Settle:
                    if (--s_framesLeft > 0) return;
                    Capture();
                    return;

                case Phase.WaitFile:
                    s_waitTime += dt;
                    Shot shot = s_shots[s_current];
                    long size = File.Exists(shot.Out) ? new FileInfo(shot.Out).Length : -1;
                    if (size > 0 && size == s_lastSize)
                    {
                        shot.Ok = true;
                        shot.Bytes = size;
                        s_phase = Phase.Next;
                        return;
                    }

                    s_lastSize = size;
                    if (s_waitTime > FileTimeout)
                    {
                        shot.Error = "screenshot file did not appear within " + FileTimeout + " s";
                        s_phase = Phase.Next;
                    }

                    return;

                case Phase.Next:
                    RestoreUi();
                    ExportLog.Unity("camera shot " + s_current + (s_shots[s_current].Ok ? " saved " + s_shots[s_current].Out : " failed: " + s_shots[s_current].Error));
                    s_current++;
                    if (s_current < s_shots.Count) Begin();
                    else Finish();
                    return;
            }
        }

        private static bool Load()
        {
            s_startedUtc = DateTime.UtcNow;
            s_shots = new List<Shot>();
            s_requestError = null;
            s_requestText = null;
            try
            {
                s_requestText = File.ReadAllText(RequestPath, Encoding.UTF8);
            }
            catch (IOException)
            {
                // Still being written by the client; try again next poll.
                s_shots = null;
                return false;
            }

            try
            {
                var root = MiniJson.Parse(s_requestText) as Dictionary<string, object>;
                if (root == null) throw new FormatException("top level must be an object");
                var list = Get(root, "shots") as List<object>;
                if (list == null)
                {
                    s_shots.Add(ParseShot(root, root, 0));
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var o = list[i] as Dictionary<string, object>;
                        if (o == null) { s_shots.Add(new Shot { Index = i, Error = "shot is not an object" }); continue; }
                        s_shots.Add(ParseShot(o, root, i));
                    }
                }
            }
            catch (Exception ex)
            {
                s_requestError = ex.GetType().Name + ": " + ex.Message;
                s_shots.Clear();
            }

            ExportLog.Unity("camera command: " + s_shots.Count + " shot(s)" + (s_requestError != null ? ", request error " + s_requestError : ""));
            return true;
        }

        private static Shot ParseShot(Dictionary<string, object> o, Dictionary<string, object> defaults, int index)
        {
            var s = new Shot { Index = index };
            try
            {
                s.X = Num(o, defaults, "x", null).Value;
                s.Z = Num(o, defaults, "z", null).Value;
                s.Y = Num(o, defaults, "y", double.NaN);
                if (s.Y.HasValue && double.IsNaN(s.Y.Value)) s.Y = null;
                s.Yaw = Num(o, defaults, "yaw", 0).Value;
                s.Pitch = Num(o, defaults, "pitch", 45).Value;
                s.Distance = Num(o, defaults, "distance", 500).Value;
                s.Frames = Math.Max(1, (int)Num(o, defaults, "frames", 3).Value);
                s.SuperSize = Math.Max(1, Math.Min(4, (int)Num(o, defaults, "superSize", 1).Value));
                object hide = Get(o, "hideUi") ?? Get(defaults, "hideUi");
                s.HideUi = hide is bool && (bool)hide;
                string outPath = Get(o, "out") as string;
                if (string.IsNullOrEmpty(outPath)) throw new FormatException("missing \"out\"");
                if (!Path.IsPathRooted(outPath)) outPath = Path.Combine(Path.Combine(Dir, "shots"), outPath);
                if (!outPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) outPath += ".png";
                s.Out = Path.GetFullPath(outPath);
            }
            catch (Exception ex)
            {
                s.Error = ex.Message;
            }

            return s;
        }

        private static object Get(Dictionary<string, object> d, string k)
        {
            object v;
            return d != null && d.TryGetValue(k, out v) ? v : null;
        }

        private static double? Num(Dictionary<string, object> o, Dictionary<string, object> defaults, string key, double? fallback)
        {
            object v = Get(o, key);
            if (v == null && !ReferenceEquals(o, defaults)) v = Get(defaults, key);
            if (v is double) return (double)v;
            if (v == null)
            {
                if (fallback.HasValue) return fallback;
                throw new FormatException("missing \"" + key + "\"");
            }

            throw new FormatException("\"" + key + "\" must be a number");
        }

        private static void Begin()
        {
            Shot shot = s_shots[s_current];
            if (shot.Error != null)
            {
                s_phase = Phase.Next;
                return;
            }

            CameraController cc = Controller();
            if (cc == null)
            {
                shot.Error = "CameraController not found";
                s_phase = Phase.Next;
                return;
            }

            float x = (float)shot.X, z = (float)shot.Z;
            float y = shot.Y.HasValue
                ? (float)shot.Y.Value
                : Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(new Vector3(x, 0f, z), false, 0f);
            var target = new Vector3(x, y, z);

            // CameraController.UpdateTransform: distance = size * max(0, 1 - height / maxDistance) / tan(fov).
            float fov = cc.m_camera != null ? cc.m_camera.fieldOfView : 45f;
            float factor = Mathf.Max(0.01f, 1f - cc.m_currentHeight / cc.m_maxDistance);
            float size = (float)shot.Distance * Mathf.Tan(Mathf.Deg2Rad * fov) / factor;
            var angle = new Vector2((float)shot.Yaw, (float)shot.Pitch);

            cc.m_targetPosition = target;
            cc.m_currentPosition = target;
            cc.m_targetAngle = angle;
            cc.m_currentAngle = angle;
            cc.m_targetSize = size;
            cc.m_currentSize = size;

            if (shot.HideUi && !s_uiHidden)
            {
                s_uiWasVisible = UIView.isVisible;
                UIView.Show(false);
                s_uiHidden = true;
            }

            s_framesLeft = shot.Frames;
            s_phase = Phase.Settle;
        }

        private static void Capture()
        {
            Shot shot = s_shots[s_current];
            CameraController cc = Controller();
            if (cc != null) shot.Actual = DescribeCamera(cc);
            string dir = Path.GetDirectoryName(shot.Out);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(shot.Out)) File.Delete(shot.Out);
            Application.CaptureScreenshot(shot.Out, shot.SuperSize);
            s_waitTime = 0f;
            s_lastSize = -1;
            s_phase = Phase.WaitFile;
        }

        private static string DescribeCamera(CameraController cc)
        {
            var sw = new StringWriter();
            var w = new JsonWriter(sw);
            Transform t = cc.transform;
            float fov = cc.m_camera != null ? cc.m_camera.fieldOfView : float.NaN;
            w.BeginObject();
            w.Prop("targetPosition", cc.m_currentPosition);
            w.Prop("yaw", cc.m_currentAngle.x);
            w.Prop("pitch", cc.m_currentAngle.y);
            w.Prop("size", cc.m_currentSize);
            w.Prop("height", cc.m_currentHeight);
            w.Prop("distance", Vector3.Distance(t.position, cc.m_currentPosition));
            w.Prop("cameraPosition", t.position);
            w.Prop("cameraEuler", t.rotation.eulerAngles);
            w.Prop("fieldOfView", fov);
            w.Prop("minDistance", cc.m_minDistance);
            w.Prop("maxDistance", cc.m_maxDistance);
            w.Name("screen").BeginObject().Prop("width", Screen.width).Prop("height", Screen.height).EndObject();
            w.EndObject();
            return sw.ToString();
        }

        private static CameraController Controller()
        {
            if (s_cc == null) s_cc = UnityEngine.Object.FindObjectOfType<CameraController>();
            return s_cc;
        }

        private static void RestoreUi()
        {
            if (!s_uiHidden) return;
            try { UIView.Show(s_uiWasVisible); }
            catch (Exception ex) { ExportLog.Unity("camera command: UI restore failed: " + ex.Message); }
            s_uiHidden = false;
        }

        private static void Finish()
        {
            RestoreUi();
            try
            {
                var sb = new StringWriter();
                var w = new JsonWriter(sb);
                w.BeginObject();
                w.Prop("schema", Schema);
                w.Prop("exporterVersion", DakotaExportMod.Version);
                w.Prop("startedUtc", s_startedUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                w.Prop("completedUtc", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                w.Prop("requestError", s_requestError);
                w.Prop("request", s_requestText);
                int ok = 0;
                w.Name("shots").BeginArray();
                if (s_shots != null)
                {
                    foreach (Shot s in s_shots)
                    {
                        if (s.Ok) ok++;
                        w.NewLine();
                        w.BeginObject();
                        w.Prop("index", s.Index);
                        w.Prop("ok", s.Ok);
                        w.Prop("error", s.Error);
                        w.Prop("out", s.Out);
                        w.Prop("bytes", s.Bytes);
                        w.Name("requested").BeginObject()
                         .Prop("x", (float)s.X).Prop("z", (float)s.Z)
                         .Prop("yaw", (float)s.Yaw).Prop("pitch", (float)s.Pitch).Prop("distance", (float)s.Distance)
                         .Prop("hideUi", s.HideUi).Prop("frames", s.Frames).Prop("superSize", s.SuperSize)
                         .EndObject();
                        w.Name("actual");
                        if (s.Actual != null) w.BeginObject().RawMembers(s.Actual.Substring(1, s.Actual.Length - 2)).EndObject();
                        else w.Null();
                        w.EndObject();
                    }
                }

                w.EndArray();
                w.Prop("okCount", ok);
                w.EndObject();
                string text = sb.ToString() + "\n";
                SafeFile.Write(DonePath, tmp => File.WriteAllText(tmp, text, new UTF8Encoding(false)));
                if (File.Exists(RequestPath)) File.Delete(RequestPath);
                ExportLog.Unity("camera command done: " + ok + "/" + (s_shots != null ? s_shots.Count : 0) + " shots, " + DonePath);
            }
            catch (Exception ex)
            {
                ExportLog.Unity("camera command: could not write done file: " + ex);
                // Avoid re-running the same request forever.
                try { if (File.Exists(RequestPath)) File.Move(RequestPath, RequestPath + ".failed"); }
                catch { }
            }

            s_shots = null;
            s_phase = Phase.Idle;
            s_pollTimer = PollInterval;
        }
    }
}
