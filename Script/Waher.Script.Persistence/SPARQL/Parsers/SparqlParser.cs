using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SPARQL.Parsers
{
	/// <summary>
	/// Parses a SPARQL statement
	/// </summary>
	public class SparqlParser : IKeyWord
	{
		/// <summary>
		/// Reference instance of SPARQL parser.
		/// </summary>
		public static readonly SparqlParser RefInstance = new SparqlParser(string.Empty);

		private readonly string preamble;
		private readonly int preambleLen;
		private readonly List<SemanticQueryTriple> triples = new List<SemanticQueryTriple>();
		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>()
		{
			{ "xsd", "http://www.w3.org/2001/XMLSchema#" },
			{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
			{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
			{ "fn", "http://www.w3.org/2005/xpath-functions#" },
			{ "sfn", "http://www.w3.org/ns/sparql#" }
		};
		private int preamblePos;
		private Uri baseUri = null;
		private int blankNodeIndex = 0;

		/// <summary>
		/// Parses a SPARQL statement
		/// </summary>
		/// <param name="Preamble">Preamble that needs to be re-parsed.</param>
		public SparqlParser(string Preamble)
		{
			this.preamble = Preamble;
			this.preambleLen = Preamble?.Length ?? 0;
			this.preamblePos = 0;
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => string.Empty;

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[] { "DISTINCT", "FROM", "WHERE", "OPTIONAL", "ORDER", "BY" };

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			Result = null;

			ScriptNode Node;
			string s;
			bool Distinct = false;
			char ch;

			s = this.PeekNextToken(Parser).ToUpper();
			if (string.IsNullOrEmpty(s))
				return false;

			while (s != "SELECT")
			{
				switch (s)
				{
					case "BASE":
						this.NextToken(Parser);
						this.SkipWhiteSpace(Parser);
						if (this.preamblePos < this.preambleLen)
							return false;

						ch = Parser.NextNonWhitespaceChar();
						if (ch != '<')
							throw Parser.SyntaxError("Expected <");

						this.baseUri = this.ParseUri(Parser).Uri;

						break;

					case "PREFIX":
						this.NextToken(Parser);
						this.SkipWhiteSpace(Parser);
						if (this.preamblePos < this.preambleLen)
							return false;

						Parser.SkipWhiteSpace();

						s = this.ParseName(Parser);

						if (Parser.NextNonWhitespaceChar() != ':')
							throw Parser.SyntaxError("Expected :");

						if (Parser.NextNonWhitespaceChar() != '<')
							throw Parser.SyntaxError("Expected <");

						this.namespaces[s] = this.ParseUri(Parser).Uri.ToString();
						break;

					default:
						return false;
				}

				s = Parser.PeekNextToken().ToUpper();
				if (string.IsNullOrEmpty(s))
					return false;
			}

			if (s != "SELECT")
				throw Parser.SyntaxError("Expected SELECT.");

			Parser.NextToken();
			s = Parser.PeekNextToken().ToUpper();
			if (string.IsNullOrEmpty(s))
				return false;

			if (s == "DISTINCT")
			{
				Parser.NextToken();
				Distinct = true;

				s = Parser.PeekNextToken().ToUpper();
			}

			List<ScriptNode> Columns = new List<ScriptNode>();
			List<KeyValuePair<string, bool>> OrderBy = null;
			ScriptNode From;

			while (!string.IsNullOrEmpty(s) && s != "WHERE" && s != "FROM")
			{
				if (s == "?")
				{
					Parser.NextToken();
					Node = Parser.ParseObject();
					if (!(Node is VariableReference))
						throw Parser.SyntaxError("Expected variable name.");

					Columns.Add(Node);
				}
				else
				{
					Node = Parser.ParseStatement();
					Columns.Add(Node);
				}

				s = Parser.PeekNextToken().ToUpper();
			}

			if (s == "FROM")
			{
				Parser.NextToken();
				Parser.SkipWhiteSpace();

				if (Parser.PeekNextChar() == '<')
				{
					int Start = Parser.Position;
					Parser.NextChar();
					UriNode FromUri = this.ParseUri(Parser);

					From = new ConstantElement(new ObjectValue(FromUri), Start, Parser.Position - Start, Parser.Expression);
				}
				else
					From = Parser.ParseObject();

				s = Parser.PeekNextToken().ToUpper();
			}
			else
				From = null;

			if (s == "WHERE")
			{
				Parser.NextToken();
				if (Parser.NextNonWhitespaceChar() != '{')
					throw Parser.SyntaxError("Expected {");

				this.ParseTriples(Parser, false);

				if (Parser.NextNonWhitespaceChar() != '}')
					throw Parser.SyntaxError("Expected }");

				s = Parser.PeekNextToken().ToUpper();
			}

			if (s == "ORDER")
			{
				Parser.NextToken();
				s = Parser.NextToken().ToUpper();
				if (s != "BY")
					throw Parser.SyntaxError("Expected BY");

				OrderBy = new List<KeyValuePair<string, bool>>();

				s = Parser.PeekNextToken().ToUpper();
				while (true)
				{
					switch (s)
					{
						case "?":
							Parser.NextToken();
							OrderBy.Add(new KeyValuePair<string, bool>(this.ParseName(Parser), true));
							break;

						case "ASC":
							Parser.NextToken();
							if (Parser.NextToken() != "(")
								throw Parser.SyntaxError("Expected (");

							OrderBy.Add(new KeyValuePair<string, bool>(this.ParseName(Parser), true));

							if (Parser.NextToken() != ")")
								throw Parser.SyntaxError("Expected )");

							break;

						case "DESC":
							Parser.NextToken();
							if (Parser.NextToken() != "(")
								throw Parser.SyntaxError("Expected (");

							OrderBy.Add(new KeyValuePair<string, bool>(this.ParseName(Parser), false));

							if (Parser.NextToken() != ")")
								throw Parser.SyntaxError("Expected )");

							break;

						default:
							s = null;
							break;
					}

					if (s is null)
						break;
					
					s = Parser.PeekNextToken().ToUpper();
				}

				s = Parser.PeekNextToken().ToUpper();
			}

			Result = new Select(Distinct, Columns.ToArray(), From, this.triples.ToArray(),
				OrderBy?.ToArray(), Parser.Start, Parser.Length, Parser.Expression);

			return true;
		}

		private char PeekNextChar(ScriptParser Parser)
		{
			if (this.preamblePos < this.preambleLen)
				return this.preamble[this.preamblePos];
			else
				return Parser.PeekNextChar();
		}

		private char NextChar(ScriptParser Parser)
		{
			if (this.preamblePos < this.preambleLen)
				return this.preamble[this.preamblePos++];
			else
				return Parser.NextChar();
		}

		private void SkipWhiteSpace(ScriptParser Parser)
		{
			char ch = this.PeekNextChar(Parser);

			while (ch != 0 && (ch <= ' ' || ch == 160))
			{
				this.NextChar(Parser);
				ch = this.PeekNextChar(Parser);
			}
		}

		private string NextToken(ScriptParser Parser)
		{
			this.SkipWhiteSpace(Parser);

			if (this.preamblePos < this.preambleLen)
			{
				int Start = this.preamblePos;
				char ch = this.preamble[this.preamblePos];

				if (char.IsLetter(ch))
				{
					while (this.preamblePos < this.preambleLen && char.IsLetterOrDigit(this.preamble[this.preamblePos]))
						this.preamblePos++;
				}
				else if (char.IsDigit(ch))
				{
					while (this.preamblePos < this.preambleLen && char.IsDigit(this.preamble[this.preamblePos]))
						this.preamblePos++;
				}
				else if (char.IsSymbol(ch))
				{
					while (this.preamblePos < this.preambleLen && char.IsSymbol(this.preamble[this.preamblePos]))
						this.preamblePos++;
				}
				else
					this.preamblePos++;

				return this.preamble.Substring(Start, this.preamblePos - Start);
			}
			else
				return Parser.NextToken();
		}

		private string PeekNextToken(ScriptParser Parser)
		{
			this.SkipWhiteSpace(Parser);

			if (this.preamblePos < this.preambleLen)
			{
				int Bak = this.preamblePos;
				string Token = this.NextToken(Parser);
				this.preamblePos = Bak;

				return Token;
			}
			else
				return Parser.PeekNextToken();
		}

		private void ParseTriples(ScriptParser Parser, bool Optional)
		{
			this.ParseTriples(Parser, null, Optional);
		}

		private void ParseTriples(ScriptParser Parser, ISemanticElement Subject, bool Optional)
		{
			ISemanticElement Predicate = null;
			ISemanticElement Object;
			int TriplePosition = Subject is null ? 0 : 1;
			bool InBlankNode = !(Subject is null);

			while (Parser.InScript)
			{
				if (TriplePosition == 0)
				{
					Parser.SkipWhiteSpace();
					if (Parser.PeekNextToken().ToUpper() == "OPTIONAL")
					{
						Parser.NextToken();
						if (Parser.NextNonWhitespaceChar() != '{')
							throw Parser.SyntaxError("Expected {");

						this.ParseTriples(Parser, null, true);

						if (Parser.NextNonWhitespaceChar() != '}')
							throw Parser.SyntaxError("Expected }");

						switch (Parser.NextNonWhitespaceChar())
						{
							case '.':
								Subject = null;
								Predicate = null;
								TriplePosition = 0;
								break;

							case '}':
								Parser.UndoChar();
								return;

							default:
								throw Parser.SyntaxError("Expected . or }");
						}

						continue;
					}
				}

				Object = this.ParseElement(Parser, TriplePosition, Optional);
				if (Object is null)
				{
					if (Subject is null)
						return;
					else if (Predicate is null)
					{
						if (InBlankNode)
							return;
						else
							throw Parser.SyntaxError("Expected predicate.");
					}
					else
						throw Parser.SyntaxError("Expected object.");
				}

				if (Subject is null)
				{
					Subject = Object;
					TriplePosition++;
				}
				else if (Predicate is null)
				{
					Predicate = Object;
					TriplePosition++;
				}
				else
				{
					this.triples.Add(new SemanticQueryTriple(Subject, Predicate, Object, Optional));

					switch (Parser.NextNonWhitespaceChar())
					{
						case '.':
							if (InBlankNode)
								throw Parser.SyntaxError("Expected ]");

							Subject = null;
							Predicate = null;
							TriplePosition = 0;
							break;

						case '}':
							if (InBlankNode)
								throw Parser.SyntaxError("Expected ]");

							Parser.UndoChar();
							return;

						case ';':
							Predicate = null;
							TriplePosition = 1;

							Parser.SkipWhiteSpace();
							if (Parser.PeekNextChar() == '.')
							{
								if (InBlankNode)
									throw Parser.SyntaxError("Expected ]");

								Parser.NextChar();
								Subject = null;
								TriplePosition = 0;
							}
							break;

						case ',':
							break;

						case ']':
							return;

						default:
							if (InBlankNode)
								throw Parser.SyntaxError("Expected triple delimiter ] ; or ,");
							else
								throw Parser.SyntaxError("Expected triple delimiter . ; or ,");
					}
				}
			}
		}

		private ISemanticElement ParseElement(ScriptParser Parser, int TriplePosition, bool Optional)
		{
			while (true)
			{
				char ch = Parser.NextNonWhitespaceChar();

				switch (ch)
				{
					case (char)0:
						return null;

					case '[':
						if (TriplePosition == 1)
							throw Parser.SyntaxError("Predicate cannot be a blank node.");

						BlankNode Node = this.CreateBlankNode();
						this.ParseTriples(Parser, Node, Optional);
						return Node;

					case '(':
						return this.ParseCollection(Parser, Optional);

					case ']':
						return null;

					case '}':
						Parser.UndoChar();
						return null;

					case '<':
						return this.ParseUri(Parser);

					case '"':
						if (TriplePosition != 2)
							throw Parser.SyntaxError("Literals can only occur in object position.");

						string s;

						if (Parser.IsNextChars('"', 2))
						{
							Parser.SkipChars(2);
							s = this.ParseString(Parser, '"', true, true);
						}
						else
							s = this.ParseString(Parser, '"', false, true);

						string Language = null;

						if (Parser.PeekNextChar() == '@')
						{
							Parser.NextChar();
							Language = this.ParseName(Parser);
						}

						if (Parser.IsNextChars('^', 2))
						{
							Parser.SkipChars(2);

							string DataType = this.ParseUriOrPrefixedToken(Parser).Uri.ToString();

							if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
							{
								LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
									?? new CustomLiteral(string.Empty, DataType);

								this.dataTypes[DataType] = LiteralType;
							}

							return LiteralType.Parse(s, DataType, Language);
						}
						else if (!string.IsNullOrEmpty(Language))
							return new StringLiteral(s, Language);
						else
							return new StringLiteral(s);

					case ':':
						return this.ParsePrefixedToken(Parser, string.Empty);

					case '?':
						int Start = Parser.Position;
						s = this.ParseName(Parser);
						return new SemanticScriptElement(new VariableReference(s, Start, Parser.Position - Start, Parser.Expression));

					default:
						if (char.IsWhiteSpace(ch))
							break;

						if (ch == '_')
						{
							if (Parser.NextNonWhitespaceChar() != ':')
								throw Parser.SyntaxError("Expected :");

							return new BlankNode(this.ParseName(Parser));
						}
						else if (char.IsLetter(ch) || ch == ':')
						{
							Parser.UndoChar();
							s = this.ParseName(Parser);

							if (Parser.PeekNextChar() == ':')
							{
								Parser.NextChar();
								return this.ParsePrefixedToken(Parser, s);
							}

							switch (s)
							{
								case "a":
									if (TriplePosition == 1)
										return RdfDocument.RdfType;
									break;

								case "true":
									if (TriplePosition == 2)
										return new BooleanLiteral(true);
									break;

								case "false":
									if (TriplePosition == 2)
										return new BooleanLiteral(false);
									break;
							}

							throw Parser.SyntaxError("Expected :");
						}
						else
						{
							if (TriplePosition != 2)
								throw Parser.SyntaxError("Literals can only occur in object position.");

							Parser.UndoChar();
							return this.ParseNumber(Parser);
						}
				}
			}
		}

		private BlankNode CreateBlankNode()
		{
			return new BlankNode("n" + (++this.blankNodeIndex).ToString());
		}

		private ISemanticElement ParseCollection(ScriptParser Parser, bool Optional)
		{
			LinkedList<ISemanticElement> Elements = null;

			Parser.SkipWhiteSpace();

			while (Parser.InScript)
			{
				if (Parser.PeekNextChar() == ')')
				{
					Parser.NextChar();

					if (Elements is null)
						return RdfDocument.RdfNil;

					LinkedListNode<ISemanticElement> Loop = Elements.First;
					BlankNode Result = this.CreateBlankNode();
					BlankNode Current = Result;

					while (!(Loop is null))
					{
						this.triples.Add(new SemanticQueryTriple(Current, RdfDocument.RdfFirst, Loop.Value, Optional));

						Loop = Loop.Next;

						if (!(Loop is null))
						{
							BlankNode Next = this.CreateBlankNode();
							this.triples.Add(new SemanticQueryTriple(Current, RdfDocument.RdfRest, Next, Optional));
							Current = Next;
						}
					}

					this.triples.Add(new SemanticQueryTriple(Current, RdfDocument.RdfRest, RdfDocument.RdfNil, Optional));

					return Result;
				}

				ISemanticElement Element = this.ParseElement(Parser, 2, Optional);
				if (Element is null)
					break;

				if (Elements is null)
					Elements = new LinkedList<ISemanticElement>();

				Elements.AddLast(Element);
				Parser.SkipWhiteSpace();
			}

			throw Parser.SyntaxError("Expected )");
		}

		private UriNode ParseUriOrPrefixedToken(ScriptParser Parser)
		{
			if (Parser.EndOfScript)
				throw Parser.SyntaxError("Expected URI or prefixed token.");

			if (Parser.PeekNextChar() == '<')
			{
				Parser.NextChar();
				return this.ParseUri(Parser);
			}

			string Prefix = this.ParseName(Parser);

			if (Parser.NextChar() != ':')
				throw Parser.SyntaxError("Expected :");

			return this.ParsePrefixedToken(Parser, Prefix);
		}

		private UriNode ParsePrefixedToken(ScriptParser Parser, string Prefix)
		{
			if (!this.namespaces.TryGetValue(Prefix, out string Namespace))
				throw Parser.SyntaxError("Prefix unknown.");

			Parser.SkipWhiteSpace();

			string LocalName = this.ParseName(Parser);

			return new UriNode(new Uri(Namespace + LocalName), Prefix + ":" + LocalName);
		}

		private string ParseName(ScriptParser Parser)
		{
			if (!TurtleDocument.IsNameStartChar(Parser.PeekNextChar()))
				return string.Empty;

			int Start = Parser.Position;
			Parser.NextChar();

			while (TurtleDocument.IsNameChar(Parser.PeekNextChar()))
				Parser.NextChar();

			return Parser.Expression.Script.Substring(Start, Parser.Position - Start);
		}

		private SemanticLiteral ParseNumber(ScriptParser Parser)
		{
			int Start = Parser.Position;
			bool HasDigits = false;
			bool HasDecimal = false;
			bool HasExponent = false;
			bool HasSign = false;

			while (true)
			{
				char ch = Parser.PeekNextChar();

				if (char.IsDigit(ch))
					HasDigits = true;
				else if (ch == '+' || ch == '-')
				{
					if (HasSign || HasDecimal)
						break;

					HasSign = true;
				}
				else if (ch == '.')
				{
					if (HasDecimal || HasExponent)
						break;

					HasDecimal = true;
				}
				else if (ch == 'e' || ch == 'E')
				{
					HasExponent = true;
					HasSign = false;
				}
				else
					break;

				Parser.NextChar();
			}

			if (Parser.Position > Start)
			{
				string s = Parser.Expression.Script.Substring(Start, Parser.Position - Start);

				if (HasExponent)
				{
					if (CommonTypes.TryParse(s, out double dbl))
						return new DoubleLiteral(dbl, s);
					else
						throw Parser.SyntaxError("Invalid double number.");
				}
				else if (HasDecimal)
				{
					if (CommonTypes.TryParse(s, out decimal dec))
						return new DecimalLiteral(dec, s);
					else
						throw Parser.SyntaxError("Invalid decimal number.");
				}
				else if (HasDigits)
				{
					if (BigInteger.TryParse(s, out BigInteger bi))
						return new IntegerLiteral(bi, s);
					else
						throw Parser.SyntaxError("Invalid integer number.");
				}
			}

			throw Parser.SyntaxError("Expected value element.");
		}

		private string ParseString(ScriptParser Parser, char EndChar, bool MultiLine, bool IncludeWhiteSpace)
		{
			StringBuilder sb = null;
			int Start = Parser.Position;
			char ch;

			while ((ch = Parser.PeekNextChar()) != (char)0)
			{
				Parser.NextChar();

				if (ch == EndChar)
				{
					if (MultiLine)
					{
						if (Parser.IsNextChars(EndChar, 2))
						{
							Parser.SkipChars(2);
							return sb?.ToString() ?? Parser.Expression.Script.Substring(Start, Parser.Position - Start - 3);
						}
						else
							sb?.Append(ch);
					}
					else
						return sb?.ToString() ?? Parser.Expression.Script.Substring(Start, Parser.Position - Start - 1);
				}
				else if (ch == '\\')
				{
					if (sb is null)
					{
						sb = new StringBuilder();

						if (Parser.Position > Start + 1)
							sb.Append(Parser.Expression.Script.Substring(Start, Parser.Position - Start - 1));
					}

					switch (ch = Parser.NextChar())
					{
						case (char)0:
							throw Parser.SyntaxError("Expected escape code.");

						case 't':
							sb.Append('\t');
							break;

						case 'n':
							sb.Append('\n');
							break;

						case 'r':
							sb.Append('\r');
							break;

						case 'v':
							sb.Append('\v');
							break;

						case 'f':
							sb.Append('\f');
							break;

						case 'b':
							sb.Append('\b');
							break;

						case 'a':
							sb.Append('\a');
							break;

						case 'u':
							if (Parser.HasCharacters(4) && int.TryParse(Parser.PeekNextChars(4), System.Globalization.NumberStyles.HexNumber, null, out int i))
							{
								sb.Append((char)i);
								Parser.SkipChars(4);
							}
							else
								throw Parser.SyntaxError("Expected 4-character hexadecimal code.");
							break;

						case 'U':
							if (Parser.HasCharacters(8) && int.TryParse(Parser.PeekNextChars(8), System.Globalization.NumberStyles.HexNumber, null, out i))
							{
								sb.Append((char)i);
								Parser.SkipChars(8);
							}
							else
								throw Parser.SyntaxError("Expected 8-character hexadecimal code.");
							break;

						default:
							sb.Append(ch);
							break;
					}
				}
				else if (IncludeWhiteSpace || !char.IsWhiteSpace(ch))
					sb?.Append(ch);
			}

			throw Parser.SyntaxError("Expected " + new string(EndChar, MultiLine ? 3 : 1));
		}

		private UriNode ParseUri(ScriptParser Parser)
		{
			string Short = this.ParseString(Parser, '>', false, false);

			if (this.baseUri is null)
			{
				if (Uri.TryCreate(Short, UriKind.RelativeOrAbsolute, out Uri URI))
					return new UriNode(URI, Short);
				else
					throw Parser.SyntaxError("Invalid URI.");
			}
			else
			{
				if (string.IsNullOrEmpty(Short))
					return new UriNode(this.baseUri, Short);
				else if (Uri.TryCreate(this.baseUri, Short, out Uri URI))
					return new UriNode(URI, Short);
				else
					throw Parser.SyntaxError("Invalid URI.");
			}
		}

	}
}
