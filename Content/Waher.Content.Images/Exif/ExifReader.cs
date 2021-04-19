using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// EXIF reader
	/// </summary>
	public class ExifReader
	{
		private readonly byte[] data;
		private readonly int len;
		private int pos;
		private bool bigEndian = true;

		/// <summary>
		/// Current position
		/// </summary>
		public int Position
		{
			get => this.pos;
			set
			{
				if (value < 0 || value >= this.len)
					throw new ArgumentOutOfRangeException(nameof(Position), "Position out of range.");

				this.pos = value;
			}
		}

		/// <summary>
		/// If Big-Endian encoding is used.
		/// </summary>
		public bool BigEndian
		{
			get => this.bigEndian;
			set => this.bigEndian = value;
		}

		/// <summary>
		/// Length of data block
		/// </summary>
		public int Length => this.len;

		/// <summary>
		/// End of File
		/// </summary>
		public bool EoF => this.pos >= this.len;

		/// <summary>
		/// EXIF reader
		/// </summary>
		/// <param name="Data">Binary representation.</param>
		public ExifReader(byte[] Data)
		{
			this.data = Data;
			this.len = Data.Length;
			this.pos = 0;
		}

		/// <summary>
		/// Gets next byte. If no more bytes are available, -1 is returned.
		/// </summary>
		/// <returns>Next byte, or -1 if none.</returns>
		public int NextByte()
		{
			if (this.pos < this.len)
				return this.data[this.pos++];
			else
				return -1;
		}

		/// <summary>
		/// Gets next SHORT (unsigned short). If no more bytes are available, -1 is returned.
		/// </summary>
		/// <returns>Next SHORT, or -1 if none.</returns>
		public int NextSHORT()
		{
			int i = this.NextByte();
			if (i < 0)
				return -1;

			int j = this.NextByte();
			if (j < 0)
				return -1;

			if (this.bigEndian)
			{
				i <<= 8;
				i |= j;

				return i;
			}
			else
			{
				j <<= 8;
				j |= i;

				return j;
			}
		}

		/// <summary>
		/// Gets next LONG (unsigned int). If no more bytes are available, null is returned.
		/// </summary>
		/// <returns>Next LONG, or null if none.</returns>
		public uint? NextLONG()
		{
			int i = this.NextSHORT();
			if (i < 0)
				return null;

			int j = this.NextSHORT();
			if (j < 0)
				return null;

			if (this.bigEndian)
			{
				i <<= 16;
				i |= (ushort)j;

				return (uint)i;
			}
			else
			{
				j <<= 16;
				j |= (ushort)i;

				return (uint)j;
			}
		}

		/// <summary>
		/// Gets the next ASCII string.
		/// </summary>
		/// <returns>ASCII string.</returns>
		public string NextASCIIString()
		{
			int Start = this.pos;
			while (this.pos < this.len && this.data[this.pos] != 0)
				this.pos++;

			string Result = Encoding.ASCII.GetString(this.data, Start, this.pos - Start);
			this.pos++;

			return Result;
		}
	}
}
