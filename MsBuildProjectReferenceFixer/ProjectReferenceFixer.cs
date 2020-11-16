// -----------------------------------------------------------------------
// <copyright file="ProjectReferenceFixer.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceFixer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal static class ProjectReferenceFixer
    {
        /// <summary>
        /// Update/Fix all ProjectReference elements within the given project file.
        /// </summary>
        /// <param name="projectPath">The path to the MSBuild Project to fix ProjectElements in.</param>
        /// <param name="projectLookupDictionary">A Dictionary from <see cref="LoadProjectGuids(string)"/> to perform lookups in.</param>
        /// <param name="saveChanges">Indicates if this tool should save the changes to the target project file.</param>
        /// <returns><c>true</c> if any ProjectReference tags within the given project are modified; otherwise, <c>false</c>.</returns>
        internal static bool UpdateProjectReferences(string projectPath, IDictionary<string, string> projectLookupDictionary, bool saveChanges)
        {
            bool projectModified = false;

            try
            {
                XDocument projXml = XDocument.Load(projectPath);

                // Find all the ProjectReference Elements
                IEnumerable<XElement> projectReferenceElements = MSBuildUtilities.GetProjectReferenceNodes(projXml);

                foreach (XElement projectReferenceElement in projectReferenceElements)
                {
                    if (FixProjectReference(projectReferenceElement, projectPath, projectLookupDictionary))
                    {
                        projectModified = true;
                    }
                }

                if (saveChanges && projectModified)
                {
                    projXml.Save(projectPath);
                }
            }
            catch (Exception ex)
            {
                string exception = $"Failed on project `{projectPath}`";
                throw new InvalidOperationException(exception, ex);
            }

            return projectModified;
        }

        /// <summary>
        /// Given a target directory spin for all project files (as defined by
        /// <see cref="GetProjectsInDirectory(string)"/>).
        /// </summary>
        /// <param name="targetDirectory">The directory to scan.</param>
        /// <returns>
        /// <see cref="IDictionary{TKey, TValue}"/> where the <c>TKey</c> is
        /// the ProjectGuid and the <c>TValue</c> is the path to the project
        /// that contains that Guid.
        /// </returns>
        internal static IDictionary<string, string> LoadProjectGuids(string targetDirectory)
        {
            IEnumerable<string> projFilesInDirectory = GetProjectsInDirectory(targetDirectory);

            ConcurrentDictionary<string, string> resultDictionary = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            Parallel.ForEach(projFilesInDirectory, projFile =>
            {
                string projectGuid = MSBuildUtilities.GetMSBuildProjectGuid(projFile);
                if (!resultDictionary.TryAdd(projectGuid, projFile))
                {
                    string exception = $"Failed to add project `{projFile}` the GUID `{projectGuid}` already existed in project `{resultDictionary[projectGuid]}`";
                    throw new InvalidOperationException(exception);
                }
            }
            );

            return resultDictionary;
        }

        /// <summary>
        /// Gets all Project Files that are understood by this
        /// tool from the given directory and all subdirectories.
        /// </summary>
        /// <param name="targetDirectory">The directory to scan for projects.</param>
        /// <returns>All projects that this tool supports.</returns>
        internal static IEnumerable<string> GetProjectsInDirectory(string targetDirectory)
        {
            HashSet<string> supportedFileExtensions =
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    ".csproj",
                    ".fsproj",
                    ".sqlproj",
                    ".synproj",
                    ".vbproj",
                };

            return
                Directory
                .EnumerateFiles(targetDirectory, "*proj", SearchOption.AllDirectories)
                .Where(currentFile => supportedFileExtensions.Contains(Path.GetExtension(currentFile)));
        }

        /// <summary>
        /// Given a <see cref="XElement"/> that represents an MSBuild
        /// ProjectReference tag, validate that the relative path to
        /// the dependency and the name of the dependency is correct.
        /// </summary>
        /// <param name="projectReference">A fragment that represents an MSBuild ProjectReference tag.</param>
        /// <param name="projectPath">The path to the project that contained this fragment.</param>
        /// <param name="projectLookupDictionary">A Dictionary from <see cref="LoadProjectGuids(string)"/> to perform lookups in.</param>
        /// <returns><c>true</c> if the fragment was modified (or "fixed"); otherwise, <c>false</c>.</returns>
        internal static bool FixProjectReference(XElement projectReference, string projectPath, IDictionary<string, string> projectLookupDictionary)
        {
            bool fragmentWasModified = false;

            // The project directory needs the trailing slash to support relative path generation
            string projectDirectory = Path.GetDirectoryName(projectPath);

            string prIncludeRelativePath = MSBuildUtilities.GetProjectReferenceIncludeValue(projectReference, projectPath);
            string prGuid = MSBuildUtilities.GetProjectReferenceGUID(projectReference, projectPath);

            // Look it up in the Dictionary
            string dictionaryLookupProjectPath = null;
            if (!projectLookupDictionary.TryGetValue(prGuid, out dictionaryLookupProjectPath))
            {
                string prIncludeActualPath = Path.GetFullPath(projectDirectory, prIncludeRelativePath);
                string exception = $"Project GUID `{prGuid}` does not exist in the lookup dictionary; according to the project it should be located here `{prIncludeActualPath}`; was it deleted?";
                throw new InvalidOperationException(exception);
            }

            // Now that we have the found path from the dictionary convert it to a relative path
            string prActualRelativePath = Path.GetRelativePath(dictionaryLookupProjectPath, projectDirectory);

            // Fix up the Relative Path to contain the correct slashes
            prActualRelativePath = prActualRelativePath.Replace(Path.DirectorySeparatorChar, '\\');

            if (!prIncludeRelativePath.Equals(prActualRelativePath))
            {
                fragmentWasModified = true;
                MSBuildUtilities.SetProjectReferenceIncludeValue(projectReference, prActualRelativePath);
            }

            // Get the name of the file that contains the reference; this will be in the Name Tag.
            string prActualName = Path.GetFileNameWithoutExtension(dictionaryLookupProjectPath);

            // Get the existing name in the project reference
            string prExistingName = MSBuildUtilities.GetOrCreateProjectReferenceName(projectReference, projectPath);
            if (!prExistingName.Equals(prActualName))
            {
                fragmentWasModified = true;
                MSBuildUtilities.SetProjectReferenceName(projectReference, prActualName);
            }

            return fragmentWasModified;
        }
    }
}
