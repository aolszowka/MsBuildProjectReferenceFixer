// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceFixer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Utility to Fix MsBuild ProjectRefence Tags when something is moved or
    /// renamed within the same subdirectory.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 0;

            if (args.Any())
            {
                string command = args.First().ToLowerInvariant();

                if (command.Equals("-?") || command.Equals("/?") || command.Equals("-help") || command.Equals("/help"))
                {
                    errorCode = ShowUsage();
                }
                else if (command.Equals("validatedirectory"))
                {
                    if (args.Length < 2)
                    {
                        string error = string.Format("You must provide a directory as a second argument to use validatedirectory");
                        Console.WriteLine(error);
                        errorCode = 1;
                    }
                    else
                    {
                        // The second argument is a directory
                        string directoryArgument = args[1];

                        if (Directory.Exists(directoryArgument))
                        {
                            errorCode = PrintToConsole(directoryArgument, false);
                        }
                        else
                        {
                            string error = string.Format("The provided directory `{0}` is invalid.", directoryArgument);
                            errorCode = 9009;
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(command))
                    {
                        string targetPath = command;
                        PrintToConsole(targetPath, true);
                        errorCode = 0;
                    }
                    else
                    {
                        string error = string.Format("The specified path `{0}` is not valid.", command);
                        Console.WriteLine(error);
                        errorCode = 1;
                    }
                }
            }
            else
            {
                // This was a bad command
                errorCode = ShowUsage();
            }

            Environment.Exit(errorCode);
        }

        private static int ShowUsage()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Scans given directory for MsBuild Projects; Correcting their ProjectReference tags.");
            message.AppendLine("Invalid Command/Arguments. Valid commands are:");
            message.AppendLine();
            message.AppendLine("[directory]                   - [MODIFIES] Spins through the specified directory\n" +
                               "                                and all subdirectories for Project files cleans\n" +
                               "                                any invalid ProjectReference tags. ALWAYS Returns 0.");
            message.AppendLine("validatedirectory [directory] - [READS] Spins through the specified directory\n" +
                               "                                and all subdirectories for Project files prints\n" +
                               "                                all paths that are NOT 'Valid'. Returns the\n" +
                               "                                number of invalid paths.");
            Console.WriteLine(message);
            return 21;
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
