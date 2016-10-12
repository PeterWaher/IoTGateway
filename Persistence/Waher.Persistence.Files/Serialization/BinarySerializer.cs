using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Manages binary serialization of data.
	/// 
	/// Note: The serializer is not thread safe.
	/// </summary>
	public class BinarySerializer
	{
		private Encoding encoding;
		private MemoryStream ms;
		private byte bits = 0;
		private byte bitOffset = 0;

		/// <summary>
		/// Manages binary serialization of data.
		/// 
		/// Note: The serializer is not thread safe.
		/// </summary>
		/// <param name="Encoding">Encoding to use for text.</param>
		public BinarySerializer(Encoding Encoding)
		{
			this.encoding = Encoding;
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Manages binary serialization of data.
		/// 
		/// Note: The serializer is not thread safe.
		/// </summary>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="Output">Data will be output to this stream.</param>
		public BinarySerializer(Encoding Encoding, MemoryStream Output)
		{
			this.encoding = Encoding;
			this.ms = Output;
			this.ms.Position = 0;
		}

		/// <summary>
		/// Text encoding to use when serializing strings.
		/// </summary>
		public Encoding Encoding
		{
			get { return this.encoding; }
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(bool Value)
		{
			this.WriteBit(Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(byte Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(short Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(int Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(long Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int i;

			for (i = 0; i < 8; i++)
			{
				this.ms.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(sbyte Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(ushort Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(uint Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(ulong Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int i;

			for (i = 0; i < 8; i++)
			{
				this.ms.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(decimal Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int[] A = decimal.GetBits(Value);
			int i;

			for (i = 0; i < 4; i++)
				this.ms.Write(BitConverter.GetBytes(A[i]), 0, 4);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(double Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.Write(BitConverter.GetBytes(Value), 0, 8);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(float Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.Write(BitConverter.GetBytes(Value), 0, 4);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(DateTime Value)
		{
			this.Write(Value.Ticks);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(TimeSpan Value)
		{
			this.Write(Value.Ticks);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(char Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			this.ms.Write(BitConverter.GetBytes(Value), 0, 2);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(Enum Value)
		{
			this.Write(Value.ToString());
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(byte[] Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int c = Value.Length;

			this.WriteVariableLengthUInt64((uint)c);
			this.ms.Write(Value, 0, c);
		}

		/// <summary>
		/// Writes some bytes to the output.
		/// </summary>
		/// <param name="Data">Data to write.</param>
		public void WriteRaw(byte[] Data)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int c = Data.Length;

			this.ms.Write(Data, 0, c);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(string Value)
		{
			this.Write(this.encoding.GetBytes(Value));
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(Guid Value)
		{
			this.ms.Write(Value.ToByteArray(), 0, 16);
		}

		/// <summary>
		/// Serializes a variable-length integer value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteVariableLengthUInt64(ulong Value)
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			byte b;

			do
			{
				b = (byte)(Value & 0x7f);
				Value >>= 7;
				if (Value > 0)
					b |= 0x80;

				this.ms.WriteByte(b);
			}
			while (Value > 0);
		}

		/// <summary>
		/// Serializes a bit.
		/// </summary>
		/// <param name="Bit">Bit value.</param>
		public void WriteBit(bool Bit)
		{
			if (Bit)
				this.bits |= (byte)(1 << this.bitOffset);

			this.bitOffset++;
			if (this.bitOffset == 8)
			{
				this.ms.WriteByte(this.bits);
				this.bits = 0;
				this.bitOffset = 0;
			}
		}

		/// <summary>
		/// Serializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="Value">Bit field value.</param>
		/// <param name="NrBits">Number of bits to serialize.</param>
		public void WriteBits(uint Value, int NrBits)
		{
			int c;

			while (NrBits > 0)
			{
				this.bits |= (byte)(Value << this.bitOffset);

				c = Math.Min(NrBits, 8 - this.bitOffset);
				this.bitOffset += (byte)c;
				NrBits -= c;
				Value >>= c;

				if (this.bitOffset == 8)
				{
					this.ms.WriteByte(this.bits);
					this.bits = 0;
					this.bitOffset = 0;
				}
			}

			if (Value != 0)
				throw new ArgumentException("Value does not fit in the given number of bits.", "Value");
		}

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		public void FlushBits()
		{
			if (this.bitOffset > 0)
			{
				this.ms.WriteByte(this.bits);
				this.bits = 0;
				this.bitOffset = 0;
			}
		}

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		public void Restart()
		{
			this.ms.Position = 0;
			this.bits = 0;
			this.bitOffset = 0;
		}

		/// <summary>
		/// Gets the binary serialization.
		/// </summary>
		/// <returns>Binary serialization.</returns>
		public byte[] GetSerialization()
		{
			if (this.bitOffset > 0)
				this.FlushBits();

			int c = (int)this.ms.Position;
			ArraySegment<byte> Buffer;
			byte[] Data;

			if (this.ms.TryGetBuffer(out Buffer))
				Data = Buffer.Array;
			else
				Data = this.ms.ToArray();

			if (Data.Length > c)
			{
				byte[] Data2 = new byte[c];
				Array.Copy(Data, 0, Data2, 0, c);
				Data = Data2;
			}

			return Data;
		}

	}
}
