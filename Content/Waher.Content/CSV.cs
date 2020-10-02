using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Comparisons;

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

		/// <summary>
		/// Encodes records as a Comma-separated values string.
		/// </summary>
		/// <param name="Records">Records</param>
		/// <returns>CSV-string</returns>
		public static string Encode(string[][] Records)
		{
			StringBuilder sb = new StringBuilder();
			bool First;

			foreach (string[] Record in Records)
			{
				First = true;

				foreach (string Field in Record)
				{
					bool Comma = false;
					bool Control = false;
					bool Quote = false;

					if (First)
						First = false;
					else
						sb.Append(',');

					if (Field is null)
						continue;

					foreach (char ch in Field)
					{
						if (ch == ',')
							Comma = true;
						else if (ch == '"')
							Quote = true;
						else if (ch < ' ')
							Control = true;
					}

					if (Comma || Quote || Control)
					{
						string Escaped = Field;

						if (Quote)
							Escaped = Escaped.Replace("\"", "\\\"");

						if (Control)
						{
							Escaped = Escaped.
								Replace("\a", "\\a").
								Replace("\b", "\\b").
								Replace("\f", "\\f").
								Replace("\n", "\\n").
								Replace("\r", "\\r").
								Replace("\t", "\\t").
								Replace("\v", "\\v");
						}

						sb.Append('"');
						sb.Append(Escaped);
						sb.Append('"');
					}
					else
						sb.Append(Field);
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Encodes a matrix as a Comma-separated values string.
		/// </summary>
		/// <param name="Matrix">Matrix</param>
		/// <returns>CSV-string</returns>
		public static string Encode(IMatrix Matrix)
		{
			List<string[]> Records = new List<string[]>();
			List<string> Fields = new List<string>();

			if (Matrix is ObjectMatrix M)
			{
				if (!(M.ColumnNames is null))
					Records.Add(M.ColumnNames);
			}

			int Row, NrRows = Matrix.Rows;
			int Column, NrColumns = Matrix.Columns;
			IElement E;

			for (Row = 0; Row < NrRows; Row++)
			{
				for (Column = 0; Column < NrColumns; Column++)
				{
					E = Matrix.GetElement(Column, Row);

					if (E is StringValue S)
						Fields.Add(S.Value);
					else if (E is DoubleNumber D)
						Fields.Add(CommonTypes.Encode(D.Value));
					else
						Fields.Add(E.AssociatedObjectValue?.ToString());
				}

				Records.Add(Fields.ToArray());
				Fields.Clear();
			}

			return Encode(Records.ToArray());
		}

		#endregion
	}
}
