using System.Diagnostics;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// Displays a <see cref="ChunkedList{T}"/> is a debugger view.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	internal class ChunkedListDebugView<T>
	{
		private readonly ChunkedList<T> list;

		/// <summary>
		/// Displays a <see cref="ChunkedList{T}"/> is a debugger view.
		/// </summary>
		/// <param name="List">Collection</param>
		public ChunkedListDebugView(ChunkedList<T> List)
		{
			this.list = List;
		}

		/// <summary>
		/// Items to display (up to 200 items).
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				int c = this.list.Count;
				if (c > 200)
					c = 200;

				T[] Result = new T[c];
				this.list.CopyTo(0, Result, 0, c);

				return Result;
			}
		}
	}
}
