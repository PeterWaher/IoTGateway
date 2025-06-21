﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Json;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content
{
	/// <summary>
	/// Helps with common JSON-related tasks.
	/// </summary>
	public static class JSON
	{
		/// <summary>
		/// Unix Date and Time epoch, starting at 1970-01-01T00:00:00Z
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private static readonly Dictionary<Type, IJsonEncoder> encoders;

		static JSON()
		{
			encoders = new Dictionary<Type, IJsonEncoder>();
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			lock (encoders)
			{
				encoders.Clear();
			}
		}

		#region Parsing

		/// <summary>
		/// Parses a JSON string.
		/// </summary>
		/// <param name="Json">JSON</param>
		/// <returns>Parsed content.</returns>
		public static object Parse(string Json)
		{
			int Pos = 0;
			int Len = Json.Length;
			object Result = Parse(Json, ref Pos, Len, false);
			char ch;

			while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
				Pos++;

			if (Pos < Len)
				throw new Exception("Unexpected content at end of string.");

			return Result;
		}

		private static object Parse(string Json, ref int Pos, int Len, bool AllowLabel)
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
							{
								Pos++;
								return System.Array.Empty<object>();
							}

							ChunkedList<object> Array = new ChunkedList<object>();

							while (true)
							{
								Array.Add(JSON.Parse(Json, ref Pos, Len, false));

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
								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (!(Parse(Json, ref Pos, Len, true) is string Key))
									throw new Exception("Expected member name.");

								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (Pos >= Len)
									throw new Exception("Unexpected end of JSON.");

								if (ch != ':')
									throw new Exception("Expected :");

								Pos++;

								Object[Key] = JSON.Parse(Json, ref Pos, Len, false);

								while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
									Pos++;

								if (Pos >= Len)
									throw new Exception("Unexpected end of JSON.");

								if (ch == '}')
									break;
								else if (ch == ',')
								{
									Pos++;

									while (Pos < Len && ((ch = Json[Pos]) <= ' ' || ch == 160))
										Pos++;

									if (Pos >= Len)
										throw new Exception("Unexpected end of JSON.");

									if (ch == '}')
										break;
								}
								else
									throw new Exception("Invalid JSON.");
							}

							Pos++;
							return Object;
						}
						else if (AllowLabel && (char.IsLetter(ch) || ch == '_' || ch == '$'))
						{
							sb = new StringBuilder();
							sb.Append(ch);
							State += 21;
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

					case 21: // Label.
						if (char.IsLetter(ch) || char.IsDigit(ch) || ch == '_' || ch == '$')
							sb.Append(ch);
						else
						{
							Pos--;
							return sb.ToString();
						}
						break;
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

		#endregion

		#region Encoding

		/// <summary>
		/// Encodes a string for inclusion in JSON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s)
		{
			return CommonTypes.Escape(s, jsonCharactersToEscape, jsonCharacterEscapes);
		}

		private static readonly char[] jsonCharactersToEscape = new char[] 
		{
			'\\', 
			'"',
			'\x00',
			'\x01',
			'\x02',
			'\x03',
			'\x04',
			'\x05',
			'\x06',
			'\a',	// 7  - 0x07
			'\b',	// 8  - 0x08
			'\t',	// 9  - 0x09
			'\n',	// 10 - 0x0a
			'\v',	// 11 - 0x0b
			'\f',	// 12 - 0x0c
			'\r',	// 13 - 0x0d
			'\x0e',
			'\x0f',
			'\x10',
			'\x11',
			'\x12',
			'\x13',
			'\x14',
			'\x15',
			'\x16',
			'\x17',
			'\x18',
			'\x19',
			'\x1a',
			'\x1b',
			'\x1c',
			'\x1d',
			'\x1e',
			'\x1f'
		};
		private static readonly string[] jsonCharacterEscapes = new string[]
		{ 
			"\\\\", 
			"\\\"",
			"\\u0000",
			"\\u0001",
			"\\u0002",
			"\\u0003",
			"\\u0004",
			"\\u0005",
			"\\u0006",
			"\\a",
			"\\b",
			"\\t",
			"\\n",
			"\\v",
			"\\f",
			"\\r",
			"\\u000e",
			"\\u000f",
			"\\u0010",
			"\\u0011",
			"\\u0012",
			"\\u0013",
			"\\u0014",
			"\\u0015",
			"\\u0016",
			"\\u0017",
			"\\u0018",
			"\\u0019",
			"\\u001a",
			"\\u001b",
			"\\u001c",
			"\\u001d",
			"\\u001e",
			"\\u001f"
		};

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

		/// <summary>
		/// Encodes an object as JSON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If JSON should be indented.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		/// <returns>Encoded object.</returns>
		public static string Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			StringBuilder Json = new StringBuilder();
			Encode(Object, Indent, Json, AdditionalProperties);
			return Json.ToString();
		}

		/// <summary>
		/// Encodes an object as JSON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If JSON should be indented.</param>
		/// <param name="Json">JSON Output.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent, StringBuilder Json,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			bool First = true;

			Json.Append('{');

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, object> Member in Object)
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
			}

			if (!(AdditionalProperties is null))
			{
				foreach (KeyValuePair<string, object> Member in AdditionalProperties)
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
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}
			}

			Json.Append('}');
		}

		/// <summary>
		/// Encodes an object as JSON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If JSON should be indented.</param>
		/// <param name="Json">JSON Output.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, IElement>> Object, int? Indent, StringBuilder Json)
		{
			bool First = true;

			Json.Append('{');

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, IElement> Member in Object)
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

					Encode(Member.Value.AssociatedObjectValue, Indent, Json);
				}
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}
			}

			Json.Append('}');
		}

		/// <summary>
		/// Extensible encoding of object <paramref name="Object"/>, by using
		/// best available <see cref="IJsonEncoder"/> for the corresponding type.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="Json">JSON output.</param>
		public static void Encode(object Object, int? Indent, StringBuilder Json)
		{
			if (Object is null)
				Json.Append("null");
			else
			{
				Type T = Object.GetType();
				IJsonEncoder Encoder;

				lock (encoders)
				{
					if (!encoders.TryGetValue(T, out Encoder))
					{
						Encoder = Types.FindBest<IJsonEncoder, Type>(T)
							?? throw new ArgumentException("Unable to encode objects of type " + T.FullName, nameof(Object));

						encoders[T] = Encoder;
					}
				}

				Encoder.Encode(Object, Indent, Json);
			}
		}

		#endregion

		#region Pretty JSON

		/// <summary>
		/// Reformats JSON to make it easier to read.
		/// </summary>
		/// <param name="Json">JSON</param>
		/// <returns>Reformatted JSON.</returns>
		public static string PrettyJson(string Json)
		{
			return Encode(Parse(Json), true);
		}

		/// <summary>
		/// Reformats JSON to make it easier to read.
		/// </summary>
		/// <param name="Object">Object to encode into pretty JSON</param>
		/// <returns>Reformatted JSON.</returns>
		public static string PrettyJson(object Object)
		{
			return Encode(Object, true);
		}

		#endregion
	}
}
