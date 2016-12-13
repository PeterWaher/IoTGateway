using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
		private GenericObjectSerializer genericSerializer;
		private ObjectBTreeFile objectFile;
		private ObjectBTreeFile indexFile;
		private IndexRecords recordHandler;
		private Encoding encoding;
		private string collectionName;

		/// <summary>
		/// This class manages an index file to a <see cref="ObjectBTreeFile"/>.
		/// </summary>
		/// <param name="Id">Internal identifier of the file.</param>
		/// <param name="FileName">File name of index file.</param>
		/// <param name="ObjectFile">Object file storing actual objects.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		internal IndexBTreeFile(int Id, string FileName, ObjectBTreeFile ObjectFile, FilesProvider Provider, 
			params string[] FieldNames)
		{
			this.objectFile = ObjectFile;
			this.collectionName = this.objectFile.CollectionName;
			this.encoding = this.objectFile.Encoding;

			this.recordHandler = new IndexRecords(this.collectionName, this.encoding, this.objectFile.InlineObjectSizeLimit, FieldNames);
			this.genericSerializer = new GenericObjectSerializer(this.objectFile.Provider);

			this.indexFile = new ObjectBTreeFile(Id, FileName, string.Empty, string.Empty, this.objectFile.BlockSize, 
				this.objectFile.BlobBlockSize, Provider, this.encoding, this.objectFile.TimeoutMilliseconds, this.objectFile.Encrypted,
				this.recordHandler);
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
		/// Name of corresponding collection name.
		/// </summary>
		public string CollectionName { get { return this.collectionName; } }

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding { get { return this.encoding; } }

		/// <summary>
		/// Field names included in the index.
		/// </summary>
		public string[] FieldNames { get { return this.recordHandler.FieldNames; } }

		/// <summary>
		/// If the corresponding field name is sorted in ascending order (true) or descending order (false).
		/// </summary>
		public bool[] Ascending { get { return this.recordHandler.Ascending; } }

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Prohibit);
			if (Bin == null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
				if (Leaf == null)
					throw new IOException("Object is already available in index.");

				await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
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
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Prohibit);
			if (Bin == null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			await this.indexFile.Lock();
			try
			{
				await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null);
			}
			finally
			{
				await this.indexFile.Release();
			}

			return true;
		}

		/// <summary>
		/// Updates an object in the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="OldObject">Object that is being changed.</param>
		/// <param name="NewObject">New version of object.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> UpdateObject(Guid ObjectId, object OldObject, object NewObject, IObjectSerializer Serializer)
		{
			byte[] OldBin = this.recordHandler.Serialize(ObjectId, OldObject, Serializer, MissingFieldAction.Prohibit);
			if (OldBin != null && OldBin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			byte[] NewBin = this.recordHandler.Serialize(ObjectId, NewObject, Serializer, MissingFieldAction.Prohibit);
			if (NewBin != null && NewBin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			if (OldBin == null && NewBin == null)
				return false;

			await this.indexFile.Lock();
			try
			{
				if (OldBin != null)
					await this.indexFile.DeleteObjectLocked(OldBin, false, true, Serializer, null);

				if (NewBin != null)
				{
					BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(NewBin);
					await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, NewBin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
				}
			}
			finally
			{
				await this.indexFile.Release();
			}

			return true;
		}

		/// <summary>
		/// Clears the database of all objects.
		/// </summary>
		/// <returns>Task object.</returns>
		internal Task ClearAsync()
		{
			return this.indexFile.ClearAsync();
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<object> GetEnumerator()
		{
			return new IndexBTreeFileEnumerator<object>(this, false, this.recordHandler, null);
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new IndexBTreeFileEnumerator<object>(this, false, this.recordHandler, null);
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
			return new IndexBTreeFileEnumerator<T>(this, false, this.recordHandler, null);
		}

		/// <summary>
		/// Calculates the rank of an object in the database, given its Object ID.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Rank of object in database.</returns>
		/// <exception cref="IOException">If the object is not found.</exception>
		public async Task<ulong> GetRank(Guid ObjectId)
		{
			object Object = await this.objectFile.LoadObject(ObjectId);

			Type ObjectType = Object.GetType();
			IObjectSerializer Serializer = this.objectFile.Provider.GetObjectSerializer(ObjectType);

			byte[] Key = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Prohibit);
			if (Key == null)
				throw new KeyNotFoundException("Object not found.");

			await this.indexFile.Lock();
			try
			{
				return await this.indexFile.GetRankLocked(Key);
			}
			finally
			{
				await this.indexFile.Release();
			}
		}

		/// <summary>
		/// Regenerates the index.
		/// </summary>
		/// <returns></returns>
		public async Task Regenerate()
		{
			await this.ClearAsync();

			using (ObjectBTreeFileEnumerator<object> e = this.objectFile.GetTypedEnumerator<object>(true))
			{
				while (e.MoveNext())
					await this.SaveNewObject((Guid)e.CurrentObjectId, e.Current, e.CurrentSerializer);
			}
		}

		/// <summary>
		/// Searches for the first object that is greater than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterOrEqualTo<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterOrEqualTo<T>(false, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterOrEqualTo<T>(bool Locked, params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterOrEqualTo<T>(Locked, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterOrEqualTo<T>(bool Locked, GenericObject Object)
		{
			byte[] Key = this.recordHandler.Serialize(Guid.Empty, Object, this.genericSerializer, MissingFieldAction.First);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				return new IndexBTreeFileEnumerator<T>(this, Locked, this.recordHandler, Leaf);
			}
			finally
			{
				if (!Locked)
					await this.indexFile.Release();
			}
		}

		/// <summary>
		/// Searches for the first object that is lasser than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindLastLesserOrEqualTo<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserOrEqualTo<T>(false, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindLastLesserOrEqualTo<T>(bool Locked, params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserOrEqualTo<T>(Locked, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileEnumerator<T>> FindLastLesserOrEqualTo<T>(bool Locked, GenericObject Object)
		{
			byte[] Key = this.recordHandler.Serialize(GuidMax, Object, this.genericSerializer, MissingFieldAction.Last);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				return new IndexBTreeFileEnumerator<T>(this, Locked, this.recordHandler, Leaf);
			}
			finally
			{
				if (!Locked)
					await this.indexFile.Release();
			}
		}

		internal static readonly Guid GuidMax = new Guid(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });

		/// <summary>
		/// Searches for the first object that is greater than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterThan<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterThan<T>(false, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterThan<T>(bool Locked, params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterThan<T>(Locked, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileEnumerator<T>> FindFirstGreaterThan<T>(bool Locked, GenericObject Object)
		{
			byte[] Key = this.recordHandler.Serialize(GuidMax, Object, this.genericSerializer, MissingFieldAction.Last);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				return new IndexBTreeFileEnumerator<T>(this, Locked, this.recordHandler, Leaf);
			}
			finally
			{
				if (!Locked)
					await this.indexFile.Release();
			}
		}

		/// <summary>
		/// Searches for the first object that is lasser than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindLastLesserThan<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserThan<T>(false, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Properties">Limit properties to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public Task<IndexBTreeFileEnumerator<T>> FindLastLesserThan<T>(bool Locked, params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserThan<T>(Locked, new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Locked">If the resulting enumerator should be opened in locked mode or not.</param>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileEnumerator<T>> FindLastLesserThan<T>(bool Locked, GenericObject Object)
		{
			byte[] Key = this.recordHandler.Serialize(Guid.Empty, Object, this.genericSerializer, MissingFieldAction.First);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			await this.indexFile.Lock();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				return new IndexBTreeFileEnumerator<T>(this, Locked, this.recordHandler, Leaf);
			}
			finally
			{
				if (!Locked)
					await this.indexFile.Release();
			}
		}

	}
}
