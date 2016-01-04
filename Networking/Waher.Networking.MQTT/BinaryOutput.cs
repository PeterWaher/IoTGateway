using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Class that helps serialize information into a a binary packet.
	/// </summary>
	public class BinaryOutput
	{
		private MemoryStream ms;

		/// <summary>
		/// Class that helps serialize information into a a binary packet.
		/// </summary>
		public BinaryOutput()
		{
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Class that helps serialize information into a a binary packet.
		/// </summary>
		/// <param name="Data">Binary Data</param>
		public BinaryOutput(byte[] Data)
		{
			this.ms = new MemoryStream(Data);
			this.ms.Position = Data.Length;
		}

		/// <summary>
		/// Class that helps serialize information into a a binary packet.
		/// </summary>
		/// <param name="Data">Binary Data</param>
		public BinaryOutput(MemoryStream Data)
		{
			this.ms = Data;
		}

		/// <summary>
		/// Writes a byte to the binary output packet.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteByte(byte Value)
		{
			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Writes a block of bytes to the binary output packet.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteBytes(byte[] Value)
		{
			this.ms.Write(Value, 0, Value.Length);
		}

		/// <summary>
		/// Writes a string to the binary output packet.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteString(string Value)
		{
			Value = Value.Normalize();

			byte[] Data = Encoding.UTF8.GetBytes(Value);
			int Length = Data.Length;
			if (Length > 65535)
				throw new ArgumentException("String too long.", "Value");

			this.WriteByte((byte)(Length >> 8));
			this.WriteByte((byte)Length);
			this.WriteBytes(Data);
		}

		/// <summary>
		/// Gets the binary packet written so far.
		/// </summary>
		/// <returns>Binary packet.</returns>
		public byte[] GetPacket()
		{
			this.ms.Flush();
			this.ms.Capacity = (int)this.ms.Position;
			return this.ms.GetBuffer();
		}

		/// <summary>
		/// Writes a variable-length unsigned integer.
		/// </summary>
		/// <param name="Value">Value to write.</param>
#pragma warning disable
		public void WriteUInt(ulong Value)
#pragma warning enable
		{
			while (Value >= 128)
			{
				this.ms.WriteByte((byte)(Value | 0x80));
				Value >>= 7;
			}

			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a variable-length signed integer.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteInt(long Value)
		{
			if (Value >= 0)
				this.WriteUInt((uint)Value << 1);
			else
				this.WriteUInt(((uint)(-Value) << 1) | 1);
		}

		/// <summary>
		/// Writes a 16-bit integer to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteUInt16(ushort Value)
		{
			this.ms.WriteByte((byte)(Value >> 8));
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a 32-bit integer to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteUInt32(uint Value)
		{
			for (int i = 0; i < 4; i++)
			{
				this.ms.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		/// <summary>
		/// Writes a 64-bit integer to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteUInt64(ulong Value)
		{
			for (int i = 0; i < 8; i++)
			{
				this.ms.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		/// <summary>
		/// Writes a single-precision floating point to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteSingle(float f)
		{
			this.WriteBytes(BitConverter.GetBytes(f));
		}

		/// <summary>
		/// Writes a double-precision floating point to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteDouble(double d)
		{
			this.WriteBytes(BitConverter.GetBytes(d));
		}

		/// <summary>
		/// Writes a TimeSpan to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteTimeSpan(TimeSpan Value)
		{
			this.WriteDouble(Value.TotalMilliseconds);
		}

		/// <summary>
		/// Writes a DateTime to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteDateTime(DateTime Value)
		{
			this.WriteDouble(Value.ToOADate());
		}

		/// <summary>
		/// Writes a Color to the stream.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public void WriteColor(Color Color)
		{
			this.WriteUInt32((uint)Color.ToArgb());
		}

		/// <summary>
		/// Writes a GUID to the stream.
		/// </summary>
		/// <param name="Guid">Value to write.</param>
		public void WriteGuid(Guid Guid)
		{
			this.WriteBytes(Guid.ToByteArray());
		}

		/// <summary>
		/// Writes a boolean value to the stream.
		/// </summary>
		/// <param name="b">Value to write.</param>
		public void WriteBool(bool b)
		{
			this.WriteByte((byte)(b ? 1 : 0));
		}

	}
}
