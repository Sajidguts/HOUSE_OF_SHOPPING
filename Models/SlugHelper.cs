using System.Text.RegularExpressions;

namespace HouseOfWani.Models
{
    public static class SlugHelper
    {
        public static string ToSlug(string phrase)
        {
            // Remove invalid characters
            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, Math.Min(str.Length, 100)); // Limit to 100 characters
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }
    }
}
