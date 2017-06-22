﻿using System;
#if SIMPLSHARP
using Crestron.SimplSharp;
#else
using System.Reflection;
#endif

namespace ICD.Common.Utils.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsAssignableTo(this Type from, Type to)
		{
			return to.IsAssignableFrom(from);
		}
	}
}