using System;
using System.Collections.Generic;

namespace FieldMapperForDotNet.Sample
{
    public sealed class App
    {
        public void Run()
        {
            const string firstNameKey = "First Name:";
            const string lastNameKey = "Last Name:";

            Console.Write($"(Enter) {firstNameKey} ");
            var firstNameValue = Console.ReadLine();
            // input: TestFirstName
            Console.Write($"(Enter) {lastNameKey} ");
            var lastNameValue = Console.ReadLine();
            // input: TestLastName

            Console.WriteLine("\n\n\n");

            string content = $"{firstNameKey} {firstNameValue}" +
                             $"{Environment.NewLine}" +
                             $"{lastNameKey} {lastNameValue}";

            Console.WriteLine("(Result) Content ->");
            Console.WriteLine(content);

            Console.WriteLine("\n\n\n");

            var fieldMapper = new FieldMapper();
            var mappings = new List<string>(2)
            {
                firstNameKey,
                lastNameKey
            };

            var result = fieldMapper.Parse(
                content, mappings);

            var parsedFirstNameValue = result[firstNameKey];
            var parsedLastNameValue = result[lastNameKey];

            Console.WriteLine($"(Parsed) {firstNameKey} {parsedFirstNameValue}");
            // output: TestFirstName
            Console.WriteLine($"(Parsed) {lastNameKey} {parsedLastNameValue}");
            // output: TestLastName

            Console.WriteLine("\n\n\n");

            Console.ReadKey(false);
        }
    }
}
