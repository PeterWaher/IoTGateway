using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Cluster.Serialization
{
	/// <summary>
	/// Cluster serializer
	/// </summary>
	public class Deserializer : IDisposable
	{
		private MemoryStream ms;

		/// <summary>
		/// Cluster serializer
		/// </summary>
		/// <param name="Data">Binary data</param>
		public Deserializer(byte[] Data)
		{
			this.ms = new MemoryStream(Data);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.ms?.Dispose();
			this.ms = null;
		}

		/// <summary>
		/// Reads a boolean value from the input.
		/// </summary>
		/// <returns>Value</returns>
		public bool ReadBoolean()
		{
			return this.ms.ReadByte() != 0;
		}

		/// <summary>
		/// Reads a byte from the input.
		/// </summary>
		/// <returns>Value</returns>
		public byte ReadByte()
		{
			return (byte)this.ms.ReadByte();
		}

		/// <summary>
		/// Reads a variable-length unsigned integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public ulong ReadVarUInt64()
		{
			ulong Value = 0;
			int Offset = 0;
			byte b;

			do
			{
				b = (byte)this.ms.ReadByte();
				Value |= ((ulong)(b & 0x7f)) << Offset;
				Offset += 7;
			}
			while ((b & 0x80) != 0);

			return Value;
		}

		/// <summary>
		/// Reads binary data from the input.
		/// </summary>
		/// <returns>Value</returns>
		public byte[] ReadBinary()
		{
			ulong Len = this.ReadVarUInt64();

			if (Len == 0)
				return null;

			Len--;
			if (Len > int.MaxValue)
				throw new Exception("Invalid binary serialization.");

			int c = (int)Len;
			byte[] Bin = new byte[c];
			this.ms.Read(Bin, 0, c);

			return Bin;
		}

		/// <summary>
		/// Reads a string from the input.
		/// </summary>
		/// <returns>Value</returns>
		public string ReadString()
		{
			ulong Len = this.ReadVarUInt64();

			switch (Len)
			{
				case 0:
					return null;

				case 1:
					return string.Empty;

				default:
					Len--;
					if (Len > int.MaxValue)
						throw new Exception("Invalid string serialization.");

					int c = (int)Len;
					byte[] Bin = new byte[c];
					this.ms.Read(Bin, 0, c);
					return Encoding.UTF8.GetString(Bin);
			}
		}

		/// <summary>
		/// Reads a 8-bit signed integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public sbyte ReadInt8()
		{
			return (sbyte)this.ms.ReadByte();
		}

		/// <summary>
		/// Reads a 16-bit signed integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public short ReadInt16()
		{
			short Value = (short)this.ms.ReadByte();
			Value |= (short)(this.ms.ReadByte() << 8);

			return Value;
		}

		/// <summary>
		/// Reads a 32-bit signed integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public int ReadInt32()
		{
			int Value = this.ReadUInt16();
			Value |= this.ReadInt16() << 16;

			return Value;
		}

		/// <summary>
		/// Reads a 64-bit signed integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public long ReadInt64()
		{
			long Value = this.ReadUInt32();
			Value |= ((long)this.ReadInt32()) << 32;

			return Value;
		}

		/// <summary>
		/// Reads a 8-bit unsigned integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public byte ReadUInt8()
		{
			return (byte)this.ms.ReadByte();
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public ushort ReadUInt16()
		{
			ushort Value = (ushort)this.ms.ReadByte();
			Value |= (ushort)(this.ms.ReadByte() << 8);

			return Value;
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public uint ReadUInt32()
		{
			uint Value = this.ReadUInt16();
			Value |= (uint)(this.ReadUInt16() << 16);

			return Value;
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer from the input.
		/// </summary>
		/// <returns>Value</returns>
		public ulong ReadUInt64()
		{
			ulong Value = this.ReadUInt32();
			Value |= ((ulong)this.ReadUInt32()) << 32;

			return Value;
		}

		/// <summary>
		/// Reads a single-precision floating point number from the input.
		/// </summary>
		/// <returns>Value</returns>
		public float ReadSingle()
		{
			byte[] Bin = new byte[4];
			this.ms.Read(Bin, 0, 4);
			return BitConverter.ToSingle(Bin, 0);
		}

		/// <summary>
		/// Reads a double-precision floating point number from the input.
		/// </summary>
		/// <returns>Value</returns>
		public double ReadDouble()
		{
			byte[] Bin = new byte[8];
			this.ms.Read(Bin, 0, 8);
			return BitConverter.ToDouble(Bin, 0);
		}

		/// <summary>
		/// Reads a decimal number from the input.
		/// </summary>
		/// <returns>Value</returns>
		public decimal ReadDecimal()
		{
			int[] A = new int[4];
			int i;

			for (i = 0; i < 4; i++)
				A[i] = this.ReadInt32();

			return new decimal(A);
		}

		/// <summary>
		/// Reads a character from the input.
		/// </summary>
		/// <returns>Value</returns>
		public char ReadCharacter()
		{
			return (char)this.ReadUInt16();
		}

		/// <summary>
		/// Reads a <see cref="DateTime"/> from the input.
		/// </summary>
		/// <returns>Value</returns>
		public DateTime ReadDateTime()
		{
			long Ticks = this.ReadInt64();
			DateTimeKind Kind = (DateTimeKind)this.ms.ReadByte();

			return new DateTime(Ticks, Kind);
		}

		/// <summary>
		/// Reads a <see cref="TimeSpan"/> from the input.
		/// </summary>
		/// <returns>Value</returns>
		public TimeSpan ReadTimeSpan()
		{
			long Ticks = this.ReadInt64();
			return new TimeSpan(Ticks);
		}

		/// <summary>
		/// Reads a <see cref="DateTimeOffset"/> from the input.
		/// </summary>
		/// <returns>Value</returns>
		public DateTimeOffset ReadDateTimeOffset()
		{
			DateTime TP = this.ReadDateTime();
			TimeSpan Offset = this.ReadTimeSpan();

			return new DateTimeOffset(TP, Offset);
		}

		/// <summary>
		/// Reads a <see cref="Guid"/> from the input.
		/// </summary>
		/// <returns>Value</returns>
		public Guid ReadGuid()
		{
			byte[] Bin = new byte[16];
			this.ms.Read(Bin, 0, 16);
			return new Guid(Bin);
		}
	}
}
