using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dafda.Configuration
{
    internal class ConfigurationReporter
    {
        public static ConfigurationReporter CreateDefault() => new();

        private readonly IList<Item> _items = new List<Item>();

        private class Item
        {
            public Item(string key, string source, string value, string keys, bool required = false)
            {
                Key = key;
                Source = source;
                Value = value;
                Keys = keys;
                Required = required;
            }

            public string Key { get; }
            public string Source { get; }
            public string Value { get; }
            public string Keys { get; }
            public bool Required { get; }
        }

        protected ConfigurationReporter()
        {
        }

        public virtual void AddMissing(string key, string source, params string[] attemptedKeys)
        {
            _items.Add(new Item(key, source, "MISSING", string.Join(", ", attemptedKeys), true));
        }

        public virtual void AddValue(string key, string source, string value, string acceptedKey)
        {
            _items.Add(new Item(key, source, value, acceptedKey, true));
        }

        public virtual void AddManual(string key, string value)
        {
            _items.Add(new Item(key, "MANUAL", value, "", false));
        }

        public virtual string Report()
        {
            var sb = new StringBuilder();

            var headers = new[] { " ", "key", "source", "value", "keys" };
            var rows = _items.Select(x => new[] { x.Required ? "R" : " ", x.Key, x.Source, x.Value, x.Keys }).ToList();

            var columns = headers.Length;

            var maxWidths = GetColumnWidths(headers, rows);
            var columnFormats = GetColumnFormats(columns, maxWidths);

            sb.AppendLine();
            AppendColumns(headers);

            sb.AppendLine(new string('-', maxWidths.Sum() + (columns - 1)));

            foreach (var row in rows)
            {
                AppendColumns(row);
            }

            return sb.ToString();

            void AppendColumns(string[] cols)
            {
                for (var i = 0; i < columnFormats.Length; i++)
                {
                    sb.AppendFormat(columnFormats[i], cols[i]);
                    if (i < columns - 1)
                    {
                        sb.Append(" ");
                    }
                }

                sb.AppendLine();
            }
        }

        private static int[] GetColumnWidths(string[] headers, List<string[]> rows)
        {
            var allRows = new List<string[]>(rows.Count + 1);
            allRows.Add(headers);
            allRows.AddRange(rows);

            var maxWidths = new int[headers.Length];

            foreach (var row in allRows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    maxWidths[i] = Math.Max(maxWidths[i], row[i]?.Length ?? 0);
                }
            }

            return maxWidths;
        }

        private static string[] GetColumnFormats(int columns, int[] maxWidths)
        {
            var columnFormats = new string[columns];

            for (var i = 0; i < maxWidths.Length; i++)
            {
                var maxWidth = maxWidths[i];

                if (i < columns - 1)
                {
                    columnFormats[i] = $"{{0,-{maxWidth}}}";
                }
                else
                {
                    columnFormats[i] = "{0}";
                }
            }

            return columnFormats;
        }
    }
}