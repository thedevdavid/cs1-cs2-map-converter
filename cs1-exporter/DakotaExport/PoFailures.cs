using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// &lt;base&gt;-po-meshes.bin: full-resolution PO meshes (edited live objects and failed containers).
    /// Little-endian. Header: 8 bytes "DKPOMSH1", int32 version (1). Then entries back to back:
    /// int32 id, int32 kind (0 = live edited object, 1 = failed container), int32 vertexCount,
    /// float32[3 * vertexCount] world-space xyz, int32 triangleIndexCount, int32[triangleIndexCount],
    /// int32 uvCount, float32[2 * uvCount]. Offsets of each entry are listed in -po.json.
    /// </summary>
    internal sealed class PoMeshBin
    {
        public const string FormatNote = "little-endian; header 'DKPOMSH1' + int32 version 1; per entry at 'offset': int32 id, int32 kind (0 live edited, 1 failed container), int32 vertexCount, float32[3*vertexCount] WORLD xyz, int32 triangleIndexCount, int32[...], int32 uvCount, float32[2*uvCount]";

        public struct Entry
        {
            public long Offset;
            public int VertexCount;
            public int TriangleIndexCount;
            public int UvCount;
        }

        private readonly string m_path;
        private readonly string m_tmp;
        private FileStream m_fs;
        private BinaryWriter m_w;
        private bool m_committed;
        public int Count;

        public PoMeshBin(string path)
        {
            m_path = path;
            m_tmp = path + ".tmp";
            m_fs = new FileStream(m_tmp, FileMode.Create, FileAccess.Write);
            m_w = new BinaryWriter(m_fs);
            m_w.Write(new[] { (byte)'D', (byte)'K', (byte)'P', (byte)'O', (byte)'M', (byte)'S', (byte)'H', (byte)'1' });
            m_w.Write(1);
        }

        public Entry Add(int id, int kind, Vector3[] world, int[] triangles, Vector2[] uv)
        {
            var e = new Entry { Offset = m_fs.Position, VertexCount = world.Length, TriangleIndexCount = triangles != null ? triangles.Length : 0, UvCount = uv != null ? uv.Length : 0 };
            m_w.Write(id);
            m_w.Write(kind);
            m_w.Write(world.Length);
            foreach (Vector3 v in world) { m_w.Write(v.x); m_w.Write(v.y); m_w.Write(v.z); }
            m_w.Write(e.TriangleIndexCount);
            if (triangles != null) foreach (int t in triangles) m_w.Write(t);
            m_w.Write(e.UvCount);
            if (uv != null) foreach (Vector2 u in uv) { m_w.Write(u.x); m_w.Write(u.y); }
            Count++;
            return e;
        }

        public void Commit()
        {
            m_committed = true;
        }

        public void Close()
        {
            if (m_w == null) return;
            m_w.Close();
            m_w = null;
            m_fs = null;
            if (m_committed)
            {
                if (File.Exists(m_path)) File.Delete(m_path);
                File.Move(m_tmp, m_path);
            }
            else
            {
                try { if (File.Exists(m_tmp)) File.Delete(m_tmp); }
                catch { }
            }
        }
    }

    /// <summary>
    /// Procedural Objects that failed to load. PO keeps them in the static
    /// ProceduralObjects.Classes.PopupStart.loading_failures (List&lt;POLoadingFailureGroup&gt;), each group
    /// with missing_asset and containers (Dictionary&lt;ProceduralObjectContainer, Exception&gt;).
    /// ProceduralObjectContainer fields: id, objectType, basePrefabName, position (SerializableVector3),
    /// rotation (SerializableQuaternion), scale, meshStatus, vertices (SerializableVector3[]),
    /// serializedMeshData (POSerializableMeshData.data: Dictionary&lt;SerializableVector3, List&lt;int&gt;&gt;),
    /// customTextureName, hasCustomTexture, color, layerId, groupRootId, belongsToGroup, visibility,
    /// renderDistance, renderDistLocked, tilingFactor, flipFaces, disableCastShadows, textParam, modulesData.
    /// </summary>
    internal static class PoFailures
    {
        public static int Write(JsonWriter w, PoMeshBin meshes, Dictionary<string, int> effectCounts, out string note)
        {
            note = null;
            Type popup = ModReflection.FindType("ProceduralObjects.Classes.PopupStart");
            if (popup == null)
            {
                note = "type ProceduralObjects.Classes.PopupStart not found";
                return 0;
            }

            IList groups = ModReflection.StaticField(popup, "loading_failures") as IList;
            if (groups == null)
            {
                note = "PopupStart.loading_failures is null";
                return 0;
            }

            int written = 0, errors = 0;
            foreach (object g in groups)
            {
                if (g == null) continue;
                string missing = ModReflection.Member(g, "missing_asset") as string;
                IDictionary containers = ModReflection.Member(g, "containers") as IDictionary;
                if (containers == null) continue;
                foreach (DictionaryEntry kv in containers)
                {
                    if (kv.Key == null) continue;
                    var buf = new StringWriter();
                    try
                    {
                        WriteContainer(JsonWriter.Fragment(buf), kv.Key, kv.Value as Exception, missing, meshes, effectCounts);
                        w.NewLine();
                        w.BeginObject().RawMembers(buf.ToString()).EndObject();
                        written++;
                    }
                    catch (Exception)
                    {
                        errors++;
                    }
                }
            }

            note = written + " failed containers written from PopupStart.loading_failures (" + groups.Count + " groups)" + (errors > 0 ? ", " + errors + " could not be written" : "");
            return written;
        }

        private static void WriteContainer(JsonWriter w, object c, Exception ex, string missing, PoMeshBin meshes, Dictionary<string, int> effectCounts)
        {
            int id = ToInt(ModReflection.Member(c, "id"));
            Vector3 pos = V3(ModReflection.Member(c, "position"));
            Quaternion rot = Q(ModReflection.Member(c, "rotation"));
            object scaleObj = ModReflection.Member(c, "scale");
            float scale = scaleObj is float ? (float)scaleObj : 0f;

            w.Prop("id", id);
            w.Prop("missingAsset", missing);
            w.Prop("exception", ex != null ? ex.GetType().Name + ": " + ex.Message : null);
            w.Prop("objectType", ModReflection.Member(c, "objectType") as string);
            w.Prop("basePrefab", ModReflection.Member(c, "basePrefabName") as string);
            w.Prop("position", pos);
            w.Name("rotation").BeginArray().Value(rot.x).Value(rot.y).Value(rot.z).Value(rot.w).EndArray();
            w.Prop("yawDegrees", rot.eulerAngles.y);
            w.Prop("scale", scale);
            object ms = ModReflection.Member(c, "meshStatus");
            w.Prop("meshStatus", ms is byte ? (int)(byte)ms : -1);
            w.Prop("tilingFactor", ToInt(ModReflection.Member(c, "tilingFactor")));
            w.Prop("customTextureName", ModReflection.Member(c, "customTextureName") as string);
            w.Prop("hasCustomTexture", ToBool(ModReflection.Member(c, "hasCustomTexture")));
            object rd = ModReflection.Member(c, "renderDistance");
            w.Name("renderDistance"); if (rd is float) w.Value((float)rd); else w.Null();
            w.Prop("renderDistLocked", ToBool(ModReflection.Member(c, "renderDistLocked")));
            object layerId = ModReflection.Member(c, "layerId");
            w.Prop("layerId", layerId is uint ? (long)(uint)layerId : -1L);
            w.Prop("groupRootId", ToInt(ModReflection.Member(c, "groupRootId")));
            w.Prop("belongsToGroup", ToBool(ModReflection.Member(c, "belongsToGroup")));
            object col = ModReflection.Member(c, "color");
            w.Name("color");
            if (col != null) w.BeginArray().Value(F(col, "r")).Value(F(col, "g")).Value(F(col, "b")).Value(F(col, "a")).EndArray();
            else w.Null();
            object vis = ModReflection.Member(c, "visibility");
            w.Prop("visibility", vis != null ? vis.ToString() : null);
            w.Prop("flipFaces", ToBool(ModReflection.Member(c, "flipFaces")));
            w.Prop("disableCastShadows", ToBool(ModReflection.Member(c, "disableCastShadows")));
            w.Prop("hasText", ModReflection.Member(c, "textParam") != null);
            IList modulesData = ModReflection.Member(c, "modulesData") as IList;
            w.Name("modulesData");
            GenericDump.Write(w, modulesData, 3);
            // Effects from the saved module data (same keys as EffectsModule.GetData), placed in world space.
            w.Name("effects").BeginArray();
            if (modulesData != null)
            {
                foreach (object md in modulesData)
                {
                    IDictionary d = md as IDictionary;
                    if (d == null || !d.Contains("effectName")) continue;
                    PoModules.WriteEffectFromData(w, d, pos, rot, effectCounts);
                }
            }

            w.EndArray();

            // Full container vertices (older save format): local, scaled like PO does on load, then to world.
            IList verts = ModReflection.Member(c, "vertices") as IList;
            w.Prop("containerVertexCount", verts != null ? verts.Count : 0);
            if (verts != null && verts.Count > 0)
            {
                var world = new Vector3[verts.Count];
                for (int i = 0; i < verts.Count; i++)
                {
                    Vector3 v = V3(verts[i]);
                    if (scale != 0f) v = v * scale;
                    world[i] = pos + rot * v;
                }

                PoMeshBin.Entry e = meshes.Add(id, 1, world, null, null);
                w.Name("meshBin").BeginObject().Prop("offset", e.Offset).Prop("vertexCount", e.VertexCount)
                 .Prop("triangleIndexCount", 0).Prop("uvCount", 0).EndObject();
            }
            else
            {
                w.Name("meshBin").Null();
            }

            // Newer format: only moved vertices, as local position -> indices into the base prefab mesh.
            object smd = ModReflection.Member(c, "serializedMeshData");
            IDictionary data = smd != null ? ModReflection.Member(smd, "data") as IDictionary : null;
            w.Name("meshDelta");
            if (data == null)
            {
                w.Null();
            }
            else
            {
                w.BeginObject();
                w.Prop("note", "PO stores moved vertices only: local (object-space, unscaled) position -> indices into the base prefab mesh; base mesh needed to rebuild");
                w.Name("positions").BeginArray();
                foreach (DictionaryEntry kv in data)
                {
                    Vector3 v = V3(kv.Key);
                    w.Value(v.x); w.Value(v.y); w.Value(v.z);
                }

                w.EndArray();
                w.Name("indices").BeginArray();
                foreach (DictionaryEntry kv in data)
                {
                    w.BeginArray();
                    IList idx = kv.Value as IList;
                    if (idx != null) foreach (object i in idx) w.Value(ToInt(i));
                    w.EndArray();
                }

                w.EndArray();
                w.EndObject();
            }
        }

        private static float F(object o, string name)
        {
            object v = ModReflection.Member(o, name);
            return v is float ? (float)v : 0f;
        }

        private static Vector3 V3(object o)
        {
            if (o == null) return Vector3.zero;
            return new Vector3(F(o, "x"), F(o, "y"), F(o, "z"));
        }

        private static Quaternion Q(object o)
        {
            if (o == null) return Quaternion.identity;
            return new Quaternion(F(o, "x"), F(o, "y"), F(o, "z"), F(o, "w"));
        }

        private static int ToInt(object o)
        {
            if (o is int) return (int)o;
            if (o is uint) return (int)(uint)o;
            if (o is byte) return (byte)o;
            if (o is short) return (short)o;
            if (o is ushort) return (ushort)o;
            return -1;
        }

        private static bool ToBool(object o)
        {
            return o is bool && (bool)o;
        }
    }
}
