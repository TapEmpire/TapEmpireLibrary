namespace TapEmpire.Utility
{
    public readonly struct Issue
    {
        public readonly IssueSeverity Severity;
        public readonly string Message;

        private Issue(IssueSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public static Issue Error(string message) => new(IssueSeverity.Error, message);

        public static Issue Warning(string message) => new(IssueSeverity.Warning, message);

        public override string ToString() => $"{Severity}: {Message}";
    }

    public enum IssueSeverity
    {
        Warning,
        Error,
    }
}
