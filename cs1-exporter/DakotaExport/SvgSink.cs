using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Collects geometry during the export and writes a quick-look SVG.
    /// SVG y = -CS1 z so north is up; viewBox covers the full ±8640 m map.
    /// </summary>
    internal sealed class SvgSink
    {
        private readonly Dictionary<string, StringBuilder> m_byService = new Dictionary<string, StringBuilder>();
        private readonly StringBuilder m_markers = new StringBuilder();

        public void Segment(Vector3 a, Vector3 b, Vector3 c, Vector3 d, string service, float halfWidth)
        {
            StringBuilder sb;
            if (!m_byService.TryGetValue(service, out sb))
            {
                sb = new StringBuilder();
                m_byService[service] = sb;
            }

            float width = Mathf.Max(4f, halfWidth * 2f);
            sb.Append("<path stroke-width=\"").Append(F(width)).Append("\" d=\"M")
              .Append(P(a)).Append(" C").Append(P(b)).Append(' ').Append(P(c)).Append(' ').Append(P(d))
              .Append("\"/>\n");
        }

        public void Water(Vector3 p, ushort type)
        {
            string fill = type == WaterSource.TYPE_NATURAL ? "#1e6fff" : "#00b7c7";
            m_markers.Append("<circle r=\"60\" fill=\"").Append(fill).Append("\" fill-opacity=\"0.8\" cx=\"")
                     .Append(F(p.x)).Append("\" cy=\"").Append(F(-p.z)).Append("\"/>\n");
        }

        public void OutsideNode(Vector3 p)
        {
            m_markers.Append("<rect width=\"120\" height=\"120\" fill=\"none\" stroke=\"#ff0000\" stroke-width=\"20\" x=\"")
                     .Append(F(p.x - 60f)).Append("\" y=\"").Append(F(-p.z - 60f)).Append("\"/>\n");
        }

        public void OutsideBuilding(Vector3 p)
        {
            m_markers.Append("<circle r=\"90\" fill=\"#ff0000\" cx=\"")
                     .Append(F(p.x)).Append("\" cy=\"").Append(F(-p.z)).Append("\"/>\n");
        }

        public void Write(string path, string title)
        {
            using (var w = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                w.Write("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"-8640 -8640 17280 17280\" width=\"2160\" height=\"2160\">\n");
                w.Write("<title>" + Escape(title) + "</title>\n");
                w.Write("<rect x=\"-8640\" y=\"-8640\" width=\"17280\" height=\"17280\" fill=\"#f4f1ea\"/>\n");
                var services = new List<string>(m_byService.Keys);
                services.Sort(System.StringComparer.Ordinal);
                int legendY = -8500;
                foreach (string service in services)
                {
                    string colour = Colour(service);
                    w.Write("<g fill=\"none\" stroke-linecap=\"round\" stroke=\"" + colour + "\" data-service=\"" + Escape(service) + "\">\n");
                    w.Write(m_byService[service].ToString());
                    w.Write("</g>\n");
                    w.Write("<text x=\"-8500\" y=\"" + legendY + "\" font-size=\"140\" fill=\"" + colour + "\">" + Escape(service) + "</text>\n");
                    legendY += 170;
                }

                w.Write(m_markers.ToString());
                w.Write("</svg>\n");
            }
        }

        private static string Colour(string service)
        {
            if (service.StartsWith("Road")) return "#555555";
            if (service.Contains("Train") || service.Contains("Metro")) return "#8b4513";
            if (service.Contains("Ship")) return "#0050a0";
            if (service.Contains("Plane")) return "#a000a0";
            if (service.Contains("Tram") || service.Contains("Bus")) return "#c08000";
            if (service.StartsWith("Electricity")) return "#e0b000";
            if (service.StartsWith("Water")) return "#2090ff";
            if (service.StartsWith("Beautification")) return "#40a040";
            if (service.StartsWith("PublicTransport")) return "#d04040";
            return "#999999";
        }

        private static string P(Vector3 v)
        {
            return F(v.x) + "," + F(-v.z);
        }

        private static string F(float f)
        {
            return f.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string Escape(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
