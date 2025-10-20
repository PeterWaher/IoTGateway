﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Getters;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Collections;
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
	/// https://www.w3.org/TR/rdf12-turtle/
	/// https://w3c.github.io/rdf-star/cg-spec/20	21-12-17.html
	/// </summary>
	public class TurtleDocument : InMemorySemanticCube, IWebServerMetaContent
	{
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>
		{
			{ "xsd", XmlSchema.Namespace },
			{ "ttl", "http://www.w3.org/2008/turtle#" }
		};
		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly string text;
		private readonly string blankNodeIdPrefix;
		private readonly int len;
		private readonly BlankNodeIdMode blankNodeIdMode;
		private DateTimeOffset? date = null;
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
				this.namespaces[string.Empty] = this.baseUri.ToString();

			this.ParseTriples();
		}

		/// <summary>
		/// Original text of document.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Server timestamp of document.
		/// </summary>
		public DateTimeOffset? Date => this.date;

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
				Object = this.ParseElement(TriplePosition,
					out ChunkedList<ISemanticTriple> AdditionalTriples);

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

					if (!(AdditionalTriples is null))
						this.triples.AddRange(AdditionalTriples);
				}
				else if (Predicate is null)
				{
					Predicate = Object;
					TriplePosition++;

					if (!(AdditionalTriples is null))
						this.triples.AddRange(AdditionalTriples);
				}
				else
				{
					this.Add(new SemanticTriple(Subject, Predicate, Object));
					if (!(AdditionalTriples is null))
						this.triples.AddRange(AdditionalTriples);

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
						case '|':
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

		/// <summary>
		/// Adds a triple to the cube.
		/// </summary>
		/// <param name="Triple">Semantic triple.</param>
		public override void Add(ISemanticTriple Triple)
		{
			base.Add(Triple);

			this.SkipWhiteSpace();
			if (this.PeekNextChars(2) == "{|")
			{
				this.pos += 2;

				if (!(Triple is SemanticTriple T))
					T = new SemanticTriple(Triple.Subject, Triple.Predicate, Triple.Object);

				this.ParseTriples(T);

				this.SkipWhiteSpace();
				if (this.NextChar() != '}')
					throw this.ParsingException("Expected }");
			}
		}

		private ISemanticElement ParseElement(int TriplePosition, 
			out ChunkedList<ISemanticTriple> AdditionalTriples)
		{
			AdditionalTriples = null;

			while (true)
			{
				char ch = this.NextNonWhitespaceChar();

				switch (ch)
				{
					case (char)0:
						return null;

					case '@':
						string s = this.ParseName();

						switch (s.ToLower())
						{
							case "base":
								ch = this.NextNonWhitespaceChar();
								if (ch != '<')
									throw this.ParsingException("Expected <");

								this.baseUri = this.ParseUri().Uri;

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

								this.namespaces[s] = this.ParseUri().Uri.ToString();

								if (this.NextNonWhitespaceChar() != '.')
									throw this.ParsingException("Expected .");

								break;

							default:
								throw this.ParsingException("Unrecognized keyword.");
						}
						break;

					case '[':
						switch (TriplePosition)
						{
							case 0:
								BlankNode Node = this.CreateBlankNode();
								this.ParseTriples(Node);

								this.SkipWhiteSpace();
								if (this.PeekNextChar() == '.')
								{
									this.pos++;
									ISemanticElement Result = this.ParseElement(0, out AdditionalTriples);
									if (!(AdditionalTriples is null))
										this.triples.AddRange(AdditionalTriples);
									return Result;
								}

								return Node;

							case 1:
								throw this.ParsingException("Predicate cannot be a blank node.");

							case 2:
								ChunkedList<ISemanticTriple> Bak = this.triples;
								this.triples = AdditionalTriples = new ChunkedList<ISemanticTriple>();

								Node = this.CreateBlankNode();
								this.ParseTriples(Node);

								this.triples = Bak;

								return Node;

							default:
								throw this.ParsingException("Unrecognized triple position.");
						}

					case '(':
						return this.ParseCollection(out AdditionalTriples);

					case ']':
						return null;

					case '<':
						if (this.PeekNextChar() == '<') // Quoted triples, part of RDF-star
						{
							this.pos++;

							ISemanticElement Subject = this.ParseElement(0, out ChunkedList<ISemanticTriple> AdditionalTriples1);
							ISemanticElement Predicate = this.ParseElement(1, out ChunkedList<ISemanticTriple> AdditionalTriples2);
							ISemanticElement Object = this.ParseElement(2, out ChunkedList<ISemanticTriple> AdditionalTriples3);

							if (this.NextNonWhitespaceChar() != '>')
								throw this.ParsingException("Expected >");

							if (this.NextNonWhitespaceChar() != '>')
								throw this.ParsingException("Expected >");

							if (!(AdditionalTriples1 is null))
								this.triples.AddRange(AdditionalTriples1);

							if (!(AdditionalTriples2 is null))
								this.triples.AddRange(AdditionalTriples2);

							if (!(AdditionalTriples3 is null))
								this.triples.AddRange(AdditionalTriples3);

							return new SemanticTriple(Subject, Predicate, Object);
						}
						else
							return this.ParseUri();

					case '"':
					case '\'':
						if (TriplePosition != 2)
							throw this.ParsingException("Literals can only occur in object position.");

						if (this.pos < this.len - 1 && this.text[this.pos] == ch && this.text[this.pos + 1] == ch)
						{
							this.pos += 2;
							s = this.ParseString(ch, true, true);
						}
						else
							s = this.ParseString(ch, false, true);

						string Language = null;

						if (this.pos < this.len && this.text[this.pos] == '@')
						{
							this.pos++;
							Language = this.ParseName();
						}

						if (this.pos < this.len - 1 && this.text[this.pos] == '^' && this.text[this.pos + 1] == '^')
						{
							this.pos += 2;

							string DataType = this.ParseUriOrPrefixedToken().Uri.ToString();

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
						return this.ParsePrefixedToken(string.Empty);

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
								return this.ParsePrefixedToken(s);
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

		private ISemanticElement ParseCollection(out ChunkedList<ISemanticTriple> AdditionalTriples)
		{
			ChunkedList<ISemanticElement> Elements = null;
			AdditionalTriples = null;

			this.SkipWhiteSpace();

			while (this.pos < this.len)
			{
				if (this.text[this.pos] == ')')
				{
					this.pos++;

					if (Elements is null)
						return RdfDocument.RdfNil;

					ChunkNode<ISemanticElement> Loop = Elements.FirstChunk;
					BlankNode Result = this.CreateBlankNode();
					BlankNode Current = Result;
					int i, c;

					while (!(Loop is null))
					{
						for (i = Loop.Start, c = Loop.Pos; i < c; i++)
						{
							this.Add(new SemanticTriple(Current, RdfDocument.RdfFirst, Loop[i]));

							if (i < c - 1 || !(Loop.Next is null))
							{
								BlankNode Next = this.CreateBlankNode();
								this.Add(new SemanticTriple(Current, RdfDocument.RdfRest, Next));
								Current = Next;
							}
						}

						Loop = Loop.Next;
					}

					this.Add(new SemanticTriple(Current, RdfDocument.RdfRest, RdfDocument.RdfNil));

					return Result;
				}

				ISemanticElement Element = this.ParseElement(2,
					out ChunkedList<ISemanticTriple> AdditionalTriples2);

				if (!(AdditionalTriples2 is null))
				{
					if (AdditionalTriples is null)
						AdditionalTriples = new ChunkedList<ISemanticTriple>();

					AdditionalTriples.AddRange(AdditionalTriples2);
				}

				if (Element is null)
					break;

				if (Elements is null)
					Elements = new ChunkedList<ISemanticElement>();

				Elements.Add(Element);
				this.SkipWhiteSpace();
			}

			throw this.ParsingException("Expected )");
		}

		private UriNode ParseUriOrPrefixedToken()
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

		private UriNode ParsePrefixedToken(string Prefix)
		{
			if (!this.namespaces.TryGetValue(Prefix, out string Namespace))
				throw this.ParsingException("Prefix unknown.");

			this.SkipWhiteSpace();

			string LocalName = this.ParseName();

			return new UriNode(new Uri(Namespace + LocalName), Prefix + ":" + LocalName);
		}

		private string ParseName()
		{
			if (!IsNameStartChar(this.PeekNextChar()))
				return string.Empty;

			int Start = this.pos++;
			bool LastPeriod = false;

			while (IsNameChar(this.PeekNextChar(), ref LastPeriod))
				this.pos++;

			if (LastPeriod)
				this.pos--;

			return this.text.Substring(Start, this.pos - Start);
		}

		/// <summary>
		/// Checks if a character is a character that can start a name.
		/// </summary>
		/// <param name="ch">Character</param>
		/// <returns>If the character can start a name.</returns>
		public static bool IsNameStartChar(char ch)
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

		/// <summary>
		/// Checks if a character can be included in a name.
		/// </summary>
		/// <param name="ch">Character</param>
		/// <param name="LastPeriod">If the last character was a period character.</param>
		/// <returns>If characters can be included in a name.</returns>
		public static bool IsNameChar(char ch, ref bool LastPeriod)
		{
			if (IsNameStartChar(ch))
			{
				LastPeriod = false;
				return true;
			}
			else if (ch < '-')
				return false;
			else if (ch == '-')
			{
				LastPeriod = false;
				return true;
			}
			else if (ch == '.')
			{
				LastPeriod = true;
				return true;
			}
			else if (ch < '0')
				return false;
			else if (ch <= '9')
			{
				LastPeriod = false;
				return true;
			}
			else if (ch < '\x00B7')
				return false;
			else if (ch == '\x00B7')
			{
				LastPeriod = false;
				return true;
			}
			else if (ch < '\x0300')
				return false;
			else if (ch <= '\x036F')
			{
				LastPeriod = false;
				return true;
			}
			else if (ch < '\x203F')
				return false;
			else if (ch <= '\x2040')
			{
				LastPeriod = false;
				return true;
			}
			else
				return false;
		}

		private SemanticLiteral ParseNumber()
		{
			int Start = this.pos;
			char ch = this.PeekNextChar();
			bool HasDigits = false;
			bool HasDecimal = false;
			bool HasExponent = false;

			if (ch == '+' || ch == '-')
			{
				this.pos++;
				ch = this.PeekNextChar();
			}

			while (char.IsDigit(ch))
			{
				this.pos++;
				ch = this.PeekNextChar();
				HasDigits = true;
			}

			if (ch == '.')
			{
				HasDecimal = true;
				this.pos++;
				ch = this.PeekNextChar();

				while (char.IsDigit(ch))
				{
					this.pos++;
					ch = this.PeekNextChar();
				}
			}

			if (ch == 'e' || ch == 'E')
			{
				HasExponent = true;
				this.pos++;
				ch = this.PeekNextChar();

				if (ch == '+' || ch == '-')
				{
					this.pos++;
					ch = this.PeekNextChar();
				}

				while (char.IsDigit(ch))
				{
					this.pos++;
					ch = this.PeekNextChar();
				}
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

		private string ParseString(char EndChar, bool MultiLine, bool IncludeWhiteSpace)
		{
			StringBuilder sb = null;
			int Start = this.pos;
			char ch;

			while ((ch = this.PeekNextChar()) != (char)0)
			{
				this.pos++;

				if (ch == EndChar)
				{
					if (MultiLine)
					{
						if (this.pos < this.len - 1 && this.text[this.pos] == EndChar && this.text[this.pos + 1] == EndChar)
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
								throw this.ParsingException("Expected 8-character hexadecimal code.");
							break;

						default:
							sb.Append(ch);
							break;
					}
				}
				else if (IncludeWhiteSpace || !char.IsWhiteSpace(ch))
					sb?.Append(ch);
			}

			throw this.ParsingException("Expected " + new string(EndChar, MultiLine ? 3 : 1));
		}

		private ParsingException ParsingException(string Message)
		{
			return new ParsingException(Message, this.text, this.pos);
		}

		private UriNode ParseUri()
		{
			string Short = this.ParseString('>', false, false);

			if (this.baseUri is null)
			{
				if (Uri.TryCreate(Short, UriKind.RelativeOrAbsolute, out Uri URI))
					return new UriNode(URI, Short);
				else
					throw this.ParsingException("Invalid URI.");
			}
			else
			{
				if (string.IsNullOrEmpty(Short))
					return new UriNode(this.baseUri, Short);
				else if (Uri.TryCreate(this.baseUri, Short, out Uri URI))
					return new UriNode(URI, Short);
				else
					throw this.ParsingException("Invalid URI.");
			}
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

		private string PeekNextChars(int NrChars)
		{
			if (this.pos + NrChars <= this.len)
				return this.text.Substring(this.pos, NrChars);
			else
				return string.Empty;
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

		/// <summary>
		/// Decodes meta-information available in the HTTP Response.
		/// </summary>
		/// <param name="HttpResponse">HTTP Response.</param>
		public Task DecodeMetaInformation(HttpResponseMessage HttpResponse)
		{
			this.date = HttpResponse.Headers.Date;
			return Task.CompletedTask;
		}
	}
}
