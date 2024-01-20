# Amg.Extensions

Frequently used extensions for .NET

Install via [Nuget](https://www.nuget.org/packages/Amg.Extensions/):
```
dotnet add package Amg.Extensions
```

## Features

## To Do

* Refactor so that Amg.Extensions does not have any other packages as dependencies
* Remove serilog
* Increase test coverage
* ChildProcess

## Done

* remove SonarAnalyzer warnings
* Migrate to .net 8, C# 12
* release script
* automated nuget package generation with Github actions
* Create classes with *"Once"* semantics
* TimeInterval
* fluent API for path and file system operations
* Enumerable extensions
* command line option parser

## Build

Update package versions with
```
dotnet nukeeper update -m 100 -a 0
```
