using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
using Cslmap2E2c;

namespace Cslmap2E2c.Tests;

public class ConverterTests
{
    static readonly string Fixture = Path.Combine(AppContext.BaseDirectory, "fixtures", "synthetic.cslmap");
    static readonly ConvertOptions Opt = new();

    static Network ConvertFixture(ConvertOptions? opt = null) =>
        Converter.Convert(CslMap.Load(Fixture), new Classifier(), opt ?? Opt);

    static RoadWay Way(Network n, string prefab) => n.Ways.Single(w => w.Prefab == prefab);

    [Fact]
    public void Parses_nodes_segments_and_segment_types()
    {
        var map = CslMap.Load(Fixture);
        Assert.Equal("4.1", map.Version);
        Assert.Equal(18, map.Nodes.Count);
        Assert.Equal(11, map.Segments.Count);
        Assert.Equal(6, map.SegmentTypes.Count);
        Assert.Equal("Old Mill Road", map.Segments.Single(s => s.Id == 5).Name);
        Assert.True(map.Nodes[30].Underground);
    }

    [Fact]
    public void Loads_gzip_exports_like_plain_xml()
    {
        var gz = Path.Combine(Path.GetTempPath(), $"cslmap2e2c-{Guid.NewGuid():N}.cslmap.gz");
        try
        {
            using (var src = File.OpenRead(Fixture))
            using (var dst = new GZipStream(File.Create(gz), CompressionLevel.Fastest))
                src.CopyTo(dst);
            Assert.Equal(CslMap.Load(Fixture).Segments.Count, CslMap.Load(gz).Segments.Count);
        }
        finally { File.Delete(gz); }
    }

    [Theory]
    [InlineData("Highway", "2452038713.USRP 2L Interstate_Data", NetClass.Motorway, Structure.Ground)]
    [InlineData("Highway", "2452038713.USRP 2L Interstate E", NetClass.Motorway, Structure.Elevated)]
    [InlineData("Highway", "2452038713.USRP 1L Interstate Ramp E", NetClass.MotorwayLink, Structure.Elevated)]
    [InlineData("Highway", "2097530472.USRP 2-Lane National Road B", NetClass.Primary, Structure.Bridge)]
    [InlineData("Highway Tunnel", "2452038713.USRP 3L Interstate T", NetClass.Motorway, Structure.Tunnel)]
    [InlineData("Gravel Road", "Gravel Road", NetClass.Residential, Structure.Ground)]
    [InlineData("Large Road", "Six-Lane Road", NetClass.Primary, Structure.Ground)]
    [InlineData("Medium Road", "Four-Lane Road", NetClass.Secondary, Structure.Ground)]
    [InlineData("Small Road", "Basic Road", NetClass.Residential, Structure.Ground)]
    [InlineData("Train Track", "2610252544.R2USA W GrWo 2x2_Data", NetClass.Rail, Structure.Ground)]
    [InlineData("Beautification Item", "1889660581.Concrete Barrier_Data", NetClass.Skip, Structure.Ground)]
    [InlineData("Beautification Item", "2105051034.Terraforming Network Ext 0.5U_Data", NetClass.Skip, Structure.Ground)]
    [InlineData("Highway", "Highway Guardrail", NetClass.Skip, Structure.Ground)]
    [InlineData("Monorail Track", "Monorail Oneway Track", NetClass.Skip, Structure.Ground)]
    public void Classifies_prefabs(string icls, string prefab, NetClass expected, Structure structure)
    {
        var (baseName, s) = Classifier.SplitPrefab(prefab, icls);
        Assert.Equal(expected, new Classifier().Classify(icls, baseName).Class);
        Assert.Equal(structure, s);
    }

    [Fact]
    public void User_mapping_rules_take_precedence()
    {
        var rules = new List<MappingRule> { new(@"\|Gravel Road$", NetClass.Skip, "test") };
        var net = Converter.Convert(CslMap.Load(Fixture), new Classifier(rules), Opt);
        Assert.DoesNotContain(net.Ways, w => w.Prefab == "Gravel Road");
    }

    [Fact]
    public void Writes_roads_skips_decoration_and_routes_rail_separately()
    {
        var net = ConvertFixture();
        Assert.Equal(8, net.Ways.Count); // 2 interstate + ramp + national + gravel + one-way + tunnel + border
        Assert.Single(net.Rail);
        Assert.Equal(1, net.Stats.SkippedPrefabs["1889660581.Concrete Barrier_Data"]);
        Assert.Equal(1, net.Stats.UnmatchedPrefabs["123456.Totally Unknown Thing_Data"]);
        Assert.Equal(11, net.Stats.SegmentsTotal);
    }

