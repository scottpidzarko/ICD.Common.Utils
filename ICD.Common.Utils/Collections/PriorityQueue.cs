﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Common.Utils.Collections
{
	/// <summary>
	/// Provides a first-in first-out collection with enhanced insertion features.
	/// </summary>
	public sealed class PriorityQueue<T> : IEnumerable<T>, ICollection
	{
		private readonly IcdOrderedDictionary<int, List<T>> m_PriorityToQueue;
		private int m_Count;

		#region Properties

		/// <summary>
		/// Gets the number of items in the collection.
		/// </summary>
		public int Count { get { return m_Count; } }

		/// <summary>
		/// Is the collection thread safe?
		/// </summary>
		public bool IsSynchronized { get { return false; } }

		/// <summary>
		/// Gets a reference for locking.
		/// </summary>
		public object SyncRoot { get { return this; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public PriorityQueue()
		{
			m_PriorityToQueue = new IcdOrderedDictionary<int, List<T>>();
		}

		#region Methods

		/// <summary>
		/// Clears the collection.
		/// </summary>
		[PublicAPI]
		public void Clear()
		{
			m_PriorityToQueue.Clear();
			m_Count = 0;
		}

		/// <summary>
		/// Adds the item to the end of the queue.
		/// </summary>
		/// <param name="item"></param>
		[PublicAPI]
		public void Enqueue(T item)
		{
			Enqueue(item, int.MaxValue);
		}

		/// <summary>
		/// Adds the item to the queue with the given priority.
		/// Lower values are dequeued first.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="priority"></param>
		[PublicAPI]
		public void Enqueue(T item, int priority)
		{
			List<T> queue;
			if (!m_PriorityToQueue.TryGetValue(priority, out queue))
			{
				queue = new List<T>();
				m_PriorityToQueue.Add(priority, queue);
			}

			queue.Add(item);
			m_Count++;
		}

		/// <summary>
		/// Adds the item to the queue with the given priority at the given index.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="priority"></param>
		/// <param name="position"></param>
		[PublicAPI]
		public void Enqueue([CanBeNull] T item, int priority, int position)
		{
			m_PriorityToQueue.GetOrAddNew(priority, ()=> new List<T>())
			                 .Insert(position, item);
			m_Count++;
		}

		/// <summary>
		/// Enqueues the item at the beginning of the queue.
		/// </summary>
		/// <param name="item"></param>
		[PublicAPI]
		public void EnqueueFirst(T item)
		{
			const int priority = int.MinValue;

			List<T> queue;
			if (!m_PriorityToQueue.TryGetValue(priority, out queue))
			{
				queue = new List<T>();
				m_PriorityToQueue.Add(priority, queue);
			}

			queue.Insert(0, item);
			m_Count++;
		}

		/// <summary>
		/// Removes any items in the queue matching the predicate.
		/// Appends the given item at the end of the given priority level.
		/// This is useful for reducing duplication, or replacing items with something more pertinent.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="remove"></param>
		[PublicAPI]
		public void EnqueueRemove(T item, Func<T, bool> remove)
		{
			if (remove == null)
				throw new ArgumentNullException("remove");

			EnqueueRemove(item, remove, int.MaxValue, false);
		}

		/// <summary>
		/// Removes any items in the queue matching the predicate.
		/// Appends the given item at the end of the given priority level.
		/// This is useful for reducing duplication, or replacing items with something more pertinent.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="remove"></param>
		/// <param name="priority"></param>
		/// <param name="deDuplicateToEndOfQueue"></param>
		[PublicAPI]
		public void EnqueueRemove([CanBeNull] T item, [NotNull] Func<T, bool> remove, int priority, bool deDuplicateToEndOfQueue)
		{
			if (remove == null)
				throw new ArgumentNullException("remove");

			int lowestMatchingPriority = int.MaxValue;
			int? firstMatchingIndex = null;

			foreach (KeyValuePair<int, List<T>> kvp in m_PriorityToQueue.ToArray())
			{
				int[] removeIndices =
					kvp.Value
					   .FindIndices(v => remove(v))
					   .Reverse()
					   .ToArray();

				if (removeIndices.Any() && kvp.Key < lowestMatchingPriority )
				{
					lowestMatchingPriority = kvp.Key;
					firstMatchingIndex = removeIndices.Last();
				}

				foreach (int removeIndex in removeIndices)
				{
					kvp.Value.RemoveAt(removeIndex);
					m_Count--;
				}

				if (kvp.Value.Count == 0)
					m_PriorityToQueue.Remove(kvp.Key);
			}


			if(deDuplicateToEndOfQueue)
				Enqueue(item, priority);
			else
			{
				if(firstMatchingIndex == null)
					Enqueue(item, lowestMatchingPriority);
				else
					Enqueue(item, lowestMatchingPriority, firstMatchingIndex.Value);
			}
		}

		/// <summary>
		/// Dequeues the first item with the lowest priority value.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public T Dequeue()
		{
			T output;
			if (TryDequeue(out output))
				return output;

			throw new InvalidOperationException("The queue is empty.");
		}

		/// <summary>
		/// Attempts to dequeue an item from the queue.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool TryDequeue(out T output)
		{
			while (true)
			{
				output = default(T);

				KeyValuePair<int, List<T>> kvp;
				if (!m_PriorityToQueue.TryFirst(out kvp))
					return false;

				int priority = kvp.Key;
				List<T> queue = kvp.Value;

				bool found = queue.TryFirst(out output);
				if (found)
				{
					queue.RemoveAt(0);
					m_Count--;
				}

				if (queue.Count == 0)
					m_PriorityToQueue.Remove(priority);

				if (found)
					return true;
			}
		}

		/// <summary>
		/// Gets an enumerator for the items.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return m_PriorityToQueue.Values
			                        .SelectMany(v => v)
			                        .GetEnumerator();
		}

		/// <summary>
		/// Copies the collection to the target array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			foreach (T item in this)
				array.SetValue(item, index++);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets an enumerator for the items.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
