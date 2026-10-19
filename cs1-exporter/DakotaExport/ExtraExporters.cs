using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ColossalFramework;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>v5 sections: terrain height grids, Network Skins, raw serializable data.</summary>
    internal static class ExtraExporters
    {
        public static readonly string[] HeightNames = { "raw", "raw2", "block" };
        private const int HeightSize = 1081;

        // ---------------------------------------------------------------- terrain heights

        /// <summary>
        /// TerrainManager.RawHeights (saved heightmap), RawHeights2 (with object terraforming) and
        /// BlockHeights (what the water simulation runs on) as 1081^2 16-bit PNGs, row 0 = north, raw units.
        /// </summary>
        public static void WriteTerrainHeights(JsonWriter w, string dir, string baseName, ExportLog log, Dictionary<string, int> stats)
        {
            TerrainManager tm = Singleton<TerrainManager>.instance;
            ushort[][] grids = { tm.RawHeights, tm.RawHeights2, tm.BlockHeights };
            string[] sources = { "TerrainManager.RawHeights (m_rawHeights, the saved heightmap)", "TerrainManager.RawHeights2 (m_rawHeights2, with building/network terraforming)", "TerrainManager.BlockHeights (m_blockHeights, what the water simulation runs on)" };
            int n = HeightSize * HeightSize;
            ushort[] raw = grids[0];

            w.Name("terrainHeights").BeginObject();
            w.Prop("pngEncoding", "1081x1081 16-bit grey, row 0 = north, value = raw height (metres * 64), same frame as <base>-water.png and the CS1 heightmap export");
            w.Name("grids").BeginObject();
            for (int g = 0; g < grids.Length; g++)
            {
                string name = HeightNames[g];
                ushort[] h = grids[g];
                w.Name(name).BeginObject();
                w.Prop("source", sources[g]);
                if (h == null || h.Length < n)
                {
                    w.Name("file").Null();
                    w.Prop("error", "array missing or length " + (h == null ? -1 : h.Length));
                    w.EndObject();
                    continue;
                }

                // Copy first so the PNG is one consistent snapshot.
                var copy = new ushort[n];
                Array.Copy(h, copy, n);
                int min = 65535, max = 0, diff = 0;
                var png = new ushort[n];
                for (int row = 0; row < HeightSize; row++)
                {
                    int dst = (HeightSize - 1 - row) * HeightSize;
                    for (int col = 0; col < HeightSize; col++)
                    {
                        ushort v = copy[row * HeightSize + col];
                        png[dst + col] = v;
                        if (v < min) min = v;
                        if (v > max) max = v;
                        if (g > 0 && raw != null && raw.Length >= n && v != raw[row * HeightSize + col]) diff++;
                    }
                }

                string path = Path.Combine(dir, baseName + "-heights-" + name + ".png");
                string method = PngWriter.WriteGray16(path, HeightSize, HeightSize, png);
                w.Prop("file", Path.GetFileName(path));
                w.Prop("compression", method);
                w.Prop("minRaw", min);
                w.Prop("maxRaw", max);
                w.Prop("minMetres", min / 64f);
                w.Prop("maxMetres", max / 64f);
                if (g > 0) w.Prop("cellsDifferentFromRaw", diff);
                w.EndObject();
                stats["terrainHeights." + name + ".maxRaw"] = max;
                if (g > 0) stats["terrainHeights." + name + ".cellsDifferentFromRaw"] = diff;
                log.Info("heights " + name + ": " + min / 64f + " .. " + max / 64f + " m" + (g > 0 ? ", " + diff + " cells differ from raw" : "") + " (" + method + ")");
            }

            w.EndObject();
            w.EndObject();
        }

        // ---------------------------------------------------------------- Network Skins

        /// <summary>
        /// Network Skins Continued (Workshop 3702700108): static NetworkSkins.Skins.NetworkSkinManager.SegmentSkins /
        /// NodeSkins (NetworkSkin[] by id). Each NetworkSkin: Prefab (NetInfo), private _modifiers
        /// (List&lt;NetworkSkinModifier&gt;) whose public readonly fields are dumped generically
        /// (StreetLight, Tree, Pillar, Catenary, Color, TerrainSurface, RoadDecoration, CustomData).
        /// Skins are shared between segments, so they are written once in "skins" and referenced by index.
        /// </summary>
        public static void WriteNetworkSkins(JsonWriter w, ExportLog log, Dictionary<string, int> stats)
        {
            Type mgr = ModReflection.FindType("NetworkSkins.Skins.NetworkSkinManager");
            if (mgr == null)
            {
                w.Name("networkSkins").Null();
                w.Prop("networkSkinsReason", "Network Skins not loaded (type NetworkSkins.Skins.NetworkSkinManager not found)");
                log.Info("network skins: not loaded");
                return;
            }

            Array segSkins = ModReflection.StaticField(mgr, "SegmentSkins") as Array;
            Array nodeSkins = ModReflection.StaticField(mgr, "NodeSkins") as Array;
            if (segSkins == null && nodeSkins == null)
            {
                w.Name("networkSkins").Null();
                w.Prop("networkSkinsReason", "Network Skins loaded but SegmentSkins/NodeSkins are null (mod not active)");
                log.Info("network skins: arrays null");
                return;
            }

            var index = new Dictionary<object, int>();
            var skins = new List<object>();
            var segPairs = new List<int>();
            var nodePairs = new List<int>();
            Collect(segSkins, index, skins, segPairs);
            Collect(nodeSkins, index, skins, nodePairs);

            w.Name("networkSkins").BeginObject();
            w.Prop("assembly", mgr.Assembly.GetName().Name + " " + mgr.Assembly.GetName().Version);
            w.Prop("encoding", "segments/nodes: flat [id, skinIndex, ...]; skins[skinIndex] = { prefab, modifiers[] }; a modifier field that is null usually means 'removed' (e.g. StreetLight null = no lights)");
            w.Name("skins").BeginArray();
            for (int k = 0; k < skins.Count; k++)
            {
                object skin = skins[k];
                NetInfo prefab = ModReflection.Member(skin, "Prefab") as NetInfo;
                w.NewLine();
                w.BeginObject();
                w.Prop("index", k);
                w.Prop("prefab", prefab != null ? prefab.name : null);
                w.Name("modifiers").BeginArray();
                IList mods = ModReflection.Member(skin, "_modifiers") as IList;
                if (mods != null)
                {
                    foreach (object m in mods)
                    {
                        if (m == null) continue;
                        w.BeginObject();
                        w.Prop("type", m.GetType().Name);
                        w.Name("fields");
                        GenericDump.WriteFields(w, m, 3);
                        w.EndObject();
                    }
                }

                w.EndArray();
                object pav = ModReflection.Member(skin, "m_createPavement");
                object grav = ModReflection.Member(skin, "m_createGravel");
                object ruin = ModReflection.Member(skin, "m_createRuining");
                w.Name("terrain").BeginObject()
                 .Prop("createPavement", pav is bool && (bool)pav)
                 .Prop("createGravel", grav is bool && (bool)grav)
                 .Prop("createRuining", ruin is bool && (bool)ruin)
                 .EndObject();
                w.EndObject();
            }

            w.EndArray();
            w.NewLine();
            WritePairs(w, "segments", segPairs);
            w.NewLine();
            WritePairs(w, "nodes", nodePairs);
            w.EndObject();
            stats["networkSkins.skins"] = skins.Count;
            stats["networkSkins.segments"] = segPairs.Count / 2;
            stats["networkSkins.nodes"] = nodePairs.Count / 2;
            log.Info("network skins: " + skins.Count + " distinct skins on " + segPairs.Count / 2 + " segments and " + nodePairs.Count / 2 + " nodes");
        }

        private static void Collect(Array arr, Dictionary<object, int> index, List<object> skins, List<int> pairs)
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++)
            {
                object skin = arr.GetValue(i);
                if (skin == null) continue;
                int k;
                if (!index.TryGetValue(skin, out k))
                {
                    k = skins.Count;
                    index[skin] = k;
                    skins.Add(skin);
                }

                pairs.Add(i);
                pairs.Add(k);
            }
        }

        private static void WritePairs(JsonWriter w, string name, List<int> pairs)
        {
            w.Name(name).BeginArray();
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i % 1000 == 0) w.NewLine();
                w.Value(pairs[i]);
            }

            w.EndArray();
        }

        // ---------------------------------------------------------------- serializable data

        /// <summary>
        /// Every SimulationManager.m_serializableDataStorage entry (mod data saved in the map) copied under
        /// the game's own lock and written to &lt;base&gt;-sdata/&lt;n&gt;-&lt;key&gt;.bin, plus index.json.
        /// </summary>
        public static void WriteSerializableData(JsonWriter w, string dir, string baseName, ExportLog log, Dictionary<string, int> stats)
        {
            Dictionary<string, byte[]> storage = Singleton<SimulationManager>.instance.m_serializableDataStorage;
            if (storage == null)
            {
                w.Name("serializableData").Null();
                return;
            }

            var copy = new List<KeyValuePair<string, byte[]>>();
            while (!Monitor.TryEnter(storage, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            try
            {
                foreach (var kv in storage) copy.Add(new KeyValuePair<string, byte[]>(kv.Key, kv.Value));
            }
            finally
            {
                Monitor.Exit(storage);
            }

            copy.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            string sub = baseName + "-sdata";
            string sdir = Path.Combine(dir, sub);
            Directory.CreateDirectory(sdir);
            long total = 0;
            int failed = 0;

            var idx = new StringWriter();
            var iw = new JsonWriter(idx);
            iw.BeginObject();
            iw.Prop("schema", "dakota-export/cs1/sdata/v1");
            iw.Prop("source", "SimulationManager.m_serializableDataStorage (Dictionary<string, byte[]>), copied under Monitor on the dictionary");
            iw.Name("entries").BeginArray();

            w.Name("serializableData").BeginObject();
            w.Prop("dir", sub);
            w.Prop("index", sub + "/index.json");
            w.Name("entries").BeginArray();
            for (int i = 0; i < copy.Count; i++)
            {
                string key = copy[i].Key;
                byte[] data = copy[i].Value ?? new byte[0];
                string file = i.ToString("D2") + "-" + SafeName(key) + ".bin";
                string error = null;
                string sha = null;
                try
                {
                    SafeFile.Write(Path.Combine(sdir, file), tmp => File.WriteAllBytes(tmp, data));
                    using (var h = SHA256.Create()) sha = Hex(h.ComputeHash(data));
                    total += data.Length;
                }
                catch (Exception ex)
                {
                    error = ex.GetType().Name + ": " + ex.Message;
                    failed++;
                }

                foreach (JsonWriter x in new[] { w, iw })
                {
                    x.NewLine();
                    x.BeginObject().Prop("key", key).Prop("file", error == null ? file : null).Prop("length", data.Length)
                     .Prop("sha256", sha).Prop("error", error).Prop("textPreview", Preview(data)).EndObject();
                }
            }

            w.EndArray();
            w.Prop("count", copy.Count);
            w.Prop("totalBytes", total);
            w.Prop("failed", failed);
            w.EndObject();
            iw.EndArray();
            iw.EndObject();
            SafeFile.Write(Path.Combine(sdir, "index.json"), tmp => File.WriteAllText(tmp, idx.ToString() + "\n", new UTF8Encoding(false)));
            stats["serializableData.entries"] = copy.Count;
            stats["serializableData.failed"] = failed;
            log.Info("serializable data: " + copy.Count + " entries, " + total + " bytes, " + failed + " failed, in " + sub);
        }

        /// <summary>Short printable preview for small text blobs (e.g. Theme Mixer's mix id); null otherwise.</summary>
        private static string Preview(byte[] data)
        {
            if (data.Length == 0 || data.Length > 256) return null;
            var sb = new StringBuilder();
            foreach (byte b in data)
            {
                if (b >= 32 && b < 127) sb.Append((char)b);
                else if (b == 0 || b < 32) sb.Append('.');
                else return null;
            }

            return sb.ToString();
        }

        private static string SafeName(string key)
        {
            var sb = new StringBuilder();
            foreach (char c in key)
                sb.Append((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.' ? c : '_');
            return sb.Length == 0 ? "key" : sb.ToString();
        }

        private static string Hex(byte[] b)
        {
            var sb = new StringBuilder(b.Length * 2);
            foreach (byte x in b) sb.Append(x.ToString("x2"));
            return sb.ToString();
        }
    }

    /// <summary>Theme Mixer 2.5 (Workshop 3031623332): current mix id/name/LUT, read without creating its singleton.</summary>
    internal static class ThemeMixerInfo
    {
        public static void Write(JsonWriter w)
        {
            w.Name("themeMixer");
            Type t = ModReflection.FindType("ThemeMixer.Themes.ThemeManager");
            if (t == null)
            {
                w.BeginObject().Prop("loaded", false).Prop("reason", "Theme Mixer not loaded (type ThemeMixer.Themes.ThemeManager not found)").EndObject();
                return;
            }

            // ThemeManager.Instance would create the component; read the private static _instance instead.
            object mgr = ModReflection.StaticField(t, "_instance");
            UnityEngine.Object uo = mgr as UnityEngine.Object;
            if (mgr == null || uo == null)
            {
                w.BeginObject().Prop("loaded", true).Prop("reason", "ThemeManager has no instance").EndObject();
                return;
            }

            object mix = ModReflection.Member(mgr, "CurrentMix");
            w.BeginObject();
            w.Prop("loaded", true);
            w.Prop("assembly", t.Assembly.GetName().Name + " " + t.Assembly.GetName().Version);
            w.Prop("mixIdSaved", ModReflection.Member(mgr, "MixID") as string);
            if (mix != null)
            {
                w.Prop("mixId", ModReflection.Member(mix, "ID") as string);
                w.Prop("mixName", ModReflection.Member(mix, "Name") as string);
                w.Prop("lut", ModReflection.Member(mix, "Lut") as string);
                w.Name("terrain");
                GenericDump.Write(w, ModReflection.Member(mix, "Terrain"), 3);
            }
            else
            {
                w.Name("mixId").Null();
                w.Name("mixName").Null();
                w.Name("lut").Null();
                w.Name("terrain").Null();
            }

            w.EndObject();
        }
    }

    /// <summary>Depth-limited, read-only dump of public instance fields (no properties, no method calls).</summary>
    internal static class GenericDump
    {
        public static void Write(JsonWriter w, object o, int depth)
        {
            if (o == null) { w.Null(); return; }
            UnityEngine.Object uo = o as UnityEngine.Object;
            if (uo != null || o is UnityEngine.Object)
            {
                // Unity "fake null" objects compare equal to null.
                if (uo == null) { w.Null(); return; }
                w.BeginObject().Prop("unityObject", uo.name).Prop("type", o.GetType().Name).EndObject();
                return;
            }

            if (o is string) { w.Value((string)o); return; }
            if (o is bool) { w.Value((bool)o); return; }
            if (o is int) { w.Value((int)o); return; }
            if (o is uint) { w.Value((uint)o); return; }
            if (o is long) { w.Value((long)o); return; }
            if (o is float) { w.Value((float)o); return; }
            if (o is double) { w.Value((float)(double)o); return; }
            if (o is byte || o is sbyte || o is short || o is ushort) { w.Value(Convert.ToInt32(o)); return; }
            if (o is Enum) { w.Value(o.ToString()); return; }
            if (o is Vector3) { w.Value((Vector3)o); return; }
            if (o is Color) { Color c = (Color)o; w.BeginArray().Value(c.r).Value(c.g).Value(c.b).Value(c.a).EndArray(); return; }
            if (o is Color32) { Color32 c = (Color32)o; w.BeginArray().Value((int)c.r).Value((int)c.g).Value((int)c.b).Value((int)c.a).EndArray(); return; }
            if (o is Quaternion) { Quaternion q = (Quaternion)o; w.BeginArray().Value(q.x).Value(q.y).Value(q.z).Value(q.w).EndArray(); return; }
            if (o is Delegate || o is Type) { w.Value(o.GetType().Name); return; }
            if (depth <= 0) { w.Value("<" + o.GetType().Name + ">"); return; }

            IDictionary dict = o as IDictionary;
            if (dict != null)
            {
                w.BeginObject();
                int n = 0;
                foreach (DictionaryEntry kv in dict)
                {
                    if (n++ >= 64) break;
                    w.Name(Convert.ToString(kv.Key));
                    Write(w, kv.Value, depth - 1);
                }

                w.EndObject();
                return;
            }

            IEnumerable list = o as IEnumerable;
            if (list != null)
            {
                w.BeginArray();
                int n = 0;
                foreach (object item in list)
                {
                    if (n++ >= 64) break;
                    Write(w, item, depth - 1);
                }

                w.EndArray();
                return;
            }

            WriteFields(w, o, depth);
        }

        public static void WriteFields(JsonWriter w, object o, int depth)
        {
            w.BeginObject();
            foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object v;
                try { v = f.GetValue(o); }
                catch (Exception) { continue; }
                w.Name(f.Name);
                Write(w, v, depth - 1);
            }

            w.EndObject();
        }
    }
}
