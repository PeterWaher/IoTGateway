using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a filtered cursor. It only returns objects that matches a given filter.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class FilteredCursor<T> : ICursor<T>
	{
		private ICursor<T> cursor;
		private IApplicableFilter filter;
		private bool untilFirstFail;
		private bool forward;

		/// <summary>
		/// Provides a filtered cursor. It only returns objects that matches a given filter.
		/// </summary>
		/// <param name="Cursor">Cursor to be filtered.</param>
		/// <param name="Filter">Filter to apply.</param>
		/// <param name="UntilFirstFail">Only return ites until first filter failure.</param>
		/// <param name="Forward">If <paramref name="Cursor"/> is to be processed forwards (true) or backwards (false).</param>
		public FilteredCursor(ICursor<T> Cursor, IApplicableFilter Filter, bool UntilFirstFail, bool Forward)
		{
			this.cursor = Cursor;
			this.filter = Filter;
			this.untilFirstFail = UntilFirstFail;
			this.forward = Forward;
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
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
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

				if (this.filter != null && !this.filter.AppliesTo(this.cursor.Current, this.cursor.CurrentSerializer))
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

				if (this.filter != null && !this.filter.AppliesTo(this.cursor.Current, this.cursor.CurrentSerializer))
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
			return new CursorEnumerator<T>(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CursorEnumerator<T>(this);
		}

	}
}
