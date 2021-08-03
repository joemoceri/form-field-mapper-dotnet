using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FieldMapperForDotNet
{
    /// <summary>
    /// This is the main class used for mapping fields.
    /// </summary>
    public class FieldMapper
    {
        /// <summary>
        /// The configuration holds options that can change the behavior of the tool, such as choosing whether to DeEntitize the content first.
        /// </summary>
        public readonly FieldMapperConfiguration Configuration;



        /// <summary>
        /// By default it uses <see cref="FieldMapperConfiguration.Default"/>.
        /// </summary>
        public FieldMapper()
            : this(FieldMapperConfiguration.Default)
        {

        }
        /// <summary>
        /// You can pass in your own <see cref="FieldMapperConfiguration"/>. By default it uses <see cref="FieldMapperConfiguration.Default"/>.
        /// </summary>
        /// <param name="configuration">Your own <see cref="FieldMapperConfiguration"/></param>
        public FieldMapper(FieldMapperConfiguration configuration)
        {
            Configuration = configuration ?? FieldMapperConfiguration.Default;
        }



        /// <summary>
        /// Use this to validate methods arguments.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        private (string Content, string[] Keys) ValidateArguments(
            string content, IEnumerable<string> keys)
        {
            var keysArray = keys as string[]
                            ?? keys?.ToArray();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "Content cannot be null or empty.",
                    nameof(content));
            }
            if (keysArray == null || keysArray.Length == 0)
            {
                throw new ArgumentException(
                    "Mappings cannot be null or empty.",
                    nameof(keys));
            }
            if (keysArray.Any(key => string.IsNullOrWhiteSpace(key)))
            {
                throw new ArgumentException(
                    "Mappings cannot contain any empty values.",
                    nameof(keys));
            }
            if (keysArray.Distinct().Count() != keysArray.Length)
            {
                throw new ArgumentException(
                    "Duplicate mappings found. Please make sure they are all unique.",
                    nameof(keys));
            }

            return (content, keysArray);
        }



        /// <summary>
        /// Use this to see what the mappings should look like before they're mapped to values.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public string PreviewContent(string content,
            IEnumerable<string> keys)
        {
            var arguments = ValidateArguments(
                content, keys);

            if (Configuration.Options.DeEntitizeContent)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(arguments.Content);

                arguments.Content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            }

            if (Configuration.Options.SeparateByLineBreaks)
            {
                arguments.Content = Regex.Replace(arguments.Content, @"\s{5,}", Environment.NewLine);
            }

            arguments.Content = arguments.Content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");

            SeparateMappingsByLineBreaks();

            return arguments.Content;

            void SeparateMappingsByLineBreaks()
            {
                foreach (var searchMapping in arguments.Keys)
                {
                    var startIndex = GetIndexOfKey(arguments.Content, arguments.Keys, searchMapping);
                    var nextLocation = int.MaxValue;

                    if (!arguments.Content.Contains(Environment.NewLine) && startIndex != -1)
                    {
                        arguments.Content = arguments.Content.Insert(startIndex, Environment.NewLine);
                    }

                    foreach (var key in arguments.Keys.Where(k => k != searchMapping))
                    {
                        var loc = arguments.Content.IndexOf(key, startIndex + searchMapping.Length);

                        if (loc != -1 && loc < nextLocation)
                        {
                            nextLocation = loc;
                        }
                    }

                    if (nextLocation != int.MaxValue)
                    {
                        arguments.Content = arguments.Content.Insert(nextLocation, Environment.NewLine);
                    }
                }
            }
        }

        /// <summary>
        /// This is the main method for getting values out of a string with mappings.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public IDictionary<string, string> Get(string content,
            IEnumerable<string> keys)
        {
            var arguments = ValidateArguments(
                content, keys);

            arguments.Content = PreviewContent(arguments.Content, arguments.Keys);

            var result = new Dictionary<string, string>();

            using (var reader = new StringReader(arguments.Content))
            {
                var line = reader.ReadLine();
                var orderedMappings = arguments.Keys.OrderByDescending(m => m.Length).ToList();
                while (line != null)
                {
                    line = line.Trim();
                    for (var i = 0; i < orderedMappings.Count(); i++)
                    {
                        var mapping = orderedMappings[i];
                        if (line.Contains(mapping) && line.IndexOf(mapping) == 0)
                        {
                            var value = line.Substring(line.IndexOf(mapping) + mapping.Length).Trim();
                            var startIndex = GetIndexOfKey(arguments.Content, arguments.Keys, mapping);

                            var nextLineValue = GetMappingValueOnSubsequentLines(arguments.Content.Substring(startIndex + line.Length));

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
                        foreach (var mapping in arguments.Keys)
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
        }



        /// <summary>
        /// Internal method used to handle various mapping issues when trying to retrieve the right index
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="searchKey">The mapping it is looking for.</param>
        /// <returns></returns>
        private int GetIndexOfKey(string content,
            IEnumerable<string> keys, string searchKey)
        {
            var nestedKey = false;
            var nonSearchedKeys = keys.Where(k => k != searchKey);

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

                var orderedKeys = keys.OrderByDescending(m => m.Length).ToList();
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
    }
}