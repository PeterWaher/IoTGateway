using System;
using System.IO;
using System.Text;
using Waher.Runtime.IO;

namespace Waher.Networking
{
	/// <summary>
	/// Class that helps deserialize information stored in a binary packet.
	/// </summary>
	public class BinaryInput
	{
		private readonly MemoryStream ms;

		/// <summary>
		/// Class that helps deserialize information stored in a binary packet.
		/// </summary>
		/// <param name="Data">Binary Data</param>
		public BinaryInput(byte[] Data)
		{
			this.ms = new MemoryStream(Data);
		}

		/// <summary>
		/// Class that helps deserialize information stored in a binary packet.
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="Offset">Offset into buffer where data starts.</param>
		/// <param name="Count">Number of bytes of data.</param>
		public BinaryInput(byte[] Data, int Offset, int Count)
		{
			this.ms = new MemoryStream(Data, Offset, Count);
		}

		/// <summary>
		/// Class that helps deserialize information stored in a binary packet.
		/// </summary>
		/// <param name="Data">Binary Data</param>
		public BinaryInput(MemoryStream Data)
		{
			this.ms = Data;
			this.ms.Position = 0;
		}

		/// <summary>
		/// Reads the next byte of the stream.
		/// </summary>
		/// <returns>Next byte</returns>
		/// <exception cref="EndOfStreamException">If there are no more bytes available.</exception>
		public byte ReadByte()
		{
			int i = this.ms.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			return (byte)i;
		}

		/// <summary>
		/// Reads the next set of bytes of the stream.
		/// </summary>
		/// <param name="Length">Number of bytes to retrieve.</param>
		/// <returns>Binary block of data.</returns>
		/// <exception cref="EndOfStreamException">If there is not sufficient bytes available.</exception>
		public byte[] ReadRaw(int Length)
		{
			byte[] Result = new byte[Length];
			this.ms.ReadAll(Result, 0, Length);

			return Result;
		}

		/// <summary>
		/// Reads a variable-length binary block of data.
		/// </summary>
		/// <exception cref="EndOfStreamException">If there is not sufficient bytes available.</exception>
		/// <exception cref="IOException">If the length field is invalid.</exception>
		public byte[] ReadData()
		{
			ulong Len = this.ReadVarLenUInt();
			if (Len == 0)
				return Array.Empty<byte>();

			if (Len < 0 || Len > int.MaxValue)
				throw new IOException("Invalid length of binary block of data.");

			return this.ReadRaw((int)Len);
		}

		/// <summary>
		/// Reads the next string of the stream, using a 16-bit length field, and
		/// UTF-8 encoding.
		/// </summary>
		/// <returns>String value.</returns>
		/// <exception cref="EndOfStreamException">If there is not sufficient bytes available.</exception>
		public string ReadString16BitLen()
		{
			int Len = this.ReadByte();
			Len <<= 8;
			Len |= this.ReadByte();

			if (Len == 0)
				return string.Empty;

			byte[] Data = this.ReadRaw(Len);

			return Encoding.UTF8.GetString(Data);
		}

		/// <summary>
		/// Reads the next string of the stream, using a variable-length field, and
		/// UTF-8 encoding.
		/// </summary>
		/// <returns>String value.</returns>
		/// <exception cref="EndOfStreamException">If there is not sufficient bytes available.</exception>
		/// <exception cref="IOException">If the length field is invalid.</exception>
		public string ReadString()
		{
			ulong Len = this.ReadVarLenUInt();
			if (Len == 0)
				return string.Empty;

			if (Len < 0 || Len > int.MaxValue)
				throw new IOException("Invalid length of string.");

			byte[] Data = this.ReadRaw((int)Len);

			return Encoding.UTF8.GetString(Data);
		}

