using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;
using Waher.Script;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Enumerates object in a <see cref="ObjectBTreeFile"/> in GUID order. You can use the enumerator to enumerate objects
	/// forwards and backwards, as well as skip a given number of objects.
	/// </summary>
	public class IndexBTreeFileEnumerator<T> : IEnumerator<T>, ICursor<T>
	{
		private ObjectBTreeFileEnumerator<object> e;
		private IObjectSerializer currentSerializer;
		private FilesProvider provider;
		private IndexBTreeFile file;
		private IndexRecords recordHandler;
		private Guid currentObjectId;
		private T current;
		private int timeoutMilliseconds;
		private bool locked;
		private bool hasCurrent;
		private bool currentTypeCompatible;

		internal IndexBTreeFileEnumerator(IndexBTreeFile File, bool Locked, IndexRecords RecordHandler, BlockInfo StartingPoint)
		{
			this.file = File;
			this.locked = Locked;
			this.recordHandler = RecordHandler;
			this.provider = this.file.ObjectFile.Provider;
			this.hasCurrent = false;
			this.currentObjectId = Guid.Empty;
			this.current = default(T);
			this.currentSerializer = null;
			this.timeoutMilliseconds = File.IndexFile.TimeoutMilliseconds;

			this.e = new ObjectBTreeFileEnumerator<object>(this.file.IndexFile, Locked, RecordHandler, StartingPoint);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			FilesProvider.Wait(this.DisposeAsync(), this.timeoutMilliseconds);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (this.locked)
			{
				await this.file.IndexFile.Release();
				this.locked = false;
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
		public T Current
		{
			get
			{
				if (this.hasCurrent)
					return this.current;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
		object IEnumerator.Current
		{
			get
			{
				if (this.hasCurrent)
					return this.current;
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
				if (this.hasCurrent)
					return this.currentTypeCompatible;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
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
				if (this.hasCurrent)
					return this.currentObjectId;
				else
					throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
			}
		}

		/// <summary>
		/// Gets the rank of the current object.
		/// </summary>
		public ulong CurrentRank
		{
			get
			{
				return this.e.CurrentRank;
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				return this.currentSerializer;
			}
		}

		/// <summary>
		/// Gets the rank of the current object.
		/// </summary>
		public Task<ulong> GetCurrentRank()
		{
			return this.e.GetCurrentRank();
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public bool MoveNext()
		{
			Task<bool> Task = this.MoveNextAsync();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			if (!await this.e.MoveNextAsync())
			{
				this.Reset();
				return false;
			}

			await this.LoadObject();

			return true;
		}

		private async Task LoadObject()
		{
			byte[] Key = (byte[])this.e.CurrentObjectId;
			BinaryDeserializer Reader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, Key);
			this.recordHandler.SkipKey(Reader, true);
			this.currentObjectId = this.recordHandler.ObjectId;

			try
			{
				if (this.currentSerializer == null)
					this.currentSerializer = this.provider.GetObjectSerializer(typeof(T));

				this.current = (T)await this.file.ObjectFile.LoadObject(this.currentObjectId, this.currentSerializer);
				this.currentTypeCompatible = true;
			}
			catch (Exception)
			{
				this.current = default(T);
				this.currentTypeCompatible = false;
			}

			this.hasCurrent = true;
		}

		/// <summary>
		/// Goes to the first object.
		/// </summary>
		/// <returns>If a first object was found.</returns>
		public Task<bool> GoToFirst()
		{
			return this.e.GoToFirst();
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public bool MovePrevious()
		{
			Task<bool> Task = this.MovePreviousAsync();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MovePreviousAsync()
		{
			if (!await this.e.MovePreviousAsync())
			{
				this.Reset();
				return false;
			}

			await this.LoadObject();

			return true;
		}

		/// <summary>
		/// Goes to the last object.
		/// </summary>
		/// <returns>If a last object was found.</returns>
		public Task<bool> GoToLast()
		{
			return this.e.GoToLast();
		}

		/// <summary>
		/// Finds the object given its order in the underlying database.
		/// </summary>
		/// <param name="ObjectIndex">Order of object in database.</param>
		/// <returns>If the corresponding object was found. If so, the <see cref="Current"/> property will contain the corresponding
		/// object.</returns>
		public async Task<bool> GoToObject(ulong ObjectIndex)
		{
			if (!await this.e.GoToObject(ObjectIndex))
			{
				this.Reset();
				return false;
			}

			await this.LoadObject();

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator{Object}.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.hasCurrent = false;
			this.currentObjectId = Guid.Empty;
			this.current = default(T);
			this.currentSerializer = null;

			this.e.Reset();
		}

		/// <summary>
		/// Resets the enumerator, and sets the starting point to a given starting point.
		/// </summary>
		/// <param name="StartingPoint">Starting point to start enumeration.</param>
		public void Reset(Bookmark StartingPoint)
		{
			this.hasCurrent = false;
			this.currentObjectId = Guid.Empty;
			this.current = default(T);
			this.currentSerializer = null;

			this.e.Reset(StartingPoint);
		}

		/// <summary>
		/// Skips a certain number of objects forward (positive <paramref name="NrObjects") or backward (negative <param name="NrObjects")./>
		/// </summary>
		/// <param name="NrObjects">Number of objects to skip forward (positive) or backward (negative).</param>
		/// <returns>If the skip operation was successful and a new object is available in <see cref="Current"/>.</returns>
		public async Task<bool> Skip(long NrObjects)
		{
			long Rank = (long)await this.GetCurrentRank();

			Rank += NrObjects;
			if (Rank < 0)
				return false;

			if (!await this.GoToObject((ulong)Rank))
				return false;

			return true;
		}

		/// <summary>
		/// Gets a bookmark for the current position. You can set the current position of the enumerator, calling the
		/// <see cref="Reset(Bookmark)"/> method.
		/// </summary>
		/// <returns>Bookmark</returns>
		public Task<Bookmark> GetBookmark()
		{
			return this.e.GetBookmark();
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
			return this.file.SameSortOrder(SortOrder);
		}

		/// <summary>
		/// If the index ordering is a reversion of a given sort order.
		/// </summary>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool ReverseSortOrder(params string[] SortOrder)
		{
			return this.file.ReverseSortOrder(SortOrder);
		}

	}
}
	