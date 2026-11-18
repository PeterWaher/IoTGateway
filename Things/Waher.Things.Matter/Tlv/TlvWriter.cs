using System;
using System.IO;
using System.Text;

namespace Waher.Things.Matter.Tlv
{
	/// <summary>
	/// Encodes data using the Matter TLV format (Matter Core Specification, Appendix A).
	/// Integers are encoded using the shortest possible representation.
	/// </summary>
	public class TlvWriter
	{
		private readonly MemoryStream output;
		private int depth = 0;

		/// <summary>
		/// Encodes data using the Matter TLV format.
		/// </summary>
		public TlvWriter()
		{
			this.output = new MemoryStream();
		}

		/// <summary>
		/// Current container depth.
		/// </summary>
		public int Depth => this.depth;

		/// <summary>
		/// Number of bytes written.
		/// </summary>
		public int Length => (int)this.output.Length;

		/// <summary>
		/// Returns the encoded bytes.
		/// </summary>
		/// <returns>Encoded TLV.</returns>
		public byte[] ToArray()
		{
			if (this.depth != 0)
				throw new InvalidOperationException("Unclosed TLV containers.");

			return this.output.ToArray();
		}

		private void WriteControl(TlvTag Tag, TlvElementType Type)
		{
			this.output.WriteByte((byte)(((byte)Tag.Control << 5) | (byte)Type));

			switch (Tag.Control)
			{
				case TlvTagControl.Anonymous:
					break;

				case TlvTagControl.ContextSpecific:
					this.output.WriteByte((byte)Tag.Number);
					break;

				case TlvTagControl.CommonProfile2:
				case TlvTagControl.ImplicitProfile2:
					this.WriteLittleEndian(Tag.Number, 2);
					break;

				case TlvTagControl.CommonProfile4:
				case TlvTagControl.ImplicitProfile4:
					this.WriteLittleEndian(Tag.Number, 4);
					break;

				case TlvTagControl.FullyQualified6:
					this.WriteLittleEndian(Tag.VendorId, 2);
					this.WriteLittleEndian(Tag.ProfileNumber, 2);
					this.WriteLittleEndian(Tag.Number, 2);
					break;

				case TlvTagControl.FullyQualified8:
					this.WriteLittleEndian(Tag.VendorId, 2);
					this.WriteLittleEndian(Tag.ProfileNumber, 2);
					this.WriteLittleEndian(Tag.Number, 4);
					break;
			}
		}

		private void WriteLittleEndian(ulong Value, int NrBytes)
		{
			while (NrBytes-- > 0)
			{
				this.output.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		/// <summary>
		/// Writes a signed integer, using the shortest possible encoding.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteInt(TlvTag Tag, long Value)
		{
			if (Value >= sbyte.MinValue && Value <= sbyte.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.SignedInt8);
				this.WriteLittleEndian((ulong)Value, 1);
			}
			else if (Value >= short.MinValue && Value <= short.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.SignedInt16);
				this.WriteLittleEndian((ulong)Value, 2);
			}
			else if (Value >= int.MinValue && Value <= int.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.SignedInt32);
				this.WriteLittleEndian((ulong)Value, 4);
			}
			else
			{
				this.WriteControl(Tag, TlvElementType.SignedInt64);
				this.WriteLittleEndian((ulong)Value, 8);
			}
		}

