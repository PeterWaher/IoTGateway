using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Cluster.Serialization
{
	/// <summary>
	/// Cluster serializer
	/// </summary>
	public class Serializer : IDisposable
	{
		private MemoryStream ms;

		/// <summary>
		/// Cluster serializer
		/// </summary>
		public Serializer()
		{
			this.ms = new MemoryStream();
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
		/// Returns the binary output.
		/// </summary>
		/// <returns>Binary output.</returns>
		public byte[] ToArray()
		{
			return this.ms.ToArray();
		}

		/// <summary>
		/// Writes a boolean value to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteBoolean(bool Value)
		{
			this.ms.WriteByte(Value ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Writes a byte to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteByte(byte Value)
		{
			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Writes a variable-length unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteVarUInt64(ulong Value)
		{
			byte b;

			do
			{
				b = (byte)(Value & 0x7f);
				Value >>= 7;
				if (Value != 0)
					b |= 0x80;

				this.ms.WriteByte(b);
			}
			while (Value != 0);
		}

		/// <summary>
		/// Writes binary data to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteBinary(byte[] Value)
		{
			if (Value is null)
				this.ms.WriteByte(0);
			else
			{
				this.WriteVarUInt64((ulong)Value.Length + 1);
				this.ms.Write(Value, 0, Value.Length);
			}
		}

		/// <summary>
		/// Writes a string to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteString(string Value)
		{
			if (Value is null)
				this.ms.WriteByte(0);
			else if (string.IsNullOrEmpty(Value))
				this.ms.WriteByte(1);
			else
			{
				byte[] Bin = Encoding.UTF8.GetBytes(Value);

				this.WriteVarUInt64((ulong)Bin.Length + 1);
				this.ms.Write(Bin, 0, Bin.Length);
			}
		}

		/// <summary>
		/// Writes a 8-bit signed integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteInt8(sbyte Value)
		{
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a 16-bit signed integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteInt16(short Value)
		{
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a 32-bit signed integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteInt32(int Value)
		{
			byte[] Bin = new byte[4];
			int i;

			for (i = 0; i < 4; i++)
			{
				Bin[i] = (byte)Value;
				Value >>= 8;
			}

			this.ms.Write(Bin, 0, 4);
		}

		/// <summary>
		/// Writes a 64-bit signed integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteInt64(long Value)
		{
			byte[] Bin = new byte[8];
			int i;

			for (i = 0; i < 8; i++)
			{
				Bin[i] = (byte)Value;
				Value >>= 8;
			}

			this.ms.Write(Bin, 0, 8);
		}

		/// <summary>
		/// Writes a 8-bit unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteUInt8(byte Value)
		{
			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Writes a 16-bit unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteUInt16(ushort Value)
		{
			this.ms.WriteByte((byte)Value);
			Value >>= 8;
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a 32-bit unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteUInt32(uint Value)
		{
			byte[] Bin = new byte[4];
			int i;

			for (i = 0; i < 4; i++)
			{
				Bin[i] = (byte)Value;
				Value >>= 8;
			}

			this.ms.Write(Bin, 0, 4);
		}

		/// <summary>
		/// Writes a 64-bit unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteUInt64(ulong Value)
		{
			byte[] Bin = new byte[8];
			int i;

			for (i = 0; i < 8; i++)
			{
				Bin[i] = (byte)Value;
				Value >>= 8;
			}

			this.ms.Write(Bin, 0, 8);
		}

		/// <summary>
		/// Writes a single-precision floating point number to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteSingle(float Value)
		{
			byte[] Bin = BitConverter.GetBytes(Value);
			this.ms.Write(Bin, 0, Bin.Length);
		}

		/// <summary>
		/// Writes a double-precision floating point number to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteDouble(double Value)
		{
			byte[] Bin = BitConverter.GetBytes(Value);
			this.ms.Write(Bin, 0, Bin.Length);
		}

		/// <summary>
		/// Writes a decimal number to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteDecimal(decimal Value)
		{
			int[] A = decimal.GetBits(Value);
			int i;

			for (i = 0; i < 4; i++)
				this.WriteInt32(A[i]);
		}

		/// <summary>
		/// Writes a character to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteCharacter(char Value)
		{
			this.WriteUInt16(Value);
		}

		/// <summary>
		/// Writes a <see cref="DateTime"/> to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteDateTime(DateTime Value)
		{
			this.WriteInt64(Value.Ticks);
			this.ms.WriteByte((byte)Value.Kind);
		}

		/// <summary>
		/// Writes a <see cref="TimeSpan"/> to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteTimeSpan(TimeSpan Value)
		{
			this.WriteInt64(Value.Ticks);
		}

		/// <summary>
		/// Writes a <see cref="DateTimeOffset"/> to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteDateTimeOffset(DateTimeOffset Value)
		{
			this.WriteDateTime(Value.DateTime);
			this.WriteTimeSpan(Value.Offset);
		}

		/// <summary>
		/// Writes a <see cref="Guid"/> to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteGuid(Guid Value)
		{
			byte[] Bin = Value.ToByteArray();
			this.ms.Write(Bin, 0, Bin.Length);
		}
	}
}