    [Fact]
    public void Consecutive_segments_share_the_cs1_node()
    {
        var net = ConvertFixture();
        var a = Way(net, "2452038713.USRP 2L Interstate_Data");
        var b = Way(net, "2452038713.USRP 2L Interstate E");
        var ramp = Way(net, "2452038713.USRP 1L Interstate Ramp_Data");
        Assert.Equal(a.NodeIds[^1], b.NodeIds[0]);
        Assert.Equal(b.NodeIds[^1], ramp.NodeIds[0]);
        Assert.Equal(12L, a.NodeIds[^1]); // CS1 node 11 -> OSM id 12
    }

    [Fact]
    public void Applies_scale_to_positions()
    {
        var net = ConvertFixture();
        var p = net.Nodes[Way(net, "2452038713.USRP 2L Interstate_Data").NodeIds[0]];
        Assert.Equal(-1000 * 0.8296, p.X, 9);
        Assert.Equal(0, p.Z, 9);
        Assert.Equal(100, p.Y, 9);
    }

    [Fact]
    public void Lane_policy_steers_e2c_resolver()
    {
        var net = ConvertFixture();
        var interstate = Way(net, "2452038713.USRP 2L Interstate_Data");
        Assert.True(interstate.OneWay);
        Assert.Equal(2, interstate.SourceLanes);
        Assert.Equal(3, interstate.Lanes); // 2 would make E2C fall back to two-way Small Road
        Assert.Equal(1, Way(net, "2452038713.USRP 1L Interstate Ramp_Data").Lanes);
        var national = Way(net, "2097530472.USRP 2-Lane National Road_Data");
        Assert.False(national.OneWay);
        Assert.Equal(2, national.Lanes);
        Assert.Equal(3, Way(net, "2452038713.USRP 3L Interstate T").Lanes); // name fallback "3L"
        Assert.Equal("Highway Oneway - 3 lanes", Converter.PredictE2cPrefab(NetClass.Motorway, true, 3));
    }

    [Fact]
    public void Backward_only_one_way_is_reversed()
    {
        var net = ConvertFixture();
        var w = Way(net, "Oneway Road Backward Test");
        Assert.True(w.OneWay);
        Assert.Equal(24L, w.NodeIds[0]); // CS1 node 23 first
        Assert.Equal(23L, w.NodeIds[^1]);
        Assert.Equal(1, net.Stats.ReversedOneWays);
        Assert.Equal(23L, Way(ConvertFixture(new ConvertOptions { ReverseOneWays = true }), w.Prefab).NodeIds[0]);
    }

    [Fact]
    public void Tunnels_are_flagged_and_can_be_dropped()
    {
        Assert.Equal(Structure.Tunnel, Way(ConvertFixture(), "2452038713.USRP 3L Interstate T").Structure);
        var dropped = ConvertFixture(new ConvertOptions { DropTunnels = true });
        Assert.DoesNotContain(dropped.Ways, w => w.Structure == Structure.Tunnel);
        Assert.Equal(1, dropped.Stats.DroppedTunnels);
    }

    [Fact]
    public void Simplification_keeps_endpoints_and_spacing()
    {
        var ramp = Way(ConvertFixture(), "2452038713.USRP 1L Interstate Ramp_Data");
        var net = ConvertFixture();
        var pts = Way(net, ramp.Prefab).NodeIds.Select(id => net.Nodes[id]).ToList();
        Assert.InRange(pts.Count, 3, 9); // 10 input points on a 50 m quarter circle
        for (int i = 1; i < pts.Count; i++)
            Assert.True(Geometry.Dist(pts[i - 1], pts[i]) >= 8 - 1e-9);
        Assert.Equal(-800 * 0.8296, pts[0].X, 6);
        Assert.Equal(50 * 0.8296, pts[^1].Z, 6);
    }

    [Fact]
    public void Border_connection_lands_on_the_playable_edge_and_is_counted()
    {
        var net = ConvertFixture();
        var end = net.Nodes[Way(net, "2452038713.USRP 3L Interstate_Data").NodeIds[^1]];
        Assert.Equal(8640 * 0.8296, end.X, 6);
        Assert.True(end.X <= 7168);
        Assert.True(net.Stats.EdgesNearBoundary >= 1);
    }

