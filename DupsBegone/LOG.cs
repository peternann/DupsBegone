﻿using System;

namespace DupsBegone
{
	/// <summary>
	/// A simple static logging class for one-line logging calls.
	/// /
	/// </summary>
	public static class LOG
	{
		private static ILogger logger = new ConsoleLogger();

		public static void d(string message)
		{
			logger.d(message);
		}

	}
}
