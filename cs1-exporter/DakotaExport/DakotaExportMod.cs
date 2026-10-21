using ICities;

namespace DakotaExport
{
    /// <summary>
    /// CS1 discovers this type through IUserMod. The mod is read-only: it never writes game state.
    /// </summary>
    public sealed class DakotaExportMod : IUserMod
    {
        public const string Version = "0.5.0";

        public string Name
        {
            get { return "Dakota Export"; }
        }

        public string Description
        {
            get { return "Read-only dump of networks, water sources, buildings, trees and props to JSON (v" + Version + ")."; }
        }
    }
}
