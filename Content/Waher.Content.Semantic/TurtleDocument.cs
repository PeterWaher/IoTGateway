using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Waher.Content.Semantic.TurtleModel;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains semantic information stored in a turtle document.
	/// </summary>
	public class TurtleDocument : SemanticModel
	{
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>
		{
			{ "xsd", "http://www.w3.org/2001/XMLSchema" }
		};
		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly string text;
		private readonly int len;
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
		{
			this.text = Text;
			this.len = this.text.Length;
			this.baseUri = BaseUri;

			if (!(this.baseUri is null))
				this.namespaces[string.Empty] = this.baseUri.AbsoluteUri;

			this.ParseTriples();
		}

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
			bool InBlankNode = !(Subject is null);

			while (this.pos < this.len)
			{
				Object = this.ParseElement();
				if (Object is null)
				{
					if (Subject is null)
						return;
					else if (Predicate is null)
						throw this.ParsingException("Expected predicate.");
					else
						throw this.ParsingException("Expected object.");
				}

				if (Subject is null)
				{
					Subject = Object;
					if (Subject is ISemanticLiteral)
						throw this.ParsingException("Subjects cannot be literals.");
				}
				else if (Predicate is null)
				{
					Predicate = Object;
					if (!(Predicate is UriNode))
						throw this.ParsingException("Predicates must be URIs.");
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
							break;

						case ';':
							Predicate = null;
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

		private ISemanticElement ParseElement()
		{
			char ch = this.NextNonWhitespaceChar();

			while (true)
			{
				switch (ch)
				{
					case (char)0:
						return null;

					case '#':
						this.SkipLine();
						break;

					case '@':
						string s = this.ParseToken();

						switch (s)
						{
							case "base":
								ch = this.NextNonWhitespaceChar();
								if (ch != '<')
									throw this.ParsingException("Expected <");

								this.baseUri = this.ParseUri();
								break;

							case "prefix":
								this.SkipWhiteSpace();
								if (this.PeekNextChar() == ':')
									s = string.Empty;
								else
									s = this.ParseToken();

								if (this.NextNonWhitespaceChar() != ':')
									throw this.ParsingException("Expected :");

								if (this.NextNonWhitespaceChar() != '<')
									throw this.ParsingException("Expected <");

								this.namespaces[s] = this.ParseUri().AbsoluteUri;
								break;

							default:
								throw this.ParsingException("Unrecognized keyword.");
						}
						break;

					case '[':
						BlankNode Node = new BlankNode(this.blankNodeIndex++);
						this.ParseTriples(Node);
						return Node;

					case '(':
						return this.ParseCollection();

					case ']':
						return null;

					case '<':
						return new UriNode(this.ParseUri());

					case '"':
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

							string DataType = this.ParseUriOrPrefixedToken().AbsoluteUri.ToLower();

							if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
							{
								LiteralType = new CustomLiteral(string.Empty, DataType);
								this.dataTypes[DataType] = LiteralType;
							}

							return LiteralType.Parse(s, DataType);
						}
						else
							return new StringLiteral(s);

					case ':':
						return new UriNode(this.ParsePrefixedToken(string.Empty));

					default:
						if (ch <= ' ' || ch == 160)
							break;

						s = this.ParseToken();

						switch (s)
						{
							case "a":
								return UriNode.RdfA;

							case "true":
								return BooleanLiteral.True;

							case "false":
								return BooleanLiteral.False;
						}

						if (BigInteger.TryParse(s, out BigInteger bi))
							return new IntegerLiteral(bi);
						
						if (CommonTypes.TryParse(s, out double dbl))
							return new DoubleLiteral(dbl);

						if (CommonTypes.TryParse(s, out decimal dec))
							return new DecimalLiteral(dec);

						if (this.NextChar() != ':')
							throw this.ParsingException("Expected literal, URI or prefix.");

						return new UriNode(this.ParsePrefixedToken(s));
				}
			}
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
						return UriNode.RdfNil;

					LinkedListNode<ISemanticElement> Loop = Elements.First;
					BlankNode Result = new BlankNode(this.blankNodeIndex++);
					BlankNode Current = Result;

					while (!(Loop is null))
					{
						this.triples.Add(new SemanticTriple(Current, UriNode.RdfFirst, Loop.Value));

						Loop = Loop.Next;

						if (!(Loop is null))
						{
							BlankNode Next = new BlankNode(this.blankNodeIndex++);
							this.triples.Add(new SemanticTriple(Current, UriNode.RdfNext, Next));
							Current = Next;
						}
					}

					this.triples.Add(new SemanticTriple(Current, UriNode.RdfNext, UriNode.RdfNil));

					return Result;
				}

				ISemanticElement Element = this.ParseElement();
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

			string Prefix = this.ParseToken();

			if (this.NextChar() != ':')
				throw this.ParsingException("Expected :");

			return this.ParsePrefixedToken(Prefix);
		}

		private Uri ParsePrefixedToken(string Prefix)
		{
			if (!this.namespaces.TryGetValue(Prefix, out string Namespace))
				throw this.ParsingException("Prefix unknown.");

			this.SkipWhiteSpace();

			string LocalName = this.ParseToken();

			return new Uri(Namespace + "#" + LocalName);
		}

		private string ParseToken()
		{
			int Start = this.pos;
			char ch;

			while ((ch = this.PeekNextChar()) > ' ' && ch != 160)
				this.pos++;

			return this.text.Substring(Start, this.pos - Start);
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
							return sb?.ToString() ?? this.text.Substring(Start, this.pos - Start - 2);
						}
						else
							sb?.Append(ch);
					}
					else
						return sb?.ToString() ?? this.text.Substring(Start, this.pos - Start);
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
					sb?.ToString();
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
				char ch = this.NextNonWhitespaceChar();
				if (ch == 0)
					break;

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
						if (Uri.TryCreate(this.baseUri, sb.ToString(), out Uri URI))
							return URI;
						else
							throw this.ParsingException("Invalid URI.");
					}
				}
				else
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

			while (((ch = this.PeekNextChar()) <= ' ' && ch != 0) || ch == 160)
				this.pos++;
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
				return this.text[this.pos++];
			else
				return (char)0;
		}

		private char NextNonWhitespaceChar()
		{
			char ch;

			while (this.pos < this.len)
			{
				ch = this.text[this.pos++];
				if (ch > ' ' && ch != 160)
					return ch;
			}

			return (char)0;
		}
	}
}
