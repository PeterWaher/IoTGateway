using System;
using System.Formats.Asn1;
using System.Numerics;
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

		#region Any

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAny(AsnReader Reader, out object Result)
		{
			if (!Reader.HasData)
			{
				Result = null;
				return false;
			}

			return TryReadAny(Reader, Reader.PeekTag(), out Result);
		}

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ExpectedTag">Expected tag.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		private static bool TryReadAny(AsnReader Reader, Asn1Tag ExpectedTag,
			out object Result)
		{
			if (ExpectedTag.TagClass == TagClass.Universal)
			{
				switch (ExpectedTag.TagValue)
				{
					case (int)UniversalTagNumber.EndOfContents:
						Result = null;
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
						Result = null;
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

						while (Inner.HasData && TryReadAny(Inner, out object Element))
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
						Result = null;
						return false;
				}
			}
			else if (ExpectedTag.TagClass == TagClass.ContextSpecific)
			{
				ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();

				if (ExpectedTag.TagClass == TagClass.ContextSpecific && ExpectedTag.IsConstructed)  // EXPLICIT
				{
					AsnReader Inner = new AsnReader(Section, Reader.RuleSet);
					return TryReadAny(Inner, out Result);
				}
				else
				{
					Result = Section.ToArray();
					return true;
				}
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyAutomatic(AsnReader Reader, int Index, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadAnyExplicit(Reader, Index, out Result);
				else
					return TryReadAnyImplicit(Reader, Index, out Result);
			}
			else
				return TryReadAny(Reader, out Result);
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyImplicit(AsnReader Reader, int Index, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			if (TryReadAny(Reader, new Asn1Tag(TagClass.ContextSpecific, Index), out Result))
				return true;
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadAnyExplicit(AsnReader Reader, int Index, out object Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadAny(Inner, out Result);

		}

		/// <summary>
		/// Reads any value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static object ReadAny(AsnReader Reader)
		{
			if (TryReadAny(Reader, out object Result))
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

			return ReadAny(Inner);
		}

		#endregion

		#region Bit String

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitString(AsnReader Reader, out byte[] Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringAutomatic(AsnReader Reader, int Index, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadBitStringExplicit(Reader, Index, out Result);
				else
					return TryReadBitStringImplicit(Reader, Index, out Result);
			}
			else
				return TryReadBitString(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringImplicit(AsnReader Reader, int Index, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = Reader.ReadBitString(out _, new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitStringExplicit(AsnReader Reader, int Index, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadBitString(Inner, out Result);

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

			return ReadBitString(Inner);
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
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadString(AsnReader Reader, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (IsString(Tag))
			{
				Result = Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
				return true;
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringAutomatic(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadStringExplicit(Reader, Index, out Result);
				else
					return TryReadStringImplicit(Reader, Index, TagNumber, out Result);
			}
			else
				return TryReadString(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="TagNumber">Expected Tag number.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringImplicit(AsnReader Reader, int Index,
			UniversalTagNumber TagNumber, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadStringExplicit(AsnReader Reader, int Index,
			out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadString(Inner, out Result);

		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static string ReadString(AsnReader Reader)
		{
			Asn1Tag Tag = Reader.PeekTag();
			return Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
		}

		/// <summary>
		/// Reads a bit string.
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
					return ReadStringExplicit(Reader, Index);
				else
					return ReadStringImplicit(Reader, Index, TagNumber);
			}
			else
				return ReadString(Reader);
		}

		/// <summary>
		/// Reads a bit string.
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
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>Value</returns>
		public static string ReadStringExplicit(AsnReader Reader, int Index)
		{
			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return ReadString(Inner);
		}

		#endregion

		#region Octet String

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetString(AsnReader Reader, out byte[] Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringAutomatic(AsnReader Reader, int Index,
			out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadOctetStringExplicit(Reader, Index, out Result);
				else
					return TryReadOctetStringImplicit(Reader, Index, out Result);
			}
			else
				return TryReadOctetString(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringImplicit(AsnReader Reader, int Index,
			out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = Reader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetStringExplicit(AsnReader Reader, int Index, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadOctetString(Inner, out Result);

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

			return ReadOctetString(Inner);
		}

		#endregion

		#region Boolean

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBoolean(AsnReader Reader, out bool? Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanAutomatic(AsnReader Reader, int Index,
			out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadBooleanExplicit(Reader, Index, out Result);
				else
					return TryReadBooleanImplicit(Reader, Index, out Result);
			}
			else
				return TryReadBoolean(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanImplicit(AsnReader Reader, int Index,
			out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = Reader.ReadBoolean(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a Boolean.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBooleanExplicit(AsnReader Reader, int Index, out bool? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadBoolean(Inner, out Result);

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

			return ReadBoolean(Inner);
		}

		#endregion

		#region Date

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDate(AsnReader Reader, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement Date type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateAutomatic(AsnReader Reader, int Index,
			out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDateExplicit(Reader, Index, out Result);
				else
					return TryReadDateImplicit(Reader, Index, out Result);
			}
			else
				return TryReadDate(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateImplicit(AsnReader Reader, int Index,
			out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Date type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateExplicit(AsnReader Reader, int Index, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadDate(Inner, out Result);

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

			return ReadDate(Inner);
		}

		#endregion

		#region Date and Time

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTime(AsnReader Reader, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.DateTime)
			{
				throw new NotImplementedException("Implement DateTime type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeAutomatic(AsnReader Reader, int Index,
			out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDateTimeExplicit(Reader, Index, out Result);
				else
					return TryReadDateTimeImplicit(Reader, Index, out Result);
			}
			else
				return TryReadDateTime(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeImplicit(AsnReader Reader, int Index,
			out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement DateTime type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTimeExplicit(AsnReader Reader, int Index, out DateTime? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadDateTime(Inner, out Result);

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

			return ReadDateTime(Inner);
		}

		#endregion

		#region Duration

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDuration(AsnReader Reader, out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Duration)
			{
				throw new NotImplementedException("Implement Duration type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationAutomatic(AsnReader Reader, int Index,
			out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadDurationExplicit(Reader, Index, out Result);
				else
					return TryReadDurationImplicit(Reader, Index, out Result);
			}
			else
				return TryReadDuration(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationImplicit(AsnReader Reader, int Index,
			out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Duration type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDurationExplicit(AsnReader Reader, int Index, out Duration? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadDuration(Inner, out Result);

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

			return ReadDuration(Inner);
		}

		#endregion

		#region Generalized Time

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTime(AsnReader Reader, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeAutomatic(AsnReader Reader, int Index,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadGeneralizedTimeExplicit(Reader, Index, out Result);
				else
					return TryReadGeneralizedTimeImplicit(Reader, Index, out Result);
			}
			else
				return TryReadGeneralizedTime(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeImplicit(AsnReader Reader, int Index,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement GeneralizedTime type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a generalized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTimeExplicit(AsnReader Reader, int Index, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadGeneralizedTime(Inner, out Result);

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

			return ReadGeneralizedTime(Inner);
		}

		#endregion

		#region Integer

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadInteger(AsnReader Reader, out BigInteger? Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerAutomatic(AsnReader Reader, int Index,
			out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadIntegerExplicit(Reader, Index, out Result);
				else
					return TryReadIntegerImplicit(Reader, Index, out Result);
			}
			else
				return TryReadInteger(Reader, out Result);
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerImplicit(AsnReader Reader, int Index,
			out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = Reader.ReadInteger(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadIntegerExplicit(AsnReader Reader, int Index, out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadInteger(Inner, out Result);

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

			return ReadInteger(Inner);
		}

		#endregion

		#region Null

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNull(AsnReader Reader)
		{
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
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullAutomatic(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadNullExplicit(Reader, Index);
				else
					return TryReadNullImplicit(Reader, Index);
			}
			else
				return TryReadNull(Reader);
		}

		/// <summary>
		/// Tries to read a Null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullImplicit(AsnReader Reader, int Index)
		{
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
		/// <returns>If able to read item.</returns>
		public static bool TryReadNullExplicit(AsnReader Reader, int Index)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadNull(Inner);

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

			return ReadNull(Inner);
		}

		#endregion

		#region Object Identifier

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectId(AsnReader Reader, out ObjectId Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdAutomatic(AsnReader Reader, int Index,
			out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadObjectIdExplicit(Reader, Index, out Result);
				else
					return TryReadObjectIdImplicit(Reader, Index, out Result);
			}
			else
				return TryReadObjectId(Reader, out Result);
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdImplicit(AsnReader Reader, int Index,
			out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an object identifier.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectIdExplicit(AsnReader Reader, int Index, out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadObjectId(Inner, out Result);

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

			return ReadObjectId(Inner);
		}

		#endregion

		#region Real

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadReal(AsnReader Reader, out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Real)
			{
				throw new NotImplementedException("Implement Real type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealAutomatic(AsnReader Reader, int Index,
			out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadRealExplicit(Reader, Index, out Result);
				else
					return TryReadRealImplicit(Reader, Index, out Result);
			}
			else
				return TryReadReal(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealImplicit(AsnReader Reader, int Index,
			out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement Real type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadRealExplicit(AsnReader Reader, int Index, out double? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadReal(Inner, out Result);
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

			return ReadReal(Inner);
		}

		#endregion

		#region Time of Day

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDay(AsnReader Reader, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal &&
				Tag.TagValue == (int)UniversalTagNumber.Date)
			{
				throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayAutomatic(AsnReader Reader, int Index,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadTimeOfDayExplicit(Reader, Index, out Result);
				else
					return TryReadTimeOfDayImplicit(Reader, Index, out Result);
			}
			else
				return TryReadTimeOfDay(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayImplicit(AsnReader Reader, int Index,
			out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				throw new NotImplementedException("Implement TimeOfDay type.");    // TODO
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a time of day.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDayExplicit(AsnReader Reader, int Index, out TimeSpan? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadTimeOfDay(Inner, out Result);

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

			return ReadTimeOfDay(Inner);
		}

		#endregion

		#region UtcTime

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTime(AsnReader Reader, out DateTimeOffset? Result)
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
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeAutomatic(AsnReader Reader, int Index,
			out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadUtcTimeExplicit(Reader, Index, out Result);
				else
					return TryReadUtcTimeImplicit(Reader, Index, out Result);
			}
			else
				return TryReadUtcTime(Reader, out Result);
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeImplicit(AsnReader Reader, int Index,
			out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = Reader.ReadUtcTime(new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read a UTC timestamp.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTimeExplicit(AsnReader Reader, int Index, out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadUtcTime(Inner, out Result);

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

			return ReadUtcTime(Inner);
		}

		#endregion

		#region Enumeration

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumeration<T>(AsnReader Reader, out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal)
			{
				Result = default;
				return false;
			}

			switch (Tag.TagValue)
			{
				case (int)UniversalTagNumber.Integer:
				case (int)UniversalTagNumber.Enumerated:
					Result = ReadEnumeration<T>(Reader);
					return true;
			}

			Result = default;
			return false;
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationAutomatic<T>(AsnReader Reader, int Index,
			out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass == TagClass.ContextSpecific)
			{
				if (Tag.IsConstructed)
					return TryReadEnumerationExplicit(Reader, Index, out Result);
				else
					return TryReadEnumerationImplicit(Reader, Index, out Result);
			}
			else
				return TryReadEnumeration(Reader, out Result);
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationImplicit<T>(AsnReader Reader, int Index,
			out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index)
			{
				Result = null;
				return false;
			}

			try
			{
				Result = (T)Reader.ReadEnumeratedValue(typeof(T),
					new Asn1Tag(TagClass.ContextSpecific, Index));
				return true;
			}
			catch (Exception)
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to read an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumerationExplicit<T>(AsnReader Reader, int Index,
			out T? Result)
			where T : struct, Enum
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.ContextSpecific ||
				Tag.TagValue != Index ||
				!Tag.IsConstructed)
			{
				Result = null;
				return false;
			}

			AsnReader Inner = Reader.ReadSequence(
				new Asn1Tag(TagClass.ContextSpecific, Index, true));

			return TryReadEnumeration(Inner, out Result);

		}

		/// <summary>
		/// Reads an integer.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static T ReadEnumeration<T>(AsnReader Reader)
			where T : struct, Enum
		{
			return (T)Reader.ReadEnumeratedValue(typeof(T));
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
					return ReadEnumerationExplicit<T>(Reader, Index);
				else
					return ReadEnumerationImplicit<T>(Reader, Index);
			}
			else
				return ReadEnumeration<T>(Reader);
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
			return (T)Reader.ReadEnumeratedValue(typeof(T),
				new Asn1Tag(TagClass.ContextSpecific, Index));
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
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadArray<T>(AsnReader Reader, TryReadItem<T> ReadItem,
			out T[] Result)
		{
			AsnReader Inner = TryReadInnerVector(Reader);
			if (Inner is null)
			{
				Result = null;
				return false;
			}

			ChunkedList<T> Items = new ChunkedList<T>();

			while (ReadItem(Inner, out T Item))
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

			while (ReadItem(Inner, out T Item))
				Items.Add(Item);

			return Items.ToArray();
		}

		/// <summary>
		/// Tries to read an array of items.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadArray<T>(AsnReader Reader,
			TryReadNullableItem<T> ReadItem, out T[] Result)
			where T : struct
		{
			AsnReader Inner = TryReadInnerVector(Reader);
			if (Inner is null)
			{
				Result = null;
				return false;
			}

			ChunkedList<T> Items = new ChunkedList<T>();

			while (ReadItem(Inner, out T? Item))
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

			while (ReadItem(Inner, out T? Item))
				Items.Add(Item.Value);

			return Items.ToArray();
		}

		#endregion
	}
}
