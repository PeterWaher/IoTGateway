using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Storage;
using Waher.Runtime.Inventory;
using Waher.Events;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Type of lock requested.
	/// </summary>
	public enum LockType
	{
		/// <summary>
		/// No lock
		/// </summary>
		None,

		/// <summary>
		/// Lock for reading
		/// </summary>
		Read,

		/// <summary>
		/// Lock for writing
		/// </summary>
		Write
	}

	/// <summary>
	/// Enumerates object in a <see cref="ObjectBTreeFile"/> in GUID order. You can use the enumerator to enumerate objects
	/// forwards and backwards, as well as skip a given number of objects.
	/// </summary>
	public class ObjectBTreeFileEnumerator<T> : IEnumerator<T>, ICursor<T>, IAsyncEnumerator
	{
		private ObjectBTreeFile file;
		private BlockHeader currentHeader;
		private BinaryDeserializer currentReader;
		private IObjectSerializer defaultSerializer;
		private IObjectSerializer currentSerializer;
		private BlockInfo startingPoint;
		private IRecordHandler recordHandler;
		private object currentObjectId;
		private string objectIdMemberName;
		private int fieldCount;
		private T current;
		private ulong? currentRank;
		private byte[] currentBlock;
		private ulong blockUpdateCounter;
		private uint currentBlockIndex;
		private int currentObjPos;
		private int timeoutMilliseconds;
		private LockType lockType;
		private bool hasCurrent;
		private bool currentTypeCompatible;

		internal static Task<ObjectBTreeFileEnumerator<T>> Create(ObjectBTreeFile File, IRecordHandler RecordHandler)
		{
			return Create(File, RecordHandler, null);
		}

		internal static async Task<ObjectBTreeFileEnumerator<T>> Create(ObjectBTreeFile File, IRecordHandler RecordHandler, IObjectSerializer DefaultSerializer)
		{
			ObjectBTreeFileEnumerator<T> Result = new ObjectBTreeFileEnumerator<T>()
			{
				file = File,
				currentBlockIndex = 0,
				currentBlock = null,
				currentReader = null,
				currentHeader = null,
				blockUpdateCounter = File.BlockUpdateCounter,
				lockType = LockType.None,
				recordHandler = RecordHandler,
				startingPoint = null,
				defaultSerializer = DefaultSerializer,
				timeoutMilliseconds = File.TimeoutMilliseconds,
				objectIdMemberName = null,
				fieldCount = 0
			};

			Result.Reset();

			if (Result.defaultSerializer is null && typeof(T) != typeof(object))
				Result.defaultSerializer = await File.Provider.GetObjectSerializer(typeof(T));

			if (Result.defaultSerializer is ObjectSerializer Serializer &&
				Serializer.HasObjectIdField)
			{
				Result.objectIdMemberName = Serializer.ObjectIdMemberName;
				Result.fieldCount = 1;
			}

			return Result;
		}

		internal void SetStartingPoint(BlockInfo StartingPoint)
		{
			this.startingPoint = StartingPoint;
		}

		/// <summary>
		/// Locks the underlying file (if not locked).
		/// </summary>
		internal async Task Lock(LockType LockType)
		{
			if (this.lockType == LockType.None)
			{
				switch (LockType)
				{
					case LockType.Read:
						await this.file.LockRead();
						break;

					case LockType.Write:
						await this.file.LockWrite();
						break;
				}

				this.lockType = LockType;
				this.blockUpdateCounter = this.file.BlockUpdateCounter;
			}
			else if (this.lockType != LockType)
				throw new InvalidOperationException("Already locked.");
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public async void Dispose()
		{
			try
			{
				await this.DisposeAsync();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public Task DisposeAsync()
		{
			LockType Temp = this.lockType;
			this.lockType = LockType.None;

			switch (Temp)
			{
				case LockType.Read:
					return this.file.EndRead();

				case LockType.Write:
					return this.file.EndWrite();

				default:
					return Task.CompletedTask;
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
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				if (this.hasCurrent)
					return this.currentSerializer;
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
		public object CurrentObjectId
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
		/// Gets the Object ID of the current object.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
		Guid ICursor<T>.CurrentObjectId
		{
			get
			{
				if (this.hasCurrent)
					return (Guid)this.currentObjectId;
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
				Task<ulong> Task = this.GetCurrentRank();
				FilesProvider.Wait(Task, this.timeoutMilliseconds);
				return Task.Result;
			}
		}

		/// <summary>
		/// Gets the rank of the current object.
		/// </summary>
		public async Task<ulong> GetCurrentRank()
		{
			if (this.hasCurrent)
			{
				if (!this.currentRank.HasValue)
				{
					if (this.lockType != LockType.None)
						this.currentRank = await this.file.GetRankLocked(this.currentObjectId);
					else if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
						throw new InvalidOperationException("Contents of file has been changed.");
					else
					{
						await this.file.LockRead();
						try
						{
							this.currentRank = await this.file.GetRankLocked(this.currentObjectId);
						}
						finally
						{
							await this.file.EndRead();
						}
					}
				}

				return this.currentRank.Value;
			}
			else
				throw new InvalidOperationException("Enumeration not started. Call MoveNext() first.");
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
			if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");

			if (!this.hasCurrent)
			{
				if (!(this.startingPoint is null))
				{
					this.GoToStartingPoint(this.startingPoint);
					this.hasCurrent = true;
					return await this.MoveNextAsync();
				}
				else
					return await this.GoToFirst();
			}

			if (this.currentRank.HasValue)
				this.currentRank++;

			object ObjectId;
			uint BlockLink;
			int Pos;

			if (this.currentReader.BytesLeft >= 4)
			{
				BlockLink = this.currentReader.ReadBlockLink();
				if (BlockLink != 0)
					return await this.GoToFirst(BlockLink);

				Pos = this.currentReader.Position;
				ObjectId = this.recordHandler.GetKey(this.currentReader);
				if (!(ObjectId is null))
				{
					await this.LoadObject(ObjectId, Pos);
					return true;
				}
			}
			else
				ObjectId = null;

			BlockLink = this.currentHeader.LastBlockIndex;
			if (BlockLink != 0)
				return await this.GoToFirst(BlockLink);

			do
			{
				BlockLink = this.currentHeader.ParentBlockIndex;
				if (BlockLink == 0 && this.currentBlockIndex == 0)
				{
					this.Reset();
					return false;
				}

				this.currentBlock = await this.LoadBlock(BlockLink);
				this.currentReader.Restart(this.currentBlock, 0);

				BlockHeader ParentHeader = new BlockHeader(this.currentReader);

				if (ParentHeader.LastBlockIndex == this.currentBlockIndex)
				{
					this.currentBlockIndex = BlockLink;
					this.currentHeader = ParentHeader;
				}
				else
				{
					uint BlockLink2;
					int Len = 0;
					bool IsEmpty;

					do
					{
						this.currentReader.Position += Len;

						BlockLink2 = this.currentReader.ReadBlockLink();
						Pos = this.currentReader.Position;

						ObjectId = this.recordHandler.GetKey(this.currentReader);
						if (IsEmpty = (ObjectId is null))
							break;

						Len = await this.recordHandler.GetPayloadSize(this.currentReader);
					}
					while (BlockLink2 != this.currentBlockIndex && this.currentReader.BytesLeft >= 4);

					if (IsEmpty || BlockLink2 != this.currentBlockIndex)
					{
						this.Reset();
						return false;
					}

					this.currentBlockIndex = BlockLink;
					this.currentHeader = ParentHeader;

					await this.LoadObject(ObjectId, Pos);
					return true;
				}
			}
			while (ObjectId is null);

			this.Reset();
			return false;
		}

		private void GoToStartingPoint(BlockInfo StartingPoint)
		{
			this.currentBlockIndex = StartingPoint.BlockIndex;
			this.currentBlock = StartingPoint.Block;
			this.currentHeader = StartingPoint.Header;
			this.currentObjPos = StartingPoint.InternalPosition;

			if (this.currentReader is null)
				this.currentReader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock, this.file.BlockLimit, StartingPoint.InternalPosition);
			else
				this.currentReader.Restart(this.currentBlock, StartingPoint.InternalPosition);
		}

		/// <summary>
		/// Goes to the first object.
		/// </summary>
		/// <returns>If a first object was found.</returns>
		public Task<bool> GoToFirst()
		{
			this.currentRank = 0;
			return this.GoToFirst(0);
		}

		private async Task<bool> GoToFirst(uint StartBlock)
		{
			object ObjectId;
			uint BlockLink;
			int Pos;
			bool IsEmpty;

			BlockLink = StartBlock;

			do
			{
				this.currentBlockIndex = BlockLink;
				this.currentBlock = await this.LoadBlock(this.currentBlockIndex);

				if (this.currentReader is null)
					this.currentReader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock, this.file.BlockLimit);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				BlockLink = this.currentReader.ReadBlockLink();
				Pos = this.currentReader.Position;
				ObjectId = this.recordHandler.GetKey(this.currentReader);
				IsEmpty = (ObjectId is null);

				if (IsEmpty)
					BlockLink = this.currentHeader.LastBlockIndex;
			}
			while (BlockLink != 0);

			if (IsEmpty)
			{
				while (this.currentBlockIndex != 0 && IsEmpty)
				{
					uint ChildLink = this.currentBlockIndex;
					int Len;

					this.currentBlockIndex = this.currentHeader.ParentBlockIndex;
					this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
					this.currentReader.Restart(this.currentBlock, 0);
					this.currentHeader = new BlockHeader(this.currentReader);

					if (this.currentHeader.LastBlockIndex != ChildLink)
					{
						do
						{
							BlockLink = this.currentReader.ReadBlockLink();
							Pos = this.currentReader.Position;

							ObjectId = this.recordHandler.GetKey(this.currentReader);
							IsEmpty = (ObjectId is null);
							if (IsEmpty)
								break;

							if (BlockLink == ChildLink)
								break;

							Len = await this.recordHandler.GetPayloadSize(this.currentReader);
							this.currentReader.Position += Len;
						}
						while (this.currentReader.BytesLeft >= 4);
					}
				}

				if (IsEmpty)
				{
					this.Reset();
					return false;
				}
			}

			await this.LoadObject(ObjectId, Pos);
			return true;
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
			if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");

			if (!this.hasCurrent)
			{
				if (!(this.startingPoint is null))
				{
					this.GoToStartingPoint(this.startingPoint);
					this.hasCurrent = true;
					return await this.MovePreviousAsync();
				}
				else
					return await this.GoToLast();
			}

			if (this.currentRank.HasValue)
				this.currentRank--;

			object ObjectId;
			object LastObjectId;
			uint BlockLink;
			uint ParentBlockLink;
			int LastPos;
			int Pos;
			int Len;
			bool IsEmpty;

			this.currentReader.Position = ObjectBTreeFile.BlockHeaderSize;

			if (this.currentObjPos > ObjectBTreeFile.BlockHeaderSize)
			{
				do
				{
					this.currentReader.SkipBlockLink();
					Pos = this.currentReader.Position;

					ObjectId = this.recordHandler.GetKey(this.currentReader);

					Len = await this.recordHandler.GetPayloadSize(this.currentReader);
					this.currentReader.Position += Len;
				}
				while (this.currentReader.Position < this.currentObjPos);

				BlockLink = this.currentReader.ReadBlockLink();

				if (BlockLink == 0)
				{
					await this.LoadObject(ObjectId, Pos);
					return true;
				}
				else
					return await this.GoToLast(BlockLink);
			}
			else
			{
				BlockLink = this.currentReader.ReadBlockLink();
				if (BlockLink != 0)
					return await this.GoToLast(BlockLink);
			}

			while (this.currentBlockIndex != 0)
			{
				ParentBlockLink = this.currentHeader.ParentBlockIndex;
				this.currentBlock = await this.LoadBlock(ParentBlockLink);
				this.currentReader.Restart(this.currentBlock, 0);
				this.currentHeader = new BlockHeader(this.currentReader);

				if (this.currentHeader.BytesUsed == 0)
					this.currentBlockIndex = ParentBlockLink;
				else if (this.currentHeader.LastBlockIndex == this.currentBlockIndex)
				{
					LastPos = this.currentReader.Position;
					LastObjectId = null;
					do
					{
						this.currentReader.SkipBlockLink();
						Pos = this.currentReader.Position;

						ObjectId = this.recordHandler.GetKey(this.currentReader);
						IsEmpty = (ObjectId is null);
						if (IsEmpty)
							break;

						LastPos = Pos;
						LastObjectId = ObjectId;
						Len = await this.recordHandler.GetPayloadSize(this.currentReader);
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 4);

					this.currentBlockIndex = ParentBlockLink;
					await this.LoadObject(LastObjectId, LastPos);
					return true;
				}
				else
				{
					LastPos = 0;
					LastObjectId = null;
					do
					{
						BlockLink = this.currentReader.ReadBlockLink();
						Pos = this.currentReader.Position;

						ObjectId = this.recordHandler.GetKey(this.currentReader);
						IsEmpty = (ObjectId is null);
						if (IsEmpty)
							break;

						if (BlockLink == this.currentBlockIndex)
							break;

						LastPos = Pos;
						LastObjectId = ObjectId;
						Len = await this.recordHandler.GetPayloadSize(this.currentReader);
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 4);

					if (IsEmpty || BlockLink != this.currentBlockIndex)
					{
						this.Reset();
						return false;
					}

					this.currentBlockIndex = ParentBlockLink;

					if (LastPos != 0)
					{
						await this.LoadObject(LastObjectId, LastPos);
						return true;
					}
				}
			}

			this.Reset();
			return false;
		}

		/// <summary>
		/// Goes to the last object.
		/// </summary>
		/// <returns>If a last object was found.</returns>
		public Task<bool> GoToLast()
		{
			this.currentRank = null;
			return this.GoToLast(0);
		}

		private async Task<bool> GoToLast(uint StartBlock)
		{
			object ObjectId;
			uint BlockLink;
			int Len;
			int Pos;
			int LastPos;
			bool IsEmpty;

			BlockLink = StartBlock;

			do
			{
				this.currentBlockIndex = BlockLink;
				this.currentBlock = await this.LoadBlock(this.currentBlockIndex);

				if (this.currentReader is null)
					this.currentReader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock, this.file.BlockLimit);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				BlockLink = this.currentHeader.LastBlockIndex;
			}
			while (BlockLink != 0);

			LastPos = this.currentReader.Position;
			do
			{
				Pos = this.currentReader.Position;

				this.currentReader.SkipBlockLink();
				ObjectId = this.recordHandler.GetKey(this.currentReader);
				IsEmpty = (ObjectId is null);
				if (IsEmpty)
					break;

				LastPos = Pos;
				Len = await this.recordHandler.GetPayloadSize(this.currentReader);
				this.currentReader.Position += Len;
			}
			while (this.currentReader.BytesLeft >= 4);

			this.currentReader.Position = LastPos;
			this.currentReader.SkipBlockLink();
			ObjectId = this.recordHandler.GetKey(this.currentReader);
			IsEmpty = (ObjectId is null);

			if (IsEmpty)
			{
				while (IsEmpty && this.currentBlockIndex != 0)
				{
					this.currentBlockIndex = this.currentHeader.ParentBlockIndex;
					this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
					this.currentReader.Restart(this.currentBlock, 0);
					this.currentHeader = new BlockHeader(this.currentReader);

					LastPos = this.currentReader.Position;
					do
					{
						Pos = this.currentReader.Position;

						this.currentReader.SkipBlockLink();
						ObjectId = this.recordHandler.GetKey(this.currentReader);
						IsEmpty = (ObjectId is null);
						if (IsEmpty)
							break;

						LastPos = Pos;
						Len = await this.recordHandler.GetPayloadSize(this.currentReader);
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 4);

					this.currentReader.Position = LastPos;
					this.currentReader.SkipBlockLink();
					ObjectId = this.recordHandler.GetKey(this.currentReader);
					IsEmpty = (ObjectId is null);
				}

				if (IsEmpty)
				{
					this.Reset();
					return false;
				}
			}

			await this.LoadObject(ObjectId, LastPos + 4);
			return true;
		}

		private async Task LoadObject(object ObjectId, int Start)
		{
			this.currentSerializer = this.defaultSerializer;

			this.currentReader.Position = Start;
			this.currentObjPos = Start - 4;
			this.recordHandler.SkipKey(this.currentReader);

			BinaryDeserializer Reader = this.currentReader;
			KeyValuePair<int, bool> P = await this.recordHandler.GetPayloadSizeEx(Reader);
			int Len = P.Key;
			bool IsBlob = P.Value;
			int PosBak = this.currentReader.Position;

			if (Len == 0)
				this.current = default;
			else
			{
				try
				{
					if (IsBlob)
					{
						this.currentReader.SkipUInt32();

						if (this.lockType != LockType.None)
							Reader = await this.file.LoadBlobLocked(this.currentBlock, Start, null, null);
						else if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
							throw new InvalidOperationException("Contents of file has been changed.");
						else
						{
							await this.file.LockRead();
							try
							{
								Reader = await this.file.LoadBlobLocked(this.currentBlock, Start, null, null);
							}
							finally
							{
								await this.file.EndRead();
							}
						}

						Start = 0;
					}

					if (this.currentSerializer is null)
					{
						ulong TypeCode = Reader.ReadVariableLengthUInt64();
						string TypeName;

						if (TypeCode == 0)
							TypeName = string.Empty;
						else
							TypeName = await this.file.Provider.GetFieldName(this.currentReader.CollectionName, TypeCode);

						if (string.IsNullOrEmpty(TypeName))
							this.currentSerializer = this.file.GenericObjectSerializer;
						else
						{
							Type T = Types.GetType(TypeName);
							if (!(T is null))
								this.currentSerializer = await this.file.Provider.GetObjectSerializer(T);
							else
								this.currentSerializer = this.file.GenericObjectSerializer;
						}
					}

					Reader.Position = Start;
					if (await this.currentSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false) is T Item)
					{
						this.current = Item;
						this.currentTypeCompatible = true;
					}
					else
					{
						this.current = default;
						this.currentTypeCompatible = false;
						this.currentReader.Position = PosBak + Len;
					}
				}
				catch (Exception)
				{
					this.current = default;
					this.currentTypeCompatible = false;
					this.currentReader.Position = PosBak + Len;
				}
			}

			this.currentObjectId = ObjectId;
			this.hasCurrent = true;
		}

		private async Task<byte[]> LoadBlock(uint BlockIndex)
		{
			if (this.lockType != LockType.None)
				return await this.file.LoadBlockLocked(BlockIndex, true);
			else if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");
			else
				return await this.file.LoadBlock(BlockIndex);
		}

		/// <summary>
		/// Finds the position of an object in the underlying database.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>If the corresponding object was found. If so, the <see cref="Current"/> property will contain the corresponding
		/// object.</returns>
		public async Task<bool> GoToObject(object ObjectId)
		{
			uint BlockIndex = 0;
			object ObjectId2;
			int Len;
			int Pos;
			uint BlockLink;
			int Comparison;
			bool IsEmpty;

			this.currentRank = null;

			while (true)
			{
				this.currentBlockIndex = BlockIndex;
				this.currentBlock = await this.LoadBlock(this.currentBlockIndex);

				if (this.currentReader is null)
					this.currentReader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock, this.file.BlockLimit);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				do
				{
					Pos = this.currentReader.Position;

					BlockLink = this.currentReader.ReadBlockLink();                  // Block link.
					ObjectId2 = this.recordHandler.GetKey(this.currentReader);                         // Object ID of object.

					IsEmpty = (ObjectId2 is null);
					if (IsEmpty)
					{
						Comparison = 1;
						break;
					}

					Comparison = this.recordHandler.Compare(ObjectId, ObjectId2);

					Len = await this.recordHandler.GetPayloadSize(this.currentReader);     // Remaining length of object.
					this.currentReader.Position += Len;
				}
				while (Comparison > 0 && this.currentReader.BytesLeft >= 4);

				if (Comparison == 0)                                       // Object ID found.
				{
					await this.LoadObject(ObjectId2, Pos + 4);
					return true;
				}
				else if (IsEmpty || Comparison > 0)
				{
					if (this.currentHeader.LastBlockIndex == 0)
					{
						this.Reset();
						return false;
					}
					else
						BlockIndex = this.currentHeader.LastBlockIndex;
				}
				else
				{
					if (BlockLink == 0)
					{
						this.Reset();
						return false;
					}
					else
						BlockIndex = BlockLink;
				}
			}
		}

		/// <summary>
		/// Finds the object given its order in the underlying database.
		/// </summary>
		/// <param name="ObjectIndex">Order of object in database.</param>
		/// <returns>If the corresponding object was found. If so, the <see cref="Current"/> property will contain the corresponding
		/// object.</returns>
		public async Task<bool> GoToObject(ulong ObjectIndex)
		{
			ulong? Index = await this.GoToObject(ObjectIndex, 0);

			if (Index.HasValue && Index.Value == ObjectIndex + 1)
			{
				this.hasCurrent = true;
				this.currentRank = ObjectIndex;
				return true;
			}
			else
			{
				this.Reset();
				return false;
			}
		}

		private async Task<ulong?> GoToObject(ulong ObjectIndex, uint BlockIndex)
		{
			if (ObjectIndex == 0)
			{
				if (await this.GoToFirst(BlockIndex))
					return 1;
				else
					return null;
			}

			object ObjectId;
			int Len;
			int Pos;
			uint BlockLink;
			bool IsEmpty;

			byte[] Block = await this.LoadBlock(BlockIndex);
			BinaryDeserializer Reader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, Block, this.file.BlockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			ulong Count = 0;
			ulong? SubtreeCount;

			if (ObjectIndex >= Header.SizeSubtree && Header.SizeSubtree < uint.MaxValue)
				return Header.SizeSubtree;

			do
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadBlockLink();                  // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);                         // Object ID of object.

				IsEmpty = (ObjectId is null);
				if (IsEmpty)
					break;

				Len = await this.recordHandler.GetPayloadSize(Reader);     // Remaining length of object.
				Reader.Position += Len;

				if (BlockLink != 0)
				{
					SubtreeCount = await this.GoToObject(ObjectIndex - Count, BlockLink);
					if (SubtreeCount.HasValue)
					{
						Count += SubtreeCount.Value;
						if (Count > ObjectIndex)
							return Count;
					}
					else
						return null;
				}

				if (Count++ == ObjectIndex)
				{
					this.currentBlockIndex = BlockIndex;
					this.currentBlock = Block;
					this.currentReader = Reader;
					this.currentHeader = Header;

					await this.LoadObject(ObjectId, Pos + 4);
					return Count;
				}
			}
			while (Reader.BytesLeft >= 4);

			if (Header.LastBlockIndex != 0)
			{
				SubtreeCount = await this.GoToObject(ObjectIndex - Count, Header.LastBlockIndex);
				if (SubtreeCount.HasValue)
				{
					Count += SubtreeCount.Value;
					if (Count == ObjectIndex)
						return Count;
				}
				else
					return null;
			}

			return Count;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset()"/>
		/// </summary>
		public void Reset()
		{
			this.hasCurrent = false;
			this.currentTypeCompatible = false;
			this.currentRank = null;
			this.currentObjectId = null;
			this.current = default;
			this.currentSerializer = null;
		}

		/// <summary>
		/// Resets the enumerator, and sets the starting point to a given starting point.
		/// </summary>
		/// <param name="StartingPoint">Starting point to start enumeration.</param>
		public void Reset(Bookmark StartingPoint)
		{
			if (StartingPoint.File != this.file)
				throw new ArgumentException("Bookmark made for different file.", nameof(StartingPoint));

			this.Reset();
			this.startingPoint = StartingPoint.Position;
		}

		/// <summary>
		/// Skips a certain number of objects forward (positive <paramref name="NrObjects"/>) or backward (negative <paramref name="NrObjects"/>).
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
		public async Task<Bookmark> GetBookmark()
		{
			if (!this.hasCurrent)
			{
				if (!(this.startingPoint is null))
					return new Bookmark(this.file, this.startingPoint);
				else
				{
					if (!await this.GoToFirst())
						return null;

					BlockInfo Position = new BlockInfo(this.currentHeader, this.currentBlock, this.currentBlockIndex, this.currentObjPos, false);
					this.Reset();

					return new Bookmark(this.file, Position);
				}
			}
			else
				return new Bookmark(this.file, new BlockInfo(this.currentHeader, this.currentBlock, this.currentBlockIndex, this.currentObjPos, false));
		}

		/// <summary>
		/// <see cref="IEnumerable{T}.GetEnumerator()"/>
		/// </summary>
		public IEnumerator<T> GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.file.TimeoutMilliseconds);
		}

		/// <summary>
		/// <see cref="IEnumerable{T}.GetEnumerator()"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CursorEnumerator<T>(this, this.file.TimeoutMilliseconds);
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
			if (SortOrder is null)
				return true;

			int SortLen = SortOrder.Length;
			if (SortLen == 0)
				return true;

			if (this.fieldCount < SortLen)
				return false;

			string s;
			int FieldIndex = 0;
			int SortIndex;
			int NrConstantsFound = 0;
			bool Ascending;

			for (SortIndex = 0; SortIndex < SortLen; SortIndex++)
			{
				s = SortOrder[SortIndex];
				if (s.StartsWith("-"))
				{
					Ascending = false;
					s = s.Substring(1);
				}
				else
				{
					Ascending = true;

					if (s.StartsWith("+"))
						s = s.Substring(1);
				}

				while (FieldIndex < this.fieldCount)
				{
					if (s == this.objectIdMemberName)
						break;
					else if (ConstantFields is null || Array.IndexOf<string>(ConstantFields, this.objectIdMemberName) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						FieldIndex++;
					}
				}

				if (FieldIndex >= this.fieldCount)
					return false;

				if (!Ascending)
					return false;

				FieldIndex++;
			}

			return true;
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
			if (SortOrder is null)
				return false;

			int SortLen = SortOrder.Length;
			if (SortLen == 0)
				return true;

			if (this.fieldCount < SortLen)
				return false;

			string s;
			int FieldIndex = 0;
			int SortIndex;
			int NrConstantsFound = 0;
			bool Ascending;

			for (SortIndex = 0; SortIndex < SortLen; SortIndex++)
			{
				s = SortOrder[SortIndex];
				if (s.StartsWith("-"))
				{
					Ascending = false;
					s = s.Substring(1);
				}
				else
				{
					Ascending = true;

					if (s.StartsWith("+"))
						s = s.Substring(1);
				}

				while (FieldIndex < this.fieldCount)
				{
					if (s == this.objectIdMemberName)
						break;
					else if (ConstantFields is null || Array.IndexOf<string>(ConstantFields, this.objectIdMemberName) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						FieldIndex++;
					}
				}

				if (FieldIndex >= this.fieldCount)
					return false;

				if (Ascending)
					return false;

				FieldIndex++;
			}

			return true;
		}

		/// <summary>
		/// Index of current block
		/// </summary>
		public uint CurrentBlockIndex => currentBlockIndex;

		/// <summary>
		/// Current object position, within block.
		/// </summary>
		public int CurrentObjectPosition => currentObjPos;
	}
}
