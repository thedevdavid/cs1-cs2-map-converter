using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;

namespace DakotaExport
{
    /// <summary>
    /// Current water surface on the 1081 x 1081 heightmap vertex grid, read directly from the water
    /// simulation's front buffer (WaterSimulation.BeginRead/EndRead, the same pair TerrainManager uses)
    /// and TerrainManager.BlockHeights. Writes the "waterSurface" section and a 16-bit PNG.
    /// </summary>
    internal static class WaterSurfaceExporter
    {
        public const int Size = 1081;
        public const float CellSize = 16f;
        public const float Threshold = 0.05f;

        public static void Write(JsonWriter w, string pngPath, ExportLog log, Dictionary<string, int> stats)
        {
            TerrainManager tm = Singleton<TerrainManager>.instance;
            WaterSimulation sim = tm.WaterSimulation;
            int n = Size * Size;

            // Copy depths (raw units, 1/64 m) under the simulation's read lock, then release it at once.
            var depthRaw = new ushort[n];
            WaterSimulation.Cell[] cells = sim.BeginRead();
            try
            {
                if (cells == null || cells.Length < n) throw new InvalidOperationException("water buffer length " + (cells == null ? -1 : cells.Length));
                for (int i = 0; i < n; i++) depthRaw[i] = cells[i].m_height;
            }
            finally
            {
                sim.EndRead();
            }

            ushort[] block = tm.BlockHeights;
            if (block == null || block.Length < n) throw new InvalidOperationException("BlockHeights length " + (block == null ? -1 : block.Length));

            ushort thresholdRaw = (ushort)Math.Ceiling(Threshold * 64f);
            var wet = new bool[n];
            var surfaceRaw = new int[n];
            int wetCount = 0;
            for (int i = 0; i < n; i++)
            {
                if (depthRaw[i] < thresholdRaw) continue; // depth > 0.05 m  <=>  raw >= 4 (0.0625 m)
                wet[i] = true;
                surfaceRaw[i] = block[i] + depthRaw[i];
                wetCount++;
            }

            // Connected wet bodies (4-neighbour).
            int bodies, largestCount;
            float largestMedian;
            LargestBody(wet, surfaceRaw, out bodies, out largestCount, out largestMedian);

            var all = new float[wetCount];
            int k = 0;
            for (int i = 0; i < n; i++) if (wet[i]) all[k++] = surfaceRaw[i] / 64f;
            Array.Sort(all);

            w.Name("waterSurface").BeginObject();
            w.Prop("source", "WaterSimulation.BeginRead() front buffer Cell.m_height (depth, raw/64 m) + TerrainManager.BlockHeights (raw/64 m)");
            w.Name("grid").BeginObject();
            w.Prop("size", Size);
            w.Prop("cellSize", CellSize);
            w.Prop("originX", -8640f);
            w.Prop("originZ", -8640f);
            w.Prop("rowAxis", "+z");
            w.Prop("note", "index = row*1081 + col; sample point x = -8640 + col*16, z = -8640 + row*16 (heightmap vertex = CS1 heightmap PNG pixel), row 0 = south. Unlike trees.total (17280/1081 m bins) this is the simulation's own vertex grid.");
            w.EndObject();
            w.Prop("threshold", Threshold);
            w.Prop("thresholdNote", "a cell is wet when Cell.m_height >= " + thresholdRaw + " raw (" + (thresholdRaw / 64f) + " m)");
            w.Prop("surfaceDefinition", "surface = (BlockHeights + m_height) / 64. The game renders depths below 1 m against its private m_rawHeights2 instead of BlockHeights; differences are a few cm.");
            w.Prop("encoding", "cells: flat [index, surfaceMetres, depthMetres, ...], rounded to 0.01 m, wet cells only");
            w.Prop("png", Path.GetFileName(pngPath));

            w.Name("stats").BeginObject();
            w.Prop("wetCells", wetCount);
            w.Prop("bodies", bodies);
            w.Name("minSurface"); if (wetCount > 0) w.Value(all[0]); else w.Null();
            w.Name("medianSurface"); if (wetCount > 0) w.Value(all[wetCount / 2]); else w.Null();
            w.Name("maxSurface"); if (wetCount > 0) w.Value(all[wetCount - 1]); else w.Null();
            w.Prop("largestBodyCells", largestCount);
            w.Name("largestBodyMedianSurface"); if (largestCount > 0) w.Value(largestMedian); else w.Null();
            w.Prop("seaLevel", sim.m_currentSeaLevel);
            w.EndObject();

            w.Name("cells").BeginArray();
            int written = 0;
            for (int i = 0; i < n; i++)
            {
                if (!wet[i]) continue;
                if (written % 500 == 0) w.NewLine();
                w.Value(i);
                w.Value2(surfaceRaw[i] / 64f);
                w.Value2(depthRaw[i] / 64f);
                written++;
            }

            w.EndArray();
            w.EndObject();

            // PNG: row 0 = north, value = surface * 64 (raw units) where wet, 0 where dry.
            var png = new ushort[n];
            for (int row = 0; row < Size; row++)
            {
                int dstRow = Size - 1 - row;
                for (int col = 0; col < Size; col++)
                {
                    int i = row * Size + col;
                    if (wet[i]) png[dstRow * Size + col] = (ushort)Math.Min(65535, surfaceRaw[i]);
                }
            }

            string method = PngWriter.WriteGray16(pngPath, Size, Size, png);
            stats["waterSurface.wetCells"] = wetCount;
            stats["waterSurface.bodies"] = bodies;
            stats["waterSurface.largestBodyCells"] = largestCount;
            log.Info("water surface: " + wetCount + " wet cells, " + bodies + " bodies, largest " + largestCount +
                     " cells at median " + largestMedian + " m; png " + Path.GetFileName(pngPath) + " (" + method + ")");
        }

        private static void LargestBody(bool[] wet, int[] surfaceRaw, out int bodies, out int largestCount, out float largestMedian)
        {
            int n = wet.Length;
            var label = new int[n];
            var queue = new int[n];
            bodies = 0;
            largestCount = 0;
            int largestLabel = 0;
            for (int start = 0; start < n; start++)
            {
                if (!wet[start] || label[start] != 0) continue;
                bodies++;
                int head = 0, tail = 0, count = 0;
                queue[tail++] = start;
                label[start] = bodies;
                while (head < tail)
                {
                    int i = queue[head++];
                    count++;
                    int row = i / Size, col = i % Size;
                    if (col > 0) Visit(i - 1, wet, label, queue, ref tail, bodies);
                    if (col < Size - 1) Visit(i + 1, wet, label, queue, ref tail, bodies);
                    if (row > 0) Visit(i - Size, wet, label, queue, ref tail, bodies);
                    if (row < Size - 1) Visit(i + Size, wet, label, queue, ref tail, bodies);
                }

                if (count > largestCount)
                {
                    largestCount = count;
                    largestLabel = bodies;
                }
            }

            largestMedian = float.NaN;
            if (largestCount == 0) return;
            var values = new float[largestCount];
            int k = 0;
            for (int i = 0; i < n; i++) if (label[i] == largestLabel) values[k++] = surfaceRaw[i] / 64f;
            Array.Sort(values);
            largestMedian = values[largestCount / 2];
        }

        private static void Visit(int j, bool[] wet, int[] label, int[] queue, ref int tail, int id)
        {
            if (!wet[j] || label[j] != 0) return;
            label[j] = id;
            queue[tail++] = j;
        }
    }
}
