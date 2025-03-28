using System;
using System.Collections;
using System.Collections.Generic;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class ChunkedList<T> : ICollection<T>
	{
		private const int initialChunkSize = 4;

		private readonly int maxChunkSize;
		private Chunk current;
		private Chunk firstChunk;
		private Chunk lastChunk;
		private int chunkSize;
		private int count = 0;

		/// <summary>
		/// Number of elements in collection.
		/// </summary>
		public int Count => this.count;

		/// <summary>
		/// If collection is read-only (false).
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
		/// </summary>
		public ChunkedList()
			: this(initialChunkSize, int.MaxValue)
		{
		}

		/// <summary>
		/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="InitialChunkSize">Initial Chunk Size.</param>
		public ChunkedList(int InitialChunkSize)
			: this(InitialChunkSize, InitialChunkSize)
		{
		}

		/// <summary>
		/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="InitialChunkSize">Initial Chunk Size.</param>
		/// <param name="MaxChunkSize">Maximum Chunk Size.</param>
		public ChunkedList(int InitialChunkSize, int MaxChunkSize)
		{
			if (InitialChunkSize <= 0)
				throw new ArgumentException("Chunk size must be positive.", nameof(InitialChunkSize));

			if (MaxChunkSize < InitialChunkSize)
				throw new ArgumentException("Max chunk size must be greater than or equal to initial chunk size.", nameof(MaxChunkSize));

			this.chunkSize = InitialChunkSize;
			this.maxChunkSize = MaxChunkSize;

			this.current = this.firstChunk = this.lastChunk = new Chunk(InitialChunkSize);

			this.chunkSize <<= 1;
			if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
				this.chunkSize = this.maxChunkSize;
		}

		private class Chunk
		{
			public T[] Elements;
			public Chunk Next;
			public Chunk Prev;
			public int Size;
			public int Count;

			public Chunk(int Size)
			{
				this.Elements = new T[Size];
				this.Size = Size;
				this.Count = 0;
				this.Next = null;
				this.Prev = null;
			}

			public Chunk(int Size, Chunk Previous)
				: this(Size)
			{
				this.Prev = Previous;
				Previous.Next = this;
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="Item">Item</param>
		public void Add(T Item)
		{
			if (this.current.Count < this.current.Size)
				this.current.Elements[this.current.Count++] = Item;
			else
			{
				this.lastChunk = new Chunk(this.chunkSize, this.current);
				this.current = this.lastChunk;

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.current.Elements[this.current.Count++] = Item;
			}

			this.count++;
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			this.current = this.firstChunk = this.lastChunk = new Chunk(this.chunkSize);
			this.count = 0;
		}

		/// <summary>
		/// Checks if an item is a member of the collection.
		/// </summary>
		/// <param name="Item">Item to search for.</param>
		/// <returns>If the item is found in the collection.</returns>
		public bool Contains(T Item)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = 0, c = Loop.Count; i < c; i++)
				{
					if (Item.Equals(Loop.Elements[i]))
						return true;
				}

				Loop = Loop.Next;
			}

			return false;
		}

		/// <summary>
		/// Copies the contents of the collection to an array.
		/// </summary>
		/// <param name="Desintation">Destination array.</param>
		/// <param name="DesintationIndex">Start index into the array where elements are to be copied.</param>
		public void CopyTo(T[] Desintation, int DesintationIndex)
		{
			Chunk Loop = this.firstChunk;

			while (!(Loop is null))
			{
				if (Loop.Count > 0)
				{
					Array.Copy(Loop.Elements, 0, Desintation, DesintationIndex, Loop.Count);
					DesintationIndex += Loop.Count;
				}

				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Removes an element from the collection.
		/// </summary>
		/// <param name="Item">Item to remove from the collection.</param>
		/// <returns>If the element was found and removed.</returns>
		public bool Remove(T Item)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = 0, c = Loop.Count; i < c; i++)
				{
					if (Item.Equals(Loop.Elements[i]))
					{
						if (i < c - 1)
							Array.Copy(Loop.Elements, i + 1, Loop.Elements, i, c - i - 1);

						Loop.Count--;
						this.count--;

						return true;
					}
				}

				Loop = Loop.Next;
			}

			return false;
		}

		/// <summary>
		/// Returns an enumerator for the collection.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new ChunkedListEnumerator(this.firstChunk);
		}

		/// <summary>
		/// Returns an enumerator for the collection.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private class ChunkedListEnumerator : IEnumerator<T>
		{
			private Chunk first;
			private Chunk current;
			private int pos;

			public ChunkedListEnumerator(Chunk FirstChunk)
			{
				this.first = FirstChunk;
				this.current = null;
				this.pos = 0;
			}

			public T Current
			{
				get
				{
					if (this.current is null)
						throw new InvalidOperationException("Enumeration must be started or reset.");
					else
						return this.current.Elements[this.pos];
				}
			}

			object IEnumerator.Current => this.Current;

			public void Dispose()
			{
				this.first = null;
				this.current = null;
			}

			public bool MoveNext()
			{
				if (this.current is null)
				{
					this.current = this.first;
					this.pos = -1;
				}

				while (++this.pos >= this.current.Count)
				{
					this.current = this.current.Next;
					if (this.current is null)
						return false;

					this.pos = -1;
				}

				return true;
			}

			public void Reset()
			{
				this.current = null;
				this.pos = -1;
			}
		}
	}
}
