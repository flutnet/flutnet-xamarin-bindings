using System;
using System.IO;
using System.Text;
using FlutterSync.Extensions;
using Medallion.Shell;

namespace FlutterSync
{
    internal class FlutterTools
    {
        /// <summary>
        /// Runs the "flutter --version" command to retrieve the current version of Flutter.
        /// </summary>
        public static FlutterVersion GetVersion(bool verbose = false)
        {
            CommandResult result = FlutnetShell.RunCommand("flutter --version --no-version-check", Environment.CurrentDirectory, verbose);

            FlutterVersion version = new FlutterVersion();

            int index;
            using (StringReader reader = new StringReader(result.StandardOutput))
            {
                string versionLine = reader.ReadLine();
                string[] parts = versionLine
                    .Replace("Flutter", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    version.Version = parts[0].Trim();
                }

                string frameworkRevLine = reader.ReadLine();
                index = frameworkRevLine.IndexOf("revision ", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                {
                    version.FrameworkRev = frameworkRevLine.Substring(index + 9, 10).Trim();
                }

                string engineRevLine = reader.ReadLine();
                index = engineRevLine.IndexOf("revision ", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                {
                    version.EngineRev = engineRevLine.Substring(index + 9).Trim();
                }
            }

            return version;
        }

        /// <summary>
        /// Runs the "flutter clean" command to remove all the files produced by the previous build(s), such as the build folder.
        /// </summary>
        public static void Clean(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter clean", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub upgrade" command.
        /// </summary>
        public static void PubUpgrade(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter pub upgrade", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub get" command to get all the dependencies listed in the pubspec.yaml file
        /// in the current working directory, as well as their transitive dependencies.
        /// </summary>
        public static void GetDependencies(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter pub get", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub run build_runner build" command in the current working directory.
        /// The directory must contain a pubspec.yaml file.
        /// </summary>
        public static void BuildBuildRunner(string projectFolder, bool deleteConflictingOutputs = false, bool verbose = false)
        {
            FlutnetShell.RunCommand(
                deleteConflictingOutputs
                    ? $"flutter pub run build_runner build --delete-conflicting-outputs"
                    : $"flutter pub run build_runner build", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter build aar" command to create an Android Archive (AAR)
        /// to be integrated into a native Android application.
        /// </summary>
        public static void BuildAndroidArchive(string projectFolder, FlutterModuleBuildConfig buildConfig, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter build aar ");

            if (buildConfig != FlutterModuleBuildConfig.Default)
            {
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Debug))
                    sb.Append("--no-debug ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Profile))
                    sb.Append("--no-profile ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Release))
                    sb.Append("--no-release ");
            }

            FlutnetShell.RunCommand(sb.ToString(), projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter build ios-framework" command to create an iOS framework
        /// to be integrated into a native iOS application (available only on macOS).
        /// </summary>
        public static void BuildIosFramework(string projectFolder, FlutterModuleBuildConfig buildConfig, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter build ios-framework ");

            if (buildConfig != FlutterModuleBuildConfig.Default)
            {
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Debug))
                    sb.Append("--no-debug ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Profile))
                    sb.Append("--no-profile ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Release))
                    sb.Append("--no-release ");
            }

            FlutnetShell.RunCommand(sb.ToString(), projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter create -t package" command to create a Flutter package.
        /// </summary>
        public static DartProject CreatePackage(string workingDir, string name, string description = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t package ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            return new DartProject(Path.Combine(workingDir, name));
        }

        /// <summary>
        /// Runs the "flutter create -t module" command to create a Flutter module.
        /// </summary>
        public static DartProject CreateModule(string workingDir, string name, string description = null, string organization = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t module ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            if (!string.IsNullOrEmpty(organization))
                sb.Append($"--org {organization} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            return new DartProject(Path.Combine(workingDir, name));
        }

        /// <summary>
        /// Runs the "flutter create -t app" command to create a Flutter app.
        /// </summary>
        public static DartProject CreateApp(string workingDir, string name, string description = null, string organization = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t app ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            if (!string.IsNullOrEmpty(organization))
                sb.Append($"--org {organization} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            return new DartProject(Path.Combine(workingDir, name));
        }
    }
}