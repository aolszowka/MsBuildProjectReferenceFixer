# MsBuildProjectReferenceFixer
![CI - Master](https://github.com/aolszowka/MsBuildProjectReferenceFixer/workflows/CI/badge.svg?branch=master)

Utility for Fixing Project References in MSBuild Project System Files.

## When To Use This Tool
This tool is intended to be used after you have done a massive source folder reorganization. The problem becomes now all of the relative paths to your Project Files are invalid in any MSBuild Project System File (IE *.CSPROJ) that used a Project Reference. In addition if you have done any renaming this would need to change too.

This is for 2 reasons as per the documentation [Microsoft Docs: Common MSBuild Project Items - ProjectReference](https://docs.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-items?view=vs-2017#projectreference):

* The Include Attribute on the ProjectReference Tag is now most likely invalid.
    * This is a relative path, relative to the Project File which the tag resides in. If you have moved this project you would need to regenerate the relative path, therefore a simple find and replace is not fesible.
* The Name Tag (Child of ProjectReference) could possibly be out of date.
    * This is the File Name of the project (without the extension) to which you have reference.

While these items are documented as being optional in practice Visual Studio (at least as of Visual Studio 2017 15.8.7) will helpfully correct any out of date elements if it can find them. However after a massive source folder reorganization this is unlikely because chances are the relative paths are all broken or the project name has changed.

Previous to this tool you had to open each affected MSBuild Project System File and remove/re-add the project in question. This is tedious and error prone.

## Operation
This tool will:

* Scan the Given Directory for MSBuild System Project Files
    * It will load up each project into a Lookup Dictionary that is keyed off of the `ProjectGuid`. If this is duplicated this utility will throw an `InvalidOperationException`. For help in identifying these conditions see https://github.com/aolszowka/MsBuildDuplicateProjectGuid to fix this condition see https://github.com/aolszowka/MsBuildSetProjectGuid.
* For Each MSBuild System Project File it will scan inside the file for `ProjectReference` Tags
    * If the `ProjectReference` tag is missing the (Optional) `Project` Child Element this utility will throw an `InvalidOperationException` for help in fixing this condition see https://github.com/aolszowka/MsBuildResyncProjectReferenceGuid
* For Each `ProjectReference` Tag, based on the `Project` Guid, it will regenerate the following:
    * Reset the `Include` attribute of `ProjectReference` to the Relative Path of the project found in the Lookup Dictionary
    * Reset the `Name` Child Element of `ProjectReference` to the file name of the project found in the Lookup Dictionary (without its file extension).

This tool will make no attempt to fix any Solution Files. See the sister project https://github.com/aolszowka/VisualStudioSolutionFixer for a way to fix those.

## Usage
There are now two ways to run this tool:

1. (Compiled Executable) Invoke the tool via `MsBuildProjectReferenceFixer` and pass the arguments.
2. (Dotnet Tool) Install this tool using the following command `dotnet tool install MsBuildProjectReferenceFixer` (assuming that you have the nuget package in your feed) then invoke it via `dotnet project-fixprojectreferences`

In both cases the flags to the tooling are identical:

```
Usage: C:\ProjectDirectory\ [-validate]

Scans given directory for MsBuild Projects; Correcting their ProjectReference
tags.

This program will modify the specified Target Directory always returning an exit
code of zero, regardless of how many projects were fixed.

If you run this program with the validate argument, instead of modifying the
projects, the exit code will be equal to the number of projects that would have
been fixed.

In all cases the program will print out the projects that needed to be fixed.

               <>            The directory to spin though for Project Files
      --validate             Indicates if this tool should only be run in
                               validation mode
  -?, -h, --help             Show this message and exit
```

## Hacking
The most likely change you will want to make is changing the supported project files. In theory this tool should support any MSBuild Project Format that utilizes a `ProjectGuid`.

See `MsBuildProjectReferenceFixer.GetProjectsInDirectory(string)` for the place to modify this.

The tool should also support those projects that utilize the same ProjectReference format as CSPROJ formats.

## Contributing
Pull requests and bug reports are welcomed so long as they are MIT Licensed.

## License
This tool is MIT Licensed.

## Third Party Licenses
This project uses other open source contributions see [LICENSES.md](LICENSES.md) for a comprehensive listing.
