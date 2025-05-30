﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Runtime.Collections
{
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
	/// Asynchronous predicate function with one argument.
	/// </summary>
	/// <typeparam name="T">Argument type.</typeparam>
	/// <param name="Arg">Argument.</param>
	/// <returns>Result of call.</returns>
	public delegate Task<bool> PredicateAsync<in T>(T Arg);

	/// <summary>
	/// Delegate for callback methods that update the elements of a <see cref="ChunkedList{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="Value">Value that can be updated.</param>
	/// <param name="Keep">If the value should be kept.</param>
	/// <returns>If the iteration can continue (true) or should be terminated early (false).</returns>
	public delegate bool UpdateCallback<T>(ref T Value, out bool Keep);

	/// <summary>
	/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(ChunkedListDebugView<>))]
	public class ChunkedList<T> : ICollection<T>, IList<T>
	{
		private const int initialChunkSize = 16;

		private readonly int maxChunkSize;
		private readonly int minChunkSize;
		private Chunk firstChunk;
		private Chunk lastChunk;
		private int chunkSize;
		private int count;
		private bool? nullable;

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
			this.count = 0;

			this.firstChunk = this.lastChunk = new Chunk(InitialChunkSize);

			this.chunkSize <<= 1;
			if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
				this.chunkSize = this.maxChunkSize;
		}

		/// <summary>
		/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>,
		/// initially filled with elements in <paramref name="InitialElements"/>.
		/// </summary>
		/// <param name="InitialElements">Initial elements.</param>
		public ChunkedList(params T[] InitialElements)
			: this(InitialElements, int.MaxValue)
		{
		}

		/// <summary>
		/// A chunked list is a linked list of chunks of objects of type <typeparamref name="T"/>,
		/// initially filled with elements in <paramref name="InitialElements"/>.
		/// </summary>
		/// <param name="InitialElements">Initial elements.</param>
		/// <param name="MaxChunkSize">Maximum Chunk Size.</param>
		public ChunkedList(T[] InitialElements, int MaxChunkSize)
		{
			this.chunkSize = this.count = InitialElements.Length;
			if (this.chunkSize == 0)
				this.chunkSize = initialChunkSize;

			if (MaxChunkSize < this.chunkSize)
				throw new ArgumentException("Max chunk size must be greater than or equal to initial chunk size.", nameof(MaxChunkSize));

			this.maxChunkSize = MaxChunkSize;
			this.minChunkSize = this.chunkSize;

			this.firstChunk = this.lastChunk = new Chunk(InitialElements);

			this.chunkSize <<= 1;
			if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
				this.chunkSize = this.maxChunkSize;
		}

		internal class Chunk
		{
			public ChunkNode<T> node;
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

			public Chunk(T[] Elements)
			{
				this.Elements = Elements;
				this.Size = this.Pos = this.Elements.Length;
				this.Start = 0;
				this.Next = null;
				this.Prev = null;
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

			public ChunkNode<T> Node
			{
				get
				{
					if (this.node is null)
						this.node = new ChunkNode<T>(this);

					return this.node;
				}
			}

			public T this[int Index]
			{
				get
				{
					if (Index < this.Start || Index >= this.Pos)
						throw new IndexOutOfRangeException();

					return this.Elements[Index];
				}

				set
				{
					if (Index < this.Start || Index >= this.Pos)
						throw new IndexOutOfRangeException();

					this.Elements[Index] = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Count = " + this.count.ToString();
		}

		/// <summary>
		/// First chunk
		/// </summary>
		public ChunkNode<T> FirstChunk => this.firstChunk.Node;

		/// <summary>
		/// Last chunk
		/// </summary>
		public ChunkNode<T> LastChunk => this.firstChunk.Node;

		#region ICollection<T>

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="Item">Item</param>
		public void Add(T Item)
		{
			if (this.lastChunk.Pos < this.lastChunk.Size)
				this.lastChunk.Elements[this.lastChunk.Pos++] = Item;
			else if (this.lastChunk.Start > 0)
			{
				if (this.lastChunk.Start < this.lastChunk.Pos)
				{
					Array.Copy(this.lastChunk.Elements, this.lastChunk.Start,
						this.lastChunk.Elements, 0, this.lastChunk.Pos - this.lastChunk.Start);
				}

				this.lastChunk.Pos -= this.lastChunk.Start;
				this.lastChunk.Start = 0;

				this.lastChunk.Elements[this.lastChunk.Pos++] = Item;
			}
			else
			{
				this.lastChunk = new Chunk(this.chunkSize, this.lastChunk);

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.lastChunk.Elements[this.lastChunk.Pos++] = Item;
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
			this.firstChunk.Start = 0;
			this.firstChunk.Pos = 0;
			this.chunkSize = this.firstChunk.Size;

			Array.Clear(this.firstChunk.Elements, 0, this.firstChunk.Size);
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
		/// <param name="Destination">Destination array.</param>
		/// <param name="DestinationIndex">Start index into the array where elements are to be copied.</param>
		public void CopyTo(T[] Destination, int DestinationIndex)
		{
			this.CopyTo(0, Destination, DestinationIndex, this.count);
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
							Loop.Start++;
						else
						{
							c = --Loop.Pos;
							if (i < c)
								Array.Copy(Loop.Elements, i + 1, Loop.Elements, i, c - i);
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

							Loop.Next = null;
							Loop.Prev = null;

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
			private bool ended;

			public ChunkedListEnumerator(Chunk FirstChunk)
			{
				this.first = FirstChunk;
				this.current = null;
				this.pos = -1;
				this.ended = false;
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
				this.ended = true;
			}

			public bool MoveNext()
			{
				if (this.current is null)
				{
					if (this.ended)
						return false;

					this.current = this.first;
					this.pos = this.current.Start - 1;
				}

				while (++this.pos >= this.current.Pos)
				{
					this.current = this.current.Next;
					if (this.current is null)
					{
						this.ended = true;
						return false;
					}

					this.pos = this.current.Start - 1;
				}

				return true;
			}

			public void Reset()
			{
				this.current = null;
				this.pos = -1;
				this.ended = false;
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
		public bool ForEach(Predicate<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
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
		public async Task<bool> ForEachAsync(PredicateAsync<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
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

		#region Update Optimization

		/// <summary>
		/// Iterates through all elements in the collection, and calls the callback method 
		/// in <paramref name="Callback"/> for each element, allowing the method to update the
		/// value or remove it. The loop can be terminated early, by returning false from the 
		/// callback method. Deleting an element is done by returning null from the callback 
		/// method.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the loop was completed (true) or terminated early (false).</returns>
		public bool Update(UpdateCallback<T> Callback)
		{
			Chunk Loop = this.firstChunk;
			Chunk Next;
			int i, c, d;
			bool Continue = true;
			bool Deleted = false;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					Continue = Callback(ref Loop.Elements[i], out bool Keep);

					if (!Keep)
					{
						if (i == Loop.Start)
							Loop.Start++;
						else
						{
							d = c - i - 1;
							if (d > 0)
								Array.Copy(Loop.Elements, i + 1, Loop.Elements, i, d);

							Loop.Pos--;
							i--;
							c--;
						}

						this.count--;
						Deleted = true;
					}

					if (!Continue)
						break;
				}

				Next = Loop.Next;

				if (Deleted)
				{
					Deleted = false;

					if (Loop.Start == Loop.Pos)
					{
						Loop.Start = 0;
						Loop.Pos = 0;

						if (Loop.Prev is null)
							this.firstChunk = Next ?? Loop;
						else
							Loop.Prev.Next = Next;

						if (Next is null)
							this.lastChunk = Loop.Prev ?? Loop;
						else
							Next.Prev = Loop.Prev;

						Loop.Next = null;
						Loop.Prev = null;

						this.chunkSize >>= 1;
						if (this.chunkSize < this.minChunkSize)
							this.chunkSize = this.minChunkSize;
					}
				}

				Loop = Next;

				if (!Continue)
					return false;
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
				{
					if (!this.nullable.HasValue)
						this.nullable = typeof(T).IsClass;

					if (this.nullable.Value)
						return default;
					else
						throw new InvalidOperationException("No last item in the collection.");
				}
				else
					return this.lastChunk.Elements[this.lastChunk.Pos - 1];
			}

			set
			{
				if (this.lastChunk is null)
				{
					this.firstChunk = this.lastChunk = new Chunk(this.chunkSize);

					this.chunkSize <<= 1;
					if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
						this.chunkSize = this.maxChunkSize;

					this.lastChunk.Pos = 1;
					this.lastChunk.Elements[0] = value;
					this.count++;
				}
				else if (this.lastChunk.Pos == this.lastChunk.Start)
				{
					this.lastChunk.Start = 0;
					this.lastChunk.Pos = 1;
					this.lastChunk.Elements[0] = value;
					this.count++;
				}
				else
					this.lastChunk.Elements[this.lastChunk.Pos - 1] = value;
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
				{
					if (!this.nullable.HasValue)
						this.nullable = typeof(T).IsClass;

					if (this.nullable.Value)
						return default;
					else
						throw new InvalidOperationException("No first item in the collection.");
				}
				else
					return this.firstChunk.Elements[this.firstChunk.Start];
			}

			set
			{
				if (this.firstChunk is null)
				{
					this.firstChunk = this.lastChunk = new Chunk(this.chunkSize);

					this.chunkSize <<= 1;
					if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
						this.chunkSize = this.maxChunkSize;

					this.firstChunk.Pos = 1;
					this.firstChunk.Elements[0] = value;
					this.count++;
				}
				else if (this.firstChunk.Pos == this.firstChunk.Start)
				{
					this.firstChunk.Start = 0;
					this.firstChunk.Pos = 1;
					this.firstChunk.Elements[0] = value;
					this.count++;
				}
				else
					this.firstChunk.Elements[0] = value;
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

		#region IList<T>

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="Index">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">index is not a valid index.</exception>"
		public T this[int Index]
		{
			get
			{
				if (Index < 0 || Index >= this.count)
					throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

				Chunk Loop = this.firstChunk;
				int c;

				while (!(Loop is null))
				{
					c = Loop.Pos - Loop.Start;

					if (Index < c)
						return Loop.Elements[Loop.Start + Index];
					else
						Index -= c;

					Loop = Loop.Next;
				}

				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));
			}

			set
			{
				if (Index < 0 || Index >= this.count)
					throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

				Chunk Loop = this.firstChunk;
				int c;

				while (!(Loop is null))
				{
					c = Loop.Pos - Loop.Start;

					if (Index < c)
					{
						Loop.Elements[Loop.Start + Index] = value;
						return;
					}
					else
						Index -= c;

					Loop = Loop.Next;
				}

				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));
			}
		}

		/// <summary>
		/// Determines the index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf(T Item)
		{
			return this.IndexOf(Item, 0, this.count);
		}

		/// <summary>
		/// Determines the index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <param name="Index">Start index of search.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf(T Item, int Index)
		{
			return this.IndexOf(Item, Index, this.count - Index);
		}

		/// <summary>
		/// Determines the index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <param name="Index">Start index of search.</param>
		/// <param name="Count">Number of items to search, maximum.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf(T Item, int Index, int Count)
		{
			if (Index < 0 || Index >= this.count)
				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

			if (Count < 0 || Index + Count > this.count)
				throw new ArgumentOutOfRangeException("Count out of bounds.", nameof(Count));

			Chunk Loop = this.firstChunk;
			int i, c, Result = 0;

			while (!(Loop is null) && Count >= 0)
			{
				c = Loop.Pos - Loop.Start;

				if (c > 0)
				{
					if (Index > 0)
					{
						if (Index >= c)
							Index -= c;
						else
						{
							c -= Index;

							if (Count < c)
								c = Count;

							if ((i = Array.IndexOf(Loop.Elements, Item, Loop.Start + Index, c)) >= 0)
								return Result + i;

							Count -= c;
						}
					}
					else
					{
						if (Count < c)
							c = Count;

						if ((i = Array.IndexOf(Loop.Elements, Item, Loop.Start, c)) >= 0)
							return Result + i;

						Count -= c;
					}

					Result += c;
				}

				Loop = Loop.Next;
			}

			return -1;
		}

		/// <summary>
		/// Determines the last index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int LastIndexOf(T Item)
		{
			return this.LastIndexOf(Item, this.count - 1, this.count);
		}

		/// <summary>
		/// Determines the last index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <param name="Index">Start index of search.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int LastIndexOf(T Item, int Index)
		{
			return this.LastIndexOf(Item, Index, Index + 1);
		}

		/// <summary>
		/// Determines the last index of a specific item
		/// </summary>
		/// <param name="Item">The object to locate</param>
		/// <param name="Index">Start index of search.</param>
		/// <param name="Count">Number of items to search, maximum.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int LastIndexOf(T Item, int Index, int Count)
		{
			if (Index < 0 || Index >= this.count)
				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

			if (Count < 0 || Count > Index + 1)
				throw new ArgumentOutOfRangeException("Count out of bounds.", nameof(Count));

			Chunk Loop = this.firstChunk;
			int i = 0, j = 0, c;

			while (!(Loop is null) && Index > 0)
			{
				c = Loop.Pos - Loop.Start;

				if (c <= Index)
				{
					Index -= c;
					j += c;
				}
				else
				{
					i = Index;
					break;
				}

				Loop = Loop.Next;
			}

			if (Loop is null)
			{
				Loop = this.lastChunk;
				i = Loop.Pos - Loop.Start;
			}

			j += Loop.Pos - Loop.Start;

			while (!(Loop is null) && Count >= 0)
			{
				c = Loop.Pos - Loop.Start;
				j -= c;

				if (c > 0)
				{
					if (Count < c)
						c = Count;

					if (i < 0)
						i = Array.LastIndexOf(Loop.Elements, Item, Loop.Pos - 1, c);
					else
						i = Array.LastIndexOf(Loop.Elements, Item, Loop.Start + i, c);

					if (i >= 0)
						return j + i;

					Count -= c;
				}

				Loop = Loop.Prev;
			}

			return -1;
		}

		#endregion

		#region Members corresponding to List<T> interface.

		/// <summary>
		/// Inserts an item to the list at the specified index.
		/// </summary>
		/// <param name="Index">The zero-based index at which item should be inserted.</param>
		/// <param name="Item">The object to insert into the list.</param>
		public void Insert(int Index, T Item)
		{
			if (Index < 0 || Index > this.count)
				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

			if (Index == this.count)
			{
				this.Add(Item);
				return;
			}

			Chunk Loop = this.firstChunk;
			int c;

			while (!(Loop is null))
			{
				c = Loop.Pos - Loop.Start;

				if (Index < c)
				{
					if (Loop.Start > 0)
					{
						if (Index > 0)
							Array.Copy(Loop.Elements, Loop.Start, Loop.Elements, Loop.Start - 1, Index);

						Loop.Elements[--Loop.Start + Index] = Item;
					}
					else if (Loop.Pos < Loop.Size)
					{
						if (Index < Loop.Pos)
							Array.Copy(Loop.Elements, Index, Loop.Elements, Index + 1, Loop.Pos - Index);

						Loop.Elements[Index] = Item;
						Loop.Pos++;
					}
					else
					{
						Chunk Temp = Loop.Next;
						Chunk NewChunk = new Chunk(Loop.Size, Loop)
						{
							Next = Temp
						};

						c = Loop.Size >> 1;

						if (Index < c)
						{
							NewChunk.Pos = Loop.Size - c;
							Array.Copy(Loop.Elements, c, NewChunk.Elements, 0, NewChunk.Pos);
							Array.Copy(Loop.Elements, Index, Loop.Elements, Index + 1, c - Index);
							Loop.Elements[Index] = Item;
							Loop.Pos = c + 1;
						}
						else
						{
							Loop.Pos = c;
							Index -= c;
							NewChunk.Pos = Loop.Size - c + 1;

							if (Index > 0)
							{
								Array.Copy(Loop.Elements, c, NewChunk.Elements, 0, Index);
								c += Index;
							}

							NewChunk.Elements[Index++] = Item;

							if (Loop.Size > c)
								Array.Copy(Loop.Elements, c, NewChunk.Elements, Index, Loop.Size - c);
						}
					}

					this.count++;
					return;
				}
				else
					Index -= c;

				Loop = Loop.Next;
			}

			throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="Index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int Index)
		{
			if (Index < 0 || Index >= this.count)
				throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));

			Chunk Loop = this.firstChunk;
			int c;

			while (!(Loop is null))
			{
				c = Loop.Pos - Loop.Start;

				if (Index < c)
				{
					if (Index == 0)
						Loop.Start++;
					else
					{
						Index += Loop.Start;
						c = --Loop.Pos;

						if (Index < c)
							Array.Copy(Loop.Elements, Index + 1, Loop.Elements, Index, c - Index);
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

						Loop.Next = null;
						Loop.Prev = null;

						this.chunkSize >>= 1;
						if (this.chunkSize < this.minChunkSize)
							this.chunkSize = this.minChunkSize;
					}

					return;
				}
				else
					Index -= c;

				Loop = Loop.Next;
			}

			throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(Index));
		}

		/// <summary>
		/// Adds a range of elements (last) to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRange(IEnumerable<T> Collection)
		{
			if (Collection is T[] A)
				this.AddRange(A);
			else
			{
				IEnumerator<T> e = Collection.GetEnumerator();

				if (this.lastChunk.Start > 0)
				{
					if (this.lastChunk.Start < this.lastChunk.Pos)
					{
						Array.Copy(this.lastChunk.Elements, this.lastChunk.Start,
							this.lastChunk.Elements, 0, this.lastChunk.Pos - this.lastChunk.Start);
					}

					this.lastChunk.Pos -= this.lastChunk.Start;
					this.lastChunk.Start = 0;
				}

				while (this.lastChunk.Pos < this.lastChunk.Size && e.MoveNext())
				{
					this.lastChunk.Elements[this.lastChunk.Pos++] = e.Current;
					this.count++;
				}

				while (this.lastChunk.Pos >= this.lastChunk.Size && e.MoveNext())
				{
					this.lastChunk = new Chunk(this.chunkSize, this.lastChunk);

					this.chunkSize <<= 1;
					if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
						this.chunkSize = this.maxChunkSize;

					this.lastChunk.Elements[this.lastChunk.Pos++] = e.Current;
					this.count++;

					while (this.lastChunk.Pos < this.lastChunk.Size && e.MoveNext())
					{
						this.lastChunk.Elements[this.lastChunk.Pos++] = e.Current;
						this.count++;
					}
				}
			}
		}

		/// <summary>
		/// Adds a range of elements (last) to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRange(ChunkedList<T> Collection)
		{
			Chunk Loop = Collection.firstChunk;

			while (!(Loop is null))
			{
				this.AddRange(Loop.Elements, Loop.Start, Loop.Pos - Loop.Start);
				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Adds a range of elements (last) to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRange(T[] Collection)
		{
			this.AddRange(Collection, 0, Collection.Length);
		}

		/// <summary>
		/// Adds a range of elements (last) to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		/// <param name="Index">Index of first element to add.</param>
		/// <param name="Count">Number of elements to add.</param>
		public void AddRange(T[] Collection, int Index, int Count)
		{
			int d;

			Count += Index;
			
			if (this.lastChunk.Start > 0)
			{
				if (this.lastChunk.Start < this.lastChunk.Pos)
				{
					Array.Copy(this.lastChunk.Elements, this.lastChunk.Start,
						this.lastChunk.Elements, 0, this.lastChunk.Pos - this.lastChunk.Start);
				}

				this.lastChunk.Pos -= this.lastChunk.Start;
				this.lastChunk.Start = 0;
			}

			while (this.lastChunk.Pos < this.lastChunk.Size && Index < Count)
			{
				d = Math.Min(this.lastChunk.Size - this.lastChunk.Pos, Count - Index);
				Array.Copy(Collection, Index, this.lastChunk.Elements, this.lastChunk.Pos, d);
				Index += d;
				this.count += d;
				this.lastChunk.Pos += d;
			}

			while (Index < Count)
			{
				this.lastChunk = new Chunk(this.chunkSize, this.lastChunk);

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.lastChunk.Elements[this.lastChunk.Pos++] = Collection[Index++];
				this.count++;

				while (this.lastChunk.Pos < this.lastChunk.Size && Index < Count)
				{
					d = Math.Min(this.lastChunk.Size - this.lastChunk.Pos, Count - Index);
					Array.Copy(Collection, Index, this.lastChunk.Elements, this.lastChunk.Pos, d);
					Index += d;
					this.count += d;
					this.lastChunk.Pos += d;
				}
			}
		}

		/// <summary>
		/// Copies the contents of the collection to an array.
		/// </summary>
		/// <param name="Destination">Destination array.</param>
		public void CopyTo(T[] Destination)
		{
			this.CopyTo(0, Destination, 0, this.count);
		}

		/// <summary>
		/// Copies the contents of the collection to an array.
		/// </summary>
		/// <param name="Index">Index of the first item to copy.</param>
		/// <param name="Destination">Destination array.</param>
		/// <param name="DestinationIndex">Start index into the array where elements are to be copied.</param>
		/// <param name="Count">Number of elements to copy.</param>
		public void CopyTo(int Index, T[] Destination, int DestinationIndex, int Count)
		{
			Chunk Loop = this.firstChunk;
			int i, c;
			bool Offset = Index > 0;

			while (!(Loop is null) && Count > 0)
			{
				i = Loop.Start;
				if ((c = Loop.Pos - i) > 0)
				{
					if (Offset)
					{
						if (Index >= c)
						{
							Index -= c;
							if (Index == 0)
								Offset = false;
						}
						else
						{
							c -= Index;
							i += Index;
							Offset = false;

							if (Count < c)
								c = Count;

							Array.Copy(Loop.Elements, i, Destination, DestinationIndex, c);
							DestinationIndex += c;
							Count -= c;
						}
					}
					else
					{
						if (Count < c)
							c = Count;

						Array.Copy(Loop.Elements, i, Destination, DestinationIndex, c);
						DestinationIndex += c;
						Count -= c;
					}
				}

				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Returns an array containing all elements of the collection.
		/// </summary>
		/// <returns>Array of elements</returns>
		public T[] ToArray()
		{
			T[] Result = new T[this.count];
			Chunk Loop = this.firstChunk;
			int i = 0, c;

			while (!(Loop is null))
			{
				if ((c = Loop.Pos - Loop.Start) > 0)
				{
					Array.Copy(Loop.Elements, Loop.Start, Result, i, c);
					i += c;
				}

				Loop = Loop.Next;
			}

			return Result;
		}

		private void MakeOneChunk(bool Trimmed)
		{
			if (!(this.firstChunk.Next is null) ||
				(Trimmed && this.firstChunk.Start > 0 || this.firstChunk.Pos < this.firstChunk.Size))
			{
				this.firstChunk = this.lastChunk = new Chunk(this.ToArray());
			}
		}

		/// <summary>
		/// Sorts the collection.
		/// </summary>
		public void Sort()
		{
			// TODO: Can be optimized
			this.MakeOneChunk(false);
			Array.Sort(this.firstChunk.Elements, this.firstChunk.Start,
				this.firstChunk.Pos - this.firstChunk.Start);
		}

		/// <summary>
		/// Sorts the collection.
		/// </summary>
		/// <param name="Comparer">Comparer to use during sort.</param>
		public void Sort(IComparer<T> Comparer)
		{
			// TODO: Can be optimized
			this.MakeOneChunk(false);
			Array.Sort(this.firstChunk.Elements, this.firstChunk.Start,
				this.firstChunk.Pos - this.firstChunk.Start, Comparer);
		}

		/// <summary>
		/// Sorts the collection.
		/// </summary>
		/// <param name="Comparison">Comparisong to use during sort.</param>
		public void Sort(Comparison<T> Comparison)
		{
			// TODO: Can be optimized
			this.MakeOneChunk(true);
			Array.Sort(this.firstChunk.Elements, Comparison);
		}

		/// <summary>
		/// Sorts a part of the collection.
		/// </summary>
		/// <param name="Index">Start index of collection.</param>
		/// <param name="Count">Number of elements to sort.</param>
		/// <param name="Comparer">Comparer to use during sort.</param>
		public void Sort(int Index, int Count, IComparer<T> Comparer)
		{
			// TODO: Can be optimized
			this.MakeOneChunk(false);
			Array.Sort(this.firstChunk.Elements, this.firstChunk.Start + Index, Count, Comparer);
		}

		/// <summary>
		/// Reverses the order of the elements in the collection.
		/// </summary>
		public void Reverse()
		{
			// TODO: Can be optimized
			this.MakeOneChunk(false);
			Array.Reverse(this.firstChunk.Elements, this.firstChunk.Start,
				this.firstChunk.Pos - this.firstChunk.Start);
		}

		/// <summary>
		/// Reverses the order of a subset of elements in the collection.
		/// </summary>
		/// <param name="Index">Index of first element.</param>
		/// <param name="Count">Number of elements.</param>
		public void Reverse(int Index, int Count)
		{
			// TODO: Can be optimized
			this.MakeOneChunk(false);
			Array.Reverse(this.firstChunk.Elements, this.firstChunk.Start + Index, Count);
		}

		#endregion

		#region AddRangeFirst/AddRangeLast

		/// <summary>
		/// Adds a range of elements last to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRangeLast(IEnumerable<T> Collection)
		{
			this.AddRange(Collection);
		}

		/// <summary>
		/// Adds a range of elements last to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRangeLast(T[] Collection)
		{
			this.AddRange(Collection);
		}

		/// <summary>
		/// Adds a range of elements first to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRangeFirst(IEnumerable<T> Collection)
		{
			if (Collection is T[] A)
				this.AddRangeFirst(A);
			else if (Collection is ICollection<T> C)
			{
				A = new T[C.Count];
				C.CopyTo(A, 0);
				this.AddRangeFirst(A);
			}
			else
			{
				IEnumerator<T> e = Collection.GetEnumerator();
				ChunkedList<T> Temp = new ChunkedList<T>();

				while (e.MoveNext())
					Temp.AddLastItem(e.Current);

				this.AddRangeFirst(Temp.ToArray());
			}
		}

		/// <summary>
		/// Adds a range of elements first to the list.
		/// </summary>
		/// <param name="Collection">Collection of elements to add.</param>
		public void AddRangeFirst(T[] Collection)
		{
			int c = Collection.Length;
			int d;

			if ((d = this.firstChunk.Size - this.firstChunk.Pos) > 0)
			{
				Array.Copy(this.firstChunk.Elements, 0, this.firstChunk.Elements, d, this.firstChunk.Pos);
				this.firstChunk.Start += d;
				this.firstChunk.Pos = this.firstChunk.Size;
			}

			if (this.firstChunk.Start > 0)
			{
				d = Math.Min(this.firstChunk.Start, c);

				Array.Copy(Collection, c - d, this.firstChunk.Elements, this.firstChunk.Start - d, d);
				c -= d;
				this.firstChunk.Start -= d;
				this.count += d;
			}

			while (c > 0)
			{
				Chunk Temp = this.firstChunk;
				this.firstChunk = new Chunk(this.chunkSize);
				Temp.Prev = this.firstChunk;
				this.firstChunk.Next = Temp;

				this.chunkSize <<= 1;
				if (this.chunkSize > this.maxChunkSize || this.chunkSize <= 0)
					this.chunkSize = this.maxChunkSize;

				this.firstChunk.Start = this.firstChunk.Pos = this.firstChunk.Size;

				d = Math.Min(this.firstChunk.Start, c);

				Array.Copy(Collection, c - d, this.firstChunk.Elements, this.firstChunk.Start - d, d);
				c -= d;
				this.firstChunk.Start -= d;
				this.count += d;
			}
		}

		#endregion

		#region InsertRange

		/// <summary>
		/// Inserts a range of elements at a given index.
		/// </summary>
		/// <param name="Index">Index to insert the range of elements.</param>
		/// <param name="Collection">Collection of elements.</param>
		public void InsertRange(int Index, IEnumerable<T> Collection)
		{
			// TODO: Can be optimized

			foreach (T Item in Collection)
				this.Insert(Index++, Item);
		}

		#endregion
	}
}
