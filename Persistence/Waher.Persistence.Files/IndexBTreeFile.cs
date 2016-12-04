using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages an index file to a <see cref="ObjectBTreeFile"/>.
	/// </summary>
	public class IndexBTreeFile : IDisposable, IEnumerable<object>
	{
		private LinkedList<Tuple<byte[], bool, IObjectSerializer>> queue = new LinkedList<Tuple<byte[], bool, IObjectSerializer>>();
		private ObjectBTreeFile objectFile;
		private ObjectBTreeFile indexFile;
		private IndexRecords recordHandler;

		public IndexBTreeFile(string FileName, int BlocksInCache, ObjectBTreeFile ObjectFile, FilesProvider Provider, params string[] FieldNames)
		{
			this.objectFile = ObjectFile;
			this.recordHandler = new IndexRecords(this.objectFile.CollectionName, this.objectFile.Encoding,
				this.objectFile.InlineObjectSizeLimit, FieldNames);
			this.indexFile = new ObjectBTreeFile(FileName, string.Empty, string.Empty, this.objectFile.BlockSize, BlocksInCache,
				this.objectFile.BlobBlockSize, Provider, this.objectFile.Encoding, this.objectFile.TimeoutMilliseconds,
				this.objectFile.Encrypted, this.recordHandler);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.indexFile != null)
			{
				this.indexFile.Dispose();
				this.indexFile = null;

				this.objectFile = null;
				this.recordHandler = null;
			}
		}

		/// <summary>
		/// Object file.
		/// </summary>
		public ObjectBTreeFile ObjectFile
		{
			get { return this.objectFile; }
		}

		/// <summary>
		/// Index file.
		/// </summary>
		public ObjectBTreeFile IndexFile
		{
			get { return this.indexFile; }
		}

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer);
			if (Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			lock (this.queue)
			{
				this.queue.AddLast(new Tuple<byte[], bool, IObjectSerializer>(Bin, true, Serializer));
			}

			await this.indexFile.Lock();
			try
			{
				await this.ProcessQueueLocked();
			}
			finally
			{
				await this.indexFile.Release();
			}

			return true;
		}

		/// <summary>
		/// Deletes an object from the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object to delete.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> DeleteObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer);
			if (Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			lock (this.queue)
			{
				this.queue.AddLast(new Tuple<byte[], bool, IObjectSerializer>(Bin, false, Serializer));
			}

			await this.indexFile.Lock();
			try
			{
				await this.ProcessQueueLocked();
			}
			finally
			{
				await this.indexFile.Release();
			}

			return true;
		}

		private async Task ProcessQueueLocked()
		{
			Tuple<byte[], bool, IObjectSerializer> P;
			IObjectSerializer Serializer;
			byte[] Bin;
			bool Add;

			while (true)
			{
				lock (this.queue)
				{
					if (this.queue.First == null)
						return;

					P = this.queue.First.Value;
					this.queue.RemoveFirst();

					Bin = P.Item1;
					Add = P.Item2;
					Serializer = P.Item3;
				}

				if (Bin == null && Serializer == null)
					await this.indexFile.ClearAsync();
				else if (Add)
				{
					BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
					await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
				}
				else
					await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null);
			}
		}

		/// <summary>
		/// Clears the database of all objects.
		/// </summary>
		/// <returns>Task object.</returns>
		internal async Task ClearAsync()
		{
			lock (this.queue)
			{
				this.queue.AddLast(new Tuple<byte[], bool, IObjectSerializer>(null, true, null));
			}

			await this.indexFile.Lock();
			try
			{
				await this.ProcessQueueLocked();
			}
			finally
			{
				await this.indexFile.Release();
			}
		}

		/// <summary>
		/// Waits until pending tasks have completed.
		/// </summary>
		/// <returns>Awaitable task object.</returns>
		public async Task WaitComplete()
		{
			await this.indexFile.Lock();
			try
			{
				await this.ProcessQueueLocked();
			}
			finally
			{
				await this.indexFile.Release();
			}
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<object> GetEnumerator()
		{
			return new IndexBTreeFileEnumerator<object>(this, false, this.recordHandler);
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new IndexBTreeFileEnumerator<object>(this, false, this.recordHandler);
		}

		/// <summary>
		/// Returns an typed enumerator that iterates through the collection in the order specified by the index. The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.
		/// </summary>
		/// <param name="Locked">If locked access to the file is requested.
		/// 
		/// If unlocked access is desired, any change to the database will invalidate the enumerator, and further access to the
		/// enumerator will cause an <see cref="InvalidOperationException"/> to be thrown.
		/// 
		/// If locked access is desired, the database cannot be updated, until the enumerator has been dispose. Make sure to call
		/// the <see cref="ObjectBTreeFileEnumerator{T}.Dispose"/> method when done with the enumerator, to release the database
		/// after use.</param>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IndexBTreeFileEnumerator<T> GetTypedEnumerator<T>(bool Locked)
		{
			return new IndexBTreeFileEnumerator<T>(this, false, this.recordHandler);
		}

		/* TODO:
		 * 
		 * Enumeration
		 * Find
		 * FindFirst
		 * FindLast
		 */

	}
}
