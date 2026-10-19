using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ColossalFramework;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Writes every live tree to a sibling file, schema dakota-export/cs1/trees/v2.
    /// v1 fields (count, prefabs, points = [x, z, prefabIdx, ...]) are unchanged; v2 adds parallel
    /// arrays in the same order: ids (TreeManager buffer index), y, flags, scale (Tree Control).
    /// Streams with a reused char buffer, so there is no per-tree allocation.
    /// </summary>
    internal static class TreePointsWriter
    {
        public const string Schema = "dakota-export/cs1/trees/v2";
        private const int PointsPerLine = 1000;
        private const string TreeControlPatches = "TreeControl.Patches.TreeInstancePatches";

        public static void Write(string path, ExportLog log, Dictionary<string, int> stats)
        {
            TreeInstance[] buffer = Singleton<TreeManager>.instance.m_trees.m_buffer;
            const ushort created = (ushort)TreeInstance.Flags.Created;
            const ushort deleted = (ushort)TreeInstance.Flags.Deleted;

            // Pass 1: prefab table (by m_infoIndex) and the list of live ids.
            var indexByInfo = new Dictionary<ushort, int>();
            var prefabs = new List<string>();
            var ids = new List<int>();
            for (int i = 1; i < buffer.Length; i++)
            {
                ushort flags = buffer[i].m_flags;
                if ((flags & created) == 0 || (flags & deleted) != 0) continue;
                ushort infoIndex = buffer[i].m_infoIndex;
                if (!indexByInfo.ContainsKey(infoIndex))
                {
                    TreeInfo info = buffer[i].Info;
                    indexByInfo[infoIndex] = prefabs.Count;
                    prefabs.Add(info != null ? info.name : "<null:" + infoIndex + ">");
                }

                ids.Add(i);
            }

            string scaleNote;
            byte[] scaling = ReadTreeControlScaling(buffer.Length, out scaleNote);
            int scaled = 0, fixedHeight = 0;

            SafeFile.Write(path, tmp =>
            {
                using (var stream = new StreamWriter(tmp, false, new UTF8Encoding(false), 1 << 16))
                {
                    var w = new JsonWriter(stream);
                    char[] scratch = new char[24];
                    w.BeginObject();
                    w.Prop("schema", Schema);
                    w.Prop("count", ids.Count);
                    w.NewLine();
                    w.StringArray("prefabs", prefabs);
                    w.NewLine();
                    w.Prop("arrays", "points = [x, z, prefabIdx] per tree; ids, y, flags, scale are parallel arrays in the same tree order");
                    w.Prop("yNote", "y = TreeInstance.Position.y (m_posY / 64) in metres, rounded to 0.1; for FixedHeight trees it is the snapped height, otherwise the terrain height at the tree");
                    w.Prop("flagsNote", "TreeInstance.Flags as integer: 1 Created, 4 Hidden, 0x10 Single, 0x20 FixedHeight, 0x40 FireDamage, 0x80 Burning, 0xF00 GrowState");
                    w.Prop("scaleSource", scaling != null ? "Tree Control " + TreeControlPatches + ".s_scalingData[id] * 0.02 (50 = 1.0)" : null);
                    w.Prop("scaleNote", scaleNote);

                    // points (v1, unchanged)
                    w.Name("points");
                    stream.Write('[');
                    for (int k = 0; k < ids.Count; k++)
                    {
                        int i = ids[k];
                        Vector3 p = buffer[i].Position;
                        if (k > 0) stream.Write(',');
                        if (k % PointsPerLine == 0) stream.Write('\n');
                        WriteTenths(stream, scratch, p.x);
                        stream.Write(',');
                        WriteTenths(stream, scratch, p.z);
                        stream.Write(',');
                        WriteInt(stream, scratch, indexByInfo[buffer[i].m_infoIndex]);
                    }

                    stream.Write("\n]");

                    // ids
                    stream.Write(",\"ids\":[");
                    for (int k = 0; k < ids.Count; k++)
                    {
                        if (k > 0) stream.Write(',');
                        if (k % PointsPerLine == 0) stream.Write('\n');
                        WriteInt(stream, scratch, ids[k]);
                    }

                    stream.Write("\n]");

                    // y
                    stream.Write(",\"y\":[");
                    for (int k = 0; k < ids.Count; k++)
                    {
                        if (k > 0) stream.Write(',');
                        if (k % PointsPerLine == 0) stream.Write('\n');
                        WriteTenths(stream, scratch, buffer[ids[k]].Position.y);
                    }

                    stream.Write("\n]");

                    // flags
                    stream.Write(",\"flags\":[");
                    for (int k = 0; k < ids.Count; k++)
                    {
                        if (k > 0) stream.Write(',');
                        if (k % PointsPerLine == 0) stream.Write('\n');
                        ushort f = buffer[ids[k]].m_flags;
                        if ((f & (ushort)TreeInstance.Flags.FixedHeight) != 0) fixedHeight++;
                        WriteInt(stream, scratch, f);
                    }

                    stream.Write("\n]");

                    // scale
                    stream.Write(",\"scale\":");
                    if (scaling == null)
                    {
                        stream.Write("null");
                    }
                    else
                    {
                        stream.Write('[');
                        for (int k = 0; k < ids.Count; k++)
                        {
                            if (k > 0) stream.Write(',');
                            if (k % PointsPerLine == 0) stream.Write('\n');
                            int id = ids[k];
                            byte b = id < scaling.Length ? scaling[id] : (byte)50;
                            if (b != 50) scaled++;
                            // b * 0.02 written exactly with two decimals: b/50 = whole + (b%50)*2/100
                            WriteHundredths(stream, scratch, b * 2);
                        }

                        stream.Write("\n]");
                    }

                    w.EndObject();
                    stream.Write('\n');
                }
            });

            stats["treePoints"] = ids.Count;
            stats["treePointPrefabs"] = prefabs.Count;
            stats["treePoints.fixedHeight"] = fixedHeight;
            stats["treePoints.scaled"] = scaled;
            log.Info("tree points: " + ids.Count + " in " + prefabs.Count + " prefabs; fixed height " + fixedHeight +
                     ", scaled " + scaled + " (" + (scaleNote ?? "Tree Control scaling read") + ")");
        }

        private static byte[] ReadTreeControlScaling(int bufferLength, out string note)
        {
            note = null;
            try
            {
                Type t = ModReflection.FindType(TreeControlPatches);
                if (t == null)
                {
                    note = "Tree Control not loaded (type " + TreeControlPatches + " not found); scale is null";
                    return null;
                }

                FieldInfo f = t.GetField("s_scalingData", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                byte[] data = f != null ? f.GetValue(null) as byte[] : null;
                if (data == null)
                {
                    note = "Tree Control loaded but s_scalingData is null; scale is null";
                    return null;
                }

                if (data.Length < bufferLength) note = "s_scalingData length " + data.Length + " < tree buffer " + bufferLength + "; missing ids written as 1.0";
                return data;
            }
            catch (Exception ex)
            {
                note = "Tree Control scaling read failed: " + ex.Message;
                return null;
            }
        }

        /// <summary>Writes v rounded to 0.1 as e.g. "-1234.5" or "12" without allocating.</summary>
        private static void WriteTenths(TextWriter w, char[] scratch, float v)
        {
            long tenths = (long)Math.Round(v * 10.0, MidpointRounding.AwayFromZero);
            bool negative = tenths < 0;
            if (negative) tenths = -tenths;
            long whole = tenths / 10;
            int frac = (int)(tenths % 10);
            int pos = scratch.Length;
            if (frac != 0)
            {
                scratch[--pos] = (char)('0' + frac);
                scratch[--pos] = '.';
            }

            do
            {
                scratch[--pos] = (char)('0' + (int)(whole % 10));
                whole /= 10;
            }
            while (whole > 0);

            if (negative) scratch[--pos] = '-';
            w.Write(scratch, pos, scratch.Length - pos);
        }

        /// <summary>Writes hundredths (non-negative) as a decimal, e.g. 186 -> "1.86", 100 -> "1".</summary>
        private static void WriteHundredths(TextWriter w, char[] scratch, int hundredths)
        {
            int whole = hundredths / 100;
            int frac = hundredths % 100;
            int pos = scratch.Length;
            if (frac != 0)
            {
                if (frac % 10 == 0)
                {
                    scratch[--pos] = (char)('0' + frac / 10);
                }
                else
                {
                    scratch[--pos] = (char)('0' + frac % 10);
                    scratch[--pos] = (char)('0' + frac / 10);
                }

                scratch[--pos] = '.';
            }

            do
            {
                scratch[--pos] = (char)('0' + whole % 10);
                whole /= 10;
            }
            while (whole > 0);

            w.Write(scratch, pos, scratch.Length - pos);
        }

        private static void WriteInt(TextWriter w, char[] scratch, int v)
        {
            int pos = scratch.Length;
            do
            {
                scratch[--pos] = (char)('0' + v % 10);
                v /= 10;
            }
            while (v > 0);

            w.Write(scratch, pos, scratch.Length - pos);
        }
    }
}
