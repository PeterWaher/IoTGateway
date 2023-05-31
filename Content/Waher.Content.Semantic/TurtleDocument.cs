using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// How blank node IDs are generated
	/// </summary>
	public enum BlankNodeIdMode
	{
		/// <summary>
		/// Sequentially
		/// </summary>
		Sequential,

		/// <summary>
		/// Using GUIDs
		/// </summary>
		Guid
	}

	/// <summary>
	/// Contains semantic information stored in a turtle document.
	/// 
	/// Ref: https://www.w3.org/TeamSubmission/turtle/
	/// </summary>
	public class TurtleDocument : SemanticModel
	{
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>
		{
			{ "xsd", "http://www.w3.org/2001/XMLSchema#" },
			{ "ttl", "http://www.w3.org/2008/turtle#" }
		};
		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly string text;
		private readonly string blankNodeIdPrefix;
		private readonly int len;
		private readonly BlankNodeIdMode blankNodeIdMode;
		private Uri baseUri = null;
		private int blankNodeIndex = 0;
		private int pos = 0;

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text content of Turtle document.</param>
		public TurtleDocument(string Text)
			: this(Text, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text content of Turtle document.</param>
		/// <param name="BaseUri">Base URI</param>
		public TurtleDocument(string Text, Uri BaseUri)
			: this(Text, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text content of Turtle document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public TurtleDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix)
			: this(Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text content of Turtle document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public TurtleDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			this.text = Text;
			this.len = this.text.Length;
			this.baseUri = BaseUri;
			this.blankNodeIdPrefix = BlankNodeIdPrefix;
			this.blankNodeIdMode = BlankNodeIdMode;

			if (!(this.baseUri is null))
				this.namespaces[string.Empty] = this.baseUri.AbsoluteUri;

			this.ParseTriples();
		}

		/// <summary>
		/// Original text of document.
		/// </summary>
		public string Text => this.text;

		private void ParseTriples()
		{
			this.ParseTriples(null);

			if (this.pos < this.len)
				throw this.ParsingException("Unexpected end of document.");
		}

		private void ParseTriples(ISemanticElement Subject)
		{
			ISemanticElement Predicate = null;
			ISemanticElement Object;
			int TriplePosition = Subject is null ? 0 : 1;
			bool InBlankNode = !(Subject is null);

			while (this.pos < this.len)
			{
				Object = this.ParseElement(TriplePosition);
				if (Object is null)
				{
					if (Subject is null)
						return;
					else if (Predicate is null)
					{
						if (InBlankNode)
							return;
						else
							throw this.ParsingException("Expected predicate.");
					}
					else
						throw this.ParsingException("Expected object.");
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
					this.triples.Add(new SemanticTriple(Subject, Predicate, Object));

					switch (this.NextNonWhitespaceChar())
					{
						case '.':
							if (InBlankNode)
								throw this.ParsingException("Expected ]");

							Subject = null;
							Predicate = null;
							TriplePosition = 0;
							break;

						case ';':
							Predicate = null;
							TriplePosition = 1;

							this.SkipWhiteSpace();
							if (this.PeekNextChar() == '.')
							{
								if (InBlankNode)
									throw this.ParsingException("Expected ]");

								this.pos++;
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
								throw this.ParsingException("Expected triple delimiter ] ; or ,");
							else
								throw this.ParsingException("Expected triple delimiter . ; or ,");
					}
				}
			}
		}

		private ISemanticElement ParseElement(int TriplePosition)
		{
			while (true)
			{
				char ch = this.NextNonWhitespaceChar();

				switch (ch)
				{
					case (char)0:
						return null;

					case '@':
						string s = this.ParseName();

						switch (s)
						{
							case "base":
								ch = this.NextNonWhitespaceChar();
								if (ch != '<')
									throw this.ParsingException("Expected <");

								this.baseUri = this.ParseUri();

								if (this.NextNonWhitespaceChar() != '.')
									throw this.ParsingException("Expected .");

								break;

							case "prefix":
								this.SkipWhiteSpace();

								s = this.ParseName();

								if (this.NextNonWhitespaceChar() != ':')
									throw this.ParsingException("Expected :");

								if (this.NextNonWhitespaceChar() != '<')
									throw this.ParsingException("Expected <");

								this.namespaces[s] = this.ParseUri().AbsoluteUri;

								if (this.NextNonWhitespaceChar() != '.')
									throw this.ParsingException("Expected .");

								break;

							default:
								throw this.ParsingException("Unrecognized keyword.");
						}
						break;

					case '[':
						if (TriplePosition == 1)
							throw this.ParsingException("Predicate cannot be a blank node.");

						BlankNode Node = this.CreateBlankNode();
						this.ParseTriples(Node);
						return Node;

					case '(':
						return this.ParseCollection();

					case ']':
						return null;

					case '<':
						return new UriNode(this.ParseUri());

					case '"':
						if (TriplePosition != 2)
							throw this.ParsingException("Literals can only occur in object position.");

						if (this.pos < this.len - 1 && this.text[this.pos] == '"' && this.text[this.pos + 1] == '"')
						{
							this.pos += 2;
							s = this.ParseString(true);
						}
						else
							s = this.ParseString(false);

						if (this.pos < this.len - 1 && this.text[this.pos] == '^' && this.text[this.pos + 1] == '^')
						{
							this.pos += 2;

							string DataType = this.ParseUriOrPrefixedToken().AbsoluteUri;

							if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
							{
								LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
									?? new CustomLiteral(string.Empty, DataType);

								this.dataTypes[DataType] = LiteralType;
							}

							return LiteralType.Parse(s, DataType);
						}
						else if (this.pos < this.len && this.text[this.pos] == '@')
						{
							this.pos++;
							return new StringLiteral(s, this.ParseName());
						}
						else
							return new StringLiteral(s);

					case ':':
						return new UriNode(this.ParsePrefixedToken(string.Empty));

					default:
						if (char.IsWhiteSpace(ch))
							break;

						if (ch == '_')
						{
							if (this.NextNonWhitespaceChar() != ':')
								throw this.ParsingException("Expected :");

							return new BlankNode(this.ParseName());
						}
						else if (char.IsLetter(ch) || ch == ':')
						{
							this.pos--;
							s = this.ParseName();

							if (this.PeekNextChar() == ':')
							{
								this.pos++;
								return new UriNode(this.ParsePrefixedToken(s));
							}

							switch (s)
							{
								case "a":
									if (TriplePosition == 1)
										return RdfDocument.RdfA;
									break;

								case "true":
									if (TriplePosition == 2)
										return BooleanLiteral.True;
									break;

								case "false":
									if (TriplePosition == 2)
										return BooleanLiteral.False;
									break;
							}

							throw this.ParsingException("Expected :");
						}
						else
						{
							if (TriplePosition != 2)
								throw this.ParsingException("Literals can only occur in object position.");

							this.pos--;
							return this.ParseNumber();
						}
				}
			}
		}

		private BlankNode CreateBlankNode()
		{
			if (this.blankNodeIdMode == BlankNodeIdMode.Guid)
				return new BlankNode(this.blankNodeIdPrefix + Guid.NewGuid().ToString());
			else
				return new BlankNode(this.blankNodeIdPrefix + (++this.blankNodeIndex).ToString());
		}

		private ISemanticElement ParseCollection()
		{
			LinkedList<ISemanticElement> Elements = null;

			this.SkipWhiteSpace();

			while (this.pos < this.len)
			{
				if (this.text[this.pos] == ')')
				{
					this.pos++;

					if (Elements is null)
						return RdfDocument.RdfNil;

					LinkedListNode<ISemanticElement> Loop = Elements.First;
					BlankNode Result = this.CreateBlankNode();
					BlankNode Current = Result;

					while (!(Loop is null))
					{
						this.triples.Add(new SemanticTriple(Current, RdfDocument.RdfFirst, Loop.Value));

						Loop = Loop.Next;

						if (!(Loop is null))
						{
							BlankNode Next = this.CreateBlankNode();
							this.triples.Add(new SemanticTriple(Current, RdfDocument.RdfNext, Next));
							Current = Next;
						}
					}

					this.triples.Add(new SemanticTriple(Current, RdfDocument.RdfNext, RdfDocument.RdfNil));

					return Result;
				}

				ISemanticElement Element = this.ParseElement(2);
				if (Element is null)
					break;

				if (Elements is null)
					Elements = new LinkedList<ISemanticElement>();

				Elements.AddLast(Element);
				this.SkipWhiteSpace();
			}

			throw this.ParsingException("Expected )");
		}

		private Uri ParseUriOrPrefixedToken()
		{
			if (this.pos >= this.len)
				throw this.ParsingException("Expected URI or prefixed token.");

			if (this.text[this.pos] == '<')
			{
				this.pos++;
				return this.ParseUri();
			}

			string Prefix = this.ParseName();

			if (this.NextChar() != ':')
				throw this.ParsingException("Expected :");

			return this.ParsePrefixedToken(Prefix);
		}

		private Uri ParsePrefixedToken(string Prefix)
		{
			if (!this.namespaces.TryGetValue(Prefix, out string Namespace))
				throw this.ParsingException("Prefix unknown.");

			this.SkipWhiteSpace();

			string LocalName = this.ParseName();

			return new Uri(Namespace + LocalName);
		}

		private string ParseName()
		{
			if (!IsNameStartChar(this.PeekNextChar()))
				return string.Empty;

			int Start = this.pos++;

			while (IsNameChar(this.PeekNextChar()))
				this.pos++;

			return this.text.Substring(Start, this.pos - Start);
		}

		private static bool IsNameStartChar(char ch)
		{
			if (ch < 'A')
				return false;
			else if (ch <= 'Z')
				return true;
			else if (ch < '_')
				return false;
			else if (ch == '_')
				return true;
			else if (ch < 'a')
				return false;
			else if (ch <= 'z')
				return true;
			else if (ch < '\xc0')
				return false;
			else if (ch <= '\xd6')
				return true;
			else if (ch < '\xd8')
				return false;
			else if (ch <= '\xf6')
				return true;
			else if (ch < '\xf8')
				return false;
			else if (ch <= '\x02ff')
				return true;
			else if (ch < '\x0370')
				return false;
			else if (ch <= '\x037d')
				return true;
			else if (ch < '\x037f')
				return false;
			else if (ch <= '\x1fff')
				return true;
			else if (ch < '\x037f')
				return false;
			else if (ch <= '\x1fff')
				return true;
			else if (ch < '\x200c')
				return false;
			else if (ch <= '\x200d')
				return true;
			else if (ch < '\x2070')
				return false;
			else if (ch <= '\x218f')
				return true;
			else if (ch < '\x2C00')
				return false;
			else if (ch <= '\x2FEF')
				return true;
			else if (ch < '\x3001')
				return false;
			else if (ch <= '\xD7FF')
				return true;
			else if (ch < '\xF900')
				return false;
			else if (ch <= '\xFDCF')
				return true;
			else if (ch < '\xFDF0')
				return false;
			else if (ch <= '\xFFFD')
				return true;
			else
				return false;
		}

		private static bool IsNameChar(char ch)
		{
			if (IsNameStartChar(ch))
				return true;
			else if (ch < '-')
				return false;
			else if (ch == '-')
				return true;
			else if (ch < '0')
				return false;
			else if (ch <= '9')
				return true;
			else if (ch < '\x00B7')
				return false;
			else if (ch == '\x00B7')
				return true;
			else if (ch < '\x0300')
				return false;
			else if (ch <= '\x036F')
				return true;
			else if (ch < '\x203F')
				return false;
			else if (ch <= '\x2040')
				return true;
			else
				return false;
		}

		private SemanticLiteral ParseNumber()
		{
			int Start = this.pos;
			bool HasDigits = false;
			bool HasDecimal = false;
			bool HasExponent = false;
			bool HasSign = false;

			while (true)
			{
				char ch = this.PeekNextChar();

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

				this.pos++;
			}

			if (this.pos > Start)
			{
				string s = this.text.Substring(Start, this.pos - Start);

				if (HasExponent)
				{
					if (CommonTypes.TryParse(s, out double dbl))
						return new DoubleLiteral(dbl, s);
					else
						throw this.ParsingException("Invalid double number.");
				}
				else if (HasDecimal)
				{
					if (CommonTypes.TryParse(s, out decimal dec))
						return new DecimalLiteral(dec, s);
					else
						throw this.ParsingException("Invalid decimal number.");
				}
				else if (HasDigits)
				{
					if (BigInteger.TryParse(s, out BigInteger bi))
						return new IntegerLiteral(bi, s);
					else
						throw this.ParsingException("Invalid integer number.");
				}
			}

			throw this.ParsingException("Expected value element.");
		}

		private string ParseString(bool MultiLine)
		{
			StringBuilder sb = null;
			int Start = this.pos;
			char ch;

			while ((ch = this.NextChar()) != (char)0)
			{
				if (ch == '"')
				{
					if (MultiLine)
					{
						if (this.pos < this.len - 1 && this.text[this.pos] == '"' && this.text[this.pos + 1] == '"')
						{
							this.pos += 2;
							return sb?.ToString() ?? this.text.Substring(Start, this.pos - Start - 3);
						}
						else
							sb?.Append(ch);
					}
					else
						return sb?.ToString() ?? this.text.Substring(Start, this.pos - Start - 1);
				}
				else if (ch == '\\')
				{
					if (sb is null)
					{
						sb = new StringBuilder();

						if (this.pos > Start + 1)
							sb.Append(this.text.Substring(Start, this.pos - Start - 1));
					}

					switch (ch = this.NextChar())
					{
						case (char)0:
							throw this.ParsingException("Expected escape code.");

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
							if (this.pos < this.len - 3 && int.TryParse(this.text.Substring(this.pos, 4), System.Globalization.NumberStyles.HexNumber, null, out int i))
							{
								sb.Append((char)i);
								this.pos += 4;
							}
							else
								throw this.ParsingException("Expected 4-character hexadecimal code.");
							break;

						case 'U':
							if (this.pos < this.len - 7 && int.TryParse(this.text.Substring(this.pos, 8), System.Globalization.NumberStyles.HexNumber, null, out i))
							{
								sb.Append((char)i);
								this.pos += 8;
							}
							else
								throw this.ParsingException("Expected 4-character hexadecimal code.");
							break;

						default:
							sb.Append(ch);
							break;
					}
				}
				else
					sb?.Append(ch);
			}

			throw this.ParsingException("Expected end of string.");
		}

		private ParsingException ParsingException(string Message)
		{
			return new ParsingException(Message, this.text, this.pos);
		}

		private Uri ParseUri()
		{
			StringBuilder sb = new StringBuilder();

			while (this.pos < this.len)
			{
				char ch = this.PeekNextChar();
				if (ch == 0)
					break;

				this.pos++;

				if (ch == '>')
				{
					if (this.baseUri is null)
					{
						if (Uri.TryCreate(sb.ToString(), UriKind.Absolute, out Uri URI))
							return URI;
						else
							throw this.ParsingException("Invalid URI.");
					}
					else
					{
						string s = sb.ToString();

						if (string.IsNullOrEmpty(s))
							return this.baseUri;
						else if (Uri.TryCreate(this.baseUri, s, out Uri URI))
							return URI;
						else
							throw this.ParsingException("Invalid URI.");
					}
				}
				else if (!char.IsWhiteSpace(ch))
					sb.Append(ch);
			}

			throw this.ParsingException("Expected >");
		}

		private void SkipLine()
		{
			char ch;

			while ((ch = this.PeekNextChar()) != '\r' && ch != '\n' && ch != 0)
				this.pos++;
		}

		private void SkipWhiteSpace()
		{
			char ch;

			while (true)
			{
				ch = this.PeekNextChar();

				if (char.IsWhiteSpace(ch))
					this.pos++;
				else if (ch == '#')
					this.SkipLine();
				else
					break;
			}
		}

		private char PeekNextChar()
		{
			if (this.pos < this.len)
				return this.text[this.pos];
			else
				return (char)0;
		}

		private char NextChar()
		{
			if (this.pos < this.len)
			{
				char ch = this.text[this.pos++];
				while (ch == '#')
				{
					this.SkipLine();
					if (this.pos < this.len)
						ch = this.text[this.pos++];
					else
						return (char)0;
				}

				return ch;
			}
			else
				return (char)0;
		}

		private char NextNonWhitespaceChar()
		{
			char ch;

			do
			{
				ch = this.NextChar();
			}
			while (ch != 0 && char.IsWhiteSpace(ch));

			return ch;
		}
	}
}
