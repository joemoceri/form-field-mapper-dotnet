### Field Mapper

This class is used for parsing data out of plain-text and html body contact form submission emails. Given a set of mappings, it will identify values from left-to-right, map them, and return a dictionary as the result.

This class can be customized using [Configurations](../api/FieldMapperForDotNet.FieldMapperConfiguration.html). Right now this supports de-entitizing html body emails and allowing or disabling separating mappings with line breaks. Default settings are recommended. When using plain-text, disabling de-entitizing is helpful to keep mappings consisent.