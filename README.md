# field-mapper

> Field Mapper is a mapping tool built for C# .NET to parse a given string into key / value pairs using the keys as markers to identify values.

* [Overview](#overview)
* [Install](#install)
* [Usage](#usage)

<a name="overview"></a>
## Overview
Given a series of keys, such as 'First Name:', 'Last Name:', 'Phone:', and a body of text like 'First Name: Joe Last Name: Moceri Phone: 5551231234' field-mapper will return a Dictionary<string, string> that looks like this.

```csharp
"First Name:" : "Joe"
"Last Name:"  : "Moceri"
"Phone:"      : "5551231234"
```

The mappings are case sensitive, so fIRST Name: wouldn't map. If 'Last Name:' key were omitted, 'First Name:' would look like "Joe Last Name: Moceri". Thus the keys are the markers for where to start and stop for the values, and the first ('earliest') key gets matched with the value.

This can be used for any kind of string mapping, and has been used in email body parsing.

<a name="install"></a>
## Install
Using NuGet
```sh
Install-Package FieldMapper
```

<a name="usage"></a>
## Usage

Please see Tests section of project for examples on how to use it and below.

```csharp
            var emailKey = "Email Name";
            var emailValue = "example@example.com";
            var phoneKey = "Phone";
            var phoneValue = "5555551234";
            var fullNameKey = "Name";
            var fullNameValue = "Joe Moceri";
            var zipKey = "Zip Name";
            var zipValue = "00000";

            var content = $"{emailKey} {emailValue} {phoneKey} {phoneValue} {fullNameKey} {fullNameValue} {zipKey} {zipValue}";
            var mappings = new List<string>();
            mappings.Add(emailKey);
            mappings.Add(fullNameKey);
            mappings.Add(phoneKey);
            mappings.Add(zipKey);

            // Act
            var parser = new FieldMapper(content, mappings);
            var result = parser.Get();
            
            // produces a Dictionary that looks like this 
            // {k:v}
            // "Email Name" : "example@example.com"
            // "Phone"      : "5555551234"
            // "Name"       : "Joe Moceri"
            // "Zip"        : "00000"
```
