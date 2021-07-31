using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FieldMapperForDotNet
{
    /// <summary>
    /// This is the main class for mapping field data from string content
    /// </summary>
    public class FieldMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public IDictionary<string, string> Get(string content, IEnumerable<string> mappings)
        {
            Validate();

            var result = new Dictionary<string, string>();

            content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");

            SeparateMappingsByLineBreaks();

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
                            var startIndex = GetIndexOfKey(mapping);

                            var nextLineValue = GetMappingValueOnSubsequentLines(content.Substring(startIndex + line.Length));
                            
                            if (nextLineValue != "")
                            {
                                value += " " + nextLineValue;
                            }

                            value = value.Trim();

                            if (result.ContainsKey(mapping))
                            {
                                result[mapping] = value;
                            }
                            else
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

            int GetIndexOfKey(string searchKey)
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

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            result += line + " | ";
                        }

                        line = reader.ReadLine();
                    }
                }

                return result.Trim(' ', '|');
            }

            void SeparateMappingsByLineBreaks()
            {
                foreach (var searchMapping in mappings)
                {
                    var startIndex = GetIndexOfKey(searchMapping);
                    var nextLocation = int.MaxValue;

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