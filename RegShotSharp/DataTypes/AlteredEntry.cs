using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegShotSharp.DataTypes
{
    /// <summary>
    /// Class for Registry entries that were added or removed between snapshots (could probably be absorbed into RegistryEntry)
    /// </summary>
    public class AlteredEntry
    {
        public bool WasAdded { get; }
        public string Key { get; } //Really the path
        public string Path { get; } //The name of the entry

        public AlteredEntry(bool a_added, string a_key, string a_path)
        {
            WasAdded = a_added;
            Key = a_key;
            Path = a_path;
        }

        public string ToCsvString() => $"{(WasAdded ? "ADDED" : "DELETED")}, {Key}, {Path}";

        public static string GetCsvHeader() => "Event, Path, Entry name";
    }
}
