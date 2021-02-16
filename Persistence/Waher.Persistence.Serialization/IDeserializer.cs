using System;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for deserializers.
	/// </summary>
	public interface IDeserializer
	{
		/// <summary>
		/// Name of current collection.
		/// </summary>
		string CollectionName
		{
			get;
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		bool ReadBoolean();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		byte ReadByte();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		short ReadInt16();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		int ReadInt32();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		long ReadInt64();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		sbyte ReadSByte();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		ushort ReadUInt16();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		uint ReadUInt32();

		/// <summary>
		/// Reads a block link
		/// </summary>
		/// <returns>Deserialized value.</returns>
		uint ReadBlockLink();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		ulong ReadUInt64();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		decimal ReadDecimal();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		double ReadDouble();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		float ReadSingle();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		DateTime ReadDateTime();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		DateTimeOffset ReadDateTimeOffset();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		TimeSpan ReadTimeSpan();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		char ReadChar();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="EnumType">Type of enum to read.</param>
		/// <returns>Deserialized value.</returns>
		Enum ReadEnum(Type EnumType);

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		byte[] ReadByteArray();

		/// <summary>
		/// Deserializes raw bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		/// <returns>Deserialized value.</returns>
		byte[] ReadRaw(int NrBytes);

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		string ReadString();

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		Guid ReadGuid();

		/// <summary>
		/// Deserializes a variable-length integer value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		ulong ReadVariableLengthUInt64();

		/// <summary>
		/// Deserializes a bit.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		bool ReadBit();

		/// <summary>
		/// Deserializes a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="NrBits">Number of bits to deserialize.</param>
		/// <returns>Deserialized value.</returns>
		uint ReadBits(int NrBits);

		/// <summary>
		/// Flushes any bit field values.
		/// </summary>
		void FlushBits();

		/// <summary>
		/// Current position.
		/// </summary>
		int Position
		{
			get;
			set;
		}

		/// <summary>
		/// Current bit-offset.
		/// </summary>
		int BitOffset
		{
			get;
		}

		/// <summary>
		/// Binary data being parsed.
		/// </summary>
		byte[] Data
		{
			get;
		}

		/// <summary>
		/// Number of bytes left to read.
		/// </summary>
		int BytesLeft
		{
			get;
		}

		/// <summary>
		/// Resets the serializer, allowing for the serialization of another object using the same resources.
		/// </summary>
		void Restart(byte[] Data, int StartPosition);

		/// <summary>
		/// Gets a bookmark of the current position.
		/// </summary>
		/// <returns>Bookmark</returns>
		StreamBookmark GetBookmark();

		/// <summary>
		/// Sets the current position to the position contained in a bookmark.
		/// </summary>
		/// <param name="Bookmark">Bookmark</param>
		void SetBookmark(StreamBookmark Bookmark);

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipBoolean();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipByte();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipInt16();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipInt32();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipInt64();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipSByte();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipUInt16();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipUInt32();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipUInt64();

		/// <summary>
		/// Skips a block link.
		/// </summary>
		void SkipBlockLink();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipDecimal();

		/// <summary>
		/// Skips a value.
		/// </summary>
		/// <returns>Deserialized value.</returns>
		void SkipDouble();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipSingle();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipDateTime();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipDateTimeOffset();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipTimeSpan();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipChar();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipEnum();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipByteArray();

		/// <summary>
		/// Skips raw bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		void SkipRaw(int NrBytes);

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipString();

		/// <summary>
		/// Skips a value.
		/// </summary>
		void SkipGuid();

		/// <summary>
		/// Skips a variable-length integer value.
		/// </summary>
		void SkipVariableLengthUInt64();

		/// <summary>
		/// Skips a bit.
		/// </summary>
		void SkipBit();

		/// <summary>
		/// Skips a value consisting of a fixed number of bits.
		/// </summary>
		/// <param name="NrBits">Number of bits to deserialize.</param>
		void SkipBits(int NrBits);

	}
}
