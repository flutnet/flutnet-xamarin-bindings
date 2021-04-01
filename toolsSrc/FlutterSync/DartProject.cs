using System;
using System.IO;
using FlutterSync.Extensions;
using SharpYaml.Serialization;
using Path = System.IO.Path;

namespace FlutterSync
{
    internal class DartProject
    {
        /// <summary>
        /// The name of the file that contains all the metadata of a Dart (Flutter) project.
        /// This file is written in the YAML language.
        /// </summary>
        public const string PubspecFilename = "pubspec.yaml";

        /// <summary>
        /// The name of the file that tracks properties of a Flutter project.
        /// This file is used by Flutter tool to assess capabilities and perform upgrades etc.
        /// </summary>
        public const string MetadataFilename = ".metadata";

        DirectoryInfo _workingDir;
        FileInfo _pubspecFile;
        FileInfo _metadataFile;
        DirectoryInfo _libFolder;
        DirectoryInfo _testFolder;

        YamlStream _pubspecStream;
        YamlDocument _pubspecDocument;

        YamlStream _metadataStream;
        YamlDocument _metadataDocument;

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Author { get; private set; }

        public string Homepage { get; private set; }

        public string Version { get; private set; }

        public string Sdk { get; private set; }

        public DartProjectType Type { get; private set; }

        /// <summary>
        /// Returns the path of the Android AAR created when building a Flutter module
        /// and used for integrating Flutter into a native Android app.
        /// </summary>
        public string GetAndroidArchivePath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (Type)
            {
                case DartProjectType.Module:
                    return GetAndroidArchivePath(_workingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }
        }

        /// <summary>
        /// Returns the path of the iOS XCFramework created when building a Flutter 2.* module
        /// and used for integrating Flutter into a native iOS app.
        /// </summary>
        public string GetIosXCFrameworkPath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (Type)
            {
                case DartProjectType.Module:
                    return GetIosXCFrameworkPath(_workingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }

        }

        /// <summary>
        /// Returns the path of the iOS Framework created when building a Flutter 1.* module
        /// and used for integrating Flutter into a native iOS app.
        /// </summary>
        public string GetIosFrameworkPath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (Type)
            {
                case DartProjectType.Module:
                    return GetIosFrameworkPath(_workingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }

        }

        public DartProject(DirectoryInfo workingDir)
        {
            _workingDir = workingDir;
            Load();
        }

        public DartProject(string path) : this(new DirectoryInfo(path))
        {

        }

        private void Load()
        {
            CheckFilesAndSubfolders();
            LoadPubspecFile();
            LoadMetadataFile(); 
            LoadProjectInfo();
        }

        private void CheckFilesAndSubfolders()
        {
            if (!_workingDir.Exists)
                throw new ArgumentException("The specified directory does not exists.");

            // Check if the folder represents a real Flutter project

            // 1) It must contains a pubspec.yaml file
            FileInfo[] files = _workingDir.GetFiles(PubspecFilename);
            if (files.Length == 0)
                throw new ArgumentException($"The specified directory does not contain a '{PubspecFilename}'' file.");
            _pubspecFile = files[0];

            // 2) It must contains a .metadata file
            files = _workingDir.GetFiles(MetadataFilename);
            if (files.Length == 0)
                throw new ArgumentException($"The specified directory does not contain a '{MetadataFilename}' file.");
            _metadataFile = files[0];

            // 3) It must contains a lib folder
            DirectoryInfo[] dirs = _workingDir.GetDirectories("lib");
            if (dirs.Length == 0)
                throw new ArgumentException("The specified directory does not contain a 'lib' folder.");
            _libFolder = dirs[0];

            // 4) It should contain a test folder
            dirs = _workingDir.GetDirectories("test");
            _testFolder = dirs.Length != 0 ? dirs[0] : _workingDir.CreateSubdirectory("test");
        }

        private void LoadPubspecFile()
        {
            // YAML parser
            _pubspecStream = new YamlStream();
            using (StreamReader reader = _pubspecFile.OpenText())
            {
                _pubspecStream.Load(reader);
            }

            /*
             * name: flutter_xamarin_protocol
               description: A new Flutter package project.
               version: 0.0.1
               author:
               homepage:
               
               environment:
               sdk: ">=2.1.0 <3.0.0"
               
               dependencies:
               flutter:
               sdk: flutter
               # Your other regular dependencies here
               json_annotation: ^2.0.0
               
               dev_dependencies:
               flutter_test:
               sdk: flutter
               # Your other dev_dependencies here
               build_runner: ^1.0.0
               json_serializable: ^2.0.0
             */


            if (_pubspecStream.Documents.Count != 1)
                throw new ArgumentException($"The specified directory contains an invalid '{PubspecFilename}' file.");
            _pubspecDocument = _pubspecStream.Documents[0];
        }

