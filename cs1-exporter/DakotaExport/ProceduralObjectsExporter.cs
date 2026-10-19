using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Procedural Objects (Workshop 3071492847) by reflection, no hard reference. Field names from
    /// ProceduralObjects.Classes.ProceduralObject: id, baseInfoType, basePrefabName, m_position,
    /// m_rotation, m_scale, meshStatus, m_mesh, customTexture, renderDistance, renderDistLocked,
    /// m_visibility, layer (Layer: m_name, m_id, m_isHidden), group (POGroup), isRootOfGroup, m_color,
    /// tilingFactor, isPloppableAsphalt, m_textParameters, m_modules, disableCastShadows, flipFaces.
    /// Writes the sibling file and the "proceduralObjects" summary in the main JSON.
    /// </summary>
    internal static class ProceduralObjectsExporter
    {
        public const string Schema = "dakota-export/cs1/po/v3";
        public const int MaxDumpVertices = 2000;

        public static void Write(JsonWriter main, string poPath, string heightsPngPath, string heightsJsonPath, string meshesPath, ExportLog log, Dictionary<string, int> stats)
        {
            string reason;
            IList objects;
            IList groups;
            object layerManager;
            Type logicType;
            if (!TryGetLogic(out logicType, out objects, out groups, out layerManager, out reason))
            {
                main.Name("proceduralObjects").Null();
                main.Prop("proceduralObjectsReason", reason);
                stats["proceduralObjects"] = 0;
                log.Info("procedural objects: none (" + reason + ")");
                return;
            }

            var groupIndex = new Dictionary<object, int>();
            if (groups != null)
                for (int g = 0; g < groups.Count; g++)
                    if (groups[g] != null && !groupIndex.ContainsKey(groups[g])) groupIndex[groups[g]] = g;

            var byPrefab = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var byType = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var layers = new SortedDictionary<long, string>();
            int count = 0, edited = 0, failed = 0;
            string firstError = null;
            var raster = new PoHeightRaster();
            var effectCountsD = new Dictionary<string, int>();
            int geomOk = 0, geomFailed = 0, skippedQuads = 0, effectsCount = 0, dumpedVertices = 0;
            string firstGeomError = null;
            Type effectsType = PoModules.EffectsType();
            string effectsReason = effectsType == null
                ? "Effects Module not loaded (type " + PoModules.EffectsTypeName + " not found)"
                : null;

            object logicObj = ModReflection.StaticField(logicType, "instance");
            object failedCountObj = ModReflection.Member(logicObj, "failedToLoadObjects");
            long poLoadFailures = failedCountObj is uint ? (long)(uint)failedCountObj : -1L;
            var meshes = new PoMeshBin(meshesPath);
            int failedWritten = 0;
            string failedReason = null;
            try
            {
            using (var stream = SafeFile.CreateText(poPath))
            {
                var w = new JsonWriter(stream);
                w.BeginObject();
                w.Prop("schema", Schema);
                w.Prop("assembly", logicType.Assembly.GetName().Name + " " + logicType.Assembly.GetName().Version);
                w.Prop("coordinates", "CS1 world metres, x east, y up, z north, origin map centre; rotation quaternion [x,y,z,w]; yaw = rotation.eulerAngles.y degrees");
                w.Prop("meshStatusLegend", "1 = base prefab mesh unchanged, 2 = mesh edited/custom (vertex data stored by PO), other = unknown");
                w.Prop("geometryNote", "geometry.vertices: world-space [x,y,z,...] rounded to 0.01 m = m_position + m_rotation * mesh vertex; decimated to at most " + MaxDumpVertices + " vertices by keeping every triangleStride-th triangle; triangles index into the dumped vertices");
                w.Name("effectsModule").BeginObject();
                w.Prop("loaded", effectsType != null);
                w.Prop("assembly", effectsType != null ? effectsType.Assembly.GetName().Name + " " + effectsType.Assembly.GetName().Version : null);
                w.Prop("reason", effectsReason);
                w.EndObject();
                w.NewLine();
                w.Name("objects").BeginArray();
                foreach (object po in objects)
                {
                    if (po == null) continue;
                    try
                    {
                        // Build each object into a buffer so a failure never leaves half an object in the file.
                        var buf = new StringWriter();
                        var ow = JsonWriter.Fragment(buf);
                        string prefab, type;
                        bool isEdited;
                        long layerId;
                        string layerName;
                        WriteObject(ow, po, groupIndex, out prefab, out type, out isEdited, out layerId, out layerName);
                        object posObj = ModReflection.Member(po, "m_position");
                        object rotObj = ModReflection.Member(po, "m_rotation");
                        Vector3 pos = posObj is Vector3 ? (Vector3)posObj : Vector3.zero;
                        Quaternion rot = rotObj is Quaternion ? (Quaternion)rotObj : Quaternion.identity;
                        int objectId = ToInt(ModReflection.Member(po, "id"));

                        // Geometry: own buffer so a failure leaves a clean {"error": ...}.
                        var gbuf = new StringWriter();
                        try
                        {
                            var gw = JsonWriter.Fragment(gbuf);
                            string readNote;
                            PoMesh mesh = PoMesh.Read(po, pos, rot, out readNote);
                            if (mesh == null) throw new InvalidOperationException(readNote);
                            bool quad = mesh.Triangles != null && mesh.TriangleCount <= 2;
                            mesh.WriteDump(gw, MaxDumpVertices);
                            gw.Prop("readNote", readNote);
                            gw.Prop("rasterized", !quad);
                            gw.Prop("skipReason", quad ? "decal/texture-only quad (<= 2 triangles)" : null);
                            if (quad) skippedQuads++;
                            else raster.Add(objectId, mesh);
                            if (isEdited)
                            {
                                PoMeshBin.Entry e = meshes.Add(objectId, 0, mesh.World, mesh.Triangles, mesh.Uv);
                                gw.Name("meshBin").BeginObject().Prop("offset", e.Offset).Prop("vertexCount", e.VertexCount)
                                  .Prop("triangleIndexCount", e.TriangleIndexCount).Prop("uvCount", e.UvCount).EndObject();
                            }
                            dumpedVertices += Math.Min(mesh.World.Length, MaxDumpVertices);
                            ow.NewLine();
                            ow.Name("geometry").BeginObject().RawMembers(gbuf.ToString()).EndObject();
                            geomOk++;
                        }
                        catch (Exception gex)
                        {
                            geomFailed++;
                            if (firstGeomError == null) firstGeomError = gex.GetType().Name + ": " + gex.Message;
                            ow.Name("geometry").BeginObject().Prop("error", gex.GetType().Name + ": " + gex.Message).EndObject();
                        }

                        var mbuf = new StringWriter();
                        try
                        {
                            effectsCount += PoModules.WriteModulesAndEffects(JsonWriter.Fragment(mbuf), po, pos, rot, effectsType, effectsReason, effectCountsD);
                            ow.RawMembers(mbuf.ToString());
                        }
                        catch (Exception mex)
                        {
                            ow.Name("modules").Null();
                            ow.Name("effects").Null();
                            ow.Prop("effectsReason", "read failed: " + mex.GetType().Name + ": " + mex.Message);
                        }

                        w.NewLine();
                        w.BeginObject().RawMembers(buf.ToString()).EndObject();

                        count++;
                        if (isEdited) edited++;
                        Inc(byPrefab, (type ?? "?") + ":" + (prefab ?? "<null>"));
                        Inc(byType, type ?? "?");
                        if (layerName != null || layerId >= 0) layers[layerId] = layerName;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        if (firstError == null) firstError = ex.GetType().Name + ": " + ex.Message;
                    }
                }

                w.EndArray();
                w.NewLine();
                w.Prop("poLoadFailures", poLoadFailures);
                w.Name("failedObjects").BeginArray();
                failedWritten = PoFailures.Write(w, meshes, effectCountsD, out failedReason);
                w.EndArray();
                w.Prop("failedObjectsNote", failedReason);
                w.NewLine();
                w.Name("meshesFile").BeginObject().Prop("file", Path.GetFileName(meshesPath)).Prop("format", PoMeshBin.FormatNote).Prop("entries", meshes.Count).EndObject();
                w.NewLine();
                w.Name("groups").BeginArray();
                if (groups != null)
                {
                    for (int g = 0; g < groups.Count; g++)
                    {
                        object grp = groups[g];
                        if (grp == null) continue;
                        IList members = ModReflection.Member(grp, "objects") as IList;
                        object root = ModReflection.Member(grp, "root");
                        w.BeginObject();
                        w.Prop("index", g);
                        w.Name("rootId"); if (root != null) w.Value(ToInt(ModReflection.Member(root, "id"))); else w.Null();
                        w.Prop("memberCount", members != null ? members.Count : 0);
                        w.EndObject();
                    }
                }

                w.EndArray();
                w.NewLine();
                w.Name("layers").BeginArray();
                foreach (var kv in layers)
                {
                    w.BeginObject().Prop("id", kv.Key).Prop("name", kv.Value).EndObject();
                }

                w.EndArray();
                w.Prop("count", count);
                w.Prop("failed", failed);
                w.Prop("firstError", firstError);
                w.EndObject();
                stream.Write('\n');
                stream.Commit();
                meshes.Commit();
            }
            }
            finally
            {
                meshes.Close();
            }

            string heightsError = null;
            string pngMethod = null;
            try
            {
                pngMethod = raster.WritePng(heightsPngPath);
                raster.WriteJson(heightsJsonPath, Path.GetFileName(heightsPngPath), skippedQuads);
            }
            catch (Exception hex)
            {
                heightsError = hex.GetType().Name + ": " + hex.Message;
                log.Info("po heights FAILED: " + hex);
            }

            main.Name("proceduralObjects").BeginObject();
            main.Prop("file", Path.GetFileName(poPath));
            main.Prop("schema", Schema);
            main.Prop("count", count);
            main.Prop("edited", edited);
            main.Prop("failed", failed);
            main.Prop("poLoadFailures", poLoadFailures);
            main.Prop("failedObjectsWritten", failedWritten);
            main.Prop("failedObjectsNote", failedReason);
            main.Prop("meshesFile", Path.GetFileName(meshesPath));
            main.Prop("meshEntries", meshes.Count);
            main.Prop("firstError", firstError);
            main.Prop("groups", groups != null ? groups.Count : 0);
            main.Name("geometry").BeginObject();
            main.Prop("objectsWithGeometry", geomOk);
            main.Prop("geometryFailed", geomFailed);
            main.Prop("firstGeometryError", firstGeomError);
            main.Prop("skippedQuadObjects", skippedQuads);
            main.Prop("rasterizedObjects", raster.Objects);
            main.Prop("vertexOnlyObjects", raster.VertexOnlyObjects);
            main.Prop("rasterizedTriangles", raster.Triangles);
            main.Prop("dumpedVertices", dumpedVertices);
            main.Prop("heightsPng", heightsError == null ? Path.GetFileName(heightsPngPath) : null);
            main.Prop("heightsPngCompression", pngMethod);
            main.Prop("heightsJson", heightsError == null ? Path.GetFileName(heightsJsonPath) : null);
            main.Prop("heightsError", heightsError);
            main.Prop("coveredCells", raster.CoveredCells());
            main.EndObject();
            main.Name("effectsModule").BeginObject();
            main.Prop("loaded", effectsType != null);
            main.Prop("reason", effectsReason);
            main.Prop("effects", effectsCount);
            main.EndObject();
            main.Name("byEffect").BeginObject();
            foreach (var kv in effectCountsD) main.Prop(kv.Key, kv.Value);
            main.EndObject();
            main.Name("byType").BeginObject();
            foreach (var kv in byType) main.Prop(kv.Key, kv.Value);
            main.EndObject();
            main.Name("byPrefab").BeginObject();
            foreach (var kv in byPrefab) main.Prop(kv.Key, kv.Value);
            main.EndObject();
            main.EndObject();

            stats["proceduralObjects"] = count;
            stats["proceduralObjects.edited"] = edited;
            stats["proceduralObjects.failed"] = failed;
            stats["proceduralObjects.loadFailures"] = (int)poLoadFailures;
            stats["proceduralObjects.failedObjectsWritten"] = failedWritten;
            stats["proceduralObjects.meshEntries"] = meshes.Count;
            stats["proceduralObjects.geometry"] = geomOk;
            stats["proceduralObjects.geometryFailed"] = geomFailed;
            stats["proceduralObjects.skippedQuads"] = skippedQuads;
            stats["proceduralObjects.effects"] = effectsCount;
            foreach (var kv in effectCountsD) stats["proceduralObjects.effect." + kv.Key] = kv.Value;
            stats["proceduralObjects.heightCells"] = raster.CoveredCells();
            foreach (var kv in byPrefab) stats["proceduralObjects.prefab." + kv.Key] = kv.Value;
            log.Info("procedural objects: " + count + " (" + edited + " edited, " + failed + " failed" +
                     (firstError != null ? ", first error " + firstError : "") + ") in " + byPrefab.Count + " base prefabs; geometry " + geomOk + " ok, " + geomFailed +
                     " failed, " + skippedQuads + " quads skipped, " + raster.Triangles + " triangles rasterized, " +
                     raster.CoveredCells() + " height cells; effects " + effectsCount + (effectsReason != null ? " (" + effectsReason + ")" : ""));
        }

        private static bool TryGetLogic(out Type logicType, out IList objects, out IList groups, out object layerManager, out string reason)
        {
            objects = null;
            groups = null;
            layerManager = null;
            logicType = ModReflection.FindType("ProceduralObjects.ProceduralObjectsLogic");
            if (logicType == null)
            {
                reason = "ProceduralObjects assembly not loaded (type ProceduralObjects.ProceduralObjectsLogic not found)";
                return false;
            }

            object logic = ModReflection.StaticField(logicType, "instance");
            if (logic == null)
            {
                // Fallback: the MonoBehaviour may exist without the static being set.
                UnityEngine.Object found = UnityEngine.Object.FindObjectOfType(logicType);
                logic = found;
            }

            if (logic == null)
            {
                reason = "ProceduralObjects is loaded but ProceduralObjectsLogic has no instance (mod disabled or not active in this load mode)";
                return false;
            }

            objects = ModReflection.Member(logic, "proceduralObjects") as IList;
            groups = ModReflection.Member(logic, "groups") as IList;
            layerManager = ModReflection.Member(logic, "layerManager");
            if (objects == null)
            {
                reason = "ProceduralObjectsLogic.proceduralObjects is null";
                return false;
            }

            reason = null;
            return true;
        }

        private static void WriteObject(JsonWriter w, object po, Dictionary<object, int> groupIndex,
            out string prefab, out string type, out bool isEdited, out long layerId, out string layerName)
        {
            prefab = ModReflection.Member(po, "basePrefabName") as string;
            type = ModReflection.Member(po, "baseInfoType") as string;
            w.Prop("id", ToInt(ModReflection.Member(po, "id")));
            w.Prop("baseInfoType", type);
            w.Prop("basePrefab", prefab);

            object posObj = ModReflection.Member(po, "m_position");
            object rotObj = ModReflection.Member(po, "m_rotation");
            Vector3 pos = posObj is Vector3 ? (Vector3)posObj : Vector3.zero;
            Quaternion rot = rotObj is Quaternion ? (Quaternion)rotObj : Quaternion.identity;
            w.Name("position"); if (posObj is Vector3) w.Value(pos); else w.Null();
            w.Name("rotation");
            if (rotObj is Quaternion) w.BeginArray().Value(rot.x).Value(rot.y).Value(rot.z).Value(rot.w).EndArray();
            else w.Null();
            w.Name("yawDegrees"); if (rotObj is Quaternion) w.Value(rot.eulerAngles.y); else w.Null();
            w.Name("eulerDegrees"); if (rotObj is Quaternion) w.Value(rot.eulerAngles); else w.Null();

            object scale = ModReflection.Member(po, "m_scale");
            w.Name("scale"); if (scale is float) w.Value((float)scale); else w.Null();

            object status = ModReflection.Member(po, "meshStatus");
            int meshStatus = status is byte ? (byte)status : -1;
            isEdited = meshStatus == 2;
            w.Prop("meshStatus", meshStatus);
            w.Prop("meshEdited", isEdited);
            Mesh mesh = ModReflection.Member(po, "m_mesh") as Mesh;
            if (mesh != null)
            {
                w.Prop("vertexCount", mesh.vertexCount);
                Bounds b = mesh.bounds;
                w.Name("localBounds").BeginObject().Prop("center", b.center).Prop("size", b.size).EndObject();
            }
            else
            {
                w.Name("vertexCount").Null();
                w.Name("localBounds").Null();
            }

            Texture tex = ModReflection.Member(po, "customTexture") as Texture;
            w.Prop("customTexture", tex != null ? tex.name : null);
            object dist = ModReflection.Member(po, "renderDistance");
            w.Name("renderDistance"); if (dist is float) w.Value((float)dist); else w.Null();
            w.Prop("renderDistLocked", ToBool(ModReflection.Member(po, "renderDistLocked")));
            object vis = ModReflection.Member(po, "m_visibility");
            w.Prop("visibility", vis != null ? vis.ToString() : null);

            object layer = ModReflection.Member(po, "layer");
            layerId = -1;
            layerName = null;
            if (layer != null)
            {
                layerName = ModReflection.Member(layer, "m_name") as string;
                object lid = ModReflection.Member(layer, "m_id");
                layerId = lid is uint ? (long)(uint)lid : -1L;
                w.Name("layer").BeginObject()
                 .Prop("id", layerId)
                 .Prop("name", layerName)
                 .Prop("hidden", ToBool(ModReflection.Member(layer, "m_isHidden")))
                 .EndObject();
            }
            else
            {
                w.Name("layer").Null();
            }

            object group = ModReflection.Member(po, "group");
            int gi;
            w.Name("group"); if (group != null && groupIndex.TryGetValue(group, out gi)) w.Value(gi); else w.Null();
            w.Prop("isRootOfGroup", ToBool(ModReflection.Member(po, "isRootOfGroup")));

            object color = ModReflection.Member(po, "m_color");
            w.Name("color");
            if (color is Color) { Color c = (Color)color; w.BeginArray().Value(c.r).Value(c.g).Value(c.b).Value(c.a).EndArray(); }
            else w.Null();
            w.Prop("tilingFactor", ToInt(ModReflection.Member(po, "tilingFactor")));
            w.Prop("ploppableAsphalt", ToBool(ModReflection.Member(po, "isPloppableAsphalt")));
            w.Prop("hasText", ModReflection.Member(po, "m_textParameters") != null);
            IList modules = ModReflection.Member(po, "m_modules") as IList;
            w.Prop("moduleCount", modules != null ? modules.Count : 0);
            w.Prop("disableCastShadows", ToBool(ModReflection.Member(po, "disableCastShadows")));
            w.Prop("flipFaces", ToBool(ModReflection.Member(po, "flipFaces")));
        }

        private static void Inc(SortedDictionary<string, int> d, string key)
        {
            int c;
            d.TryGetValue(key, out c);
            d[key] = c + 1;
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
