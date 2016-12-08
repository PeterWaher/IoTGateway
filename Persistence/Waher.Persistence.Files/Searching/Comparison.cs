using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Static class that performs comparisons of property values.
	/// </summary>
	public static class Comparison
	{
		/// <summary>
		/// Tries to make sure <paramref name="x"/> and <paramref name="y"/> have the same type.
		/// </summary>
		/// <param name="x">First value.</param>
		/// <param name="y">Second value.</param>
		/// <returns>If <paramref name="x"/> and <paramref name="y"/> are of the same type after the call.</returns>
		public static bool TryMakeSameType(ref object x, ref object y)
		{
			Type xType = x.GetType();
			Type yType = y.GetType();

			if (xType == yType)
				return true;

			uint xTypeCode = FilesProvider.GetFieldDataTypeCode(xType);
			uint yTypeCode = FilesProvider.GetFieldDataTypeCode(yType);

			Upgrade(ref x, ref xTypeCode);
			Upgrade(ref y, ref yTypeCode);

			if (xTypeCode == yTypeCode)
				return true;

			if (yTypeCode == ObjectSerializer.TYPE_STRING)
			{
				x = ToString(x, xTypeCode);
				return true;
			}

			switch (xTypeCode)
			{
				case ObjectSerializer.TYPE_DECIMAL:
					if (yTypeCode == ObjectSerializer.TYPE_DOUBLE)
					{
						y = (decimal)((double)y);
						return true;
					}
					else
						return false;

				case ObjectSerializer.TYPE_DOUBLE:
					if (yTypeCode == ObjectSerializer.TYPE_DECIMAL)
					{
						x = (decimal)((double)x);
						return true;
					}
					else
						return false;

				case ObjectSerializer.TYPE_STRING:
					y = ToString(y, yTypeCode);
					return true;

				default:
					return false;
			}
		}

		private static string ToString(object Value, uint TypeCode)
		{
			if (TypeCode == ObjectSerializer.TYPE_DOUBLE || TypeCode == ObjectSerializer.TYPE_DECIMAL)
				return Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
			else if (TypeCode == ObjectSerializer.TYPE_DATETIME)
				return ((DateTime)Value).ToString("yyyy-MM-ddTHH:mm:ss");
			else
				return Value.ToString();
		}

		private static void Upgrade(ref object Value, ref uint TypeCode)
		{
			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					Value = (decimal)(((bool)Value) ? 1 : 0);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_BYTE:
					Value = (decimal)((byte)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_INT16:
					Value = (decimal)((short)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_INT32:
					Value = (decimal)((int)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_INT64:
					Value = (decimal)((long)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_SBYTE:
					Value = (decimal)((sbyte)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_UINT16:
					Value = (decimal)((ushort)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_UINT32:
					Value = (decimal)((uint)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_UINT64:
					Value = (decimal)((ulong)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					break;

				case ObjectSerializer.TYPE_SINGLE:
					Value = (double)((float)Value);
					TypeCode = ObjectSerializer.TYPE_DOUBLE;
					break;

				case ObjectSerializer.TYPE_DATETIME:
					break;

				case ObjectSerializer.TYPE_TIMESPAN:
					break;

				case ObjectSerializer.TYPE_CHAR:
					Value = (decimal)((char)Value);
					TypeCode = ObjectSerializer.TYPE_DECIMAL;
					break;

				case ObjectSerializer.TYPE_STRING:
					break;

				case ObjectSerializer.TYPE_ENUM:
				case ObjectSerializer.TYPE_GUID:
					Value = Value.ToString();
					TypeCode = ObjectSerializer.TYPE_STRING;
					break;
			}
		}

		/// <summary>
		/// Compares two values. The values can be of different, but compatible types.
		/// </summary>
		/// <param name="Value1">First value.</param>
		/// <param name="Value2">Second value.</param>
		/// <returns>
		/// Negative, if <paramref name="Value1"/>&lt;<paramref name="Value2"/>.
		/// Positive, if <paramref name="Value1"/>&gt;<paramref name="Value2"/>.
		/// Zero, if <paramref name="Value1"/>=<paramref name="Value2"/>.
		/// </returns>
		public static int? Compare(object Value1, object Value2)
		{
			bool IsNull1 = Value1 == null;
			bool IsNull2 = Value2 == null;

			if (IsNull1 ^ IsNull2)
			{
				if (IsNull1)
					return -1;
				else
					return 1;
			}
			else if (IsNull1)
				return 0;

			if (!TryMakeSameType(ref Value1, ref Value2))
				return null;

			IComparable Comparable = Value1 as IComparable;
			if (Comparable == null)
				return null;

			return Comparable.CompareTo(Value2);
		}
	}
}
