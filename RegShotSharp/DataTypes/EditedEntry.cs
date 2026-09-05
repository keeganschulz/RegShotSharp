using RegShotSharp.Extensions;

namespace RegShotSharp.DataTypes
{
    /// <summary>
    /// Class for Registry entries where values/value types were changed between snapshots (could probably be absorbed into RegistryEntry)
    /// </summary>
    public class EditedEntry
    {
        public string Path { get; set; }
        public object OldValue { get; }
        public object NewValue { get; }
        public string OldValueType { get; }
        public string NewValueType { get; }

        public EditedEntry(string a_path, string a_oldValue, string a_newValue, string a_oldValueType, string a_newValueType)
        {
            Path = a_path;
            OldValue = DecodeSavedRegistryValue(a_oldValueType, a_oldValue);
            NewValue = DecodeSavedRegistryValue(a_newValueType, a_newValue);
            OldValueType = RegistryTypeSymbolToName(a_oldValueType);
            NewValueType = RegistryTypeSymbolToName(a_newValueType);
        }

        public string ToCsvString() => $"{Path},{OldValueType},{OldValue},{NewValueType},{NewValue}";

        public static string CsvHeader() => "Path,Old Value Type, Old Value, New Value Type, New Value";

        // This may not belong here
        private string RegistryTypeSymbolToName(string a_value)
        {
            switch (a_value)
            {
                case "B":
                    return "REG_BINARY";
                case "D":
                    return "REG_DWORD";
                case "E":
                    return "REG_EXPAND_SZ";
                case "M":
                    return "REG_MULTI_SZ";
                case "Q":
                    return "REG_QWORD";
                case "S":
                    return "REG_SZ";
                case "U":
                    return "Unknown";
                default:
                    return "None";
            }
        }

        // This may also not belong here
        public object DecodeSavedRegistryValue(string a_typeCode, string a_value)
        {
            switch (a_typeCode)
            {
                case "E":
                case "M":
                case "S":
                    return a_value.Base64Decode();
                case "B":
                case "U":
                    return a_value;
                case "D":
                    return int.Parse(a_value); //(int)a_savedValue;
                case "Q":
                    return Int64.Parse(a_value); //(Int64)a_savedValue;
                default:
                    return a_value;
            }
        }
    }
}
