// Water cap for the falls-steps layer: rocks in the CS1 river channel are lowered to just under the CS1 water surface so the
// river steps down over them instead of being dammed. Heights come from the extractor's full-resolution layer raster;
// per-object ownership comes from rasterising each object's (decimated) mesh from po-offline.json into the playable frame.
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HmRebuild;

public static class FallsCap
{
    const int N = Peaks.N, S = Program.SrcN;
    const double CellM = Peaks.CellM;

    public sealed record ObjCap(string Id, int Px, int WetPx, int CapPx, int CapCells, double MaxAddRawM, double MaxAddCappedM, bool Skipped);

    /// <param name="WaterCap">per playable pixel: CS1 water surface − offset where wet, NaN where dry (for the final surface cap)</param>
    public sealed record CapResult(Stamp.PoField Field, List<ObjCap> Objects, int CapPx, int CapCells, int UnownedPx, int SkippedObjects, double[] WaterCap);

    /// <summary>CS1 water surface (1081², surface × unitsPerM, row 0 north, 0 = dry) in metres; NaN = dry.</summary>
    public static double[] LoadWater(string path, double unitsPerM = 64)
    {
        using var img = Image.Load<L16>(path);
        if (img.Width != S || img.Height != S) throw new InputException($"{path}: expected {S}x{S}");
        var w = new double[S * S];
        img.ProcessPixelRows(a => { for (int y = 0; y < S; y++) { var r = a.GetRowSpan(y); for (int x = 0; x < S; x++) w[y * S + x] = r[x].PackedValue == 0 ? double.NaN : r[x].PackedValue / unitsPerM; } });
        return w;
    }

    /// <summary>Water at a playable pixel: wet if the nearest CS1 cell is wet; surface = bilinear over the wet cells of the
    /// surrounding 2x2 (renormalised). NaN = dry.</summary>
    public static double WaterAt(double[] w, int px, int py)
    {
        double x = -7168 + CellM * (px + 0.5), z = 7168 - CellM * (py + 0.5);
        double fc = (x + 8640) / 16, fr = (8640 - z) / 16;
        int nc = (int)Math.Round(fc), nr = (int)Math.Round(fr);
        if (nc < 0 || nr < 0 || nc >= S || nr >= S || double.IsNaN(w[nr * S + nc])) return double.NaN;
        int c0 = Math.Clamp((int)Math.Floor(fc), 0, S - 2), r0 = Math.Clamp((int)Math.Floor(fr), 0, S - 2);
        double a = fc - c0, b = fr - r0, acc = 0, ws = 0;
        for (int dr = 0; dr <= 1; dr++) for (int dc = 0; dc <= 1; dc++)
            {
                double v = w[(r0 + dr) * S + c0 + dc]; if (double.IsNaN(v)) continue;
                double k = (dc == 1 ? a : 1 - a) * (dr == 1 ? b : 1 - b); acc += k * v; ws += k;
            }
        return ws > 1e-9 ? acc / ws : w[nr * S + nc];
    }

