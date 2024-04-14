namespace Usignert.Logging
{
    public interface ILoggingConfig
    {
        public LoggingExtensions.LogVerbosity Verbosity { get; set; }
    }
}
