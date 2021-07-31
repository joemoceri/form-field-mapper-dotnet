using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FieldMapperForDotNet
{
    public class FieldMapper
    {
        private readonly FieldMapperConfiguration configuration;

        public FieldMapper(): this(new FieldMapperConfiguration()) { }

        public FieldMapper(FieldMapperConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string PreviewContent(string content, IEnumerable<string> mappings)
        {
            if (configuration.options.DeEntitizeContent)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            }

            if (configuration.options.SeparateByLineBreaks)
            {
                content = Regex.Replace(content, @"\s{5,}", Environment.NewLine);
            }

            content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");

            SeparateMappingsByLineBreaks();

            return content;

            void SeparateMappingsByLineBreaks()
            {
                foreach (var searchMapping in mappings)
                {
                    var startIndex = GetIndexOfKey(content, mappings, searchMapping);
                    var nextLocation = int.MaxValue;

                    if (!content.Contains(Environment.NewLine) && startIndex != -1)
                    {
                        content = content.Insert(startIndex, Environment.NewLine);
                    }

                    foreach (var key in mappings.Where(k => k != searchMapping))
                    {
                        var loc = content.IndexOf(key, startIndex + searchMapping.Length);

                        if (loc != -1 && loc < nextLocation)
                        {
                            nextLocation = loc;
                        }
                    }

                    if (nextLocation != int.MaxValue)
                    {
                        content = content.Insert(nextLocation, Environment.NewLine);
                    }
                }
            }
        }

        private int GetIndexOfKey(string content, IEnumerable<string> mappings, string searchKey)
        {
            var nestedKey = false;
            var nonSearchedKeys = mappings.Where(k => k != searchKey);

            foreach (var key in nonSearchedKeys)
            {
                if (key.Contains(searchKey))
                {
                    nestedKey = true;
                }
            }

            if (nestedKey)
            {
                var tempContent = content;

                var orderedKeys = mappings.OrderByDescending(m => m.Length).ToList();
                for (var i = 0; i < orderedKeys.Count(); i++)
                {
                    tempContent = tempContent.Replace(orderedKeys[i], orderedKeys[i].ToUpperInvariant());
                }

                var nonSearchedLargerKeys = nonSearchedKeys.Where(k => k.Length > searchKey.Length);

                foreach (var key in nonSearchedLargerKeys)
                {
                    tempContent = tempContent.Replace(key.ToUpperInvariant(), key.ToLowerInvariant());
                }

                return tempContent.IndexOf(searchKey.ToUpperInvariant());
            }
            else
            {
                return content.IndexOf(searchKey);
            }
        }

        public IDictionary<string, string> Get(string content, IEnumerable<string> mappings)
        {
            Validate();

            content = PreviewContent(content, mappings);

            var result = new Dictionary<string, string>();

            using (var reader = new StringReader(content))
            {
                var line = reader.ReadLine();
                var orderedMappings = mappings.OrderByDescending(m => m.Length).ToList();
                while (line != null)
                {
                    line = line.Trim();
                    for (var i = 0; i < orderedMappings.Count(); i++)
                    {
                        var mapping = orderedMappings[i];
                        if (line.Contains(mapping) && line.IndexOf(mapping) == 0)
                        {
                            var value = line.Substring(line.IndexOf(mapping) + mapping.Length).Trim();
                            var startIndex = GetIndexOfKey(content, mappings, mapping);

                            var nextLineValue = GetMappingValueOnSubsequentLines(content.Substring(startIndex + line.Length));

                            value = value.Trim();

                            if (!result.ContainsKey(mapping))
                            {
                                result.Add(mapping, value);
                            }

                            break;
                        }
                    }

                    line = reader.ReadLine();
                }
            }

            return result;

            string GetMappingValueOnSubsequentLines(string subContent)
            {
                var result = string.Empty;

                using (var reader = new StringReader(subContent))
                {
                    var line = reader.ReadLine();

                    while (line != null)
                    {
                        foreach (var mapping in mappings)
                        {
                            if (line.Contains(mapping))
                            {
                                return result.Trim(' ', '|');
                            }
                        }

                        line = reader.ReadLine();
                    }
                }

                return result.Trim(' ', '|');
            }

            void Validate()
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new ArgumentException("Content cannot be null or empty.", nameof(content));
                }

                if (mappings == null || mappings.Count() == 0)
                {
                    throw new ArgumentException("Mappings cannot be null or empty.", nameof(mappings));
                }
                
                if (mappings.Any(m => string.IsNullOrWhiteSpace(m)))
                {
                    throw new ArgumentException("Mappings cannot contain any empty values.", nameof(mappings));
                }

                if (mappings.Distinct().Count() != mappings.Count())
                {
                    throw new ArgumentException("Duplicate mappings found. Please make sure they are all unique.");
                }
            }
        }

    }
}