    [Theory]
    [InlineData(0.0, 0.0, 0.01)]
    [InlineData(31.25, -34.75, 0.6)] // E2C's Atlantic default centre: float32 lat/lon costs ~0.2-0.5 m
    public void Projection_round_trips_through_e2c_model(double lat0, double lon0, double tolerance)
    {
        var proj = new E2cProjection(lat0, lon0);
        for (double x = -7100; x <= 7100; x += 710)
            for (double z = -7100; z <= 7100; z += 710)
            {
                var (lat, lon) = proj.ToGeo(x, z);
                var (gx, gz) = proj.SimulateE2c(lat, lon);
                Assert.True(Math.Abs(gx - x) < tolerance && Math.Abs(gz - z) < tolerance, $"({x},{z}) -> ({gx},{gz})");
            }
    }

    [Fact]
    public void Projection_clamps_outside_points_like_e2c()
    {
        var proj = new E2cProjection();
        var (lat, lon) = proj.ToGeo(9000, -9000);
        var (x, z) = proj.SimulateE2c(lat, lon);
        Assert.Equal(7168f, x, 0.01f);
        Assert.Equal(-7168f, z, 0.01f);
    }

    [Fact]
    public void Osm_output_matches_e2c_reader_expectations()
    {
        var net = ConvertFixture();
        var proj = new E2cProjection();
        var dir = Directory.CreateTempSubdirectory("cslmap2e2c-");
        try
        {
            var osmPath = Path.Combine(dir.FullName, "overpass_roads.osm");
            Writers.WriteOsm(osmPath, net, proj);
            var doc = XDocument.Load(osmPath);
            Assert.Equal("osm", doc.Root!.Name.LocalName);
            var b = doc.Root.Element("bounds")!;
            Assert.Equal(-(double)b.Attribute("minlat")!, (double)b.Attribute("maxlat")!, 6);

            var nodes = doc.Root.Elements("node").ToDictionary(n => (string)n.Attribute("id")!, n => ((double)n.Attribute("lat")!, (double)n.Attribute("lon")!));
            var ways = doc.Root.Elements("way").ToList();
            Assert.Equal(net.Ways.Count, ways.Count);
            foreach (var w in ways)
            {
                var tags = w.Elements("tag").ToDictionary(t => (string)t.Attribute("k")!, t => (string)t.Attribute("v")!);
                Assert.Contains(tags["highway"], new[] { "motorway", "motorway_link", "trunk", "primary", "secondary", "tertiary", "residential" });
                Assert.All(w.Elements("nd"), nd => Assert.True(nodes.ContainsKey((string)nd.Attribute("ref")!)));
            }

            // Interstate start node: x = -829.6 m -> lon = -829.6 / 111320, lat = 0.
            var (lat, lon) = nodes["11"];
            Assert.Equal(0, lat, 12);
            Assert.Equal(-1000 * 0.8296 / 111320.0, lon, 9);
            var gravel = ways.Single(w => w.Elements("tag").Any(t => (string)t.Attribute("v")! == "Gravel Road"));
            Assert.Contains(gravel.Elements("tag"), t => (string)t.Attribute("k")! == "name" && (string)t.Attribute("v")! == "Old Mill Road");
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public void Roads_json_follows_e2c_schema_with_junctions()
    {
        var net = ConvertFixture();
        var dir = Directory.CreateTempSubdirectory("cslmap2e2c-");
        try
        {
            var path = Path.Combine(dir.FullName, "roads.json");
            Writers.WriteRoadsJson(path, net, new E2cProjection());
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            Assert.Equal("WGS84", root.GetProperty("meta").GetProperty("crs").GetString());
            Assert.Equal(4, root.GetProperty("meta").GetProperty("bbox").GetArrayLength());
            var roads = root.GetProperty("roads").EnumerateArray().ToList();
            Assert.Equal(net.Ways.Count, roads.Count);
            var first = roads[0];
            Assert.Equal("motorway", first.GetProperty("category").GetString());
            Assert.True(first.GetProperty("one_way").GetBoolean());
            Assert.Equal(3, first.GetProperty("lanes").GetInt32());
            var pts = first.GetProperty("points").EnumerateArray().ToList();
            Assert.True(pts[^1].GetProperty("junction").GetBoolean()); // shared with the next segment
            Assert.False(pts[0].TryGetProperty("junction", out _));
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public void Rail_geojson_keeps_cs2_metres_and_height()
    {
        var net = ConvertFixture();
        var path = Path.Combine(Path.GetTempPath(), $"rail-{Guid.NewGuid():N}.geojson");
        try
        {
            Writers.WriteRailGeoJson(path, net, Opt.Scale);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var f = doc.RootElement.GetProperty("features")[0];
            var c = f.GetProperty("geometry").GetProperty("coordinates")[2];
            Assert.Equal(Math.Round(90 * 0.8296, 3), c[0].GetDouble(), 6);
            Assert.Equal(Math.Round(-500 * 0.8296, 3), c[1].GetDouble(), 6);
            Assert.Equal(131, c[2].GetDouble(), 6);
            Assert.Equal("2610252544.R2USA W GrWo 2x2_Data", f.GetProperty("properties").GetProperty("prefab").GetString());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Calibration_cross_has_expected_geometry()
    {
        var net = Calibration.Build(250);
        Assert.Equal(3, net.Ways.Count);
        Assert.All(net.Ways, w => Assert.Contains(1L, w.NodeIds)); // all meet at the origin node
        var x = net.Ways.Single(w => w.Id == 1);
        var z = net.Ways.Single(w => w.Id == 2);
        var d = net.Ways.Single(w => w.Id == 3);
        Assert.Equal(21, x.NodeIds.Count);
        Assert.Equal((-2500.0, 0.0), (net.Nodes[x.NodeIds[0]].X, net.Nodes[x.NodeIds[0]].Z));
        Assert.Equal((0.0, 2500.0), (net.Nodes[z.NodeIds[^1]].X, net.Nodes[z.NodeIds[^1]].Z));
        var end = net.Nodes[d.NodeIds[^1]];
        Assert.Equal(5000, Math.Sqrt(end.X * end.X + end.Z * end.Z), 6);
        Assert.Equal(45, Math.Atan2(end.Z, end.X) * 180 / Math.PI, 9);
        Assert.Equal(1L, d.NodeIds[0]);
        Assert.True(d.OneWay);
        Assert.NotEqual(x.Class, z.Class);
    }

    [Fact]
    public void Clip_drops_outside_and_cuts_crossing_pieces()
    {
        // Fixture border road lies wholly outside ±7168 at scale 1 -> dropped.
        var net = ConvertFixture(new ConvertOptions { Scale = 1.0, Clip = true });
        Assert.DoesNotContain(net.Ways, w => w.Prefab == "2452038713.USRP 3L Interstate_Data");
        Assert.Equal(1, net.Stats.ClippedAway);
        Assert.Equal(7, net.Ways.Count);
        Assert.All(net.Nodes.Values, p => Assert.True(Math.Abs(p.X) <= 7168 && Math.Abs(p.Z) <= 7168));
    }

    [Fact]
    public void ClipToSquare_ends_on_border_with_interpolated_height()
    {
        var runs = Geometry.ClipToSquare([(7000, 10, 0), (7100, 20, 0), (7300, 40, 0)], 7168);
        var r = Assert.Single(runs);
        Assert.True(r.StartKept);
        Assert.False(r.EndKept);
        Assert.Equal(3, r.Points.Count);
        Assert.Equal(7168, r.Points[^1].X, 9);
        Assert.Equal(20 + 20 * 68 / 200.0, r.Points[^1].Y, 9);
    }

    [Fact]
    public void ClipToSquare_splits_out_and_back_in()
    {
        var runs = Geometry.ClipToSquare([(7000, 0, 0), (7300, 0, 0), (7300, 0, 500), (7000, 0, 500)], 7168);
        Assert.Equal(2, runs.Count);
        Assert.Equal((7168.0, 0.0), (runs[0].Points[^1].X, runs[0].Points[^1].Z));
        Assert.Equal((7168.0, 500.0), (runs[1].Points[0].X, runs[1].Points[0].Z));
        Assert.False(runs[1].StartKept);
        Assert.True(runs[1].EndKept);
        Assert.Empty(Geometry.ClipToSquare([(8000, 0, 0), (9000, 0, 0)], 7168));
    }

    [Fact]
    public void Clipped_way_gets_a_fresh_border_node_and_keeps_inner_cs1_node()
    {
        var map = CslMap.Load(Fixture);
        map.Nodes[60] = map.Nodes[60] with { X = 7000 }; // make the border road cross 7168
        var seg = map.Segments.Single(s => s.Id == 10);
        map.Segments[map.Segments.IndexOf(seg)] = seg with { Points = [(7000, 80, 100), (8640, 80, 100)] };
        var net = Converter.Convert(map, new Classifier(), new ConvertOptions { Scale = 1.0, Clip = true });
        var w = Way(net, "2452038713.USRP 3L Interstate_Data");
        Assert.Equal(61L, w.NodeIds[0]);          // CS1 node 60 survives
        Assert.True(w.NodeIds[^1] >= 1_000_000_000); // cut point is a new node
        Assert.Equal(7168, net.Nodes[w.NodeIds[^1]].X, 9);
        Assert.Equal(1, net.Stats.ClippedAtBorder);
    }
}
