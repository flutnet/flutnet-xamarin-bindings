using System;
using CommandLine;

namespace FlutterSync
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<Options>(args)
                    .MapResult(
                        FlutterSync.Go,
                        errors => ReturnCodes.OptionsParsingError);
            }
            catch (OptionsValidationException ex)
            {
                Console.WriteLine(ex.Message);
                return ReturnCodes.OptionsValidationError;
            }
        }
    }
}
