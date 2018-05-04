﻿using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.IO;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Common.Utils.Extensions
{
	public static class AssemblyExtensions
	{
		/// <summary>
		/// Gets the path for the assembly. Returns null if the assembly can not be found on disk.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[CanBeNull]
		public static string GetPath(this Assembly extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string path = extends
#if SIMPLSHARP
				.GetName()
#endif
				.CodeBase;

			if (path == null)
			{
#if STANDARD
				path = extends.Location;
#endif
			}
			else
			{
				const string prefix = @"file:///";
				if (path.StartsWith(prefix))
					path = path.Substring(prefix.Length);
			}

			return IcdFile.Exists(path) ? path : null;
		}

		/// <summary>
		/// Gets the creation date of the assembly.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static DateTime GetCreationTime(this Assembly extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string path = extends.GetPath();
			return path == null ? DateTime.MinValue : IcdFile.GetCreationTime(path);
		}

		/// <summary>
		/// Gets the informational version for the assembly.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[PublicAPI]
		public static string GetInformationalVersion(this Assembly extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string version;
			if (extends.TryGetInformationalVersion(out version))
				return version;

			throw new InvalidOperationException("Assembly has no informational version attribute.");
		}

		/// <summary>
		/// Tries to get the informational version for the assembly.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool TryGetInformationalVersion(this Assembly extends, out string version)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetCustomAttributes<AssemblyInformationalVersionAttribute>()
			              .Select(a => a.InformationalVersion)
			              .TryFirst(out version);
		}
	}
}
