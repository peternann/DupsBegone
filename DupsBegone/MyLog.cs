using System;

// Concept from: http://stackoverflow.com/questions/5646820/logger-wrapper-best-practice

namespace DupsBegone
{
	public interface ILogger
	{
		void Log(LogEntry entry);
	}

	public enum LoggingEventType { Debug, Information, Warning, Error, Fatal };

	// Immutable DTO that contains the log information.
	public class LogEntry 
	{
		public readonly LoggingEventType Severity;
		public readonly string Message;
		public readonly Exception Exception;

		public LogEntry(LoggingEventType severity, string message, Exception exception = null)
		{
//			Requires.IsNotNullOrEmpty(message, "message");
//			Requires.IsValidEnum(severity, "severity");
			this.Severity = severity;
			this.Message = message;
			this.Exception = exception;
		}
	}

	//
	// This 'Extension Class' using "this * *" params essentially allows the methods to be called on a logger.
	// Like so:
	//  ILogger logger = new ConsoleLogger();   // ConsoleLogger being an impl of ILogger
	//  logger.d("message");     // Note method 'd' is not in logger, it is supplied via LoggerExtensions:
	//
	public static class LoggerExtensions
	{
		public static void d(this ILogger logger, string message) {
			logger.Log(new LogEntry(LoggingEventType.Debug, message));
		}

		public static void Log(this ILogger logger, string message) {
			logger.Log(new LogEntry(LoggingEventType.Information, message));
		}

		public static void Log(this ILogger logger, Exception exception) {
			logger.Log(new LogEntry(LoggingEventType.Error, exception.Message, exception));
		}


		// More methods here.
	}

	public class ConsoleLogger : ILogger
	{
		public void Log(LogEntry entry)
		{
			Console.WriteLine(entry.Message);
		}
	}
}

