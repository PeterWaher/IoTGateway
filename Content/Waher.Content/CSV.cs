using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content
{
	/// <summary>
	/// Helps with common CSV-related tasks.
	/// </summary>
	public static class CSV
	{
		#region Encoding/Decoding

		/// <summary>
		/// Parses a CSV string.
		/// </summary>
		/// <param name="Csv">CSV</param>
		/// <returns>Parsed content.</returns>
		public static string[][] Parse(string Csv)
		{
			int Pos = 0;
			int Len = Csv.Length;
			string[][] Result = Parse(Csv, ref Pos, Len);
			char ch;

			while (Pos < Len && ((ch = Csv[Pos]) <= ' ' || ch == 160))
				Pos++;

			if (Pos < Len)
				throw new Exception("Unexpected content at end of string.");

			return Result;
		}

		private static string[][] Parse(string Csv, ref int Pos, int Len)
		{
			List<string[]> Records = new List<string[]>();
			List<string> Fields = new List<string>();
			StringBuilder sb = new StringBuilder();
			int State = 0;
			int i = 0;
			char ch;
			bool sbEmpty = true;

			while (Pos < Len)
			{
				ch = Csv[Pos++];
				switch (State)
				{
					case 0:
						if (ch == '"')
							State += 2;
						else if (ch == ',')
							Fields.Add(string.Empty);
						else if (ch == '\r' || ch == '\n')
						{
							if (Fields.Count > 0)
							{
								Records.Add(Fields.ToArray());
								Fields.Clear();
							}
						}
						else
						{
							sb.Append(ch);
							sbEmpty = false;
							State++;
						}
						break;

					case 1: // Undelimited string
						if (ch == ',')
						{
							Fields.Add(sb.ToString());
							sb.Clear();
							sbEmpty = true;
							State = 0;
						}
						else if (ch == '\r' || ch == '\n')
						{
							Fields.Add(sb.ToString());
							sb.Clear();
							sbEmpty = true;
							State = 0;

							Records.Add(Fields.ToArray());
							Fields.Clear();
						}
						else
						{
							sb.Append(ch);
							sbEmpty = false;
						}
						break;

					case 2: // String.
						if (ch == '\\')
							State++;
						else if (ch == '"')
							State--;
						else
						{
							sb.Append(ch);
							sbEmpty = false;
						}
						break;

					case 3: // String, escaped character.
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

						sbEmpty = false;
						State--;
						break;

					case 4: // hex digit 1(4)
						i = JSON.HexDigit(ch);
						State++;
						break;

					case 5: // hex digit 2(4)
						i <<= 4;
						i |= JSON.HexDigit(ch);
						State++;
						break;

					case 6: // hex digit 3(4)
						i <<= 4;
						i |= JSON.HexDigit(ch);
						State++;
						break;

					case 7: // hex digit 4(4)
						i <<= 4;
						i |= JSON.HexDigit(ch);
						sb.Append((char)i);
						sbEmpty = false;
						State -= 5;
						break;
				}
			}

			if (!sbEmpty)
				Fields.Add(sb.ToString());

			if (Fields.Count > 0)
				Records.Add(Fields.ToArray());

			return Records.ToArray();
		}

		#endregion
	}
}
