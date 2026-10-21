using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DakotaExport
{
    /// <summary>
    /// Minimal JSON reader for command files: objects (Dictionary&lt;string, object&gt;), arrays (List&lt;object&gt;),
    /// strings, numbers (double), true/false/null. Throws FormatException on malformed input.
    /// </summary>
    internal sealed class MiniJson
    {
        private readonly string m_s;
        private int m_i;

        private MiniJson(string s)
        {
            m_s = s;
        }

        public static object Parse(string text)
        {
            var p = new MiniJson(text);
            p.Ws();
            object v = p.Value();
            p.Ws();
            if (p.m_i != p.m_s.Length) throw new FormatException("trailing characters at " + p.m_i);
            return v;
        }

        private object Value()
        {
            if (m_i >= m_s.Length) throw new FormatException("unexpected end");
            char c = m_s[m_i];
            if (c == '{') return Obj();
            if (c == '[') return Arr();
            if (c == '"') return Str();
            if (Lit("true")) return true;
            if (Lit("false")) return false;
            if (Lit("null")) return null;
            return Num();
        }

        private Dictionary<string, object> Obj()
        {
            var d = new Dictionary<string, object>();
            m_i++;
            Ws();
            if (Peek('}')) { m_i++; return d; }
            while (true)
            {
                Ws();
                string k = Str();
                Ws();
                Expect(':');
                Ws();
                d[k] = Value();
                Ws();
                if (Peek(',')) { m_i++; continue; }
                Expect('}');
                return d;
            }
        }

        private List<object> Arr()
        {
            var l = new List<object>();
            m_i++;
            Ws();
            if (Peek(']')) { m_i++; return l; }
            while (true)
            {
                Ws();
                l.Add(Value());
                Ws();
                if (Peek(',')) { m_i++; continue; }
                Expect(']');
                return l;
            }
        }

        private string Str()
        {
            Expect('"');
            var sb = new StringBuilder();
            while (true)
            {
                if (m_i >= m_s.Length) throw new FormatException("unterminated string");
                char c = m_s[m_i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (m_i >= m_s.Length) throw new FormatException("bad escape");
                char e = m_s[m_i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (m_i + 4 > m_s.Length) throw new FormatException("bad \\u escape");
                        sb.Append((char)int.Parse(m_s.Substring(m_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        m_i += 4;
                        break;
                    default: throw new FormatException("bad escape \\" + e);
                }
            }
        }

        private double Num()
        {
            int start = m_i;
            while (m_i < m_s.Length && "+-0123456789.eE".IndexOf(m_s[m_i]) >= 0) m_i++;
            if (m_i == start) throw new FormatException("unexpected '" + m_s[m_i] + "' at " + m_i);
            return double.Parse(m_s.Substring(start, m_i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private bool Lit(string word)
        {
            if (string.CompareOrdinal(m_s, m_i, word, 0, word.Length) != 0) return false;
            m_i += word.Length;
            return true;
        }

        private bool Peek(char c)
        {
            return m_i < m_s.Length && m_s[m_i] == c;
        }

        private void Expect(char c)
        {
            if (!Peek(c)) throw new FormatException("expected '" + c + "' at " + m_i);
            m_i++;
        }

        private void Ws()
        {
            while (m_i < m_s.Length && char.IsWhiteSpace(m_s[m_i])) m_i++;
        }
    }
}
