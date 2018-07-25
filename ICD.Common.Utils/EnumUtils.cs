﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Common.Utils
{
	public static class EnumUtils
	{
		private static readonly Dictionary<Type, object> s_EnumValuesCache;
		private static readonly Dictionary<Type, Dictionary<int, object>> s_EnumFlagsCache;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static EnumUtils()
		{
			s_EnumValuesCache = new Dictionary<Type, object>();
			s_EnumFlagsCache = new Dictionary<Type, Dictionary<int, object>>();
		}

		/// <summary>
		/// Returns true if the given type is an enum.
		/// </summary>
		/// <returns></returns>
		public static bool IsEnumType<T>()
		{
			return IsEnumType(typeof(T));
		}

		/// <summary>
		/// Returns true if the given type is an enum.
		/// </summary>
		/// <returns></returns>
		private static bool IsEnumType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return type
#if !SIMPLSHARP
				       .GetTypeInfo()
#endif
				       .IsEnum || type.IsAssignableTo(typeof(Enum));
		}

		/// <summary>
		/// Returns true if the given value is an enum.
		/// </summary>
		/// <returns></returns>
		public static bool IsEnum<T>(T value)
		{
// ReSharper disable once CompareNonConstrainedGenericWithNull
			return value != null && IsEnumType(value.GetType());
		}

		/// <summary>
		/// Returns true if the given value is defined as part of the given enum type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDefined<T>(T value)
		{
			if (!IsEnumType<T>())
				throw new InvalidOperationException(string.Format("{0} is not an enum", typeof(T).Name));

			if (!IsFlagsEnum<T>())
				return GetValues<T>().Any(v => v.Equals(value));

			int valueInt = (int)(object)value;

			// Check if all of the flag values are defined
			foreach (T flag in GetFlags(value))
			{
				int flagInt = (int)(object)flag;
				valueInt = valueInt - flagInt;
			}

			return valueInt == 0;
		}

		#region Values

		/// <summary>
		/// Gets the values from an enumeration.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetValues<T>()
		{
			Type type = typeof(T);

			// Reflection is slow and this method is called a lot, so we cache the results.
			object cache;
			if (!s_EnumValuesCache.TryGetValue(type, out cache))
			{
				cache = GetValuesUncached<T>().ToArray();
				s_EnumValuesCache[type] = cache;
			}

			return cache as T[];
		}

		/// <summary>
		/// Gets the values from an enumeration without performing any caching. This is slow because of reflection.
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<T> GetValuesUncached<T>()
		{
			Type type = typeof(T);

			if (!IsEnumType<T>())
				throw new InvalidOperationException(string.Format("{0} is not an enum", type.Name));

			return type
#if SIMPLSHARP
				.GetCType()
#else
				.GetTypeInfo()
#endif
				.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(x => x.GetValue(null))
				.Cast<T>();
		}

		/// <summary>
		/// Gets the values from an enumeration except the 0 value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetValuesExceptNone<T>()
		{
			return GetFlagsExceptNone<T>();
		}

		#endregion

		#region Flags

		/// <summary>
		/// Returns true if the given enum type has the Flags attribute set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool IsFlagsEnum<T>()
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			return typeof(T)
#if !SIMPLSHARP
                .GetTypeInfo()
#endif
				.IsDefined(typeof(FlagsAttribute), false);
		}

		/// <summary>
		/// Gets the overlapping values of the given enum flags.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <returns></returns>
		public static T GetFlagsIntersection<T>(params T[] values)
		{
			if (values.Length == 0)
				return default(T);

			int output = (int)(object)values.First();
			foreach (T item in values.Skip(1))
				output &= (int)(object)item;

			return (T)Enum.ToObject(typeof(T), output);
		}

		/// <summary>
		/// Gets the overlapping values of the given enum flags.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static T GetFlagsIntersection<T>(T a, T b)
		{
			int aInt = (int)(object)a;
			int bInt = (int)(object)b;

			return (T)Enum.ToObject(typeof(T), aInt & bInt);
		}

		/// <summary>
		/// Gets all of the set flags on the given enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetFlags<T>(T value)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			Type type = typeof(T);
			int valueInt = (int)(object)value;

			Dictionary<int, object> cache;
			if (!s_EnumFlagsCache.TryGetValue(type, out cache))
			{
				cache = new Dictionary<int, object>();
				s_EnumFlagsCache[type] = cache;
			}

			object flags;
			if (!cache.TryGetValue(valueInt, out flags))
			{
				flags = GetValues<T>().Where(e => HasFlag(value, e)).ToArray();
				cache[valueInt] = flags;
			}

			return flags as T[];
		}

		/// <summary>
		/// Gets all of the set flags on the given enum type except 0.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetFlagsExceptNone<T>()
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			T allValue = GetFlagsAllValue<T>();
			return GetFlagsExceptNone(allValue);
		}

		/// <summary>
		/// Gets all of the set flags on the given enum except 0.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetFlagsExceptNone<T>(T value)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			return GetFlags(value).Except(default(T));
		}

		/// <summary>
		/// Gets all of the flag combinations on the given flag enum value.
		/// 
		/// IE: If you have an enum type with flags{a, b, c}, and you pass this method {a|b},
		/// It will return {a, b, a|b}
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetAllFlagCombinationsExceptNone<T>(T value)
		{
			if (!IsEnum(value))
				// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			int maxEnumValue = (GetValues<T>().Max(v => (int)(object)v) * 2) -1;
			return Enumerable.Range(1, maxEnumValue).Select(i => (T)(object)i ).Where(v => HasFlags(value, v));
		}

		/// <summary>
		/// Gets an enum value of the given type with every flag set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetFlagsAllValue<T>()
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			int output = GetValues<T>().Aggregate(0, (current, value) => current | (int)(object)value);
			return (T)Enum.ToObject(typeof(T), output);
		}

		/// <summary>
		/// Returns true if the enum contains the given flag.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public static bool HasFlag<T>(T value, T flag)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			if (!IsEnum(flag))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", flag == null ? "NULL" : flag.GetType().Name), "flag");

			return ToEnum(value).HasFlag(ToEnum(flag));
		}

		/// <summary>
		/// Returns true if the enum contains all of the given flags.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static bool HasFlags<T>(T value, T flags)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			if (!IsEnum(flags))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", flags == null ? "NULL" : flags.GetType().Name), "flags");

			return ToEnum(value).HasFlags(ToEnum(flags));
		}

		/// <summary>
		/// Returns true if only a single flag is set on the given enum value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool HasSingleFlag<T>(T value)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			return HasAnyFlags(value) && !HasMultipleFlags(value);
		}

		/// <summary>
		/// Returns true if the enum has more than 1 flag set.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool HasMultipleFlags<T>(T value)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			return HasMultipleFlags((int)(object)value);
		}

		/// <summary>
		/// Returns true if the enum has more than 1 flag set.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool HasMultipleFlags(int value)
		{
			return (value & (value - 1)) != 0;
		}

		/// <summary>
		/// Returns true if the enum contains any flags.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool HasAnyFlags<T>(T value)
		{
			return HasAnyFlags((int)(object)value);
		}

		/// <summary>
		/// Returns true if the enum has any flags set.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool HasAnyFlags(int value)
		{
			return value > 0;
		}

		/// <summary>
		/// Returns true if the enum contains any of the given flag values.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		[PublicAPI]
		public static bool HasAnyFlags<T>(T value, T other)
		{
			T intersection = GetFlagsIntersection(value, other);
			return HasAnyFlags(intersection);
		}

		#endregion

		#region Conversion

		/// <summary>
		/// Shorthand for parsing string to enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static T Parse<T>(string data, bool ignoreCase)
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			T output;
			if (TryParse(data, ignoreCase, out output))
				return output;

			string message = string.Format("Failed to parse {0} as {1}", StringUtils.ToRepresentation(data), typeof(T).Name);
			throw new FormatException(message);
		}

		/// <summary>
		/// Shorthand for parsing a string to enum. Returns false if the parse failed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse<T>(string data, bool ignoreCase, out T result)
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			result = default(T);

			try
			{
				result = (T)Enum.Parse(typeof(T), data, ignoreCase);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Shorthand for parsing string to enum.
		/// Will fail if the resulting value is not defined as part of the enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static T ParseStrict<T>(string data, bool ignoreCase)
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			T output;

			try
			{
				output = Parse<T>(data, ignoreCase);
			}
			catch (Exception e)
			{
				throw new FormatException(
					string.Format("Failed to parse {0} as {1}", StringUtils.ToRepresentation(data), typeof(T).Name), e);
			}

			if (!IsDefined(output))
				throw new ArgumentOutOfRangeException(string.Format("{0} is not a valid {1}", output, typeof(T).Name));

			return output;
		}

		/// <summary>
		/// Shorthand for parsing a string to enum. Returns false if the parse failed.
		/// Will fail if the resulting value is not defined as part of the enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseStrict<T>(string data, bool ignoreCase, out T result)
		{
			if (!IsEnumType<T>())
				throw new ArgumentException(string.Format("{0} is not an enum", typeof(T).Name));

			result = default(T);

			try
			{
				result = ParseStrict<T>(data, ignoreCase);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Converts the given enum value to an Enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Enum ToEnum<T>(T value)
		{
			if (!IsEnum(value))
// ReSharper disable once CompareNonConstrainedGenericWithNull
				throw new ArgumentException(string.Format("{0} is not an enum", value == null ? "NULL" : value.GetType().Name), "value");

			return (Enum)(object)value;
		}

		#endregion
	}
}
