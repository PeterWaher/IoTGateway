using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages an index file to a <see cref="ObjectBTreeFile"/>.
	/// </summary>
	public class IndexBTreeFile : IDisposable
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
		/// <param name="FileName">File name of index file.</param>
		/// <param name="ObjectFile">Object file storing actual objects.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		internal static async Task<IndexBTreeFile> Create(string FileName, ObjectBTreeFile ObjectFile, FilesProvider Provider,
			params string[] FieldNames)
		{
			IndexBTreeFile Result = new IndexBTreeFile()
			{
				objectFile = ObjectFile,
				collectionName = ObjectFile.CollectionName,
				encoding = ObjectFile.Encoding
			};

			Result.recordHandler = new IndexRecords(Result.collectionName, Result.encoding, Result.objectFile.InlineObjectSizeLimit, FieldNames);
			Result.genericSerializer = new GenericObjectSerializer(Result.objectFile.Provider);

			Result.indexFile = await ObjectBTreeFile.Create(FileName, Result.collectionName, string.Empty, Result.objectFile.BlockSize,
				Result.objectFile.BlobBlockSize, Provider, Result.encoding, Result.objectFile.TimeoutMilliseconds,
				Result.objectFile.Encrypted, Result.recordHandler, ObjectFile.FileAccess);
			Result.recordHandler.Index = Result;

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.indexFile?.Dispose();
			this.indexFile = null;

			this.objectFile = null;
			this.recordHandler = null;
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
		/// Access to underlying Index file. Should only be accessed when the main file is properly locked.
		/// </summary>
		public ObjectBTreeFile IndexFileLocked => this.indexFile;

		internal FilesProvider Provider => this.objectFile.Provider;
		internal uint BlockLimit => this.indexFile?.BlockLimit ?? uint.MaxValue;
		internal int Id => this.indexFile.Id;
		internal int BlockSize => this.indexFile.BlockSize;
		internal int BlobBlockSize => this.indexFile.BlobBlockSize;
		internal int InlineObjectSizeLimit => this.indexFile.InlineObjectSizeLimit;
		internal int TimeoutMilliseconds => this.indexFile.TimeoutMilliseconds;
		internal string FileName => this.indexFile.FileName;
		internal string BlobFileName => this.indexFile.BlobFileName;
		internal bool Encrypted => this.indexFile.Encrypted;
		internal bool IsReadOnly => this.indexFile.IsReadOnly;

		internal Task<ObjectBTreeFileCursor<object>> GetCursor(IndexRecords RecordHandler)
		{
			return ObjectBTreeFileCursor<object>.CreateLocked(this.indexFile, RecordHandler);
		}

		internal Task<object> TryLoadObjectLocked(Guid ObjectId, IObjectSerializer Serializer)
		{
			return this.objectFile.TryLoadObjectLocked(ObjectId, Serializer);
		}

		internal Task ExportGraphXMLLocked(XmlWriter Output, bool Properties)
		{
			return this.indexFile.ExportGraphXMLLocked(Output, Properties);
		}

		internal Task EndWritePriv()
		{
			return this.indexFile.EndWritePriv();
		}

		/// <summary>
		/// Number of objects in file.
		/// </summary>
		public Task<ulong> CountAsync => this.GetCountAsync();

		/// <summary>
		/// Number of objects in file.
		/// </summary>
		internal Task<ulong> CountAsyncLocked => this.indexFile.GetObjectCountLocked(0, true);

		private async Task<ulong> GetCountAsync()
		{
			await this.objectFile.BeginRead();
			try
			{
				return await this.indexFile.GetObjectCountLocked(0, true);
			}
			finally
			{
				await this.objectFile.EndRead();
			}
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
			return this.recordHandler.SameSortOrder(ConstantFields, SortOrder);
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
			return this.recordHandler.ReverseSortOrder(ConstantFields, SortOrder);
		}

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObjectLocked(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = await this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
			if (Leaf is null)
				throw new FileException("Object is already available in index.", this.indexFile.FileName, this.collectionName);

			await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);

			return true;
		}

		/// <summary>
		/// Saves a new set of objects to the file.
		/// </summary>
		/// <param name="ObjectIds">Object IDs</param>
		/// <param name="Objects">Objects to persist.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObjectsLocked(IEnumerable<Guid> ObjectIds, IEnumerable<object> Objects, IObjectSerializer Serializer)
		{
			IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
			IEnumerator<object> e2 = Objects.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext())
			{
				byte[] Bin = await this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
				if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
					return false;

				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
				if (Leaf is null)
					throw new FileException("Object is already available in index.", this.indexFile.FileName, this.collectionName);

				await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
			}

			return true;
		}

		/// <summary>
		/// Deletes an object from the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object to delete.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was deleted from the index (true), or if the object did not exist in the index.</returns>
		internal async Task<bool> DeleteObjectLocked(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = await this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			try
			{
				await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null, 0);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Deletes a set of objects from the file.
		/// </summary>
		/// <param name="ObjectIds">Object IDs</param>
		/// <param name="Objects">Objects to delete.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was deleted from the index (true), or if the object did not exist in the index.</returns>
		internal async Task DeleteObjectsLocked(IEnumerable<Guid> ObjectIds, IEnumerable<object> Objects, IObjectSerializer Serializer)
		{
			IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
			IEnumerator<object> e2 = Objects.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext())
			{
				try
				{
					byte[] Bin = await this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
					if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
						continue;

					await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null, 0);
				}
				catch (KeyNotFoundException)
				{
					continue;
				}
			}
		}

		/// <summary>
		/// Updates an object in the file.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="OldObject">Object that is being changed.</param>
		/// <param name="NewObject">New version of object.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> UpdateObjectLocked(Guid ObjectId, object OldObject, object NewObject, IObjectSerializer Serializer)
		{
			byte[] OldBin = await this.recordHandler.Serialize(ObjectId, OldObject, Serializer, MissingFieldAction.Null);
			if (!(OldBin is null) && OldBin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			byte[] NewBin = await this.recordHandler.Serialize(ObjectId, NewObject, Serializer, MissingFieldAction.Null);
			if (!(NewBin is null) && NewBin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			if (OldBin is null && NewBin is null)
				return false;

			int i, c;

			if ((c = OldBin.Length) == NewBin.Length)
			{
				for (i = 0; i < c; i++)
				{
					if (OldBin[i] != NewBin[i])
						break;
				}

				if (i == c)
					return true;
			}

			if (!(OldBin is null))
			{
				try
				{
					await this.indexFile.DeleteObjectLocked(OldBin, false, true, Serializer, null, 0);
				}
				catch (KeyNotFoundException)
				{
					// Ignore.
				}
			}

			if (!(NewBin is null))
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(NewBin);
				if (Leaf is null)
					throw Database.FlagForRepair(this.collectionName, "Object seems to exist twice in index.");
				else
					await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, NewBin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
			}

			return true;
		}

		/// <summary>
		/// Updates a series of objects in the file.
		/// </summary>
		/// <param name="ObjectIds">Object IDs</param>
		/// <param name="OldObjects">Objects that are being changed.</param>
		/// <param name="NewObjects">New versions of objects.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task UpdateObjectsLocked(IEnumerable<Guid> ObjectIds, IEnumerable<object> OldObjects, IEnumerable<object> NewObjects,
			IObjectSerializer Serializer)
		{
			IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
			IEnumerator<object> e2 = OldObjects.GetEnumerator();
			IEnumerator<object> e3 = NewObjects.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
			{
				byte[] OldBin = await this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
				if (!(OldBin is null) && OldBin.Length > this.indexFile.InlineObjectSizeLimit)
					continue;

				byte[] NewBin = await this.recordHandler.Serialize(e1.Current, e3.Current, Serializer, MissingFieldAction.Null);
				if (!(NewBin is null) && NewBin.Length > this.indexFile.InlineObjectSizeLimit)
					continue;

				if (OldBin is null && NewBin is null)
					continue;

				int i, c;

				if ((c = OldBin.Length) == NewBin.Length)
				{
					for (i = 0; i < c; i++)
					{
						if (OldBin[i] != NewBin[i])
							break;
					}

					if (i == c)
						continue;
				}

				if (!(OldBin is null))
				{
					try
					{
						await this.indexFile.DeleteObjectLocked(OldBin, false, true, Serializer, null, 0);
					}
					catch (KeyNotFoundException)
					{
						// Ignore.
					}
				}

				if (!(NewBin is null))
				{
					BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(NewBin);
					await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, NewBin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
				}
			}
		}

		/// <summary>
		/// Clears the database of all objects.
		/// </summary>
		/// <returns>Task object.</returns>
		internal Task ClearAsyncLocked()
		{
			return this.indexFile.ClearAsyncLocked();
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumeratorLocked{T}()"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public async Task<IndexBTreeFileCursor<object>> GetCursorAsyncLocked()
		{
			return await IndexBTreeFileCursor<object>.CreateLocked(this, this.recordHandler);
		}

		/// <summary>
		/// Returns an typed enumerator that iterates through the collection in the order specified by the index. The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public Task<IndexBTreeFileCursor<T>> GetTypedEnumeratorLocked<T>()
		{
			return IndexBTreeFileCursor<T>.CreateLocked(this, this.recordHandler);
		}

		/// <summary>
		/// Calculates the rank of an object in the database, given its Object ID.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Rank of object in database.</returns>
		/// <exception cref="KeyNotFoundException">If the object is not found.</exception>
		public async Task<ulong> GetRankLocked(Guid ObjectId)
		{
			object Object = await this.objectFile.LoadObject(ObjectId);

			Type ObjectType = Object.GetType();
			IObjectSerializer Serializer = await this.objectFile.Provider.GetObjectSerializer(ObjectType);

			byte[] Key = await this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Key is null)
				throw new KeyNotFoundException("Object not found.");

			return await this.indexFile.GetRankLocked(Key);
		}

		/// <summary>
		/// Regenerates the index.
		/// </summary>
		/// <returns></returns>
		internal async Task RegenerateLocked()
		{
			LinkedList<object> Objects = new LinkedList<object>();
			LinkedList<Guid> ObjectIds = new LinkedList<Guid>();
			IObjectSerializer LastSerializer = null;
			int Count = 0;
			int c = 0;
			int d = 0;

			await this.ClearAsyncLocked();

			ObjectBTreeFileCursor<object> e = await this.objectFile.GetTypedEnumeratorAsyncLocked<object>();

			while (await e.MoveNextAsyncLocked())
			{
				object Obj = e.Current;
				IObjectSerializer Serializer = e.CurrentSerializer;

				if (!(Obj is null))
				{
					if (LastSerializer is null || Serializer != LastSerializer)
					{
						if (Count > 0)
						{
							await this.SaveNewObjectsLocked(ObjectIds, Objects, LastSerializer);
							ObjectIds.Clear();
							Objects.Clear();
							Count = 0;
						}

						LastSerializer = Serializer;
					}

					ObjectIds.AddLast((Guid)e.CurrentObjectId);
					Objects.AddLast(Obj);
					c++;
					Count++;

					if (Count >= 1000)
					{
						await this.SaveNewObjectsLocked(ObjectIds, Objects, LastSerializer);
						ObjectIds.Clear();
						Objects.Clear();
						Count = 0;
					}
				}
				else
					d++;
			}

			if (Count > 0)
				await this.SaveNewObjectsLocked(ObjectIds, Objects, LastSerializer);

			Log.Notice("Index regenerated.", this.indexFile.FileName,
				new KeyValuePair<string, object>("NrObjects", c),
				new KeyValuePair<string, object>("NrNotLoadable", d));
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
		public Task<IndexBTreeFileCursor<T>> FindFirstGreaterOrEqualToLocked<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterOrEqualToLocked<T>(new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileCursor<T>> FindFirstGreaterOrEqualToLocked<T>(GenericObject Object)
		{
			byte[] Key = await this.recordHandler.Serialize(Guid.Empty, Object, this.genericSerializer, MissingFieldAction.First);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			IndexBTreeFileCursor<T> Result = await IndexBTreeFileCursor<T>.CreateLocked(this, this.recordHandler);

			BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
			Result.SetStartingPoint(Leaf);

			return Result;
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
		public Task<IndexBTreeFileCursor<T>> FindLastLesserOrEqualToLocked<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserOrEqualToLocked<T>(new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than or equal to a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than or equal to the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileCursor<T>> FindLastLesserOrEqualToLocked<T>(GenericObject Object)
		{
			byte[] Key = await this.recordHandler.Serialize(GuidMax, Object, this.genericSerializer, MissingFieldAction.Last);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			IndexBTreeFileCursor<T> Result = await IndexBTreeFileCursor<T>.CreateLocked(this, this.recordHandler);

			BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
			Result.SetStartingPoint(Leaf);

			return Result;
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
		public Task<IndexBTreeFileCursor<T>> FindFirstGreaterThanLocked<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindFirstGreaterThanLocked<T>(new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is greater than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is greater than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileCursor<T>> FindFirstGreaterThanLocked<T>(GenericObject Object)
		{
			byte[] Key = await this.recordHandler.Serialize(GuidMax, Object, this.genericSerializer, MissingFieldAction.Last);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			IndexBTreeFileCursor<T> Result = await IndexBTreeFileCursor<T>.CreateLocked(this, this.recordHandler);

			BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
			Result.SetStartingPoint(Leaf);

			return Result;
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
		public Task<IndexBTreeFileCursor<T>> FindLastLesserThanLocked<T>(params KeyValuePair<string, object>[] Properties)
		{
			return this.FindLastLesserThanLocked<T>(new GenericObject(this.collectionName, string.Empty, Guid.Empty, Properties));
		}

		/// <summary>
		/// Searches for the first object that is lasser than a hypothetical limit object.
		/// </summary>
		/// <typeparam name="T">The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.</typeparam>
		/// <param name="Object">Limit object to search for.</param>
		/// <returns>Enumerator that can be used to enumerate objects in index order. First object will be the first
		/// object that is lesser than the limit object. If null is returned, the search operation could
		/// not be performed.</returns>
		public async Task<IndexBTreeFileCursor<T>> FindLastLesserThanLocked<T>(GenericObject Object)
		{
			byte[] Key = await this.recordHandler.Serialize(Guid.Empty, Object, this.genericSerializer, MissingFieldAction.First);
			if (Key.Length > this.indexFile.InlineObjectSizeLimit)
				return null;

			IndexBTreeFileCursor<T> Result = await IndexBTreeFileCursor<T>.CreateLocked(this, this.recordHandler);

			BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
			Result.SetStartingPoint(Leaf);

			return Result;
		}

		/// <summary>
		/// Goes through the entire file and computes statistics abouts its composition.
		/// </summary>
		/// <param name="ExistingIds">Object ID available in master file.</param>
		/// <returns>File statistics.</returns>
		public virtual Task<FileStatistics> ComputeStatisticsLocked(Dictionary<Guid, bool> ExistingIds)
		{
			Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
			return this.indexFile.ComputeStatisticsLocked(ObjectIds, ExistingIds);
		}

	}
}