        private void LoadMetadataFile()
        {
            // YAML parser
            _metadataStream = new YamlStream();
            using (StreamReader reader = _metadataFile.OpenText())
            {
                _metadataStream.Load(reader);
            }

            /*
               # This file tracks properties of this Flutter project.
               # Used by Flutter tool to assess capabilities and perform upgrades etc.
               #
               # This file should be version controlled and should not be manually edited.
               
               version:
               revision: 27321ebbad34b0a3fafe99fac037102196d655ff
               channel: stable
               
               project_type: module
            */

            if (_metadataStream.Documents.Count != 1)
                throw new ArgumentException($"The specified directory contains an invalid '{MetadataFilename}'' file.");
            _metadataDocument = _metadataStream.Documents[0];
        }

        private void LoadProjectInfo()
        {
            string projectType = _metadataDocument.GetScalarValue(new[] { "project_type" });
            switch (projectType)
            {
                case "app":
                    Type = DartProjectType.App;
                    break;
                case "module":
                    Type = DartProjectType.Module;
                    break;
                case "package":
                    Type = DartProjectType.Package;
                    break;
                default:
                    throw new ArgumentException($"Unknown Dart project type: {projectType}");
            }

            ValidatePubspecFile();

            Name = _pubspecDocument.GetScalarValue(new[] { NameYamlNode });
            Description = _pubspecDocument.GetScalarValue(new[] { DescriptionYamlNode });
            Author = _pubspecDocument.GetScalarValue(new[] { AuthorYamlNode });
            Homepage = _pubspecDocument.GetScalarValue(new[] { HomepageYamlNode });
            Version = _pubspecDocument.GetScalarValue(new[] { VersionYamlNode });
            Sdk = _pubspecDocument.GetScalarValue(new[] { EnvironmentYamlNode, SdkYamlNode });
        }

