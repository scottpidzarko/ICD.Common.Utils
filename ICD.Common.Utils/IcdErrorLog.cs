﻿using System;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharp;
#endif

namespace ICD.Common.Utils
{
	public static class IcdErrorLog
	{
		private const int MUTEX_TIMEOUT = 30 * 1000;
		private static readonly SafeMutex s_SafeMutex;
		private static string s_LastMessage;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static IcdErrorLog()
		{
			s_SafeMutex = new SafeMutex();
		}

		[PublicAPI]
		public static void Error(string message)
		{
			if(s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				try
				{
					s_LastMessage = message;
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_RED);
					ErrorLog.Error(message);
#else
			    System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Error.WriteLine(message);
                System.Console.ResetColor();
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Error(string message, params object[] args)
		{
			Error(string.Format(message, args));
		}

		[PublicAPI]
		public static void Warn(string message)
		{
			if (s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				try
				{
					s_LastMessage = message;
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_YELLOW);
					ErrorLog.Warn(message);
#else
			    System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Error.WriteLine(message);
			    System.Console.ResetColor();
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Warn(string message, params object[] args)
		{
			Warn(string.Format(message, args));
		}

		[PublicAPI]
		public static void Notice(string message)
		{
			if (s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				s_LastMessage = message;
				try
				{
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_BLUE);
					ErrorLog.Notice(message);
#else
			    System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.Error.WriteLine(message);
			    System.Console.ResetColor();
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Notice(string message, params object[] args)
		{
			Notice(string.Format(message, args));
		}

		[PublicAPI]
		public static void Ok(string message)
		{
			if (s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				s_LastMessage = message;
				try
				{
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_GREEN);
					ErrorLog.Ok(message);
#else
			    System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Error.WriteLine(message);
			    System.Console.ResetColor();
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Ok(string message, params object[] args)
		{
			Ok(string.Format(message, args));
		}

		[PublicAPI]
		public static void Exception(Exception ex, string message)
		{
			if (s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				s_LastMessage = message;
				try
				{
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_YELLOW_ON_RED_BACKGROUND);
					ErrorLog.Exception(message, ex);
#else
			    System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.BackgroundColor = ConsoleColor.Red;
                System.Console.Error.WriteLine("{0}: {1}", ex.GetType().Name, message);
			    System.Console.ResetColor();
                System.Console.Error.WriteLine(ex.StackTrace);
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Exception(Exception ex, string message, params object[] args)
		{
			message = string.Format(message, args);
			Exception(ex, message);
		}

		[PublicAPI]
		public static void Info(string message)
		{
			if (s_SafeMutex.WaitForMutex(MUTEX_TIMEOUT))
			{
				s_LastMessage = message;
				try
				{
#if SIMPLSHARP
					message = FormatConsoleColor(message, ConsoleColorExtensions.CONSOLE_CYAN);
					ErrorLog.Info(message);
#else
			    System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.Error.WriteLine(message);
			    System.Console.ResetColor();
#endif
				}
				finally
				{
					s_SafeMutex.ReleaseMutex();
				}
			}
			else
			{
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, "Deadlock in Error Logging, last logged message is:");
				IcdConsole.PrintLine(eConsoleColor.YellowOnRed, StringUtils.ToMixedReadableHexLiteral(s_LastMessage));
			}
		}

		[PublicAPI]
		public static void Info(string message, params object[] args)
		{
			Info(string.Format(message, args));
		}

		/// <summary>
		/// Formats the text with the given console color.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		private static string FormatConsoleColor(string text, string color)
		{
			return string.Format("{0}{1}{2}", color, text, ConsoleColorExtensions.CONSOLE_RESET);
		}
	}
}
