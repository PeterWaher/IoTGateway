using System;
using System.Diagnostics;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// Displays a <see cref="ChunkNode{T}"/> is a debugger view.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	internal class ChunkNodeDebugView<T>
	{
		private readonly ChunkNode<T> node;

		/// <summary>
		/// Displays a <see cref="ChunkNode{T}"/> is a debugger view.
		/// </summary>
		/// <param name="Node">Chunk node</param>
		public ChunkNodeDebugView(ChunkNode<T> Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Items to display (up to 200 items).
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				int c = this.node.Pos - this.node.Start;
				if (c > 200)
					c = 200;

				T[] Result = new T[c];
				Array.Copy(this.node.Elements, this.node.Start, Result, 0, c);

				return Result;
			}
		}
	}
}
