using System;
using System.Text;
using System.Xml;
using Waher.Script.Model;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.Xml
{
	/// <summary>
	/// Parses an XML document
	/// </summary>
	public class XmlParser : IKeyWord
	{
		/// <summary>
		/// Parses an XML document
		/// </summary>
		public XmlParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "<";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[0];

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			StringBuilder Fragment = new StringBuilder();
			int State = 0;
			int Depth = 0;
			char ch;

			Result = null;

			while ((ch = Parser.NextChar()) != 0)
			{
				switch (State)
				{
					case 0:     // Waiting for first <
						if (ch == '<')
						{
							Fragment.Append(ch);
							State++;
						}
						else if (ch > ' ')
							return false;
						break;

					case 1:     // Waiting for ? or >
						Fragment.Append(ch);
						if (ch == '?')
							State++;
						else if (ch == '>')
						{
							State = 5;
							Depth = 1;
						}
						break;

					case 2:     // In processing instruction. Waiting for ?>
						Fragment.Append(ch);
						if (ch == '>')
							State = 5;
						break;

					// 3
					// 4

					case 5: // Waiting for start element.
						if (ch == '<')
						{
							Fragment.Append(ch);
							State++;
						}
						else if (Depth > 1)
							Fragment.Append(ch);
						else if (ch > ' ')
							return false;
						break;

					case 6: // Second character in tag
						Fragment.Append(ch);
						if (ch == '/')
							State++;
						else if (ch == '!')
							State = 13;
						else
							State += 2;
						break;

					case 7: // Waiting for end of closing tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							Depth--;
							if (Depth < 0)
								return false;
							else
							{
								if (Depth == 0)
									break;

								if (State > 0)
									State = 5;
							}
						}
						break;

					case 8: // Wait for end of start tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							Depth++;
							State = 5;
						}
						else if (ch == '/')
							State++;
						else if (ch <= ' ')
							State += 2;
						break;

					case 9: // Check for end of childless tag.
						Fragment.Append(ch);
						if (ch == '>')
						{
							if (Depth == 0)
								break;

							if (State != 0)
								State = 5;
						}
						else
							State--;
						break;

					case 10:    // Check for attributes.
						Fragment.Append(ch);
						if (ch == '>')
						{
							Depth++;
							State = 5;
						}
						else if (ch == '/')
							State--;
						else if (ch == '"')
							State++;
						else if (ch == '\'')
							State += 2;
						break;

					case 11:    // Double quote attribute.
						Fragment.Append(ch);
						if (ch == '"')
							State--;
						break;

					case 12:    // Single quote attribute.
						Fragment.Append(ch);
						if (ch == '\'')
							State -= 2;
						break;

					case 13:    // Third character in start of comment
						Fragment.Append(ch);
						if (ch == '-')
							State++;
						else if (ch == '[')
							State = 18;
						else
							return false;
						break;

					case 14:    // Fourth character in start of comment
						Fragment.Append(ch);
						if (ch == '-')
							State++;
						else
							return false;
						break;

					case 15:    // In comment
						Fragment.Append(ch);
						if (ch == '-')
							State++;
						break;

					case 16:    // Second character in end of comment
						Fragment.Append(ch);
						if (ch == '-')
							State++;
						else
							State--;
						break;

					case 17:    // Third character in end of comment
						Fragment.Append(ch);
						if (ch == '>')
							State = 5;
						else
							State -= 2;
						break;

					case 18:    // Fourth character in start of CDATA
						Fragment.Append(ch);
						if (ch == 'C')
							State++;
						else
							return false;
						break;

					case 19:    // Fifth character in start of CDATA
						Fragment.Append(ch);
						if (ch == 'D')
							State++;
						else
							return false;
						break;

					case 20:    // Sixth character in start of CDATA
						Fragment.Append(ch);
						if (ch == 'A')
							State++;
						else
							return false;
						break;

					case 21:    // Seventh character in start of CDATA
						Fragment.Append(ch);
						if (ch == 'T')
							State++;
						else
							return false;
						break;

					case 22:    // Eighth character in start of CDATA
						Fragment.Append(ch);
						if (ch == 'A')
							State++;
						else
							return false;
						break;

					case 23:    // Ninth character in start of CDATA
						Fragment.Append(ch);
						if (ch == '[')
							State++;
						else
							return false;
						break;

					case 24:    // In CDATA
						Fragment.Append(ch);
						if (ch == ']')
							State++;
						break;

					case 25:    // Second character in end of CDATA
						Fragment.Append(ch);
						if (ch == ']')
							State++;
						else
							State--;
						break;

					case 26:    // Third character in end of CDATA
						Fragment.Append(ch);
						if (ch == '>')
							State = 5;
						else
							State -= 2;
						break;

					default:
						break;
				}
			}

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Fragment.ToString());

			Result = new ConstantElement(new ObjectValue(Doc), Parser.Start, Parser.Length, Parser.Expression);

			return true;
		}

	}
}
