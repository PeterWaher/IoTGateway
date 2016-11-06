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
			if (!this.hasCurrent)
				return await this.GoToFirst();

			Guid Guid;
			uint BlockLink;

			if (this.currentReader.BytesLeft >= 21)
			{
				BlockLink = this.currentReader.ReadUInt32();

				while (BlockLink != 0)
				{
					this.currentBlockIndex = BlockLink;
					this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
					this.currentReader.Restart(this.currentBlock, 0);

					this.currentHeader = new BlockHeader(this.currentReader);

					BlockLink = this.currentReader.ReadUInt32();
				}

				Guid = this.currentReader.ReadGuid();
				if (!Guid.Equals(Guid.Empty))
				{
					this.current = this.LoadObject();
					this.hasCurrent = true;
					return true;
				}
			}
			else
				Guid = Guid.Empty;

			BlockLink = this.currentHeader.LastBlockIndex;
			if (BlockLink != 0)
			{
				do
				{
					while (BlockLink != 0)
					{
						this.currentBlockIndex = BlockLink;
						this.currentBlock = await this.LoadBlock(this.currentBlockIndex);
						this.currentReader.Restart(this.currentBlock, 0);

						this.currentHeader = new BlockHeader(this.currentReader);

						BlockLink = this.currentReader.ReadUInt32();
					}

					Guid = this.currentReader.ReadGuid();
					if (!Guid.Equals(Guid.Empty))
					{
						this.current = this.LoadObject();
						this.hasCurrent = true;
						return true;
					}

					BlockLink = this.currentHeader.LastBlockIndex;
				}
				while (BlockLink != 0);
			}

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

					this.current = this.LoadObject();
					this.hasCurrent = true;
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
		/// <returns></returns>
		public async Task<bool> GoToFirst()
		{
			uint BlockLink;
			Guid Guid;
			bool IsEmpty;

			BlockLink = 0;

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
				this.hasCurrent = false;
				return false;
			}

			this.current = this.LoadObject();
			this.hasCurrent = true;

			return true;
		}

		private T LoadObject()
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

			return (T)Serializer.Deserialize(this.currentReader, ObjectSerializer.TYPE_OBJECT, false);
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

		// TODO:
		// Last
		// MovePrevious
		// SkipForward(n)
		// SkipBackward(n)
	}
}
