using System.Globalization;
using Cslmap2E2c;

return Cli.Run(args);

static class Cli
{
    const string Usage = """
        cslmap2e2c - CSL Map View export (.cslmap / .cslmap.gz) to Earth2Cities input

        usage:
          cslmap2e2c <input.cslmap[.gz]> [-o <outdir>] [options]
          cslmap2e2c --calibration [-o <outdir>] [--calib-spacing 250]
          cslmap2e2c --print-mapping

        outputs in <outdir> (default ./out):
          overpass_roads.osm   the file Earth2Cities reads (copy into its mod folder)
          roads.json           E2C's intermediate schema, for inspection (E2C regenerates its own)
          rail.geojson         rail segments in CS2 metres, for a later importer
          stats.txt            segment counts per class, skipped prefabs, predicted E2C prefabs, risks
          e2c-settings.txt     the four Overpass bbox values to type into E2C

        options:
          --scale <f>             CS1->CS2 horizontal factor (default 0.8296)
          --origin-lat <deg>      bbox centre latitude written into the OSM (default 0)
          --origin-lon <deg>      bbox centre longitude (default 0)
          --map-width <m>         CS2 playable width (default 14336)
          --mapping <file.tsv>    extra rules "regex<TAB>class", tried before the defaults;
                                  matched against "icls|basename"; classes: motorway motorway_link
                                  trunk primary secondary tertiary residential rail skip
          --simplify <m>          Douglas-Peucker tolerance (default 0.5; 0 = off)
          --min-spacing <m>       drop interior points closer than this (default 8)
          --max-seg <m>           densify straight pieces longer than this (default 0 = off)
          --motorway-min-lanes <n> lanes written for one-way motorways below n (default 3)
          --link-lanes <n>        lanes written for ramps (default 1; 0 = keep)
          --clip                  cut roads and rail at the playable square (±map-width/2); ends land on the border
          --drop-tunnels          omit tunnel segments (E2C would lay them on the surface)
          --reverse-oneways       flip the direction of every one-way way
          --stats                 print stats.txt to the console as well
        """;

    public static int Run(string[] args)
    {
        try { return RunCore(args); }
        catch (Exception ex) when (ex is FormatException or IOException or InvalidDataException or ArgumentException or System.Xml.XmlException)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 2;
        }
    }

    static int RunCore(string[] args)
    {
        string? input = null, outDir = "out", mapping = null;
        bool calibration = false, printStats = false, printMapping = false;
        double originLat = 0, originLon = 0, calibSpacing = 250;
        var opt = new ConvertOptions();

        for (int i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{args[i]} needs a value");
            double Num() => double.Parse(Next(), NumberStyles.Float, CultureInfo.InvariantCulture);
            switch (args[i])
            {
                case "-h" or "--help": Console.WriteLine(Usage); return 0;
                case "-o" or "--out": outDir = Next(); break;
                case "--calibration": calibration = true; break;
                case "--calib-spacing": calibSpacing = Num(); break;
                case "--print-mapping": printMapping = true; break;
                case "--stats": printStats = true; break;
                case "--scale": opt.Scale = Num(); break;
                case "--origin-lat": originLat = Num(); break;
                case "--origin-lon": originLon = Num(); break;
                case "--map-width": opt.PlayableWidth = Num(); break;
                case "--mapping": mapping = Next(); break;
                case "--simplify": opt.SimplifyTolerance = Num(); break;
                case "--min-spacing": opt.MinSpacing = Num(); break;
                case "--max-seg": opt.MaxSegment = Num(); break;
                case "--motorway-min-lanes": opt.MotorwayMinLanes = (int)Num(); break;
                case "--link-lanes": opt.LinkLanes = (int)Num(); break;
                case "--clip": opt.Clip = true; break;
                case "--drop-tunnels": opt.DropTunnels = true; break;
                case "--reverse-oneways": opt.ReverseOneWays = true; break;
                default:
                    if (args[i].StartsWith('-') || input != null) throw new ArgumentException($"unexpected argument '{args[i]}'\n\n{Usage}");
                    input = args[i];
                    break;
            }
        }

        var classifier = new Classifier(mapping != null ? Classifier.LoadRules(mapping) : null);
        if (printMapping)
        {
            foreach (var r in classifier.Rules) Console.WriteLine($"{r.Pattern}\t{r.Class.ToString().ToLowerInvariant()}\t# {r.Source}");
            return 0;
        }
        if (!calibration && input == null) { Console.Error.WriteLine(Usage); return 1; }

        var proj = new E2cProjection(originLat, originLon, opt.PlayableWidth);
        var net = calibration ? Calibration.Build(calibSpacing) : Converter.Convert(CslMap.Load(input!), classifier, opt);

        Directory.CreateDirectory(outDir);
        Writers.WriteOsm(Path.Combine(outDir, "overpass_roads.osm"), net, proj);
        Writers.WriteRoadsJson(Path.Combine(outDir, "roads.json"), net, proj);
        if (!calibration) Writers.WriteRailGeoJson(Path.Combine(outDir, "rail.geojson"), net, opt.Scale);
        var stats = Writers.FormatStats(net, opt);
        File.WriteAllText(Path.Combine(outDir, "stats.txt"), stats);
        var settings = Writers.FormatSettings(proj, opt);
        File.WriteAllText(Path.Combine(outDir, "e2c-settings.txt"), settings);

        double maxErr = SelfCheck(net, proj);
        if (printStats) Console.WriteLine(stats);
        Console.WriteLine($"wrote {net.Ways.Count} road ways, {net.Rail.Count} rail lines to {Path.GetFullPath(outDir)}");
        Console.WriteLine($"round-trip through E2C's projection model: max error {maxErr:0.000} m");
        Console.WriteLine(settings);
        return 0;
    }

    /// <summary>Largest distance between a node's intended game position and where E2C's float32 projection puts it.</summary>
    static double SelfCheck(Network net, E2cProjection proj)
    {
        double max = 0;
        double half = proj.PlayableWidth / 2;
        foreach (var p in net.Nodes.Values)
        {
            if (Math.Abs(p.X) > half || Math.Abs(p.Z) > half) continue; // clamped by design
            var (lat, lon) = proj.ToGeo(p.X, p.Z);
            // E2C reads the OSM text with double.Parse, then narrows to float.
            var (x, z) = proj.SimulateE2c(double.Parse(lat.ToString("0.#########", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                                          double.Parse(lon.ToString("0.#########", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
            max = Math.Max(max, Math.Sqrt((x - p.X) * (x - p.X) + (z - p.Z) * (z - p.Z)));
        }
        return max;
    }
}
