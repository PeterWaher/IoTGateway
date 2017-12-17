using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content
{
	/// <summary>
	/// Helps with common JSON-related tasks.
	/// </summary>
	public static class JSON
	{
		#region Encoding/Decoding

		/// <summary>
		/// Parses a JSON string.
		/// </summary>
		/// <param name="Json">JSON</param>
		/// <returns>Parsed content.</returns>
		public static object Parse(string Json)
		{
			int Pos = 0;
			int Len = Json.Length;
			object Result = Parse(Json, ref Pos, Len);
			char ch;

			while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
				Pos++;

			if (Pos < Len)
				throw new Exception("Unexpected content at end of string.");

			return Result;
		}

		private static object Parse(string Json, ref int Pos, int Len)
		{
			StringBuilder sb = null;
			int State = 0;
			int Start = 0;
			int i = 0;
			char ch;

			while (Pos < Len)
			{
				ch = Json[Pos++];
				switch (State)
				{
					case 0:
						if (ch <= ' ' || ch == 160)
							break;

						if ((ch >= '0' && ch <= '9') || (ch == '-') || (ch == '+'))
						{
							Start = Pos - 1;
							State++;
						}
						else if (ch == '.')
						{
							Start = Pos - 1;
							State += 2;
						}
						else if (ch == '"')
						{
							sb = new StringBuilder();
							State += 5;
						}
						else if (ch == 't')
							State += 11;
						else if (ch == 'f')
							State += 14;
						else if (ch == 'n')
							State += 18;
						else if (ch == '[')
						{
							while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
								Pos++;

							if (Pos >= Len)
								throw new Exception("Unexpected end of JSON.");

							if (ch == ']')
								return new object[0];

							List<object> Array = new List<object>();

							while (true)
							{
								Array.Add(JSON.Parse(Json, ref Pos, Len));

								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (Pos >= Len)
									throw new Exception("Unexpected end of JSON.");

								if (ch == ']')
									break;
								else if (ch == ',')
									Pos++;
								else
									throw new Exception("Invalid JSON.");
							}

							Pos++;
							return Array.ToArray();
						}
						else if (ch == '{')
						{
							Dictionary<string, object> Object = new Dictionary<string, object>();

							while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
								Pos++;

							if (Pos >= Len)
								throw new Exception("Unexpected end of JSON.");

							if (ch == '}')
							{
								Pos++;
								return Object;
							}

							while (true)
							{
								string Key = JSON.Parse(Json, ref Pos, Len) as string;
								if (Key == null)
									throw new Exception("Expected member name.");

								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (Pos >= Len)
									throw new Exception("Unexpected end of JSON.");

								if (ch != ':')
									throw new Exception("Expected :");

								Pos++;

								Object[Key] = JSON.Parse(Json, ref Pos, Len);

								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (Pos >= Len)
									throw new Exception("Unexpected end of JSON.");

								if (ch == '}')
									break;
								else if (ch == ',')
									Pos++;
								else
									throw new Exception("Invalid JSON.");
							}

							Pos++;
							return Object;
						}
						else
							throw new Exception("Invalid JSON.");
						break;

					case 1: // Number (Integer?)
						if (ch >= '0' && ch <= '9')
							break;
						else if (ch == '.')
							State++;
						else if (ch == 'e' || ch == 'E')
							State += 2;
						else
						{
							Pos--;
							string s = Json.Substring(Start, Pos - Start);
							if (int.TryParse(s, out i))
								return i;
							else if (long.TryParse(s, out long l))
								return l;
							else if (CommonTypes.TryParse(s, out double d))
								return d;
							else if (CommonTypes.TryParse(s, out decimal dec))
								return dec;
							else
								throw new Exception("Invalid JSON.");
						}
						break;

					case 2: // Decimal number, decimal part.
						if (ch >= '0' && ch <= '9')
							break;
						else if (ch == 'e' || ch == 'E')
							State++;
						else
						{
							Pos--;
							string s = Json.Substring(Start, Pos - Start);
							if (CommonTypes.TryParse(s, out double d))
								return d;
							else if (CommonTypes.TryParse(s, out decimal dec))
								return dec;
							else
								throw new Exception("Invalid JSON.");
						}
						break;

					case 3: // Decimal number, exponent sign.
						if (ch == '+' || ch == '-' || (ch >= '0' && ch <= '9'))
							State++;
						else
							throw new Exception("Invalid JSON.");
						break;

					case 4: // Decimal number, exponent.
						if (ch >= '0' && ch <= '9')
							break;
						else
						{
							Pos--;
							string s = Json.Substring(Start, Pos - Start);
							if (CommonTypes.TryParse(s, out double d))
								return d;
							else if (CommonTypes.TryParse(s, out decimal dec))
								return dec;
							else
								throw new Exception("Invalid JSON.");
						}

					case 5: // String.
						if (ch == '\\')
							State++;
						else if (ch == '"')
							return sb.ToString();
						else
							sb.Append(ch);
						break;

					case 6: // String, escaped character.
						switch (ch)
						{
							case 'a':
								sb.Append('\a');
								break;

							case 'b':
								sb.Append('\b');
								break;

							case 'f':
								sb.Append('\f');
								break;

							case 'n':
								sb.Append('\n');
								break;

							case 'r':
								sb.Append('\r');
								break;

							case 't':
								sb.Append('\t');
								break;

							case 'v':
								sb.Append('\v');
								break;

							case 'x':
								i = 0;
								State += 4;
								break;

							case 'u':
								i = 0;
								State += 2;
								break;

							default:
								sb.Append(ch);
								break;
						}

						State--;
						break;

					case 7: // hex digit 1(4)
						i = HexDigit(ch);
						State++;
						break;

					case 8: // hex digit 2(4)
						i <<= 4;
						i |= HexDigit(ch);
						State++;
						break;

					case 9: // hex digit 3(4)
						i <<= 4;
						i |= HexDigit(ch);
						State++;
						break;

					case 10: // hex digit 4(4)
						i <<= 4;
						i |= HexDigit(ch);
						sb.Append((char)i);
						State -= 5;
						break;

					case 11: // True
						if (ch == 'r')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 12: // tRue
						if (ch == 'u')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 13: // trUe
						if (ch == 'e')
							return true;
						else
							throw new Exception("Invalid JSON.");

					case 14: // False
						if (ch == 'a')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 15: // fAlse
						if (ch == 'l')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 16: // faLse
						if (ch == 's')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 17: // falsE
						if (ch == 'e')
							return false;
						else
							throw new Exception("Invalid JSON.");

					case 18: // Null
						if (ch == 'u')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 19: // nUll
						if (ch == 'l')
						{
							State++;
							break;
						}
						else
							throw new Exception("Invalid JSON.");

					case 20: // nuLl
						if (ch == 'l')
							return null;
						else
							throw new Exception("Invalid JSON.");
				}
			}

			if (State == 1)
			{
				string s = Json.Substring(Start, Pos - Start);
				if (int.TryParse(s, out i))
					return i;
				else if (long.TryParse(s, out long l))
					return l;
				else if (CommonTypes.TryParse(s, out double d))
					return d;
				else if (CommonTypes.TryParse(s, out decimal dec))
					return dec;
				else
					throw new Exception("Invalid JSON.");
			}
			else if (State == 2)
			{
				string s = Json.Substring(Start, Pos - Start);
				if (CommonTypes.TryParse(s, out double d))
					return d;
				else if (CommonTypes.TryParse(s, out decimal dec))
					return dec;
				else
					throw new Exception("Invalid JSON.");
			}
			else if (State == 4)
			{
				string s = Json.Substring(Start, Pos - Start);
				if (CommonTypes.TryParse(s, out double d))
					return d;
				else if (CommonTypes.TryParse(s, out decimal dec))
					return dec;
				else
					throw new Exception("Invalid JSON.");
			}
			else
				throw new Exception("Unexpected end of JSON.");
		}

		internal static int HexDigit(char ch)
		{
			if (ch >= '0' && ch <= '9')
				return ch - '0';
			else if (ch >= 'A' && ch <= 'F')
				return ch - 'A' + 10;
			else if (ch >= 'a' && ch <= 'f')
				return ch - 'a' + 10;
			else
				throw new Exception("Invalid hexadecimal digit.");
		}

		/// <summary>
		/// Encodes a string for inclusion in JSON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s)
		{
			return CommonTypes.Escape(s, jsonCharactersToEscape, jsonCharacterEscapes);
		}

		private static readonly char[] jsonCharactersToEscape = new char[] { '\\', '"', '\n', '\r', '\t', '\b', '\f', '\a' };
		private static readonly string[] jsonCharacterEscapes = new string[] { "\\\\", "\\\"", "\\n", "\\r", "\\t", "\\b", "\\f", "\\a" };

		/// <summary>
		/// Encodes an object as JSON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If JSON should be indented.</param>
		/// <returns>JSON string.</returns>
		public static string Encode(object Object, bool Indent)
		{
			StringBuilder sb = new StringBuilder();
			Encode(Object, Indent, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Encodes an object as JSON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Json">JSON Output.</param>
		/// <param name="Indent">If JSON should be indented.</param>
		public static void Encode(object Object, bool Indent, StringBuilder Json)
		{
			Encode(Object, Indent ? (int?)0 : null, Json);
		}

		private static void Encode(object Object, int? Indent, StringBuilder Json)
		{
			if (Object == null)
				Json.Append("null");
			else
			{
				Type T = Object.GetType();
				TypeInfo TI = T.GetTypeInfo();

				if (TI.IsValueType)
				{
					if (Object is bool b)
						Json.Append(CommonTypes.Encode(b));
					else if (Object is char ch)
					{
						Json.Append('"');
						Json.Append(Encode(new string(ch, 1)));
						Json.Append('"');
					}
					else if (Object is double dbl)
						Json.Append(CommonTypes.Encode(dbl));
					else if (Object is float fl)
						Json.Append(CommonTypes.Encode(fl));
					else if (Object is decimal dec)
						Json.Append(CommonTypes.Encode(dec));
					else if (TI.IsEnum)
					{
						Json.Append('"');
						Json.Append(Encode(Object.ToString()));
						Json.Append('"');
					}
					else
						Json.Append(Object.ToString());
				}
				else if (Object is string s)
				{
					Json.Append('"');
					Json.Append(Encode(s));
					Json.Append('"');
				}
				else if (Object is Dictionary<string, object> Obj)
				{
					bool First = true;

					Json.Append('{');

					if (Indent.HasValue)
						Indent = Indent + 1;

					foreach (KeyValuePair<string, object> Member in Obj)
					{
						if (First)
							First = false;
						else
							Json.Append(',');

						if (Indent.HasValue)
						{
							Json.AppendLine();
							Json.Append(new string('\t', Indent.Value));
						}

						Json.Append('"');
						Json.Append(Encode(Member.Key));
						Json.Append("\":");

						if (Indent.HasValue)
							Json.Append(' ');

						Encode(Member.Value, Indent, Json);
					}

					if (!First)
					{
						Json.AppendLine();

						if (Indent.HasValue)
							Indent = Indent - 1;

						Json.Append(new string('\t', Indent.Value));
					}

					Json.Append('}');
				}
				else if (Object is IEnumerable E)
				{
					IEnumerator e = E.GetEnumerator();
					bool First = true;

					Json.Append('[');

					if (Indent.HasValue)
						Indent = Indent + 1;

					while (e.MoveNext())
					{
						if (First)
							First = false;
						else
							Json.Append(',');

						if (Indent.HasValue)
						{
							Json.AppendLine();
							Json.Append(new string('\t', Indent.Value));
						}

						Encode(e.Current, Indent, Json);
					}

					if (!First)
					{
						Json.AppendLine();

						if (Indent.HasValue)
							Indent = Indent - 1;

						Json.Append(new string('\t', Indent.Value));
					}

					Json.Append(']');
				}
				else
					throw new ArgumentException("Unsupported type: " + T.FullName, nameof(Object));
			}
		}

		#endregion
	}
}
