using CommandLine;

namespace FlutterSync
{
    public class Options
    {
        [Option(nameof(TargetDirectory), Required = true, HelpText = "Path to the folder where Flutter engine's archives and frameworks will be stored.")]
        public string TargetDirectory { get; set; }

        [Option(nameof(NoAndroid), Required = false, HelpText = "Prevents synchronizing Android archives.")]
        public bool NoAndroid { get; set; }

        [Option(nameof(NoIos), Required = false, HelpText = "Prevents synchronizing iOS frameworks.")]
        public bool NoIos { get; set; }

        [Option(nameof(GradleCacheDirectory), Required = false, HelpText = "Path to Gradle build cache where Flutter engine's AARs and JARs are stored.")]
        public string GradleCacheDirectory { get; set; }

        [Option(nameof(Verbose), Required = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        public virtual void Validate()
        {
            // Add support for validation and custom types
            // https://github.com/commandlineparser/commandline/issues/146
        }
    }
}
