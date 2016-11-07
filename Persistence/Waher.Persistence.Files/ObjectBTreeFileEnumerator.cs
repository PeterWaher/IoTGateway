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
	public class ObjectBTreeFileEnumerator<T> : IEnumerator<T>
	{
		private ObjectBTreeFile file;
		private BlockHeader currentHeader;
		private BinaryDeserializer currentReader;
		private IObjectSerializer defaultSerializer;
		private T current;
		private byte[] currentBlock;
		private ulong blockUpdateCounter;
		private uint currentBlockIndex;
		private int currentObjPos;
		private bool locked;
		private bool hasCurrent;

		internal ObjectBTreeFileEnumerator(ObjectBTreeFile File, bool Locked)
		{
			this.file = File;
			this.currentBlockIndex = 0;
			this.currentBlock = null;
			this.currentReader = null;
			this.currentHeader = null;
			this.blockUpdateCounter = File.BlockUpdateCounter;
			this.locked = Locked;
			this.current = default(T);
			this.hasCurrent = false;

			if (typeof(T) == typeof(object))
				this.defaultSerializer = null;
			else
				this.defaultSerializer = this.file.Provider.GetObjectSerializer(typeof(T));
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
				await this.file.Release();
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
			if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");

			if (!this.hasCurrent)
				return await this.GoToFirst();

			Guid Guid;
			uint BlockLink;

			if (this.currentReader.BytesLeft >= 21)
			{
				BlockLink = this.currentReader.ReadUInt32();
				if (BlockLink != 0)
					return await this.GoToFirst(BlockLink);

				Guid = this.currentReader.ReadGuid();
				if (!Guid.Equals(Guid.Empty))
				{
					this.LoadObject();
					return true;
				}
			}
			else
				Guid = Guid.Empty;

			BlockLink = this.currentHeader.LastBlockIndex;
			if (BlockLink != 0)
				return await this.GoToFirst(BlockLink);

			do
			{
				BlockLink = this.currentHeader.ParentBlockIndex;
				if (BlockLink == 0 && this.currentBlockIndex == 0)
				{
					this.hasCurrent = false;
					this.current = default(T);
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
					int Pos;
					bool IsEmpty;

					do
					{
						this.currentReader.Position += Len;
						Pos = this.currentReader.Position;

						BlockLink2 = this.currentReader.ReadUInt32();
						Guid = this.currentReader.ReadGuid();
						if (IsEmpty = Guid.Equals(Guid.Empty))
							break;

						Len = (int)this.currentReader.ReadVariableLengthUInt64();
					}
					while (BlockLink2 != this.currentBlockIndex && this.currentReader.BytesLeft >= 21);

					if (IsEmpty || BlockLink2 != this.currentBlockIndex)
					{
						this.hasCurrent = false;
						this.current = default(T);
						return false;
					}

					this.currentBlockIndex = BlockLink;
					this.currentHeader = ParentHeader;
					this.currentReader.Position = Pos + 20;

					this.LoadObject();
					return true;
				}
			}
			while (Guid.Equals(Guid.Empty));

			this.current = default(T);
			this.hasCurrent = false;

			return false;
		}

		/// <summary>
		/// Goes to the first object.
		/// </summary>
		/// <returns>If a first object was found.</returns>
		public Task<bool> GoToFirst()
		{
			return this.GoToFirst(0);
		}

		private async Task<bool> GoToFirst(uint StartBlock)
		{
			Guid Guid;
			uint BlockLink;
			bool IsEmpty;

			BlockLink = StartBlock;

			do
			{
				this.currentBlockIndex = BlockLink;
				this.currentBlock = await this.LoadBlock(this.currentBlockIndex);

				if (this.currentReader == null)
					this.currentReader = new Serialization.BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				BlockLink = this.currentReader.ReadUInt32();
				Guid = this.currentReader.ReadGuid();
				IsEmpty = Guid.Equals(Guid.Empty);

				if (IsEmpty)
					BlockLink = this.currentHeader.LastBlockIndex;
			}
			while (BlockLink != 0);

			if (IsEmpty)
			{
				if (this.currentBlockIndex != 0)
				{
					this.currentBlockIndex = this.currentHeader.ParentBlockIndex;
					this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
					this.currentReader.Restart(this.currentBlock, 0);
					this.currentHeader = new BlockHeader(this.currentReader);

					BlockLink = this.currentReader.ReadUInt32();
					Guid = this.currentReader.ReadGuid();
					IsEmpty = Guid.Equals(Guid.Empty);
				}

				if (IsEmpty)
				{
					this.hasCurrent = false;
					this.current = default(T);
					return false;
				}
			}

			this.LoadObject();
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
			if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");

			if (!this.hasCurrent)
				return await this.GoToLast();

			Guid Guid;
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
					BlockLink = this.currentReader.ReadUInt32();
					Pos = this.currentReader.Position;
					this.currentReader.Position += 16;
					Len = (int)this.currentReader.ReadVariableLengthUInt64();
					this.currentReader.Position += Len;
				}
				while (this.currentReader.Position < this.currentObjPos);

				BlockLink = this.currentReader.ReadUInt32();

				if (BlockLink == 0)
				{
					this.currentReader.Position = Pos + 16;
					this.LoadObject();
					return true;
				}
				else
					return await this.GoToLast(BlockLink);
			}
			else
			{
				BlockLink = this.currentReader.ReadUInt32();
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
					Len = 0;
					LastPos = this.currentReader.Position;
					do
					{
						Pos = this.currentReader.Position;

						BlockLink = this.currentReader.ReadUInt32();
						Guid = this.currentReader.ReadGuid();
						IsEmpty = Guid.Equals(Guid.Empty);
						if (IsEmpty)
							break;

						LastPos = Pos;
						Len = (int)this.currentReader.ReadVariableLengthUInt64();
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 21);

					this.currentBlockIndex = ParentBlockLink;
					this.currentReader.Position = LastPos + 20;
					this.LoadObject();
					return true;
				}
				else
				{
					Len = LastPos = 0;
					do
					{
						Pos = this.currentReader.Position;

						BlockLink = this.currentReader.ReadUInt32();
						Guid = this.currentReader.ReadGuid();
						IsEmpty = Guid.Equals(Guid.Empty);
						if (IsEmpty)
							break;

						if (BlockLink == this.currentBlockIndex)
							break;

						LastPos = Pos;
						Len = (int)this.currentReader.ReadVariableLengthUInt64();
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 21);

					if (IsEmpty || BlockLink != this.currentBlockIndex)
					{
						this.current = default(T);
						this.hasCurrent = false;
						return false;
					}

					this.currentBlockIndex = ParentBlockLink;

					if (LastPos != 0)
					{
						this.currentReader.Position = LastPos + 20;
						this.LoadObject();
						return true;
					}
				}
			}

			this.current = default(T);
			this.hasCurrent = false;
			return false;
		}

		/// <summary>
		/// Goes to the last object.
		/// </summary>
		/// <returns>If a last object was found.</returns>
		public Task<bool> GoToLast()
		{
			return this.GoToLast(0);
		}

		private async Task<bool> GoToLast(uint StartBlock)
		{
			Guid Guid;
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

				if (this.currentReader == null)
					this.currentReader = new Serialization.BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				BlockLink = this.currentHeader.LastBlockIndex;
			}
			while (BlockLink != 0);

			Len = 0;
			LastPos = this.currentReader.Position;
			do
			{
				Pos = this.currentReader.Position;

				BlockLink = this.currentReader.ReadUInt32();
				Guid = this.currentReader.ReadGuid();
				IsEmpty = Guid.Equals(Guid.Empty);
				if (IsEmpty)
					break;

				LastPos = Pos;
				Len = (int)this.currentReader.ReadVariableLengthUInt64();
				this.currentReader.Position += Len;
			}
			while (this.currentReader.BytesLeft >= 21);

			this.currentReader.Position = LastPos;
			BlockLink = this.currentReader.ReadUInt32();
			Guid = this.currentReader.ReadGuid();
			IsEmpty = Guid.Equals(Guid.Empty);

			if (IsEmpty)
			{
				while (IsEmpty && this.currentBlockIndex != 0)
				{
					this.currentBlockIndex = this.currentHeader.ParentBlockIndex;
					this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
					this.currentReader.Restart(this.currentBlock, 0);
					this.currentHeader = new BlockHeader(this.currentReader);

					Len = 0;
					LastPos = this.currentReader.Position;
					do
					{
						Pos = this.currentReader.Position;

						BlockLink = this.currentReader.ReadUInt32();
						Guid = this.currentReader.ReadGuid();
						IsEmpty = Guid.Equals(Guid.Empty);
						if (IsEmpty)
							break;

						LastPos = Pos;
						Len = (int)this.currentReader.ReadVariableLengthUInt64();
						this.currentReader.Position += Len;
					}
					while (this.currentReader.BytesLeft >= 21);

					this.currentReader.Position = LastPos;
					BlockLink = this.currentReader.ReadUInt32();
					Guid = this.currentReader.ReadGuid();
					IsEmpty = Guid.Equals(Guid.Empty);
				}

				if (IsEmpty)
				{
					this.hasCurrent = false;
					this.current = default(T);
					return false;
				}
			}

			this.LoadObject();
			return true;
		}

		private void LoadObject()
		{
			IObjectSerializer Serializer = this.defaultSerializer;
			int Start = this.currentReader.Position - 16;

			if (Serializer == null)
			{
				this.currentReader.ReadVariableLengthUInt64();  // Length
				string TypeName = this.currentReader.ReadString();
				if (string.IsNullOrEmpty(TypeName))
					Serializer = this.file.GenericObjectSerializer;
				else
				{
					Type T = Types.GetType(TypeName);
					if (T != null)
						Serializer = this.file.Provider.GetObjectSerializer(T);
					else
						Serializer = this.file.GenericObjectSerializer;
				}
			}

			this.currentReader.Position = Start;
			this.currentObjPos = Start - 4;
			this.current = (T)Serializer.Deserialize(this.currentReader, ObjectSerializer.TYPE_OBJECT, false);
			this.hasCurrent = true;
		}

		private async Task<byte[]> LoadBlock(uint BlockIndex)
		{
			long PhysicalPosition = BlockIndex;
			PhysicalPosition *= this.file.BlockSize;

			if (this.locked)
				return await this.file.LoadBlockLocked(PhysicalPosition, true);
			else if (this.blockUpdateCounter != this.file.BlockUpdateCounter)
				throw new InvalidOperationException("Contents of file has been changed.");
			else
				return await this.file.LoadBlock(PhysicalPosition);
		}

		/// <summary>
		/// <see cref="IEnumerator{Object}.Reset"/>
		/// </summary>
		public void Reset()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Finds the position of an object in the underlying database.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>If the corresponding object was found. If so, the <see cref="Current"/> property will contain the corresponding
		/// object.</returns>
		public async Task<bool> GoToObject(Guid ObjectId)
		{
			uint BlockIndex = 0;
			Guid Guid;
			int Len;
			int Pos;
			uint BlockLink;
			int Comparison;
			bool IsEmpty;

			while (true)
			{
				this.currentBlockIndex = BlockIndex;
				this.currentBlock = await this.LoadBlock(this.currentBlockIndex);

				if (this.currentReader == null)
					this.currentReader = new Serialization.BinaryDeserializer(this.file.CollectionName, this.file.Encoding, this.currentBlock);
				else
					this.currentReader.Restart(this.currentBlock, 0);

				this.currentHeader = new BlockHeader(this.currentReader);

				do
				{
					Pos = this.currentReader.Position;

					BlockLink = this.currentReader.ReadUInt32();                  // Block link.
					Guid = this.currentReader.ReadGuid();                         // Object ID of object.

					IsEmpty = Guid.Equals(Guid.Empty);
					Comparison = ObjectId.CompareTo(Guid);

					if (IsEmpty)
						break;

					Len = (int)this.currentReader.ReadVariableLengthUInt64();     // Remaining length of object.
					this.currentReader.Position += Len;
				}
				while (Comparison > 0 && this.currentReader.BytesLeft >= 21);

				if (Comparison == 0)                                       // Object ID found.
				{
					this.currentReader.Position = Pos + 20;
					this.LoadObject();
					return true;
				}
				else if (IsEmpty || Comparison > 0)
				{
					if (this.currentHeader.LastBlockIndex == 0)
					{
						this.hasCurrent = false;
						this.current = default(T);
						return false;
					}
					else
						BlockIndex = this.currentHeader.LastBlockIndex;
				}
				else
				{
					if (BlockLink == 0)
					{
						this.hasCurrent = false;
						this.current = default(T);
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
				return true;
			}
			else
			{
				this.hasCurrent = false;
				this.current = default(T);
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

			Guid Guid;
			int Len;
			int Pos;
			uint BlockLink;
			bool IsEmpty;

			byte[] Block = await this.LoadBlock(BlockIndex);
			BinaryDeserializer Reader = new BinaryDeserializer(this.file.CollectionName, this.file.Encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			ulong Count = 0;
			ulong? SubtreeCount;

			if (ObjectIndex >= Header.SizeSubtree && Header.SizeSubtree < uint.MaxValue)
				return Header.SizeSubtree;

			do
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                  // Block link.
				Guid = Reader.ReadGuid();                         // Object ID of object.

				IsEmpty = Guid.Equals(Guid.Empty);
				if (IsEmpty)
					break;

				Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
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
					Reader.Position = Pos + 20;

					this.currentBlockIndex = BlockIndex;
					this.currentBlock = Block;
					this.currentReader = Reader;
					this.currentHeader = Header;

					this.LoadObject();
					return Count;
				}
			}
			while (Reader.BytesLeft >= 21);

			if (Header.LastBlockIndex != 0)
			{
				SubtreeCount = await this.GoToObject(Header.LastBlockIndex - Count, BlockLink);
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

	}
}
