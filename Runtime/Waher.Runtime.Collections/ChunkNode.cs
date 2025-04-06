using System.Diagnostics;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// Node referencing a chunk in a <see cref="ChunkedList{T}"/>
	/// </summary>
	[DebuggerDisplay("Count = {Count}, Start = {Start}, Pos = {Pos}, Size = {Size}")]
	[DebuggerTypeProxy(typeof(ChunkNodeDebugView<>))]
	public class ChunkNode<T>
	{
		private readonly ChunkedList<T>.Chunk chunk;

		/// <summary>
		/// Node referencing a chunk in a <see cref="ChunkedList{T}"/>
		/// </summary>
		/// <param name="Chunk">Chunk</param>
		internal ChunkNode(ChunkedList<T>.Chunk Chunk)
		{
			this.chunk = Chunk;
		}

		/// <summary>
		/// Next chunk
		/// </summary>
		public ChunkNode<T> Next => this.chunk.Next?.Node;

		/// <summary>
		/// Previous chunk
		/// </summary>
		public ChunkNode<T> Prev => this.chunk.Prev?.Node;

		/// <summary>
		/// Array of elements in chunk.
		/// </summary>
		public T[] Elements => this.chunk.Elements;

		/// <summary>
		/// Size of chunk.
		/// </summary>
		public int Size => this.chunk.Size;

		/// <summary>
		/// Index of first element in chunk.
		/// </summary>
		public int Start => this.chunk.Start;

		/// <summary>
		/// Index after the last element in chunk.
		/// </summary>
		public int Pos => this.chunk.Pos;

		/// <summary>
		/// Number of elements in chunk.
		/// </summary>
		public int Count => this.chunk.Pos - this.chunk.Start;

		/// <summary>
		/// String representation of chunk.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.chunk.ToString();
		}

		/// <summary>
		/// Access directly into the chunk. Valid indices are from <see cref="Start"/>
		/// to <see cref="Pos"/>-1.
		/// </summary>
		/// <param name="Index"></param>
		/// <returns></returns>
		public T this[int Index]
		{
			get => this.chunk[Index];
			set => this.chunk[Index] = value;
		}
	}
}
