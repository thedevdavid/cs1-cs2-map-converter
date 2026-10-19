using System;
using System.IO;

namespace DakotaExport
{
    /// <summary>
    /// Writes a file through "&lt;path&gt;.tmp" and renames it on success, so a failed or interrupted
    /// write never leaves a truncated file that looks complete. On failure the .tmp is deleted.
    /// </summary>
    internal static class SafeFile
    {
        public static void Write(string path, Action<string> writeTo)
        {
            string tmp = path + ".tmp";
            try
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                writeTo(tmp);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch
            {
                try { if (File.Exists(tmp)) File.Delete(tmp); }
                catch { }
                throw;
            }
        }

        public static T Write<T>(string path, Func<string, T> writeTo)
        {
            T result = default(T);
            Write(path, tmp => { result = writeTo(tmp); });
            return result;
        }

        /// <summary>UTF-8 (no BOM) text writer on "&lt;path&gt;.tmp"; renamed to path on Dispose only after Commit().</summary>
        public static SafeTextWriter CreateText(string path)
        {
            return new SafeTextWriter(path);
        }
    }

    internal sealed class SafeTextWriter : StreamWriter
    {
        private readonly string m_path;
        private readonly string m_tmp;
        private bool m_committed;
        private bool m_done;

        public SafeTextWriter(string path)
            : base(path + ".tmp", false, new System.Text.UTF8Encoding(false), 1 << 16)
        {
            m_path = path;
            m_tmp = path + ".tmp";
        }

        /// <summary>Marks the content complete; the file is moved into place when the writer is disposed.</summary>
        public void Commit()
        {
            m_committed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing || m_done) return;
            m_done = true;
            if (m_committed)
            {
                if (File.Exists(m_path)) File.Delete(m_path);
                File.Move(m_tmp, m_path);
            }
            else
            {
                try { if (File.Exists(m_tmp)) File.Delete(m_tmp); }
                catch { }
            }
        }
    }
}
