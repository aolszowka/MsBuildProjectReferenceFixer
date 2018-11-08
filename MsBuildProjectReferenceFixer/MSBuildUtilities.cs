// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceFixer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public static class MSBuildUtilities
    {
        internal static XNamespace msbuildNS = @"http://schemas.microsoft.com/developer/msbuild/2003";


        /// <summary>
        /// Extracts the Project GUID from the specified proj File.
        /// </summary>
        /// <param name="pathToProjFile">The proj File to extract the Project GUID from.</param>
        /// <returns>The specified proj File's Project GUID.</returns>
        public static string GetMSBuildProjectGuid(string pathToProjFile)
        {
            XDocument projFile = XDocument.Load(pathToProjFile);
            XElement projectGuid = projFile.Descendants(msbuildNS + "ProjectGuid").FirstOrDefault();

            if (projectGuid == null)
            {
                string exception = $"Project {pathToProjFile} did not contain a ProjectGuid.";
                throw new InvalidOperationException(pathToProjFile);
            }

            return projectGuid.Value;
        }

        public static IEnumerable<XElement> GetProjectReferenceNodes(XDocument projXml)
        {
            return projXml.Descendants(msbuildNS + "ProjectReference");
        }

        public static string GetProjectReferenceGUID(XElement projectReference, string projectPath)
        {
            // Get the existing Project Reference GUID
            XElement projectReferenceGuidElement = projectReference.Descendants(msbuildNS + "Project").FirstOrDefault();
            if (projectReferenceGuidElement == null)
            {
                string exception = $"A ProjectReference in {projectPath} does not contain a Project Element; this is invalid.";
                throw new InvalidOperationException(exception);
            }

            // This is the referenced project
            string projectReferenceGuid = projectReferenceGuidElement.Value;

            return projectReferenceGuid;
        }

        public static string GetProjectReferenceName(XElement projectReference, string projectPath)
        {
            // Get the existing Project Reference Name
            XElement projectReferenceNameElement = projectReference.Descendants(msbuildNS + "Name").FirstOrDefault();
            if (projectReferenceNameElement == null)
            {
                string exception = $"A ProjectReference in {projectPath} does not contain a Name Element; this is invalid.";
                throw new InvalidOperationException(exception);
            }

            // This is the referenced project
            string projectReferenceName = projectReferenceNameElement.Value;

            return projectReferenceName;
        }

        public static void SetProjectReferenceName(XElement projectReference, string name)
        {
            projectReference.Descendants(msbuildNS + "Name").First().SetValue(name);
        }

        public static string GetProjectReferenceIncludeValue(XElement projectReference, string projectPath)
        {
            // Get the existing Project Reference Include Value
            XAttribute projectReferenceIncludeAttribute = projectReference.Attribute("Include");

            if (projectReferenceIncludeAttribute == null)
            {
                string exception = $"A ProjectReference in {projectPath} does not contain an Include Attribute on it; this is invalid.";
                throw new InvalidOperationException(exception);
            }

            // This is the referenced project
            string projectReferenceInclude = projectReferenceIncludeAttribute.Value;

            return projectReferenceInclude;
        }

        public static void SetProjectReferenceIncludeValue(XElement projectReference, string includeValue)
        {
            // Fix up the slashes to be Windows Slashes
            includeValue = includeValue.Replace('/', '\\');

            projectReference.Attribute("Include").SetValue(includeValue);
        }

    }
}
