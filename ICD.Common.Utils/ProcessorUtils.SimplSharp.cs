﻿using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
#if SIMPLSHARP
using System;
using System.Text.RegularExpressions;
using ICD.Common.Properties;

namespace ICD.Common.Utils
{
	public static partial class ProcessorUtils
	{
		private const string VER_REGEX =
			@"(?'model'\S+) (?'type'\S+) (?'lang'\S+) \[v(?'version'\d+.\d+.\d+.\d+) \((?'date'\S+ \d+ \d+)\), #(?'serial'[A-F0-9]+)\] @E-(?'mac'[a-z0-9]+)";

		private const string UPTIME_COMMAND = "uptime";
		private const string PROGUPTIME_COMMAND_ROOT = "proguptime:{0}";
		private const string UPTIME_REGEX = @".*(?'uptime'\d+ days \d{2}:\d{2}:\d{2}\.\d+)";

		private const string RAMFREE_COMMAND = "ramfree";
		private const string RAMFREE_DIGITS_REGEX = @"^(\d*)";

		private static string s_VersionResult;

		#region Properties

		/// <summary>
		/// Gets the version text from the console.
		/// </summary>
		private static string VersionResult
		{
			get
			{
				if (string.IsNullOrEmpty(s_VersionResult))
				{
					if (!IcdConsole.SendControlSystemCommand("version", ref s_VersionResult))
					{
						ServiceProvider.TryGetService<ILoggerService>()
						               .AddEntry(eSeverity.Warning, "{0} - Failed to send console command \"{1}\"",
						                         typeof(ProcessorUtils).Name, "version");
					}
				}

				return s_VersionResult;
			}
		}

