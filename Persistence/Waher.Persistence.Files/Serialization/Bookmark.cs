using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Serialization bookmark.
	/// </summary>
	public class Bookmark
	{
		private int pos;
		private byte bitOffset;

		/// <summary>
		/// Serialization bookmark.
		/// </summary>
		/// <param name="Pos">Current position.</param>
		/// <param name="BitOffset">Current bit offset.</param>
		internal Bookmark(int Pos, byte BitOffset)
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
