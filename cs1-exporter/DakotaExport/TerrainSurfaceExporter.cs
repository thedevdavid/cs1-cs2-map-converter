using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Terrain surface classification. Three sources, combined on a 1080 x 1080 / 16 m grid:
    /// 1. Surface Painter mod data (persistent, 4 m), read by reflection if the mod is loaded;
    /// 2. TerrainManager.RawSurface (not serialized; rebuilt on load from buildings, networks and props);
    /// 3. NaturalResourceManager m_sand (persistent, the map editor's sand brush, 33.75 m).
    /// Values use TerrainModify.Surface flags so one legend fits all; 128 = sand.
    /// </summary>
    internal static class TerrainSurfaceExporter
    {
        public const int Size = 1080;
        public const float CellSize = 16f;
        public const int SandValue = 128;
        public const int ChannelThreshold = 128;

        private static readonly int[] s_legendValues = { 0, 1, 2, 4, 8, 16, 32, 64, 128 };
        private static readonly string[] s_legendNames = { "None", "Clip", "PavementA", "PavementB", "Ruined", "Gravel", "Field", "RuinedWeak", "Sand" };
        private static readonly byte[] s_pngGrey = { 0, 40, 70, 100, 130, 160, 190, 215, 245 };

        public static void Write(JsonWriter w, string pngPath, ExportLog log, Dictionary<string, int> stats)
        {
            int n = Size * Size;
            var combined = new byte[n];
            var sourceCounts = new Dictionary<string, int>();
            var notes = new List<string>();

            // ---- 2. object-driven raw surface (lowest priority after sand)
            byte[] objectGrid = ReadRawSurface(notes);

            // ---- 3. sand from natural resources
            byte[] sandGrid = ReadSand(notes);

            // ---- 1. Surface Painter
            SurfacePainterData sp = SurfacePainterData.TryRead(notes, log);

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    int i = row * Size + col;
                    int v = 0;
                    string src = null;
                    if (sp != null)
                    {
                        v = sp.Majority16m(col, row);
                        if (v != 0) src = "surfacePainter";
                    }

                    if (v == 0 && objectGrid != null && objectGrid[i] != 0) { v = objectGrid[i]; src = "objects"; }
                    if (v == 0 && sandGrid != null && sandGrid[i] != 0) { v = SandValue; src = "sand"; }
                    combined[i] = (byte)v;
                    if (src != null)
                    {
                        int c;
                        sourceCounts.TryGetValue(src, out c);
                        sourceCounts[src] = c + 1;
                    }
                }
            }

            var valueCounts = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
            {
                if (combined[i] == 0) continue;
                int c;
                valueCounts.TryGetValue(combined[i], out c);
                valueCounts[combined[i]] = c + 1;
            }

            w.Name("terrainSurfaces").BeginObject();
            w.Name("grid").BeginObject();
            w.Prop("size", Size);
            w.Prop("cellSize", CellSize);
            w.Prop("originX", -8640f);
            w.Prop("originZ", -8640f);
            w.Prop("rowAxis", "+z");
            w.Prop("note", "index = row*1080 + col; sample point x = -8640 + col*16, z = -8640 + row*16 (TerrainManager raw surface grid), row 0 = south");
            w.EndObject();
            w.Name("legend").BeginObject();
            for (int l = 0; l < s_legendValues.Length; l++) w.Prop(s_legendValues[l].ToString(), s_legendNames[l]);
            w.EndObject();
            w.Prop("priority", "surfacePainter (majority of the 16 4-m cells, painted if >= 8) > objects (RawSurface channel >= 128, strongest wins) > sand (m_sand >= 128)");
            w.Prop("encoding", "cells: sparse flat [index, value, ...] for value != 0");
            w.Prop("png", Path.GetFileName(pngPath));
            w.Name("pngGrey").BeginObject();
            for (int l = 0; l < s_legendValues.Length; l++) w.Prop(s_legendNames[l], (int)s_pngGrey[l]);
            w.EndObject();
            w.Name("sources").BeginObject();
            w.Name("surfacePainter");
            if (sp != null) sp.WriteSummary(w); else w.Null();
            w.Name("objects").BeginObject();
            w.Prop("available", objectGrid != null);
            w.Prop("persistent", false);
            w.Prop("source", "TerrainManager.RawSurface (SurfaceCell m_clipped, m_pavementA, m_pavementB, m_ruined, m_gravel, m_field); not serialized, rebuilt by TerrainModify from buildings, networks and props on load");
            w.EndObject();
            w.Name("sand").BeginObject();
            w.Prop("available", sandGrid != null);
            w.Prop("persistent", true);
            w.Prop("source", "NaturalResourceManager.m_naturalResources[].m_sand (serialized; map editor sand brush), 512 x 33.75 m, nearest-sampled at each 16 m point");
            w.EndObject();
            w.EndObject();
            w.Name("cellsBySource").BeginObject();
            foreach (var kv in sourceCounts) w.Prop(kv.Key, kv.Value);
            w.EndObject();
            w.Name("counts").BeginObject();
            foreach (var kv in valueCounts) w.Prop(kv.Key.ToString(), kv.Value);
            w.EndObject();
            w.StringArray("notes", notes);
            w.Name("cells").BeginArray();
            int written = 0;
            for (int i = 0; i < n; i++)
            {
                if (combined[i] == 0) continue;
                if (written % 1000 == 0) w.NewLine();
                w.Value(i);
                w.Value((int)combined[i]);
                written++;
            }

            w.EndArray();
            if (sp != null)
            {
                w.NewLine();
                sp.WriteNative(w);
            }

            w.EndObject();

            // PNG, row 0 = north.
            var png = new byte[n];
            for (int row = 0; row < Size; row++)
            {
                int dst = (Size - 1 - row) * Size;
                for (int col = 0; col < Size; col++) png[dst + col] = Grey(combined[row * Size + col]);
            }

            string method = PngWriter.WriteGray8(pngPath, Size, Size, png);
            stats["terrainSurfaces.cells"] = written;
            foreach (var kv in sourceCounts) stats["terrainSurfaces." + kv.Key] = kv.Value;
            log.Info("terrain surfaces: " + written + " classified cells (" + string.Join(", ", Describe(sourceCounts)) + "); png " +
                     Path.GetFileName(pngPath) + " (" + method + ")");
        }

        private static string[] Describe(Dictionary<string, int> d)
        {
            var list = new List<string>();
            foreach (var kv in d) list.Add(kv.Key + " " + kv.Value);
            return list.ToArray();
        }

        private static byte Grey(byte value)
        {
            for (int l = 0; l < s_legendValues.Length; l++) if (s_legendValues[l] == value) return s_pngGrey[l];
            return 255;
        }

        private static byte[] ReadRawSurface(List<string> notes)
        {
            try
            {
                TerrainManager.SurfaceCell[] raw = Singleton<TerrainManager>.instance.RawSurface;
                if (raw == null || raw.Length != Size * Size)
                {
                    notes.Add("RawSurface unavailable or unexpected length " + (raw == null ? -1 : raw.Length));
                    return null;
                }

                var grid = new byte[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                {
                    TerrainManager.SurfaceCell c = raw[i];
                    int best = ChannelThreshold - 1, v = 0;
                    Pick(c.m_clipped, 1, ref best, ref v);
                    Pick(c.m_pavementA, 2, ref best, ref v);
                    Pick(c.m_pavementB, 4, ref best, ref v);
                    Pick(c.m_ruined, 8, ref best, ref v);
                    Pick(c.m_gravel, 16, ref best, ref v);
                    Pick(c.m_field, 32, ref best, ref v);
                    grid[i] = (byte)v;
                }

                return grid;
            }
            catch (Exception ex)
            {
                notes.Add("RawSurface read failed: " + ex.Message);
                return null;
            }
        }

        private static void Pick(byte channel, int value, ref int best, ref int v)
        {
            if (channel > best)
            {
                best = channel;
                v = value;
            }
        }

        private static byte[] ReadSand(List<string> notes)
        {
            try
            {
                NaturalResourceManager.ResourceCell[] res = Singleton<NaturalResourceManager>.instance.m_naturalResources;
                const int rs = NaturalResourceManager.RESOURCEGRID_RESOLUTION;
                const float rc = NaturalResourceManager.RESOURCEGRID_CELL_SIZE;
                if (res == null || res.Length != rs * rs)
                {
                    notes.Add("natural resources unavailable for sand");
                    return null;
                }

                var sand = new byte[rs * rs];
                bool any = false;
                for (int i = 0; i < sand.Length; i++)
                {
                    sand[i] = res[i].m_sand;
                    if (sand[i] != 0) any = true;
                }

                if (!any)
                {
                    notes.Add("m_sand is zero everywhere");
                    return null;
                }

                var grid = new byte[Size * Size];
                for (int row = 0; row < Size; row++)
                {
                    float z = -8640f + row * CellSize;
                    int rz = Mathf.Clamp((int)(z / rc + rs / 2f), 0, rs - 1);
                    for (int col = 0; col < Size; col++)
                    {
                        float x = -8640f + col * CellSize;
                        int rx = Mathf.Clamp((int)(x / rc + rs / 2f), 0, rs - 1);
                        if (sand[rz * rs + rx] >= ChannelThreshold) grid[row * Size + col] = 1;
                    }
                }

                return grid;
            }
            catch (Exception ex)
            {
                notes.Add("sand read failed: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Reflection view of SurfacePainter.SurfaceManager (Workshop 689937287). Its grid is 4320 x 4 m
        /// with 81 Tiles, else 2400 x 4 m covering the centre 25 tiles (offset 960 cells), index z*width + x.
        /// Each item is { Surface surface; bool overrideExisting } with Surface = TerrainModify.Surface.
        /// </summary>
        private sealed class SurfacePainterData
        {
            private const int FullGrid = 4320;

            private int[] m_values;      // TerrainModify.Surface value, +256 when overrideExisting
            private int m_width;
            private int m_offset;        // cell offset of this array inside the 4320 frame
            private bool m_eightyOne;
            private string m_assembly;
            private readonly int[] m_hv = new int[16];
            private readonly int[] m_hc = new int[16];

            public static SurfacePainterData TryRead(List<string> notes, ExportLog log)
            {
                Type mgrType = ModReflection.FindType("SurfacePainter.SurfaceManager");
                if (mgrType == null)
                {
                    notes.Add("Surface Painter not loaded (type SurfacePainter.SurfaceManager not found)");
                    return null;
                }

                try
                {
                    object mgr = ModReflection.SingletonIfExists(mgrType);
                    if (mgr == null)
                    {
                        notes.Add("Surface Painter loaded but its SurfaceManager singleton does not exist (mod disabled?)");
                        return null;
                    }

                    FieldInfo arrField = mgrType.GetField("m_surfaces", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    FieldInfo eightyField = mgrType.GetField("isEightyOneEnabled", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    Array arr = arrField != null ? arrField.GetValue(mgr) as Array : null;
                    if (arr == null)
                    {
                        notes.Add("Surface Painter loaded but m_surfaces is null (nothing painted or not yet loaded)");
                        return null;
                    }

                    Type itemType = arr.GetType().GetElementType();
                    var arrParam = Expression.Parameter(typeof(Array), "a");
                    var idxParam = Expression.Parameter(typeof(int), "i");
                    var element = Expression.ArrayIndex(Expression.Convert(arrParam, arr.GetType()), idxParam);
                    var surface = Expression.Convert(Expression.Field(element, "surface"), typeof(int));
                    var over = Expression.Field(element, "overrideExisting");
                    var body = Expression.Add(surface, Expression.Condition(over, Expression.Constant(256), Expression.Constant(0)));
                    Func<Array, int, int> get = Expression.Lambda<Func<Array, int, int>>(body, arrParam, idxParam).Compile();

                    var data = new SurfacePainterData();
                    data.m_assembly = mgrType.Assembly.GetName().Name + " " + mgrType.Assembly.GetName().Version;
                    data.m_eightyOne = eightyField != null && (bool)eightyField.GetValue(mgr);
                    int len = arr.Length;
                    data.m_width = (int)Math.Round(Math.Sqrt(len));
                    if (data.m_width * data.m_width != len) throw new InvalidOperationException("m_surfaces length " + len + " is not square");
                    data.m_offset = (FullGrid - data.m_width) / 2;
                    data.m_values = new int[len];
                    for (int i = 0; i < len; i++) data.m_values[i] = get(arr, i);
                    log.Info("surface painter: " + data.m_width + "^2 cells, 81 tiles " + data.m_eightyOne);
                    return data;
                }
                catch (Exception ex)
                {
                    notes.Add("Surface Painter read failed: " + ex.GetType().Name + ": " + ex.Message);
                    return null;
                }
            }

            private int At(int fx, int fz)
            {
                int x = fx - m_offset, z = fz - m_offset;
                if (x < 0 || z < 0 || x >= m_width || z >= m_width) return 0;
                return m_values[z * m_width + x];
            }

            /// <summary>Majority surface of the 4x4 4-m cells starting at raw sample (col,row); 0 if fewer than 8 are painted.</summary>
            public int Majority16m(int col, int row)
            {
                int bx = col * 4, bz = row * 4;
                int painted = 0, best = 0, bestCount = 0, used = 0;
                // 16 samples hold at most 16 distinct values; m_hv/m_hc are reused scratch slots.
                for (int dz = 0; dz < 4; dz++)
                {
                    for (int dx = 0; dx < 4; dx++)
                    {
                        int v = At(bx + dx, bz + dz) & 0xFF;
                        if (v == 0) continue;
                        painted++;
                        int s = 0;
                        while (s < used && m_hv[s] != v) s++;
                        if (s == used) { m_hv[used] = v; m_hc[used] = 0; used++; }
                        m_hc[s]++;
                    }
                }

                if (painted < 8) return 0;
                for (int s = 0; s < used; s++)
                {
                    if (m_hc[s] > bestCount) { best = m_hv[s]; bestCount = m_hc[s]; }
                }

                return best;
            }

            public void WriteSummary(JsonWriter w)
            {
                var counts = new Dictionary<int, int>();
                foreach (int v in m_values)
                {
                    if (v == 0) continue;
                    int c;
                    counts.TryGetValue(v, out c);
                    counts[v] = c + 1;
                }

                w.BeginObject();
                w.Prop("available", true);
                w.Prop("persistent", true);
                w.Prop("assembly", m_assembly);
                w.Prop("source", "SurfacePainter.SurfaceManager.m_surfaces (saved as serializable data 'SurfaceManager')");
                w.Prop("eightyOneTiles", m_eightyOne);
                w.Prop("nativeWidth", m_width);
                w.Prop("nativeOffsetCells", m_offset);
                w.Name("nativeCounts").BeginObject();
                foreach (var kv in counts) w.Prop(kv.Key.ToString(), kv.Value);
                w.EndObject();
                w.EndObject();
            }

            /// <summary>Full-resolution data as row runs on the 4320 x 4 m frame.</summary>
            public void WriteNative(JsonWriter w)
            {
                w.Name("surfacePainterNative").BeginObject();
                w.Name("grid").BeginObject();
                w.Prop("size", FullGrid);
                w.Prop("cellSize", 4f);
                w.Prop("originX", -8640f);
                w.Prop("originZ", -8640f);
                w.Prop("rowAxis", "+z");
                w.Prop("note", "index = row*4320 + col, cell covers x in [-8640 + col*4, +4), row 0 = south");
                w.EndObject();
                w.Prop("valueEncoding", "TerrainModify.Surface value, +256 when the cell was painted with overrideExisting");
                w.Prop("encoding", "runs: flat [startIndex, length, value, ...] along rows, value 0 omitted");
                w.Name("runs").BeginArray();
                int runs = 0;
                for (int z = 0; z < m_width; z++)
                {
                    int x = 0;
                    while (x < m_width)
                    {
                        int v = m_values[z * m_width + x];
                        int start = x;
                        while (x < m_width && m_values[z * m_width + x] == v) x++;
                        if (v == 0) continue;
                        if (runs % 500 == 0) w.NewLine();
                        w.Value((z + m_offset) * FullGrid + start + m_offset);
                        w.Value(x - start);
                        w.Value(v);
                        runs++;
                    }
                }

                w.EndArray();
                w.Prop("runCount", runs);
                w.EndObject();
            }
        }
    }
}
