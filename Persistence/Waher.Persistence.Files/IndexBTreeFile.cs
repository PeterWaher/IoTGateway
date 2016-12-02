using System;
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
	public class IndexBTreeFile : IDisposable
	{
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
/*
		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObject(Guid Guid, object Object, ObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(Guid, Object, Serializer);
			if (Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked();
				await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
			}
			finally
			{
				await this.indexFile.Release();
			}
		}
		*/
	}
}
