using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Waher.Content.Asn1.Exceptions;
using Waher.Content.Asn1.Model;
using Waher.Content.Asn1.Model.Restrictions;
using Waher.Content.Asn1.Model.Sets;
using Waher.Content.Asn1.Model.Types;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Represents an ASN.1 document.
	/// </summary>
	public class Asn1Document
	{
		private readonly Asn1Node root;
		private readonly string text;
		private readonly int len;
		private int pos = 0;

		/// <summary>
		/// Represents an ASN.1 document.
		/// </summary>
		/// <param name="Text">ASN.1 text to parse.</param>
		public Asn1Document(string Text)
		{
			this.text = Text;
			this.len = this.text.Length;
			this.root = this.ParseDefinitions();
		}

		/// <summary>
		/// ASN.1 Root node
		/// </summary>
		public Asn1Node Root => this.root;

		/// <summary>
		/// ASN.1 text
		/// </summary>
		public string Text => this.text;

		private void SkipWhiteSpace()
		{
			char ch;

			while (this.pos < this.len && ((ch = this.text[this.pos]) <= ' ' || ch == (char)160))
				this.pos++;
		}

		private char NextChar()
		{
			if (this.pos < this.len)
				return this.text[this.pos++];
			else
				return (char)0;
		}

		private char PeekNextChar()
		{
			if (this.pos < this.len)
				return this.text[this.pos];
			else
				return (char)0;
		}

		private string NextToken()
		{
			this.SkipWhiteSpace();

			char ch = this.NextChar();

			if (char.IsLetter(ch) || char.IsDigit(ch) || ch == '-' || ch == '_')
			{
				int Start = this.pos - 1;

				while (this.pos < this.len && (char.IsLetter(ch = this.text[this.pos]) || char.IsDigit(ch) || ch == '-' || ch == '_'))
					this.pos++;

				return this.text.Substring(Start, this.pos - Start);
			}
			else
			{
				switch (ch)
				{
					case ':':
						switch (this.PeekNextChar())
						{
							case ':':
								this.pos++;
								switch (this.PeekNextChar())
								{
									case '=':
										this.pos++;
										return "::=";

									default:
										return "::";
								}

							default:
								return new string(ch, 1);
						}

					case '.':
						if (this.PeekNextChar() == '.')
						{
							this.pos++;
							if (this.PeekNextChar() == '.')
							{
								this.pos++;
								return "...";
							}
							else
								return "..";
						}
						else
							return ".";

					case '-':
						switch (this.PeekNextChar())
						{
							case '-':
								this.pos++;
								while (this.pos < this.len && (ch = this.text[this.pos]) != '\r' && ch != 'n')
									this.pos++;

								return this.NextToken();

							default:
								return new string(ch, 1);
						}

					default:
						return new string(ch, 1);
				}
			}
		}

		private string PeekNextToken()
		{
			this.SkipWhiteSpace();

			int Bak = this.pos;
			string s = this.NextToken();
			this.pos = Bak;
			return s;
		}

		private Asn1SyntaxException SyntaxError(string Message)
		{
			return new Asn1SyntaxException(Message, this.text, this.pos);
		}

		private Asn1Node ParseDefinitions()
		{
			string Identifier = this.ParseTypeNameIdentifier();
			string s = this.PeekNextToken();
			Asn1Oid Oid = null;

			if (s == "{")
			{
				Oid = this.ParseOid();
				s = this.PeekNextToken();
			}

			if (s != "DEFINITIONS")
			{
				if (!(Oid is null))
					throw this.SyntaxError("DEFINITIONS expected.");

				return new Asn1TypeReference(Identifier);
			}

			this.pos += 11;

			Asn1Tags? Tags = null;
			bool Abstract = false;

			while (true)
			{
				switch (s = this.NextToken())
				{
					case "AUTOMATIC":
					case "IMPLICIT":
					case "EXPLICIT":
						if (Tags.HasValue)
							throw this.SyntaxError("TAGS already specified.");

						switch (s)
						{
							case "AUTOMATIC":
								Tags = Asn1Tags.Automatic;
								break;

							case "IMPLICIT":
								Tags = Asn1Tags.Implicit;
								break;

							case "EXPLICIT":
								Tags = Asn1Tags.Explicit;
								break;
						}

						s = this.NextToken();
						if (s != "TAGS")
							throw this.SyntaxError("TAGS expected.");
						break;

					case "ABSTRACT-SYNTAX":
						Abstract = true;
						break;

					case "::=":
						s = null;
						break;

					default:
						throw this.SyntaxError("::= expected.");
				}

				if (s is null)
					break;
			}

			Asn1Node Body = this.ParseModule();

			return new Asn1Definitions(Identifier, Oid, Tags, Abstract, Body);
		}

		private Asn1Node ParseModule()
		{
			string s = this.PeekNextToken();
			if (s != "BEGIN")
				return this.ParseStatement();

			this.pos += 5;

			List<Asn1Import> Imports = null;
			List<string> Exports = null;

			do
			{
				s = this.PeekNextToken();

				if (s == "IMPORTS")
				{
					this.pos += 7;

					if (Imports is null)
						Imports = new List<Asn1Import>();

					do
					{
						string Identifier = this.ParseTypeNameIdentifier();
						string ModuleRef = null;

						s = this.PeekNextToken();

						if (s == "FROM")
						{
							this.pos += 4;
							ModuleRef = this.ParseTypeNameIdentifier();
							s = this.PeekNextToken();
						}
						else if (s == ".")
						{
							this.pos++;
							ModuleRef = Identifier;
							Identifier = this.ParseTypeNameIdentifier();
							s = this.PeekNextToken();
						}

						Imports.Add(new Asn1Import(Identifier, ModuleRef));

						if (s == ",")
						{
							this.pos++;
							continue;
						}
						else if (s == ";")
						{
							this.pos++;
							break;
						}
						else
							throw this.SyntaxError("; expected");
					}
					while (true);
				}
				else if (s == "EXPORTS")
				{
					this.pos += 7;

					if (Exports is null)
						Exports = new List<string>();

					do
					{
						string Identifier = this.ParseTypeNameIdentifier();
						Exports.Add(Identifier);

						s = this.PeekNextToken();

						if (s == ",")
						{
							this.pos++;
							continue;
						}
						else if (s == ";")
						{
							this.pos++;
							break;
						}
						else
							throw this.SyntaxError("; expected");
					}
					while (true);
				}
				else
					break;
			}
			while (true);

			List<Asn1Node> Items = new List<Asn1Node>();

			while ((s = this.PeekNextToken()) != "END")
			{
				if (string.IsNullOrEmpty(s))
					throw this.SyntaxError("END expected.");

				Items.Add(this.ParseStatement());
			}

			this.pos += 3;

			return new Asn1Module(Imports?.ToArray(), Exports?.ToArray(), Items.ToArray());
		}

		private Asn1Node ParseStatement()
		{
			string s = this.PeekNextToken();
			if (string.IsNullOrEmpty(s))
				throw this.SyntaxError("Unexpected end of file.");

			char ch = s[0];
			string s2;

			if (char.IsLetter(ch))
			{
				this.pos += s.Length;
				s2 = this.PeekNextToken();

				if (char.IsUpper(ch))   // Type
				{
					if (s2 != "::=")
						throw this.SyntaxError("::= expected.");

					this.pos += s2.Length;
					Asn1Type TypeDefinition = this.ParseType(s, true);

					return new Asn1TypeDefinition(s, TypeDefinition);
				}
				else                    // name
				{
					if (s2 == "::=")    // XML notation
						throw this.SyntaxError("XML notation not supported.");

					int? Tag = null;

					if (s2 == "[")
					{
						this.pos++;
						s2 = this.NextToken();
						if (!int.TryParse(s2, out int i))
							throw this.SyntaxError("Tag expected.");

						Tag = i;

						if (this.NextToken() != "]")
							throw this.SyntaxError("] expected.");

						s2 = this.PeekNextToken();
					}

					if (!IsTypeIdentifier(s2))
						throw this.SyntaxError("Type name expected.");

					Asn1Type Type = this.ParseType(s, false);
					Asn1Restriction Restriction = null;
					List<Asn1NamedValue> NamedOptions = null;
					bool? Optional = false;
					bool? Unique = false;
					bool? Present = false;
					bool? Absent = false;
					Asn1Node Default = null;

					while (true)
					{
						s2 = this.PeekNextToken();

						switch (s2)
						{
							case "::=":
								this.pos += 3;
								Asn1Node Value = this.ParseValue();
								return new Asn1FieldValueDefinition(s, s2, Value);

							case "(":
								Restriction = this.ParseRestriction();
								break;

							case "{":
								this.pos++;

								NamedOptions = new List<Asn1NamedValue>();

								while (true)
								{
									s2 = this.NextToken();
									if (!IsFieldIdentifier(s2))
										throw this.SyntaxError("Value name expected.");

									if (this.PeekNextToken() == "(")
									{
										this.pos++;

										NamedOptions.Add(new Asn1NamedValue(s2, this.ParseValue()));

										if (this.NextToken() != ")")
											throw this.SyntaxError(") expected");
									}
									else
										NamedOptions.Add(new Asn1NamedValue(s2, null));

									s2 = this.NextToken();

									if (s2 == ",")
										continue;
									else if (s2 == "}")
										break;
									else
										throw this.SyntaxError("Unexpected token.");
								}
								break;

							case "OPTIONAL":
								this.pos += 8;
								Optional = true;
								break;

							case "PRESENT":
								this.pos += 7;
								Present = true;
								break;

							case "ABSENT":
								this.pos += 6;
								Absent = true;
								break;

							case "DEFAULT":
								this.pos += 7;
								Optional = true;
								Default = this.ParseValue();
								break;

							case "UNIQUE":
								this.pos += 6;
								Unique = true;
								break;

							default:
								return new Asn1FieldDefinition(s, Tag, Type, Restriction,
									Optional, Unique, Present, Absent, Default,
									NamedOptions?.ToArray());
						}
					}

				}
			}
			else if (s == "...")
			{
				this.pos += 3;
				return new Asn1Extension();
			}
			else
				throw this.SyntaxError("Identifier expected.");
		}


		private Asn1Restriction ParseRestriction()
		{
			if (this.NextToken() != "(")
				throw this.SyntaxError("( expected.");

			Asn1Restriction Result = this.ParseOrs();

			if (this.NextToken() != ")")
				throw this.SyntaxError(") expected.");

			return Result;
		}

		private Asn1Restriction ParseOrs()
		{
			Asn1Restriction Result = this.ParseAnds();

			string s = this.PeekNextToken();

			while (s == "|")
			{
				this.pos++;
				Result = new Asn1Or(Result, this.ParseAnds());
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1Restriction ParseAnds()
		{
			Asn1Restriction Result = this.ParseRestrictionRule();

			string s = this.PeekNextToken();

			while (s == "^")
			{
				this.pos++;
				Result = new Asn1And(Result, this.ParseRestrictionRule());
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1Restriction ParseRestrictionRule()
		{
			string s = this.PeekNextToken();

			switch (s)
			{
				case "(":
					this.pos++;

					Asn1Restriction Result = this.ParseOrs();

					if (this.NextToken() != ")")
						throw this.SyntaxError(") expected.");

					return Result;

				case "SIZE":
					this.pos += 4;
					return new Asn1Size(this.ParseSet());

				case "PATTERN":
					this.pos += 7;
					return new Asn1Pattern(this.ParseValue());

				case "FROM":
					this.pos += 4;
					return new Asn1From(this.ParseSet());

				case "CONTAINING":
					this.pos += 10;
					return new Asn1Containing(this.ParseValue());

				case "ENCODED":
					this.pos += 7;
					if (this.NextToken() != "BY")
						throw this.SyntaxError("BY expected.");

					return new Asn1EncodedBy(this.ParseValue());

				case "WITH":
					this.pos += 4;
					if (this.NextToken() != "COMPONENTS")
						throw this.SyntaxError("COMPONENTS expected.");

					return new Asn1WithComponents(this.ParseValue());

				default:
					return new Asn1InSet(this.ParseUnions());
			}
		}

		private Asn1Values ParseSet()
		{
			if (this.NextToken() != "(")
				throw this.SyntaxError("( expected.");

			Asn1Values Result = this.ParseUnions();

			if (this.NextToken() != ")")
				throw this.SyntaxError(") expected.");

			return Result;
		}

		private Asn1Values ParseUnions()
		{
			Asn1Values Result = this.ParseIntersections();

			string s = this.PeekNextToken();

			while (s == "|" || s == "UNION")
			{
				this.pos += s.Length;
				Result = new Asn1Union(Result, this.ParseIntersections());
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1Values ParseIntersections()
		{
			Asn1Values Result = this.ParseIntervals();

			string s = this.PeekNextToken();

			while (s == "^" || s == "INTERSECTION")
			{
				this.pos += s.Length;
				Result = new Asn1Union(Result, this.ParseIntervals());
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1Values ParseIntervals()
		{
			string s = this.PeekNextToken();

			if (s == "(")
			{
				this.pos++;

				Asn1Values Result = this.ParseUnions();

				if (this.NextToken() != ")")
					throw this.SyntaxError(") expected.");

				return Result;
			}
			else if (s == "ALL")
			{
				this.pos += 3;
				return new Asn1All();
			}
			else
			{
				Asn1Value Value = this.ParseValue();

				if (this.PeekNextToken() == "..")
				{
					this.pos += 2;
					Asn1Value Value2 = this.ParseValue();

					return new Asn1Interval(Value, Value2);
				}
				else
					return new Asn1Element(Value);
			}
		}

		private string ParseTypeNameIdentifier()
		{
			string s = this.NextToken();

			if (!IsTypeIdentifier(s))
				throw this.SyntaxError("Typer name identifier expected.");

			return s;
		}

		private static bool IsTypeIdentifier(string s)
		{
			char ch;
			return !string.IsNullOrEmpty(s) && char.IsLetter(ch = s[0]) && char.IsUpper(ch);
		}

		private static bool IsFieldIdentifier(string s)
		{
			char ch;
			return !string.IsNullOrEmpty(s) && char.IsLetter(ch = s[0]) && char.IsLower(ch);
		}

		private Asn1Oid ParseOid()
		{
			Asn1Node[] Values = this.ParseValues();
			return new Asn1Oid(Values);
		}

		private Asn1Type ParseType(string Name, bool TypeDef)
		{
			string s = this.NextToken();
			if (string.IsNullOrEmpty(s))
				throw this.SyntaxError("Unexpected end of file.");

			switch (s)
			{
				case "BIT":
					if (this.PeekNextToken() == "STRING")
						this.pos += 6;
					else
						throw this.SyntaxError("STRING expected.");

					return new Asn1BitString();

				case "BMPString":
					return new Asn1BmpString();

				case "BOOLEAN":
					return new Asn1Boolean();

				case "CHARACTER":
					return new Asn1Character();

				case "CHOICE":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList();
						return new Asn1Choice(Name, TypeDef, Nodes);
					}
					else
						throw this.SyntaxError("{ expected.");

				case "DATE":
					return new Asn1Date();

				case "DATE-TIME":
					return new Asn1DateTime();

				case "ENUMERATED":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseValues();
						return new Asn1Enumeration(Name, TypeDef, Nodes);
					}
					else
						throw this.SyntaxError("{ expected.");

				case "GeneralizedTime":
					return new Asn1GeneralizedTime();

				case "GeneralString":
					return new Asn1GeneralString();

				case "GraphicString":
					return new Asn1GraphicString();

				case "IA5String":
					return new Asn1Ia5String();

				case "INTEGER":
					return new Asn1Integer();

				case "ISO646String":
					return new Asn1Iso646String();

				case "NULL":
					return new Asn1Null();

				case "NumericString":
					return new Asn1NumericString();

				case "OBJECT":
					if (this.PeekNextToken() == "IDENTIFIER")
						this.pos += 10;
					else
						throw this.SyntaxError("IDENTIFIER expected.");

					return new Asn1ObjectIdentifier();

				case "OCTET":
					if (this.PeekNextToken() == "STRING")
						this.pos += 6;
					else
						throw this.SyntaxError("STRING expected.");

					return new Asn1OctetString();

				case "PrintableString":
					return new Asn1PrintableString();

				case "REAL":
					return new Asn1Real();

				case "SET":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList();
						return new Asn1Set(Name, TypeDef, Nodes);
					}
					else if (s == "(")
					{
						this.pos++;

						Asn1Values Size = null;

						while (true)
						{
							s = this.PeekNextToken();

							if (s == "SIZE")
							{
								if (!(Size is null))
									throw this.SyntaxError("SIZE already specified.");

								this.pos += 4;
								Size = this.ParseSet();
							}
							else if (s == ")")
							{
								this.pos++;
								break;
							}
							else
								throw this.SyntaxError("Unexpected token.");
						}

						if (this.NextToken() != "OF")
							throw this.SyntaxError("OF expected.");

						if (Size is null)
							throw this.SyntaxError("SIZE expected.");

						s = this.ParseTypeNameIdentifier();

						return new Asn1SetOf(Size, s);
					}
					else
						throw this.SyntaxError("{ expected.");

				case "SEQUENCE":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList();
						return new Asn1Sequence(Name, TypeDef, Nodes);
					}
					else
					{
						Asn1Values Size = null;

						if (s == "(")
						{
							this.pos++;

							while (true)
							{
								s = this.PeekNextToken();

								if (s == "SIZE")
								{
									if (!(Size is null))
										throw this.SyntaxError("SIZE already specified.");

									this.pos += 4;
									Size = this.ParseSet();
								}
								else if (s == ")")
								{
									this.pos++;
									break;
								}
								else
									throw this.SyntaxError("Unexpected token.");
							}
						}

						if (this.NextToken() != "OF")
							throw this.SyntaxError("Unexpected token.");

						return new Asn1SequenceOf(Name, TypeDef, Size, this.ParseType(Name, TypeDef));
					}

				case "T61String":
					return new Asn1T61String();

				case "TeletexString":
					return new Asn1TeletexString();

				case "TIME-OF-DAY":
					return new Asn1TimeOfDay();

				case "UniversalString":
					return new Asn1UniversalString();

				case "UTCTime":
					return new Asn1UtcTime();

				case "UTF8String":
					return new Asn1Utf8String();

				case "VideotexString":
					return new Asn1VideotexString();

				case "VisibleString":
					return new Asn1VisibleString();

				case "ObjectDescriptor":
				case "EXTERNAL":
				case "EMBEDDED":
				case "RELATIVE-OID":
				case "CLASS":
				case "COMPONENTS":
				case "INSTANCE":
					throw this.SyntaxError("Token not implemented.");

				default:
					if (char.IsUpper(s[0]))
						return new Asn1TypeReference(s);
					else
						throw this.SyntaxError("Type name expected.");
			}
		}

		private Asn1Node[] ParseList()
		{
			if (this.NextToken() != "{")
				throw this.SyntaxError("{ expected.");

			List<Asn1Node> Items = new List<Asn1Node>();

			while (true)
			{
				Items.Add(this.ParseStatement());

				switch (this.PeekNextToken())
				{
					case ",":
						this.pos++;
						continue;

					case "}":
						this.pos++;
						return Items.ToArray();

					default:
						throw this.SyntaxError(", or } expected.");
				}
			}
		}

		private Asn1Node[] ParseValues()
		{
			if (this.NextToken() != "{")
				throw this.SyntaxError("{ expected.");

			List<Asn1Node> Items = new List<Asn1Node>();

			while (true)
			{
				Items.Add(this.ParseValue());

				switch (this.PeekNextToken())
				{
					case ",":
						this.pos++;
						continue;

					case "}":
						this.pos++;
						return Items.ToArray();

					default:
						throw this.SyntaxError(", or } expected.");
				}
			}
		}

		private Asn1Value ParseValue()
		{
			return this.ParseValue(true);
		}

		private Asn1Value ParseValue(bool AllowNamed)
		{
			string s = this.PeekNextToken();
			if (string.IsNullOrEmpty(s))
				throw this.SyntaxError("Expected value.");

			switch (s)
			{
				case "FALSE":
					this.pos += 5;
					return new Asn1BooleanValue(false);

				case "MAX":
					this.pos += 3;
					return new Asn1Max();

				case "MIN":
					this.pos += 3;
					return new Asn1Min();

				case "TRUE":
					this.pos += 4;
					return new Asn1BooleanValue(true);

				case "...":
					this.pos += 3;
					return new Asn1Extension();

				case "\"":
					int Start = ++this.pos;
					char ch;

					while (this.pos < this.len && this.text[this.pos] != '"')
						this.pos++;

					if (this.pos >= this.len)
						throw this.SyntaxError("\" expected.");

					s = this.text.Substring(Start, this.pos - Start);
					this.pos++;

					return new Asn1StringValue(s);

				case "'":
					Start = ++this.pos;

					while (this.pos < this.len && this.text[this.pos] != '\'')
						this.pos++;

					if (this.pos >= this.len)
						throw this.SyntaxError("' expected.");

					s = this.text.Substring(Start, this.pos - Start);
					this.pos++;

					switch (this.NextToken())
					{
						case "H":
						case "h":
							this.pos++;
							if (long.TryParse(s, NumberStyles.HexNumber, null, out long l))
								return new Asn1IntegerValue(l);
							else
								throw this.SyntaxError("Invalid hexadecimal string.");

						case "D":
						case "d":
							this.pos++;
							if (long.TryParse(s, out l))
								return new Asn1IntegerValue(l);
							else
								throw this.SyntaxError("Invalid decimal string.");

						case "B":
						case "b":
							this.pos++;
							if (TryParseBinary(s, out l))
								return new Asn1IntegerValue(l);
							else
								throw this.SyntaxError("Invalid binary string.");

						case "O":
						case "o":
							this.pos++;
							if (TryParseOctal(s, out l))
								return new Asn1IntegerValue(l);
							else
								throw this.SyntaxError("Invalid octal string.");

						default:
							throw this.SyntaxError("Unexpected token.");
					}

				case "{":
					this.pos++;

					List<Asn1Value> Items = new List<Asn1Value>();

					while (true)
					{
						Asn1Value Value = this.ParseValue();

						switch (this.PeekNextToken())
						{
							case ",":
								this.pos++;
								Items.Add(Value);
								continue;

							case "}":
								this.pos++;
								Items.Add(Value);
								return new Asn1Array(Items.ToArray());

							default:
								if (Value is Asn1ValueReference Identifier)
								{
									Value = this.ParseValue();
									Items.Add(new Asn1NamedValue(Identifier.Identifier, Value));
									continue;
								}
								else
									throw this.SyntaxError("Unexpected token.");
						}
					}

				default:
					if (char.IsLetter(s[0]))
					{
						this.pos += s.Length;

						if (this.PeekNextToken() == ":")
						{
							if (!AllowNamed)
								throw this.SyntaxError("Value expected.");

							this.pos++;
							Asn1Value Value = this.ParseValue(false);
							return new Asn1NamedValue(s, Value);
						}
						else if (!char.IsUpper(s[0]))
							return new Asn1ValueReference(s);
						else
							throw this.SyntaxError("Type references not permitted here.");
					}
					else if (long.TryParse(s, out long l))
					{
						Start = this.pos;
						this.pos += s.Length;

						ch = this.PeekNextChar();

						if ((ch == '.' && this.pos < this.len - 1 && this.text[this.pos + 1] != '.') ||
							ch == 'e' || ch == 'E')
						{
							int? DecPos = null;

							if (ch == '.')
							{
								DecPos = this.pos++;
								while (this.pos < this.len && char.IsDigit(ch = this.text[this.pos]))
									this.pos++;
							}

							if (ch == 'e' || ch == 'E')
							{
								this.pos++;

								if ((ch = this.PeekNextChar()) == '-' || ch == '+')
									this.pos++;

								while (this.pos < this.len && char.IsDigit(this.text[this.pos]))
									this.pos++;
							}

							s = this.text.Substring(Start, this.pos - Start);

							string s2 = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
							if (DecPos.HasValue && s2 != ".")
								s = s.Replace(".", s2);

							if (!double.TryParse(s, out double d))
								throw this.SyntaxError("Invalid floating-point number.");

							return new Asn1FloatingPointValue(d);
						}

						return new Asn1IntegerValue(l);
					}
					break;
			}

			throw this.SyntaxError("Value expected.");
		}

		private static bool TryParseBinary(string s, out long l)
		{
			l = 0;

			foreach (char ch in s)
			{
				if (ch < '0' || ch > '1')
					return false;

				long l2 = l;
				l <<= 1;
				if (l < l2)
					return false;

				l |= (byte)(ch - '0');
			}

			return true;
		}

		private static bool TryParseOctal(string s, out long l)
		{
			l = 0;

			foreach (char ch in s)
			{
				if (ch < '0' || ch > '7')
					return false;

				long l2 = l;
				l <<= 3;
				if (l < l2)
					return false;

				l |= (byte)(ch - '0');
			}

			return true;
		}

		/// <summary>
		/// Exports ASN.1 schemas to C#
		/// </summary>
		/// <param name="Namespace">What namespace to generate classes in.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <returns>C# code</returns>
		public string ExportCSharp(string Namespace, CSharpExportSettings Settings)
		{
			StringBuilder Output = new StringBuilder();
			this.ExportCSharp(Output, Namespace, Settings);
			return Output.ToString();
		}

		/// <summary>
		/// Exports ASN.1 schemas to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Namespace">What namespace to generate classes in.</param>
		/// <param name="Settings">C# export settings.</param>
		public void ExportCSharp(StringBuilder Output, string Namespace, CSharpExportSettings Settings)
		{
			Output.AppendLine("using System;");
			Output.AppendLine("using System.Text;");
			Output.AppendLine("using System.Collections.Generic;");
			Output.AppendLine("using Waher.Content.Asn1;");
			Output.AppendLine();
			Output.Append("namespace ");
			Output.AppendLine(Namespace);
			Output.AppendLine("{");

			this.root?.ExportCSharp(Output, Settings, 1);

			Output.AppendLine("}");
		}
	}
}
