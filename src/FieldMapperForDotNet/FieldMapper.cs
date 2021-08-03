using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RIS.Extensions;

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
        /// <param name="configuration">Your own <see cref="FieldMapperConfiguration"/>.</param>
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
        public string PrepareContent(string content,
            IEnumerable<string> keys)
        {
            var arguments = ValidateArguments(
                content, keys);

            var keysList = arguments.Keys
                .ToList();
            var keysCount = keysList.Count;

            if (Configuration.Options.DeEntitizeContent)
            {
                var doc = new HtmlDocument();

                doc.LoadHtml(arguments.Content);

                arguments.Content = HtmlEntity.DeEntitize(
                    doc.DocumentNode.InnerText);
            }

            if (Configuration.Options.SeparateByLineBreaks)
            {
                arguments.Content = Regex.Replace(arguments.Content,
                    @"\s{5,}", Environment.NewLine);
            }

            arguments.Content = arguments.Content
                .ReplaceEOLChars(" ")
                .Replace('\r', ' ')
                .Trim();

            var startIndex = 0;

            for (int i = 0; i < keysCount; ++i)
            {
                var keyLocation = arguments.Content.IndexOfAny(
                    keysList.ToArray(),
                    startIndex);

                if (keyLocation.Index == -1)
                    continue;

                if (keyLocation.Index != 0)
                {
                    arguments.Content = arguments.Content.Insert(
                        keyLocation.Index,
                        Environment.NewLine);
                }

                var key = arguments.Content.Substring(
                    keyLocation.Index + Environment.NewLine.Length,
                    keyLocation.Count);

                for (int j = 0; j < keysList.Count; ++j)
                {
                    if (keysList[j] != key)
                        continue;

                    keysList.RemoveAt(j);

                    break;
                }

                var nextKeyLocation = arguments.Content.IndexOfAny(
                    keysList.ToArray(),
                    keyLocation.Index + Environment.NewLine.Length + keyLocation.Count);

                if (nextKeyLocation.Index == -1)
                    continue;

                startIndex = nextKeyLocation.Index;
            }

            return arguments.Content;
        }

        /// <summary>
        /// This is the main method for getting values out of a string with mappings.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public IDictionary<string, string> Parse(string content,
            IEnumerable<string> keys)
        {
            var arguments = ValidateArguments(
                content, keys);

            arguments.Content = PrepareContent(
                arguments.Content, arguments.Keys);

            using var reader = new StringReader(
                arguments.Content);

            var result = new Dictionary<string, string>();
            var line = reader.ReadLine();

            while (line != null)
            {
                line = line.Trim();

                foreach (var key in arguments.Keys)
                {
                    if (!line.StartsWith(key))
                        continue;

                    var value = line
                        .Substring(key.Length)
                        .Trim();

                    result[key] = value;

                    break;
                }

                line = reader.ReadLine();
            }

            return result;
        }
    }
}