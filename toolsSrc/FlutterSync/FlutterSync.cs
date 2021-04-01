using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace FlutterSync
{
    internal class FlutterSync
    {
        static readonly string DefaultTargetDirectory;
        static readonly string DefaultGradleCacheDirectory;

        static FlutterSync()
        {
            // NOTE: We suppose that the program executable is located under <repository root>/tools
            //       while the native references will be stored under <repository root>/assets/xamarin-native-references
            DefaultTargetDirectory = "../assets/xamarin-native-references";
            DefaultGradleCacheDirectory = OperatingSystem.IsMacOS() 
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".gradle", "caches", "modules-2", "files-2.1", "io.flutter") 
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gradle", "caches", "modules-2", "files-2.1", "io.flutter");
        }

        public static int Go(Options options)
        {
            if (options.NoAndroid && options.NoIos)
            {
                Console.WriteLine("Nothing to do.");
                return ReturnCodes.Success;
            }

            string targetFolder = !string.IsNullOrEmpty(options.TargetDirectory)
                ? Path.GetFullPath(options.TargetDirectory)
                : DefaultTargetDirectory;

            string targetFolderAndroid = Path.Combine(targetFolder, "Android");
            string targetFolderAndroidDebug = Path.Combine(targetFolderAndroid, "debug");
            string targetFolderAndroidRelease = Path.Combine(targetFolderAndroid, "release");

            string tmpTargetAndroidOfficial = Path.Combine(targetFolderAndroid, "official_repo");

            string targetFolderIos = Path.Combine(targetFolder, "iOS");
            string targetFolderIosDebug = Path.Combine(targetFolderIos, "debug");
            string targetFolderIosRelease = Path.Combine(targetFolderIos, "release");

            string gradleCacheFolder = !string.IsNullOrEmpty(options.GradleCacheDirectory)
                ? Path.GetFullPath(options.GradleCacheDirectory)
                : DefaultGradleCacheDirectory;

            string tmpFolder = Path.GetTempPath();
            string tmpModuleName = $"module_{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            string tmpModulePath = Path.Combine(tmpFolder, tmpModuleName);

            try
            {
                Console.WriteLine("Detecting Flutter version...");
                FlutterVersion version = FlutterTools.GetVersion();
                Console.WriteLine("Done (current version is {0}).", version.Version);

                Console.WriteLine("Creating temporary Flutter module..."); 
                DartProject project = FlutterTools.CreateModule(tmpFolder, tmpModuleName); 
                Console.WriteLine("Done.");

                if (!options.NoAndroid)
                {
                    Console.WriteLine("Building Android archives...");
                    FlutterTools.BuildAndroidArchive(tmpModulePath, FlutterModuleBuildConfig.Debug | FlutterModuleBuildConfig.Release);
                    Console.WriteLine("Done.");

                    if (Directory.Exists(targetFolderAndroid))
                        Directory.Delete(targetFolderAndroid, true);
                    Directory.CreateDirectory(targetFolderAndroidDebug);
                    Directory.CreateDirectory(targetFolderAndroidRelease);

                    // NOTE: The JARs / AARs found in the Gradle cache folder must be manipulated
                    //       in order for them to be embedded into a Xamarin bindings library

                    Console.WriteLine("Copying Android archives into destination folder...");

                    Directory.CreateDirectory(tmpTargetAndroidOfficial);
                    DirectoryInfo gradleCacheDir = new DirectoryInfo(gradleCacheFolder);
                    foreach (var gradleCacheArchDir in gradleCacheDir.EnumerateDirectories()
                        .Where(di => !di.Name.Contains("profile", StringComparison.OrdinalIgnoreCase)))
                    {
                        var dir = gradleCacheArchDir.GetDirectories($"*-{version.EngineRev}*")[0];
                        foreach (var subdir in dir.EnumerateDirectories())
                        {
                            var files = subdir.GetFiles();
                            if (files.Length == 0)
                                continue;

                            var file = files[0];
                            if (!string.Equals(file.Extension, ".jar", StringComparison.InvariantCultureIgnoreCase) &&
                                !string.Equals(file.Extension, ".aar", StringComparison.InvariantCultureIgnoreCase))
                                continue;

                            file.CopyTo(Path.Combine(tmpTargetAndroidOfficial, file.Name));
                        }
                    }

                    Console.WriteLine("Done.");

                    Console.WriteLine("Configuring Android archives for Xamarin bindings...");

                    foreach (string filename in Directory.EnumerateFiles(tmpTargetAndroidOfficial, "*_debug-*.jar"))
                    {
                        FileInfo fi = new FileInfo(filename);
                        if (fi.Name.StartsWith("flutter_embedding"))
                        {
                            fi.CopyTo(Path.Combine(targetFolderAndroidDebug, "flutter_embedding.jar"));
                        }
                        else
                        {
                            int index = fi.Name.IndexOf("_debug-", StringComparison.InvariantCultureIgnoreCase);
                            // https://docs.microsoft.com/en-US/xamarin/android/app-fundamentals/cpu-architectures?tabs=windows
                            // arm64_v8a must become arm64-v8a
                            // armeabi_v7a must become armeabi-v7a
                            // x86_64 must not change
                            string arch = fi.Name.Substring(0, index).Replace("_v", "-v");
                            string libFolder = Path.Combine(targetFolderAndroidDebug, "lib", arch);
                            Directory.CreateDirectory(libFolder);

                            using (ZipInputStream stream = new ZipInputStream(fi.OpenRead()))
                            {
                                ZipEntry entry;
                                while ((entry = stream.GetNextEntry()) != null)
                                {
                                    string entryFilename = Path.GetFileName(entry.Name);
                                    if (string.IsNullOrEmpty(entryFilename) ||
                                        !string.Equals(Path.GetExtension(entryFilename), ".so", StringComparison.InvariantCultureIgnoreCase))
                                        continue;

                                    using (FileStream streamWriter = File.Create(Path.Combine(libFolder, entryFilename)))
                                    {
                                        int size;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = stream.Read(data, 0, data.Length);
                                            if (size > 0)
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (string filename in Directory.EnumerateFiles(tmpTargetAndroidOfficial, "*_release-*.jar"))
                    {
                        FileInfo fi = new FileInfo(filename);
                        if (fi.Name.StartsWith("flutter_embedding"))
                        {
                            fi.CopyTo(Path.Combine(targetFolderAndroidRelease, "flutter_embedding.jar"));
                        }
                        else
                        {
                            int index = fi.Name.IndexOf("_release-", StringComparison.InvariantCultureIgnoreCase);
                            // https://docs.microsoft.com/en-US/xamarin/android/app-fundamentals/cpu-architectures?tabs=windows
                            // arm64_v8a must become arm64-v8a
                            // armeabi_v7a must become armeabi-v7a
                            // x86_64 must not change
                            string arch = fi.Name.Substring(0, index).Replace("_v", "-v");
                            string libFolder = Path.Combine(targetFolderAndroidRelease, "lib", arch);
                            Directory.CreateDirectory(libFolder);

                            using (ZipInputStream stream = new ZipInputStream(fi.OpenRead()))
                            {
                                ZipEntry entry;
                                while ((entry = stream.GetNextEntry()) != null)
                                {
                                    string entryFilename = Path.GetFileName(entry.Name);
                                    if (string.IsNullOrEmpty(entryFilename) ||
                                        !string.Equals(Path.GetExtension(entryFilename), ".so", StringComparison.InvariantCultureIgnoreCase))
                                        continue;

                                    using (FileStream streamWriter = File.Create(Path.Combine(libFolder, entryFilename)))
                                    {
                                        int size;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = stream.Read(data, 0, data.Length);
                                            if (size > 0)
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine("Done.");
                }

                if (!options.NoIos && OperatingSystem.IsMacOS())
                {
                    Console.WriteLine("Building iOS frameworks...");
                    FlutterTools.BuildIosFramework(tmpModulePath, FlutterModuleBuildConfig.Debug | FlutterModuleBuildConfig.Release);
                    Console.WriteLine("Done.");

                    if (Directory.Exists(targetFolderIos))
                        Directory.Delete(targetFolderIos, true);
                    Directory.CreateDirectory(targetFolderIosDebug);
                    Directory.CreateDirectory(targetFolderIosRelease);

                    Console.WriteLine("Copying iOS frameworks into destination folder...");

                    bool xcframework = !version.Version.StartsWith("1");
                    string appFrameworkName = xcframework ? "App.xcframework" : "App.framework";
                    string flutterFrameworkName = xcframework ? "Flutter.xcframework" : "Flutter.framework";

                    string appFrameworkDebug = xcframework
                        ? project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Debug)
                        : project.GetIosFrameworkPath(FlutterModuleBuildConfig.Debug);
                    string flutterFrameworkDebug = appFrameworkDebug.Replace(appFrameworkName, flutterFrameworkName, StringComparison.InvariantCultureIgnoreCase);
                    DirectoryInfo flutterFrameworkDebugDir = new DirectoryInfo(flutterFrameworkDebug);
                    DirectoryInfo flutterFrameworkOutputDebugDir = new DirectoryInfo(Path.Combine(targetFolderIosDebug, flutterFrameworkName));
                    flutterFrameworkOutputDebugDir.Create();
                    CopyAll(flutterFrameworkDebugDir, flutterFrameworkOutputDebugDir);

                    string appFrameworkRelease = xcframework
                        ? project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Release)
                        : project.GetIosFrameworkPath(FlutterModuleBuildConfig.Release);
                    string flutterFrameworkRelease = appFrameworkRelease.Replace(appFrameworkName, flutterFrameworkName, StringComparison.InvariantCultureIgnoreCase);
                    DirectoryInfo flutterFrameworkReleaseDir = new DirectoryInfo(flutterFrameworkRelease);
                    DirectoryInfo flutterFrameworkOutputReleaseDir = new DirectoryInfo(Path.Combine(targetFolderIosRelease, flutterFrameworkName));
                    flutterFrameworkOutputReleaseDir.Create();
                    CopyAll(flutterFrameworkReleaseDir, flutterFrameworkOutputReleaseDir);

                    Console.WriteLine("Done.");
                }

                return ReturnCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ReturnCodes.CommandExecutionError;
            }
            finally
            {
                Console.WriteLine("Clearing temporary files...");
                if (Directory.Exists(tmpModulePath))
                    Directory.Delete(tmpModulePath, true);
                if (Directory.Exists(tmpTargetAndroidOfficial))
                    Directory.Delete(tmpTargetAndroidOfficial, true);
                Console.WriteLine("Done.");
            }
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
