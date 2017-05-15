using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a cursor into a sorted set of objects.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class SortedCursor<T> : ICursor<T>
	{
		private SortedDictionary<SortRec, Tuple<T, IObjectSerializer, Guid>> sortedObjects;
		private SortedDictionary<SortRec, Tuple<T, IObjectSerializer, Guid>>.ValueCollection.Enumerator e;
		private IndexRecords recordHandler;
		private int timeoutMilliseconds;

		/// <summary>
		/// Provides a cursor into a sorted set of objects.
		/// </summary>
		/// <param name="SortedObjects">Sorted set of objects.</param>
		/// <param name="TimeoutMilliseconds">Time to wait to get access to underlying database.</param>
		internal SortedCursor(SortedDictionary<SortRec, Tuple<T, IObjectSerializer, Guid>> SortedObjects, IndexRecords RecordHandler,
			int TimeoutMilliseconds)
		{
			this.sortedObjects = SortedObjects;
			this.recordHandler = RecordHandler;
			e = this.sortedObjects.Values.GetEnumerator();
			this.timeoutMilliseconds = TimeoutMilliseconds;
		}

		internal class SortRec : IComparable
		{
			private byte[] key;
			private IComparer<byte[]> comparer;

			internal SortRec(byte[] Key, IComparer<byte[]> Comparer)
			{
				this.key = Key;
				this.comparer = Comparer;
			}

			public int CompareTo(object obj)
			{
				SortRec Rec = (SortRec)obj;
				return this.comparer.Compare(this.key, Rec.key);
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNextAsync()"/> to start the enumeration after creating or resetting it.</exception>
		public T Current
		{
			get
			{
				return this.e.Current.Item1;
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				return this.e.Current.Item2;
			}
		}

		/// <summary>
		/// If the curent object is type compatible with <typeparamref name="T"/> or not. If not compatible, <see cref="Current"/> 
		/// will be null, even if there exists an object at the current position.
		/// </summary>
		public bool CurrentTypeCompatible
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the Object ID of the current object.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
		public Guid CurrentObjectId
		{
			get
			{
				return this.e.Current.Item3;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.e.Dispose();
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MoveNextAsync()
		{
			return Task.FromResult<bool>(this.e.MoveNext());
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MovePreviousAsync()
		{
			return this.MoveNextAsync();	// Ordering only in one direction.
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.timeoutMilliseconds);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.timeoutMilliseconds);
		}

		/// <summary>
		/// If the index ordering corresponds to a given sort order.
		/// </summary>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool SameSortOrder(params string[] SortOrder)
		{
			return this.recordHandler.SameSortOrder(SortOrder);
		}

		/// <summary>
		/// If the index ordering is a reversion of a given sort order.
		/// </summary>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool ReverseSortOrder(params string[] SortOrder)
		{
			return this.recordHandler.ReverseSortOrder(SortOrder);
		}

	}
}
