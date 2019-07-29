# field-mapper

> Mapping Utility built for C# .NET to parse a given string into key / value pairs

* [Overview](#overview)
* [Install](#install)
* [Usage](#usage)

<a name="overview"></a>
## Overview
This project was originally built to parse email bodies and retrieve them using a series of user-defined mappings

<a name="install"></a>
## Install
Using NuGet
```sh
Install-Package FieldMapper
```

<a name="usage"></a>
## Usage
Please see Tests section of project for examples on how to use it and below
```csharp
            var emailKey = "Email Name";
            var emailValue = "example@example.com";
            var phoneKey = "Phone";
            var phoneValue = "5555551234";
            var fullNameKey = "Name";
            var fullNameValue = "John Doe";
            var zipKey = "Zip Name";
            var zipValue = "55555";

            var content = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", emailKey, emailValue, phoneKey, phoneValue, fullNameKey, fullNameValue, zipKey, zipValue);
            var mappings = new List<string>();
            mappings.Add(emailKey);
            mappings.Add(fullNameKey);
            mappings.Add(phoneKey);
            mappings.Add(zipKey);

            // Act
            var parser = new FieldMapper(content, mappings);
            var result = parser.Get();
```