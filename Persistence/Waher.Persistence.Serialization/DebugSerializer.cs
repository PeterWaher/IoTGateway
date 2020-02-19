using System;
using System.IO;
using System.Text;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Debugs use of a serializer.
	/// </summary>
	public class DebugSerializer : ISerializer
	{
		private readonly ISerializer serializer;
		private readonly TextWriter output;

		/// <summary>
		/// Debugs use of a serializer.
		/// </summary>
		/// <param name="Serializer">Serializer to debug.</param>
		/// <param name="Output">Debug output.</param>
		public DebugSerializer(ISerializer Serializer, TextWriter Output)
		{
			this.serializer = Serializer;
			this.output = Output;
		}

		/// <summary>
		/// Name of current collection.
		/// </summary>
		public string CollectionName => this.serializer.CollectionName;

		/// <summary>
		/// Text encoding to use when serializing strings.
		/// </summary>
		public Encoding Encoding => this.serializer.Encoding;

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(bool Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Bool: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(byte Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Byte: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(short Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Short: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(int Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Int: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(long Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Long: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(sbyte Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("SByte: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(ushort Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("UShort: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(uint Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("UInt: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(ulong Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("ULong: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(decimal Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Decimal: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(double Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Double: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(float Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Single: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(DateTime Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("DateTime: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(DateTimeOffset Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("DateTimeOffset: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(TimeSpan Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("TimeSpan: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(char Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Char: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(Enum Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Enum: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(byte[] Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("Byte[]: " + Convert.ToBase64String(Value));
		}

		/// <summary>
		/// Writes some bytes to the output.
		/// </summary>
		/// <param name="Data">Data to write.</param>
		public void WriteRaw(byte[] Data)
		{
			this.serializer.WriteRaw(Data);
			this.output.WriteLine("Raw: " + System.Convert.ToBase64String(Data));
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(string Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("String: " + Value);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void Write(Guid Value)
		{
			this.serializer.Write(Value);
			this.output.WriteLine("GUID: " + Value);
		}

		/// <summary>
		/// Serializes a variable-length integer value.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteVariableLengthUInt64(ulong Value)
		{
			this.serializer.WriteVariableLengthUInt64(Value);
			this.output.WriteLine("VarUInt: " + Value);
		}

		/// <summary>
		/// Serializes a bit.
		/// </summary>
		/// <param name="Bit">Bit value.</param>
		public void WriteBit(bool Bit)
		{
			this.serializer.WriteBit(Bit);
			this.output.WriteLine("Bit: " + Bit);
		}

		/// <summary>
		/// Serializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="Value">Bit field value.</param>
		/// <param name="NrBits">Number of bits to serialize.</param>
		public void WriteBits(uint Value, int NrBits)
		{
			this.serializer.WriteBits(Value, NrBits);
			this.output.WriteLine("Bits" + NrBits + ": " + Value);
		}

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		public void FlushBits()
		{
			this.serializer.FlushBits();
			this.output.WriteLine("Flush");
		}

		/// <summary>
		/// Current bit-offset.
		/// </summary>
		public int BitOffset => this.serializer.BitOffset;

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		public void Restart()
		{
			this.serializer.Restart();
			this.output.WriteLine("Restart");
		}

		/// <summary>
		/// Gets the binary serialization.
		/// </summary>
		/// <returns>Binary serialization.</returns>
		public byte[] GetSerialization()
		{
			return this.serializer.GetSerialization();
		}

		/// <summary>
		/// Creates a new serializer of the same type and properties.
		/// </summary>
		/// <returns></returns>
		public ISerializer CreateNew()
		{
			this.output.WriteLine("Embedded serializer");
			return new DebugSerializer(this.serializer.CreateNew(), this.output);
		}

	}
}
