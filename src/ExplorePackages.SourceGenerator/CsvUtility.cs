﻿using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Knapcode.ExplorePackages
{
    internal static class CsvUtility
    {
        public static void WriteWithQuotes(TextWriter writer, string value)
        {
            if (value == null)
            {
                return;
            }

            if (value.StartsWith(" ")
                || value.EndsWith(" ")
                || value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) > -1)
            {
                writer.Write('"');
                writer.Write(value.Replace("\"", "\"\""));
                writer.Write('"');
            }
            else
            {
                writer.Write(value);
            }
        }

        public static async Task WriteWithQuotesAsync(TextWriter writer, string value)
        {
            if (value == null)
            {
                return;
            }

            if (value.StartsWith(" ")
                || value.EndsWith(" ")
                || value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) > -1)
            {
                await writer.WriteAsync('"');
                await writer.WriteAsync(value.Replace("\"", "\"\""));
                await writer.WriteAsync('"');
            }
            else
            {
                await writer.WriteAsync(value);
            }
        }

        public static T ParseReference<T>(string input, Func<string, T> parse) where T : class
        {
            return string.IsNullOrEmpty(input) ? null : parse(input);
        }

        public static T? ParseNullable<T>(string input, Func<string, T> parse) where T : struct
        {
            return string.IsNullOrEmpty(input) ? new T?() : parse(input);
        }

        public static string FormatBool(bool input)
        {
            return input ? "true" : "false";
        }

        public static string FormatBool(bool? input)
        {
            if (input.HasValue)
            {
                return FormatBool(input.Value);
            }

            return string.Empty;
        }

        public static string FormatDateTimeOffset(DateTimeOffset input)
        {
            return input.ToString("O", CultureInfo.InvariantCulture);
        }

        public static string FormatDateTimeOffset(DateTimeOffset? input)
        {
            return input?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static DateTimeOffset ParseDateTimeOffset(string input)
        {
            return DateTimeOffset.ParseExact(input, "O", CultureInfo.InvariantCulture);
        }
    }
}
