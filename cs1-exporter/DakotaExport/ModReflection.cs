using System;
using System.Reflection;

namespace DakotaExport
{
    /// <summary>Reflection helpers for optional mods; nothing here creates objects or calls mod code with side effects.</summary>
    internal static class ModReflection
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static Type FindType(string fullName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t;
                try { t = a.GetType(fullName, false); }
                catch { continue; }
                if (t != null) return t;
            }

            return null;
        }

        /// <summary>
        /// Returns ColossalFramework.Singleton&lt;T&gt;.instance only when Singleton&lt;T&gt;.exists is true,
        /// because reading .instance would otherwise create the singleton.
        /// </summary>
        public static object SingletonIfExists(Type t)
        {
            Type singleton = typeof(ColossalFramework.Singleton<>).MakeGenericType(t);
            PropertyInfo exists = singleton.GetProperty("exists", AnyStatic);
            if (exists != null && !(bool)exists.GetValue(null, null)) return null;
            PropertyInfo instance = singleton.GetProperty("instance", AnyStatic);
            return instance != null ? instance.GetValue(null, null) : null;
        }

        public static object StaticField(Type t, string name)
        {
            FieldInfo f = t.GetField(name, AnyStatic);
            return f != null ? f.GetValue(null) : null;
        }

        /// <summary>Instance field or property value, or null when absent.</summary>
        public static object Member(object o, string name)
        {
            if (o == null) return null;
            Type t = o.GetType();
            FieldInfo f = t.GetField(name, AnyInstance);
            if (f != null) return f.GetValue(o);
            PropertyInfo p = t.GetProperty(name, AnyInstance);
            if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(o, null);
            return null;
        }

        public static bool HasMember(object o, string name)
        {
            if (o == null) return false;
            Type t = o.GetType();
            return t.GetField(name, AnyInstance) != null || t.GetProperty(name, AnyInstance) != null;
        }
    }
}
