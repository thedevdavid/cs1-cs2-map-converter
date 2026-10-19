using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Minimal streaming JSON writer (net35 has no serializer we can rely on in the game).
    /// Commas are tracked per container; numbers use the invariant culture.
    /// </summary>
    internal sealed class JsonWriter
    {
        private readonly TextWriter m_out;
        private readonly Stack<bool> m_first = new Stack<bool>();
        private bool m_afterName;

        public JsonWriter(TextWriter output)
        {
            m_out = output;
        }

        /// <summary>
        /// A writer that emits object members ("a":1,"b":2) without braces, for splicing into a parent
        /// object with <see cref="RawMembers"/>.
        /// </summary>
        public static JsonWriter Fragment(TextWriter output)
        {
            var w = new JsonWriter(output);
            w.m_first.Push(true);
            return w;
        }

        /// <summary>Splices members produced by a <see cref="Fragment"/> writer into the current object.</summary>
        public JsonWriter RawMembers(string members)
        {
            if (string.IsNullOrEmpty(members)) return this;
            Separate();
            m_out.Write(members);
            return this;
        }

        public JsonWriter Null() { Separate(); m_out.Write("null"); return this; }

        /// <summary>Writes a float rounded to two decimals (water levels etc.).</summary>
        public JsonWriter Value2(float value)
        {
            Separate();
            if (float.IsNaN(value) || float.IsInfinity(value)) m_out.Write("null");
            else m_out.Write(value.ToString("0.##", CultureInfo.InvariantCulture));
            return this;
        }

        public JsonWriter BeginObject() { Separate(); m_out.Write('{'); m_first.Push(true); return this; }
        public JsonWriter EndObject() { m_first.Pop(); m_out.Write('}'); return this; }
        public JsonWriter BeginArray() { Separate(); m_out.Write('['); m_first.Push(true); return this; }
        public JsonWriter EndArray() { m_first.Pop(); m_out.Write(']'); return this; }

        public JsonWriter Name(string name)
        {
            Separate();
            WriteString(name);
            m_out.Write(':');
            m_afterName = true;
            return this;
        }

        public JsonWriter Value(string value)
        {
            Separate();
            if (value == null) m_out.Write("null"); else WriteString(value);
            return this;
        }

        public JsonWriter Value(bool value) { Separate(); m_out.Write(value ? "true" : "false"); return this; }
        public JsonWriter Value(int value) { Separate(); m_out.Write(value.ToString(CultureInfo.InvariantCulture)); return this; }
        public JsonWriter Value(long value) { Separate(); m_out.Write(value.ToString(CultureInfo.InvariantCulture)); return this; }
        public JsonWriter Value(uint value) { Separate(); m_out.Write(value.ToString(CultureInfo.InvariantCulture)); return this; }

        public JsonWriter Value(float value)
        {
            Separate();
            if (float.IsNaN(value) || float.IsInfinity(value)) m_out.Write("null");
            else m_out.Write(value.ToString("R", CultureInfo.InvariantCulture));
            return this;
        }

        /// <summary>Writes a Vector3 as [x, y, z].</summary>
        public JsonWriter Value(Vector3 v)
        {
            BeginArray();
            Value(v.x); Value(v.y); Value(v.z);
            return EndArray();
        }

        public JsonWriter Prop(string name, string value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, bool value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, int value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, long value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, uint value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, float value) { return Name(name).Value(value); }
        public JsonWriter Prop(string name, Vector3 value) { return Name(name).Value(value); }

        public JsonWriter StringArray(string name, IEnumerable<string> values)
        {
            Name(name).BeginArray();
            foreach (string v in values) Value(v);
            return EndArray();
        }

        /// <summary>Emits a newline between top-level items to keep huge files diffable.</summary>
        public void NewLine()
        {
            m_out.Write('\n');
        }

        private void Separate()
        {
            if (m_afterName)
            {
                m_afterName = false;
                return;
            }

            if (m_first.Count == 0) return;
            if (m_first.Peek())
            {
                m_first.Pop();
                m_first.Push(false);
            }
            else
            {
                m_out.Write(',');
            }
        }

        private void WriteString(string s)
        {
            m_out.Write('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': m_out.Write("\\\""); break;
                    case '\\': m_out.Write("\\\\"); break;
                    case '\n': m_out.Write("\\n"); break;
                    case '\r': m_out.Write("\\r"); break;
                    case '\t': m_out.Write("\\t"); break;
                    default:
                        if (c < 0x20) m_out.Write("\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else m_out.Write(c);
                        break;
                }
            }

            m_out.Write('"');
        }
    }
}
