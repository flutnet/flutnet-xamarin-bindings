using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CommandLine;

namespace FlutterSync
{
    internal class OptionsValidationException : ValidationException
    {
        public OptionsValidationException(string message) : base(message)
        {
        }

        public OptionsValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public OptionsValidationException(Type verbType, string message) : base(message)
        {
            Verb = (VerbAttribute) verbType.GetCustomAttribute(typeof(VerbAttribute));
        }

        public OptionsValidationException(Type verbType, string message, Exception innerException) : base(message, innerException)
        {
            Verb = (VerbAttribute) verbType.GetCustomAttribute(typeof(VerbAttribute));
        }

        public VerbAttribute Verb { get; }
    }
}