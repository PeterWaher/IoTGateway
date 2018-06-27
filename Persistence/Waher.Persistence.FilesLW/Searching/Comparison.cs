using System;
using System.Collections.Generic;
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

		internal static string ToString(object Value)
		{
			if (Value == null)
				return string.Empty;
			else
				return ToString(Value, FilesProvider.GetFieldDataTypeCode(Value.GetType()));
		}

		internal static string ToString(object Value, uint TypeCode)
		{
			if (TypeCode == ObjectSerializer.TYPE_NULL)
				return string.Empty;
			else if (TypeCode == ObjectSerializer.TYPE_DOUBLE || TypeCode == ObjectSerializer.TYPE_DECIMAL)
				return Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
			else if (TypeCode == ObjectSerializer.TYPE_DATETIME)
				return ((DateTime)Value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "z";
			else if (TypeCode == ObjectSerializer.TYPE_BYTEARRAY)
				return Convert.ToBase64String((byte[])Value);
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
					Value = ((DateTime)Value).ToUniversalTime();
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

			if (!(Value1 is IComparable Comparable))
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
						Value = (byte)2;
					else
						Value = true;

					return true;

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
					{
						double d = ulong.MaxValue;
						if (Increment(ref d))
						{
							Value = d;
							return true;
						}
						else
							return false;
					}
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
						Value = new DateTime(DT.Ticks + 1, DT.Kind);
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
						Value = char.MaxValue + 1;
					else
						Value = (char)(ch + 1);
					return true;

				case ObjectSerializer.TYPE_STRING:
					string s = (string)Value;
					if (Increment(ref s))
					{
						Value = s;
						return true;
					}
					else
						return false;

				case ObjectSerializer.TYPE_ENUM:
					s = Value.ToString();
					if (Increment(ref s))
					{
						Value = s;
						return true;
					}
					else
						return false;

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
			if (Value == 0)
			{
				Value = double.Epsilon;
				return true;
			}

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
			if (Value == 0)
			{
				Value = float.Epsilon;
				return true;
			}

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
			if (Value == 0)
			{
				Value = DecimalEpsilon;
				return true;
			}

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

		/// <summary>
		/// Smallest value represented by the <see cref="System.Decimal"/> data type.
		/// </summary>
		public static readonly decimal DecimalEpsilon = new decimal(1, 0, 0, false, 28);

		/// <summary>
		/// Increments <paramref name="Value"/> to the smallest value greater than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Increment(ref string Value)
		{
			Value += "\x00";
			return true;
		}

		/// <summary>
		/// Decrements <paramref name="Value"/> to the largest value smaller than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Decrement(ref object Value)
		{
			if (Value == null)
				return false;

			Type T = Value.GetType();
			uint TypeCode = FilesProvider.GetFieldDataTypeCode(T);

			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					if (!(bool)Value)
						Value = (sbyte)-1;
					else
						Value = false;

					return true;

				case ObjectSerializer.TYPE_BYTE:
					byte b = (byte)Value;
					if (b == byte.MinValue)
						Value = (sbyte)-1;
					else
					{
						b--;
						Value = b;
					}
					return true;

				case ObjectSerializer.TYPE_INT16:
					short i16 = (short)Value;
					if (i16 == short.MinValue)
						Value = short.MinValue - 1;
					else
					{
						i16--;
						Value = i16;
					}
					return true;

				case ObjectSerializer.TYPE_INT32:
					int i32 = (int)Value;
					if (i32 == int.MinValue)
						Value = (long)int.MinValue - 1;
					else
					{
						i32--;
						Value = i32;
					}
					return true;

				case ObjectSerializer.TYPE_INT64:
					long i64 = (long)Value;
					if (i64 == long.MinValue)
					{
						double d = long.MinValue;
						if (Decrement(ref d))
						{
							Value = d;
							return true;
						}
						else
							return false;
					}
					else
					{
						i64--;
						Value = i64;
					}
					return true;

				case ObjectSerializer.TYPE_SBYTE:
					sbyte i8 = (sbyte)Value;
					if (i8 == sbyte.MinValue)
						Value = (short)sbyte.MinValue - 1;
					else
					{
						i8--;
						Value = i8;
					}
					return true;

				case ObjectSerializer.TYPE_UINT16:
					ushort ui16 = (ushort)Value;
					if (ui16 == ushort.MinValue)
						Value = (short)ushort.MinValue - 1;
					else
					{
						ui16--;
						Value = ui16;
					}
					return true;

				case ObjectSerializer.TYPE_UINT32:
					uint ui32 = (uint)Value;
					if (ui32 == uint.MinValue)
						Value = (int)uint.MinValue - 1;
					else
					{
						ui32--;
						Value = ui32;
					}
					return true;

				case ObjectSerializer.TYPE_UINT64:
					ulong ui64 = (ulong)Value;
					if (ui64 == ulong.MinValue)
						Value = (long)ulong.MinValue - 1;
					else
					{
						ui64--;
						Value = ui64;
					}
					return true;

				case ObjectSerializer.TYPE_DECIMAL:
					decimal dec = (decimal)Value;
					if (!Decrement(ref dec))
						return false;
					else
					{
						Value = dec;
						return true;
					}

				case ObjectSerializer.TYPE_DOUBLE:
					double dbl = (double)Value;
					if (!Decrement(ref dbl))
						return false;
					else
					{
						Value = dbl;
						return true;
					}

				case ObjectSerializer.TYPE_SINGLE:
					float sng = (float)Value;
					if (!Decrement(ref sng))
						return false;
					else
					{
						Value = sng;
						return true;
					}

				case ObjectSerializer.TYPE_DATETIME:
					DateTime DT = (DateTime)Value;
					if (DT.Ticks == long.MinValue)
						return false;
					else
					{
						Value = new DateTime(DT.Ticks - 1, DT.Kind);
						return true;
					}

				case ObjectSerializer.TYPE_TIMESPAN:
					TimeSpan TS = (TimeSpan)Value;
					if (TS.Ticks == long.MinValue)
						return false;
					else
					{
						Value = new TimeSpan(TS.Ticks - 1);
						return true;
					}

				case ObjectSerializer.TYPE_CHAR:
					char ch = (char)Value;
					if (ch == char.MinValue)
						Value = char.MinValue - 1;
					else
						Value = (char)(ch - 1);

					return true;

				case ObjectSerializer.TYPE_STRING:
					string s = (string)Value;
					if (Decrement(ref s))
					{
						Value = s;
						return true;
					}
					else
						return false;

				case ObjectSerializer.TYPE_ENUM:
					s = Value.ToString();
					if (Decrement(ref s))
					{
						Value = s;
						return true;
					}
					else
						return false;

				case ObjectSerializer.TYPE_GUID:
					Guid Guid = (Guid)Value;
					byte[] A = Guid.ToByteArray();

					A[15]--;
					if (A[15] == 0xff)
					{
						A[14]--;
						if (A[14] == 0xff)
						{
							A[13]--;
							if (A[13] == 0xff)
							{
								A[12]--;
								if (A[12] == 0xff)
								{
									A[11]--;
									if (A[11] == 0xff)
									{
										A[10]--;
										if (A[10] == 0xff)
										{
											A[9]--;
											if (A[9] == 0xff)
											{
												A[8]--;
												if (A[8] == 0xff)
												{
													A[6]--;
													if (A[6] == 0xff)
													{
														A[7]--;
														if (A[7] == 0xff)
														{
															A[4]--;
															if (A[4] == 0xff)
															{
																A[5]--;
																if (A[5] == 0xff)
																{
																	A[0]--;
																	if (A[0] == 0xff)
																	{
																		A[1]--;
																		if (A[1] == 0xff)
																		{
																			A[2]--;
																			if (A[2] == 0xff)
																			{
																				A[3]--;
																				if (A[3] == 0xff)
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
		/// Decrements <paramref name="Value"/> to the largest value smaller than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Decrement(ref double Value)
		{
			if (Value == 0)
			{
				Value = -double.Epsilon;
				return true;
			}

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
		/// Decrements <paramref name="Value"/> to the largest value smaller than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Decrement(ref float Value)
		{
			if (Value == 0)
			{
				Value = -float.Epsilon;
				return true;
			}

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
		/// Decrements <paramref name="Value"/> to the largest value smaller than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Decrement(ref decimal Value)
		{
			if (Value == 0)
			{
				Value = -DecimalEpsilon;
				return true;
			}

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

		/// <summary>
		/// Decrements <paramref name="Value"/> to the largest value smaller than <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>If operation was successful.</returns>
		public static bool Decrement(ref string Value)
		{
			if (string.IsNullOrEmpty(Value))
				return false;

			int c = Value.Length;
			char ch = Value[c - 1];

			if (ch == 0)
				Value = Value.Substring(0, c - 1);
			else
				Value = Value.Substring(0, c - 1) + ((char)(ch - 1)) + char.MaxValue;

			return true;
		}

	}
}