		/// <summary>
		/// Gets the model name of the processor.
		/// </summary>
		[PublicAPI]
		public static string ModelName
		{
			get
			{
				Regex regex = new Regex(VER_REGEX);
				Match match = regex.Match(VersionResult);

				if (match.Success)
					return match.Groups["model"].Value;

				ServiceProvider.TryGetService<ILoggerService>()
				               .AddEntry(eSeverity.Warning, "Unable to get model name from \"{0}\"", VersionResult);
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the processor firmware version.
		/// </summary>
		[PublicAPI]
		public static Version ModelVersion
		{
			get
			{
				Regex regex = new Regex(VER_REGEX);
				Match match = regex.Match(VersionResult);

				if (match.Success)
					return new Version(match.Groups["version"].Value);

				ServiceProvider.TryGetService<ILoggerService>()
				               .AddEntry(eSeverity.Warning, "Unable to get model version from \"{0}\"", VersionResult);
				return new Version(0, 0);
			}
		}

		/// <summary>
		/// Gets the date that the firmware was updated.
		/// </summary>
		[PublicAPI]
		public static string ModelVersionDate
		{
			get
			{
				Regex regex = new Regex(VER_REGEX);
				Match match = regex.Match(VersionResult);

				if (match.Success)
					return match.Groups["date"].Value;

				ServiceProvider.TryGetService<ILoggerService>()
							   .AddEntry(eSeverity.Warning, "Unable to get model version date from \"{0}\"", VersionResult);
				
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the serial number of the processor
		/// </summary>
		[PublicAPI]
		public static string ProcessorSerialNumber
		{
			get
			{
				Regex regex = new Regex(VER_REGEX);
				Match match = regex.Match(VersionResult);

				if (!match.Success)
				{
					ServiceProvider.TryGetService<ILoggerService>()
					               .AddEntry(eSeverity.Warning, "Unable to get serial number from \"{0}\"", VersionResult);

					return string.Empty;
				}

				int decValue = int.Parse(match.Groups["serial"].Value, System.Globalization.NumberStyles.HexNumber);
				return decValue.ToString();
			}
		}

		/// <summary>
		/// Gets the ram usage in the range 0 - 1.
		/// </summary>
		public static float RamUsagePercent
		{
			get
			{
				string ramFree = GetRamFree();
				string digits = Regex.Matches(ramFree, RAMFREE_DIGITS_REGEX, RegexOptions.Multiline)[0].Groups[1].Value;
				return float.Parse(digits) / 100.0f;
			}
		}

		/// <summary>
		/// Gets the total number of bytes of physical memory.
		/// </summary>
		public static ulong RamTotalBytes
		{
			get
			{
				string ramFree = GetRamFree();
				string digits = Regex.Matches(ramFree, RAMFREE_DIGITS_REGEX, RegexOptions.Multiline)[1].Groups[1].Value;
				return ulong.Parse(digits);
			}
		}

		/// <summary>
		/// Gets the total number of bytes of physical memory being used by the control system.
		/// </summary>
		public static ulong RamUsedBytes
		{
			get
			{
				string ramFree = GetRamFree();
				string digits = Regex.Matches(ramFree, RAMFREE_DIGITS_REGEX, RegexOptions.Multiline)[2].Groups[1].Value;
				return ulong.Parse(digits);
			}
		}

		/// <summary>
		/// Gets the total number of bytes of physical memory not being used by the control system.
		/// </summary>
		public static ulong RamBytesFree
		{
			get
			{
				string ramFree = GetRamFree();
				string digits = Regex.Matches(ramFree, RAMFREE_DIGITS_REGEX, RegexOptions.Multiline)[3].Groups[1].Value;
				return ulong.Parse(digits);
			}
		}

		/// <summary>
		/// Gets the total number of bytes that can be reclaimed.
		/// </summary>
		public static ulong RamBytesReclaimable
		{
			get
			{
				string ramFree = GetRamFree();
				string digits = Regex.Matches(ramFree, RAMFREE_DIGITS_REGEX, RegexOptions.Multiline)[4].Groups[1].Value;
				return ulong.Parse(digits);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Restarts this program.
		/// </summary>
		[PublicAPI]
		public static void RestartProgram()
		{
			string consoleResult = string.Empty;
			string command = string.Format("progreset -p:{0:D2}", ProgramUtils.ProgramNumber);
			IcdConsole.SendControlSystemCommand(command, ref consoleResult);
		}

		/// <summary>
		/// Reboots the processor.
		/// </summary>
		[PublicAPI]
		public static void Reboot()
		{
			string consoleResult = string.Empty;
			IcdConsole.SendControlSystemCommand("reboot", ref consoleResult);
		}

		/// <summary>
		/// Gets the uptime for the system
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public static string GetSystemUptime()
		{
			string uptime = GetUptime();
			Match match = Regex.Match(uptime, UPTIME_REGEX);
			return match.Groups["uptime"].Value;
		}

		/// <summary>
		/// Gets the uptime 
		/// </summary>
		/// <param name="progslot"></param>
		/// <returns></returns>
		[PublicAPI]
		public static string GetProgramUptime(int progslot)
		{
			string uptime = GetUptime(progslot);
			Match match = Regex.Match(uptime, UPTIME_REGEX);
			return match.Groups["uptime"].Value;
		}

		#endregion

		/// <summary>
		/// Gets the result from the ramfree console command.
		/// </summary>
		/// <returns></returns>
		private static string GetRamFree()
		{
			string ramfree = null;
			if (!IcdConsole.SendControlSystemCommand(RAMFREE_COMMAND, ref ramfree))
			{
				ServiceProvider.TryGetService<ILoggerService>()
				               .AddEntry(eSeverity.Warning, "{0} - Failed to send console command \"{1}\"",
				                         typeof(ProcessorUtils).Name, RAMFREE_COMMAND);
			}
			return ramfree;
		}

		private static string GetUptime()
		{
			string uptime = null;
			if (!IcdConsole.SendControlSystemCommand(UPTIME_COMMAND, ref uptime))
			{
				ServiceProvider.TryGetService<ILoggerService>()
							   .AddEntry(eSeverity.Warning, "{0} - Failed to send console command \"{1}\"",
										 typeof(ProcessorUtils).Name, UPTIME_COMMAND);
			}
			return uptime;
		}

		private static string GetUptime(int programSlot)
		{
			string uptime = null;
			if (!IcdConsole.SendControlSystemCommand(string.Format(PROGUPTIME_COMMAND_ROOT, programSlot), ref uptime))
			{
				ServiceProvider.TryGetService<ILoggerService>()
							   .AddEntry(eSeverity.Warning, "{0} - Failed to send console command \"{1}\"",
										 typeof(ProcessorUtils).Name, UPTIME_COMMAND);
			}
			return uptime;
		}
	}
}

#endif