		/// <summary>
		/// Writes an unsigned integer, using the shortest possible encoding.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteUInt(TlvTag Tag, ulong Value)
		{
			if (Value <= byte.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.UnsignedInt8);
				this.WriteLittleEndian(Value, 1);
			}
			else if (Value <= ushort.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.UnsignedInt16);
				this.WriteLittleEndian(Value, 2);
			}
			else if (Value <= uint.MaxValue)
			{
				this.WriteControl(Tag, TlvElementType.UnsignedInt32);
				this.WriteLittleEndian(Value, 4);
			}
			else
			{
				this.WriteControl(Tag, TlvElementType.UnsignedInt64);
				this.WriteLittleEndian(Value, 8);
			}
		}

		/// <summary>
		/// Writes an unsigned integer using a fixed number of octets.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		/// <param name="NrBytes">Number of octets (1, 2, 4 or 8).</param>
		public void WriteUIntFixed(TlvTag Tag, ulong Value, int NrBytes)
		{
			switch (NrBytes)
			{
				case 1: this.WriteControl(Tag, TlvElementType.UnsignedInt8); break;
				case 2: this.WriteControl(Tag, TlvElementType.UnsignedInt16); break;
				case 4: this.WriteControl(Tag, TlvElementType.UnsignedInt32); break;
				case 8: this.WriteControl(Tag, TlvElementType.UnsignedInt64); break;
				default: throw new ArgumentOutOfRangeException(nameof(NrBytes));
			}

			this.WriteLittleEndian(Value, NrBytes);
		}

		/// <summary>
		/// Writes a boolean value.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteBoolean(TlvTag Tag, bool Value)
		{
			this.WriteControl(Tag, Value ? TlvElementType.BooleanTrue : TlvElementType.BooleanFalse);
		}

		/// <summary>
		/// Writes a single precision floating point value.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteFloat(TlvTag Tag, float Value)
		{
			this.WriteControl(Tag, TlvElementType.Float);
			byte[] Bin = BitConverter.GetBytes(Value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(Bin);
			this.output.Write(Bin, 0, 4);
		}

		/// <summary>
		/// Writes a double precision floating point value.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteDouble(TlvTag Tag, double Value)
		{
			this.WriteControl(Tag, TlvElementType.Double);
			byte[] Bin = BitConverter.GetBytes(Value);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(Bin);
			this.output.Write(Bin, 0, 8);
		}

		/// <summary>
		/// Writes a UTF-8 string.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteString(TlvTag Tag, string Value)
		{
			byte[] Bin = Encoding.UTF8.GetBytes(Value ?? string.Empty);
			this.WriteLengthPrefixed(Tag, TlvElementType.Utf8String1, Bin);
		}

		/// <summary>
		/// Writes an octet string.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteBytes(TlvTag Tag, byte[] Value)
		{
			this.WriteLengthPrefixed(Tag, TlvElementType.ByteString1, Value ?? Array.Empty<byte>());
		}

		private void WriteLengthPrefixed(TlvTag Tag, TlvElementType BaseType, byte[] Bin)
		{
			int c = Bin.Length;

			if (c <= byte.MaxValue)
			{
				this.WriteControl(Tag, BaseType);
				this.WriteLittleEndian((ulong)c, 1);
			}
			else if (c <= ushort.MaxValue)
			{
				this.WriteControl(Tag, BaseType + 1);
				this.WriteLittleEndian((ulong)c, 2);
			}
			else
			{
				this.WriteControl(Tag, BaseType + 2);
				this.WriteLittleEndian((ulong)c, 4);
			}

			this.output.Write(Bin, 0, c);
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		public void WriteNull(TlvTag Tag)
		{
			this.WriteControl(Tag, TlvElementType.Null);
		}

		/// <summary>
		/// Starts a structure.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		public void StartStructure(TlvTag Tag)
		{
			this.WriteControl(Tag, TlvElementType.Structure);
			this.depth++;
		}

		/// <summary>
		/// Starts an anonymous structure.
		/// </summary>
		public void StartStructure()
		{
			this.StartStructure(TlvTag.Anonymous);
		}

		/// <summary>
		/// Starts an array.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		public void StartArray(TlvTag Tag)
		{
			this.WriteControl(Tag, TlvElementType.Array);
			this.depth++;
		}

		/// <summary>
		/// Starts a list.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		public void StartList(TlvTag Tag)
		{
			this.WriteControl(Tag, TlvElementType.List);
			this.depth++;
		}

		/// <summary>
		/// Ends the current container.
		/// </summary>
		public void EndContainer()
		{
			if (this.depth <= 0)
				throw new InvalidOperationException("No open TLV container.");

			this.output.WriteByte((byte)TlvElementType.EndOfContainer);
			this.depth--;
		}

		/// <summary>
		/// Writes pre-encoded TLV data.
		/// </summary>
		/// <param name="Encoded">Encoded TLV element(s).</param>
		public void WriteRaw(byte[] Encoded)
		{
			this.output.Write(Encoded, 0, Encoded.Length);
		}

		/// <summary>
		/// Writes a pre-encoded TLV element using a new tag.
		/// </summary>
		/// <param name="Tag">Tag to use.</param>
		/// <param name="Encoded">Encoded TLV element (any tag in the encoding is replaced).</param>
		public void WriteRetagged(TlvTag Tag, byte[] Encoded)
		{
			TlvElement Element = TlvElement.Decode(Encoded);
			Element.Tag = Tag;
			Element.Encode(this);
		}

		/// <summary>
		/// Writes a value of a generic .NET type, choosing a suitable TLV encoding.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		public void WriteObject(TlvTag Tag, object Value)
		{
			switch (Value)
			{
				case null:
					this.WriteNull(Tag);
					break;

				case TlvElement Element:
					TlvTag Prev = Element.Tag;
					Element.Tag = Tag;
					try
					{
						Element.Encode(this);
					}
					finally
					{
						Element.Tag = Prev;
					}
					break;

				case bool b: this.WriteBoolean(Tag, b); break;
				case byte ui8: this.WriteUInt(Tag, ui8); break;
				case ushort ui16: this.WriteUInt(Tag, ui16); break;
				case uint ui32: this.WriteUInt(Tag, ui32); break;
				case ulong ui64: this.WriteUInt(Tag, ui64); break;
				case sbyte i8: this.WriteInt(Tag, i8); break;
				case short i16: this.WriteInt(Tag, i16); break;
				case int i32: this.WriteInt(Tag, i32); break;
				case long i64: this.WriteInt(Tag, i64); break;
				case float f: this.WriteFloat(Tag, f); break;
				case double d: this.WriteDouble(Tag, d); break;
				case string s: this.WriteString(Tag, s); break;
				case byte[] Bin: this.WriteBytes(Tag, Bin); break;

				case Enum e:
					Type T = Enum.GetUnderlyingType(e.GetType());
					if (T == typeof(long) || T == typeof(int) || T == typeof(short) || T == typeof(sbyte))
						this.WriteInt(Tag, Convert.ToInt64(e));
					else
						this.WriteUInt(Tag, Convert.ToUInt64(e));
					break;

				default:
					throw new ArgumentException("Unable to encode values of type " + Value.GetType().FullName + " as TLV.", nameof(Value));
			}
		}
	}
}
