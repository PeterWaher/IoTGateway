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

		/// <summary>
		/// Increments <paramref name="Value"/> to the smallest value greater than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Increment(ref object Value)
		{
			if (Value == null)
				return false;

			Type T = Value.GetType();
			uint TypeCode = FilesProvider.GetFieldDataTypeCode(T);

			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					if ((bool)Value)
						return false;
					else
					{
						Value = true;
						return true;
					}

				case ObjectSerializer.TYPE_BYTE:
					byte b = (byte)Value;
					if (b == byte.MaxValue)
						Value = (ushort)(byte.MaxValue + 1);
					else
					{
						b++;
						Value = b;
					}
					return true;

				case ObjectSerializer.TYPE_INT16:
					short i16 = (short)Value;
					if (i16 == short.MaxValue)
						Value = short.MaxValue + 1;
					else
					{
						i16++;
						Value = i16;
					}
					return true;

				case ObjectSerializer.TYPE_INT32:
					int i32 = (int)Value;
					if (i32 == int.MaxValue)
						Value = (long)int.MaxValue + 1;
					else
					{
						i32++;
						Value = i32;
					}
					return true;

				case ObjectSerializer.TYPE_INT64:
					long i64 = (long)Value;
					if (i64 == long.MaxValue)
						Value = (ulong)long.MaxValue + 1;
					else
					{
						i64++;
						Value = i64;
					}
					return true;

				case ObjectSerializer.TYPE_SBYTE:
					sbyte i8 = (sbyte)Value;
					if (i8 == sbyte.MaxValue)
						Value = (short)sbyte.MaxValue + 1;
					else
					{
						i8++;
						Value = i8;
					}
					return true;

				case ObjectSerializer.TYPE_UINT16:
					ushort ui16 = (ushort)Value;
					if (ui16 == ushort.MaxValue)
						Value = (uint)ushort.MaxValue + 1;
					else
					{
						ui16++;
						Value = ui16;
					}
					return true;

				case ObjectSerializer.TYPE_UINT32:
					uint ui32 = (uint)Value;
					if (ui32 == uint.MaxValue)
						Value = (ulong)uint.MaxValue + 1;
					else
					{
						ui32++;
						Value = ui32;
					}
					return true;

				case ObjectSerializer.TYPE_UINT64:
					ulong ui64 = (ulong)Value;
					if (ui64 == ulong.MaxValue)
						Value = (double)ulong.MaxValue + 1;
					else
					{
						ui64++;
						Value = ui64;
					}
					return true;

				case ObjectSerializer.TYPE_DECIMAL:
					decimal dec = (decimal)Value;
					if (!Increment(ref dec))
						return false;
					else
					{
						Value = dec;
						return true;
					}

				case ObjectSerializer.TYPE_DOUBLE:
					double dbl = (double)Value;
					if (!Increment(ref dbl))
						return false;
					else
					{
						Value = dbl;
						return true;
					}

				case ObjectSerializer.TYPE_SINGLE:
					float sng = (float)Value;
					if (!Increment(ref sng))
						return false;
					else
					{
						Value = sng;
						return true;
					}

				case ObjectSerializer.TYPE_DATETIME:
					DateTime DT = (DateTime)Value;
					if (DT.Ticks == long.MaxValue)
						return false;
					else
					{
						Value = new DateTime(DT.Ticks + 1);
						return true;
					}

				case ObjectSerializer.TYPE_TIMESPAN:
					TimeSpan TS = (TimeSpan)Value;
					if (TS.Ticks == long.MaxValue)
						return false;
					else
					{
						Value = new TimeSpan(TS.Ticks + 1);
						return true;
					}

				case ObjectSerializer.TYPE_CHAR:
					char ch = (char)Value;
					if (ch == char.MaxValue)
						return false;
					else
					{
						Value = (char)(ch + 1);
						return true;
					}

				case ObjectSerializer.TYPE_STRING:
					string s = (string)Value;
					s += "\x00";
					Value = s;
					return true;

				case ObjectSerializer.TYPE_ENUM:
					s = Value.ToString();
					s += "\x00";
					Value = s;
					return true;

				case ObjectSerializer.TYPE_GUID:
					Guid Guid = (Guid)Value;
					byte[] A = Guid.ToByteArray();

					A[15]++;
					if (A[15] == 0)
					{
						A[14]++;
						if (A[14] == 0)
						{
							A[13]++;
							if (A[13] == 0)
							{
								A[12]++;
								if (A[12] == 0)
								{
									A[11]++;
									if (A[11] == 0)
									{
										A[10]++;
										if (A[10] == 0)
										{
											A[9]++;
											if (A[9] == 0)
											{
												A[8]++;
												if (A[8] == 0)
												{
													A[6]++;
													if (A[6] == 0)
													{
														A[7]++;
														if (A[7] == 0)
														{
															A[4]++;
															if (A[4] == 0)
															{
																A[5]++;
																if (A[5] == 0)
																{
																	A[0]++;
																	if (A[0] == 0)
																	{
																		A[1]++;
																		if (A[1] == 0)
																		{
																			A[2]++;
																			if (A[2] == 0)
																			{
																				A[3]++;
																				if (A[3] == 0)
																					return false;
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}

					Value = new Guid(A);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Increments <paramref name="Value"/> to the smallest value greater than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Increment(ref double Value)
		{
			double Eps = Value / Math.Pow(2, 51);
			double Diff;
			double Last = Value;

			while (Value != (Diff = (Value + Eps)))
			{
				Last = Diff;
				Eps /= 2;
			}

			if (Value != Last)
			{
				Value = Last;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Increments <paramref name="Value"/> to the smallest value greater than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Increment(ref float Value)
		{
			float Eps = Value / (float)Math.Pow(2, 21);
			float Diff;
			float Last = Value;

			while (Value != (Diff = (Value + Eps)))
			{
				Last = Diff;
				Eps /= 2;
			}

			if (Value != Last)
			{
				Value = Last;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Increments <paramref name="Value"/> to the smallest value greater than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Increment(ref decimal Value)
		{
			decimal Eps = Value / (decimal)Math.Pow(2, 92);
			decimal Diff;
			decimal Last = Value;

			while (Value != (Diff = (Value + Eps)))
			{
				Last = Diff;
				Eps /= 2;
			}

			if (Value != Last)
			{
				Value = Last;
				return true;
			}
			else
				return false;
		}

	}
}
