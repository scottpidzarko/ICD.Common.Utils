﻿using System;
using System.Collections.Generic;
using ICD.Common.Properties;

namespace ICD.Common.Utils.Extensions
{
	/// <summary>
	/// Extension methods for working with Lists.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Adds the item into a sorted list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="extends"></param>
		/// <param name="item"></param>
		[PublicAPI]
		public static void AddSorted<T>(this List<T> extends, T item)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.AddSorted(item, Comparer<T>.Default);
		}

		/// <summary>
		/// Adds the item into a sorted list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="extends"></param>
		/// <param name="item"></param>
		/// <param name="comparer"></param>
		[PublicAPI]
		public static void AddSorted<T>(this List<T> extends, T item, IComparer<T> comparer)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (comparer == null)
				throw new ArgumentNullException("comparer");

			if (extends.Count == 0)
			{
				extends.Add(item);
				return;
			}

			if (comparer.Compare(extends[extends.Count - 1], item) <= 0)
			{
				extends.Add(item);
				return;
			}

			if (comparer.Compare(extends[0], item) >= 0)
			{
				extends.Insert(0, item);
				return;
			}

			int index = extends.BinarySearch(item, comparer);
			if (index < 0)
				index = ~index;

			extends.Insert(index, item);
		}
	}
}
