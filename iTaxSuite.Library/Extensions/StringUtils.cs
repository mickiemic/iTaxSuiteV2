using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace iTaxSuite.Library.Extensions
{
    public static class StringUtils
    {
        public static bool IsIntegral(string input)
        {
            long value;
            if (!string.IsNullOrWhiteSpace(input))
            {
                return long.TryParse(input, out value);
            }
            return false;
        }

        public static bool IsIntegral(string input, out long value)
        {
            value = 0L;
            if (!string.IsNullOrWhiteSpace(input))
            {
                return long.TryParse(input, out value);
            }
            return false;
        }

        public static bool IsDecimal(string input)
        {
            decimal value;
            if (!string.IsNullOrWhiteSpace(input))
            {
                return decimal.TryParse(input, out value);
            }
            return false;
        }

        public static bool IsDecimal(string input, out decimal value)
        {
            value = default(decimal);
            if (!string.IsNullOrWhiteSpace(input))
            {
                return decimal.TryParse(input, out value);
            }
            return false;
        }

        public static bool IsAlphabet(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^[a-zA-Z]+$");
        }

        public static bool IsAlphaNum(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^[a-zA-Z0-9]+$");
        }

        public static string NullIfEmpty(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input;
            }
            return null;
        }

        public static string NullIfWhiteSpace(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            return null;
        }

        public static string EmptyIfNull(this string input)
        {
            return input ?? string.Empty;
        }

        public static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name.Trim();
            }
            name = Regex.Replace(name, "(\\.)([A-Za-z])", "$1 $2");
            name = Regex.Replace(name, ",", ".");
            name = Regex.Replace(name, "(\\s)(\\.)(\\s)", "$2$3");
            name = Regex.Replace(name, "(\\w{2,})(\\.)", "$1");
            name = Regex.Replace(name, "[\\s]{2,}", " ");
            return name.Trim();
        }

        public static string WithMaxLength(this string input, int maxLength, bool trim = false)
        {
            if (trim)
            {
                return input?.Substring(0, Math.Min(input.Trim().Length, maxLength)).Trim();
            }
            return input?.Substring(0, Math.Min(input.Length, maxLength));
        }

        public static string SafeTrim(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input.Trim();
            }
            return input;
        }

        public static string ToTitleCase(this string input)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
        public static string InsertXterByPos(this string input, int position, string separator)
        {
            if (string.IsNullOrWhiteSpace(input) || position <= 0 || string.IsNullOrWhiteSpace(separator))
                return input;
            var output = Regex.Replace(input, $".{{{position}}}", $"$0{separator}");
            if (output.EndsWith(separator))
                output = output.Substring(0, (output.Length - separator.Length));
            return output;
        }
    }

    public static class LinqExtensions
    {
        public static List<List<T>> Split<T>(this IList<T> source, int length)
        {
            return Enumerable
                .Range(0, (source.Count + length - 1) / length)
                .Select(n => source.Skip(n * length).Take(length).ToList())
                .ToList();
        }
    }

    public static class EnumExtensions
    {
        public static string GetEnumMemberValue(this Enum enumValue)
        {
            Type type = enumValue.GetType();
            MemberInfo[] memInfo = type.GetMember(enumValue.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return ((EnumMemberAttribute)attributes[0]).Value;
                }
            }
            return null; // Or throw an exception if no EnumMemberAttribute is found
        }
    }

}
