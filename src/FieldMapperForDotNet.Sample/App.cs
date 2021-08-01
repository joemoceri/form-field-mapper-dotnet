using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldMapperForDotNet.Sample
{
    public class App
    {
        public void Run()
        {
            var fieldMapper = new FieldMapper();
            var content = $"First Name: TestFirstName{Environment.NewLine}Last Name: TestLastName";
            var mappings = new List<string>();
            mappings.Add("First Name:");
            mappings.Add("Last Name:");

            var result = fieldMapper.Get(content, mappings);

            var firstName = result["First Name:"]; // outputs TestFirstName
            var lastName = result["Last Name:"]; // outputs TestLastName
        }
    }
}
