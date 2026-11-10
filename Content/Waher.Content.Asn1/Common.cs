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
	/// Common ASN.1 input/output methods.
	/// </summary>
	public static class Common
	{
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

			Asn1Tag Tag = Reader.PeekTag();

			if (Tag.TagClass == TagClass.Universal)
			{
				switch (Tag.TagValue)
				{
					case (int)UniversalTagNumber.EndOfContents:
						Result = null;
						return false;

					case (int)UniversalTagNumber.Boolean:
						Result = ReadBoolean(Reader);
						return true;

					case (int)UniversalTagNumber.Integer:
					case (int)UniversalTagNumber.Enumerated:
						Result = Reader.ReadInteger();
						return true;

					case (int)UniversalTagNumber.BitString:
						Result = ReadBitString(Reader);
						return true;

					case (int)UniversalTagNumber.OctetString:
						byte[] Bin = Reader.ReadOctetString();

						if (Tag.IsConstructed)
						{
							// TODO: Try dedcode binary
						}

						Result = Bin;
						return true;

					case (int)UniversalTagNumber.Null:
						Reader.ReadNull();
						Result = null;
						return true;

					case (int)UniversalTagNumber.ObjectIdentifier:
						Result = ReadObjectId(Reader);
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
						Result = Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
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
						Result = Reader.ReadEncodedValue();   // TODO
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

						if (Tag.TagValue == (int)UniversalTagNumber.Sequence)
							Inner = Inner.ReadSequence();
						else
							Inner = Inner.ReadSetOf(Tag);

						// TODO: read elements

						Result = Inner;
						return true;

					case (int)UniversalTagNumber.UtcTime:
						Result = Reader.ReadUtcTime();
						return true;

					case (int)UniversalTagNumber.GeneralizedTime:
						Result = Reader.ReadGeneralizedTime();
						return true;

					default:
						Result = null;
						return false;
				}
			}
			else if (Tag.TagClass == TagClass.ContextSpecific)
			{
				ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();

				if (Tag.IsConstructed)
				{
					AsnReader Inner = new AsnReader(Section, Reader.RuleSet);

					// TODO: Parse inner

					Result = Inner;
					return true;
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
		/// Tries to read a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadBitString(AsnReader Reader, out Array<byte> Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.BitString)
			{
				Result = null;
				return false;
			}

			Result = Reader.ReadBitString(out _);
			return true;
		}

		/// <summary>
		/// Reads a bit string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static Array<byte> ReadBitString(AsnReader Reader)
		{
			return Reader.ReadBitString(out _);
		}

		/// <summary>
		/// Tries to read a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadString(AsnReader Reader, out string Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal)
			{
				Result = null;
				return false;
			}

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
					Result = Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
					return true;

				default:
					Result = null;
					return false;
			}
		}

		/// <summary>
		/// Reads a string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static string ReadString(AsnReader Reader)
		{
			Asn1Tag Tag = Reader.PeekTag();
			return Reader.ReadCharacterString((UniversalTagNumber)Tag.TagValue);
		}

		/// <summary>
		/// Tries to read an octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadOctetString(AsnReader Reader, out byte[] Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.OctetString)
			{
				Result = null;
				return false;
			}

			Result = Reader.ReadOctetString();
			return false;
		}

		/// <summary>
		/// Reads an octet string.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static byte[] ReadOctetString(AsnReader Reader)
		{
			return Reader.ReadOctetString();
		}

		/// <summary>
		/// Tries to read a Boolean value.
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

			ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();

			if (Section.Length == 3)
			{
				Result = Section.Span[Section.Length - 1] != 0;
				return true;
			}

			Result = null;
			return false;
		}

		/// <summary>
		/// Reads a Boolean value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static bool ReadBoolean(AsnReader Reader)
		{
			ReadOnlyMemory<byte> Section = Reader.ReadEncodedValue();

			if (Section.Length == 3)
				return Section.Span[Section.Length - 1] != 0;
			else
				return Reader.ReadBoolean();
		}

		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static char ReadCharacter(AsnReader Reader)
		{
			string s = ReadString(Reader);
			if (string.IsNullOrEmpty(s))
				return (char)0;
			else
				return s[0];
		}

		/// <summary>
		/// Tries to read a date value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDate(AsnReader Reader, out DateTime? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a date.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTime ReadDate(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read a date and time value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDateTime(AsnReader Reader, out DateTime? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a date and time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTime ReadDateTime(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read a duration value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadDuration(AsnReader Reader, out Duration? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a duration.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static Duration ReadDuration(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read a generalized time value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadGeneralizedTime(AsnReader Reader, out TimeSpan? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a generlized time.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadGeneralizedTime(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read an integer value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadInteger(AsnReader Reader, out BigInteger? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.Integer)
			{
				Result = null;
				return false;
			}

			Result = Reader.ReadInteger();
			return true;
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
		/// Tries to read a null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadNull(AsnReader Reader)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.Null)
			{
				return false;
			}

			Reader.ReadNull();
			return true;
		}

		/// <summary>
		/// Reads a null value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static object ReadNull(AsnReader Reader)
		{
			Reader.ReadNull();
			return null;
		}

		/// <summary>
		/// Tries to read an object identifier value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadObjectId(AsnReader Reader, out ObjectId Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.ObjectIdentifier)
			{
				Result = null;
				return false;
			}

			Result = ReadObjectId(Reader);
			return true;
		}

		/// <summary>
		/// Reads an Object ID.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static ObjectId ReadObjectId(AsnReader Reader)
		{
			string s = Reader.ReadObjectIdentifier();
			string[] Segments = s.Split('.');
			int i, c = Segments.Length;
			uint[] Parsed = new uint[c];

			for (i = 0; i < c; i++)
				Parsed[i] = uint.Parse(Segments[i]);

			return new ObjectId(Parsed);
		}

		/// <summary>
		/// Tries to read a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadReal(AsnReader Reader, out double? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a floating-point value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static double ReadReal(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read a time-of-day value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadTimeOfDay(AsnReader Reader, out TimeSpan? Result)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Reads a time-of-day value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static TimeSpan ReadTimeOfDay(AsnReader Reader)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to read a UTC timestamp value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadUtcTime(AsnReader Reader, out DateTimeOffset? Result)
		{
			Asn1Tag Tag = Reader.PeekTag();
			if (Tag.TagClass != TagClass.Universal ||
				Tag.TagValue != (int)UniversalTagNumber.UtcTime)
			{
				Result = null;
				return false;
			}

			Result = Reader.ReadUtcTime();
			return true;
		}

		/// <summary>
		/// Reads a UTC timestamp value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static DateTimeOffset ReadUtcTime(AsnReader Reader)
		{
			return Reader.ReadUtcTime();
		}

		/// <summary>
		/// Tries to read an enumeration value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadEnumeration<T>(AsnReader Reader, out T Result)
			where T : Enum
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
		/// Reads a time-of-day value.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <returns>Value</returns>
		public static T ReadEnumeration<T>(AsnReader Reader)
			where T : Enum
		{
			BigInteger Value = ReadInteger(Reader);
			if (Value >= int.MinValue && Value <= int.MaxValue)
				return (T)Enum.ToObject(typeof(T), (int)Value);
			else if (Value >= long.MinValue && Value <= long.MaxValue)
				return (T)Enum.ToObject(typeof(T), (long)Value);
			else
				return (T)Enum.ToObject(typeof(T), Value);
		}

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
			out Array<T> Result)
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

			Result = new Array<T>(Items.ToArray());
			return true;
		}

		/// <summary>
		/// Reads an array of items.
		/// </summary>
		/// <typeparam name="T">Type of item to read.</typeparam>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <returns>Array of items.</returns>
		public static Array<T> ReadArray<T>(AsnReader Reader, TryReadItem<T> ReadItem)
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

			return new Array<T>(Items.ToArray());
		}

		/// <summary>
		/// Tries to read an array of items.
		/// </summary>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <param name="Result">Result, if successful.</param>
		/// <returns>If able to read item.</returns>
		public static bool TryReadArray<T>(AsnReader Reader, 
			TryReadNullableItem<T> ReadItem, out Array<T> Result)
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

			Result = new Array<T>(Items.ToArray());
			return true;
		}

		/// <summary>
		/// Reads an array of items.
		/// </summary>
		/// <typeparam name="T">Type of item to read.</typeparam>
		/// <param name="Reader">ASN.1 reader</param>
		/// <param name="ReadItem">Item read, if successful.</param>
		/// <returns>Array of items.</returns>
		public static Array<T> ReadArray<T>(AsnReader Reader, 
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

			return new Array<T>(Items.ToArray());
		}

	}
}
