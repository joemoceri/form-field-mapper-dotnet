using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FieldMapper
{
    public class FieldMapper
    {
        private string content;
        private readonly IEnumerable<string> mappings;

        public FieldMapper(string content, IEnumerable<string> mappings)
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

            this.content = content;
            this.mappings = mappings;
        }

        public IDictionary<string, string> Get()
        {
            var result = new Dictionary<string, string>();

            // Remove all line breaks first with spaces to ensure a clean separation
            content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");

            // Make sure every key is separated by lines
            SeparateMappingsByLineBreaks();

            using (var reader = new StringReader(content))
            {
                var line = reader.ReadLine();
                // Use an ordered set of mappings by length will handle nested keys at the beginning of the keys string (Ex: (Address) and (Address City))
                var orderedMappings = mappings.OrderByDescending(m => m.Length).ToList();
                while (line != null)
                {
                    line = line.Trim();
                    for (var i = 0; i < orderedMappings.Count(); i++)
                    {
                        var mapping = orderedMappings[i];
                        if (line.Contains(mapping) && line.IndexOf(mapping) == 0) // Make sure it's at the beginning to handle nested keys at the end of the key string (Ex: (First Name) and (Name))
                        {
                            var value = line.Substring(line.IndexOf(mapping) + mapping.Length).Trim();
                            var startIndex = GetIndexOfKey(mapping);

                            // This form field data might be on the next line, so I'll check
                            var nextLineValue = GetMappingValueOnSubsequentLines(content.Substring(startIndex + line.Length));
                            if (nextLineValue != "")
                                value += " " + nextLineValue;

                            value = value.Trim();

                            // If the field already exists, update it                            
                            if (result.ContainsKey(mapping))
                            {
                                result[mapping] = value;
                            }
                            // Otherwise add it
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
        }

        /// <summary>
        /// Separates every mapping by a line break. If there's already a line break, it will still add one.
        /// </summary>
        private void SeparateMappingsByLineBreaks()
        {
            // Make sure new lines separate every key in the form
            foreach (var searchMapping in mappings)
            {
                // Get the starting index of the key, so it can look ahead to find the next key
                // If it's the end of the content, the app will just take everything that's left
                var startIndex = GetIndexOfKey(searchMapping);

                var nextLocation = int.MaxValue;
                foreach (var key in mappings.Where(k => k != searchMapping)) // Look ahead for the next key using the keys except for the searched one (non-searched keys)
                {
                    var loc = content.IndexOf(key, startIndex + searchMapping.Length);
                    if (loc != -1 && loc < nextLocation)
                        nextLocation = loc;
                }

                if (nextLocation != int.MaxValue) // If it found a location, insert a new line
                    content = content.Insert(nextLocation, Environment.NewLine);
            }
        }

        /// <summary>
        /// Using the provided string, gather all values, line by line, until a mapping key is found.
        /// Once it's found, return all values found.
        /// </summary>
        /// <param name="subContent">The string to iterate over.</param>
        /// <returns><see cref="string"/></returns>
        private string GetMappingValueOnSubsequentLines(string subContent)
        {
            var result = string.Empty; // Set it to be empty, not null, simplifies the return statements

            using (var reader = new StringReader(subContent))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    foreach (var mapping in mappings)
                    {
                        if (line.Contains(mapping)) // If it found a key, return everything it found up to that point
                            return result.Trim(' ', '|');
                    }
                    if (!string.IsNullOrWhiteSpace(line)) // If the line contains information, add it
                        result += line + " | ";

                    line = reader.ReadLine();
                }
            }

            return result.Trim(' ', '|');
        }

        /// <summary>
        /// Find the index of a key in the class member content.
        /// </summary>
        /// <param name="searchKey">The key to look for.</param>
        /// <returns><see cref="string"/></returns>
        private int GetIndexOfKey(string searchKey)
        {
            var nestedKey = false;
            var nonSearchedKeys = mappings.Where(k => k != searchKey); // Only use non-searched keys

            foreach (var key in nonSearchedKeys)
            {
                if (key.Contains(searchKey)) // The searchKey is found inside another key, it's a nested key
                    nestedKey = true;
            }

            if (nestedKey)
            {
                var tempContent = content;

                // Initialize all keys to be upper case, regardless of what they were
                // Make sure to do this by an ordered set of keys, largest to smallest
                // So the smaller keys replace don't interfere with the larger keys
                var orderedKeys = mappings.OrderByDescending(m => m.Length).ToList();
                for (var i = 0; i < orderedKeys.Count(); i++)
                    tempContent = tempContent.Replace(orderedKeys[i], orderedKeys[i].ToUpperInvariant());

                // Exclude all keys that are smaller than the current key
                // Doing this allows (double - nth) nested keys to be found, since they will exclude the other keys
                // that normally would interfere
                var nonSearchedLargerKeys = nonSearchedKeys.Where(k => k.Length > searchKey.Length);

                // Set all non-searched larger keys to lower
                // Doing this it allows the app to search for the nested key without hindrance
                // Because a larger key (Address Zip) will be able to find itself even if a smaller
                // nested key (Address) isn't lower case. This wouldn't matter for smaller keys 
                // since all keys that would matter are larger than itself.
                foreach (var key in nonSearchedLargerKeys)
                    tempContent = tempContent.Replace(key.ToUpperInvariant(), key.ToLowerInvariant());

                return tempContent.IndexOf(searchKey.ToUpperInvariant());
            }
            else
                return content.IndexOf(searchKey);
        }
    }
}