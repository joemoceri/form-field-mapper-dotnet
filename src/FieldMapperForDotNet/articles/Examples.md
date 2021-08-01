### Examples

**Table of Contents**
- [Description](#description)
- [Field Mapper](#field-mapper)

#### Description

To see these in action, check out the [Tests Project](https://github.com/joemoceri/field-mapper-dotnet/tree/main/src/FieldMapperForDotNet.Tests) and [Sample Project](https://github.com/joemoceri/field-mapper-dotnet/tree/main/src/FieldMapperForDotNet.Sample).

#### Field Mapper

Map a first name out of a given string.

```csharp
public void MapFirstName()
{
    // Arrange
    string key = "First Name:";
    string value = "Joe";

    var content = $"{key} {value}";
    var mappings = new List<string>();
    mappings.Add(key);

    // Act
    var parser = new FieldMapper();
    var result = parser.Get(content, mappings);

    // Assert
    Assert.AreEqual(value, result[key]);
}
```