    /// <summary>Per-pixel owner (index into ids) from each object's mesh: the object whose own rasterised top is highest at
    /// the pixel. Pixels of the layer no mesh covers take the nearest owned pixel within 4 px.</summary>
    public static int[] Owners(string poJson, List<string> ids)
    {
        var index = ids.Select((id, k) => (id, k)).ToDictionary(t => t.id, t => t.k);
        var ownerTop = new double[N * N]; Array.Fill(ownerTop, double.NegativeInfinity);
        var owner = new int[N * N]; Array.Fill(owner, -1);
        foreach (var line in File.ReadLines(poJson))
        {
            var t = line.Trim().TrimEnd(',');
            if (!t.StartsWith("{\"id\"")) continue;
            using var doc = JsonDocument.Parse(t);
            var root = doc.RootElement;
            var idEl = root.GetProperty("id");
            var id = idEl.ValueKind == JsonValueKind.String ? idEl.GetString()! : idEl.GetRawText();
            if (!index.TryGetValue(id, out int k)) continue;
            if (root.GetProperty("triangles").ValueKind != JsonValueKind.Array) continue;
            var v = root.GetProperty("vertices").EnumerateArray().Select(e => e.GetDouble()).ToArray();
            var tri = root.GetProperty("triangles").EnumerateArray().Select(e => e.GetInt32()).ToArray();
            void Put(int px, int py, double y) { if (px < 0 || py < 0 || px >= N || py >= N) return; int i = py * N + px; if (y > ownerTop[i]) { ownerTop[i] = y; owner[i] = k; } }
            double X(int vi) => (v[3 * vi] + 7168) / CellM - 0.5;
            double Y(int vi) => (7168 - v[3 * vi + 2]) / CellM - 0.5;
            double H(int vi) => v[3 * vi + 1];
            for (int j = 0; j + 2 < tri.Length; j += 3)
            {
                int a = tri[j], b = tri[j + 1], c = tri[j + 2];
                double ax = X(a), ay = Y(a), bx = X(b), by = Y(b), cx = X(c), cy = Y(c);
                double den = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy);
                if (Math.Abs(den) < 1e-12) continue;
                int x0 = (int)Math.Ceiling(Math.Min(ax, Math.Min(bx, cx))), x1 = (int)Math.Floor(Math.Max(ax, Math.Max(bx, cx)));
                int y0 = (int)Math.Ceiling(Math.Min(ay, Math.Min(by, cy))), y1 = (int)Math.Floor(Math.Max(ay, Math.Max(by, cy)));
                for (int py = y0; py <= y1; py++)
                    for (int px = x0; px <= x1; px++)
                    {
                        double l1 = ((by - cy) * (px - cx) + (cx - bx) * (py - cy)) / den, l2 = ((cy - ay) * (px - cx) + (ax - cx) * (py - cy)) / den, l3 = 1 - l1 - l2;
                        if (l1 < -1e-9 || l2 < -1e-9 || l3 < -1e-9) continue;
                        Put(px, py, l1 * H(a) + l2 * H(b) + l3 * H(c));
                    }
            }
            for (int vi = 0; vi < v.Length / 3; vi++) Put((int)Math.Round(X(vi)), (int)Math.Round(Y(vi)), H(vi));   // thin walls
        }
        return owner;
    }

    /// <summary>Applies the cap: top' = min(top, water − offset) where the pixel is wet in CS1, top elsewhere; objects whose
    /// largest capped rise above the terrain is below minAddM are dropped.</summary>
    public static CapResult Apply(Stamp.PoField layer, int[] owner, List<string> ids, double[] water, double[] terrainM, double offsetM, double minAddM)
    {
        // Owner for layer pixels without a mesh owner: nearest owned pixel within 4 px.
        var (dOwn, nearOwn) = Stamp.EdtNearest(N, N, owner.Select(o => o >= 0).ToArray());
        int nObj = ids.Count;
        var px = new int[nObj]; var wet = new int[nObj]; var capPx = new int[nObj]; var addRaw = new double[nObj]; var addCap = new double[nObj];
        var capCells = Enumerable.Range(0, nObj).Select(_ => new HashSet<int>()).ToArray();
        var allCapCells = new HashSet<int>();
        var capped = new double[N * N]; Array.Fill(capped, double.NaN);
        var own = new int[N * N]; Array.Fill(own, -1);
        int unowned = 0, capTotal = 0;
        for (int i = 0; i < N * N; i++)
        {
            if (double.IsNaN(layer.Top[i])) continue;
            int o = owner[i] >= 0 ? owner[i] : (dOwn[i] <= 4 && nearOwn[i] >= 0 ? owner[nearOwn[i]] : -1);
            if (o < 0) { unowned++; continue; }
            own[i] = o; px[o]++;
            double top = layer.Top[i], t = top;
            double wv = WaterAt(water, i % N, i / N);
            if (!double.IsNaN(wv))
            {
                wet[o]++;
                if (top > wv - offsetM)
                {
                    t = wv - offsetM; capPx[o]++; capTotal++;
                    double x = -7168 + CellM * (i % N + 0.5), z = 7168 - CellM * (i / N + 0.5);
                    int cell = (int)Math.Round((8640 - z) / 16) * S + (int)Math.Round((x + 8640) / 16);
                    capCells[o].Add(cell); allCapCells.Add(cell);
                }
            }
            capped[i] = t;
            addRaw[o] = Math.Max(addRaw[o], top - terrainM[i]);
            addCap[o] = Math.Max(addCap[o], t - terrainM[i]);
        }
        var f = new Stamp.PoField();
        f.Sources.AddRange(layer.Sources);
        f.Sources.Add($"water cap: top' = min(top, CS1 water - {offsetM} m) on wet cells; objects adding < {minAddM} m after the cap are skipped");
        var objs = new List<ObjCap>(); int skipped = 0;
        var skip = new bool[nObj];
        for (int o = 0; o < nObj; o++)
        {
            skip[o] = px[o] == 0 || addCap[o] < minAddM;
            if (px[o] > 0 && skip[o]) skipped++;
            objs.Add(new ObjCap(ids[o], px[o], wet[o], capPx[o], capCells[o].Count, px[o] > 0 ? addRaw[o] : 0, px[o] > 0 ? addCap[o] : 0, skip[o]));
            if (!skip[o]) f.SourceCells[ids[o]] = px[o];
        }
        for (int i = 0; i < N * N; i++)
        {
            int o = own[i]; if (o < 0 || skip[o]) continue;
            f.Top[i] = capped[i]; f.Key[i] = ids[o]; f.Steep[i] = layer.Steep[i];
        }
        var wcap = new double[N * N];
        Parallel.For(0, N, y => { for (int x = 0; x < N; x++) { double wv = WaterAt(water, x, y); wcap[y * N + x] = double.IsNaN(wv) ? double.NaN : wv - offsetM; } });
        return new CapResult(f, objs.OrderByDescending(x => x.Px).ToList(), capTotal, allCapCells.Count, unowned, skipped, wcap);
    }
}
