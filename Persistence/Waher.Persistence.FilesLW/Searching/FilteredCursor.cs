using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a filtered cursor. It only returns objects that matches a given filter.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class FilteredCursor<T> : ICursor<T>
	{
		private readonly ICursor<T> cursor;
		private readonly IApplicableFilter filter;
		private readonly FilesProvider provider;
		private readonly string[] constantFields;
		private readonly int timeoutMilliseconds;
		private readonly bool untilFirstFail;
		private readonly bool forward;

		/// <summary>
		/// Provides a filtered cursor. It only returns objects that matches a given filter.
		/// </summary>
		/// <param name="Cursor">Cursor to be filtered.</param>
		/// <param name="Filter">Filter to apply.</param>
		/// <param name="UntilFirstFail">Only return ites until first filter failure.</param>
		/// <param name="Forward">If <paramref name="Cursor"/> is to be processed forwards (true) or backwards (false).</param>
		/// <param name="TimeoutMilliseconds">Time to wait to get access to underlying database.</param>
		/// <param name="Provider">Files provider.</param>
		public FilteredCursor(ICursor<T> Cursor, IApplicableFilter Filter, bool UntilFirstFail, bool Forward, int TimeoutMilliseconds,
			FilesProvider Provider)
		{
			this.cursor = Cursor;
			this.filter = Filter;
			this.untilFirstFail = UntilFirstFail;
			this.forward = Forward;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.provider = Provider;
			this.constantFields = Filter?.ConstantFields;
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
				return this.cursor.Current;
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				return this.cursor.CurrentSerializer;
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
				return this.cursor.CurrentTypeCompatible;
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
				return this.cursor.CurrentObjectId;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.cursor.Dispose();
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			while (true)
			{
				if (this.forward)
				{
					if (!await this.cursor.MoveNextAsync())
						return false;
				}
				else
				{
					if (!await this.cursor.MovePreviousAsync())
						return false;
				}

				if (!this.cursor.CurrentTypeCompatible)
					continue;
				
				if (!(this.filter is null) && !await this.filter.AppliesTo(this.cursor.Current, this.cursor.CurrentSerializer, this.provider))
				{
					if (this.untilFirstFail)
						return false;
					else
						continue;
				}

				return true;
			}
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MovePreviousAsync()
		{
			while (true)
			{
				if (this.forward)
				{
					if (!await this.cursor.MovePreviousAsync())
						return false;
				}
				else
				{
					if (!await this.cursor.MoveNextAsync())
						return false;
				}

				if (!this.cursor.CurrentTypeCompatible)
					continue;

				if (!(this.filter is null) && !await this.filter.AppliesTo(this.cursor.Current, this.cursor.CurrentSerializer, this.provider))
				{
					if (this.untilFirstFail)
						return false;
					else
						continue;
				}

				return true;
			}
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
		/// <param name="ConstantFields">Optional array of names of fields that will be constant during the enumeration.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool SameSortOrder(string[] ConstantFields, string[] SortOrder)
		{
			ConstantFields = FilterAnd.MergeConstantFields(ConstantFields, this.constantFields);

			if (this.forward)
				return this.cursor.SameSortOrder(ConstantFields, SortOrder);
			else
				return this.cursor.ReverseSortOrder(ConstantFields, SortOrder);
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
			ConstantFields = FilterAnd.MergeConstantFields(ConstantFields, this.constantFields);

			if (this.forward)
				return this.cursor.ReverseSortOrder(ConstantFields, SortOrder);
			else
				return this.cursor.SameSortOrder(ConstantFields, SortOrder);
		}

	}
}
