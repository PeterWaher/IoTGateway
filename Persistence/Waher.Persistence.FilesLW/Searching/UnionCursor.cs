using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;
using System.Collections;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a cursor that joins results from multiple cursors. It only returns an object once, regardless of how many times
	/// it appears in the different child cursors.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class UnionCursor<T> : ICursor<T>
	{
		private Dictionary<Guid, bool> returned = new Dictionary<Guid, bool>();
		private ICursor<T> currentCursor;
		private Filter[] childFilters;
		private ObjectBTreeFile file;
		private int currentCursorPosition = 0;
		private int nrCursors;
		private bool locked;

		/// <summary>
		/// Provides a cursor that joins results from multiple cursors. It only returns an object once, regardless of how many times
		/// it appears in the different child cursors.
		/// </summary>
		/// <param name="ChildFilters">Child filters.</param>
		/// <param name="File">File being searched.</param>
		/// <param name="Locked">If locked access is desired.</param>
		public UnionCursor(Filter[] ChildFilters, ObjectBTreeFile File, bool Locked)
		{
			this.childFilters = ChildFilters;
			this.nrCursors = this.childFilters.Length;
			this.file = File;
			this.currentCursor = null;
			this.locked = Locked;
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
				return this.CurrentCursor.Current;
			}
		}

		private ICursor<T> CurrentCursor
		{
			get
			{
				if (this.currentCursor == null)
					throw new InvalidOperationException("Enumeration not started or has already ended.");
				else
					return this.currentCursor;
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				return this.CurrentCursor.CurrentSerializer;
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
				return this.CurrentCursor.CurrentTypeCompatible;
			}
		}

		/// <summary>
		/// Gets the Object ID of the current object.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNextAsync()"/> to start the enumeration after creating or resetting it.</exception>
		public Guid CurrentObjectId
		{
			get
			{
				return this.CurrentCursor.CurrentObjectId;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.currentCursor != null)
			{
				this.currentCursor.Dispose();
				this.currentCursor = null;
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			Guid ObjectId;

			while (true)
			{
				if (this.currentCursor == null)
				{
					if (this.currentCursorPosition >= this.nrCursors)
						return false;

					this.currentCursor = await this.file.ConvertFilterToCursor<T>(this.childFilters[this.currentCursorPosition++], this.locked);
				}

				if (!await this.currentCursor.MoveNextAsync())
				{
					this.currentCursor.Dispose();
					this.currentCursor = null;
					continue;
				}

				if (!this.currentCursor.CurrentTypeCompatible)
					continue;

				ObjectId = this.currentCursor.CurrentObjectId;
				if (this.returned.ContainsKey(ObjectId))
					continue;

				this.returned[ObjectId] = true;
				return true;
			}
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MovePreviousAsync()
		{
			return this.MoveNextAsync();	// Union operator not ordered.
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.file.TimeoutMilliseconds);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.file.TimeoutMilliseconds);
		}

		/// <summary>
		/// If the index ordering corresponds to a given sort order.
		/// </summary>
		/// <param name="ConstantFields">Optional array of names of fields that will be constant during the enumeration.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool SameSortOrder(string[] ConstantFields, string[] SortOrder)
		{
			return false;
		}

		/// <summary>
		/// If the index ordering is a reversion of a given sort order.
		/// </summary>
		/// <param name="ConstantFields">Optional array of names of fields that will be constant during the enumeration.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool ReverseSortOrder(string[] ConstantFields, string[] SortOrder)
		{
			return false;
		}
	}
}
