using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using ColossalFramework;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Reads the managers' buffers and writes one JSON, one SVG and one log file.
    /// Everything here is read-only: buffers are copied or iterated, nothing is assigned back.
    /// </summary>
    internal static class MapExporter
    {
        public const string Schema = "dakota-export/cs1/v5";

        /// <summary>CS1 map half extent in metres (81 tiles, 17 280 m, origin at centre).</summary>
        public const float HalfExtent = 8640f;
        public const float Extent = 17280f;

        /// <summary>Tree/prop raster resolution. 270 cells = 64 m; set to 1081 for the heightmap grid.</summary>
        public const int RasterSize = 270;
        public const int TotalTreeRasterSize = 1081;

        /// <summary>Props are written as points only below this count; otherwise as rasters.</summary>
        public const int PropPointLimit = int.MaxValue;

        public static void Run(string loadMode)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DakotaExport");
            Directory.CreateDirectory(dir);

            string mapName = ResolveMapName();
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string baseName = Sanitize(mapName) + "-" + stamp;
            string jsonPath = Path.Combine(dir, baseName + ".json");
            string svgPath = Path.Combine(dir, baseName + ".svg");
            string logPath = Path.Combine(dir, baseName + ".log");

            using (ExportLog log = new ExportLog(logPath))
            {
                log.Info("export start: map '" + mapName + "', load mode " + loadMode + ", version " + DakotaExportMod.Version);
                var stats = new Dictionary<string, int>();
                var svg = new SvgSink();

                string treesPath = Path.Combine(dir, baseName + "-trees.json");
                string waterPngPath = Path.Combine(dir, baseName + "-water.png");
                string surfacesPngPath = Path.Combine(dir, baseName + "-surfaces.png");
                string poPath = Path.Combine(dir, baseName + "-po.json");
                string poHeightsPngPath = Path.Combine(dir, baseName + "-po-heights.png");
                string poHeightsJsonPath = Path.Combine(dir, baseName + "-po-heights.json");
                string poMeshesPath = Path.Combine(dir, baseName + "-po-meshes.bin");
                var errors = new Dictionary<string, string>();
                var total = Stopwatch.StartNew();
                using (var stream = SafeFile.CreateText(jsonPath))
                {
                    var w = new JsonWriter(stream);
                    w.BeginObject();
                    w.Prop("schema", Schema);
                    Section(w, log, errors, "metadata", new[] { "metadata" }, s => WriteMetadata(s, mapName, loadMode));
                    Section(w, log, errors, "mapSettings", new[] { "mapSettings" }, s => WriteMapSettings(s, log));
                    Section(w, log, errors, "networks", new[] { "nodes", "segments", "netPrefabs" }, s => WriteNetworks(s, log, stats, svg));
                    Section(w, log, errors, "waterSources", new[] { "waterSources" }, s => WriteWaterSources(s, log, stats, svg));
                    Section(w, log, errors, "buildings", new[] { "buildings", "skippedBuildings" }, s => WriteBuildings(s, log, stats, svg));
                    Section(w, log, errors, "trees", new[] { "trees" }, s => WriteTrees(s, log, stats));
                    Section(w, log, errors, "props", new[] { "props" }, s => WriteProps(s, log, stats));
                    Section(w, log, errors, "naturalResources", new[] { "naturalResources" }, s => WriteNaturalResources(s, log, stats));
                    Section(w, log, errors, "treePoints", new string[0], s => TreePointsWriter.Write(treesPath, log, stats));
                    Section(w, log, errors, "waterSurface", new[] { "waterSurface" }, s => WaterSurfaceExporter.Write(s, waterPngPath, log, stats));
                    Section(w, log, errors, "terrainSurfaces", new[] { "terrainSurfaces" }, s => TerrainSurfaceExporter.Write(s, surfacesPngPath, log, stats));
                    Section(w, log, errors, "proceduralObjects", new[] { "proceduralObjects" }, s => ProceduralObjectsExporter.Write(s, poPath, poHeightsPngPath, poHeightsJsonPath, poMeshesPath, log, stats));
                    Section(w, log, errors, "terrainHeights", new[] { "terrainHeights" }, s => ExtraExporters.WriteTerrainHeights(s, dir, baseName, log, stats));
                    Section(w, log, errors, "networkSkins", new[] { "networkSkins" }, s => ExtraExporters.WriteNetworkSkins(s, log, stats));
                    Section(w, log, errors, "serializableData", new[] { "serializableData" }, s => ExtraExporters.WriteSerializableData(s, dir, baseName, log, stats));
                    w.NewLine();
                    w.Name("sectionErrors").BeginObject();
                    foreach (var kv in errors) w.Prop(kv.Key, kv.Value);
                    w.EndObject();
                    w.NewLine();
                    w.Name("stats").BeginObject();
                    foreach (var kv in stats) w.Prop(kv.Key, kv.Value);
                    w.EndObject();
                    w.EndObject();
                    w.NewLine();
                    stream.Commit();
                }

                LogSize(log, "json", jsonPath);
                LogSize(log, "tree points", treesPath);
                LogSize(log, "water png", waterPngPath);
                LogSize(log, "surfaces png", surfacesPngPath);
                LogSize(log, "procedural objects", poPath);
                LogSize(log, "po heights png", poHeightsPngPath);
                LogSize(log, "po heights json", poHeightsJsonPath);
                LogSize(log, "po meshes", poMeshesPath);
                foreach (string h in ExtraExporters.HeightNames) LogSize(log, "heights " + h, Path.Combine(dir, baseName + "-heights-" + h + ".png"));
                try
                {
                    Timed(log, "svg", () => svg.Write(svgPath, mapName));
                    LogSize(log, "svg", svgPath);
                }
                catch (Exception ex)
                {
                    log.Info("section svg FAILED: " + ex);
                    errors["svg"] = ex.Message;
                }

                log.Info("total time: " + total.ElapsedMilliseconds + " ms; failed sections: " +
                         (errors.Count == 0 ? "none" : string.Join(", ", new List<string>(errors.Keys).ToArray())));
                log.Info("export complete: " + jsonPath);
            }
        }

        /// <summary>
        /// Runs one section into a buffer and splices it into the main object. On an exception the
        /// section's keys are written as null, the error is logged and recorded, and the export continues.
        /// </summary>
        private static void Section(JsonWriter w, ExportLog log, Dictionary<string, string> errors, string name,
            string[] keys, Action<JsonWriter> body)
        {
            var sw = Stopwatch.StartNew();
            var buf = new StringWriter(CultureInfo.InvariantCulture);
            try
            {
                body(JsonWriter.Fragment(buf));
                w.NewLine();
                w.RawMembers(buf.ToString());
                log.Info("section " + name + ": " + sw.ElapsedMilliseconds + " ms");
            }
            catch (Exception ex)
            {
                errors[name] = ex.GetType().Name + ": " + ex.Message;
                log.Info("section " + name + " FAILED after " + sw.ElapsedMilliseconds + " ms: " + ex);
                foreach (string key in keys)
                {
                    w.NewLine();
                    w.Name(key).Null();
                }
            }
        }

        private static void LogSize(ExportLog log, string what, string path)
        {
            try
            {
                if (File.Exists(path)) log.Info(what + " written: " + path + " (" + new FileInfo(path).Length + " bytes)");
                else log.Info(what + ": no file");
            }
            catch (Exception ex)
            {
                log.Info(what + ": size unavailable (" + ex.Message + ")");
            }
        }

        // ---------------------------------------------------------------- metadata

        private static void WriteMetadata(JsonWriter w, string mapName, string loadMode)
        {
            w.Name("metadata").BeginObject();
            w.Prop("mapName", mapName);
            w.Prop("loadMode", loadMode);
            w.Prop("exporterVersion", DakotaExportMod.Version);
            w.Prop("gameVersion", SafeGameVersion());
            w.Prop("exportedAtUtc", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            SimulationMetaData meta = Singleton<SimulationManager>.instance.m_metaData;
            if (meta != null)
            {
                w.Prop("metaMapName", meta.m_MapName);
                w.Prop("metaCityName", meta.m_CityName);
                w.Prop("environment", meta.m_environment);
            }

            w.Name("coordinates").BeginObject();
            w.Prop("units", "metres");
            w.Prop("axes", "x east, y up, z north (CS1 world space)");
            w.Prop("origin", "map centre");
            w.Prop("halfExtent", HalfExtent);
            w.Prop("extent", Extent);
            w.Prop("heightmapPixels", 1081);
            w.Prop("heightmapRawToMetres", 1f / 64f);
            w.Prop("seaLevel", SafeSeaLevel());
            w.EndObject();
            w.EndObject();
        }

        private static string SafeGameVersion()
        {
            try { return BuildConfig.applicationVersionFull; }
            catch { return Application.version; }
        }

        private static float SafeSeaLevel()
        {
            try { return Singleton<TerrainManager>.instance.WaterSimulation.m_currentSeaLevel; }
            catch { return float.NaN; }
        }

        private static string ResolveMapName()
        {
            SimulationMetaData meta = Singleton<SimulationManager>.instance.m_metaData;
            if (meta != null)
            {
                if (!string.IsNullOrEmpty(meta.m_MapName)) return meta.m_MapName;
                if (!string.IsNullOrEmpty(meta.m_CityName)) return meta.m_CityName;
                if (!string.IsNullOrEmpty(meta.m_mapEditorMapPackage)) return meta.m_mapEditorMapPackage;
            }

            // The editor leaves the names above null. An optional override file names the export;
            // otherwise use the Workshop id of the loaded map if known.
            try
            {
                string overridePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DakotaExport"), "mapname.txt");
                if (File.Exists(overridePath))
                {
                    string name = File.ReadAllText(overridePath).Trim();
                    if (name.Length > 0) return name;
                }
            }
            catch
            {
            }

            try
            {
                if (meta != null && meta.m_WorkshopPublishedFileId != ColossalFramework.PlatformServices.PublishedFileId.invalid)
                    return "workshop-" + meta.m_WorkshopPublishedFileId.AsUInt64;
            }
            catch
            {
            }

            return "map";
        }

        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
                sb.Append(ok ? c : '_');
            }

            return sb.Length == 0 ? "map" : sb.ToString();
        }

        private static int CountOwned(List<int> ids, Dictionary<int, int> owner)
        {
            int n = 0;
            foreach (int id in ids) if (owner.ContainsKey(id)) n++;
            return n;
        }

        /// <summary>Every NetInfo-typed field on the prefab's AI (m_elevatedInfo, m_bridgeInfo, m_slopeInfo, m_tunnelInfo, ...).</summary>
        private static void WriteVariants(JsonWriter w, NetInfo info)
        {
            w.Name("variants").BeginObject();
            if (info.m_netAI != null)
            {
                foreach (System.Reflection.FieldInfo f in info.m_netAI.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                {
                    if (f.FieldType != typeof(NetInfo)) continue;
                    NetInfo v = f.GetValue(info.m_netAI) as NetInfo;
                    w.Prop(f.Name, v != null ? v.name : null);
                }
            }

            w.EndObject();
        }

        private static void WriteLaneProps(JsonWriter w, NetInfo.Lane lane)
        {
            w.Name("props").BeginArray();
            if (lane.m_laneProps != null && lane.m_laneProps.m_props != null)
            {
                foreach (NetLaneProps.Prop p in lane.m_laneProps.m_props)
                {
                    if (p == null) continue;
                    w.BeginObject();
                    w.Prop("prop", p.m_prop != null ? p.m_prop.name : null);
                    w.Prop("tree", p.m_tree != null ? p.m_tree.name : null);
                    w.Prop("position", p.m_position);
                    w.Prop("angle", p.m_angle);
                    w.Prop("segmentOffset", p.m_segmentOffset);
                    w.Prop("repeatDistance", p.m_repeatDistance);
                    w.Prop("probability", p.m_probability);
                    w.EndObject();
                }
            }

            w.EndArray();
        }

        // ---------------------------------------------------------------- networks

        private static void WriteNetworks(JsonWriter w, ExportLog log, Dictionary<string, int> stats, SvgSink svg)
        {
            NetManager nm = Singleton<NetManager>.instance;
            TerrainManager tm = Singleton<TerrainManager>.instance;
            NetNode[] nodes = nm.m_nodes.m_buffer;
            NetSegment[] segments = nm.m_segments.m_buffer;
            var prefabs = new Dictionary<string, NetInfo>();
            var prefabUse = new Dictionary<string, int>();
            var usedNodes = new List<ushort>();

            for (int i = 1; i < nodes.Length; i++)
            {
                if ((nodes[i].m_flags & NetNode.Flags.Created) == 0 || (nodes[i].m_flags & NetNode.Flags.Deleted) != 0) continue;
                usedNodes.Add((ushort)i);
            }

            w.Name("nodes").BeginArray();
            foreach (ushort id in usedNodes)
            {
                NetNode n = nodes[id];
                NetInfo info = n.Info;
                w.NewLine();
                w.BeginObject();
                w.Prop("id", (int)id);
                w.Prop("prefab", info != null ? info.name : null);
                w.Prop("position", n.m_position);
                w.Prop("elevation", (int)n.m_elevation);
                w.Prop("terrainOriginal", tm.SampleOriginalRawHeightSmooth(n.m_position));
                w.Prop("terrain", tm.SampleRawHeightSmooth(n.m_position));
                w.Prop("flagsRaw", (int)n.m_flags);
                w.StringArray("flags", NodeFlagNames(n.m_flags));
                w.Prop("building", (int)n.m_building);
                w.EndObject();
                if (info != null) prefabs[info.name] = info;
                if ((n.m_flags & NetNode.Flags.Outside) != 0) svg.OutsideNode(n.m_position);
            }

            w.EndArray();
            stats["nodes"] = usedNodes.Count;
            log.Info("nodes: " + usedNodes.Count);

            int segCount = 0;
            w.NewLine();
            w.Name("segments").BeginArray();
            for (int i = 1; i < segments.Length; i++)
            {
                NetSegment s = segments[i];
                if ((s.m_flags & NetSegment.Flags.Created) == 0 || (s.m_flags & NetSegment.Flags.Deleted) != 0) continue;
                NetInfo info = s.Info;
                NetNode a = nodes[s.m_startNode];
                NetNode d = nodes[s.m_endNode];
                bool smoothStart = (a.m_flags & NetNode.Flags.Middle) != 0;
                bool smoothEnd = (d.m_flags & NetNode.Flags.Middle) != 0;
                Vector3 b, c;
                NetSegment.CalculateMiddlePoints(a.m_position, s.m_startDirection, d.m_position, s.m_endDirection,
                    smoothStart, smoothEnd, out b, out c);

                w.NewLine();
                w.BeginObject();
                w.Prop("id", i);
                w.Prop("prefab", info != null ? info.name : null);
                w.Prop("startNode", (int)s.m_startNode);
                w.Prop("endNode", (int)s.m_endNode);
                w.Prop("startDirection", s.m_startDirection);
                w.Prop("endDirection", s.m_endDirection);
                w.Prop("invert", (s.m_flags & NetSegment.Flags.Invert) != 0);
                w.Prop("flagsRaw", (int)s.m_flags);
                w.Name("bezier").BeginArray().Value(a.m_position).Value(b).Value(c).Value(d.m_position).EndArray();
                w.Prop("length", s.m_averageLength);
                w.EndObject();

                segCount++;
                if (info != null)
                {
                    prefabs[info.name] = info;
                    int used;
                    prefabUse.TryGetValue(info.name, out used);
                    prefabUse[info.name] = used + 1;
                    svg.Segment(a.m_position, b, c, d.m_position, ServiceName(info.m_class), info.m_halfWidth);
                }
            }

            w.EndArray();
            stats["segments"] = segCount;
            stats["netPrefabs"] = prefabs.Count;
            log.Info("segments: " + segCount + ", net prefabs: " + prefabs.Count);

            w.NewLine();
            w.Name("netPrefabs").BeginObject();
            var names = new List<string>(prefabs.Keys);
            names.Sort(StringComparer.Ordinal);
            foreach (string name in names)
            {
                NetInfo info = prefabs[name];
                int used;
                prefabUse.TryGetValue(name, out used);
                w.NewLine();
                w.Name(name).BeginObject();
                WriteItemClass(w, info.m_class);
                w.Prop("ai", info.m_netAI != null ? info.m_netAI.GetType().Name : null);
                w.Prop("halfWidth", info.m_halfWidth);
                w.Prop("segmentCount", used);
                int lanes = info.m_lanes != null ? info.m_lanes.Length : 0;
                int ped = 0;
                if (info.m_lanes != null)
                {
                    foreach (NetInfo.Lane lane in info.m_lanes)
                        if ((lane.m_laneType & NetInfo.LaneType.Pedestrian) != 0) ped++;
                }

                w.Prop("laneCount", lanes);
                w.Prop("forwardVehicleLanes", info.m_forwardVehicleLaneCount);
                w.Prop("backwardVehicleLanes", info.m_backwardVehicleLaneCount);
                w.Prop("pedestrianLanes", ped);
                WriteVariants(w, info);
                w.Name("terrain").BeginObject()
                 .Prop("flattenTerrain", info.m_flattenTerrain)
                 .Prop("lowerTerrain", info.m_lowerTerrain)
                 .Prop("clipTerrain", info.m_clipTerrain)
                 .Prop("createPavement", info.m_createPavement)
                 .Prop("createGravel", info.m_createGravel)
                 .Prop("createRuining", info.m_createRuining)
                 .Prop("clipSegmentEnds", info.m_clipSegmentEnds)
                 .EndObject();
                w.Name("lanes").BeginArray();
                if (info.m_lanes != null)
                {
                    foreach (NetInfo.Lane lane in info.m_lanes)
                    {
                        w.BeginObject();
                        w.Prop("type", lane.m_laneType.ToString());
                        w.Prop("vehicleType", lane.m_vehicleType.ToString());
                        w.Prop("direction", lane.m_direction.ToString());
                        w.Prop("position", lane.m_position);
                        w.Prop("width", lane.m_width);
                        w.Prop("verticalOffset", lane.m_verticalOffset);
                        w.Prop("speedLimit", lane.m_speedLimit);
                        WriteLaneProps(w, lane);
                        w.EndObject();
                    }
                }

                w.EndArray();
                w.EndObject();
            }

            w.EndObject();
        }

        private static readonly NetNode.Flags[] s_nodeFlags =
        {
            NetNode.Flags.End, NetNode.Flags.Middle, NetNode.Flags.Bend, NetNode.Flags.Junction,
            NetNode.Flags.Untouchable, NetNode.Flags.Outside, NetNode.Flags.Double, NetNode.Flags.OnGround,
            NetNode.Flags.Underground, NetNode.Flags.Transition, NetNode.Flags.LevelCrossing,
            NetNode.Flags.OneWayIn, NetNode.Flags.OneWayOut, NetNode.Flags.Water,
        };

        private static IEnumerable<string> NodeFlagNames(NetNode.Flags flags)
        {
            foreach (NetNode.Flags f in s_nodeFlags)
                if ((flags & f) != 0) yield return f.ToString();
        }

        private static void WriteItemClass(JsonWriter w, ItemClass cls)
        {
            w.Prop("service", cls != null ? cls.m_service.ToString() : null);
            w.Prop("subService", cls != null ? cls.m_subService.ToString() : null);
            w.Prop("level", cls != null ? cls.m_level.ToString() : null);
        }

        private static string ServiceName(ItemClass cls)
        {
            if (cls == null) return "None";
            return cls.m_subService != ItemClass.SubService.None
                ? cls.m_service + "/" + cls.m_subService
                : cls.m_service.ToString();
        }

        // ---------------------------------------------------------------- timing

        private static void Timed(ExportLog log, string section, Action body)
        {
            var sw = Stopwatch.StartNew();
            body();
            log.Info("section " + section + ": " + sw.ElapsedMilliseconds + " ms");
        }

        // ---------------------------------------------------------------- map settings

        private static void WriteMapSettings(JsonWriter w, ExportLog log)
        {
            SimulationMetaData meta = Singleton<SimulationManager>.instance.m_metaData;
            w.Name("mapSettings").BeginObject();
            if (meta != null)
            {
                w.Prop("environment", meta.m_environment);
                w.Prop("startingDateTime", meta.m_startingDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
                w.Prop("invertTraffic", meta.m_invertTraffic.ToString());
                w.Prop("disableAchievements", meta.m_disableAchievements.ToString());
                w.Prop("mapEditorMapPackage", meta.m_mapEditorMapPackage);
            }
            else
            {
                w.Name("environment").Value((string)null);
                w.Name("startingDateTime").Value((string)null);
                w.Name("invertTraffic").Value((string)null);
                w.Name("disableAchievements").Value((string)null);
                w.Name("mapEditorMapPackage").Value((string)null);
            }

            w.Prop("seaLevel", SafeSeaLevel());
            WriteGameAreas(w, log);
            WriteMapTheme(w, meta != null ? meta.m_MapThemeMetaData : null);
            ThemeMixerInfo.Write(w);
            w.EndObject();
        }

        private static void WriteGameAreas(JsonWriter w, ExportLog log)
        {
            w.Name("gameAreas");
            GameAreaManager gam = Singleton<GameAreaManager>.instance;
            int[] grid = gam != null ? gam.m_areaGrid : null;
            if (grid == null)
            {
                w.Value((string)null);
                return;
            }

            // Vanilla grid is GameAreaManager.AREAGRID_RESOLUTION (5) cells of AREAGRID_CELL_SIZE (1920 m),
            // centred on the map. Tile-count mods may enlarge m_areaGrid, so derive the size from it.
            int size = Mathf.RoundToInt(Mathf.Sqrt(grid.Length));
            int startX, startZ;
            gam.GetStartTile(out startX, out startZ);
            w.BeginObject();
            w.Prop("gridSize", size);
            w.Prop("vanillaGridSize", GameAreaManager.AREAGRID_RESOLUTION);
            w.Prop("cellSize", GameAreaManager.AREAGRID_CELL_SIZE);
            w.Prop("note", "tile index = z*gridSize + x; x along +x, z along +z, tile (0,0) at the south-west corner of the grid, grid centred on the map");
            w.Prop("startTile", startZ * GameAreaManager.AREAGRID_RESOLUTION + startX);
            w.Prop("startTileX", startX);
            w.Prop("startTileZ", startZ);
            w.Prop("areaCount", gam.m_areaCount);
            w.Prop("maxAreaCount", gam.m_maxAreaCount);
            w.Name("unlocked").BeginArray();
            for (int i = 0; i < grid.Length; i++)
                if (grid[i] != 0) w.Value(i);
            w.EndArray();
            w.Name("areaGridRaw").BeginArray();
            for (int i = 0; i < grid.Length; i++) w.Value(grid[i]);
            w.EndArray();
            w.EndObject();
            if (size * size != grid.Length) log.Info("warning: m_areaGrid length " + grid.Length + " is not square");
        }

        private static void WriteMapTheme(JsonWriter w, MapThemeMetaData theme)
        {
            w.Name("mapTheme");
            if (theme == null)
            {
                w.Value((string)null);
                return;
            }

            w.BeginObject();
            w.Prop("name", theme.name);
            w.Prop("mapThemeRef", theme.mapThemeRef);
            w.Prop("environment", theme.environment);
            w.Prop("longitude", theme.longitude);
            w.Prop("latitude", theme.latitude);
            w.Prop("sunSize", theme.sunSize);
            w.Prop("sunAnisotropy", theme.sunAnisotropy);
            w.Prop("rayleigh", theme.rayleight);
            w.Prop("mie", theme.mie);
            w.Prop("exposure", theme.exposure);
            w.Name("waterClean"); WriteColor(w, theme.waterClean);
            w.Name("waterDirty"); WriteColor(w, theme.waterDirty);
            w.Name("waterUnder"); WriteColor(w, theme.waterUnder);
            w.Prop("minTemperatureDay", theme.minTemperatureDay);
            w.Prop("maxTemperatureDay", theme.maxTemperatureDay);
            w.Prop("minTemperatureNight", theme.minTemperatureNight);
            w.Prop("maxTemperatureNight", theme.maxTemperatureNight);
            w.EndObject();
        }

        private static void WriteColor(JsonWriter w, Color c)
        {
            w.BeginArray().Value(c.r).Value(c.g).Value(c.b).Value(c.a).EndArray();
        }

        // ---------------------------------------------------------------- natural resources

        private static readonly string[] s_resourceLayers = { "ore", "oil", "forest", "fertility", "pollution", "water", "sand", "shore" };

        private static void WriteNaturalResources(JsonWriter w, ExportLog log, Dictionary<string, int> stats)
        {
            const int size = NaturalResourceManager.RESOURCEGRID_RESOLUTION;
            const float cellSize = NaturalResourceManager.RESOURCEGRID_CELL_SIZE;
            NaturalResourceManager nrm = Singleton<NaturalResourceManager>.instance;
            NaturalResourceManager.ResourceCell[] cells = nrm.m_naturalResources;
            int n = cells.Length;
            var layers = new byte[s_resourceLayers.Length][];
            for (int l = 0; l < layers.Length; l++) layers[l] = new byte[n];

            // Same lock and spin the game uses (NaturalResourceManager.UpdateTexture etc.); copy only.
            while (!Monitor.TryEnter(cells, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            try
            {
                for (int i = 0; i < n; i++)
                {
                    layers[0][i] = cells[i].m_ore;
                    layers[1][i] = cells[i].m_oil;
                    layers[2][i] = cells[i].m_forest;
                    layers[3][i] = cells[i].m_fertility;
                    layers[4][i] = cells[i].m_pollution;
                    layers[5][i] = cells[i].m_water;
                    layers[6][i] = cells[i].m_sand;
                    layers[7][i] = cells[i].m_shore;
                }
            }
            finally
            {
                Monitor.Exit(cells);
            }

            w.Name("naturalResources").BeginObject();
            w.Name("grid").BeginObject();
            w.Prop("size", size);
            w.Prop("cellSize", cellSize);
            w.Prop("originX", -size * cellSize / 2f);
            w.Prop("originZ", -size * cellSize / 2f);
            w.Prop("rowAxis", "+z");
            w.Prop("note", "index = row*" + size + " + col, col along +x, row 0 = south");
            w.Prop("cellCount", n);
            w.EndObject();
            if (n != size * size) log.Info("warning: m_naturalResources length " + n + " != " + (size * size));

            w.Name("layers").BeginObject();
            for (int l = 0; l < layers.Length; l++)
            {
                long sum = 0;
                byte[] layer = layers[l];
                for (int i = 0; i < n; i++) sum += layer[i];
                stats["naturalResources." + s_resourceLayers[l] + ".sum"] = (int)Math.Min(sum, int.MaxValue);
                w.NewLine();
                w.Name(s_resourceLayers[l]);
                if (sum == 0)
                {
                    w.Value((string)null);
                    continue;
                }

                w.BeginArray();
                for (int i = 0; i < n; i++)
                {
                    if (i > 0 && i % size == 0) w.NewLine();
                    w.Value((int)layer[i]);
                }

                w.EndArray();
            }

            w.EndObject();
            w.EndObject();
            log.Info("natural resources: " + n + " cells, grid " + size + " x " + cellSize + " m");
        }

        // ---------------------------------------------------------------- water

        private static void WriteWaterSources(JsonWriter w, ExportLog log, Dictionary<string, int> stats, SvgSink svg)
        {
            WaterSimulation sim = Singleton<TerrainManager>.instance.WaterSimulation;
            FastList<WaterSource> list = sim.m_waterSources;
            WaterSource[] copy;
            // Same lock the simulation uses; we only copy, never write back.
            Monitor.Enter(list);
            try
            {
                copy = new WaterSource[list.m_size];
                Array.Copy(list.m_buffer, copy, list.m_size);
            }
            finally
            {
                Monitor.Exit(list);
            }

            int count = 0;
            w.Name("waterSources").BeginArray();
            for (int i = 0; i < copy.Length; i++)
            {
                WaterSource ws = copy[i];
                if (ws.m_type == WaterSource.TYPE_NONE) continue;
                w.NewLine();
                w.BeginObject();
                w.Prop("id", i + 1);
                w.Prop("type", WaterTypeName(ws.m_type));
                w.Prop("typeRaw", (int)ws.m_type);
                w.Prop("inputPosition", ws.m_inputPosition);
                w.Prop("outputPosition", ws.m_outputPosition);
                w.Prop("targetRaw", (int)ws.m_target);
                w.Prop("targetHeight", ws.m_target / 64f);
                w.Prop("inputRate", ws.m_inputRate);
                w.Prop("outputRate", ws.m_outputRate);
                w.Prop("water", ws.m_water);
                w.Prop("pollution", ws.m_pollution);
                w.Prop("flow", ws.m_flow);
                w.EndObject();
                svg.Water(ws.m_outputPosition, ws.m_type);
                count++;
            }

            w.EndArray();
            stats["waterSources"] = count;
            log.Info("water sources: " + count);
        }

        private static string WaterTypeName(ushort type)
        {
            switch (type)
            {
                case WaterSource.TYPE_NATURAL: return "Natural";
                case WaterSource.TYPE_FACILITY: return "Facility";
                case WaterSource.TYPE_CLEANER: return "Cleaner";
                default: return "Unknown";
            }
        }

        // ---------------------------------------------------------------- buildings

        private static void WriteBuildings(JsonWriter w, ExportLog log, Dictionary<string, int> stats, SvgSink svg)
        {
            Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            int count = 0, outside = 0, skippedSub = 0;
            var skipped = new List<int>();
            w.Name("buildings").BeginArray();
            for (int i = 1; i < buffer.Length; i++)
            {
                Building b = buffer[i];
                if ((b.m_flags & Building.Flags.Created) == 0 || (b.m_flags & Building.Flags.Deleted) != 0) continue;
                BuildingInfo info = b.Info;
                bool isOutside = info != null && info.m_buildingAI is OutsideConnectionAI;
                bool untouchable = (b.m_flags & Building.Flags.Untouchable) != 0;
                // "Plopped" = top-level buildings; sub-buildings and network-owned buildings are skipped,
                // except outside connections, which the game may create as untouchable.
                if (!isOutside && (b.m_parentBuilding != 0 || untouchable))
                {
                    skippedSub++;
                    skipped.Add(i);
                    continue;
                }

                w.NewLine();
                w.BeginObject();
                w.Prop("id", i);
                w.Prop("prefab", info != null ? info.name : null);
                WriteItemClass(w, info != null ? info.m_class : null);
                w.Prop("ai", info != null && info.m_buildingAI != null ? info.m_buildingAI.GetType().Name : null);
                w.Prop("position", b.m_position);
                w.Prop("angle", b.m_angle);
                w.Prop("width", (int)b.m_width);
                w.Prop("length", (int)b.m_length);
                w.Prop("flagsRaw", (int)b.m_flags);
                w.Prop("untouchable", untouchable);
                w.Prop("outsideConnection", isOutside);
                if (isOutside)
                {
                    w.Prop("incoming", (b.m_flags & Building.Flags.Incoming) != 0);
                    w.Prop("outgoing", (b.m_flags & Building.Flags.Outgoing) != 0);
                    w.Prop("netNode", (int)b.m_netNode);
                    outside++;
                    svg.OutsideBuilding(b.m_position);
                }

                w.EndObject();
                count++;
            }

            w.EndArray();

            // Skipped (sub-buildings and Untouchable, mostly network pillars) kept in their own array, flagged.
            var ownerNode = new Dictionary<int, int>();
            NetNode[] nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            for (int n = 1; n < nodes.Length; n++)
            {
                if ((nodes[n].m_flags & NetNode.Flags.Created) == 0 || nodes[n].m_building == 0) continue;
                ownerNode[nodes[n].m_building] = n;
            }

            w.NewLine();
            w.Name("skippedBuildings").BeginArray();
            foreach (int i in skipped)
            {
                Building b = buffer[i];
                BuildingInfo info = b.Info;
                int owner;
                bool hasOwner = ownerNode.TryGetValue(i, out owner);
                w.NewLine();
                w.BeginObject();
                w.Prop("id", i);
                w.Prop("prefab", info != null ? info.name : null);
                WriteItemClass(w, info != null ? info.m_class : null);
                w.Prop("ai", info != null && info.m_buildingAI != null ? info.m_buildingAI.GetType().Name : null);
                w.Prop("position", b.m_position);
                w.Prop("angle", b.m_angle);
                w.Prop("width", (int)b.m_width);
                w.Prop("length", (int)b.m_length);
                w.Prop("flagsRaw", (int)b.m_flags);
                w.Prop("untouchable", (b.m_flags & Building.Flags.Untouchable) != 0);
                w.Prop("parentBuilding", (int)b.m_parentBuilding);
                w.Name("ownerNode"); if (hasOwner) w.Value(owner); else w.Null();
                w.Prop("reason", b.m_parentBuilding != 0 ? "sub-building" : (hasOwner ? "network node building (pillar/pylon)" : "untouchable"));
                w.EndObject();
            }

            w.EndArray();
            stats["buildings"] = count;
            stats["outsideConnections"] = outside;
            stats["buildingsSkippedSubOrUntouchable"] = skippedSub;
            stats["buildingsSkippedOwnedByNode"] = CountOwned(skipped, ownerNode);
            log.Info("buildings: " + count + " (outside connections " + outside + ", skipped " + skippedSub + ")");
        }

        // ---------------------------------------------------------------- trees and props

        private static void WriteTrees(JsonWriter w, ExportLog log, Dictionary<string, int> stats)
        {
            TreeInstance[] buffer = Singleton<TreeManager>.instance.m_trees.m_buffer;
            var perPrefab = new Dictionary<string, Dictionary<int, int>>();
            var total = new Dictionary<int, int>();
            int count = 0, outOfRange = 0;
            for (int i = 1; i < buffer.Length; i++)
            {
                ushort flags = buffer[i].m_flags;
                if ((flags & (ushort)TreeInstance.Flags.Created) == 0 || (flags & (ushort)TreeInstance.Flags.Deleted) != 0) continue;
                TreeInfo info = buffer[i].Info;
                Vector3 p = buffer[i].Position;
                int cell = Cell(p, RasterSize);
                int totalCell = Cell(p, TotalTreeRasterSize);
                if (cell < 0 || totalCell < 0) { outOfRange++; continue; }
                string name = info != null ? info.name : "<null>";
                Increment(perPrefab, name, cell);
                Increment(total, totalCell);
                count++;
            }

            w.Name("trees").BeginObject();
            w.Prop("count", count);
            w.Prop("outOfRange", outOfRange);
            WriteRasterHeader(w, RasterSize);
            w.Name("total").BeginObject();
            w.Prop("size", TotalTreeRasterSize);
            w.Prop("cellSize", Extent / TotalTreeRasterSize);
            w.NewLine();
            WriteSparse(w, "cells", total);
            w.EndObject();
            WriteRasters(w, perPrefab);
            w.EndObject();
            stats["trees"] = count;
            stats["treePrefabs"] = perPrefab.Count;
            log.Info("trees: " + count + " in " + perPrefab.Count + " prefabs (out of range " + outOfRange + ")");
        }

        private static void WriteProps(JsonWriter w, ExportLog log, Dictionary<string, int> stats)
        {
            PropInstance[] buffer = Singleton<PropManager>.instance.m_props.m_buffer;
            var live = new List<int>();
            for (int i = 1; i < buffer.Length; i++)
            {
                ushort flags = buffer[i].m_flags;
                if ((flags & (ushort)PropInstance.Flags.Created) == 0 || (flags & (ushort)PropInstance.Flags.Deleted) != 0) continue;
                live.Add(i);
            }

            bool asPoints = live.Count < PropPointLimit;
            w.Name("props").BeginObject();
            w.Prop("count", live.Count);
            w.Prop("encoding", asPoints ? "points" : "raster");
            if (asPoints)
            {
                w.Name("points").BeginArray();
                foreach (int i in live)
                {
                    PropInfo info = buffer[i].Info;
                    w.NewLine();
                    w.BeginObject();
                    w.Prop("id", i);
                    w.Prop("prefab", info != null ? info.name : null);
                    w.Prop("position", buffer[i].Position);
                    w.Prop("angle", buffer[i].Angle);
                    w.Prop("fixedHeight", (buffer[i].m_flags & (ushort)PropInstance.Flags.FixedHeight) != 0);
                    w.EndObject();
                }

                w.EndArray();
            }
            else
            {
                var perPrefab = new Dictionary<string, Dictionary<int, int>>();
                foreach (int i in live)
                {
                    int cell = Cell(buffer[i].Position, RasterSize);
                    if (cell < 0) continue;
                    PropInfo info = buffer[i].Info;
                    Increment(perPrefab, info != null ? info.name : "<null>", cell);
                }

                WriteRasterHeader(w, RasterSize);
                WriteRasters(w, perPrefab);
            }

            w.EndObject();
            stats["props"] = live.Count;
            log.Info("props: " + live.Count + " written as " + (asPoints ? "points" : "raster"));
        }

        private static int Cell(Vector3 p, int size)
        {
            float cellSize = Extent / size;
            int cx = Mathf.FloorToInt((p.x + HalfExtent) / cellSize);
            int cz = Mathf.FloorToInt((p.z + HalfExtent) / cellSize);
            if (cx < 0 || cz < 0 || cx >= size || cz >= size) return -1;
            return cz * size + cx;
        }

        private static void Increment(Dictionary<string, Dictionary<int, int>> map, string key, int cell)
        {
            Dictionary<int, int> inner;
            if (!map.TryGetValue(key, out inner))
            {
                inner = new Dictionary<int, int>();
                map[key] = inner;
            }

            Increment(inner, cell);
        }

        private static void Increment(Dictionary<int, int> map, int cell)
        {
            int n;
            map.TryGetValue(cell, out n);
            map[cell] = n + 1;
        }

        private static void WriteRasterHeader(JsonWriter w, int size)
        {
            w.Name("raster").BeginObject();
            w.Prop("size", size);
            w.Prop("cellSize", Extent / size);
            w.Prop("origin", "cell 0 = (x=-8640, z=-8640); index = row * size + col, row along +z, col along +x");
            w.Prop("encoding", "sparse flat array [index0, count0, index1, count1, ...]");
            w.EndObject();
        }

        private static void WriteRasters(JsonWriter w, Dictionary<string, Dictionary<int, int>> perPrefab)
        {
            var names = new List<string>(perPrefab.Keys);
            names.Sort(StringComparer.Ordinal);
            w.Name("byPrefab").BeginObject();
            foreach (string name in names)
            {
                Dictionary<int, int> cells = perPrefab[name];
                int sum = 0;
                foreach (int v in cells.Values) sum += v;
                w.NewLine();
                w.Name(name).BeginObject();
                w.Prop("count", sum);
                WriteSparse(w, "cells", cells);
                w.EndObject();
            }

            w.EndObject();
        }

        private static void WriteSparse(JsonWriter w, string name, Dictionary<int, int> cells)
        {
            var keys = new List<int>(cells.Keys);
            keys.Sort();
            w.Name(name).BeginArray();
            foreach (int k in keys)
            {
                w.Value(k);
                w.Value(cells[k]);
            }

            w.EndArray();
        }
    }
}
