# GrokParser

A dotnet library to parse and process Grok patterns in .NET


[![Build](https://github.com/raminhz90/GrokParser/actions/workflows/nugetPublish.yml/badge.svg?branch=main)](https://github.com/raminhz90/GrokParser/actions/workflows/nugetPublish.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/raminhz90/GrokParser/blob/main/LICENSE) 

[![NuGet version](https://badge.fury.io/nu/Ramin.GrokParser.svg)](https://www.nuget.org/packages/Ramin.GrokParser)
[![Nuget](https://img.shields.io/nuget/dt/Ramin.GrokParser.svg)](https://www.nuget.org/packages/Ramin.GrokParser)

# Install
Install  from [Nuget](http://nuget.org):

**[GrokParser](https://www.nuget.org/packages/Ramin.GrokParser)**

    PM> dotnet add package Ramin.GrokParser

# What is a Grok
A grok is a way to match a line of log data with a regular expression and extract structured fields from the line. It is commonly used in the context of log processing, where log data is stored in text files and it is necessary to extract specific information from the logs in order to analyze and understand them.

here is a sample grok pattern 


    %{TIMESTAMP_ISO8601:timestamp} \[%{LOGLEVEL:level}\] \[%{DATA:service}.%{DATA:module}\] %{GREEDYDATA:message}

this grok pattern matches the following string

    2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input

In this example, the grok pattern is using named capture groups (e.g. TIMESTAMP_ISO8601, LOGLEVEL, DATA, and GREEDYDATA) to identify specific pieces of information in the log line. The named capture groups are enclosed in %{} and are separated by : from the field names (e.g. timestamp, level, service, module, and message) that will be used to represent the extracted information.

When the log line is processed using this grok pattern, the following field-value pairs will be extracted:

    timestamp: 2022-12-27T10:15:30.456789Z
    level: ERROR
    service: my_service
    module: my_module
    message: An error occurred: invalid input

# Usage

## Simple pattern
```csharp
var grokPattern = @"%{TIMESTAMP_ISO8601:timestamp:datetime} \[%{LOGLEVEL:level}\] \[%{DATA:service}.%{DATA:module}\] %{GREEDYDATA:message}";
var logs = @"2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input";
var grokBuilder = new GrokBuilder(grokPattern);
var grokParser = grokBuilder.Build();
var grokResult = grokParser.Parse(logs);
```

## Adding custom patterns
you can add custom grok patterns to complement or override the default patterns

grok builder accepts any variable that implements the IDictionary<string,string> Interface

```csharp
var customPatterns = new Dictionary<string,string>();
customPatterns.Add("TIMESTAMP_ISO8601", @"(?<timestamp>(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(\.(?<millisecond>\d{1,9}))?(?<timezone>Z|[+-]\d{2}:\d{2}))");
var grokPattern = @"%{TIMESTAMP_ISO8601:timestamp:datetime} \[%{LOGLEVEL:level}\] \[%{DATA:service}.%{DATA:module}\] %{GREEDYDATA:message}";
var logs = @"2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input";
var grokBuilder = new GrokBuilder(grokPattern,customPatterns);
var grokParser = grokBuilder.Build();
var grokResult = grokParser.Parse(logs);
```

## Postprocessors

you can further match extracted fields with with additional grok patterns

the postProcessor argument accepts any variable that implements IEnumerable<KeyValuePair<string, string>>

after processing the main grok pattern any postProcess patterns will run on already extracted fields

```csharp
var postProcessor = new List<KeyValuePair<string,string>>();
postProcessor.Add(new KeyValuePair<string, string>("timestamp", "postProcessPattern"));
var logs = @"2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input";
var grokBuilder = new GrokBuilder(grokPattern,postProcessors:postProcessor);
var grokParser = grokBuilder.Build();
var grokResult = grokParser.Parse(logs);
```

## Filters

you can remove some of the extracted fields using the filters argument

in this example timestamp will not be in extracted fields

```csharp
var filters = new List<string>();
filters.Add("timestamp");
var logs = @"2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input";
var grokBuilder = new GrokBuilder(grokPattern,filters:filters);
var grokParser = grokBuilder.Build();
var grokResult = grokParser.Parse(logs);
```

# Timeout

since grok patterns are usually very complex regex patterns it is possible that matching a string will take a long time or worse never complete

you can use the timeout argument to ensure the match completes or fails in required time

the default timeout value is 1 second

```csharp
var filters = new List<string>();
filters.Add("timestamp");
var logs = @"2022-12-27T10:15:30.456789Z [ERROR] [my_service.my_module] An error occurred: invalid input";
var grokBuilder = new GrokBuilder(grokPattern,timeout: TimeSpan.FromSeconds(30));
var grokParser = grokBuilder.Build();
var grokResult = grokParser.Parse(logs);
```

# Types
you can specify the type of the field using the following syntax

```
%{TIMESTAMP_ISO8601:timestamp:datetime}
```

the last part specifies the type this filed needs to be converted to

currently the following types are supported
* int
* double
* float
* bool
* datetime
* long
* datetimeoffset