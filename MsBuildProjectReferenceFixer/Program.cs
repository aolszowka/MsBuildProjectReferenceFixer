// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceFixer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using MsBuildProjectReferenceFixer.Properties;

    using NDesk.Options;

    /// <summary>
    /// Utility to Fix MsBuild ProjectRefence Tags when something is moved or
    /// renamed within the same subdirectory.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string targetDirectory = string.Empty;
            bool validateOnly = false;
            bool showHelp = false;

            OptionSet p = new OptionSet()
            {
                { "<>", Strings.TargetDirectoryDescription, v => targetDirectory = v },
                { "validate", Strings.ValidateDescription, v => validateOnly = v != null },
                { "?|h|help", Strings.HelpDescription, v => showHelp = v != null },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine(Strings.ShortUsageMessage);
                Console.WriteLine($"Try `--help` for more information.");
                Environment.ExitCode = 160;
                return;
            }

            if (showHelp || string.IsNullOrEmpty(targetDirectory))
            {
                ShowUsage(p);
            }
            else if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine(Strings.InvalidTargetArgument, targetDirectory);
                Environment.ExitCode = 9009;
            }
            else
            {
                if (validateOnly == true)
                {
                    Environment.ExitCode = PrintToConsole(targetDirectory, false);
                }
                else
                {
                    // We throw away the return code here because we are modifying the projects
                    PrintToConsole(targetDirectory, true);
                    Environment.ExitCode = 0;
                }
            }
        }

        private static int ShowUsage(OptionSet p)
        {
            Console.WriteLine(Strings.ShortUsageMessage);
            Console.WriteLine();
            Console.WriteLine(Strings.LongDescription);
            Console.WriteLine();
            Console.WriteLine($"               <>            {Strings.TargetDirectoryDescription}");
            p.WriteOptionDescriptions(Console.Out);
            return 160;
        }

        static int PrintToConsole(string targetDirectory, bool fixProjects)
        {
            // Create our lookup Dictionary
            IDictionary<string, string> projectLookupDictionary = ProjectReferenceFixer.LoadProjectGuids(targetDirectory);

            // Now Scan Each Project
            IEnumerable<string> projectsToFix = ProjectReferenceFixer.GetProjectsInDirectory(targetDirectory);

            string[] brokenProjects =
                projectsToFix
                .AsParallel()
                .Where(projectToFix => ProjectReferenceFixer.UpdateProjectReferences(projectToFix, projectLookupDictionary, fixProjects))
                .ToArray();

            foreach (string brokenProject in brokenProjects)
            {
                Console.WriteLine(brokenProject);
            }

            return brokenProjects.Length;
        }
    }
}
