# MsBuildProjectReferenceFixer
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
```
Usage: MsBuildProjectReferenceFixer.exe [validatedirectory] directory

Scans given directory for MsBuild Projects; Correcting their ProjectReference tags.
Invalid Command/Arguments. Valid commands are:

[directory]                   - [MODIFIES] Spins through the specified directory
                                and all subdirectories for Project files cleans
                                any invalid ProjectReference tags. ALWAYS Returns 0.
validatedirectory [directory] - [READS] Spins through the specified directory
                                and all subdirectories for Project files prints
                                all paths that are NOT 'Valid'. Returns the
                                number of invalid paths.
```

## Hacking
The most likely change you will want to make is changing the supported project files. In theory this tool should support any MSBuild Project Format that utilizes a ProjectGuid.

See MsBuildProjectReferenceFixer.GetProjectsInDirectory(string) for the place to modify this.

The tool should also support those projects that utilize the same ProjectReference format as CSPROJ formats.

## Contributing
Pull requests and bug reports are welcomed so long as they are MIT Licensed.

## License
This tool is MIT Licensed.

## Third Party Licenses
This project uses other open source contributions see [LICENSES.md](LICENSES.md) for a comprehensive listing.
