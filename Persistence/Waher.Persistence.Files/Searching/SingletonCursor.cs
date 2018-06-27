using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a cursor into a set of a single object.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class SingletonCursor<T> : ICursor<T>
	{
		private readonly T value;
		private readonly ObjectSerializer serializer;
		private readonly Guid objectId;
		private bool isCurrent = false;

		/// <summary>
		/// Provides a cursor into a set of a single object.
		/// </summary>
		/// <param name="Value">Singleton value.</param>
		/// <param name="Serializer">Serializer of <paramref name="Value"/>.</param>
		///	<param name="ObjectId">Object ID.</param>
		internal SingletonCursor(T Value, ObjectSerializer Serializer, Guid ObjectId)
		{
			this.value = Value;
			this.serializer = Serializer;
			this.objectId = ObjectId;
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
				if (this.isCurrent)
					return this.value;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				if (this.isCurrent)
					return this.serializer;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
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
		/// Call <see cref="MoveNextAsync()"/> to start the enumeration after creating or resetting it.</exception>
		public Guid CurrentObjectId
		{
			get
			{
				if (this.isCurrent)
					return this.objectId;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MoveNextAsync()
		{
			this.isCurrent = !this.isCurrent;
			return Task.FromResult<bool>(this.isCurrent);
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MovePreviousAsync()
		{
			return this.MoveNextAsync();    // Ordering only in one direction.
		}

		public IEnumerator<T> GetEnumerator()
		{
			T[] A = new T[] { this.value };
			return (IEnumerator<T>)A.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			T[] A = new T[] { this.value };
			return A.GetEnumerator();
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
			if (SortOrder == null || SortOrder.Length != 1)
				return false;

			string s = this.serializer.ObjectIdMemberName;
			return (SortOrder[0] == s || SortOrder[0] == "+" + s);
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
			if (SortOrder == null || SortOrder.Length != 1)
				return false;

			string s = this.serializer.ObjectIdMemberName;
			return (SortOrder[0] == "-" + s);
		}

	}
}
