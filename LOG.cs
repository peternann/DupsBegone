using System;
using System.Text;

//using MonoDevelop.Core.Logging;

namespace DupsBegone
{
	/// <summary>
	/// A simple static logging class for one-line logging calls.
	/// /
	/// </summary>
	public static class LOG
	{
//		private static ILogger logger = new ConsoleLogger();

		public static void d(string message)
		{
//			logger.Log(LogLevel.Debug,message);
			Console.WriteLine(message);
		}
		public static void d(StringBuilder message)
		{
			d(message.ToString());
		}

	}
}

