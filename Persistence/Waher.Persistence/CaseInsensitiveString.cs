using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence
{
	/// <summary>
	/// Represents a case-insensitive string.
	/// </summary>
	public class CaseInsensitiveString : IComparable<CaseInsensitiveString>, IComparable
	{
		private readonly string value;
		private readonly string lowerCase;

		/// <summary>
		/// Represents a case-insensitive string.
		/// </summary>
		/// <param name="Value">Value</param>
		public CaseInsensitiveString(string Value)
		{
			this.value = Value;
			this.lowerCase = Value.ToLower();
		}

		/// <summary>
		/// String-representation of the case-insensitive string. (Representation is case sensitive.)
		/// </summary>
		public string Value => this?.value;

		/// <summary>
		/// Lower-case representation of the case-insensitive string.
		/// </summary>
		public string LowerCase => this?.lowerCase;

		/// <summary>
		/// <see cref="Object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			return this.value;
		}

		/// <summary>
		/// Returns the hash code for this string.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return this.lowerCase.GetHashCode();
		}

		/// <summary>
		/// Determines whether this instance and a specified object, which must also be a
		/// System.CaseInsensitiveString object, have the same value.
		/// </summary>
		/// <param name="obj">The string to compare to this instance.</param>
		/// <returns>
		/// true if obj is a System.CaseInsensitiveString and its value is the same as this instance; otherwise,
		/// false. If obj is null, the method returns false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is CaseInsensitiveString S)
				return this.lowerCase.Equals(S.lowerCase);
			else if (obj is string S2)
				return this.lowerCase.Equals(S2.ToLower());
			else
				return false;
		}

		/// <summary>
		/// Compares this instance with a specified System.CaseInsensitiveString object and indicates whether
		/// this instance precedes, follows, or appears in the same position in the sort
		/// order as the specified string.
		/// </summary>
		/// <param name="other">The string to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates whether this instance precedes, follows,
		/// or appears in the same position in the sort order as the strB parameter.Value
		/// Condition Less than zero This instance precedes strB. Zero This instance has
		/// the same position in the sort order as strB. Greater than zero This instance
		/// follows strB.-or- strB is null.
		/// </returns>
		public int CompareTo(CaseInsensitiveString other)
		{
			return this.lowerCase.CompareTo(other.lowerCase);
		}

		/// <summary>
		/// Compares this instance with a specified System.CaseInsensitiveString object and indicates whether
		/// this instance precedes, follows, or appears in the same position in the sort
		/// order as the specified string.
		/// </summary>
		/// <param name="obj">The string to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates whether this instance precedes, follows,
		/// or appears in the same position in the sort order as the strB parameter.Value
		/// Condition Less than zero This instance precedes strB. Zero This instance has
		/// the same position in the sort order as strB. Greater than zero This instance
		/// follows strB.-or- strB is null.
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj is CaseInsensitiveString S)
				return this.lowerCase.CompareTo(S.lowerCase);
			else if (obj is string S2)
				return this.lowerCase.CompareTo(S2.ToLower());
			else
				return -1;
		}

		/// <summary>
		/// Implicitly cases a case-insensitive string to a case-sensitive string.
		/// </summary>
		/// <param name="S">Case-insensitive string</param>
		/// <returns>Case-sensitive string.</returns>
		public static implicit operator string(CaseInsensitiveString S)
		{
			return S.value;
		}

		/// <summary>
		/// Implicitly cases a case-sensitive string to a case-insensitive string.
		/// </summary>
		/// <param name="S">Case-sensitive string</param>
		/// <returns>Case-insensitive string.</returns>
		public static implicit operator CaseInsensitiveString(string S)
		{
			return new CaseInsensitiveString(S);
		}

		/// <summary>
		/// Equality operator
		/// </summary>
		public static bool operator ==(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return S1?.lowerCase == S2?.lowerCase;
		}

		/// <summary>
		/// Non-Equality operator
		/// </summary>
		public static bool operator !=(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return S1?.lowerCase != S2?.lowerCase;
		}

		/// <summary>
		/// Lesser-than operator
		/// </summary>
		public static bool operator <(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.lowerCase, S2.lowerCase) < 0;
		}

		/// <summary>
		/// Greater-than operator
		/// </summary>
		public static bool operator >(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.lowerCase, S2.lowerCase) > 0;
		}

		/// <summary>
		/// Lesser-than-or-equal-to operator
		/// </summary>
		public static bool operator <=(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.lowerCase, S2.lowerCase) <= 0;
		}

		/// <summary>
		/// Greater-than-or-equal-to operator
		/// </summary>
		public static bool operator >=(CaseInsensitiveString S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.lowerCase, S2.lowerCase) >= 0;
		}

		/// <summary>
		/// Equality operator
		/// </summary>
		public static bool operator ==(CaseInsensitiveString S1, string S2)
		{
			return S1.lowerCase == S2.ToLower();
		}

		/// <summary>
		/// Non-Equality operator
		/// </summary>
		public static bool operator !=(CaseInsensitiveString S1, string S2)
		{
			return S1.lowerCase != S2.ToLower();
		}

		/// <summary>
		/// Lesser-than operator
		/// </summary>
		public static bool operator <(CaseInsensitiveString S1, string S2)
		{
			return string.Compare(S1.lowerCase, S2.ToLower()) < 0;
		}

		/// <summary>
		/// Greater-than operator
		/// </summary>
		public static bool operator >(CaseInsensitiveString S1, string S2)
		{
			return string.Compare(S1.lowerCase, S2.ToLower()) > 0;
		}

		/// <summary>
		/// Lesser-than-or-equal-to operator
		/// </summary>
		public static bool operator <=(CaseInsensitiveString S1, string S2)
		{
			return string.Compare(S1.lowerCase, S2.ToLower()) <= 0;
		}

		/// <summary>
		/// Greater-than-or-equal-to operator
		/// </summary>
		public static bool operator >=(CaseInsensitiveString S1, string S2)
		{
			return string.Compare(S1.lowerCase, S2.ToLower()) >= 0;
		}

		/// <summary>
		/// Equality operator
		/// </summary>
		public static bool operator ==(string S1, CaseInsensitiveString S2)
		{
			return S1.ToLower() == S2.lowerCase;
		}

		/// <summary>
		/// Non-Equality operator
		/// </summary>
		public static bool operator !=(string S1, CaseInsensitiveString S2)
		{
			return S1.ToLower() != S2.lowerCase;
		}

		/// <summary>
		/// Lesser-than operator
		/// </summary>
		public static bool operator <(string S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.ToLower(), S2.lowerCase) < 0;
		}

		/// <summary>
		/// Greater-than operator
		/// </summary>
		public static bool operator >(string S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.ToLower(), S2.lowerCase) > 0;
		}

		/// <summary>
		/// Lesser-than-or-equal-to operator
		/// </summary>
		public static bool operator <=(string S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.ToLower(), S2.lowerCase) <= 0;
		}

		/// <summary>
		/// Greater-than-or-equal-to operator
		/// </summary>
		public static bool operator >=(string S1, CaseInsensitiveString S2)
		{
			return string.Compare(S1.ToLower(), S2.lowerCase) >= 0;
		}

		/// <summary>
		/// Empty case-insensitive string
		/// </summary>
		public static readonly CaseInsensitiveString Empty = new CaseInsensitiveString(string.Empty);

		/// <summary>
		/// Initializes a new instance of the <see cref="CaseInsensitiveString"/> class to the value indicated
		/// by an array of Unicode characters.
		/// </summary>
		/// <param name="value">An array of Unicode characters.</param>
		public CaseInsensitiveString(char[] value)
			: this(new string(value))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CaseInsensitiveString"/> class to the value indicated
		/// by a specified Unicode character repeated a specified number of times.
		/// </summary>
		/// <param name="c">A Unicode character.</param>
		/// <param name="count">The number of times c occurs.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is less than zero.
		/// </exception>
		public CaseInsensitiveString(char c, int count)
			: this(new string(c, count))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CaseInsensitiveString"/> class to the value indicated
		/// by an array of Unicode characters, a starting character position within that
		/// array, and a length.
		/// </summary>
		/// <param name="value">An array of Unicode characters.</param>
		/// <param name="startIndex">The starting position within value.</param>
		/// <param name="length">The number of characters within value to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex or length is less than zero.-or- The sum of startIndex and length
		/// is greater than the number of elements in value.
		/// </exception>
		public CaseInsensitiveString(char[] value, int startIndex, int length)
			: this(new string(value, startIndex, length))
		{
		}

		/// <summary>
		/// Gets the System.Char object at a specified position in the current <see cref="CaseInsensitiveString"/>
		/// object.
		/// </summary>
		/// <param name="index">A position in the current string.</param>
		/// <returns>
		/// The object at position index.
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// index is greater than or equal to the length of this object or less than zero.
		/// </exception>
		public char this[int index] => this.value[index];

		/// <summary>
		/// Gets the number of characters in the current <see cref="CaseInsensitiveString"/> object.
		/// </summary>
		/// <returns>
		/// The number of characters in the current string.
		/// </returns>
		public int Length => this.value.Length;

		//
		/// <summary>
		/// Compares two specified <see cref="CaseInsensitiveString"/> objects and returns an integer that indicates
		/// their relative position in the sort order.
		/// </summary>
		/// <param name="strA">The first string to compare.</param>
		/// <param name="strB">The second string to compare.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the lexical relationship between the two
		/// comparands.Value Condition Less than zero strA precedes strB in the sort order.
		/// Zero strA occurs in the same position as strB in the sort order. Greater than
		/// zero strA follows strB in the sort order.
		/// </returns>
		public static int Compare(CaseInsensitiveString strA, CaseInsensitiveString strB)
		{
			return string.Compare(strA.lowerCase, strB.lowerCase);
		}

		/// <summary>
		/// Compares substrings of two specified <see cref="CaseInsensitiveString"/> objects using the specified
		/// rules, and returns an integer that indicates their relative position in the sort
		/// order.
		/// </summary>
		/// <param name="strA">The first string to use in the comparison.</param>
		/// <param name="indexA">The position of the substring within strA.</param>
		/// <param name="strB">The second string to use in the comparison.</param>
		/// <param name="indexB">The position of the substring within strB.</param>
		/// <param name="length">The maximum number of characters in the substrings to compare.</param>
		/// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the lexical relationship between the two
		/// comparands.Value Condition Less than zero The substring in strA precedes the
		/// substring in strB in the sort order.Zero The substrings occur in the same position
		/// in the sort order, or the length parameter is zero. Greater than zero The substring
		/// in strA follllows the substring in strB in the sort order.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// indexA is greater than strA.<see cref="CaseInsensitiveString"/>.Length.-or- indexB is greater than
		/// strB.<see cref="CaseInsensitiveString"/>.Length.-or- indexA, indexB, or length is negative. -or-Either
		/// indexA or indexB is null, and length is greater than zero.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a StringComparison value.
		/// </exception>
		public static int Compare(CaseInsensitiveString strA, int indexA, CaseInsensitiveString strB, int indexB, int length, StringComparison comparisonType)
		{
			return string.Compare(strA.lowerCase, indexA, strB.lowerCase, indexB, length, comparisonType);
		}

		/// <summary>
		/// Compares substrings of two specified <see cref="CaseInsensitiveString"/> objects and returns an integer
		/// that indicates their relative position in the sort order.
		/// </summary>
		/// <param name="strA">
		/// The first string to use in the comparison.
		/// </param>
		/// <param name="indexA">
		/// The position of the substring within strA.
		/// </param>
		/// <param name="strB">
		/// The second string to use in the comparison.
		/// </param>
		/// <param name="indexB">
		/// The position of the substring within strB.
		/// </param>
		/// <param name="length">
		/// The maximum number of characters in the substrings to compare.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer indicating the lexical relationship between the two comparands.Value
		/// Condition Less than zero The substring in strA precedes the substring in strB
		/// in the sort order. Zero The substrings occur in the same position in the sort
		/// order, or length is zero. Greater than zero The substring in strA follows the
		/// substring in strB in the sort order.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// indexA is greater than strA.<see cref="CaseInsensitiveString"/>.Length.-or- indexB is greater than
		/// strB.<see cref="CaseInsensitiveString"/>.Length.-or- indexA, indexB, or length is negative. -or-Either
		/// indexA or indexB is null, and length is greater than zero.
		/// </exception>
		public static int Compare(CaseInsensitiveString strA, int indexA, CaseInsensitiveString strB, int indexB, int length)
		{
			return string.Compare(strA.lowerCase, indexA, strB.lowerCase, indexB, length);
		}

		//
		/// <summary>
		/// Compares two specified <see cref="CaseInsensitiveString"/> objects using the specified rules, and returns
		/// an integer that indicates their relative position in the sort order.
		/// </summary>
		/// <param name="strA">
		/// The first string to compare.
		/// </param>
		/// <param name="strB">
		/// The second string to compare.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules to use in the comparison.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer that indicates the lexical relationship between the two
		/// comparands.Value Condition Less than zero strA precedes strB in the sort order.
		/// Zero strA is in the same position as strB in the sort order. Greater than zero
		/// strA follows strB in the sort order.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <see cref="CaseInsensitiveString"/>Comparison is not supported.
		/// </exception>
		public static int Compare(CaseInsensitiveString strA, CaseInsensitiveString strB, StringComparison comparisonType)
		{
			return string.Compare(strA.lowerCase, strB.lowerCase, comparisonType);
		}

		//
		/// <summary>
		/// Compares substrings of two specified <see cref="CaseInsensitiveString"/> objects by evaluating the
		/// numeric values of the corresponding System.Char objects in each substring.
		/// </summary>
		/// <param name="strA">
		/// The first string to use in the comparison.
		/// </param>
		/// <param name="indexA">
		/// The starting index of the substring in strA.
		/// </param>
		/// <param name="strB">
		/// The second string to use in the comparison.
		/// </param>
		/// <param name="indexB">
		/// The starting index of the substring in strB.
		/// </param>
		/// <param name="length">
		/// The maximum number of characters in the substrings to compare.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer that indicates the lexical relationship between the two
		/// comparands.ValueCondition Less than zero The substring in strA is less than the
		/// substring in strB. Zero The substrings are equal, or length is zero. Greater
		/// than zero The substring in strA is greater than the substring in strB.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// strA is not null and indexA is greater than strA.<see cref="CaseInsensitiveString"/>.Length.-or- strB
		/// is not null andindexB is greater than strB.<see cref="CaseInsensitiveString"/>.Length.-or- indexA,
		/// indexB, or length is negative.
		/// </exception>
		public static int CompareOrdinal(CaseInsensitiveString strA, int indexA, CaseInsensitiveString strB, int indexB, int length)
		{
			return string.CompareOrdinal(strA.lowerCase, indexA, strB.lowerCase, indexB, length);
		}

		//
		/// <summary>
		/// Compares two specified <see cref="CaseInsensitiveString"/> objects by evaluating the numeric values
		/// of the corresponding System.Char objects in each string.
		/// </summary>
		/// <param name="strA">
		/// The first string to compare.
		/// </param>
		/// <param name="strB">
		/// The second string to compare.
		/// </param>
		/// <returns>
		/// An integer that indicates the lexical relationship between the two comparands.ValueCondition
		/// Less than zero strA is less than strB. Zero strA and strB are equal. Greater
		/// than zero strA is greater than strB.
		/// </returns>
		public static int CompareOrdinal(CaseInsensitiveString strA, CaseInsensitiveString strB)
		{
			return string.CompareOrdinal(strA.lowerCase, strB.lowerCase);
		}

		//
		/// <summary>
		/// Concatenates the members of an System.Collections.Generic.IEnumerable`1 implementation.
		/// </summary>
		/// <param name="values">
		/// A collection object that implements the System.Collections.Generic.IEnumerable`1
		/// interface.
		/// </param>
		/// <typeparam name="T">
		/// The type of the members of values.
		/// </typeparam>
		/// <returns>
		/// The concatenated members in values.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		public static CaseInsensitiveString Concat<T>(IEnumerable<T> values)
		{
			return new CaseInsensitiveString(string.Concat<T>(values));
		}

		/// <summary>
		/// Concatenates the elements of a specified <see cref="CaseInsensitiveString"/> array.
		/// </summary>
		/// <param name="values">
		/// An array of string instances.
		/// </param>
		/// <returns>
		/// The concatenated elements of values.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		/// <exception cref="System.OutOfMemoryException">
		/// Out of memory.
		/// </exception>
		public static CaseInsensitiveString Concat(params CaseInsensitiveString[] values)
		{
			return new CaseInsensitiveString(string.Concat<CaseInsensitiveString>(values));
		}

		/// <summary>
		/// Concatenates two specified instances of <see cref="CaseInsensitiveString"/>.
		/// </summary>
		/// <param name="str0">
		/// The first string to concatenate.
		/// </param>
		/// <param name="str1">
		/// The second string to concatenate.
		/// </param>
		/// <returns>
		/// The concatenation of str0 and str1.
		/// </returns>
		public static CaseInsensitiveString Concat(CaseInsensitiveString str0, CaseInsensitiveString str1)
		{
			return new CaseInsensitiveString(string.Concat(str0.value, str1.value));
		}

		/// <summary>
		/// Concatenates four specified instances of <see cref="CaseInsensitiveString"/>.
		/// </summary>
		/// <param name="str0">
		/// The first string to concatenate.
		/// </param>
		/// <param name="str1">
		/// The second string to concatenate.
		/// </param>
		/// <param name="str2">
		/// The third string to concatenate.
		/// </param>
		/// <param name="str3">
		/// The fourth string to concatenate.
		/// </param>
		/// <returns>
		/// The concatenation of str0, str1, str2, and str3.
		/// </returns>
		public static CaseInsensitiveString Concat(CaseInsensitiveString str0, CaseInsensitiveString str1, CaseInsensitiveString str2, CaseInsensitiveString str3)
		{
			return new CaseInsensitiveString(string.Concat(str0.value, str1.value, str2.value, str3.value));
		}

		/// <summary>
		/// Concatenates the string representations of three specified objects.
		/// </summary>
		/// <param name="arg0">
		/// The first object to concatenate.
		/// </param>
		/// <param name="arg1">
		/// The second object to concatenate.
		/// </param>
		/// <param name="arg2">
		/// The third object to concatenate.
		/// </param>
		/// <returns>
		/// The concatenated string representations of the values of arg0, arg1, and arg2.
		/// </returns>
		public static CaseInsensitiveString Concat(object arg0, object arg1, object arg2)
		{
			return new CaseInsensitiveString(string.Concat(arg0, arg1, arg2));
		}

		/// <summary>
		/// Concatenates the string representations of two specified objects.
		/// </summary>
		/// <param name="arg0">
		/// The first object to concatenate.
		/// </param>
		/// <param name="arg1">
		/// The second object to concatenate.
		/// </param>
		/// <returns>
		/// The concatenated string representations of the values of arg0 and arg1.
		/// </returns>
		public static CaseInsensitiveString Concat(object arg0, object arg1)
		{
			return new CaseInsensitiveString(string.Concat(arg0, arg1));
		}

		/// <summary>
		/// Creates the string representation of a specified object.
		/// </summary>
		/// <param name="arg0">
		/// The object to represent, or null.
		/// </param>
		/// <returns>
		/// The string representation of the value of arg0, or <see cref="CaseInsensitiveString.Empty"/> if arg0
		/// is null.
		/// </returns>
		public static CaseInsensitiveString Concat(object arg0)
		{
			return new CaseInsensitiveString(string.Concat(arg0));
		}

		/// <summary>
		/// Concatenates the members of a constructed System.Collections.Generic.IEnumerable`1
		/// collection of type <see cref="CaseInsensitiveString"/>.
		/// </summary>
		/// <param name="values">
		/// A collection object that implements System.Collections.Generic.IEnumerable`1
		/// and whose generic type argument is <see cref="CaseInsensitiveString"/>.
		/// </param>
		/// <returns>
		/// The concatenated strings in values.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		public static CaseInsensitiveString Concat(IEnumerable<CaseInsensitiveString> values)
		{
			return Concat<CaseInsensitiveString>(values);
		}

		/// <summary>
		/// Concatenates the string representations of the elements in a specified System.Object
		/// array.
		/// </summary>
		/// <param name="args">
		/// An object array that contains the elements to concatenate.
		/// </param>
		/// <returns>
		/// The concatenated string representations of the values of the elements in args.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// args is null.
		/// </exception>
		/// <exception cref="System.OutOfMemoryException">
		/// Out of memory.
		/// </exception>
		public static CaseInsensitiveString Concat(params object[] args)
		{
			return new CaseInsensitiveString(string.Concat(args));
		}

		/// <summary>
		/// Concatenates three specified instances of <see cref="CaseInsensitiveString"/>.
		/// </summary>
		/// <param name="str0">
		/// The first string to concatenate.
		/// </param>
		/// <param name="str1">
		/// The second string to concatenate.
		/// </param>
		/// <param name="str2">
		/// The third string to concatenate.
		/// </param>
		/// <returns>
		/// The concatenation of str0, str1, and str2.
		/// </returns>
		public static CaseInsensitiveString Concat(CaseInsensitiveString str0, CaseInsensitiveString str1, CaseInsensitiveString str2)
		{
			return new CaseInsensitiveString(string.Concat(str0.value, str1.value, str2.value));
		}

		/// <summary>
		/// Determines whether two specified System.CaseInsensitiveString objects have the same value. A
		/// parameter specifies the culture, case, and sort rules used in the comparison.
		/// </summary>
		/// <param name="a">
		/// The first string to compare, or null.
		/// </param>
		/// <param name="b">
		/// The second string to compare, or null.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the comparison.
		/// </param>
		/// <returns>
		/// true if the value of the a parameter is equal to the value of the b parameter;
		/// otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public static bool Equals(CaseInsensitiveString a, CaseInsensitiveString b, StringComparison comparisonType)
		{
			return string.Equals(a.lowerCase, b.lowerCase, comparisonType);
		}

		/// <summary>
		/// Determines whether two specified System.CaseInsensitiveString objects have the same value.
		/// </summary>
		/// <param name="a">
		/// The first string to compare, or null.
		/// </param>
		/// <param name="b">
		/// The second string to compare, or null.
		/// </param>
		/// <returns>
		/// true if the value of a is the same as the value of b; otherwise, false. If both
		/// a and b are null, the method returns true.
		/// </returns>
		public static bool Equals(CaseInsensitiveString a, CaseInsensitiveString b)
		{
			return string.Equals(a.lowerCase, b.lowerCase);
		}

		/// <summary>
		/// Replaces one or more format items in a specified string with the string representation
		/// of a specified object.
		/// </summary>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which any format items are replaced by the string representation
		/// of arg0.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format item in format is invalid.-or- The index of a format item is not zero.
		/// </exception>
		public static CaseInsensitiveString Format(CaseInsensitiveString format, object arg0)
		{
			return new CaseInsensitiveString(string.Format(format.lowerCase, arg0));
		}

		/// <summary>
		/// Replaces the format items in a specified string with the string representations
		/// of corresponding objects in a specified array. A parameter supplies culture-specific
		/// formatting information.
		/// </summary>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information.
		/// </param>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="args">
		/// An object array that contains zero or more objects to format.
		/// </param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the string representation
		/// of the corresponding objects in args.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format or args is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than or equal to the length of the args array.
		/// </exception>
		public static CaseInsensitiveString Format(IFormatProvider provider, CaseInsensitiveString format, params object[] args)
		{
			return new CaseInsensitiveString(string.Format(provider, format.lowerCase, args));
		}

		/// <summary>
		/// Replaces the format items in a specified string with the string representation
		/// of three specified objects. An parameter supplies culture-specific formatting
		/// information.
		/// </summary>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information.
		/// </param>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The first object to format.
		/// </param>
		/// <param name="arg1">
		/// The second object to format.
		/// </param>
		/// <param name="arg2">
		/// The third object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the string representations
		/// of arg0, arg1, and arg2.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format, arg0, arg1, or arg2 is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than or equal to three.
		/// </exception>
		public static CaseInsensitiveString Format(IFormatProvider provider, CaseInsensitiveString format, object arg0, object arg1, object arg2)
		{
			return new CaseInsensitiveString(string.Format(format.lowerCase, arg0, arg1, arg2));
		}

		/// <summary>
		/// Replaces the format items in a specified string with the string representation
		/// of two specified objects.
		/// </summary>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The first object to format.
		/// </param>
		/// <param name="arg1">
		/// The second object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which format items are replaced by the string representations
		/// of arg0 and arg1.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is not zero or one.
		/// </exception>
		public static CaseInsensitiveString Format(CaseInsensitiveString format, object arg0, object arg1)
		{
			return new CaseInsensitiveString(string.Format(format.lowerCase, arg0, arg1));
		}

		/// <summary>
		/// Replaces the format items in a specified string with the string representation
		/// of two specified objects. A parameter supplies culture-specific formatting information.
		/// </summary>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information.
		/// </param>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The first object to format.
		/// </param>
		/// <param name="arg1">
		/// The second object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which format items are replaced by the string representations
		/// of arg0 and arg1.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format, arg0, or arg1 is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than or equal to two.
		/// </exception>
		public static CaseInsensitiveString Format(IFormatProvider provider, CaseInsensitiveString format, object arg0, object arg1)
		{
			return new CaseInsensitiveString(string.Format(provider, format.lowerCase, arg0, arg1));
		}

		/// <summary>
		/// Replaces the format item or items in a specified string with the string representation
		/// of the corresponding object. A parameter supplies culture-specific formatting
		/// information.
		/// </summary>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information.
		/// </param>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which the format item or items have been replaced by the
		/// string representation of arg0.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format or arg0 is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than or equal to one.
		/// </exception>
		public static CaseInsensitiveString Format(IFormatProvider provider, CaseInsensitiveString format, object arg0)
		{
			return new CaseInsensitiveString(string.Format(provider, format.lowerCase, arg0));
		}

		/// <summary>
		/// Replaces the format item in a specified string with the string representation
		/// of a corresponding object in a specified array.
		/// </summary>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="args">
		/// An object array that contains zero or more objects to format.
		/// </param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the string representation
		/// of the corresponding objects in args.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format or args is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than or equal to the length of the args array.
		/// </exception>
		public static CaseInsensitiveString Format(CaseInsensitiveString format, params object[] args)
		{
			return new CaseInsensitiveString(string.Format(format.lowerCase, args));
		}

		/// <summary>
		/// Replaces the format items in a specified string with the string representation
		/// of three specified objects.
		/// </summary>
		/// <param name="format">
		/// A composite format string.
		/// </param>
		/// <param name="arg0">
		/// The first object to format.
		/// </param>
		/// <param name="arg1">
		/// The second object to format.
		/// </param>
		/// <param name="arg2">
		/// The third object to format.
		/// </param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the string representations
		/// of arg0, arg1, and arg2.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// format is invalid.-or- The index of a format item is less than zero, or greater
		/// than two.
		/// </exception>
		public static CaseInsensitiveString Format(CaseInsensitiveString format, object arg0, object arg1, object arg2)
		{
			return new CaseInsensitiveString(string.Format(format.lowerCase, arg0, arg1, arg2));
		}

		/// <summary>
		/// Indicates whether the specified string is null or an <see cref="CaseInsensitiveString.Empty"/> string.
		/// </summary>
		/// <param name="value">
		/// The string to test.
		/// </param>
		/// <returns>
		/// true if the value parameter is null or an empty string (""); otherwise, false.
		/// </returns>
		public static bool IsNullOrEmpty(CaseInsensitiveString value)
		{
			return string.IsNullOrEmpty(value?.value);
		}

		/// <summary>
		/// Indicates whether a specified string is null, empty, or consists only of white-space
		/// characters.
		/// </summary>
		/// <param name="value">
		/// The string to test.
		/// </param>
		/// <returns>
		/// true if the value parameter is null or <see cref="CaseInsensitiveString.Empty"/>, or if value consists
		/// exclusively of white-space characters.
		/// </returns>
		public static bool IsNullOrWhiteSpace(CaseInsensitiveString value)
		{
			return string.IsNullOrWhiteSpace(value?.value);
		}

		/// <summary>
		/// Concatenates the elements of an object array, using the specified separator between
		/// each element.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. separator is included in the returned string
		/// only if values has more than one element.
		/// </param>
		/// <param name="values">
		/// An array that contains the elements to concatenate.
		/// </param>
		/// <returns>
		/// A string that consists of the elements of values delimited by the separator string.
		/// If values is an empty array, the method returns <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		public static CaseInsensitiveString Join(CaseInsensitiveString separator, params object[] values)
		{
			return new CaseInsensitiveString(string.Join(separator.value, values));
		}

		/// <summary>
		/// Concatenates all the elements of a string array, using the specified separator
		/// between each element.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. separator is included in the returned string
		/// only if value has more than one element.
		/// </param>
		/// <param name="value">
		/// An array that contains the elements to concatenate.
		/// </param>
		/// <returns>
		/// A string that consists of the elements in value delimited by the separator string.
		/// If value is an empty array, the method returns <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public static CaseInsensitiveString Join(CaseInsensitiveString separator, params CaseInsensitiveString[] value)
		{
			return new CaseInsensitiveString(string.Join(separator.value, ToStringArray(value)));
		}

		private static string[] ToStringArray(CaseInsensitiveString[] A)
		{
			int i, c = A.Length;
			string[] B = new string[c];

			for (i = 0; i < c; i++)
				B[i] = A[i].value;

			return B;
		}

		/// <summary>
		/// Concatenates the members of a collection, using the specified separator between
		/// each member.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. separator is included in the returned string
		/// only if values has more than one element.
		/// </param>
		/// <param name="values">
		/// A collection that contains the objects to concatenate.
		/// </param>
		/// <typeparam name="T">
		/// The type of the members of values.
		/// </typeparam>
		/// <returns>
		/// A string that consists of the members of values delimited by the separator string.
		/// If values has no members, the method returns <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		public static CaseInsensitiveString Join<T>(CaseInsensitiveString separator, IEnumerable<T> values)
		{
			return new CaseInsensitiveString(string.Join(separator.value, values));
		}

		/// <summary>
		/// Concatenates the specified elements of a string array, using the specified separator
		/// between each element.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. separator is included in the returned string
		/// only if value has more than one element.
		/// </param>
		/// <param name="value">
		/// An array that contains the elements to concatenate.
		/// </param>
		/// <param name="startIndex">
		/// The first element in value to use.
		/// </param>
		/// <param name="count">
		/// The number of elements of value to use.
		/// </param>
		/// <returns>
		/// A string that consists of the strings in value delimited by the separator string.
		/// -or-<see cref="CaseInsensitiveString.Empty"/> if count is zero, value has no elements, or separator
		/// and all the elements of value are <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex or count is less than 0.-or- startIndex plus count is greater than
		/// the number of elements in value.
		/// </exception>
		/// <exception cref="System.OutOfMemoryException">
		/// Out of memory.
		/// </exception>
		public static CaseInsensitiveString Join(CaseInsensitiveString separator, CaseInsensitiveString[] value, int startIndex, int count)
		{
			return new CaseInsensitiveString(string.Join(separator.value, ToStringArray(value), startIndex, count));
		}

		/// <summary>
		/// Concatenates the members of a constructed System.Collections.Generic.IEnumerable`1
		/// collection of type <see cref="CaseInsensitiveString"/>, using the specified separator between each
		/// member.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. separator is included in the returned string
		/// only if values has more than one element.
		/// </param>
		/// <param name="values">
		/// A collection that contains the strings to concatenate.
		/// </param>
		/// <returns>
		/// A string that consists of the members of values delimited by the separator string.
		/// If values has no members, the method returns <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// values is null.
		/// </exception>
		public static CaseInsensitiveString Join(CaseInsensitiveString separator, IEnumerable<CaseInsensitiveString> values)
		{
			return new CaseInsensitiveString(string.Join<CaseInsensitiveString>(separator.value, values));
		}

		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <returns>
		/// true if the value parameter occurs within this string, or if value is the empty
		/// string (""); otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public bool Contains(CaseInsensitiveString value)
		{
			return this.lowerCase.Contains(value.lowerCase);
		}

		/// <summary>
		/// Copies a specified number of characters from a specified position in this instance
		/// to a specified position in an array of Unicode characters.
		/// </summary>
		/// <param name="sourceIndex">
		/// The index of the first character in this instance to copy.
		/// </param>
		/// <param name="destination">
		/// An array of Unicode characters to which characters in this instance are copied.
		/// </param>
		/// <param name="destinationIndex">
		/// The index in destination at which the copy operation begins.
		/// </param>
		/// <param name="count">
		/// The number of characters in this instance to copy to destination.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// destination is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// sourceIndex, destinationIndex, or count is negative -or- sourceIndex does not
		/// identify a position in the current instance. -or-destinationIndex does not identify
		/// a valid index in the destination array. -or-count is greater than the length
		/// of the substring from startIndex to the end of this instance -or- count is greater
		/// than the length of the subarray from destinationIndex to the end of the destination
		/// array.
		/// </exception>
		public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			this.value.CopyTo(sourceIndex, destination, destinationIndex, count);
		}

		/// <summary>
		/// Determines whether the end of this string instance matches the specified string
		/// when compared using the specified comparison option.
		/// </summary>
		/// <param name="value">
		/// The string to compare to the substring at the end of this instance.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that determines how this string and value are compared.
		/// </param>
		/// <returns>
		/// true if the value parameter matches the end of this string; otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public bool EndsWith(CaseInsensitiveString value, StringComparison comparisonType)
		{
			return this.lowerCase.EndsWith(value.lowerCase, comparisonType);
		}

		/// <summary>
		/// Determines whether the end of this string instance matches the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to compare to the substring at the end of this instance.
		/// </param>
		/// <returns>
		/// true if value matches the end of this instance; otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public bool EndsWith(CaseInsensitiveString value)
		{
			return this.lowerCase.EndsWith(value.lowerCase);
		}

		/// <summary>
		/// Determines whether this instance and another specified System.CaseInsensitiveString object have
		/// the same value.
		/// </summary>
		/// <param name="value">
		/// The string to compare to this instance.
		/// </param>
		/// <returns>
		/// true if the value of the value parameter is the same as the value of this instance;
		/// otherwise, false. If value is null, the method returns false.
		/// </returns>
		public bool Equals(CaseInsensitiveString value)
		{
			return this.lowerCase.Equals(value.lowerCase);
		}

		/// <summary>
		/// Determines whether this string and a specified System.CaseInsensitiveString object have the
		/// same value. A parameter specifies the culture, case, and sort rules used in the
		/// comparison.
		/// </summary>
		/// <param name="value">
		/// The string to compare to this instance.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies how the strings will be compared.
		/// </param>
		/// <returns>
		/// true if the value of the value parameter is the same as this string; otherwise,
		/// false.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public bool Equals(CaseInsensitiveString value, StringComparison comparisonType)
		{
			return this.lowerCase.Equals(value.lowerCase, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in the current System.CaseInsensitiveString object. A parameter specifies the type of search
		/// to use for the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it
		/// is not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is 0.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value, StringComparison comparisonType)
		{
			return this.lowerCase.IndexOf(value.lowerCase, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in the current System.CaseInsensitiveString object. Parameters specify the starting search position
		/// in the current string, the number of characters in the current string to search,
		/// and the type of search to use for the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The zero-based index position of the value parameter if that string is found,
		/// or -1 if it is not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count or startIndex is negative.-or- startIndex is greater than the length of
		/// this instance.-or-count is greater than the length of this string minus startIndex.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value, int startIndex, int count, StringComparison comparisonType)
		{
			return this.lowerCase.IndexOf(value.lowerCase, startIndex, count, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in this instance. The search starts at a specified character position and examines
		/// a specified number of character positions.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that string is found, or -1 if it is
		/// not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count or startIndex is negative.-or- startIndex is greater than the length of
		/// this string.-or-count is greater than the length of this string minus startIndex.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value, int startIndex, int count)
		{
			return this.lowerCase.IndexOf(value.lowerCase, startIndex, count);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in this instance. The search starts at a specified character position.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that string is found, or -1 if it is
		/// not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is less than 0 (zero) or greater than the length of this string.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value, int startIndex)
		{
			return this.lowerCase.IndexOf(value.lowerCase, startIndex);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in this instance.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that string is found, or -1 if it is
		/// not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is 0.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value)
		{
			return this.lowerCase.IndexOf(value.lowerCase);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified character
		/// in this instance. The search starts at a specified character position and examines
		/// a specified number of character positions.
		/// </summary>
		/// <param name="value">
		/// A Unicode character to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count or startIndex is negative.-or- startIndex is greater than the length of
		/// this string.-or-count is greater than the length of this string minus startIndex.
		/// </exception>
		public int IndexOf(char value, int startIndex, int count)
		{
			return this.lowerCase.IndexOf(char.ToLower(value), startIndex, count);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified Unicode
		/// character in this string. The search starts at a specified character position.
		/// </summary>
		/// <param name="value">
		/// A Unicode character to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is less than 0 (zero) or greater than the length of the string.
		/// </exception>
		public int IndexOf(char value, int startIndex)
		{
			return this.lowerCase.IndexOf(char.ToLower(value), startIndex);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string
		/// in the current System.CaseInsensitiveString object. Parameters specify the starting search position
		/// in the current string and the type of search to use for the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The zero-based index position of the value parameter if that string is found,
		/// or -1 if it is not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is less than 0 (zero) or greater than the length of this string.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int IndexOf(CaseInsensitiveString value, int startIndex, StringComparison comparisonType)
		{
			return this.lowerCase.IndexOf(value.lowerCase, startIndex, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified Unicode
		/// character in this string.
		/// </summary>
		/// <param name="value">
		/// A Unicode character to seek.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not.
		/// </returns>
		public int IndexOf(char value)
		{
			return this.lowerCase.IndexOf(char.ToLower(value));
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any
		/// character in a specified array of Unicode characters. The search starts at a
		/// specified character position and examines a specified number of character positions.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The zero-based index position of the first occurrence in this instance where
		/// any character in anyOf was found; -1 if no character in anyOf was found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count or startIndex is negative.-or- count + startIndex is greater than the number
		/// of characters in this instance.
		/// </exception>
		public int IndexOfAny(char[] anyOf, int startIndex, int count)
		{
			return this.lowerCase.IndexOfAny(ToLower(anyOf), startIndex, count);
		}

		private static char[] ToLower(char[] A)
		{
			int i, c = A.Length;

			A = (char[])A.Clone();

			for (i = 0; i < c; i++)
				A[i] = char.ToLower(A[i]);

			return A;
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any
		/// character in a specified array of Unicode characters. The search starts at a
		/// specified character position.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position.
		/// </param>
		/// <returns>
		/// The zero-based index position of the first occurrence in this instance where
		/// any character in anyOf was found; -1 if no character in anyOf was found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is negative.-or- startIndex is greater than the number of characters
		/// in this instance.
		/// </exception>
		public int IndexOfAny(char[] anyOf, int startIndex)
		{
			return this.lowerCase.IndexOfAny(ToLower(anyOf), startIndex);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any
		/// character in a specified array of Unicode characters.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <returns>
		/// The zero-based index position of the first occurrence in this instance where
		/// any character in anyOf was found; -1 if no character in anyOf was found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		public int IndexOfAny(char[] anyOf)
		{
			return this.lowerCase.IndexOfAny(ToLower(anyOf));
		}

		/// <summary>
		/// Checks for the first occurrence of the case-insensitive strings in <paramref name="anyOf"/>.
		/// </summary>
		/// <param name="anyOf">Case-insensitive strings to search for.</param>
		/// <param name="startIndex">Index where search starts.</param>
		/// <param name="count">Number of characters to search.</param>
		/// <returns>First occurrence found.</returns>
		public int IndexOfAny(CaseInsensitiveString[] anyOf, int startIndex, int count)
		{
			int Result = int.MaxValue;
			int i, j, c = anyOf.Length;
			bool Found = false;

			for (i = 0; i < c; i++)
			{
				j = this.lowerCase.IndexOf(anyOf[i].lowerCase, startIndex, count);
				if (j >= 0 && j < Result)
				{
					Result = j;
					Found = true;
					count = j - startIndex;
					if (count == 0)
						break;
				}
			}

			if (Found)
				return Result;
			else
				return -1;
		}

		/// <summary>
		/// Checks for the first occurrence of the case-insensitive strings in <paramref name="anyOf"/>.
		/// </summary>
		/// <param name="anyOf">Case-insensitive strings to search for.</param>
		/// <param name="startIndex">Index where search starts.</param>
		/// <returns>First occurrence found.</returns>
		public int IndexOfAny(CaseInsensitiveString[] anyOf, int startIndex)
		{
			return this.IndexOfAny(anyOf, startIndex, this.value.Length - startIndex);
		}

		/// <summary>
		/// Checks for the first occurrence of the case-insensitive strings in <paramref name="anyOf"/>.
		/// </summary>
		/// <param name="anyOf">Case-insensitive strings to search for.</param>
		/// <returns>First occurrence found.</returns>
		public int IndexOfAny(CaseInsensitiveString[] anyOf)
		{
			return this.IndexOfAny(anyOf, 0, this.value.Length);
		}

		/// <summary>
		/// Returns a new string in which a specified string is inserted at a specified index
		/// position in this instance.
		/// </summary>
		/// <param name="startIndex">
		/// The zero-based index position of the insertion.
		/// </param>
		/// <param name="value">
		/// The string to insert.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance, but with value inserted at
		/// position startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is negative or greater than the length of this instance.
		/// </exception>
		public CaseInsensitiveString Insert(int startIndex, CaseInsensitiveString value)
		{
			return new CaseInsensitiveString(this.value.Insert(startIndex, value.value));
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified Unicode
		/// character within this instance.
		/// </summary>
		/// <param name="value">
		/// The Unicode character to seek.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not.
		/// </returns>
		public int LastIndexOf(char value)
		{
			return this.lowerCase.LastIndexOf(char.ToLower(value));
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified Unicode
		/// character within this instance. The search starts at a specified character position
		/// and proceeds backward toward the beginning of the string.
		/// </summary>
		/// <param name="value">
		/// The Unicode character to seek.
		/// </param>
		/// <param name="startIndex">
		/// The starting position of the search. The search proceeds from startIndex toward
		/// the beginning of this instance.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less
		/// than zero or greater than or equal to the length of this instance.
		/// </exception>
		public int LastIndexOf(char value, int startIndex)
		{
			return this.lowerCase.LastIndexOf(char.ToLower(value), startIndex);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of the specified
		/// Unicode character in a substring within this instance. The search starts at a
		/// specified character position and proceeds backward toward the beginning of the
		/// string for a specified number of character positions.
		/// </summary>
		/// <param name="value">
		/// The Unicode character to seek.
		/// </param>
		/// <param name="startIndex">
		/// The starting position of the search. The search proceeds from startIndex toward
		/// the beginning of this instance.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it
		/// is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less
		/// than zero or greater than or equal to the length of this instance.-or-The current
		/// instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex - count + 1 is less
		/// than zero.
		/// </exception>
		public int LastIndexOf(char value, int startIndex, int count)
		{
			return this.lowerCase.LastIndexOf(char.ToLower(value), startIndex, count);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified string
		/// within this instance.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of value if that string is found, or -1
		/// if it is not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is the last index
		/// position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified string
		/// within this instance. The search starts at a specified character position and
		/// proceeds backward toward the beginning of the string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of value if that string is found, or -1
		/// if it is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>. If
		/// value is <see cref="CaseInsensitiveString.Empty"/>, the return value is the smaller of startIndex and
		/// the last index position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less
		/// than zero or greater than the length of the current instance. -or-The current
		/// instance equals <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less than -1 or greater
		/// than zero.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value, int startIndex)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase, startIndex);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified string
		/// within this instance. The search starts at a specified character position and
		/// proceeds backward toward the beginning of the string for a specified number of
		/// character positions.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of value if that string is found, or -1
		/// if it is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>. If
		/// value is <see cref="CaseInsensitiveString.Empty"/>, the return value is the smaller of startIndex and
		/// the last index position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is negative.-or-The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>,
		/// and startIndex is negative.-or- The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>,
		/// and startIndex is greater than the length of this instance.-or-The current instance
		/// does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex - count + 1 specifies a position
		/// that is not within this instance. -or-The current instance equals <see cref="CaseInsensitiveString.Empty"/>
		/// and start is less than -1 or greater than zero. -or-The current instance equals
		/// <see cref="CaseInsensitiveString.Empty"/> and count is greater than 1.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value, int startIndex, int count)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase, startIndex, count);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified string
		/// within this instance. The search starts at a specified character position and
		/// proceeds backward toward the beginning of the string for the specified number
		/// of character positions. A parameter specifies the type of comparison to perform
		/// when searching for the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of the value parameter if that string
		/// is found, or -1 if it is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>.
		/// If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is the smaller of startIndex
		/// and the last index position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is negative.-or-The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>,
		/// and startIndex is negative.-or- The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>,
		/// and startIndex is greater than the length of this instance.-or-The current instance
		/// does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex + 1 - count specifies a position
		/// that is not within this instance. -or-The current instance equals <see cref="CaseInsensitiveString.Empty"/>
		/// and start is less than -1 or greater than zero. -or-The current instance equals
		/// <see cref="CaseInsensitiveString.Empty"/> and count is greater than 1.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value, int startIndex, int count, StringComparison comparisonType)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase, startIndex, count, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the last occurrence of a specified string within
		/// the current System.CaseInsensitiveString object. The search starts at a specified character
		/// position and proceeds backward toward the beginning of the string. A parameter
		/// specifies the type of comparison to perform when searching for the specified
		/// string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of the value parameter if that string
		/// is found, or -1 if it is not found or if the current instance equals <see cref="CaseInsensitiveString.Empty"/>.
		/// If value is <see cref="CaseInsensitiveString.Empty"/>, the return value is the smaller of startIndex
		/// and the last index position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less
		/// than zero or greater than the length of the current instance. -or-The current
		/// instance equals <see cref="CaseInsensitiveString.Empty"/>, and startIndex is less than -1 or greater
		/// than zero.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value, int startIndex, StringComparison comparisonType)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase, startIndex, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index of the last occurrence of a specified string within
		/// the current System.CaseInsensitiveString object. A parameter specifies the type of search to
		/// use for the specified string.
		/// </summary>
		/// <param name="value">
		/// The string to seek.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that specifies the rules for the search.
		/// </param>
		/// <returns>
		/// The zero-based starting index position of the value parameter if that string
		/// is found, or -1 if it is not. If value is <see cref="CaseInsensitiveString.Empty"/>, the return value
		/// is the last index position in this instance.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a valid <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public int LastIndexOf(CaseInsensitiveString value, StringComparison comparisonType)
		{
			return this.lowerCase.LastIndexOf(value.lowerCase, comparisonType);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence in this instance
		/// of one or more characters specified in a Unicode array.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <returns>
		/// The index position of the last occurrence in this instance where any character
		/// in anyOf was found; -1 if no character in anyOf was found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		public int LastIndexOfAny(char[] anyOf)
		{
			return this.lowerCase.LastIndexOfAny(ToLower(anyOf));
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence in this instance
		/// of one or more characters specified in a Unicode array. The search starts at
		/// a specified character position and proceeds backward toward the beginning of
		/// the string.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <returns>
		/// The index position of the last occurrence in this instance where any character
		/// in anyOf was found; -1 if no character in anyOf was found or if the current instance
		/// equals <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and startIndex specifies
		/// a position that is not within this instance.
		/// </exception>
		public int LastIndexOfAny(char[] anyOf, int startIndex)
		{
			return this.lowerCase.LastIndexOfAny(ToLower(anyOf), startIndex);
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence in this instance
		/// of one or more characters specified in a Unicode array. The search starts at
		/// a specified character position and proceeds backward toward the beginning of
		/// the string for a specified number of character positions.
		/// </summary>
		/// <param name="anyOf">
		/// A Unicode character array containing one or more characters to seek.
		/// </param>
		/// <param name="startIndex">
		/// The search starting position. The search proceeds from startIndex toward the
		/// beginning of this instance.
		/// </param>
		/// <param name="count">
		/// The number of character positions to examine.
		/// </param>
		/// <returns>
		/// The index position of the last occurrence in this instance where any character
		/// in anyOf was found; -1 if no character in anyOf was found or if the current instance
		/// equals <see cref="CaseInsensitiveString.Empty"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// anyOf is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and count or startIndex
		/// is negative.-or- The current instance does not equal <see cref="CaseInsensitiveString.Empty"/>, and
		/// startIndex minus count + 1 is less than zero.
		/// </exception>
		public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
		{
			return this.lowerCase.LastIndexOfAny(ToLower(anyOf), startIndex, count);
		}

		/// <summary>
		/// Returns a new string that right-aligns the characters in this instance by padding
		/// them with spaces on the left, for a specified total length.
		/// </summary>
		/// <param name="totalWidth">
		/// The number of characters in the resulting string, equal to the number of original
		/// characters plus any additional padding characters.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance, but right-aligned and padded
		/// on the left with as many spaces as needed to create a length of totalWidth. However,
		/// if totalWidth is less than the length of this instance, the method returns a
		/// reference to the existing instance. If totalWidth is equal to the length of this
		/// instance, the method returns a new string that is identical to this instance.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// totalWidth is less than zero.
		/// </exception>
		public CaseInsensitiveString PadLeft(int totalWidth)
		{
			return new CaseInsensitiveString(this.value.PadLeft(totalWidth));
		}

		/// <summary>
		/// Returns a new string that right-aligns the characters in this instance by padding
		/// them on the left with a specified Unicode character, for a specified total length.
		/// </summary>
		/// <param name="totalWidth">
		/// The number of characters in the resulting string, equal to the number of original
		/// characters plus any additional padding characters.
		/// </param>
		/// <param name="paddingChar">
		/// A Unicode padding character.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance, but right-aligned and padded
		/// on the left with as many paddingChar characters as needed to create a length
		/// of totalWidth. However, if totalWidth is less than the length of this instance,
		/// the method returns a reference to the existing instance. If totalWidth is equal
		/// to the length of this instance, the method returns a new string that is identical
		/// to this instance.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// totalWidth is less than zero.
		/// </exception>
		public CaseInsensitiveString PadLeft(int totalWidth, char paddingChar)
		{
			return new CaseInsensitiveString(this.value.PadLeft(totalWidth, paddingChar));
		}

		/// <summary>
		/// Returns a new string that left-aligns the characters in this string by padding
		/// them with spaces on the right, for a specified total length.
		/// </summary>
		/// <param name="totalWidth">
		/// The number of characters in the resulting string, equal to the number of original
		/// characters plus any additional padding characters.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance, but left-aligned and padded
		/// on the right with as many spaces as needed to create a length of totalWidth.
		/// However, if totalWidth is less than the length of this instance, the method returns
		/// a reference to the existing instance. If totalWidth is equal to the length of
		/// this instance, the method returns a new string that is identical to this instance.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// totalWidth is less than zero.
		/// </exception>
		public CaseInsensitiveString PadRight(int totalWidth)
		{
			return new CaseInsensitiveString(this.value.PadRight(totalWidth));
		}

		/// <summary>
		/// Returns a new string that left-aligns the characters in this string by padding
		/// them on the right with a specified Unicode character, for a specified total length.
		/// </summary>
		/// <param name="totalWidth">
		/// The number of characters in the resulting string, equal to the number of original
		/// characters plus any additional padding characters.
		/// </param>
		/// <param name="paddingChar">
		/// A Unicode padding character.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance, but left-aligned and padded
		/// on the right with as many paddingChar characters as needed to create a length
		/// of totalWidth. However, if totalWidth is less than the length of this instance,
		/// the method returns a reference to the existing instance. If totalWidth is equal
		/// to the length of this instance, the method returns a new string that is identical
		/// to this instance.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// totalWidth is less than zero.
		/// </exception>
		public CaseInsensitiveString PadRight(int totalWidth, char paddingChar)
		{
			return new CaseInsensitiveString(this.value.PadRight(totalWidth, paddingChar));
		}

		/// <summary>
		/// Returns a new string in which all the characters in the current instance, beginning
		/// at a specified position and continuing through the last position, have been deleted.
		/// </summary>
		/// <param name="startIndex">
		/// The zero-based position to begin deleting characters.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this string except for the removed characters.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is less than zero.-or- startIndex specifies a position that is not
		/// within this string.
		/// </exception>
		public CaseInsensitiveString Remove(int startIndex)
		{
			return new CaseInsensitiveString(this.value.Remove(startIndex));
		}

		/// <summary>
		/// Returns a new string in which a specified number of characters in the current
		/// instance beginning at a specified position have been deleted.
		/// </summary>
		/// <param name="startIndex">
		/// The zero-based position to begin deleting characters.
		/// </param>
		/// <param name="count">
		/// The number of characters to delete.
		/// </param>
		/// <returns>
		/// A new string that is equivalent to this instance except for the removed characters.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Either startIndex or count is less than zero.-or- startIndex plus count specify
		/// a position outside this instance.
		/// </exception>
		public CaseInsensitiveString Remove(int startIndex, int count)
		{
			return new CaseInsensitiveString(this.value.Remove(startIndex, count));
		}

		/// <summary>
		/// Returns a new string in which all occurrences of a specified string in the current
		/// instance are replaced with another specified string.
		/// </summary>
		/// <param name="oldValue">
		/// The string to be replaced.
		/// </param>
		/// <param name="newValue">
		/// The string to replace all occurrences of oldValue.
		/// </param>
		/// <returns>
		/// A string that is equivalent to the current string except that all instances of
		/// oldValue are replaced with newValue. If oldValue is not found in the current
		/// instance, the method returns the current instance unchanged.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// oldValue is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// oldValue is the empty string ("").
		/// </exception>
		public CaseInsensitiveString Replace(CaseInsensitiveString oldValue, CaseInsensitiveString newValue)
		{
			string s = this.value;
			string s2 = this.lowerCase;
			int i = s2.IndexOf(oldValue.lowerCase);
			int c = oldValue.Length;
			int d = newValue.Length;
			int Diff = 0;

			while (i >= 0)
			{
				s = s.Remove(i + Diff, c);
				s = s.Insert(i + Diff, newValue.value);

				i = s2.IndexOf(oldValue.lowerCase, i + c);
				Diff += d - c;
			}

			return new CaseInsensitiveString(s);
		}

		/// <summary>
		/// Returns a new string in which all occurrences of a specified Unicode character
		/// in this instance are replaced with another specified Unicode character.
		/// </summary>
		/// <param name="oldChar">
		/// The Unicode character to be replaced.
		/// </param>
		/// <param name="newChar">
		/// The Unicode character to replace all occurrences of oldChar.
		/// </param>
		/// <returns>
		/// A string that is equivalent to this instance except that all instances of oldChar
		/// are replaced with newChar. If oldChar is not found in the current instance, the
		/// method returns the current instance unchanged.
		/// </returns>
		public CaseInsensitiveString Replace(char oldChar, char newChar)
		{
			string s = this.value.Replace(oldChar, newChar);
			char ch = char.ToLower(oldChar);

			if (ch != oldChar)
				s = s.Replace(ch, char.ToLower(newChar));
			else
				s = s.Replace(char.ToUpper(oldChar), char.ToUpper(newChar));

			return new CaseInsensitiveString(s);
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this instance that are
		/// delimited by elements of a specified Unicode character array.
		/// </summary>
		/// <param name="separator">
		/// An array of Unicode characters that delimit the substrings in this instance,
		/// an empty array that contains no delimiters, or null.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this instance that are delimited
		/// by one or more characters in separator. For more information, see the Remarks
		/// section.
		/// </returns>
		public CaseInsensitiveString[] Split(params char[] separator)
		{
			return this.Split(separator, int.MaxValue, StringSplitOptions.None);
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this instance that are
		/// delimited by elements of a specified Unicode character array. A parameter specifies
		/// the maximum number of substrings to return.
		/// </summary>
		/// <param name="separator">
		/// An array of Unicode characters that delimit the substrings in this instance,
		/// an empty array that contains no delimiters, or null.
		/// </param>
		/// <param name="count">
		/// The maximum number of substrings to return.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this instance that are delimited
		/// by one or more characters in separator. For more information, see the Remarks
		/// section.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is negative.
		/// </exception>
		public CaseInsensitiveString[] Split(char[] separator, int count)
		{
			return this.Split(separator, count, StringSplitOptions.None);
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this string that are delimited
		/// by elements of a specified Unicode character array. Parameters specify the maximum
		/// number of substrings to return and whether to return empty array elements.
		/// </summary>
		/// <param name="separator">
		/// An array of Unicode characters that delimit the substrings in this string, an
		/// empty array that contains no delimiters, or null.
		/// </param>
		/// <param name="count">
		/// The maximum number of substrings to return.
		/// </param>
		/// <param name="options">
		/// <see cref="CaseInsensitiveString"/>SplitOptions.RemoveEmptyEntries to omit empty array elements from
		/// the array returned; or <see cref="CaseInsensitiveString"/>SplitOptions.None to include empty array
		/// elements in the array returned.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this string that are delimited
		/// by one or more characters in separator. For more information, see the Remarks
		/// section.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is negative.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// options is not one of the <see cref="CaseInsensitiveString"/>SplitOptions values.
		/// </exception>
		public CaseInsensitiveString[] Split(char[] separator, int count, StringSplitOptions options)
		{
			char[] A = ToLower(separator);
			int i = this.lowerCase.IndexOfAny(A);
			int Last = -1;
			List<CaseInsensitiveString> Result = new List<CaseInsensitiveString>();
			string s;

			while (i >= 0 && count > 0)
			{
				s = this.value.Substring(Last + 1, i - Last - 1);

				if (options != StringSplitOptions.RemoveEmptyEntries || !string.IsNullOrEmpty(s))
				{
					count--;
					Result.Add(new CaseInsensitiveString(s));
				}

				Last = i;
				i = this.lowerCase.IndexOfAny(A, i + 1);
			}

			if (count > 0)
			{
				s = this.value.Substring(Last + 1);

				if (options != StringSplitOptions.RemoveEmptyEntries || !string.IsNullOrEmpty(s))
					Result.Add(new CaseInsensitiveString(this.value.Substring(Last + 1)));
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this string that are delimited
		/// by elements of a specified Unicode character array. A parameter specifies whether
		/// to return empty array elements.
		/// </summary>
		/// <param name="separator">
		/// An array of Unicode characters that delimit the substrings in this string, an
		/// empty array that contains no delimiters, or null.
		/// </param>
		/// <param name="options">
		/// <see cref="CaseInsensitiveString"/>SplitOptions.RemoveEmptyEntries to omit empty array elements from
		/// the array returned; or <see cref="CaseInsensitiveString"/>SplitOptions.None to include empty array
		/// elements in the array returned.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this string that are delimited
		/// by one or more characters in separator. For more information, see the Remarks
		/// section.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// options is not one of the <see cref="CaseInsensitiveString"/>SplitOptions values.
		/// </exception>
		public CaseInsensitiveString[] Split(char[] separator, StringSplitOptions options)
		{
			return this.Split(separator, int.MaxValue, options);
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this string that are delimited
		/// by elements of a specified string array. Parameters specify the maximum number
		/// of substrings to return and whether to return empty array elements.
		/// </summary>
		/// <param name="separator">
		/// A string array that delimits the substrings in this string, an empty array that
		/// contains no delimiters, or null.
		/// </param>
		/// <param name="count">
		/// The maximum number of substrings to return.
		/// </param>
		/// <param name="options">
		/// <see cref="CaseInsensitiveString"/>SplitOptions.RemoveEmptyEntries to omit empty array elements from
		/// the array returned; or <see cref="CaseInsensitiveString"/>SplitOptions.None to include empty array
		/// elements in the array returned.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this string that are delimited
		/// by one or more strings in separator. For more information, see the Remarks section.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// count is negative.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// options is not one of the <see cref="CaseInsensitiveString"/>SplitOptions values.
		/// </exception>
		public CaseInsensitiveString[] Split(CaseInsensitiveString[] separator, int count, StringSplitOptions options)
		{
			int i = this.IndexOfAny(separator);
			int Last = -1;
			List<CaseInsensitiveString> Result = new List<CaseInsensitiveString>();
			string s;

			while (i >= 0 && count > 0)
			{
				s = this.value.Substring(Last + 1, i - Last - 1);

				if (options != StringSplitOptions.RemoveEmptyEntries || !string.IsNullOrEmpty(s))
				{
					count--;
					Result.Add(new CaseInsensitiveString(s));
				}

				Last = i;
				i = this.IndexOfAny(separator, i + 1);
			}

			if (count > 0)
			{
				s = this.value.Substring(Last + 1);

				if (options != StringSplitOptions.RemoveEmptyEntries || !string.IsNullOrEmpty(s))
					Result.Add(new CaseInsensitiveString(this.value.Substring(Last + 1)));
			}

			return Result.ToArray();

		}

		/// <summary>
		/// Returns a string array that contains the substrings in this string that are delimited
		/// by elements of a specified string array. A parameter specifies whether to return
		/// empty array elements.
		/// </summary>
		/// <param name="separator">
		/// A string array that delimits the substrings in this string, an empty array that
		/// contains no delimiters, or null.
		/// </param>
		/// <param name="options">
		/// <see cref="CaseInsensitiveString"/>SplitOptions.RemoveEmptyEntries to omit empty array elements from
		/// the array returned; or <see cref="CaseInsensitiveString"/>SplitOptions.None to include empty array
		/// elements in the array returned.
		/// </param>
		/// <returns>
		/// An array whose elements contain the substrings in this string that are delimited
		/// by one or more strings in separator. For more information, see the Remarks section.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// options is not one of the <see cref="CaseInsensitiveString"/>SplitOptions values.
		/// </exception>
		public CaseInsensitiveString[] Split(CaseInsensitiveString[] separator, StringSplitOptions options)
		{
			return this.Split(separator, int.MaxValue, options);
		}

		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified
		/// string.
		/// </summary>
		/// <param name="value">
		/// The string to compare.
		/// </param>
		/// <returns>
		/// true if value matches the beginning of this string; otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		public bool StartsWith(CaseInsensitiveString value)
		{
			return this.lowerCase.StartsWith(value.lowerCase);
		}

		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified
		/// string when compared using the specified comparison option.
		/// </summary>
		/// <param name="value">
		/// The string to compare.
		/// </param>
		/// <param name="comparisonType">
		/// One of the enumeration values that determines how this string and value are compared.
		/// </param>
		/// <returns>
		/// true if this instance begins with value; otherwise, false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// value is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// comparisonType is not a <see cref="CaseInsensitiveString"/>Comparison value.
		/// </exception>
		public bool StartsWith(CaseInsensitiveString value, StringComparison comparisonType)
		{
			return this.lowerCase.StartsWith(value.lowerCase, comparisonType);
		}

		/// <summary>
		/// Retrieves a substring from this instance. The substring starts at a specified
		/// character position and has a specified length.
		/// </summary>
		/// <param name="startIndex">
		/// The zero-based starting character position of a substring in this instance.
		/// </param>
		/// <param name="length">
		/// The number of characters in the substring.
		/// </param>
		/// <returns>
		/// A string that is equivalent to the substring of length length that begins at
		/// startIndex in this instance, or <see cref="CaseInsensitiveString.Empty"/> if startIndex is equal to
		/// the length of this instance and length is zero.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex plus length indicates a position not within this instance.-or- startIndex
		/// or length is less than zero.
		/// </exception>
		public CaseInsensitiveString Substring(int startIndex, int length)
		{
			return new CaseInsensitiveString(this.value.Substring(startIndex, length));
		}

		/// <summary>
		/// Retrieves a substring from this instance. The substring starts at a specified
		/// character position and continues to the end of the string.
		/// </summary>
		/// <param name="startIndex">
		/// The zero-based starting character position of a substring in this instance.
		/// </param>
		/// <returns>
		/// A string that is equivalent to the substring that begins at startIndex in this
		/// instance, or <see cref="CaseInsensitiveString.Empty"/> if startIndex is equal to the length of this
		/// instance.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex is less than zero or greater than the length of this instance.
		/// </exception>
		public CaseInsensitiveString Substring(int startIndex)
		{
			return new CaseInsensitiveString(this.value.Substring(startIndex));
		}

		/// <summary>
		/// Copies the characters in a specified substring in this instance to a Unicode
		/// character array.
		/// </summary>
		/// <param name="startIndex">
		/// The starting position of a substring in this instance.
		/// </param>
		/// <param name="length">
		/// The length of the substring in this instance.
		/// </param>
		/// <returns>
		/// A Unicode character array whose elements are the length number of characters
		/// in this instance starting from character position startIndex.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// startIndex or length is less than zero.-or- startIndex plus length is greater
		/// than the length of this instance.
		/// </exception>
		public char[] ToCharArray(int startIndex, int length)
		{
			return this.value.ToCharArray(startIndex, length);
		}

		/// <summary>
		/// Copies the characters in this instance to a Unicode character array.
		/// </summary>
		/// <returns>
		/// A Unicode character array whose elements are the individual characters of this
		/// instance. If this instance is an empty string, the returned array is empty and
		/// has a zero length.
		/// </returns>
		public char[] ToCharArray()
		{
			return this.value.ToCharArray();
		}

		/// <summary>
		/// Removes all leading and trailing white-space characters from the current <see cref="CaseInsensitiveString"/>
		/// object.
		/// </summary>
		/// <returns>
		/// The string that remains after all white-space characters are removed from the
		/// start and end of the current string. If no characters can be trimmed from the
		/// current instance, the method returns the current instance unchanged.
		/// </returns>
		public CaseInsensitiveString Trim()
		{
			return new CaseInsensitiveString(this.value.Trim());
		}

		/// <summary>
		/// Removes all leading and trailing occurrences of a set of characters specified
		/// in an array from the current System.CaseInsensitiveString object.
		/// </summary>
		/// <param name="trimChars">
		/// An array of Unicode characters to remove, or null.
		/// </param>
		/// <returns>
		/// The string that remains after all occurrences of the characters in the trimChars
		/// parameter are removed from the start and end of the current string. If trimChars
		/// is null or an empty array, white-space characters are removed instead. If no
		/// characters can be trimmed from the current instance, the method returns the current
		/// instance unchanged.
		/// </returns>
		public CaseInsensitiveString Trim(params char[] trimChars)
		{
			return new CaseInsensitiveString(this.value.Trim(UpperAndLowerCases(trimChars)));
		}

		private static char[] UpperAndLowerCases(char[] trimChars)
		{
			List<char> A = new List<char>();
			int i, c = trimChars.Length;
			char ch, ch2;

			for (i = 0; i < c; i++)
			{
				ch = trimChars[i];
				A.Add(ch);

				ch2 = char.ToLower(ch);
				if (ch2 != ch)
					A.Add(ch2);

				ch2 = char.ToUpper(ch);
				if (ch2 != ch)
					A.Add(ch2);
			}

			return A.ToArray();
		}

		/// <summary>
		/// Removes all trailing occurrences of a set of characters specified in an array
		/// from the current System.CaseInsensitiveString object.
		/// </summary>
		/// <param name="trimChars">
		/// An array of Unicode characters to remove, or null.
		/// </param>
		/// <returns>
		/// The string that remains after all occurrences of the characters in the trimChars
		/// parameter are removed from the end of the current string. If trimChars is null
		/// or an empty array, Unicode white-space characters are removed instead. If no
		/// characters can be trimmed from the current instance, the method returns the current
		/// instance unchanged.
		/// </returns>
		public CaseInsensitiveString TrimEnd(params char[] trimChars)
		{
			return new CaseInsensitiveString(this.value.TrimEnd(UpperAndLowerCases(trimChars)));
		}

		/// <summary>
		/// Removes all leading occurrences of a set of characters specified in an array
		/// from the current System.CaseInsensitiveString object.
		/// </summary>
		/// <param name="trimChars">
		/// An array of Unicode characters to remove, or null.
		/// </param>
		/// <returns>
		/// The string that remains after all occurrences of characters in the trimChars
		/// parameter are removed from the start of the current string. If trimChars is null
		/// or an empty array, white-space characters are removed instead.
		/// </returns>
		public CaseInsensitiveString TrimStart(params char[] trimChars)
		{
			return new CaseInsensitiveString(this.value.TrimStart(UpperAndLowerCases(trimChars)));
		}

	}
}
