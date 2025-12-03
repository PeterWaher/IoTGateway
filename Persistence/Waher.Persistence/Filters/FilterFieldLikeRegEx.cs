using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : FilterField
	{
		private readonly string regularExpression;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName)
		{
			this.regularExpression = RegularExpression;
		}

		/// <summary>
		/// Regular expression.
		/// </summary>
		public string RegularExpression
		{
			get
			{
				return this.regularExpression;
			}
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inverse of the filter.</returns>
		public override Filter Negate()
		{
			return new FilterNot(this.Copy());
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterFieldLikeRegEx(this.FieldName, this.regularExpression);
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.Copy();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.FieldName + " LIKE " + this.regularExpression.ToString();
		}

		/// <summary>
		/// Performs a comparison on the object with the field value <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Field value for comparison.</param>
		/// <returns>Result of comparison.</returns>
		public override bool Compare(object Value)
		{
			if (Value is null)
				return false;

			string s = Value?.ToString();

			if (this.regex is null)
				this.regex = new Regex(this.regularExpression, RegexOptions.Singleline);

			Match M = this.regex.Match(s);

			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		private Regex regex = null;

		/// <summary>
		/// Gets any constant prefix from a regular expression. This prefix
		/// </summary>
		/// <param name="RegularExpression">String-representation of regular expression.</param>
		/// <param name="Regex">Parsed version of regular expression.</param>
		/// <returns>Contant prefix.</returns>
		public static string GetRegExConstantPrefix(string RegularExpression, Regex Regex)
		{
			StringBuilder Result = new StringBuilder();
			int[] GroupNumbers = null;
			int i, j, k, l, c = RegularExpression.Length;
			char ch;

			for (i = 0; i < c; i++)
			{
				ch = RegularExpression[i];
				if (ch == '\\')
				{
					i++;
					if (i < c)
					{
						ch = RegularExpression[i];

						switch (ch)
						{
							case 'a':
								Result.Append('\a');
								break;

							case 'b':
								Result.Append('\b');
								break;

							case 't':
								Result.Append('\t');
								break;

							case 'r':
								Result.Append('\r');
								break;

							case 'v':
								Result.Append('\v');
								break;

							case 'f':
								Result.Append('\f');
								break;

							case 'n':
								Result.Append('\n');
								break;

							case '.':
							case '$':
							case '^':
							case '{':
							case '[':
							case '(':
							case '|':
							case ')':
							case '*':
							case '+':
							case '?':
							case '\\':
								Result.Append(ch);
								break;

							case 'e':
								Result.Append('\u001B');
								break;

							case 'c':
								i++;
								if (i < c)
								{
									ch = RegularExpression[i++];

									if (ch == '@')
										Result.Append((char)ch);
									else if (ch >= 'A' && ch <= 'Z')
										Result.Append((char)(ch - 64));
									else
									{
										switch (ch)
										{
											case '[': Result.Append((char)27); break;
											case '\\': Result.Append((char)28); break;
											case ']': Result.Append((char)29); break;
											case '^': Result.Append((char)30); break;
											case '_': Result.Append((char)31); break;
											default: return Result.ToString();
										}
									}
								}
								else
									return Result.ToString();

								break;

							case 'x':
								i++;
								if (i + 2 <= c && int.TryParse(RegularExpression.Substring(i, 2), NumberStyles.HexNumber, null, out j))
								{
									i += 2;
									Result.Append((char)j);
								}
								else
									return Result.ToString();
								break;

							case 'u':
								i++;
								if (i + 4 <= c && int.TryParse(RegularExpression.Substring(i, 4), NumberStyles.HexNumber, null, out j))
								{
									i += 4;
									Result.Append((char)j);
								}
								else
									return Result.ToString();
								break;

							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
								j = i++;
								while ((k = i - j) < 3 && i < c && (ch = RegularExpression[i]) >= '0' && ch <= '9')
									i++;

								if (k == 1)
									return Result.ToString();

								l = int.Parse(RegularExpression.Substring(j, k));

								if (GroupNumbers is null)
									GroupNumbers = Regex.GetGroupNumbers();

								if (Array.IndexOf(GroupNumbers, l) >= 0)
									return Result.ToString();

								k = 0;
								j = 0;
								while (l > 0)
								{
									k += (l % 10) << j;
									j += 3;
									l /= 10;
								}

								Result.Append((char)k);
								break;

							default:
								return Result.ToString();
						}
					}
				}
				else if (ch == '|')
					return string.Empty;
				else if (ch == '?' || ch == '*')
				{
					j = Result.Length;
					if (j <= 1)
						return string.Empty;
					else
						return Result.ToString().Substring(0, j - 1);
				}
				else if (".$^{[()+".IndexOf(ch) >= 0)
					return Result.ToString();
				else
					Result.Append(ch);
			}

			return Result.ToString();
		}

	}
}
