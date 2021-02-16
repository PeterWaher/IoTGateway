using System;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Serialization bookmark.
	/// </summary>
	public class StreamBookmark
	{
		private readonly int pos;
		private readonly byte bitOffset;

		/// <summary>
		/// Serialization bookmark.
		/// </summary>
		/// <param name="Pos">Current position.</param>
		/// <param name="BitOffset">Current bit offset.</param>
		internal StreamBookmark(int Pos, byte BitOffset)
		{
			this.pos = Pos;
			this.bitOffset = BitOffset;
		}

		/// <summary>
		/// Position.
		/// </summary>
		public int Position
		{
			get { return this.pos; }
		}

		/// <summary>
		/// Bit offset
		/// </summary>
		public byte BitOffset
		{
			get { return this.bitOffset; }
		}
	}
}
