using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for serializers.
	/// </summary>
	public interface ISerializer
	{
		/// <summary>
		/// Name of current collection.
		/// </summary>
		string CollectionName
		{
			get;
		}

		/// <summary>
		/// Text encoding to use when serializing strings.
		/// </summary>
		Encoding Encoding
		{
			get;
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(bool Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(byte Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(short Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(int Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(long Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(sbyte Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(ushort Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(uint Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(ulong Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(decimal Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(double Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(float Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(DateTime Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(DateTimeOffset Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(TimeSpan Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(char Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(Enum Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(byte[] Value);

		/// <summary>
		/// Writes some bytes to the output.
		/// </summary>
		/// <param name="Data">Data to write.</param>
		void WriteRaw(byte[] Data);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(string Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Value">Value</param>
		void Write(Guid Value);

		/// <summary>
		/// Serializes a variable-length integer value.
		/// </summary>
		/// <param name="Value">Value</param>
		void WriteVariableLengthUInt64(ulong Value);

		/// <summary>
		/// Serializes a bit.
		/// </summary>
		/// <param name="Bit">Bit value.</param>
		void WriteBit(bool Bit);

		/// <summary>
		/// Serializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="Value">Bit field value.</param>
		/// <param name="NrBits">Number of bits to serialize.</param>
		void WriteBits(uint Value, int NrBits);

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		void FlushBits();

		/// <summary>
		/// Current bit-offset.
		/// </summary>
		int BitOffset
		{
			get;
		}

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		void Restart();

		/// <summary>
		/// Gets the binary serialization.
		/// </summary>
		/// <returns>Binary serialization.</returns>
		byte[] GetSerialization();

		/// <summary>
		/// Creates a new serializer of the same type and properties.
		/// </summary>
		/// <returns></returns>
		ISerializer CreateNew();
	}
}
