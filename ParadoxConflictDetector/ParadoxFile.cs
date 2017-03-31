using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParadoxConflictDetector
{
    internal sealed class ParadoxFile
    {
        private readonly List<ParadoxElement> elements;

        private ParadoxFile(List<ParadoxElement> elements)
        {
            this.elements = elements;
        }

        public static ParadoxFile Parse(string filePath)
        {
            using (var reader = new StreamReader(filePath))
                return Parse(reader);
        }

        public static ParadoxFile Parse(StreamReader reader)
        {
            var elements = new List<ParadoxElement>();
            while (!reader.EndOfStream)
            {
                var element = ParseElement(reader);
                if (element == null)
                    break;

                elements.Add(element);
            }

            return new ParadoxFile(elements);
        }

        private static ParadoxElement ParseElement(TextReader reader)
        {
            if (!ParseWhitespace(reader))
                return null;

            if (!ParseValue(reader, out var key))
                return string.IsNullOrEmpty(key) ? null : new ParadoxElement(key);

            if (!ParseWhitespace(reader))
                return string.IsNullOrEmpty(key) ? null : new ParadoxElement(key);

            if (reader.Peek() != '=')
                return string.IsNullOrEmpty(key) ? null : new ParadoxElement(key);

            reader.Read();

            if (!ParseWhitespace(reader))
                return new ParadoxElement(key, "");

            if (reader.Peek() == '{')
            {
                reader.Read();

                var children = new List<ParadoxElement>();
                while (true)
                {
                    var chr = reader.Peek();
                    if (chr == -1)
                        break;

                    if (chr == '}')
                    {
                        reader.Read();
                        break;
                    }

                    var element = ParseElement(reader);
                    if (element != null)
                        children.Add(element);
                }

                return new ParadoxElement(key, children);
            }

            ParseValue(reader, out var value);
            return new ParadoxElement(key, value);
        }

        private static bool ParseValue(TextReader reader, out string value)
        {
            var builder = new StringBuilder();
            var insideQuotes = false;
            while (true)
            {
                var next = reader.Peek();
                if (next == -1)
                {
                    value = builder.Length == 0 ? null : builder.ToString();
                    return false;
                }

                if (next == '"' && builder.Length == 0)
                {
                    insideQuotes = true;
                    reader.Read();
                }
                else if (next == '"' && insideQuotes)
                {
                    reader.Read();
                    value = builder.Length == 0 ? null : builder.ToString();
                    return true;
                }
                else
                {
                    if (char.IsWhiteSpace((char)next) && !insideQuotes || next == '=' || next == '{' || next == '}' || next == '"')
                    {
                        value = builder.Length == 0 ? null : builder.ToString();
                        return true;
                    }

                    builder.Append((char)next);
                    reader.Read();
                }
            }
        }

        private static bool ParseWhitespace(TextReader reader)
        {
            while (true)
            {
                var next = reader.Peek();
                if (next == -1)
                    return false;

                if (!char.IsWhiteSpace((char)next))
                    return true;

                reader.Read();
            }
        }

        public ParadoxElement this[string key]
        {
            get { return elements.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)); }
        }
    }
}