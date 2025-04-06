using System.Text;

namespace Waher.Runtime.Collections
{
	/// <summary>
	/// ChunkedList extensions.
	/// </summary>
	public static class ChunkedListExtensions
	{
		/// <summary>
		/// Appends the elements of a chunked list with string elements, to a string builder.
		/// </summary>
		/// <param name="sb">Elements will be appended to this string builder.</param>
		/// <param name="List">List of string elements.</param>
		public static void Append(this StringBuilder sb, ChunkedList<string> List)
		{
			sb.Append(List, null);
		}

		/// <summary>
		/// Appends the elements of a chunked list with string elements, to a string builder.
		/// </summary>
		/// <param name="sb">Elements will be appended to this string builder.</param>
		/// <param name="List">List of string elements.</param>
		/// <param name="Delimiter">Optional delimiter.</param>
		public static void Append(this StringBuilder sb, ChunkedList<string> List,
			string Delimiter)
		{
			ChunkNode<string> Loop = List.FirstChunk;
			bool HasDelimiter = !string.IsNullOrEmpty(Delimiter);
			int i, c;
			bool First = true;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					if (HasDelimiter)
					{
						if (First)
							First = false;
						else
							sb.Append(Delimiter);
					}

					sb.Append(Loop[i]);
				}

				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Concatenates the string elements of a chunked list.
		/// </summary>
		/// <param name="List">List of string elements.</param>
		/// <returns>Concatenation of elements.</returns>
		public static string Concatenate(this ChunkedList<string> List)
		{
			return List.Concatenate(null);
		}

		/// <summary>
		/// Concatenates the string elements of a chunked list.
		/// </summary>
		/// <param name="List">List of string elements.</param>
		/// <param name="Delimiter">Optional delimiter.</param>
		/// <returns>Concatenation of elements.</returns>
		public static string Concatenate(this ChunkedList<string> List, string Delimiter)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(List, Delimiter);
			return sb.ToString();
		}
	}
}