        private void ValidatePubspecFile()
        {
            YamlDocument doc = _pubspecDocument;
            DartProjectType type = Type;

            // Check if "name" exists
            YamlScalarNode name = doc.GetScalarNode(new[] { NameYamlNode });
            if (name == null)
                throw new ArgumentException($"Missing '{NameYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "description" exists
            YamlScalarNode description = doc.GetScalarNode(new[] { DescriptionYamlNode });
            if (description == null)
                throw new ArgumentException($"Missing '{DescriptionYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "author" exists
            YamlScalarNode author = doc.GetScalarNode(new[] { AuthorYamlNode });
            if (author == null && type == DartProjectType.Package)
                throw new ArgumentException($"Missing '{AuthorYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "homepage" exists
            YamlScalarNode homepage = doc.GetScalarNode(new[] { HomepageYamlNode });
            if (homepage == null && type == DartProjectType.Package)
                throw new ArgumentException($"Missing '{HomepageYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "version" exists
            YamlScalarNode version = doc.GetScalarNode(new[] { VersionYamlNode });
            if (version == null)
                throw new ArgumentException($"Missing '{VersionYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "dependencies" exists
            YamlMappingNode dependencies = doc.GetMappingNode(new[] { DependenciesYaml });
            if (dependencies == null)
                throw new ArgumentException($"Missing '{DependenciesYaml}' node inside '{PubspecFilename}' file.");

            // Check if "dev_dependencies" exists
            YamlMappingNode devDependencies = doc.GetMappingNode(new[] { DevDependenciesYaml });
            if (devDependencies == null)
                throw new ArgumentException($"Missing '{DevDependenciesYaml}' node inside '{PubspecFilename}' file.");

            // Check if "environment" exists
            YamlMappingNode environment = doc.GetMappingNode(new[] { EnvironmentYamlNode });
            if (environment == null)
                throw new ArgumentException($"Missing '{EnvironmentYamlNode}' node inside '{PubspecFilename}' file.");

            // Check if "sdk" exists
            YamlScalarNode sdk = doc.GetScalarNode(new[] { EnvironmentYamlNode, SdkYamlNode });
            if (sdk == null)
                throw new ArgumentException($"Missing '{SdkYamlNode}' node inside '{PubspecFilename}' file.");
        }

        /// <summary>
        /// Gets all the dependencies listed in the pubspec.yaml file of this Dart project,
        /// as well as their transitive dependencies.
        /// </summary>
        public void GetDependencies(bool verbose = false)
        {
            FlutterTools.GetDependencies(_workingDir.FullName, verbose);
        }

        #region pubspec.yaml utilities

        // All the relevant yaml nodes inside a Dart project pubspec.yaml file

        const string NameYamlNode = "name";
        const string DescriptionYamlNode = "description";
        const string AuthorYamlNode = "author";
        const string HomepageYamlNode = "homepage";
        const string VersionYamlNode = "version";

        const string DependenciesYaml = "dependencies";
        const string DevDependenciesYaml = "dev_dependencies";

        const string EnvironmentYamlNode = "environment";
        const string SdkYamlNode = "sdk";

        #endregion

        #region Flutter Module utilities

        /// <summary>
        /// Returns the path of the Android AAR created when building a Flutter module.
        /// </summary>
        public static string GetAndroidArchivePath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            // Please consider a Flutter module created with the following command:
            //
            // flutter create -t module --org com.example hello_world
            //
            // Now, build the module for Android integration:
            //
            // flutter build aar
            //
            // The output AAR for Debug configuration is located under:
            //
            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\flutter_debug\1.0\flutter_debug-1.0.aar

            // <MODULE_FOLDER>\build\host\outputs\repo\
            string repoRootFolder = Path.Combine(flutterFolder, "build", "host", "outputs", "repo");

            // Find the 'androidPackage' info from the pubspec.yaml file
            string package = pubspecFile.GetScalarValue(new [] { "flutter", "module", "androidPackage" });

            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\
            string packageRootFolder = repoRootFolder;
            string[] packageFolders = string.IsNullOrEmpty(package) ? new string[0] : package.Split(".");
            foreach (string folder in packageFolders)
            {
                packageRootFolder = Path.Combine(packageRootFolder, folder);
            }

            string version = "1.0";
            string build;
            switch (buildConfig)
            {
                case FlutterModuleBuildConfig.Debug:
                    build = "debug";
                    break;
                case FlutterModuleBuildConfig.Profile:
                    build = "profile";
                    break;
                case FlutterModuleBuildConfig.Release:
                    build = "release";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildConfig), buildConfig, null);
            }

            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\flutter_debug\1.0\flutter_debug-1.0.aar
            string path = Path.Combine(packageRootFolder, $"flutter_{build}", version, $"flutter_{build}-{version}.aar");
            return path;
        }

        /// <summary>
        /// Returns the path of the iOS XCFramework created when building a Flutter 2.* module with Flutter.
        /// </summary>
        public static string GetIosXCFrameworkPath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            return GetIosFrameworkPathCore(flutterFolder, pubspecFile, buildConfig, true);
        }

        /// <summary>
        /// Returns the path of the iOS Framework created when building a Flutter 1.* module with Flutter.
        /// </summary>
        public static string GetIosFrameworkPath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            return GetIosFrameworkPathCore(flutterFolder, pubspecFile, buildConfig, false);
        }

        private static string GetIosFrameworkPathCore(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig, bool xcframework)
        {
            // Please consider a Flutter module created with the following command:
            //
            // flutter create -t module --org com.example hello_world
            //
            // Now, build the module for iOS integration:
            //
            // flutter build ios-framework
            //
            // The output framework for Debug configuration is located under:
            //
            // <MODULE_FOLDER>\build\ios\Debug\App.xcframework (Flutter 2.*)
            //
            // or
            //
            // <MODULE_FOLDER>\build\ios\Debug\App.framework   (Flutter 1.*)


            // <MODULE_FOLDER>\build\ios\framework\
            string frameworkRootFolder = Path.Combine(flutterFolder, "build", "ios", "framework");

            // Find the 'iosBundleIdentifier' info from the pubspec.yaml file
            string bundle = pubspecFile.GetScalarValue(new [] { "flutter", "module", "iosBundleIdentifier" });

            string build;
            switch (buildConfig)
            {
                case FlutterModuleBuildConfig.Debug:
                    build = "Debug";
                    break;
                case FlutterModuleBuildConfig.Profile:
                    build = "Profile";
                    break;
                case FlutterModuleBuildConfig.Release:
                    build = "Release";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildConfig), buildConfig, null);
            }

            // <MODULE_FOLDER>\build\ios\Debug\App.framework
            string path = Path.Combine(frameworkRootFolder, build, xcframework ? "App.xcframework" : "App.framework");
            return path;
        }

        #endregion
    }
}
