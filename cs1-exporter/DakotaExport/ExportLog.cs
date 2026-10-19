using System;
using System.IO;
using UnityEngine;

namespace DakotaExport
{
    /// <summary>
    /// Writes to output_log.txt and, during an export, to a sidecar .log file.
    /// </summary>
    internal sealed class ExportLog : IDisposable
    {
        private const string Prefix = "[DakotaExport] ";

        private readonly StreamWriter m_file;

        public ExportLog(string path)
        {
            m_file = new StreamWriter(path, false, new System.Text.UTF8Encoding(false));
            m_file.AutoFlush = true;
        }

        public static void Unity(string message)
        {
            Debug.Log(Prefix + message);
        }

        public void Info(string message)
        {
            Unity(message);
            m_file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
        }

        public void Dispose()
        {
            m_file.Dispose();
        }
    }
}
