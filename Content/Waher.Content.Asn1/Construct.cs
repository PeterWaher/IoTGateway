using System;
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Text;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Delegate for reading items in arrays.
	/// </summary>
	/// <typeparam name="T">Type of item to read.</typeparam>
	/// <param name="Reader">ASN.1 reader object.</param>
	/// <param name="Item">Item read, if successful.</param>
	/// <returns>If able to read item.</returns>
	public delegate bool TryReadItem<T>(AsnReader Reader, out T Item);

	/// <summary>
	/// Delegate for reading items in arrays.
	/// </summary>
	/// <typeparam name="T">Type of item to read.</typeparam>
	/// <param name="Reader">ASN.1 reader object.</param>
	/// <param name="Item">Item read, if successful.</param>
	/// <returns>If able to read item.</returns>
	public delegate bool TryReadNullableItem<T>(AsnReader Reader, out T? Item)
		where T : struct;

	/// <summary>
	/// Abstract base class for ASN.1 constructs
	/// </summary>
	public abstract class Construct
	{
		#region Encoding/Decoding

		/// <summary>
		/// Decodes the object from an ASN.1 encoding.
		/// </summary>
		/// <param name="Reader">ASN.1 reader.</param>
		public abstract void Read(AsnReader Reader);

		/// <summary>
		/// Encodes the object to an ASN.1 encoding.
		/// </summary>
		/// <param name="Writer">ASN.1 writer.</param>
		public abstract void Write(AsnWriter Writer);

		#endregion

		#region Tags

		/// <summary>
		/// Converts an ASN.1 tag to an integer.
		/// </summary>
		/// <param name="Tag">ASN.1 Tag</param>
		/// <returns>Integer</returns>
		public static int ToInt32(Asn1Tag Tag)
		{
			int Result = (int)Tag.TagClass;
			Result |= Tag.TagValue;
			if (Tag.IsConstructed)
				Result |= 0x20;
			return Result;
		}

		#endregion

		#region Data Types

		#region Any

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAny(AsnReader Reader, object Default, out object Result)
		{
			if (!Reader.HasData)
			{
				Result = Default;
				return false;
			}

			return TryReadAny(Reader, Reader.PeekTag(), out Result);
		}

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ExpectedTag">Expected tag.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAny(AsnReader Reader, Asn1Tag ExpectedTag,
			object Default, out object Result)
		{
			if (ExpectedTag.TagClass == TagClass.Universal)
			{
				switch (ExpectedTag.TagValue)
				{
					case (int)UniversalTagNumber.EndOfContents:
						Result = Default;
						return false;

					case (int)UniversalTagNumber.Boolean:
						Result = Reader.ReadBoolean(ExpectedTag);
						return true;

					case (int)UniversalTagNumber.Integer:
					case (int)UniversalTagNumber.Enumerated:
						Result = Reader.ReadInteger(ExpectedTag);
						return true;

					case (int)UniversalTagNumber.BitString:
						Result = Reader.ReadBitString(out _, ExpectedTag);
						return true;

					case (int)UniversalTagNumber.OctetString:
						Result = Reader.ReadOctetString(ExpectedTag);
						return true;

					case (int)UniversalTagNumber.Null:
						Reader.ReadNull(ExpectedTag);
						Result = Default;
						return true;

					case (int)UniversalTagNumber.ObjectIdentifier:
						Result = new ObjectId(Reader.ReadObjectIdentifier(ExpectedTag));
						return true;

					case (int)UniversalTagNumber.ObjectDescriptor:  // Obsolete
					case (int)UniversalTagNumber.UTF8String:
					case (int)UniversalTagNumber.NumericString:
					case (int)UniversalTagNumber.PrintableString:
					case (int)UniversalTagNumber.TeletexString:     // Same as UniversalTagNumber.T61String:
					case (int)UniversalTagNumber.VideotexString:
					case (int)UniversalTagNumber.IA5String:
					case (int)UniversalTagNumber.GraphicString:
					case (int)UniversalTagNumber.VisibleString:     // Same as UniversalTagNumber.ISO646String:
					case (int)UniversalTagNumber.GeneralString:
					case (int)UniversalTagNumber.UniversalString:
					case (int)UniversalTagNumber.UnrestrictedCharacterString:
					case (int)UniversalTagNumber.BMPString:
						Result = Reader.ReadCharacterString((UniversalTagNumber)ExpectedTag.TagValue, ExpectedTag);
						return true;

					case (int)UniversalTagNumber.Real:
					case (int)UniversalTagNumber.RelativeObjectIdentifier:
					case (int)UniversalTagNumber.Time:
					case (int)UniversalTagNumber.Date:
					case (int)UniversalTagNumber.TimeOfDay:
					case (int)UniversalTagNumber.DateTime:
					case (int)UniversalTagNumber.Duration:
					case (int)UniversalTagNumber.ObjectIdentifierIRI:
					case (int)UniversalTagNumber.RelativeObjectIdentifierIRI:
						Result = Reader.ReadEncodedValue().ToArray();   // TODO
						return true;

					case (int)UniversalTagNumber.Sequence:          // Same as UniversalTagNumber.SequenceOf:
					case (int)UniversalTagNumber.External:          // Same as UniversalTagNumber.InstanceOf:
					case (int)UniversalTagNumber.Set:               // Same as UniversalTagNumber.SetOf:
					case (int)UniversalTagNumber.Embedded:

						ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();
						AsnReader Inner = new AsnReader(Section, Reader.RuleSet,
							new AsnReaderOptions()
							{
								SkipSetSortOrderVerification = true
							});

						if (ExpectedTag.TagValue == (int)UniversalTagNumber.Sequence)
							Inner = Inner.ReadSequence(ExpectedTag);
						else
							Inner = Inner.ReadSetOf(ExpectedTag);

						ChunkedList<object> Elements = new ChunkedList<object>();

						while (Inner.HasData && TryReadAny(Inner, null, out object Element))
							Elements.Add(Element);

						Result = Elements.ToArray();
						return true;

					case (int)UniversalTagNumber.UtcTime:
						Result = Reader.ReadUtcTime(ExpectedTag);
						return true;

					case (int)UniversalTagNumber.GeneralizedTime:
						Result = Reader.ReadGeneralizedTime(ExpectedTag);
						return true;

					default:
						Result = Default;
						return false;
				}
			}
			else if (ExpectedTag.TagClass == TagClass.ContextSpecific)
			{
				ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();

				if (ExpectedTag.TagClass == TagClass.ContextSpecific && ExpectedTag.IsConstructed)  // EXPLICIT
				{
					AsnReader Inner = new AsnReader(Section, Reader.RuleSet);
					return TryReadAny(Inner, Default, out Result);
				}
				else
				{
					Result = Section.ToArray();
					return true;
				}
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyAutomatic(AsnReader Reader, int Index,
			object Default, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadAnyExplicit(Reader, Index, Default, out Result);
				else
					return TryReadAnyImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadAny(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyImplicit(AsnReader Reader, int Index,
			object Default, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			if (TryReadAny(Reader, new Asn1Tag(TagClass.ContextSpecific, Index), out Result))
				return true;
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyExplicit(AsnReader Reader, int Index,
			object Default, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadAny(Inner, Default, out Result), Inner);
		}

		private static bool CheckComplete(bool Result, AsnReader Reader)
		{
			if (Result && Reader.HasData)
				throw new AsnContentException("Unexpected inner content found.");

			return Result;
		}

		private static T CheckComplete<T>(T Result, AsnReader Reader)
		{
			if (Reader.HasData)
				throw new AsnContentException("Unexpected inner content found.");

			return Result;
		}

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static object ReadAny(AsnReader Reader)
		{
			if (TryReadAny(Reader, null, out object Result))
				return Result;
			else
				return null;
		}

		/// <summary>
		/// Reads a any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadAnyAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadAnyExplicit(Reader, Index);
				else
					return ReadAnyImplicit(Reader, Index);
			}
			else
				return ReadAny(Reader);
		}

		/// <summary>
		/// Reads a any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadAnyImplicit(AsnReader Reader, int Index)
		{
			if (TryReadAny(Reader, new Asn1Tag(TagClass.ContextSpecific, Index),
				out object Item))
			{
				return Item;
			}
			else
				throw new AsnContentException("Unable to read item.");
		}

		/// <summary>
		/// Reads a any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadAnyExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadAny(Inner), Inner);
		}

		/// <summary>
		/// Writes any value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteAny(AsnWriter Writer, object Value)
		{
			WriteAny(Writer, null, Value);
		}

		/// <summary>
		/// Writes any value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteAnyImplicit(AsnWriter Writer, int Index, object Value)
		{
			WriteAny(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes any value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteAnyExplicit(AsnWriter Writer, int Index, object Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteAny(Writer, Value);
			}
		}

		/// <summary>
		/// Writes any value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteAny(AsnWriter Writer, Asn1Tag? Tag, object Value)
		{
			if (Value is null)
			{
				WriteNull(Writer, Tag);
				return;
			}
			else
			{
				switch (Convert.GetTypeCode(Value))
				{
					case TypeCode.Empty:
					case TypeCode.DBNull:
						WriteNull(Writer, Tag);
						return;

					case TypeCode.Object:
						break;

					case TypeCode.Boolean:
						WriteBoolean(Writer, Tag, (bool)Value);
						return;

					case TypeCode.Char:
						WriteString(Writer, Tag, new string((char)Value, 1),
							UniversalTagNumber.UTF8String);
						return;

					case TypeCode.SByte:
						WriteInteger(Writer, Tag, (sbyte)Value);
						return;

					case TypeCode.Byte:
						WriteInteger(Writer, Tag, (byte)Value);
						return;

					case TypeCode.Int16:
						WriteInteger(Writer, Tag, (short)Value);
						return;

					case TypeCode.UInt16:
						WriteInteger(Writer, Tag, (ushort)Value);
						return;

					case TypeCode.Int32:
						WriteInteger(Writer, Tag, (int)Value);
						return;

					case TypeCode.UInt32:
						WriteInteger(Writer, Tag, (uint)Value);
						return;

					case TypeCode.Int64:
						WriteInteger(Writer, Tag, (long)Value);
						return;

					case TypeCode.UInt64:
						WriteInteger(Writer, Tag, (ulong)Value);
						return;

					case TypeCode.Single:
						WriteReal(Writer, Tag, (float)Value);
						return;

					case TypeCode.Double:
						WriteReal(Writer, Tag, (double)Value);
						return;

					case TypeCode.Decimal:
						WriteReal(Writer, Tag, (double)(decimal)Value);
						return;

					case TypeCode.DateTime:
						WriteDateTime(Writer, Tag, (DateTime)Value);
						return;

					case TypeCode.String:
						WriteString(Writer, Tag, (string)Value,
							UniversalTagNumber.UTF8String);
						return;
				}
			}

			if (Value is Enum Enum)
			{
				WriteEnumeration(Writer, Tag, Enum);
				return;
			}
			else if (Value is byte[] Bin)
			{
				WriteOctetString(Writer, Tag, Bin);
				return;
			}
			else if (Value is ObjectId ObjectId)
			{
				WriteObjectId(Writer, Tag, ObjectId);
				return;
			}
			else if (Value is TimeSpan TimeSpan)
			{
				WriteGeneralizedTime(Writer, Tag, TimeSpan);
				return;
			}
			else if (Value is DateTimeOffset DateTimeOffset)
			{
				WriteUtcTime(Writer, Tag, DateTimeOffset);
				return;
			}
			else if (Value is Duration Duration)
			{
				WriteDuration(Writer, Tag, Duration);
				return;
			}
			else if (Value is Array A)
			{
				WriteArray(Writer, Tag, A);
				return;
			}
			else
			{
				throw new ArgumentException("Unsupported value type: " +
					Value.GetType().FullName, nameof(Value));
			}
		}

		#endregion

		#region Bit String

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitString(AsnReader Reader, byte[] Default,
			out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.BitString)
			{
				Result = Reader.ReadBitString(out _);
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringAutomatic(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadBitStringExplicit(Reader, Index, Default, out Result);
				else
					return TryReadBitStringImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadBitString(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringImplicit(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadBitString(out _,
					new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringExplicit(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadBitString(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static byte[] ReadBitString(AsnReader Reader)
		{
			return Reader.ReadBitString(out _);
		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadBitStringAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadBitStringExplicit(Reader, Index);
				else
					return ReadBitStringImplicit(Reader, Index);
			}
			else
				return ReadBitString(Reader);
		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadBitStringImplicit(AsnReader Reader, int Index)
		{
			return Reader.ReadBitString(out _, new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadBitStringExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadBitString(Inner), Inner);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitString(AsnWriter Writer, byte[] Value)
		{
			WriteBitString(Writer, null, Value);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitStringImplicit(AsnWriter Writer, int Index, byte[] Value)
		{
			WriteBitString(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitStringExplicit(AsnWriter Writer, int Index, byte[] Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteBitString(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteBitString(AsnWriter Writer, Asn1Tag? Tag, byte[] Value)
		{
			if (Value is null)
				throw new ArgumentNullException(nameof(Value));

			Writer.WriteBitString(Value, 0, Tag);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitString(AsnWriter Writer, Enum Value)
		{
			WriteBitString(Writer, null, Value);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitStringImplicit(AsnWriter Writer, int Index, Enum Value)
		{
			WriteBitString(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBitStringExplicit(AsnWriter Writer, int Index, Enum Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteBitString(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a bit string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteBitString(AsnWriter Writer, Asn1Tag? Tag, Enum Value)
		{
			if (Value is null)
				throw new ArgumentNullException(nameof(Value));

			Type EnumType = Value.GetType();
			Type UnderlyingType = Enum.GetUnderlyingType(EnumType);
			byte[] Bin;

			switch (Convert.GetTypeCode(UnderlyingType))
			{
				case TypeCode.SByte:
					Bin = new byte[] { (byte)Convert.ToSByte(Value) };
					break;

				case TypeCode.Byte:
					Bin = new byte[] { Convert.ToByte(Value) };
					break;

				case TypeCode.Int16:
					Bin = BitConverter.GetBytes(Convert.ToInt16(Value));
					break;

				case TypeCode.UInt16:
					Bin = BitConverter.GetBytes(Convert.ToUInt16(Value));
					break;

				case TypeCode.Int32:
					Bin = BitConverter.GetBytes(Convert.ToInt32(Value));
					break;

				case TypeCode.UInt32:
					Bin = BitConverter.GetBytes(Convert.ToUInt32(Value));
					break;

				case TypeCode.Int64:
					Bin = BitConverter.GetBytes(Convert.ToInt64(Value));
					break;

				case TypeCode.UInt64:
					Bin = BitConverter.GetBytes(Convert.ToUInt64(Value));
					break;

				default:
					throw new AsnContentException("Unsupported enumeration type: " +
						UnderlyingType.FullName);
			}

			Writer.WriteBitString(Bin, 0, Tag);
		}

		#endregion

		#region String

		/// <summary>
		/// Checks if a Tag refers to a string type.
		/// </summary>
		/// <param name="Tag">ASN.1 tag</param>
		/// <returns>If the tag represents a string type.</returns>
		public static bool IsString(Asn1Tag Tag)
		{
			if (Tag.TagClass != TagClass.Universal)
				return false;

			switch (Tag.TagValue)
			{
				case (int)UniversalTagNumber.ObjectDescriptor:  // Obsolete
				case (int)UniversalTagNumber.UTF8String:
				case (int)UniversalTagNumber.NumericString:
				case (int)UniversalTagNumber.PrintableString:
				case (int)UniversalTagNumber.TeletexString:     // Same as UniversalTagNumber.T61String:
				case (int)UniversalTagNumber.VideotexString:
				case (int)UniversalTagNumber.IA5String:
				case (int)UniversalTagNumber.GraphicString:
				case (int)UniversalTagNumber.VisibleString:     // Same as UniversalTagNumber.ISO646String:
				case (int)UniversalTagNumber.GeneralString:
				case (int)UniversalTagNumber.UniversalString:
				case (int)UniversalTagNumber.UnrestrictedCharacterString:
				case (int)UniversalTagNumber.BMPString:
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Tries to read a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadString(AsnReader Reader, UniversalTagNumber TagNumber,
			string Default, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (IsString(Tag))
			{
				Result = Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
				return true;
			}
			else if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.OctetString)
			{
				try
				{
					Result = ReadString(Reader, TagNumber);
					return true;
				}
				catch (Exception)
				{
					Result = Default;
					return false;
				}
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringAutomatic(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber, string Default, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
				{
					return TryReadStringExplicit(Reader, Index, TagNumber, Default,
						out Result);
				}
				else
				{
					return TryReadStringImplicit(Reader, Index, TagNumber, Default,
						out Result);
				}
			}
			else
				return TryReadString(Reader, TagNumber, Default, out Result);
		}

		/// <summary>
		/// Tries to read a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringImplicit(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber, string Default, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadCharacterString(TagNumber,
					new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringExplicit(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber, string Default, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			if (!TryReadString(Inner, TagNumber, Default, out Result))
				return false;

			if (Inner.HasData)
			{
				StringBuilder sb = new StringBuilder(Result);

				while (Inner.HasData && TryReadString(Inner, TagNumber, null, out Result))
					sb.Append(Result);

				Result = sb.ToString();
			}

			return CheckComplete(true, Inner);
		}

		/// <summary>
		/// Reads a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <returns>Value</returns>
		public static string ReadString(AsnReader Reader, UniversalTagNumber TagNumber)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (IsString(Tag))
				return Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);

			if (Tag.TagClass == TagClass.Universal && Tag.TagValue == (int)UniversalTagNumber.OctetString)
			{
				bool BuiltInCodec;

				switch (TagNumber)
				{
					case UniversalTagNumber.UTF8String:
					case UniversalTagNumber.NumericString:
					case UniversalTagNumber.PrintableString:
					case UniversalTagNumber.TeletexString: // Also T61String.
					case UniversalTagNumber.IA5String:
					case UniversalTagNumber.VisibleString: // Also ISO646String.
					case UniversalTagNumber.BMPString:
						BuiltInCodec = true;
						break;

					case UniversalTagNumber.UniversalString:
						BuiltInCodec = false;
						break;

					case UniversalTagNumber.ObjectDescriptor:
					case UniversalTagNumber.VideotexString:
					case UniversalTagNumber.GraphicString:
					case UniversalTagNumber.GeneralString:
						throw new NotSupportedException(
							TagNumber + " requires a suitable ISO-2022 character-set decoder.");

					case UniversalTagNumber.UnrestrictedCharacterString:
						throw new NotSupportedException(
							"CHARACTER STRING requires decoding its identification " +
							"and string-value components.");

					default:
						throw new ArgumentOutOfRangeException(nameof(TagNumber), TagNumber,
							"Unsupported character-string type.");
				}

				byte[] Encoded = Reader.PeekEncodedValue().ToArray();
				Encoded[0] = (byte)(0x80 | (Tag.IsConstructed ? 0x20 : 0));

				Asn1Tag TemporaryTag = new Asn1Tag(TagClass.ContextSpecific, 0);
				AsnReader ValueReader = new AsnReader(Encoded, Reader.RuleSet);

				string Result;

				if (BuiltInCodec)
					Result = ValueReader.ReadCharacterString(TagNumber, TemporaryTag);
				else
				{
					// UniversalString: four-octet, big-endian Unicode values.
					byte[] Bin = ValueReader.ReadOctetString(TemporaryTag);

					try
					{
						Result = universalString.GetString(Bin);
					}
					catch (DecoderFallbackException ex)
					{
						throw new AsnContentException(
							"Invalid UniversalString contents.", ex);
					}
				}

				ValueReader.ThrowIfNotEmpty();
				Reader.ReadEncodedValue();

				return Result;
			}
			else
				throw new AsnContentException("Expected string tag, but read " + Tag.ToString());
		}

		private static readonly UTF32Encoding universalString = new UTF32Encoding(
			bigEndian: true, byteOrderMark: false, throwOnInvalidCharacters: true);

		/// <summary>
		/// Reads a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static string ReadStringAutomatic(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadStringExplicit(Reader, Index, TagNumber);
				else
					return ReadStringImplicit(Reader, Index, TagNumber);
			}
			else
				return ReadString(Reader, TagNumber);
		}

		/// <summary>
		/// Reads a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <returns>Value</returns>
		public static string ReadStringImplicit(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber)
		{
			return Reader.ReadCharacterString(TagNumber,
				new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <returns>Value</returns>
		public static string ReadStringExplicit(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			string s = ReadString(Inner, TagNumber);

			if (Inner.HasData)
			{
				StringBuilder sb = new StringBuilder(s);

				while (Inner.HasData)
				{
					s = ReadString(Inner, TagNumber);
					sb.Append(s);
				}

				s = sb.ToString();
			}

			return CheckComplete(s, Inner);
		}

		/// <summary>
		/// Writes a string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="TagNumber">How to encode string.</param>
		public static void WriteString(AsnWriter Writer, string Value,
			UniversalTagNumber TagNumber)
		{
			WriteString(Writer, new Asn1Tag(TagNumber), Value, TagNumber);
		}

		/// <summary>
		/// Writes a string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="TagNumber">How to encode string.</param>
		public static void WriteStringImplicit(AsnWriter Writer, int Index, string Value,
			UniversalTagNumber TagNumber)
		{
			WriteString(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value,
				TagNumber);
		}

		/// <summary>
		/// Writes a string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="TagNumber">How to encode string.</param>
		public static void WriteStringExplicit(AsnWriter Writer, int Index, string Value,
			UniversalTagNumber TagNumber)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteString(Writer, Value, TagNumber);
			}
		}

		/// <summary>
		/// Writes a string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="TagNumber">How to encode string.</param>
		private static void WriteString(AsnWriter Writer, Asn1Tag? Tag, string Value,
			UniversalTagNumber TagNumber)
		{
			if (Value is null)
				throw new ArgumentNullException(nameof(Value));

			Writer.WriteCharacterString(TagNumber, Value, Tag);
		}

		#endregion

		#region Octet String

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetString(AsnReader Reader, byte[] Default,
			out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.OctetString)
			{
				Result = Reader.ReadOctetString();
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringAutomatic(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadOctetStringExplicit(Reader, Index, Default, out Result);
				else
					return TryReadOctetStringImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadOctetString(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringImplicit(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringExplicit(AsnReader Reader, int Index,
			byte[] Default, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadOctetString(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static byte[] ReadOctetString(AsnReader Reader)
		{
			return Reader.ReadOctetString();
		}

		/// <summary>
		/// Reads a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadOctetStringAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadOctetStringExplicit(Reader, Index);
				else
					return ReadOctetStringImplicit(Reader, Index);
			}
			else
				return ReadOctetString(Reader);
		}

		/// <summary>
		/// Reads a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadOctetStringImplicit(AsnReader Reader, int Index)
		{
			return Reader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static byte[] ReadOctetStringExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadOctetString(Inner), Inner);
		}

		/// <summary>
		/// Writes a octet string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteOctetString(AsnWriter Writer, byte[] Value)
		{
			WriteOctetString(Writer, null, Value);
		}

		/// <summary>
		/// Writes a octet string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteOctetStringImplicit(AsnWriter Writer, int Index, byte[] Value)
		{
			WriteOctetString(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a octet string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteOctetStringExplicit(AsnWriter Writer, int Index, byte[] Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteOctetString(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a octet string value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteOctetString(AsnWriter Writer, Asn1Tag? Tag, byte[] Value)
		{
			if (Value is null)
				throw new ArgumentNullException(nameof(Value));

			Writer.WriteOctetString(Value, Tag);
		}

		#endregion

		#region Boolean

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBoolean(AsnReader Reader, bool? Default, out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Boolean)
			{
				Result = Reader.ReadBoolean();
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanAutomatic(AsnReader Reader, int Index,
			bool? Default, out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadBooleanExplicit(Reader, Index, Default, out Result);
				else
					return TryReadBooleanImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadBoolean(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanImplicit(AsnReader Reader, int Index,
			bool? Default, out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadBoolean(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanExplicit(AsnReader Reader, int Index,
			bool? Default, out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadBoolean(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static bool ReadBoolean(AsnReader Reader)
		{
			return Reader.ReadBoolean();
		}

		/// <summary>
		/// Reads a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static bool ReadBooleanAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadBooleanExplicit(Reader, Index);
				else
					return ReadBooleanImplicit(Reader, Index);
			}
			else
				return ReadBoolean(Reader);
		}

		/// <summary>
		/// Reads a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static bool ReadBooleanImplicit(AsnReader Reader, int Index)
		{
			return Reader.ReadBoolean(new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static bool ReadBooleanExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadBoolean(Inner), Inner);
		}

		/// <summary>
		/// Writes a Boolean value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBoolean(AsnWriter Writer, bool Value)
		{
			WriteBoolean(Writer, null, Value);
		}

		/// <summary>
		/// Writes a Boolean value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBooleanImplicit(AsnWriter Writer, int Index, bool Value)
		{
			WriteBoolean(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a Boolean value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteBooleanExplicit(AsnWriter Writer, int Index, bool Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteBoolean(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a Boolean value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteBoolean(AsnWriter Writer, Asn1Tag? Tag, bool Value)
		{
			Writer.WriteBoolean(Value, Tag);
		}

		#endregion

		#region Date

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDate(AsnReader Reader, DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement Date type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateAutomatic(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDateExplicit(Reader, Index, Default, out Result);
				else
					return TryReadDateImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadDate(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateImplicit(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Date type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateExplicit(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadDate(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTime ReadDate(AsnReader Reader)
		{
			throw new NotImplementedException("Implement Date type.");    // TODO
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadDateExplicit(Reader, Index);
				else
					return ReadDateImplicit(Reader, Index);
			}
			else
				return ReadDate(Reader);
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement Date type.");    // TODO
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadDate(Inner), Inner);
		}

		/// <summary>
		/// Writes a date value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDate(AsnWriter Writer, DateTime Value)
		{
			WriteDate(Writer, null, Value);
		}

		/// <summary>
		/// Writes a date value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDateImplicit(AsnWriter Writer, int Index, DateTime Value)
		{
			WriteDate(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a date value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDateExplicit(AsnWriter Writer, int Index, DateTime Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteDate(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a date value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteDate(AsnWriter Writer, Asn1Tag? Tag, DateTime Value)
		{
			throw new NotImplementedException("Implement Date type.");    // TODO
		}

		#endregion

		#region Date and Time

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTime(AsnReader Reader, DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.DateTime)
			{
				throw new NotImplementedException("Implement DateTime type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeAutomatic(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDateTimeExplicit(Reader, Index, Default, out Result);
				else
					return TryReadDateTimeImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadDateTime(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeImplicit(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement DateTime type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeExplicit(AsnReader Reader, int Index,
			DateTime? Default, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadDateTime(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateTime(AsnReader Reader)
		{
			throw new NotImplementedException("Implement DateTime type.");    // TODO
		}

		/// <summary>
		/// Reads a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateTimeAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadDateTimeExplicit(Reader, Index);
				else
					return ReadDateTimeImplicit(Reader, Index);
			}
			else
				return ReadDateTime(Reader);
		}

		/// <summary>
		/// Reads a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateTimeImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement DateTime type.");    // TODO
		}

		/// <summary>
		/// Reads a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateTimeExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadDateTime(Inner), Inner);
		}

		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDateTime(AsnWriter Writer, DateTime Value)
		{
			WriteDateTime(Writer, null, Value);
		}

		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDateTimeImplicit(AsnWriter Writer, int Index, DateTime Value)
		{
			WriteDateTime(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDateTimeExplicit(AsnWriter Writer, int Index, DateTime Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteDateTime(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteDateTime(AsnWriter Writer, Asn1Tag? Tag, DateTime Value)
		{
			throw new NotImplementedException("Implement DateTime type.");    // TODO
		}

		#endregion

		#region Duration

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDuration(AsnReader Reader, Duration? Default,
			out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Duration)
			{
				throw new NotImplementedException("Implement Duration type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationAutomatic(AsnReader Reader, int Index,
			Duration? Default, out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDurationExplicit(Reader, Index, Default, out Result);
				else
					return TryReadDurationImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadDuration(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationImplicit(AsnReader Reader, int Index,
			Duration? Default, out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Duration type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationExplicit(AsnReader Reader, int Index,
			Duration? Default, out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadDuration(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static Duration ReadDuration(AsnReader Reader)
		{
			throw new NotImplementedException("Implement Duration type.");    // TODO
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static Duration ReadDurationAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadDurationExplicit(Reader, Index);
				else
					return ReadDurationImplicit(Reader, Index);
			}
			else
				return ReadDuration(Reader);
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static Duration ReadDurationImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement Duration type.");    // TODO
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static Duration ReadDurationExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadDuration(Inner), Inner);
		}

		/// <summary>
		/// Writes a duration value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDuration(AsnWriter Writer, Duration Value)
		{
			WriteDuration(Writer, null, Value);
		}

		/// <summary>
		/// Writes a duration value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDurationImplicit(AsnWriter Writer, int Index, Duration Value)
		{
			WriteDuration(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a duration value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteDurationExplicit(AsnWriter Writer, int Index, Duration Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteDuration(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a duration value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteDuration(AsnWriter Writer, Asn1Tag? Tag, Duration Value)
		{
			throw new NotImplementedException("Implement Duration type.");    // TODO
		}

		#endregion

		#region Generalized Time

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTime(AsnReader Reader, TimeSpan? Default,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeAutomatic(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadGeneralizedTimeExplicit(Reader, Index, Default, out Result);
				else
					return TryReadGeneralizedTimeImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadGeneralizedTime(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeImplicit(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeExplicit(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadGeneralizedTime(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadGeneralizedTime(AsnReader Reader)
		{
			throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
		}

		/// <summary>
		/// Reads a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadGeneralizedTimeAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadGeneralizedTimeExplicit(Reader, Index);
				else
					return ReadGeneralizedTimeImplicit(Reader, Index);
			}
			else
				return ReadGeneralizedTime(Reader);
		}

		/// <summary>
		/// Reads a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadGeneralizedTimeImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
		}

		/// <summary>
		/// Reads a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadGeneralizedTimeExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadGeneralizedTime(Inner), Inner);
		}

		/// <summary>
		/// Writes a generalized time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteGeneralizedTime(AsnWriter Writer, TimeSpan Value)
		{
			WriteGeneralizedTime(Writer, null, Value);
		}

		/// <summary>
		/// Writes a generalized time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteGeneralizedTimeImplicit(AsnWriter Writer, int Index, TimeSpan Value)
		{
			WriteGeneralizedTime(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a generalized time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteGeneralizedTimeExplicit(AsnWriter Writer, int Index, TimeSpan Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteGeneralizedTime(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a generalized time value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteGeneralizedTime(AsnWriter Writer, Asn1Tag? Tag, TimeSpan Value)
		{
			throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
		}

		#endregion

		#region Integer

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadInteger(AsnReader Reader, BigInteger? Default,
			out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Integer)
			{
				Result = Reader.ReadInteger();
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerAutomatic(AsnReader Reader, int Index,
			BigInteger? Default, out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadIntegerExplicit(Reader, Index, Default, out Result);
				else
					return TryReadIntegerImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadInteger(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerImplicit(AsnReader Reader, int Index,
			BigInteger? Default, out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadInteger(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerExplicit(AsnReader Reader, int Index,
			BigInteger? Default, out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadInteger(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static BigInteger ReadInteger(AsnReader Reader)
		{
			return Reader.ReadInteger();
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static BigInteger ReadIntegerAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadIntegerExplicit(Reader, Index);
				else
					return ReadIntegerImplicit(Reader, Index);
			}
			else
				return ReadInteger(Reader);
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static BigInteger ReadIntegerImplicit(AsnReader Reader, int Index)
		{
			return Reader.ReadInteger(new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static BigInteger ReadIntegerExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadInteger(Inner), Inner);
		}

		/// <summary>
		/// Writes an integer value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteInteger(AsnWriter Writer, BigInteger Value)
		{
			WriteInteger(Writer, null, Value);
		}

		/// <summary>
		/// Writes an integer value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteIntegerImplicit(AsnWriter Writer, int Index, BigInteger Value)
		{
			WriteInteger(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes an integer value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteIntegerExplicit(AsnWriter Writer, int Index, BigInteger Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteInteger(Writer, Value);
			}
		}

		/// <summary>
		/// Writes an integer value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteInteger(AsnWriter Writer, Asn1Tag? Tag, BigInteger Value)
		{
			Writer.WriteInteger(Value, Tag);
		}

		#endregion

		#region Null

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Null result value.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNull(AsnReader Reader, out object Result)
		{
			Result = null;
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Null)
			{
				Reader.ReadNull();
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Null result value.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullAutomatic(AsnReader Reader, int Index,
			out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadNullExplicit(Reader, Index, out Result);
				else
					return TryReadNullImplicit(Reader, Index, out Result);
			}
			else
				return TryReadNull(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Null result value.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullImplicit(AsnReader Reader, int Index,
			out object Result)
		{
			Result = null;
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				return false;
			}

			try
			{
				Reader.ReadNull(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Null result value.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullExplicit(AsnReader Reader, int Index,
			out object Result)
		{
			Result = null;
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadNull(Inner, out Result), Inner);

		}

		/// <summary>
		/// Reads a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static object ReadNull(AsnReader Reader)
		{
			Reader.ReadNull();
			return null;
		}

		/// <summary>
		/// Reads a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadNullAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadNullExplicit(Reader, Index);
				else
					return ReadNullImplicit(Reader, Index);
			}
			else
				return ReadNull(Reader);
		}

		/// <summary>
		/// Reads a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadNullImplicit(AsnReader Reader, int Index)
		{
			Reader.ReadNull(new Asn1Tag(TagClass.ContextSpecific, Index));
			return null;
		}

		/// <summary>
		/// Reads a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static object ReadNullExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadNull(Inner), Inner);
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		public static void WriteNull(AsnWriter Writer)
		{
			WriteNull(Writer, null);
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		public static void WriteNullImplicit(AsnWriter Writer, int Index)
		{
			WriteNull(Writer, new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		public static void WriteNullExplicit(AsnWriter Writer, int Index)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteNull(Writer);
			}
		}

		/// <summary>
		/// Writes a null value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		private static void WriteNull(AsnWriter Writer, Asn1Tag? Tag)
		{
			Writer.WriteNull(Tag);
		}

		#endregion

		#region Object Identifier

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectId(AsnReader Reader, ObjectId Default,
			out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.ObjectIdentifier)
			{
				Result = new ObjectId(Reader.ReadObjectIdentifier());
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdAutomatic(AsnReader Reader, int Index,
			ObjectId Default, out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadObjectIdExplicit(Reader, Index, Default, out Result);
				else
					return TryReadObjectIdImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadObjectId(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdImplicit(AsnReader Reader, int Index,
			ObjectId Default, out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = new ObjectId(Reader.ReadObjectIdentifier(
					new Asn1Tag(TagClass.ContextSpecific, Index)));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdExplicit(AsnReader Reader, int Index,
			ObjectId Default, out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadObjectId(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static ObjectId ReadObjectId(AsnReader Reader)
		{
			return new ObjectId(Reader.ReadObjectIdentifier());
		}

		/// <summary>
		/// Reads an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static ObjectId ReadObjectIdAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadObjectIdExplicit(Reader, Index);
				else
					return ReadObjectIdImplicit(Reader, Index);
			}
			else
				return ReadObjectId(Reader);
		}

		/// <summary>
		/// Reads an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static ObjectId ReadObjectIdImplicit(AsnReader Reader, int Index)
		{
			return new ObjectId(Reader.ReadObjectIdentifier(
				new Asn1Tag(TagClass.ContextSpecific, Index)));
		}

		/// <summary>
		/// Reads an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static ObjectId ReadObjectIdExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadObjectId(Inner), Inner);
		}

		/// <summary>
		/// Writes an object identifier value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteObjectId(AsnWriter Writer, ObjectId Value)
		{
			WriteObjectId(Writer, null, Value);
		}

		/// <summary>
		/// Writes an object identifier value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteObjectIdImplicit(AsnWriter Writer, int Index, ObjectId Value)
		{
			WriteObjectId(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes an object identifier value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteObjectIdExplicit(AsnWriter Writer, int Index, ObjectId Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteObjectId(Writer, Value);
			}
		}

		/// <summary>
		/// Writes an object identifier value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteObjectId(AsnWriter Writer, Asn1Tag? Tag, ObjectId Value)
		{
			Writer.WriteObjectIdentifier(Value.Oid, Tag);
		}

		#endregion

		#region Real

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadReal(AsnReader Reader, double? Default,
			out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Real)
			{
				throw new NotImplementedException("Implement Real type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealAutomatic(AsnReader Reader, int Index,
			double Default, out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadRealExplicit(Reader, Index, Default, out Result);
				else
					return TryReadRealImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadReal(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealImplicit(AsnReader Reader, int Index, double Default,
			out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Real type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealExplicit(AsnReader Reader, int Index,
			double Default, out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadReal(Inner, Default, out Result), Inner);
		}

		/// <summary>
		/// Reads a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static double ReadReal(AsnReader Reader)
		{
			throw new NotImplementedException("Implement Real type.");    // TODO
		}

		/// <summary>
		/// Reads a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static double ReadRealAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadRealExplicit(Reader, Index);
				else
					return ReadRealImplicit(Reader, Index);
			}
			else
				return ReadReal(Reader);
		}

		/// <summary>
		/// Reads a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static double ReadRealImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement Real type.");    // TODO
		}

		/// <summary>
		/// Reads a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static double ReadRealExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadReal(Inner), Inner);
		}

		/// <summary>
		/// Writes a floating-point value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteReal(AsnWriter Writer, double Value)
		{
			WriteReal(Writer, null, Value);
		}

		/// <summary>
		/// Writes a floating-point value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteRealImplicit(AsnWriter Writer, int Index, double Value)
		{
			WriteReal(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a floating-point value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteRealExplicit(AsnWriter Writer, int Index, double Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteReal(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a floating-point value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteReal(AsnWriter Writer, Asn1Tag? Tag, double Value)
		{
			throw new NotImplementedException("Implement Real type.");    // TODO
		}

		#endregion

		#region Time of Day

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDay(AsnReader Reader, TimeSpan? Default,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayAutomatic(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadTimeOfDayExplicit(Reader, Index, Default, out Result);
				else
					return TryReadTimeOfDayImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadTimeOfDay(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayImplicit(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayExplicit(AsnReader Reader, int Index,
			TimeSpan? Default, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadTimeOfDay(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadTimeOfDay(AsnReader Reader)
		{
			throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
		}

		/// <summary>
		/// Reads a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadTimeOfDayAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadTimeOfDayExplicit(Reader, Index);
				else
					return ReadTimeOfDayImplicit(Reader, Index);
			}
			else
				return ReadTimeOfDay(Reader);
		}

		/// <summary>
		/// Reads a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadTimeOfDayImplicit(AsnReader Reader, int Index)
		{
			throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
		}

		/// <summary>
		/// Reads a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadTimeOfDayExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadTimeOfDay(Inner), Inner);
		}

		/// <summary>
		/// Writes a time of day value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteTimeOfDay(AsnWriter Writer, TimeSpan Value)
		{
			WriteTimeOfDay(Writer, null, Value);
		}

		/// <summary>
		/// Writes a time of day value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteTimeOfDayImplicit(AsnWriter Writer, int Index, TimeSpan Value)
		{
			WriteTimeOfDay(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a time of day value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteTimeOfDayExplicit(AsnWriter Writer, int Index, TimeSpan Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteTimeOfDay(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a time of day value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteTimeOfDay(AsnWriter Writer, Asn1Tag? Tag, TimeSpan Value)
		{
			throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
		}

		#endregion

		#region UtcTime

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTime(AsnReader Reader, DateTimeOffset? Default,
			out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.UtcTime)
			{
				Result = Reader.ReadUtcTime();
				return true;
			}
			else
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeAutomatic(AsnReader Reader, int Index,
			DateTimeOffset? Default, out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadUtcTimeExplicit(Reader, Index, Default, out Result);
				else
					return TryReadUtcTimeImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadUtcTime(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeImplicit(AsnReader Reader, int Index,
			DateTimeOffset? Default, out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = Reader.ReadUtcTime(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeExplicit(AsnReader Reader, int Index,
			DateTimeOffset? Default, out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(TryReadUtcTime(Inner, Default, out Result), Inner);

		}

		/// <summary>
		/// Reads a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTimeOffset ReadUtcTime(AsnReader Reader)
		{
			return Reader.ReadUtcTime();
		}

		/// <summary>
		/// Reads a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTimeOffset ReadUtcTimeAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return ReadUtcTimeExplicit(Reader, Index);
				else
					return ReadUtcTimeImplicit(Reader, Index);
			}
			else
				return ReadUtcTime(Reader);
		}

		/// <summary>
		/// Reads a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTimeOffset ReadUtcTimeImplicit(AsnReader Reader, int Index)
		{
			return Reader.ReadUtcTime(new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static DateTimeOffset ReadUtcTimeExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return CheckComplete(ReadUtcTime(Inner), Inner);
		}

		/// <summary>
		/// Writes a UTC timestamp value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteUtcTime(AsnWriter Writer, DateTimeOffset Value)
		{
			WriteUtcTime(Writer, null, Value);
		}

		/// <summary>
		/// Writes a UTC timestamp value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteUtcTimeImplicit(AsnWriter Writer, int Index, DateTimeOffset Value)
		{
			WriteUtcTime(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes a UTC timestamp value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteUtcTimeExplicit(AsnWriter Writer, int Index, DateTimeOffset Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteUtcTime(Writer, Value);
			}
		}

		/// <summary>
		/// Writes a UTC timestamp value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteUtcTime(AsnWriter Writer, Asn1Tag? Tag, DateTimeOffset Value)
		{
			Writer.WriteUtcTime(Value, Tag);
		}

		#endregion

		#region Enumeration

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumeration<T>(AsnReader Reader, T? Default,
			out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal)
			{
				Result = Default;
				return false;
			}

			switch (Tag.TagValue)
			{
				case (int)UniversalTagNumber.Integer:
				case (int)UniversalTagNumber.Enumerated:
				case (int)UniversalTagNumber.BitString:
					Result = AssertValidEnum(ReadEnumeration<T>(Reader, Tag));
					return true;
			}

			Result = Default;
			return false;
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationAutomatic<T>(AsnReader Reader, int Index,
			T? Default, out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadEnumerationExplicit(Reader, Index, Default, out Result);
				else
					return TryReadEnumerationImplicit(Reader, Index, Default, out Result);
			}
			else
				return TryReadEnumeration(Reader, Default, out Result);
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationImplicit<T>(AsnReader Reader, int Index,
			T? Default, out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = Default;
				return false;
			}

			try
			{
				Result = ReadEnumeration<T>(Reader);
				return true;
			}
			catch (Exception)
			{
				Result = Default;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationExplicit<T>(AsnReader Reader, int Index,
			T? Default, out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = Default;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadEnumeration(Inner, Default, out Result);

		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static T ReadEnumeration<T>(AsnReader Reader)
			where T : struct, Enum
		{
			return ReadEnumeration<T>(Reader, null);
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ExpectedTag">Expected tag.</param>
		/// <returns>Value</returns>
		public static T ReadEnumeration<T>(AsnReader Reader, Asn1Tag? ExpectedTag)
			where T : struct, Enum
		{
			Type EnumType = typeof(T);

			if (EnumType.GetCustomAttribute<FlagsAttribute>() is null)
			{
				return AssertValidEnum((T)Reader.ReadEnumeratedValue(EnumType,
					ExpectedTag));
			}
			else
			{
				byte[] Bin = Reader.ReadBitString(out _, ExpectedTag);
				switch (Bin.Length)
				{
					case 1: return (T)Enum.ToObject(EnumType, Bin[0]);
					case 2: return (T)Enum.ToObject(EnumType, BitConverter.ToUInt16(Bin, 0));
					case 3:
						Array.Resize(ref Bin, 4);
						return (T)Enum.ToObject(EnumType, BitConverter.ToUInt32(Bin, 0));
					case 4: return (T)Enum.ToObject(EnumType, BitConverter.ToUInt32(Bin, 0));
					case 5:
					case 6:
					case 7:
						Array.Resize(ref Bin, 8);
						return (T)Enum.ToObject(EnumType, BitConverter.ToUInt64(Bin, 0));
					case 8: return (T)Enum.ToObject(EnumType, BitConverter.ToUInt64(Bin, 0));
					default:
						throw new AsnContentException("Unsupported Flags enum size: " + Bin.Length.ToString() + " bytes");
				}
			}
		}

		private static T AssertValidEnum<T>(T Value)
		{
			if (!Enum.IsDefined(typeof(T), Value))
			{
				throw new AsnContentException("Invalid enumeration of type " +
					typeof(T).FullName + ": " + Value.ToString());
			}

			return Value;
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static T ReadEnumerationAutomatic<T>(AsnReader Reader, int Index)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return AssertValidEnum(ReadEnumerationExplicit<T>(Reader, Index));
				else
					return AssertValidEnum(ReadEnumerationImplicit<T>(Reader, Index));
			}
			else
				return AssertValidEnum(ReadEnumeration<T>(Reader));
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static T ReadEnumerationImplicit<T>(AsnReader Reader, int Index)
			where T : struct, Enum
		{
			return ReadEnumeration<T>(Reader, new Asn1Tag(TagClass.ContextSpecific, Index));
		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static T ReadEnumerationExplicit<T>(AsnReader Reader, int Index)
			where T : struct, Enum
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return ReadEnumeration<T>(Inner);
		}

		/// <summary>
		/// Writes an enumerated value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteEnumeration(AsnWriter Writer, Enum Value)
		{
			WriteEnumeration(Writer, null, Value);
		}

		/// <summary>
		/// Writes an enumerated value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteEnumerationImplicit(AsnWriter Writer, int Index, Enum Value)
		{
			WriteEnumeration(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes an enumerated value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteEnumerationExplicit(AsnWriter Writer, int Index, Enum Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteEnumeration(Writer, Value);
			}
		}

		/// <summary>
		/// Writes an enumerated value.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteEnumeration(AsnWriter Writer, Asn1Tag? Tag, Enum Value)
		{
			Type EnumType = Value.GetType();

			if (EnumType.GetCustomAttribute<FlagsAttribute>() is null)
				Writer.WriteEnumeratedValue(Value, Tag);
			else
				WriteBitString(Writer, Tag, Value);
		}

		#endregion

		#region Arrays

		/// <summary>
		/// Tries to read an inner vector (Sequence, Set, External, etc.)
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Inner reader, if successful, null otherwise.</returns>
		public static AsnReader TryReadInnerVector(AsnReader Reader)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal)
				return null;

			ReadOnlyMemory<byte> Section;
			AsnReader Inner;

			switch (Tag.TagValue)
			{
				case (int)UniversalTagNumber.Sequence:
					Section = Reader.ReadEncodedValue();
					Inner = new AsnReader(Section, Reader.RuleSet,
						new AsnReaderOptions()
						{
							SkipSetSortOrderVerification = true
						});
					return Inner.ReadSequence();

				case (int)UniversalTagNumber.Set:
				case (int)UniversalTagNumber.Embedded:
				case (int)UniversalTagNumber.External:
					Section = Reader.ReadEncodedValue();
					Inner = new AsnReader(Section, Reader.RuleSet,
						new AsnReaderOptions()
						{
							SkipSetSortOrderVerification = true
						});
					return Inner.ReadSetOf(Tag);
			}

			return null;
		}

		/// <summary>
		/// Tries to read an array of items.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadArray<T>(AsnReader Reader, TryReadItem<T> ReadItem,
			T[] Default, out T[] Result)
		{
			AsnReader Inner = TryReadInnerVector(Reader);
			if (Inner is null)
			{
				Result = Default;
				return false;
			}

			ChunkedList<T> Items = new ChunkedList<T>();

			while (Inner.HasData && ReadItem(Inner, out T Item))
				Items.Add(Item);

			Result = Items.ToArray();
			return true;
		}

		/// <summary>
		/// Reads an array of items.
		/// </summary>
		/// <typeparam name="T">Type of item to read.</typeparam>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <returns>Array of items.</returns>
		public static T[] ReadArray<T>(AsnReader Reader, TryReadItem<T> ReadItem)
		{
			Asn1Tag Tag = Reader.PeekTag();
			ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();
			AsnReader Inner = new AsnReader(Section, Reader.RuleSet,
				new AsnReaderOptions()
				{
					SkipSetSortOrderVerification = true
				});

			if (Tag.TagValue == (int)UniversalTagNumber.Sequence)
				Inner = Inner.ReadSequence();
			else
				Inner = Inner.ReadSetOf(Tag);

			ChunkedList<T> Items = new ChunkedList<T>();

			while (Inner.HasData && ReadItem(Inner, out T Item))
				Items.Add(Item);

			return Items.ToArray();
		}

		/// <summary>
		/// Tries to read an array of items.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <param name="Default">Default value.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadArray<T>(AsnReader Reader,
			TryReadNullableItem<T> ReadItem, T[] Default, out T[] Result)
			where T : struct
		{
			AsnReader Inner = TryReadInnerVector(Reader);
			if (Inner is null)
			{
				Result = Default;
				return false;
			}

			ChunkedList<T> Items = new ChunkedList<T>();

			while (Inner.HasData && ReadItem(Inner, out T? Item))
				Items.Add(Item.Value);

			Result = Items.ToArray();
			return true;
		}

		/// <summary>
		/// Reads an array of items.
		/// </summary>
		/// <typeparam name="T">Type of item to read.</typeparam>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <returns>Array of items.</returns>
		public static T[] ReadArray<T>(AsnReader Reader,
			TryReadNullableItem<T> ReadItem)
			where T : struct
		{
			Asn1Tag Tag = Reader.PeekTag();
			ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();
			AsnReader Inner = new AsnReader(Section, Reader.RuleSet,
				new AsnReaderOptions()
				{
					SkipSetSortOrderVerification = true
				});

			if (Tag.TagValue == (int)UniversalTagNumber.Sequence)
				Inner = Inner.ReadSequence();
			else
				Inner = Inner.ReadSetOf(Tag);

			ChunkedList<T> Items = new ChunkedList<T>();

			while (Inner.HasData && ReadItem(Inner, out T? Item))
				Items.Add(Item.Value);

			return Items.ToArray();
		}

		/// <summary>
		/// Writes an array of values.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteArray(AsnWriter Writer, Array Value)
		{
			WriteArray(Writer, null, Value);
		}

		/// <summary>
		/// Writes an array of values.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteArrayImplicit(AsnWriter Writer, int Index, Array Value)
		{
			WriteArray(Writer, new Asn1Tag(TagClass.ContextSpecific, Index), Value);
		}

		/// <summary>
		/// Writes an array of values.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Value">Value to write.</param>
		public static void WriteArrayExplicit(AsnWriter Writer, int Index, Array Value)
		{
			using (Writer.PushSequence(new Asn1Tag(TagClass.ContextSpecific, Index, true)))
			{
				WriteArray(Writer, Value);
			}
		}

		/// <summary>
		/// Writes an array of values.
		/// </summary>
		/// <param name="Writer">ASN.1 writer</param>
		/// <param name="Tag">How to encode tag.</param>
		/// <param name="Value">Value to write.</param>
		private static void WriteArray(AsnWriter Writer, Asn1Tag? Tag, Array Value)
		{
			using (Writer.PushSequence(Tag))
			{
				foreach (object Item in Value)
					WriteAny(Writer, Item);
			}
		}

		#endregion

		#endregion

		#region Sets

		/// <summary>
		/// Abstract interface for sets.
		/// </summary>
		public interface ISet
		{
			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			bool Contains(object Element);
		}

		/// <summary>
		/// Set of all values.
		/// </summary>
		public class AllValues : ISet
		{
			/// <summary>
			/// Set of all values.
			/// </summary>
			public AllValues()
			{
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element) => true;
		}

		/// <summary>
		/// Set containing a single value.
		/// </summary>
		public class SingleValue<T> : ISet
		{
			private readonly T value;

			/// <summary>
			/// Set containing a single value.
			/// </summary>
			/// <param name="Value">Value</param>
			public SingleValue(T Value)
			{
				this.value = Value;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(T Element)
			{
				return this.value.Equals(Element);
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				if (!(Element is T Typed))
					Typed = (T)Convert.ChangeType(Element, typeof(T));

				return this.Contains(Typed);
			}
		}

		/// <summary>
		/// Set defined by an interval.
		/// </summary>
		public class Interval<T> : ISet
			where T : IComparable
		{
			private readonly T from;
			private readonly T to;

			/// <summary>
			/// Set defined by an interval.
			/// </summary>
			/// <param name="From">Start value of interval.</param>
			/// <param name="To">Stop value of interval.</param>
			public Interval(T From, T To)
			{
				this.from = From;
				this.to = To;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(T Element)
			{
				return
					this.from.CompareTo(Element) <= 0 &&
					this.to.CompareTo(Element) >= 0;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				if (!(Element is T Typed))
					Typed = (T)Convert.ChangeType(Element, typeof(T));

				return this.Contains(Typed);
			}
		}

		/// <summary>
		/// Set defined by an interval that is open upwards.
		/// </summary>
		public class IntervalOpenUp<T> : ISet
			where T : IComparable
		{
			private readonly T from;

			/// <summary>
			/// Set defined by an interval that is open upwards.
			/// </summary>
			/// <param name="From">Start value of interval.</param>
			public IntervalOpenUp(T From)
			{
				this.from = From;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(T Element)
			{
				return this.from.CompareTo(Element) <= 0;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				if (!(Element is T Typed))
					Typed = (T)Convert.ChangeType(Element, typeof(T));

				return this.Contains(Typed);
			}
		}

		/// <summary>
		/// Set defined by an interval that is open downwards.
		/// </summary>
		public class IntervalOpenDown<T> : ISet
			where T : IComparable
		{
			private readonly T to;

			/// <summary>
			/// Set defined by an interval that is open downwards.
			/// </summary>
			/// <param name="To">Stop value of interval.</param>
			public IntervalOpenDown(T To)
			{
				this.to = To;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(T Element)
			{
				return this.to.CompareTo(Element) >= 0;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				if (!(Element is T Typed))
					Typed = (T)Convert.ChangeType(Element, typeof(T));

				return this.Contains(Typed);
			}
		}

		/// <summary>
		/// Intersection of two sets.
		/// </summary>
		public class Intersection : ISet
		{
			private readonly ISet left;
			private readonly ISet right;

			/// <summary>
			/// Intersection of two sets.
			/// </summary>
			/// <param name="Left">Left set.</param>
			/// <param name="Right">Right set.</param>
			public Intersection(ISet Left, ISet Right)
			{
				this.left = Left;
				this.right = Right;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				return
					this.left.Contains(Element) &&
					this.right.Contains(Element);
			}
		}

		/// <summary>
		/// Union of two sets.
		/// </summary>
		public class Union : ISet
		{
			private readonly ISet left;
			private readonly ISet right;

			/// <summary>
			/// Union of two sets.
			/// </summary>
			/// <param name="Left">Left set.</param>
			/// <param name="Right">Right set.</param>
			public Union(ISet Left, ISet Right)
			{
				this.left = Left;
				this.right = Right;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				return
					this.left.Contains(Element) ||
					this.right.Contains(Element);
			}
		}

		/// <summary>
		/// Complement of right set in the left set.
		/// </summary>
		public class Complement : ISet
		{
			private readonly ISet left;
			private readonly ISet right;

			/// <summary>
			/// Complement of right set in the left set.
			/// </summary>
			/// <param name="Left">Left set.</param>
			/// <param name="Right">Right set.</param>
			public Complement(ISet Left, ISet Right)
			{
				this.left = Left;
				this.right = Right;
			}

			/// <summary>
			/// Checks if the set contains an element.
			/// </summary>
			/// <param name="Element">Element to check</param>
			/// <returns>If the set contains the element.</returns>
			public bool Contains(object Element)
			{
				return
					this.left.Contains(Element) &&
					!this.right.Contains(Element);
			}
		}

		#endregion

		#region Restrictions

		#region Size

		/// <summary>
		/// Checks the size of a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid values.</param>
		/// <returns>If the size is within accepted bounds.</returns>
		public static bool CheckSize(string Value, ISet Valid)
		{
			int Count = 0;
			int i, c = Value.Length;

			for (i = 0; i < c; i++)
			{
				if (char.IsSurrogatePair(Value, i))
					i++;

				Count++;
			}

			return Valid.Contains((BigInteger)Count);
		}

		/// <summary>
		/// Checks the size of a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid values.</param>
		/// <returns>If the size is within accepted bounds.</returns>
		public static bool CheckSize(Array Value, ISet Valid)
		{
			return Valid.Contains((BigInteger)Value.Length);
		}

		/// <summary>
		/// Checks the size of a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid values.</param>
		/// <returns>If the size is within accepted bounds.</returns>
		public static bool CheckSize(object Value, ISet Valid)
		{
			if (Value is string s)
				return CheckSize(s, Valid);
			else if (Value is Array A)
				return CheckSize(A, Valid);
			else if (Value is null)
				throw new AsnContentException("Null value incompatible with SIZE.");
			else
			{
				throw new AsnContentException("Type incompatible with SIZE: " +
					Value.GetType().FullName);
			}
		}

		#endregion

		#region InSet

		/// <summary>
		/// Checks if a value is a member of a set.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid values.</param>
		/// <returns>If the value is in the set.</returns>
		public static bool CheckInSet(object Value, ISet Valid)
		{
			return Valid.Contains(Value);
		}

		#endregion

		#region From

		/// <summary>
		/// Checks if a value conforms to a given alphabet.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid characters.</param>
		/// <returns>If the value conforms to the alphabet.</returns>
		public static bool CheckFrom(string Value, ISet Valid)
		{
			foreach (char ch in Value)
			{
				if (!Valid.Contains(ch))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if a value conforms to a given alphabet.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Valid">Set of valid characters.</param>
		/// <returns>If the value conforms to the alphabet.</returns>
		public static bool CheckFrom(object Value, ISet Valid)
		{
			if (Value is string s)
				return CheckFrom(s, Valid);
			else if (Value is null)
				throw new AsnContentException("Null value incompatible with FROM.");
			else
			{
				throw new AsnContentException("Type incompatible with FROM: " +
					Value.GetType().FullName);
			}
		}

		#endregion

		// TODO: CheckContaining
		// TODO: CheckEncodedBy
		// TODO: CheckPattern
		// TODO: CheckWithComponents

		#endregion

		#region Comparisons

		/// <summary>
		/// Checks if two objects are equal.
		/// </summary>
		/// <param name="Object1">Object 1</param>
		/// <param name="Object2">Object 2</param>
		/// <returns>If objects are equal</returns>
		public static bool AreEqual(object Object1, object Object2)
		{
			if ((Object1 is null) ^ (Object2 is null))
				return false;

			if (Object1 is null)
				return true;

			return Object1.Equals(Object2);
		}

		/// <summary>
		/// Checks if two arrays are equal.
		/// </summary>
		/// <param name="Array1">Arrat 1</param>
		/// <param name="Array2">Arrat 2</param>
		/// <returns>If arrays are equal.</returns>
		public static bool AreEqual(Array Array1, Array Array2)
		{
			if ((Array1 is null) ^ (Array2 is null))
				return false;

			if (Array1 is null)
				return true;

			int i, c = Array1.Length;
			if (Array2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				object Element1 = Array1.GetValue(i);
				object Element2 = Array2.GetValue(i);

				if (Element1 is Array A1 && Element2 is Array A2)
				{
					if (!AreEqual(A1, A2))
						return false;
				}
				else if (!AreEqual(Element1, Element2))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates the hash code for an object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Hash code.</returns>
		public static int CalcHashCode(object Object)
		{
			return Object?.GetHashCode() ?? 0;
		}

		/// <summary>
		/// Calculates the hash code for an array.
		/// </summary>
		/// <param name="Array">Array.</param>
		/// <returns>Hash code.</returns>
		public static int CalcHashCode(Array Array)
		{
			if (Array is null)
				return 0;

			int i, c = Array.Length;
			int Result = 0;
			object Element;

			for (i = 0; i < c; i++)
			{
				Element = Array.GetValue(i);

				if (Element is Array A)
					Result ^= Result << 5 ^ CalcHashCode(A);
				else
					Result ^= Result << 5 ^ Element.GetHashCode();
			}

			return Result;
		}

		#endregion
	}
}
