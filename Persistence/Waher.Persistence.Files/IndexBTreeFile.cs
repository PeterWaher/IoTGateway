using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages an index file to a <see cref="ObjectBTreeFile"/>.
	/// </summary>
	public class IndexBTreeFile : IDisposable, IEnumerable<object>
	{
		private readonly GenericObjectSerializer genericSerializer;
		private ObjectBTreeFile objectFile;
		private ObjectBTreeFile indexFile;
		private IndexRecords recordHandler;
		private readonly Encoding encoding;
		private readonly string collectionName;

		/// <summary>
		/// This class manages an index file to a <see cref="ObjectBTreeFile"/>.
		/// </summary>
		/// <param name="FileName">File name of index file.</param>
		/// <param name="ObjectFile">Object file storing actual objects.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		internal IndexBTreeFile(string FileName, ObjectBTreeFile ObjectFile, FilesProvider Provider,
			params string[] FieldNames)
		{
			this.objectFile = ObjectFile;
			this.collectionName = this.objectFile.CollectionName;
			this.encoding = this.objectFile.Encoding;

			this.recordHandler = new IndexRecords(this.collectionName, this.encoding, this.objectFile.InlineObjectSizeLimit, FieldNames);
			this.genericSerializer = new GenericObjectSerializer(this.objectFile.Provider);

			this.indexFile = new ObjectBTreeFile(FileName, string.Empty, string.Empty, this.objectFile.BlockSize,
				this.objectFile.BlobBlockSize, Provider, this.encoding, this.objectFile.TimeoutMilliseconds,
				this.objectFile.Encrypted, this.recordHandler);
			this.recordHandler.Index = this;
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
		internal async Task<bool> SaveNewObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			await this.indexFile.LockWrite();
			try
			{
				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
				if (Leaf is null)
					throw new IOException("Object is already available in index.");

				await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
			}
			finally
			{
				await this.indexFile.EndWrite();
			}

			return true;
		}

		/// <summary>
		/// Saves a new set of objects to the file.
		/// </summary>
		/// <param name="ObjectIds">Object IDs</param>
		/// <param name="Objects">Objects to persist.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <returns>If the object was saved in the index (true), or if the index property values of the object did not exist, or were too big to fit in an index record.</returns>
		internal async Task<bool> SaveNewObjects(IEnumerable<Guid> ObjectIds, IEnumerable<object> Objects, IObjectSerializer Serializer)
		{
			await this.indexFile.LockWrite();
			try
			{
				IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
				IEnumerator<object> e2 = Objects.GetEnumerator();

				while (e1.MoveNext() && e2.MoveNext())
				{
					byte[] Bin = this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
					if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
						return false;

					BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Bin);
					if (Leaf is null)
						throw new IOException("Object is already available in index.");

					await this.indexFile.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
				}
			}
			finally
			{
				await this.indexFile.EndWrite();
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
		internal async Task<bool> DeleteObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
		{
			byte[] Bin = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			await this.indexFile.LockWrite();
			try
			{
				await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
			finally
			{
				await this.indexFile.EndWrite();
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
		internal async Task DeleteObjects(IEnumerable<Guid> ObjectIds, IEnumerable<object> Objects, IObjectSerializer Serializer)
		{
			await this.indexFile.LockWrite();
			try
			{
				IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
				IEnumerator<object> e2 = Objects.GetEnumerator();

				while (e1.MoveNext() && e2.MoveNext())
				{
					try
					{
						byte[] Bin = this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
						if (Bin is null || Bin.Length > this.indexFile.InlineObjectSizeLimit)
							continue;

						await this.indexFile.DeleteObjectLocked(Bin, false, true, Serializer, null);
					}
					catch (KeyNotFoundException)
					{
						continue;
					}
				}
			}
			finally
			{
				await this.indexFile.EndWrite();
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
		internal async Task<bool> UpdateObject(Guid ObjectId, object OldObject, object NewObject, IObjectSerializer Serializer)
		{
			byte[] OldBin = this.recordHandler.Serialize(ObjectId, OldObject, Serializer, MissingFieldAction.Null);
			if (!(OldBin is null) && OldBin.Length > this.indexFile.InlineObjectSizeLimit)
				return false;

			byte[] NewBin = this.recordHandler.Serialize(ObjectId, NewObject, Serializer, MissingFieldAction.Null);
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

			await this.indexFile.LockWrite();
			try
			{
				if (!(OldBin is null))
				{
					try
					{
						await this.indexFile.DeleteObjectLocked(OldBin, false, true, Serializer, null);
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
			finally
			{
				await this.indexFile.EndWrite();
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
		internal async Task UpdateObjects(IEnumerable<Guid> ObjectIds,
			IEnumerable<object> OldObjects, IEnumerable<object> NewObjects,
			IObjectSerializer Serializer)
		{
			await this.indexFile.LockWrite();
			try
			{
				IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
				IEnumerator<object> e2 = OldObjects.GetEnumerator();
				IEnumerator<object> e3 = NewObjects.GetEnumerator();

				while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
				{
					byte[] OldBin = this.recordHandler.Serialize(e1.Current, e2.Current, Serializer, MissingFieldAction.Null);
					if (!(OldBin is null) && OldBin.Length > this.indexFile.InlineObjectSizeLimit)
						continue;

					byte[] NewBin = this.recordHandler.Serialize(e1.Current, e3.Current, Serializer, MissingFieldAction.Null);
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
							await this.indexFile.DeleteObjectLocked(OldBin, false, true, Serializer, null);
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
			finally
			{
				await this.indexFile.EndWrite();
			}
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
			return new IndexBTreeFileEnumerator<object>(this, this.recordHandler);
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection in the order specified by the index.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new IndexBTreeFileEnumerator<object>(this, this.recordHandler);
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
		/// the <see cref="IndexBTreeFileEnumerator{T}.Dispose"/> method when done with the enumerator, to release the database
		/// after use.</param>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public async Task<IndexBTreeFileEnumerator<T>> GetTypedEnumerator<T>(bool Locked)
		{
			IndexBTreeFileEnumerator<T> e = new IndexBTreeFileEnumerator<T>(this, this.recordHandler);
			if (Locked)
				await e.LockRead();

			return e;
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

			byte[] Key = this.recordHandler.Serialize(ObjectId, Object, Serializer, MissingFieldAction.Null);
			if (Key is null)
				throw new KeyNotFoundException("Object not found.");

			await this.indexFile.LockRead();
			try
			{
				return await this.indexFile.GetRankLocked(Key);
			}
			finally
			{
				await this.indexFile.EndRead();
			}
		}

		/// <summary>
		/// Regenerates the index.
		/// </summary>
		/// <returns></returns>
		public async Task Regenerate()
		{
			LinkedList<object> Objects = new LinkedList<object>();
			LinkedList<Guid> ObjectIds = new LinkedList<Guid>();
			IObjectSerializer LastSerializer = null;
			int Count = 0;
			int c = 0;
			int d = 0;

			await this.ClearAsync();

			using (ObjectBTreeFileEnumerator<object> e = await this.objectFile.GetTypedEnumeratorAsync<object>(true))
			{
				while (await e.MoveNextAsync())
				{
					object Obj = e.Current;
					IObjectSerializer Serializer = e.CurrentSerializer;

					if (!(Obj is null))
					{
						if (LastSerializer is null || Serializer != LastSerializer)
						{
							if (Count > 0)
							{
								await this.SaveNewObjects(ObjectIds, Objects, LastSerializer);
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
							await this.SaveNewObjects(ObjectIds, Objects, LastSerializer);
							ObjectIds.Clear();
							Objects.Clear();
							Count = 0;
						}
					}
					else
						d++;
				}

				if (Count > 0)
					await this.SaveNewObjects(ObjectIds, Objects, LastSerializer);
			}

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

			IndexBTreeFileEnumerator<T> Result = null;

			try
			{
				Result = new IndexBTreeFileEnumerator<T>(this, this.recordHandler);
				if (Locked)
					await Result.LockRead();

				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				Result.SetStartingPoint(Leaf);
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				Result = null;

				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

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

			IndexBTreeFileEnumerator<T> Result = null;

			try
			{
				Result = new IndexBTreeFileEnumerator<T>(this, this.recordHandler);
				if (Locked)
					await Result.LockRead();

				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				Result.SetStartingPoint(Leaf);
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				Result = null;

				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

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

			IndexBTreeFileEnumerator<T> Result = null;

			try
			{
				Result = new IndexBTreeFileEnumerator<T>(this, this.recordHandler);
				if (Locked)
					await Result.LockRead();

				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				Result.SetStartingPoint(Leaf);
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				Result = null;

				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

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

			IndexBTreeFileEnumerator<T> Result = null;

			try
			{
				Result = new IndexBTreeFileEnumerator<T>(this, this.recordHandler);
				if (Locked)
					await Result.LockRead();

				BlockInfo Leaf = await this.indexFile.FindLeafNodeLocked(Key);
				Result.SetStartingPoint(Leaf);
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				Result = null;

				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Result;
		}

		/// <summary>
		/// Goes through the entire file and computes statistics abouts its composition.
		/// </summary>
		/// <param name="ExistingIds">Object ID available in master file.</param>
		/// <returns>File statistics.</returns>
		public virtual async Task<FileStatistics> ComputeStatistics(Dictionary<Guid, bool> ExistingIds)
		{
			FileStatistics Result;

			await this.indexFile.LockWrite();
			try
			{
				Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
				Result = await this.indexFile.ComputeStatisticsLocked(ObjectIds, ExistingIds);
			}
			finally
			{
				await this.indexFile.EndWrite();
			}

			return Result;
		}

	}
}
