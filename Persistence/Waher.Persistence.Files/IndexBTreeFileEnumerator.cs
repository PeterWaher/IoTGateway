using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public class IndexBTreeFileEnumerator<T> : IEnumerator<T>
	{
		private ObjectBTreeFileEnumerator<object> e;
		private IndexBTreeFile file;
		private IndexRecords recordHandler;
		private Guid currentObjectId;
		private T current;
		private bool locked;
		private bool hasCurrent;

		internal IndexBTreeFileEnumerator(IndexBTreeFile File, bool Locked, IndexRecords RecordHandler)
		{
			this.file = File;
			this.locked = Locked;
			this.recordHandler = RecordHandler;

			this.Reset();

			this.e = new ObjectBTreeFileEnumerator<object>(this.file.IndexFile, Locked, RecordHandler);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.DisposeAsync().Wait();
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
			Task.Wait();
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
			BinaryDeserializer Reader = new BinaryDeserializer(this.file.IndexFile.CollectionName, this.file.IndexFile.Encoding, Key);
			this.recordHandler.SkipKey(Reader, true);
			this.currentObjectId = this.recordHandler.ObjectId;

			this.current = await this.file.ObjectFile.LoadObject<T>(this.currentObjectId);
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
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the end of the collection.</returns>
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
		}

	}
}
