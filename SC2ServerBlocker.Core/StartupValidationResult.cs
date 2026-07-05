using System;

namespace SC2ServerBlocker
{
    public class FirewallOperationException : Exception
    {
        public FirewallOperationException(string message)
            : base(message)
        {
        }

        public FirewallOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class StartupValidationResult
    {
        private StartupValidationResult(StartupValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public StartupValidationSeverity Severity { get; private set; }

        public string Message { get; private set; }

        public bool AllowsBlocking
        {
            get { return Severity != StartupValidationSeverity.Error; }
        }

        public static StartupValidationResult Ok()
        {
            return new StartupValidationResult(StartupValidationSeverity.None, null);
        }

        public static StartupValidationResult Warning(string message)
        {
            return new StartupValidationResult(StartupValidationSeverity.Warning, message);
        }

        public static StartupValidationResult Error(string message)
        {
            return new StartupValidationResult(StartupValidationSeverity.Error, message);
        }
    }

    public enum StartupValidationSeverity
    {
        None,
        Warning,
        Error
    }
}
