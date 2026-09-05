using System.Text;

namespace RegShotSharp.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Formats string for usage; removes trailing newlines and wraps the value if it contains a comma
        /// (MAY NOT BEE NEEDED ANYMORE)
        /// </summary>
        /// <param name="str">String value to format</param>
        /// <returns>A formated string</returns>
        public static string FormatString(this string str)
        {
            str = str.TrimEnd('\r', '\n');
            if (str.Contains(',')) return $"\"{str}\"";
            return str;
        }

        /// <summary>
        /// Converts [unicode] string to Base64 string
        /// </summary>
        /// <param name="str">String value</param>
        /// <returns>Base64 string</returns>
        public static string Base64Encode(this string str)
        {
            byte[] stringBytes = Encoding.Unicode.GetBytes(str);
            return Convert.ToBase64String(stringBytes);
        }

        /// <summary>
        /// Converts Base64 string to [unicode] string
        /// </summary>
        /// <param name="str">Base64 string</param>
        /// <returns>[unicode] string</returns>
        public static string Base64Decode(this string str)
        {
            byte[] stringBytes = Convert.FromBase64String(str);
            return Encoding.Unicode.GetString(stringBytes);
        }
    }
}