		/// <summary>
		/// Reads a variable-length unsigned integer from the stream.
		/// </summary>
		/// <returns>Unsigned integer.</returns>
		public ulong ReadVarLenUInt()
		{
			byte b = this.ReadByte();
			int Offset = 0;
			uint Result = (uint)(b & 127);

			while ((b & 128) != 0)
			{
				b = this.ReadByte();
				Offset += 7;
				Result |= (uint)((b & 127) << Offset);
			}

			return Result;
		}

		/// <summary>
		/// Reads a variable-length signed integer from the stream.
		/// </summary>
		/// <returns>Signed integer.</returns>
		public long ReadVarLenInt()
		{
			ulong l = this.ReadVarLenUInt();

			if ((l & 1) == 0)
				return (long)(l >> 1);
			else
				return -(long)(l >> 1);
		}

		/// <summary>
		/// Reads an unsignd 16-bit integer.
		/// </summary>
		/// <returns>16-bit integer.</returns>
		public ushort ReadUInt16()
		{
			ushort Result = this.ReadByte();
			Result <<= 8;
			Result |= this.ReadByte();

			return Result;
		}

		/// <summary>
		/// Reads an unsignd 16-bit integer.
		/// </summary>
		/// <returns>32-bit integer.</returns>
		public uint ReadUInt32()
		{
			int Offset = 0;
			uint Result = 0;
			int i;

			for (i = 0; i < 4; i++)
			{
				Result |= (uint)(this.ReadByte() << Offset);
				Offset += 8;
			}

			return Result;
		}

		/// <summary>
		/// Reads an unsignd 64-bit integer.
		/// </summary>
		/// <returns>64-bit integer.</returns>
		public ulong ReadUInt64()
		{
			int Offset = 0;
			ulong Result = 0;
			int i;

			for (i = 0; i < 8; i++)
			{
				Result |= ((ulong)this.ReadByte() << Offset);
				Offset += 8;
			}

			return Result;
		}

		/// <summary>
		/// Reads a single-precision floating point value.
		/// </summary>
		/// <returns>Single-precision floating point value</returns>
		public float ReadSingle()
		{
			return BitConverter.ToSingle(this.ReadRaw(4), 0);
		}

		/// <summary>
		/// Reads a double-precision floating point value.
		/// </summary>
		/// <returns>Double-precision floating point value</returns>
		public double ReadDouble()
		{
			return BitConverter.ToDouble(this.ReadRaw(8), 0);
		}

		/// <summary>
		/// Reads a TimeSpan value.
		/// </summary>
		/// <returns>TimeSpan.</returns>
		public TimeSpan ReadTimeSpan()
		{
			return TimeSpan.FromMilliseconds(this.ReadDouble());
		}

		/// <summary>
		/// Reads a DateTime value.
		/// </summary>
		/// <returns>DateTime.</returns>
		public DateTime ReadDateTime()
		{
			return BinaryOutput.unixEpoch.AddSeconds(this.ReadDouble()).ToLocalTime();
		}

		/// <summary>
		/// Current position in input stream.
		/// </summary>
		public int Position => (int)this.ms.Position;

		/// <summary>
		/// Number of bytes left.
		/// </summary>
		public int BytesLeft => (int)(this.ms.Length - this.ms.Position);

		/// <summary>
		/// Gets the remaining bytes.
		/// </summary>
		/// <returns>Remaining bytes.</returns>
		public byte[] GetRemainingData()
		{
			int c = this.BytesLeft;
			byte[] Result = new byte[c];

			if (c > 0)
				this.ms.Read(Result, 0, c);

			return Result;
		}

		/// <summary>
		/// Reads a GUID value.
		/// </summary>
		/// <returns>GUID.</returns>
		public Guid ReadGuid()
		{
			byte[] Bin = this.ReadRaw(16);
			return new Guid(Bin);
		}

		/// <summary>
		/// Reads a boolean value.
		/// </summary>
		/// <returns>Boolean value.</returns>
		public bool ReadBool()
		{
			return this.ReadByte() != 0;
		}
	}
}
