using System;
using System.IO;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Debugs use of a deserializer.
	/// </summary>
	public class DebugDeserializer : IDeserializer
	{
		private readonly IDeserializer deserializer;
		private readonly TextWriter output;

		/// <summary>
		/// Debugs use of a deserializer.
		/// </summary>
		/// <param name="Deserializer">Deserializer to debug.</param>
		/// <param name="Output">Debug output.</param>
		public DebugDeserializer(IDeserializer Deserializer, TextWriter Output)
		{
			this.deserializer = Deserializer;
			this.output = Output;
		}

		/// <summary>
		/// Name of current collection.
		/// </summary>
		public string CollectionName => this.deserializer.CollectionName;

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public bool ReadBoolean()
		{
			bool Result = this.deserializer.ReadBoolean();
			this.output.WriteLine("Bool: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public byte ReadByte()
		{
			byte Result = this.deserializer.ReadByte();
			this.output.WriteLine("Byte: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public short ReadInt16()
		{
			short Result = this.deserializer.ReadInt16();
			this.output.WriteLine("Short: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public int ReadInt32()
		{
			int Result = this.deserializer.ReadInt32();
			this.output.WriteLine("Int: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public long ReadInt64()
		{
			long Result = this.deserializer.ReadInt64();
			this.output.WriteLine("Long: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public sbyte ReadSByte()
		{
			sbyte Result = this.deserializer.ReadSByte();
			this.output.WriteLine("SByte: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ushort ReadUInt16()
		{
			ushort Result = this.deserializer.ReadUInt16();
			this.output.WriteLine("UShort: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public uint ReadUInt32()
		{
			uint Result = this.deserializer.ReadUInt32();
			this.output.WriteLine("UInt: " + Result);
			return Result;
		}

		/// <summary>
		/// Reads a block link
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public uint ReadBlockLink()
		{
			uint Result = this.deserializer.ReadBlockLink();
			this.output.WriteLine("BlockLink: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ulong ReadUInt64()
		{
			ulong Result = this.deserializer.ReadUInt64();
			this.output.WriteLine("ULong: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public decimal ReadDecimal()
		{
			decimal Result = this.deserializer.ReadDecimal();
			this.output.WriteLine("Decimal: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public double ReadDouble()
		{
			double Result = this.deserializer.ReadDouble();
			this.output.WriteLine("Double: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public float ReadSingle()
		{
			float Result = this.deserializer.ReadSingle();
			this.output.WriteLine("Single: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public DateTime ReadDateTime()
		{
			DateTime Result = this.deserializer.ReadDateTime();
			this.output.WriteLine("DateTime: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public DateTimeOffset ReadDateTimeOffset()
		{
			DateTimeOffset Result = this.deserializer.ReadDateTimeOffset();
			this.output.WriteLine("DateTimeOffset: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public TimeSpan ReadTimeSpan()
		{
			TimeSpan Result = this.deserializer.ReadTimeSpan();
			this.output.WriteLine("TimeSpan: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public char ReadChar()
		{
			char Result = this.deserializer.ReadChar();
			this.output.WriteLine("Char: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="EnumType">Type of enum to read.</param>
		/// <returns>Deserialized value.</returns>
		public Enum ReadEnum(Type EnumType)
		{
			Enum Result = this.deserializer.ReadEnum(EnumType);
			this.output.WriteLine("Enum: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public byte[] ReadByteArray()
		{
			byte[] Result = this.deserializer.ReadByteArray();
			this.output.WriteLine("Byte[]: " + Convert.ToBase64String(Result));
			return Result;
		}

		/// <summary>
		/// Deserializes raw bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		/// <returns>Deserialized value.</returns>
		public byte[] ReadRaw(int NrBytes)
		{
			byte[] Result = this.deserializer.ReadRaw(NrBytes);
			this.output.WriteLine("Raw: " + Convert.ToBase64String(Result));
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public string ReadString()
		{
			string Result = this.deserializer.ReadString();
			this.output.WriteLine("String: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public Guid ReadGuid()
		{
			Guid Result = this.deserializer.ReadGuid();
			this.output.WriteLine("GUID: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a variable-length integer value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public ulong ReadVariableLengthUInt64()
		{
			ulong Result = this.deserializer.ReadVariableLengthUInt64();
			this.output.WriteLine("VarUInt: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a bit.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public bool ReadBit()
		{
			bool Result = this.deserializer.ReadBit();
			this.output.WriteLine("Bit: " + Result);
			return Result;
		}

		/// <summary>
		/// Deserializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="NrBits">Number of bits to deserialize.</param>
		/// <returns>Deserialized value.</returns>
		public uint ReadBits(int NrBits)
		{
			uint Result = this.deserializer.ReadBits(NrBits);
			this.output.WriteLine("Bits" + NrBits + ": " + Result);
			return Result;
		}

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		public void FlushBits()
		{
			this.deserializer.FlushBits();
			this.output.WriteLine("Flush");
		}

		/// <summary>
		/// Current position.
		/// </summary>
		public int Position
		{
			get => this.deserializer.Position;
			set => this.deserializer.Position = value;
		}

		/// <summary>
		/// Current bit-offset.
		/// </summary>
		public int BitOffset => this.deserializer.BitOffset;

		/// <summary>
		/// Binary data being parsed.
		/// </summary>
		public byte[] Data => this.deserializer.Data;

		/// <summary>
		/// Number of bytes left to read.
		/// </summary>
		public int BytesLeft => this.deserializer.BytesLeft;

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		public void Restart(byte[] Data, int StartPosition)
		{
			this.deserializer.Restart(Data, StartPosition);
			this.output.WriteLine("Restart");
		}

		/// <summary>
		/// Gets a bookmark of the current position.
		/// </summary>
		/// <returns>Bookmark</returns>
		public StreamBookmark GetBookmark()
		{
			return this.deserializer.GetBookmark();
		}

		/// <summary>
		/// Sets the current position to the position contained in a bookmark.
		/// </summary>
		/// <param name="Bookmark">Bookmark</param>
		public void SetBookmark(StreamBookmark Bookmark)
		{
			this.deserializer.SetBookmark(Bookmark);
			this.output.WriteLine("Setting bookmark");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipBoolean()
		{
			this.deserializer.SkipBoolean();
			this.output.WriteLine("Skip Bool");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipByte()
		{
			this.deserializer.SkipByte();
			this.output.WriteLine("Skip Byte");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipInt16()
		{
			this.deserializer.SkipInt16();
			this.output.WriteLine("Skip Short");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipInt32()
		{
			this.deserializer.SkipInt32();
			this.output.WriteLine("Skip Int");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipInt64()
		{
			this.deserializer.SkipInt64();
			this.output.WriteLine("Skip Long");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipSByte()
		{
			this.deserializer.SkipSByte();
			this.output.WriteLine("Skip SByte");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipUInt16()
		{
			this.deserializer.SkipUInt16();
			this.output.WriteLine("Skip UShort");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipUInt32()
		{
			this.deserializer.SkipUInt32();
			this.output.WriteLine("Skip UInt");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipUInt64()
		{
			this.deserializer.SkipUInt64();
			this.output.WriteLine("Skip ULong");
		}

		/// <summary>
		/// Skips a block link.
		/// </summary>
		public void SkipBlockLink()
		{
			this.deserializer.SkipBlockLink();
			this.output.WriteLine("Skip BlockLink");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipDecimal()
		{
			this.deserializer.SkipDecimal();
			this.output.WriteLine("Skip Decimal");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		public void SkipDouble()
		{
			this.deserializer.SkipDouble();
			this.output.WriteLine("Skip Double");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipSingle()
		{
			this.deserializer.SkipSingle();
			this.output.WriteLine("Skip Single");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipDateTime()
		{
			this.deserializer.SkipDateTime();
			this.output.WriteLine("Skip DateTime");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipDateTimeOffset()
		{
			this.deserializer.SkipDateTimeOffset();
			this.output.WriteLine("Skip DateTimeOffset");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipTimeSpan()
		{
			this.deserializer.SkipTimeSpan();
			this.output.WriteLine("Skip TimeSpan");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipChar()
		{
			this.deserializer.SkipChar();
			this.output.WriteLine("Skip Char");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipEnum()
		{
			this.deserializer.SkipEnum();
			this.output.WriteLine("Skip Enum");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipByteArray()
		{
			this.deserializer.SkipByteArray();
			this.output.WriteLine("Skip Byte[]");
		}

		/// <summary>
		/// Skips raw bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		public void SkipRaw(int NrBytes)
		{
			this.deserializer.SkipRaw(NrBytes);
			this.output.WriteLine("Skip Raw");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipString()
		{
			this.deserializer.SkipString();
			this.output.WriteLine("Skip String");
		}

		/// <summary>
		/// Skips a value.
		/// </summary>
		public void SkipGuid()
		{
			this.deserializer.SkipGuid();
			this.output.WriteLine("Skip GUID");
		}

		/// <summary>
		/// Skips a variable-length integer value.
		/// </summary>
		public void SkipVariableLengthUInt64()
		{
			this.deserializer.SkipVariableLengthUInt64();
			this.output.WriteLine("Skip VarUInt");
		}

		/// <summary>
		/// Skips a bit.
		/// </summary>
		public void SkipBit()
		{
			this.deserializer.SkipBit();
			this.output.WriteLine("Skip Bit");
		}

		/// <summary>
		/// Skips a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="NrBits">Number of bits to deserialize.</param>
		public void SkipBits(int NrBits)
		{
			this.deserializer.SkipBits(NrBits);
			this.output.WriteLine("Skip Bits" + NrBits);
		}

	}
}
