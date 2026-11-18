namespace Waher.Things.Matter.Tlv
{
	/// <summary>
	/// TLV element types, as encoded in the lower five bits of the control octet.
	/// </summary>
	public enum TlvElementType : byte
	{
		/// <summary>
		/// Signed integer, 1 octet.
		/// </summary>
		SignedInt8 = 0x00,

		/// <summary>
		/// Signed integer, 2 octets.
		/// </summary>
		SignedInt16 = 0x01,

		/// <summary>
		/// Signed integer, 4 octets.
		/// </summary>
		SignedInt32 = 0x02,

		/// <summary>
		/// Signed integer, 8 octets.
		/// </summary>
		SignedInt64 = 0x03,

		/// <summary>
		/// Unsigned integer, 1 octet.
		/// </summary>
		UnsignedInt8 = 0x04,

		/// <summary>
		/// Unsigned integer, 2 octets.
		/// </summary>
		UnsignedInt16 = 0x05,

		/// <summary>
		/// Unsigned integer, 4 octets.
		/// </summary>
		UnsignedInt32 = 0x06,

		/// <summary>
		/// Unsigned integer, 8 octets.
		/// </summary>
		UnsignedInt64 = 0x07,

		/// <summary>
		/// Boolean false.
		/// </summary>
		BooleanFalse = 0x08,

		/// <summary>
		/// Boolean true.
		/// </summary>
		BooleanTrue = 0x09,

		/// <summary>
		/// Single precision floating point.
		/// </summary>
		Float = 0x0a,

		/// <summary>
		/// Double precision floating point.
		/// </summary>
		Double = 0x0b,

		/// <summary>
		/// UTF-8 string, 1-octet length.
		/// </summary>
		Utf8String1 = 0x0c,

		/// <summary>
		/// UTF-8 string, 2-octet length.
		/// </summary>
		Utf8String2 = 0x0d,

		/// <summary>
		/// UTF-8 string, 4-octet length.
		/// </summary>
		Utf8String4 = 0x0e,

		/// <summary>
		/// UTF-8 string, 8-octet length.
		/// </summary>
		Utf8String8 = 0x0f,

		/// <summary>
		/// Octet string, 1-octet length.
		/// </summary>
		ByteString1 = 0x10,

		/// <summary>
		/// Octet string, 2-octet length.
		/// </summary>
		ByteString2 = 0x11,

		/// <summary>
		/// Octet string, 4-octet length.
		/// </summary>
		ByteString4 = 0x12,

		/// <summary>
		/// Octet string, 8-octet length.
		/// </summary>
		ByteString8 = 0x13,

		/// <summary>
		/// Null value.
		/// </summary>
		Null = 0x14,

		/// <summary>
		/// Structure.
		/// </summary>
		Structure = 0x15,

		/// <summary>
		/// Array.
		/// </summary>
		Array = 0x16,

		/// <summary>
		/// List.
		/// </summary>
		List = 0x17,

		/// <summary>
		/// End of container.
		/// </summary>
		EndOfContainer = 0x18
	}

	/// <summary>
	/// Logical value types of decoded TLV elements.
	/// </summary>
	public enum TlvValueType
	{
		/// <summary>
		/// Signed integer (stored as <see cref="long"/>).
		/// </summary>
		SignedInteger,

		/// <summary>
		/// Unsigned integer (stored as <see cref="ulong"/>).
		/// </summary>
		UnsignedInteger,

		/// <summary>
		/// Boolean (stored as <see cref="bool"/>).
		/// </summary>
		Boolean,

		/// <summary>
		/// Single precision float (stored as <see cref="float"/>).
		/// </summary>
		Float,

		/// <summary>
		/// Double precision float (stored as <see cref="double"/>).
		/// </summary>
		Double,

		/// <summary>
		/// UTF-8 string (stored as <see cref="string"/>).
		/// </summary>
		String,

		/// <summary>
		/// Octet string (stored as byte array).
		/// </summary>
		ByteString,

		/// <summary>
		/// Null value.
		/// </summary>
		Null,

		/// <summary>
		/// Structure (container with uniquely tagged members).
		/// </summary>
		Structure,

		/// <summary>
		/// Array (container with anonymous members).
		/// </summary>
		Array,

		/// <summary>
		/// List (ordered container, possibly with tagged members).
		/// </summary>
		List
	}
}
