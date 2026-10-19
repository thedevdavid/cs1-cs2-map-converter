using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// World-space mesh of one Procedural Object. PO renders with
    /// Graphics.DrawMesh(m_mesh, m_position, m_rotation, ...) (no scale; m_scale is baked into the vertices),
    /// so world = m_position + m_rotation * localVertex.
    /// </summary>
    internal sealed class PoMesh
    {
        public Vector3[] World;
        public int[] Triangles;      // null when only vertex positions were readable
        public Vector2[] Uv;         // null when unavailable
        public string Source;        // "m_mesh" or "vertices"

        public int TriangleCount { get { return Triangles != null ? Triangles.Length / 3 : 0; } }

        public static PoMesh Read(object po, Vector3 position, Quaternion rotation, out string error)
        {
            error = null;
            Vector3[] local = null;
            int[] tris = null;
            Vector2[] uv = null;
            string source = null;

            Mesh mesh = ModReflection.Member(po, "m_mesh") as Mesh;
            if (mesh != null)
            {
                try
                {
                    local = mesh.vertices;      // returns an empty array (and logs) when the mesh is not readable
                    tris = mesh.triangles;
                    uv = mesh.uv;
                    source = "m_mesh";
                }
                catch (Exception ex)
                {
                    error = "m_mesh not readable: " + ex.Message;
                    local = null;
                    tris = null;
                }

                if (local != null && local.Length == 0 && mesh.vertexCount > 0)
                {
                    error = "m_mesh not readable (vertexCount " + mesh.vertexCount + ", vertices empty)";
                    local = null;
                    tris = null;
                }
            }

            if (local == null)
            {
                // Fallback: PO's own editable vertex list (ProceduralObjects.Classes.Vertex.Position, object space).
                IList verts = ModReflection.Member(po, "vertices") as IList;
                if (verts != null && verts.Count > 0)
                {
                    local = new Vector3[verts.Count];
                    for (int i = 0; i < verts.Count; i++)
                    {
                        object p = ModReflection.Member(verts[i], "Position");
                        local[i] = p is Vector3 ? (Vector3)p : Vector3.zero;
                    }

                    tris = null;
                    source = "vertices";
                }
            }

            if (local == null)
            {
                if (error == null) error = "no mesh and no vertex list";
                return null;
            }

            if (tris != null)
            {
                for (int i = 0; i < tris.Length; i++)
                {
                    if (tris[i] < 0 || tris[i] >= local.Length)
                    {
                        error = "triangle index out of range; triangles ignored";
                        tris = null;
                        break;
                    }
                }
            }

            var world = new Vector3[local.Length];
            for (int i = 0; i < local.Length; i++) world[i] = position + rotation * local[i];
            if (uv != null && uv.Length != local.Length) uv = null;
            return new PoMesh { World = world, Triangles = tris, Source = source, Uv = tris != null ? uv : null };
        }

        /// <summary>
        /// Writes "geometry" members: full counts plus a decimated world-space dump of at most maxVertices
        /// vertices, chosen by keeping every k-th triangle (smallest k that fits).
        /// </summary>
        public void WriteDump(JsonWriter w, int maxVertices)
        {
            w.Prop("source", Source);
            w.Prop("fullVertexCount", World.Length);
            w.Prop("fullTriangleCount", TriangleCount);

            if (Triangles == null)
            {
                int stride = Math.Max(1, (World.Length + maxVertices - 1) / maxVertices);
                w.Prop("triangleStride", 0);
                w.Prop("vertexStride", stride);
                w.Name("vertices").BeginArray();
                int n = 0;
                for (int i = 0; i < World.Length; i += stride) { WriteV(w, World[i]); n++; }
                w.EndArray();
                w.Prop("vertexCount", n);
                w.Name("triangles").Null();
                return;
            }

            int triCount = TriangleCount;
            int k = 1;
            Dictionary<int, int> remap = null;
            while (true)
            {
                remap = new Dictionary<int, int>();
                for (int t = 0; t < triCount; t += k)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        int vi = Triangles[t * 3 + c];
                        if (!remap.ContainsKey(vi)) remap[vi] = remap.Count;
                    }

                    if (remap.Count > maxVertices) break;
                }

                if (remap.Count <= maxVertices || k >= triCount) break;
                // Grow the stride roughly in proportion to the overshoot, at least by one.
                k = Math.Max(k + 1, (int)(k * 1.25f));
            }

            var order = new int[remap.Count];
            foreach (var kv in remap) order[kv.Value] = kv.Key;
            w.Prop("triangleStride", k);
            w.Prop("vertexStride", 0);
            w.Name("vertices").BeginArray();
            foreach (int vi in order) WriteV(w, World[vi]);
            w.EndArray();
            w.Prop("vertexCount", order.Length);
            w.Name("triangles").BeginArray();
            for (int t = 0; t < triCount; t += k)
            {
                int a, b, c;
                if (!remap.TryGetValue(Triangles[t * 3], out a) ||
                    !remap.TryGetValue(Triangles[t * 3 + 1], out b) ||
                    !remap.TryGetValue(Triangles[t * 3 + 2], out c)) continue;
                w.Value(a); w.Value(b); w.Value(c);
            }

            w.EndArray();
        }

        private static void WriteV(JsonWriter w, Vector3 v)
        {
            w.Value2(v.x); w.Value2(v.y); w.Value2(v.z);
        }
    }

    /// <summary>
    /// Max/min world height of PO geometry on the 1081 heightmap vertex grid. Each cell is the 16 m square
    /// centred on its sample point (x = -8640 + col*16). A triangle contributes at every sample point it
    /// covers (barycentric height), at its vertices, and along its edges every 4 m, so thin and vertical
    /// faces still mark the cells they pass through.
    /// </summary>
    internal sealed class PoHeightRaster
    {
        public const int Size = 1081;
        public const float CellSize = 16f;
        private const float Origin = -8640f;

        private readonly float[] m_max = new float[Size * Size];
        private readonly float[] m_min = new float[Size * Size];
        private readonly int[] m_owner = new int[Size * Size];
        private readonly bool[] m_hit = new bool[Size * Size];
        public int Objects;
        public int Triangles;
        public int VertexOnlyObjects;

        public void Add(int objectId, PoMesh mesh)
        {
            Objects++;
            Vector3[] v = mesh.World;
            if (mesh.Triangles == null)
            {
                VertexOnlyObjects++;
                for (int i = 0; i < v.Length; i++) Point(v[i].x, v[i].z, v[i].y, objectId);
                return;
            }

            int[] t = mesh.Triangles;
            for (int i = 0; i + 2 < t.Length; i += 3)
            {
                Triangle(v[t[i]], v[t[i + 1]], v[t[i + 2]], objectId);
                Triangles++;
            }
        }

        private void Triangle(Vector3 a, Vector3 b, Vector3 c, int id)
        {
            Point(a.x, a.z, a.y, id);
            Point(b.x, b.z, b.y, id);
            Point(c.x, c.z, c.y, id);
            Edge(a, b, id);
            Edge(b, c, id);
            Edge(c, a, id);

            float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x)), maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            float minZ = Mathf.Min(a.z, Mathf.Min(b.z, c.z)), maxZ = Mathf.Max(a.z, Mathf.Max(b.z, c.z));
            int c0 = Mathf.Max(0, Mathf.CeilToInt((minX - Origin) / CellSize));
            int c1 = Mathf.Min(Size - 1, Mathf.FloorToInt((maxX - Origin) / CellSize));
            int r0 = Mathf.Max(0, Mathf.CeilToInt((minZ - Origin) / CellSize));
            int r1 = Mathf.Min(Size - 1, Mathf.FloorToInt((maxZ - Origin) / CellSize));
            if (c0 > c1 || r0 > r1) return;

            float den = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
            if (Mathf.Abs(den) < 1e-6f) return; // degenerate in plan view (vertical face): edges cover it
            for (int r = r0; r <= r1; r++)
            {
                float pz = Origin + r * CellSize;
                for (int col = c0; col <= c1; col++)
                {
                    float px = Origin + col * CellSize;
                    float l1 = ((b.z - c.z) * (px - c.x) + (c.x - b.x) * (pz - c.z)) / den;
                    float l2 = ((c.z - a.z) * (px - c.x) + (a.x - c.x) * (pz - c.z)) / den;
                    float l3 = 1f - l1 - l2;
                    if (l1 < -1e-4f || l2 < -1e-4f || l3 < -1e-4f) continue;
                    Cell(r * Size + col, l1 * a.y + l2 * b.y + l3 * c.y, id);
                }
            }
        }

        private void Edge(Vector3 a, Vector3 b, int id)
        {
            float len = Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.z - a.z) * (b.z - a.z));
            int steps = Mathf.Min(4096, Mathf.CeilToInt(len / 4f));
            for (int s = 1; s < steps; s++)
            {
                float f = s / (float)steps;
                Point(a.x + (b.x - a.x) * f, a.z + (b.z - a.z) * f, a.y + (b.y - a.y) * f, id);
            }
        }

        private void Point(float x, float z, float y, int id)
        {
            int col = Mathf.RoundToInt((x - Origin) / CellSize);
            int row = Mathf.RoundToInt((z - Origin) / CellSize);
            if (col < 0 || row < 0 || col >= Size || row >= Size) return;
            Cell(row * Size + col, y, id);
        }

        private void Cell(int i, float y, int id)
        {
            if (!m_hit[i])
            {
                m_hit[i] = true;
                m_max[i] = y;
                m_min[i] = y;
                m_owner[i] = id;
                return;
            }

            if (y > m_max[i]) { m_max[i] = y; m_owner[i] = id; }
            if (y < m_min[i]) m_min[i] = y;
        }

        public int CoveredCells()
        {
            int n = 0;
            foreach (bool h in m_hit) if (h) n++;
            return n;
        }

        public string WritePng(string path)
        {
            var png = new ushort[Size * Size];
            for (int row = 0; row < Size; row++)
            {
                int dst = (Size - 1 - row) * Size;
                for (int col = 0; col < Size; col++)
                {
                    int i = row * Size + col;
                    if (!m_hit[i]) continue;
                    float raw = m_max[i] * 64f;
                    png[dst + col] = (ushort)Mathf.Clamp(Mathf.RoundToInt(raw), 1, 65535);
                }
            }

            return PngWriter.WriteGray16(path, Size, Size, png);
        }

        public void WriteJson(string path, string pngName, int skippedQuads)
        {
            int clipped = 0;
            using (var stream = SafeFile.CreateText(path))
            {
                var w = new JsonWriter(stream);
                w.BeginObject();
                w.Prop("schema", "dakota-export/cs1/po-heights/v1");
                w.Name("grid").BeginObject();
                w.Prop("size", Size);
                w.Prop("cellSize", CellSize);
                w.Prop("originX", Origin);
                w.Prop("originZ", Origin);
                w.Prop("rowAxis", "+z");
                w.Prop("note", "index = row*1081 + col; cell = 16 m square centred on x = -8640 + col*16, z = -8640 + row*16; row 0 = south (same grid as waterSurface)");
                w.EndObject();
                w.Prop("encoding", "cells: flat [index, maxY, minY, objectId, ...] in world metres rounded to 0.01; objectId = PO id owning maxY");
                w.Prop("png", pngName);
                w.Prop("pngEncoding", "1081x1081 16-bit grey, row 0 = north, value = round(maxY*64) clamped to 1..65535 on covered cells, 0 elsewhere");
                w.Prop("objectsRasterized", Objects);
                w.Prop("vertexOnlyObjects", VertexOnlyObjects);
                w.Prop("trianglesRasterized", Triangles);
                w.Prop("skippedQuadObjects", skippedQuads);
                w.Prop("coveredCells", CoveredCells());
                w.NewLine();
                w.Name("cells").BeginArray();
                int written = 0;
                for (int i = 0; i < m_hit.Length; i++)
                {
                    if (!m_hit[i]) continue;
                    if (m_max[i] * 64f > 65535f) clipped++;
                    if (written % 500 == 0) w.NewLine();
                    w.Value(i);
                    w.Value2(m_max[i]);
                    w.Value2(m_min[i]);
                    w.Value(m_owner[i]);
                    written++;
                }

                w.EndArray();
                w.Prop("cellsAbovePngRange", clipped);
                w.EndObject();
                stream.Write('\n');
                stream.Commit();
            }
        }
    }

    /// <summary>
    /// PO modules by reflection. Generic: ProceduralObjects.POModule (ModuleType.TypeID/Name, enabled,
    /// _get_data(true), which only builds the dictionary PO saves). Effects Module (EffectsPOModule.EffectsModule,
    /// Workshop 2632235148): effect (ParticleEffect), offset, direction, Width, Height, magnitude,
    /// acceleration, emitRate; it renders at parent.m_position + parent.m_rotation * offset.
    /// </summary>
    internal static class PoModules
    {
        public const string EffectsTypeName = "EffectsPOModule.EffectsModule";

        public static Type EffectsType()
        {
            return ModReflection.FindType(EffectsTypeName);
        }

        public static int WriteModulesAndEffects(JsonWriter w, object po, Vector3 position, Quaternion rotation, Type effectsType, string effectsReason, Dictionary<string, int> effectCounts)
        {
            IList modules = ModReflection.Member(po, "m_modules") as IList;
            var dataEffects = new List<IDictionary>();
            w.Name("modules").BeginArray();
            if (modules != null)
            {
                foreach (object m in modules)
                {
                    if (m == null) continue;
                    w.BeginObject();
                    w.Prop("class", m.GetType().FullName);
                    object mt = ModReflection.Member(m, "ModuleType");
                    w.Prop("typeId", mt != null ? ModReflection.Member(mt, "TypeID") as string : null);
                    w.Prop("name", mt != null ? ModReflection.Member(mt, "Name") as string : null);
                    object en = ModReflection.Member(m, "enabled");
                    w.Prop("enabled", en is bool && (bool)en);
                    w.Name("data");
                    IDictionary data = null;
                    try
                    {
                        var get = m.GetType().GetMethod("_get_data", new[] { typeof(bool) });
                        if (get != null) data = get.Invoke(m, new object[] { true }) as IDictionary;
                    }
                    catch (Exception)
                    {
                        data = null;
                    }

                    if (data != null)
                    {
                        w.BeginObject();
                        foreach (DictionaryEntry kv in data) w.Prop(Convert.ToString(kv.Key), Convert.ToString(kv.Value));
                        w.EndObject();
                        if (data.Contains("effectName")) dataEffects.Add(data);
                    }
                    else
                    {
                        w.Null();
                    }

                    w.EndObject();
                }
            }

            w.EndArray();

            w.Name("effects");
            if (effectsType == null)
            {
                // Module class not loaded: fall back to any module data that carries an effectName.
                if (dataEffects.Count == 0)
                {
                    w.Null();
                    w.Prop("effectsReason", effectsReason);
                    return 0;
                }

                w.BeginArray();
                foreach (IDictionary data in dataEffects) WriteEffectFromData(w, data, position, rotation, effectCounts);
                w.EndArray();
                w.Prop("effectsReason", effectsReason + "; effects rebuilt from module data");
                return dataEffects.Count;
            }

            int count = 0;
            w.BeginArray();
            if (modules != null)
            {
                foreach (object m in modules)
                {
                    if (m == null || !effectsType.IsInstanceOfType(m)) continue;
                    UnityEngine.Object effect = ModReflection.Member(m, "effect") as UnityEngine.Object;
                    object off = ModReflection.Member(m, "offset");
                    object dir = ModReflection.Member(m, "direction");
                    Vector3 offset = off is Vector3 ? (Vector3)off : Vector3.zero;
                    Vector3 direction = dir is Vector3 ? (Vector3)dir : Vector3.up;
                    w.BeginObject();
                    string effectName = effect != null ? effect.name : null;
                    w.Prop("prefab", effectName);
                    w.Prop("position", position + rotation * offset);
                    w.Prop("worldPosition", position + rotation * offset);
                    w.Prop("worldDirection", rotation * direction);
                    w.Name("rotation").BeginArray().Value(rotation.x).Value(rotation.y).Value(rotation.z).Value(rotation.w).EndArray();
                    w.Prop("direction", rotation * direction);
                    w.Prop("localOffset", offset);
                    WriteFloat(w, m, "Width", "width");
                    WriteFloat(w, m, "Height", "height");
                    WriteFloat(w, m, "magnitude", "magnitude");
                    WriteFloat(w, m, "acceleration", "acceleration");
                    object rate = ModReflection.Member(m, "emitRate");
                    w.Name("emitRate"); if (rate is byte) w.Value((int)(byte)rate); else w.Null();
                    object en = ModReflection.Member(m, "enabled");
                    w.Prop("enabled", en is bool && (bool)en);
                    w.EndObject();
                    Count(effectCounts, effectName);
                    count++;
                }
            }

            w.EndArray();
            return count;
        }

        /// <summary>
        /// One effect from a module data dictionary (keys effectName, width, height, magnitude, acceleration,
        /// direction, offset, emitRate, as EffectsModule.GetData writes them; vectors are "(x, y, z)").
        /// Used for failed containers (ProceduralObjectContainer.modulesData) and when the module class is absent.
        /// </summary>
        public static void WriteEffectFromData(JsonWriter w, IDictionary data, Vector3 position, Quaternion rotation, Dictionary<string, int> effectCounts)
        {
            string name = Str(data, "effectName");
            Vector3 offset, direction;
            bool hasOffset = TryParseVector3(Str(data, "offset"), out offset);
            bool hasDir = TryParseVector3(Str(data, "direction"), out direction);
            if (!hasOffset) offset = Vector3.zero;
            if (!hasDir) direction = Vector3.up;
            w.BeginObject();
            w.Prop("source", "moduleData");
            w.Prop("prefab", name);
            w.Prop("position", position + rotation * offset);
            w.Prop("worldPosition", position + rotation * offset);
            w.Prop("worldDirection", rotation * direction);
            w.Name("rotation").BeginArray().Value(rotation.x).Value(rotation.y).Value(rotation.z).Value(rotation.w).EndArray();
            w.Prop("direction", rotation * direction);
            w.Prop("localOffset", offset);
            w.Prop("offsetParsed", hasOffset);
            w.Prop("directionParsed", hasDir);
            WriteParsedFloat(w, data, "width");
            WriteParsedFloat(w, data, "height");
            WriteParsedFloat(w, data, "magnitude");
            WriteParsedFloat(w, data, "acceleration");
            WriteParsedFloat(w, data, "emitRate");
            string en = Str(data, "enabled");
            w.Name("enabled"); if (en != null) w.Value(string.Equals(en, "True", StringComparison.OrdinalIgnoreCase)); else w.Null();
            w.Prop("typeId", Str(data, "TypeID"));
            w.EndObject();
            Count(effectCounts, name);
        }

        public static void Count(Dictionary<string, int> counts, string name)
        {
            if (counts == null) return;
            string key = name ?? "<null>";
            int c;
            counts.TryGetValue(key, out c);
            counts[key] = c + 1;
        }

        private static string Str(IDictionary d, string key)
        {
            return d.Contains(key) ? Convert.ToString(d[key]) : null;
        }

        private static void WriteParsedFloat(JsonWriter w, IDictionary d, string key)
        {
            float f;
            w.Name(key);
            if (TryParseFloat(Str(d, key), out f)) w.Value(f); else w.Null();
        }

        /// <summary>PO writes floats with the game's current culture; try that first, then invariant.</summary>
        public static bool TryParseFloat(string s, out float f)
        {
            f = 0f;
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim();
            try
            {
                f = float.Parse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture);
                return true;
            }
            catch (Exception)
            {
            }

            try
            {
                f = float.Parse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Parses PO's "(x, y, z)" (VertexUtils.ToStringUnrounded; components separated by ", ").</summary>
        public static bool TryParseVector3(string s, out Vector3 v)
        {
            v = Vector3.zero;
            if (string.IsNullOrEmpty(s)) return false;
            string[] parts = s.Replace("(", "").Replace(")", "").Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                // Invariant fallback when the separator differs: "x,y,z" with '.' decimals.
                parts = s.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
                if (parts.Length != 3) return false;
            }

            float x, y, z;
            if (!TryParseFloat(parts[0], out x) || !TryParseFloat(parts[1], out y) || !TryParseFloat(parts[2], out z)) return false;
            v = new Vector3(x, y, z);
            return true;
        }

        private static void WriteFloat(JsonWriter w, object o, string member, string key)
        {
            object v = ModReflection.Member(o, member);
            w.Name(key);
            if (v is float) w.Value((float)v); else w.Null();
        }
    }
}
