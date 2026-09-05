using Microsoft.Win32;
using RegShotSharp.Extensions;

namespace RegShotSharp.DataTypes
{
    /// <summary>
    /// Class to hold the registry values
    /// </summary>
    public class RegistryEntry
    {
        public bool SUCCESS { get; set; } = false;
        public string REGISTRY_KEY { get; set; }
        public string REGISTRY_PATH { get; set; }
        public string? REGISTRY_VALUE { get; set; }
        public string? REGISTRY_TYPE { get; set; }

        public RegistryEntry(RegistryKey a_registryKey, string a_registryPath) 
        {
            try
            {
                REGISTRY_KEY = a_registryKey.ToString();
                REGISTRY_PATH = a_registryPath;
                FormatRegistryValue(a_registryKey, a_registryPath);
                SUCCESS = true;
            } catch 
            { 
                SUCCESS = false;    
            }
        }

        public RegistryEntry(RegistryKey a_registryKey, string a_registryPath, bool a_success)
        {
            if (a_success)
            {
                REGISTRY_KEY = a_registryKey.ToString();
                REGISTRY_PATH = a_registryPath;
                FormatRegistryValue(a_registryKey, a_registryPath);
                SUCCESS = true;
            } else
            {
                SUCCESS = false;
                REGISTRY_KEY = a_registryKey.ToString();
                REGISTRY_PATH = a_registryPath;
            }
        }

        public void FormatRegistryValue(RegistryKey a_key, string a_name)
        {
            try
            {
                switch (a_key.GetValueKind(a_name))
                {
                    case RegistryValueKind.Binary:
                        REGISTRY_TYPE = "B";
                        REGISTRY_VALUE = BitConverter.ToString((byte[])a_key.GetValue(a_name));
                        break;
                    case RegistryValueKind.DWord:
                        REGISTRY_TYPE = "D";
                        REGISTRY_VALUE = a_key.GetValue(a_name).ToString();
                        break;
                    case RegistryValueKind.ExpandString:
                        REGISTRY_TYPE = "E";
                        REGISTRY_VALUE = ((string)a_key.GetValue(a_name)).FormatString().Base64Encode();
                        break;
                    case RegistryValueKind.MultiString:
                        REGISTRY_TYPE = "M";
                        REGISTRY_VALUE = string.Join(",", (string[])a_key.GetValue(a_name)).FormatString().Base64Encode();
                        break;
                    case RegistryValueKind.None:
                        REGISTRY_TYPE = "N";
                        REGISTRY_VALUE = "NONE";
                        break;
                    case RegistryValueKind.QWord:
                        REGISTRY_TYPE = "Q";
                        REGISTRY_VALUE = a_key.GetValue(a_name).ToString();
                        break;
                    case RegistryValueKind.String:
                        REGISTRY_TYPE = "S";
                        REGISTRY_VALUE = ((string)a_key.GetValue(a_name)).FormatString().Base64Encode();
                        break;
                    case RegistryValueKind.Unknown:
                        REGISTRY_TYPE = "U";
                        object rvalue = a_key.GetValue(a_name);
                        REGISTRY_VALUE = rvalue == null ? "CAN NOT READ" : BitConverter.ToString((byte[])a_key.GetValue(a_name));
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Issue with {a_key}/{a_name} --- " + e.Message);
            }
        }
    }
}