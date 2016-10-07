using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Manages binary deserialization of data.
	/// </summary>
	public class BinaryDeserializer
	{
		private Encoding encoding;
		private byte[] data;
		private int pos;
		private byte bitOffset = 0;

		/// <summary>
		/// Manages binary deserialization of data.
		/// </summary>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="Data">Binary data to deserialize.</param>
		/// <param name="StartPosition">Starting position.</param>
		public BinaryDeserializer(Encoding Encoding, byte[] Data)
			: this(Encoding, Data, 0)
		{
		}

		/// <summary>
		/// Manages binary deserialization of data.
		/// </summary>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="Data">Binary data to deserialize.</param>
		/// <param name="StartPosition">Starting position.</param>
		public BinaryDeserializer(Encoding Encoding, byte[] Data, int StartPosition)
		{
			this.encoding = Encoding;
			this.data = Data;
			this.pos = StartPosition;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public bool ReadBoolean()
		{
			return this.ReadBit();
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public byte ReadByte()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			return this.data[this.pos++];
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public short ReadInt16()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			ushort Result = this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 2;

			return (short)Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public int ReadInt32()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			uint Result = this.data[this.pos + 3];
			Result <<= 8;
			Result |= this.data[this.pos + 2];
			Result <<= 8;
			Result |= this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 4;

			return (int)Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public long ReadInt64()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			ulong Result = this.data[this.pos + 7];
			Result <<= 8;
			Result |= this.data[this.pos + 6];
			Result <<= 8;
			Result |= this.data[this.pos + 5];
			Result <<= 8;
			Result |= this.data[this.pos + 4];
			Result <<= 8;
			Result |= this.data[this.pos + 3];
			Result <<= 8;
			Result |= this.data[this.pos + 2];
			Result <<= 8;
			Result |= this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 8;

			return (long)Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public sbyte ReadSByte()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			return (sbyte)this.data[this.pos++];
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ushort ReadUInt16()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			ushort Result = this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 2;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public uint ReadUInt32()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			uint Result = this.data[this.pos + 3];
			Result <<= 8;
			Result |= this.data[this.pos + 2];
			Result <<= 8;
			Result |= this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 4;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ulong ReadUInt64()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			ulong Result = this.data[this.pos + 7];
			Result <<= 8;
			Result |= this.data[this.pos + 6];
			Result <<= 8;
			Result |= this.data[this.pos + 5];
			Result <<= 8;
			Result |= this.data[this.pos + 4];
			Result <<= 8;
			Result |= this.data[this.pos + 3];
			Result <<= 8;
			Result |= this.data[this.pos + 2];
			Result <<= 8;
			Result |= this.data[this.pos + 1];
			Result <<= 8;
			Result |= this.data[this.pos];

			this.pos += 8;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public decimal ReadDecimal()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int[] A = new int[4];
			int i;

			for (i = 0; i < 4; i++)
				A[i] = this.ReadInt32();

			return new decimal(A);
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public double ReadDouble()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			double Result = BitConverter.ToDouble(this.data, this.pos);

			this.pos += 8;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public float ReadSingle()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			float Result = BitConverter.ToSingle(this.data, this.pos);

			this.pos += 4;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public DateTime ReadDateTime()
		{
			return new DateTime(this.ReadInt64());
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(this.ReadInt64());
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public char ReadChar()
		{
			return (char)this.ReadUInt16();
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="EnumType">Type of enum to read.</param>
		/// <returns>Deserialized value.</returns>
		public Enum ReadEnum(Type EnumType)
		{
			string s = this.ReadString();
			return (Enum)Enum.Parse(EnumType, s);
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public byte[] ReadByteArray()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int c = (int)this.ReadVariableLengthUInt64();
			byte[] Result = new byte[c];

			Array.Copy(this.data, this.pos, Result, 0, c);
			this.pos += c;

			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public string ReadString()
		{
			int c = (int)this.ReadVariableLengthUInt64();
			string s = this.encoding.GetString(this.data, this.pos, c);

			this.pos += c;

			return s;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public Guid ReadGuid()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			byte[] Data = new byte[16];
			Array.Copy(this.data, this.pos, Data, 0, 16);
			this.pos += 16;

			Guid Result = new Guid(Data);

			return Result;
		}

		/// <summary>
		/// Deserializes a variable-length integer value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ulong ReadVariableLengthUInt64()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			byte b = this.data[this.pos++];
			ulong Result = (byte)(b & 0x7f);
			int Offset = 0;

			while ((b & 0x80) != 0)
			{
				Offset += 7;
				b = this.data[this.pos++];
				Result |= (ulong)(((long)(b & 0x7f)) << Offset);
			}

			return Result;
		}

		/// <summary>
		/// Deserializes a bit.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public bool ReadBit()
		{
			bool Result = ((this.data[this.pos] >> this.bitOffset) & 1) != 0;

			this.bitOffset++;
			if (this.bitOffset == 8)
			{
				this.pos++;
				this.bitOffset = 0;
			}

			return Result;
		}

		/// <summary>
		/// Deserializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="NrBits">Number of bits to deserialize.</param>
		/// <returns>Deserialized value.</returns>
		public uint ReadBits(int NrBits)
		{
			uint Result = 0;
			int Offset = 0;
			int c;
			byte b;

			while (NrBits > 0)
			{
				c = Math.Min(NrBits, 8 - this.bitOffset);
				b = (byte)(this.data[this.pos] >> this.bitOffset);

				this.bitOffset += (byte)c;
				NrBits -= c;

				if (this.bitOffset == 8)
				{
					Result |= (uint)(b << Offset);
					this.bitOffset = 0;
					this.pos++;
				}
				else
				{
					b &= (byte)(0xff >> (8 - c));
					Result |= (uint)(b << Offset);
				}

				Offset += c;
			}

			return Result;
		}

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		public void FlushBits()
		{
			if (this.bitOffset > 0)
			{
				this.pos++;
				this.bitOffset = 0;
			}
		}

		/// <summary>
		/// Current position.
		/// </summary>
		public int Position
		{
			get { return this.pos; }
		}

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		public void Restart(byte[] Data, int StartPosition)
		{
			this.data = Data;
			this.pos = StartPosition;
			this.bitOffset = 0;
		}

		/// <summary>
		/// Gets a bookmark of the current position.
		/// </summary>
		/// <returns>Bookmark</returns>
		public Bookmark GetBookmark()
		{
			return new Bookmark(this.pos, this.bitOffset);
		}

		/// <summary>
		/// Sets the current position to the position contained in a bookmark.
		/// </summary>
		/// <param name="Bookmark">Bookmark</param>
		public void SetBookmark(Bookmark Bookmark)
		{
			this.pos = Bookmark.Position;
			this.bitOffset = Bookmark.BitOffset;
		}

	}
}
