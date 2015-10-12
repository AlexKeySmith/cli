﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.DotNet.ProjectModel;
using NuGet.Frameworks;

namespace Microsoft.DotNet.Tools.DependencyResolver
{
    public class Program
    {
        public void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "dotnet resolve-references";
            app.Description = "Resolves the absolute path of all dependencies for a project";
            app.HelpOption("-h|--help");

            var packages = app.Option("-p|--packages <PACKAGES_DIRECTORY>", "Path to the directories containing packages to resolve.", CommandOptionType.MultipleValue);
            var framework = app.Option("-f|--framework <FRAMEWORK_NAME>", "The framework to resolve dependencies for.", CommandOptionType.SingleValue);
            var runtime = app.Option("-r|--runtime <RUNTIME_IDENTIFIER>", "The runtime to resolve dependencies for.", CommandOptionType.SingleValue);
            var output = app.Option("-o|--output <OUTPUT_FILE>", "The path in which to write the output file (formatted as text with one line per dependency)", CommandOptionType.SingleValue);
            var assetType = app.Option("-a|--assets <ASSET_TYPE>", "The type of assets to resolve (common values include: compile, runtime, native)", CommandOptionType.MultipleValue);
            var project = app.Argument("PROJECT", "The project to resolve. A directory or a path to a project.lock.json may be used. Defaults to the current directory");

            app.OnExecute(() =>
            {
                // Check required args
                if (!framework.HasValue())
                {
                    Console.Error.WriteLine("Missing required argument: --framework");
                    app.ShowHelp();
                    return 1;
                }
                if (!assetType.HasValue())
                {
                    Console.Error.WriteLine("Missing required argument: --assets");
                    app.ShowHelp();
                    return 1;
                }

                // Determine packages folder
                var packagesDirs = packages.Values;
                if (!packagesDirs.Any())
                {
                    var defaultDir = GetDefaultPackagesFolder();
                    if (string.IsNullOrEmpty(defaultDir))
                    {
                        Console.Error.WriteLine("Unable to locate packages directory! Try using --packages to specify it manually.");
                        app.ShowHelp();
                        return 1;
                    }
                    packagesDirs.Add(defaultDir);
                }
                foreach (var packagesDir in packagesDirs)
                {
                    Console.Error.WriteLine($"Using packages directory: {packagesDir}");
                }

                var path = project.Value ?? Directory.GetCurrentDirectory();
                var projectContext = ProjectContext.CreateAsync(path,
                    NuGetFramework.Parse(framework.Value()),
                    runtime.Value());
                return Resolver.Execute(packagesDirs, output.Value(), assetType.Values, projectContext);
            });

            app.Execute(args);
        }

        private string GetDefaultPackagesFolder()
        {
            // TODO: Read DNX_PACKAGES (or equivalent)?
            // TODO: Read global.json
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            if (string.IsNullOrEmpty(userProfile))
            {
                userProfile = Environment.GetEnvironmentVariable("HOME");
            }
            if (string.IsNullOrEmpty(userProfile))
            {
                return null;
            }

            // TODO: Use NuGet folder, and AppData on Windows.
            return Path.Combine(userProfile, ".dnx", "packages");
        }
    }
}
