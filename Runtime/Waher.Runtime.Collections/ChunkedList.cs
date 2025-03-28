using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// Callback method for the ForEach method.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="Item">Item being iterated.</param>
	/// <returns>If the iteration can continue (true) or should be terminated early (false).</returns>
	public delegate bool ForEachCallback<T>(T Item);

	/// <summary>
	/// Asynchronous callback method for the ForEachAsync method.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="Item">Item being iterated.</param>
	/// <returns>If the iteration can continue (true) or should be terminated early (false).</returns>
	public delegate Task<bool> ForEachAsyncCallback<T>(T Item);

	/// <summary>
	/// Callback method for the ForEachChunk method.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="Chunk">Chunk being iterated.</param>
	/// <param name="Offset">Offset into chunk where elements begin.</param>
	/// <param name="Count">Number of items in chunk.</param>
	/// <returns>If the iteration can continue (true) or should be terminated early (false).</returns>
	public delegate bool ForEachChunkCallback<T>(T[] Chunk, int Offset, int Count);

	/// <summary>
	/// Asynchronous callback method for the ForEachChunkAsync method.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="Chunk">Chunk being iterated.</param>
	/// <param name="Offset">Offset into chunk where elements begin.</param>
	/// <param name="Count">Number of items in chunk.</param>
	/// <returns>If the iteration can continue (true) or should be terminated early (false).</returns>
	public delegate Task<bool> ForEachChunkAsyncCallback<T>(T[] Chunk, int Offset, int Count);

	/// <summary>
	/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class ChunkedList<T> : ICollection<T>
	{
		private const int initialChunkSize = 16;

		private readonly int maxChunkSize;
		private readonly int minChunkSize;
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
			this.minChunkSize = InitialChunkSize;

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
			public int Start;
			public int Pos;

			public Chunk(int Size)
			{
				this.Elements = new T[Size];
				this.Size = Size;
				this.Start = 0;
				this.Pos = 0;
				this.Next = null;
				this.Prev = null;
			}

			public Chunk(int Size, Chunk Previous)
				: this(Size)
			{
				this.Prev = Previous;
				Previous.Next = this;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("Size: ");
				sb.Append(this.Size);
				sb.Append(", Start: ");
				sb.Append(this.Start);
				sb.Append(", Pos: ");
				sb.Append(this.Pos);
				sb.Append(", Count: ");
				sb.Append(this.Pos - this.Start);
				sb.Append(", HasNext: ");
				sb.Append(!(this.Next is null));
				sb.Append(", HasPrev: ");
				sb.Append(!(this.Prev is null));

				return base.ToString();
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Count = " + this.count.ToString();
		}

		#region ICollection<T>

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="Item">Item</param>
		public void Add(T Item)
		{
			if (this.current.Pos < this.current.Size)
				this.current.Elements[this.current.Pos++] = Item;
			else if (this.current.Start > 0)
			{
				if (this.current.Start < this.current.Pos)
				{
					Array.Copy(this.current.Elements, this.current.Start,
						this.current.Elements, 0, this.current.Pos - this.current.Start);
				}

				this.current.Pos -= this.current.Start;
				this.current.Start = 0;

				this.current.Elements[this.current.Pos++] = Item;
			}
			else
			{
				this.lastChunk = new Chunk(this.chunkSize, this.current);
				this.current = this.lastChunk;

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.current.Elements[this.current.Pos++] = Item;
			}

			this.count++;
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			this.lastChunk = this.firstChunk;
			this.firstChunk.Next = null;
			this.current = this.firstChunk;

			this.current.Start = 0;
			this.current.Pos = 0;

			Array.Clear(this.current.Elements, 0, this.current.Size);
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

			while (!(Loop is null))
			{
				if (Loop.Start < Loop.Pos &&
					Array.IndexOf(Loop.Elements, Item, Loop.Start, Loop.Pos - Loop.Start) >= 0)
				{
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
			int c;

			while (!(Loop is null))
			{
				if ((c = Loop.Pos - Loop.Start) > 0)
				{
					Array.Copy(Loop.Elements, Loop.Start, Desintation, DesintationIndex, c);
					DesintationIndex += c;
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
				c = Loop.Pos - Loop.Start;
				if (c > 0)
				{
					i = Array.IndexOf(Loop.Elements, Item, Loop.Start, c);
					if (i >= 0)
					{
						if (i == Loop.Start)
							Loop.Elements[Loop.Start++] = default;
						else
						{
							c = --Loop.Pos;
							if (i < c)
								Array.Copy(Loop.Elements, i + 1, Loop.Elements, i, c - i);
						
							Loop.Elements[c] = default;
						}

						this.count--;

						if (Loop.Start == Loop.Pos)
						{
							Loop.Start = 0;
							Loop.Pos = 0;

							if (Loop.Prev is null)
								this.firstChunk = Loop.Next ?? Loop;
							else
								Loop.Prev.Next = Loop.Next;

							if (Loop.Next is null)
								this.lastChunk = Loop.Prev ?? Loop;
							else
								Loop.Next.Prev = Loop.Prev;

							this.chunkSize >>= 1;
							if (this.chunkSize < this.minChunkSize)
								this.chunkSize = this.minChunkSize;

						}

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
			// More efficient than yield.
			// (But ForEach is more efficient that using an enumerator.)
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
				this.pos = -1;
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
					this.pos = this.current.Start - 1;
				}

				while (++this.pos >= this.current.Pos)
				{
					this.current = this.current.Next;
					if (this.current is null)
						return false;

					this.pos = this.current.Start - 1;
				}

				return true;
			}

			public void Reset()
			{
				this.current = null;
				this.pos = -1;
			}
		}

		#endregion

		#region ForEach Optimization

		/// <summary>
		/// Iterates through all elements in the collection, and calls the callback method 
		/// in <paramref name="Callback"/> for each element. The loop can be terminated 
		/// early, by returning false from the callback method.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the loop was completed (true) or terminated early (false).</returns>
		public bool ForEach(ForEachCallback<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = 0, c = Loop.Pos; i < c; i++)
				{
					if (!Callback(Loop.Elements[i]))
						return false;
				}

				Loop = Loop.Next;
			}

			return true;
		}

		/// <summary>
		/// Iterates through all elements in the collection, and calls the callback method 
		/// in <paramref name="Callback"/> for each element. The loop can be terminated 
		/// early, by returning false from the callback method.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the loop was completed (true) or terminated early (false).</returns>
		public async Task<bool> ForEachAsync(ForEachAsyncCallback<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = 0, c = Loop.Pos; i < c; i++)
				{
					if (!await Callback(Loop.Elements[i]))
						return false;
				}

				Loop = Loop.Next;
			}

			return true;
		}

		/// <summary>
		/// Iterates through all chunks of elements in the collection, and calls the callback 
		/// method in <paramref name="Callback"/> for each chunk. The loop can be terminated 
		/// early, by returning false from the callback method.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the loop was completed (true) or terminated early (false).</returns>
		public bool ForEachChunk(ForEachChunkCallback<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				i = Loop.Start;
				c = Loop.Pos - i;
				if (c > 0)
				{
					if (!Callback(Loop.Elements, i, c))
						return false;
				}

				Loop = Loop.Next;
			}

			return true;
		}

		/// <summary>
		/// Iterates through all chunks of elements in the collection, and calls the callback 
		/// method in <paramref name="Callback"/> for each chunk. The loop can be terminated 
		/// early, by returning false from the callback method.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the loop was completed (true) or terminated early (false).</returns>
		public async Task<bool> ForEachChunkAsync(ForEachChunkAsyncCallback<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				i = Loop.Start;
				c = Loop.Pos - i;
				if (c > 0)
				{
					if (!await Callback(Loop.Elements, i, c))
						return false;
				}

				Loop = Loop.Next;
			}

			return true;
		}

		#endregion

		#region Members corresponding to LinkedList<T> interface.

		/// <summary>
		/// If there is a last item in the collection
		/// </summary>
		public bool HasLastItem => !(this.lastChunk is null) && this.lastChunk.Pos > this.lastChunk.Start;

		/// <summary>
		/// Last item in the collection.
		/// </summary>
		public T LastItem
		{
			get
			{
				if (this.lastChunk is null || this.lastChunk.Pos == this.lastChunk.Start)
					throw new InvalidOperationException("No last item available.");

				return this.lastChunk.Elements[this.lastChunk.Pos - 1];
			}
		}

		/// <summary>
		/// If there is a first item in the collection
		/// </summary>
		public bool HasFirstItem => !(this.firstChunk is null) && this.firstChunk.Pos > this.firstChunk.Start;

		/// <summary>
		/// First item in the collection.
		/// </summary>
		public T FirstItem
		{
			get
			{
				if (this.firstChunk is null || this.firstChunk.Pos == this.firstChunk.Start)
					throw new InvalidOperationException("No first item available.");

				return this.firstChunk.Elements[this.firstChunk.Start];
			}
		}

		/// <summary>
		/// Adds a new item first in the collection.
		/// </summary>
		/// <param name="Value">Item to add.</param>
		/// <returns></returns>
		public void AddFirstItem(T Value)
		{
			if (this.firstChunk.Start > 0)
				this.firstChunk.Elements[--this.firstChunk.Start] = Value;
			else if (this.firstChunk.Pos == 0)
			{
				this.firstChunk.Start = this.firstChunk.Pos = this.firstChunk.Size;
				this.firstChunk.Elements[--this.firstChunk.Start] = Value;
			}
			else
			{
				Chunk NewChunk = new Chunk(this.chunkSize)
				{
					Next = this.firstChunk,
				};

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.firstChunk.Prev = NewChunk;
				this.firstChunk = NewChunk;

				this.firstChunk.Start = this.firstChunk.Pos = this.firstChunk.Size;
				this.firstChunk.Elements[--this.firstChunk.Start] = Value;
			}

			this.count++;
		}

		/// <summary>
		/// Adds a new item last in the collection.
		/// </summary>
		/// <param name="Value">Item to add.</param>
		/// <returns></returns>
		public void AddLastItem(T Value)
		{
			this.Add(Value);
		}

		/// <summary>
		/// Removes the first item in the collection.
		/// </summary>
		/// <returns>The removed item.</returns>
		public T RemoveFirst()
		{
			if (this.firstChunk.Start < this.firstChunk.Pos)
			{
				T Result = this.firstChunk.Elements[this.firstChunk.Start++];
				this.count--;

				if (this.firstChunk.Start == this.firstChunk.Pos)
				{
					this.firstChunk.Start = 0;
					this.firstChunk.Pos = 0;

					if (!(this.firstChunk.Next is null))
					{
						Chunk Temp = this.firstChunk;
						this.firstChunk = this.firstChunk.Next;
						this.firstChunk.Prev = null;
						Temp.Next = null;

						this.chunkSize >>= 1;
						if (this.chunkSize < this.minChunkSize)
							this.chunkSize = this.minChunkSize;
					}
				}

				return Result;
			}
			else
				throw new InvalidOperationException("No first item to remove.");
		}

		/// <summary>
		/// Removes the last item in the collection.
		/// </summary>
		/// <returns>The removed item.</returns>
		public T RemoveLast()
		{
			if (this.lastChunk.Pos > this.lastChunk.Start)
			{
				T Result = this.lastChunk.Elements[--this.lastChunk.Pos];
				this.count--;

				if (this.lastChunk.Pos == this.lastChunk.Start)
				{
					this.lastChunk.Start = 0;
					this.lastChunk.Pos = 0;

					if (!(this.lastChunk.Prev is null))
					{
						Chunk Temp = this.lastChunk;
						this.lastChunk = this.lastChunk.Prev;
						this.lastChunk.Next = null;
						Temp.Prev = null;

						this.chunkSize >>= 1;
						if (this.chunkSize < this.minChunkSize)
							this.chunkSize = this.minChunkSize;
					}
				}

				return Result;
			}
			else
				throw new InvalidOperationException("No last item to remove.");
		}

		#endregion
	}
}
