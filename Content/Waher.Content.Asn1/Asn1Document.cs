using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Waher.Content.Asn1.Exceptions;
using Waher.Content.Asn1.Model;
using Waher.Content.Asn1.Model.Macro;
using Waher.Content.Asn1.Model.Restrictions;
using Waher.Content.Asn1.Model.Sets;
using Waher.Content.Asn1.Model.Types;
using Waher.Content.Asn1.Model.Values;
using Waher.Runtime.IO;
using Waher.Runtime.Collections;
using System;
using System.Formats.Asn1;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Represents an ASN.1 document.
	/// </summary>
	public class Asn1Document : Asn1Node
	{
		internal readonly Dictionary<string, Asn1Node> namedNodes = new Dictionary<string, Asn1Node>();
		internal readonly Dictionary<string, Asn1TypeDefinition> aliases = new Dictionary<string, Asn1TypeDefinition>();
		internal readonly Dictionary<string, Asn1FieldValueDefinition> values = new Dictionary<string, Asn1FieldValueDefinition>();
		internal int pos = 0;
		private readonly string text;
		private readonly string location;
		private readonly string[] importFolders;
		private readonly int len;
		private readonly int lenm1;
		private Asn1Definitions root;
		private int unnamedIndex = 1;

		private Asn1Document(string Text, string Location, string[] ImportFolders)
			: base(null)
		{
			this.text = Text;
			this.location = Location;
			this.importFolders = ImportFolders;
			this.len = this.text.Length;
			this.lenm1 = this.len - 1;
			this.Document = this;
		}

		/// <summary>
		/// Represents an ASN.1 document.
		/// </summary>
		/// <param name="Text">ASN.1 text to parse.</param>
		/// <param name="Location">Location of file.</param>
		/// <param name="ImportFolders">Import Folders.</param>
		public static async Task<Asn1Document> CreateAsync(string Text, string Location, string[] ImportFolders)
		{
			Asn1Document Result = new Asn1Document(Text, Location, ImportFolders);
			Result.root = await Result.ParseDefinitions();
			Result.root.SetParentNode(Result);
			return Result;
		}

		/// <summary>
		/// ASN.1 Root node
		/// </summary>
		public Asn1Definitions Root => this.root;

		/// <summary>
		/// ASN.1 text
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Location of document.
		/// </summary>
		public string Location => this.location;

		/// <summary>
		/// Import folders.
		/// </summary>
		public string[] ImportFolders => this.importFolders;

		/// <summary>
		/// Read from file.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="ImportFolders">Import folders.</param>
		/// <returns>ASN.1 document</returns>
		public static async Task<Asn1Document> FromFile(string FileName, string[] ImportFolders)
		{
			string Text = await Files.ReadAllTextAsync(FileName);
			return await CreateAsync(Text, FileName, ImportFolders);
		}

		internal void SkipWhiteSpace()
		{
			char ch;

			while (this.pos < this.len)
			{
				ch = this.text[this.pos];

				if (ch <= ' ' || ch == (char)160)
					this.pos++;
				else if (ch == '-' && this.pos < this.lenm1 && this.text[this.pos + 1] == '-')
				{
					this.pos += 2;

					while (this.pos < this.len)
					{
						ch = this.text[this.pos];
						if (ch == '\r' || ch == '\n')
							break;

						if (ch == '-' && this.pos < this.len - 1 && this.text[this.pos + 1] == '-')
						{
							this.pos += 2;
							break;
						}

						this.pos++;
					}
				}
				else if (ch == '/' && this.pos < this.lenm1 && this.text[this.pos + 1] == '*')
				{
					this.pos += 2;

					while ((this.pos < this.len && this.text[this.pos] != '*') ||
						(this.pos < this.lenm1 && this.text[this.pos + 1] != '/'))
					{
						this.pos++;
					}
				}
				else
					break;
			}
		}

		internal char NextChar()
		{
			if (this.pos < this.len)
				return this.text[this.pos++];
			else
				return (char)0;
		}

		internal char PeekNextChar()
		{
			if (this.pos < this.len)
				return this.text[this.pos];
			else
				return (char)0;
		}

		internal string NextToken()
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

					case '[':
						if (this.PeekNextChar() == '[')
						{
							this.pos++;
							return "[[";
						}
						else
							return "[";

					case ']':
						if (this.PeekNextChar() == ']')
						{
							this.pos++;
							return "]]";
						}
						else
							return "]";

					default:
						return new string(ch, 1);
				}
			}
		}

		internal string PeekNextToken()
		{
			this.SkipWhiteSpace();

			int Bak = this.pos;
			string s = this.NextToken();
			this.pos = Bak;
			return s;
		}

		internal void AssertNextToken(string ExpectedToken)
		{
			string s = this.NextToken();
			if (s != ExpectedToken)
				throw this.SyntaxError(ExpectedToken + " expected.");
		}

		internal Asn1SyntaxException SyntaxError(string Message)
		{
			return new Asn1SyntaxException(Message, this.text, this.pos);
		}

		private async Task<Asn1Definitions> ParseDefinitions()
		{
			string Identifier = this.ParseTypeNameIdentifier();
			string s = this.PeekNextToken();
			Asn1Oid Oid = null;

			while (true)
			{
				if (s == "{")
				{
					Oid = this.ParseOid();
					s = this.PeekNextToken();

					if (s == "DEFINITIONS")
						break;

					this.namedNodes[Identifier] = Oid;
					Identifier = s;
					this.pos += s.Length;
					s = this.PeekNextToken();
				}
				else if (s == "DEFINITIONS")
					break;
				else
					throw this.SyntaxError("DEFINITIONS expected.");
			}

			this.pos += 11;

			Asn1Tags? Tags = null;
			bool Abstract = false;

			while (true)
			{
				switch (s = this.NextToken())
				{
					case "AUTOMATIC":
						if (Tags.HasValue)
							throw this.SyntaxError("TAGS already specified.");

						Tags = Asn1Tags.Automatic;
						this.AssertNextToken("TAGS");
						break;

					case "IMPLICIT":
						if (Tags.HasValue)
							throw this.SyntaxError("TAGS already specified.");

						Tags = Asn1Tags.Implicit;
						this.AssertNextToken("TAGS");
						break;

					case "EXPLICIT":
						if (Tags.HasValue)
							throw this.SyntaxError("TAGS already specified.");

						Tags = Asn1Tags.Explicit;
						this.AssertNextToken("TAGS");
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

			Asn1Module Body = await this.ParseModule(Tags);

			return new Asn1Definitions(Identifier, Oid, Tags, Abstract, Body, this);
		}

		private async Task<Asn1Module> ParseModule(Asn1Tags? DefaultTags)
		{
			string s;

			this.AssertNextToken("BEGIN");

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

					List<string> Identifiers = new List<string>();

					do
					{
						string Identifier = this.ParseIdentifier();
						string ModuleRef;

						s = this.PeekNextToken();

						if (s == "FROM")
						{
							this.pos += 4;
							Identifiers.Add(Identifier);
							ModuleRef = this.ParseTypeNameIdentifier();

							Imports.Add(new Asn1Import(Identifiers.ToArray(), ModuleRef, this));
							Identifiers.Clear();

							s = this.PeekNextToken();

							if (s == "{")
							{
								Asn1Oid Oid = this.ParseOid();
								if (this.namedNodes.TryGetValue(ModuleRef, out Asn1Node PrevModuleDef))
								{
									if (!Oid.Equals(PrevModuleDef))
										throw this.SyntaxError(ModuleRef + " already defined.");
								}
								else
									this.namedNodes[ModuleRef] = Oid;

								s = this.PeekNextToken();
							}

							if (s != ";")
								continue;
						}
						else if (s == ".")
						{
							this.pos++;
							ModuleRef = Identifier;
							Identifier = this.ParseIdentifier();
							Imports.Add(new Asn1Import(new string[] { ModuleRef }, Identifier, this));
							s = this.PeekNextToken();
						}
						else
							Identifiers.Add(Identifier);

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
							throw this.SyntaxError("Unexpected token.");
					}
					while (true);

					if (Identifiers.Count > 0)
						Imports.Add(new Asn1Import(Identifiers.ToArray(), string.Empty, this));

					foreach (Asn1Import Import in Imports)
					{
						Asn1Document Doc = await Import.LoadDocument();

						foreach (string Identifier in Import.Identifiers)
						{
							if (!Doc.namedNodes.TryGetValue(Identifier, out Asn1Node ImportedNode))
								throw this.SyntaxError(Identifier + " not found in " + Import.Module);

							this.namedNodes[Identifier] = ImportedNode;

							if (Doc.aliases.TryGetValue(Identifier, out Asn1TypeDefinition TypeDef))
								this.aliases[Identifier] = TypeDef;

							if (Doc.values.TryGetValue(Identifier, out Asn1FieldValueDefinition ValueDef))
								this.values[Identifier] = ValueDef;
						}
					}
				}
				else if (s == "EXPORTS")
				{
					this.pos += 7;

					if (Exports is null)
						Exports = new List<string>();

					do
					{
						string Identifier = this.ParseIdentifier();
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

				Asn1Node Node = this.ParseStatement(DefaultTags);
				Items.Add(Node);

				Node.ForEach(Node2 =>
				{
					if (Node2 is INamedNode NamedNode)
					{
						if (this.namedNodes.TryGetValue(NamedNode.Name, out Asn1Node Prev))
						{
							if (NamedNode is Asn1TypeDefinition TypeDef)
								this.namedNodes[NamedNode.Name] = TypeDef;
							else if (!(Prev is Asn1TypeDefinition) && !Prev.Equals(NamedNode))
								throw new Exception("Duplicate type: " + NamedNode.Name);
						}
						else
							this.namedNodes[NamedNode.Name] = Node2;
					}

					return true;
				});
			}

			this.pos += 3;

			return new Asn1Module(Imports?.ToArray(), Exports?.ToArray(), Items.ToArray(), this);
		}

		private Asn1Node ParseStatement(Asn1Tags? DefaultTags)
		{
			string s = this.PeekNextToken();
			if (string.IsNullOrEmpty(s))
				throw this.SyntaxError("Unexpected end of file.");

			char ch = s[0];
			string s2;

			if (char.IsLetter(ch))
			{
				int PosBak = this.pos;

				this.pos += s.Length;
				s2 = this.PeekNextToken();

				if (s2 == "::=")
				{
					if (!char.IsUpper(ch))   // !Type = XML notation
						throw this.SyntaxError("XML notation not supported.");

					this.pos += s2.Length;

					s2 = this.PeekNextToken();
				}
				else if (s2 == "MACRO")
				{
					this.pos += 5;
					this.AssertNextToken("::=");
					this.AssertNextToken("BEGIN");
					this.AssertNextToken("TYPE");
					this.AssertNextToken("NOTATION");
					this.AssertNextToken("::=");

					UserDefinedItem TypeNotation = this.ParseUserDefinedOptions("VALUE", DefaultTags);

					this.AssertNextToken("VALUE");
					this.AssertNextToken("NOTATION");
					this.AssertNextToken("::=");

					UserDefinedItem ValueNotation = this.ParseUserDefinedOptions("END", DefaultTags);
					List<SupportingSyntax> SupportingSyntax = new List<SupportingSyntax>();

					while (this.PeekNextToken() != "END")
					{
						string Name = this.ParseIdentifier();
						this.AssertNextToken("::=");
						SupportingSyntax.Add(new SupportingSyntax(Name, this.ParseUserDefinedOptions("END", DefaultTags)));
					}

					this.AssertNextToken("END");

					return new Asn1Macro(s, TypeNotation, ValueNotation, SupportingSyntax.ToArray(), this);
				}
				else if (this.namedNodes.TryGetValue(s2, out Asn1Node Node) &&
					Node is Asn1Macro Macro)
				{
					this.pos += s2.Length;

					Asn1Value Value = Macro.ParseValue(this);
					Asn1FieldValueDefinition ValueDef = new Asn1FieldValueDefinition(s, Macro.GetValueType(), Value, this);

					this.values[s] = ValueDef;

					return ValueDef;
				}
				else
				{
					if (char.IsUpper(ch))   // Type or macro
					{
						s2 = s;
						s = "unnamed" + (this.unnamedIndex++).ToString();
						ch = 'u';
						this.pos = PosBak;
					}
				}

				TagClass? Class = null;
				int? TagValue = null;

				if (s2 == "[")
				{
					Class = TagClass.ContextSpecific;	// Default class when providing index.

					this.pos++;
					s2 = this.NextToken();

					if (s2 == "TAG")
					{
						this.AssertNextToken(":");
						s2 = this.NextToken();
					}

					switch (s2)
					{
						case "APPLICATION":
							Class = TagClass.Application;
							s2 = this.NextToken();
							break;

						case "PRIVATE":
							Class = TagClass.Private;
							s2 = this.NextToken();
							break;

						case "UNIVERSAL":
							Class = TagClass.Universal;
							s2 = this.NextToken();
							break;
					}

					if (!int.TryParse(s2, out int i))
						throw this.SyntaxError("Tag expected.");

					TagValue = i;

					this.AssertNextToken("]");

					s2 = this.PeekNextToken();
				}

				if (char.IsUpper(ch))   // Type
				{
					Asn1Type Definition;

					if (this.namedNodes.TryGetValue(s2, out Asn1Node Node) &&
						Node is Asn1Macro Macro)
					{
						this.pos += s2.Length;
						Definition = Macro.ParseType(s, this);
					}
					else
						Definition = this.ParseType(s, true, DefaultTags);

					Asn1TypeDefinition TypeDef = new Asn1TypeDefinition(s,
						Class.HasValue && TagValue.HasValue ? 
						new Asn1Tag(Class.Value, TagValue.Value) : (Asn1Tag?)null, 
						Definition, this);

					if (!Definition.ConstructedType)
						this.aliases[s] = TypeDef;

					return TypeDef;
				}
				else                    // name
				{
					if (!IsTypeIdentifier(s2))
						throw this.SyntaxError("Type name expected.");

					Asn1Type Type = this.ParseType(s, false, DefaultTags);

					if (this.PeekNextToken() == "::=")
					{
						this.pos += 3;
						Asn1Value Value = this.ParseValue();
						Asn1FieldValueDefinition ValueDef = new Asn1FieldValueDefinition(s, Type, Value, this);

						this.values[s] = ValueDef;

						return ValueDef;
					}
					else
					{
						return new Asn1FieldDefinition(s,
							Class.HasValue && TagValue.HasValue ?
							new Asn1Tag(Class.Value, TagValue.Value) : (Asn1Tag?)null, 
							Type, this);
					}
				}
			}
			else if (s == "...")
			{
				this.pos += 3;
				return new Asn1Extension(this);
			}
			else
				throw this.SyntaxError("Identifier expected.");
		}

		private UserDefinedItem ParseUserDefinedOptions(string EndKeyWord, Asn1Tags? DefaultTags)
		{
			UserDefinedItem Item = this.ParseUserDefinedOption(EndKeyWord, DefaultTags);
			List<UserDefinedItem> Options = null;

			while (this.PeekNextToken() == "|")
			{
				this.pos++;

				if (Options is null)
					Options = new List<UserDefinedItem>();

				Options.Add(Item);
				Item = this.ParseUserDefinedOption(EndKeyWord, DefaultTags);
			}

			if (Options is null)
				return Item;
			else
			{
				Options.Add(Item);
				return new UserDefinedOptions(Options.ToArray());
			}
		}

		private UserDefinedItem ParseUserDefinedOption(string EndKeyWord, Asn1Tags? DefaultTags)
		{
			UserDefinedItem Item = null;
			List<UserDefinedItem> Items = new List<UserDefinedItem>();
			string s;
			int PosBak = this.pos;

			while ((s = this.PeekNextToken()) != EndKeyWord && s != "::=" && s != "|")
			{
				if (!(Item is null))
				{
					if (Items is null)
						Items = new List<UserDefinedItem>();

					Items.Add(Item);
				}

				PosBak = this.pos;
				Item = this.ParseUserDefinedItem(DefaultTags);
			}

			if (Item is null)
				throw this.SyntaxError("Items expected.");

			if (s == "::=")
			{
				if (Items is null)
					throw this.SyntaxError("Items expected.");

				this.pos = PosBak;
				if (Items.Count == 1)
					return Items[0];
			}
			else if (Items is null)
				return Item;
			else
				Items.Add(Item);

			return new UserDefinedOption(Items.ToArray());
		}

		private UserDefinedItem ParseUserDefinedItem(Asn1Tags? DefaultTags)
		{
			string s = this.PeekNextToken();

			if (s == "\"")
			{
				if (!(this.ParseValue() is Asn1StringValue Label))
					throw this.SyntaxError("String label expected.");

				return new UserDefinedLiteral(Label.Value);
			}

			if (!IsIdentifier(s))
				throw this.SyntaxError("Identifier or literal expected.");

			this.pos += s.Length;

			if (this.PeekNextToken() == "(")
			{
				this.pos++;

				string Name = this.ParseIdentifier();
				if (this.PeekNextToken() == ")")
				{
					this.pos++;
					return new UserDefinedSpecifiedPart(s, string.Empty, new Asn1TypeReference(Name, this));
				}
				else
				{
					Asn1Type Type = this.ParseType(Name, false, DefaultTags);

					this.AssertNextToken(")");

					return new UserDefinedSpecifiedPart(s, Name, Type);
				}
			}
			else
				return new UserDefinedPart(s);
		}

		private Asn1Restriction ParseRestriction()
		{
			this.AssertNextToken("(");
			Asn1Restriction Result = this.ParseOrs();
			this.AssertNextToken(")");

			return Result;
		}

		private Asn1Restriction ParseOrs()
		{
			Asn1Restriction Result = this.ParseAnds();

			string s = this.PeekNextToken();

			while (s == "|")
			{
				this.pos++;
				Result = new Asn1Or(Result, this.ParseAnds(), this);
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
				Result = new Asn1And(Result, this.ParseRestrictionRule(), this);
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
					this.AssertNextToken(")");

					return Result;

				case "SIZE":
					this.pos += 4;
					return new Asn1Size(this.ParseSet(), this);

				case "PATTERN":
					this.pos += 7;
					return new Asn1Pattern(this.ParseValue(), this);

				case "FROM":
					this.pos += 4;
					return new Asn1From(this.ParseSet(), this);

				case "CONTAINING":
					this.pos += 10;
					return new Asn1Containing(this.ParseValue(), this);

				case "ENCODED":
					this.pos += 7;
					this.AssertNextToken("BY");

					return new Asn1EncodedBy(this.ParseValue(), this);

				case "WITH":
					this.pos += 4;
					this.AssertNextToken("COMPONENTS");

					return new Asn1WithComponents(this.ParseValue(), this);

				default:
					return new Asn1InSet(this.ParseUnions(), this);
			}
		}

		private Asn1SetOfValues ParseSet()
		{
			this.AssertNextToken("(");
			Asn1SetOfValues Result = this.ParseUnions();
			this.AssertNextToken(")");

			return Result;
		}

		private Asn1SetOfValues ParseUnions()
		{
			Asn1SetOfValues Result = this.ParseIntersections();

			string s = this.PeekNextToken();

			while (s == "|" || s == "UNION")
			{
				this.pos += s.Length;
				Result = new Asn1Union(Result, this.ParseIntersections(), this);
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1SetOfValues ParseIntersections()
		{
			Asn1SetOfValues Result = this.ParseIntervals();

			string s = this.PeekNextToken();

			while (s == "^" || s == "INTERSECTION")
			{
				this.pos += s.Length;
				Result = new Asn1Union(Result, this.ParseIntervals(), this);
				s = this.PeekNextToken();
			}

			return Result;
		}

		private Asn1SetOfValues ParseIntervals()
		{
			string s = this.PeekNextToken();

			if (s == "(")
			{
				this.pos++;

				Asn1SetOfValues Result = this.ParseUnions();
				this.AssertNextToken(")");

				return Result;
			}
			else if (s == "ALL")
			{
				this.pos += 3;
				return new Asn1All(this);
			}
			else
			{
				Asn1Value Value = this.ParseValue();

				if (this.PeekNextToken() == "..")
				{
					this.pos += 2;
					Asn1Value Value2 = this.ParseValue();

					return new Asn1Interval(Value, Value2, this);
				}
				else
					return new Asn1Element(Value, this);
			}
		}

		private string ParseIdentifier()
		{
			string s = this.NextToken();

			if (!IsIdentifier(s))
				throw this.SyntaxError("Identifier expected.");

			return s;
		}

		private string ParseTypeNameIdentifier()
		{
			string s = this.NextToken();

			if (!IsTypeIdentifier(s))
				throw this.SyntaxError("Type name identifier expected.");

			return s;
		}

		private static bool IsIdentifier(string s)
		{
			return !string.IsNullOrEmpty(s) && char.IsLetter(s[0]);
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
			Asn1Node[] Values = this.ParseValues(false);
			return new Asn1Oid(Values, this);
		}

		internal Asn1Type ParseType(string Name, bool TypeDef, Asn1Tags? DefaultTags)
		{
			Asn1Tags? Tags = DefaultTags;

			switch (this.PeekNextToken())
			{
				case "IMPLICIT":
					this.pos += 8;
					Tags = Asn1Tags.Implicit;
					break;

				case "EXPLICIT":
					this.pos += 8;
					Tags = Asn1Tags.Explicit;
					break;

				case "AUTOMATIC":
					this.pos += 9;
					Tags = Asn1Tags.Automatic;
					break;
			}

			string s2;
			Asn1Type Result = this.ParseDataType(Name, TypeDef, DefaultTags);
			Result.Tags = Tags;

			while (true)
			{
				s2 = this.PeekNextToken();

				switch (s2)
				{
					case "(":
						Result.Restriction = this.ParseRestriction();
						break;

					case "{":
						this.pos++;

						List<Asn1NamedValue> NamedOptions = new List<Asn1NamedValue>();

						while (true)
						{
							s2 = this.NextToken();
							if (!IsFieldIdentifier(s2))
								throw this.SyntaxError("Value name expected.");

							if (this.PeekNextToken() == "(")
							{
								this.pos++;

								Asn1Value Value = this.ParseValue();

								if (!this.namedNodes.ContainsKey(s2))
									this.namedNodes[s2] = Value;

								NamedOptions.Add(new Asn1NamedValue(s2, Value, Result, this));

								if (this.NextToken() != ")")
									throw this.SyntaxError(") expected");
							}
							else
								NamedOptions.Add(new Asn1NamedValue(s2, null, Result, this));

							s2 = this.NextToken();

							if (s2 == ",")
								continue;
							else if (s2 == "}")
							{
								Result.NamedOptions = NamedOptions.ToArray();
								break;
							}
							else
								throw this.SyntaxError("Unexpected token.");
						}
						break;

					case "OPTIONAL":
						this.pos += 8;
						Result.SetOptional(true);
						break;

					case "PRESENT":
						this.pos += 7;
						Result.Present = true;
						break;

					case "ABSENT":
						this.pos += 6;
						Result.Absent = true;
						break;

					case "DEFAULT":
						this.pos += 7;
						Result.SetOptional(true);
						Result.Default = this.ParseValue();
						break;

					case "UNIQUE":
						this.pos += 6;
						Result.Unique = true;
						break;

					default:
						return Result;
				}
			}
		}

		private Asn1Type ParseDataType(string Name, bool TypeDef, Asn1Tags? DefaultTags)
		{
			string s = this.NextToken();
			if (string.IsNullOrEmpty(s))
				throw this.SyntaxError("Unexpected end of file.");

			switch (s)
			{
				case "ANY":
					s = this.PeekNextToken();
					if (s != "DEFINED")
						return new Asn1Any(this);

					this.pos += 7;
					s = this.NextToken();
					if (s != "BY")
						throw this.SyntaxError("BY expected.");

					s = this.ParseIdentifier();
					return new Asn1Any(s, this);

				case "BIT":
					this.AssertNextToken("STRING");

					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseValues(true);
						return new Asn1Enumeration(Name, TypeDef, true,
							new Asn1BitString(this), Nodes, this);
					}
					else
						return new Asn1BitString(this);

				case "BMPString":
					return new Asn1BmpString(this);

				case "BOOLEAN":
					return new Asn1Boolean(this);

				case "CHARACTER":
					this.AssertNextToken("STRING");
					return new Asn1UnrestrictedCharacterString(this);

				case "CHOICE":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList(DefaultTags);
						return new Asn1Choice(Name, TypeDef, Nodes, this);
					}
					else
						throw this.SyntaxError("{ expected.");

				case "DATE":
					return new Asn1Date(this);

				case "DATE-TIME":
					return new Asn1DateTime(this);

				case "DURATION":
					return new Asn1Duration(this);

				case "ENUMERATED":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseValues(true);
						return new Asn1Enumeration(Name, TypeDef, false, null, Nodes, this);
					}
					else
						throw this.SyntaxError("{ expected.");

				case "GeneralizedTime":
					return new Asn1GeneralizedTime(this);

				case "GeneralString":
					return new Asn1GeneralString(this);

				case "GraphicString":
					return new Asn1GraphicString(this);

				case "IA5String":
					return new Asn1Ia5String(this);

				case "INTEGER":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseValues(true);
						return new Asn1Enumeration(Name, TypeDef, false,
							new Asn1Integer(this), Nodes, this);
					}
					else
						return new Asn1Integer(this);

				case "ISO646String":
					return new Asn1Iso646String(this);

				case "NULL":
					return new Asn1Null(this);

				case "NumericString":
					return new Asn1NumericString(this);

				case "OBJECT":
					this.AssertNextToken("IDENTIFIER");
					return new Asn1ObjectIdentifier(false, this);

				case "OCTET":
					this.AssertNextToken("STRING");
					return new Asn1OctetString(this);

				case "PrintableString":
					return new Asn1PrintableString(this);

				case "REAL":
					return new Asn1Real(this);

				case "RELATIVE":
					this.AssertNextToken("OID");
					return new Asn1ObjectIdentifier(true, this);

				case "RELATIVE-OID":
					return new Asn1ObjectIdentifier(true, this);

				case "SET":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList(DefaultTags);
						return new Asn1Set(Name, TypeDef, Nodes, this);
					}
					else
					{
						Asn1SetOfValues Size = this.ParseSizeOf(s);
						Asn1Type Type = this.ParseDataType(Name, TypeDef, DefaultTags);
						return new Asn1SetOf(Size, Type, this);
					}

				case "SEQUENCE":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList(DefaultTags);
						return new Asn1Sequence(Name, TypeDef, Nodes, this);
					}
					else
					{
						Asn1SetOfValues Size = this.ParseSizeOf(s);
						Asn1Type Type = this.ParseDataType(Name, false, DefaultTags);
						return new Asn1SequenceOf(Name, TypeDef, Size, Type, this);
					}

				case "T61String":
					return new Asn1T61String(this);

				case "TeletexString":
					return new Asn1TeletexString(this);

				case "TIME-OF-DAY":
					return new Asn1TimeOfDay(this);

				case "UniversalString":
					return new Asn1UniversalString(this);

				case "UTCTime":
					return new Asn1UtcTime(this);

				case "UTF8String":
					return new Asn1Utf8String(this);

				case "VideotexString":
					return new Asn1VideotexString(this);

				case "VisibleString":
					return new Asn1VisibleString(this);

				case "ObjectDescriptor":
				case "EXTERNAL":
				case "EMBEDDED":
				case "CLASS":
				case "COMPONENTS":
				case "INSTANCE":
				case "OID-IRI":
					throw this.SyntaxError("Token not implemented.");

				default:
					if (char.IsUpper(s[0]))
						return new Asn1TypeReference(s, this);
					else
						throw this.SyntaxError("Type name expected.");
			}
		}

		private Asn1SetOfValues ParseSizeOf(string s)
		{
			Asn1SetOfValues Size = null;
			bool InParenthesis;
			bool ParsingProperties = true;

			if (s == "(")
			{
				this.pos++;
				InParenthesis = true;
			}
			else
				InParenthesis = false;

			while (ParsingProperties)
			{
				s = this.PeekNextToken();

				switch (s)
				{
					case "SIZE":
						if (!(Size is null))
							throw this.SyntaxError("SIZE already specified.");

						this.pos += 4;
						Size = this.ParseSet();
						break;

					case ")":
						if (InParenthesis)
						{
							this.pos++;
							ParsingProperties = false;
						}
						else
							throw this.SyntaxError("Unexpected ).");

						break;

					case "OF":
						if (InParenthesis)
							throw this.SyntaxError(") expected.");
						else
							ParsingProperties = false;
						break;

					default:
						throw this.SyntaxError("Unexpected token.");
				}
			}

			this.AssertNextToken("OF");

			return Size;
		}

		private Asn1Node[] ParseList(Asn1Tags? DefaultTags)
		{
			this.AssertNextToken("{");

			List<Asn1Node> Items = new List<Asn1Node>();

			while (true)
			{
				Items.Add(this.ParseStatement(DefaultTags));

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

		private Asn1Node[] ParseValues(bool RequireDelimiters)
		{
			this.AssertNextToken("{");

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

					case "(":
						this.pos++;
						Asn1Value Value = this.ParseValue();
						this.AssertNextToken(")");

						int LastIndex = Items.Count - 1;

						if (Items[LastIndex] is Asn1ValueReference Ref)
							Items[LastIndex] = new Asn1NamedValue(Ref.Identifier, Value, null, this);
						else
							throw this.SyntaxError("Invalid value reference.");
						break;

					default:
						if (RequireDelimiters)
							throw this.SyntaxError(", or } expected.");
						break;
				}
			}
		}

		internal Asn1Value ParseValue()
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
					return new Asn1BooleanValue(false, this);

				case "MAX":
					this.pos += 3;
					return new Asn1Max(this);

				case "MIN":
					this.pos += 3;
					return new Asn1Min(this);

				case "TRUE":
					this.pos += 4;
					return new Asn1BooleanValue(true, this);

				case "INF":
					this.pos += 3;
					return new Asn1FloatingPointValue(double.PositiveInfinity, this);

				case "NaN":
					this.pos += 3;
					return new Asn1FloatingPointValue(double.NaN, this);

				case "...":
					this.pos += 3;
					return new Asn1Extension(this);

				case "\"":
					int Start = ++this.pos;
					char ch;

					while (this.pos < this.len && this.text[this.pos] != '"')
						this.pos++;

					if (this.pos >= this.len)
						throw this.SyntaxError("\" expected.");

					s = this.text.Substring(Start, this.pos - Start);
					this.pos++;

					return new Asn1StringValue(s, this);

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
								return new Asn1IntegerValue(l, this);
							else
								throw this.SyntaxError("Invalid hexadecimal string.");

						case "D":
						case "d":
							this.pos++;
							if (long.TryParse(s, out l))
								return new Asn1IntegerValue(l, this);
							else
								throw this.SyntaxError("Invalid decimal string.");

						case "B":
						case "b":
							this.pos++;
							if (TryParseBinary(s, out l))
								return new Asn1IntegerValue(l, this);
							else
								throw this.SyntaxError("Invalid binary string.");

						case "O":
						case "o":
							this.pos++;
							if (TryParseOctal(s, out l))
								return new Asn1IntegerValue(l, this);
							else
								throw this.SyntaxError("Invalid octal string.");

						default:
							throw this.SyntaxError("Unexpected token.");
					}

				case "{":
					this.pos++;

					List<Asn1Value> Items = new List<Asn1Value>();
					bool Oid = false;

					while (true)
					{
						Asn1Value Value = this.ParseValue();

						s = this.PeekNextToken();

						switch (s)
						{
							case ",":
								this.pos++;
								Items.Add(Value);
								break;

							case "}":
								this.pos++;
								Items.Add(Value);

								if (Oid)
									return new Asn1Oid(Items.ToArray(), this);
								else
									return new Asn1Array(Items.ToArray(), this);

							default:
								if (Items.Count == 0)
									Oid = true;

								if (Oid)
									Items.Add(Value);
								else
									throw this.SyntaxError("Unexpected token.");
								break;
						}
					}

				default:
					if (char.IsLetter(s[0]))
					{
						this.pos += s.Length;

						switch (this.PeekNextToken())
						{
							case ":":
								if (!AllowNamed)
									throw this.SyntaxError("Value expected.");

								this.pos++;
								Asn1Value Value = this.ParseValue(false);
								return new Asn1NamedValue(s, Value, null, this);

							case "(":
								Asn1Restriction Restriction = this.ParseRestriction();

								if (Restriction is Asn1InSet Set && Set.Set is Asn1Element Element)
									return new Asn1NamedValue(s, Element.Element, null, this);
								else
									return new Asn1RestrictedValueReference(s, Restriction, this);

							default:
								if (!char.IsUpper(s[0]))
									return new Asn1ValueReference(s, this);
								else
									throw this.SyntaxError("Type references not permitted here.");
						}
					}
					else
					{
						Start = this.pos;

						bool Sign = s.StartsWith("-");
						if (Sign)
						{
							s = s.Substring(1);
							this.pos++;
						}

						if (ulong.TryParse(s, out ulong l))
						{
							this.pos += s.Length;

							ch = this.PeekNextChar();

							if ((ch == '.' && this.pos < this.lenm1 && this.text[this.pos + 1] != '.') ||
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

								return new Asn1FloatingPointValue(d, this);
							}

							if (l <= long.MaxValue)
								return new Asn1IntegerValue(Sign ? -(long)l : (long)l, this);
							else if (Sign)
								throw this.SyntaxError("Number does not fit into a 64-bit signed integer.");
							else
								return new Asn1UnsignedIntegerValue(l, this);
						}
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
		/// Declares named types.
		/// </summary>
		/// <param name="Settings">C# export settings.</param>
		public void DeclareTypes(CSharpExportSettings Settings)
		{
			foreach (Asn1Node Node in this.namedNodes.Values)
			{
				if (Node is INamedNode NamedNode)
					Settings.DeclareType(Settings, Node, NamedNode.Name, NamedNode.MustBeQualified);
			}
		}

		/// <summary>
		/// Gets the fully qualified name of an identifier.
		/// </summary>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Identifier">Identifier</param>
		/// <returns>Fully qualified name of identifier, as segments.</returns>
		public string[] GetFullyQualifiedName(CSharpExportSettings Settings,
			string Identifier)
		{
			if (this.namedNodes.TryGetValue(Identifier, out Asn1Node Node) &&
				Node is INamedNode NamedNode)
			{
				ChunkedList<string> Fqn = new ChunkedList<string>();
				NamedNode.GetFullyQualifiedName(Fqn);
				Fqn.Add(Settings.BaseNamespace);

				Fqn.Reverse();

				return Fqn.ToArray();
			}
			else
				return new string[] { Identifier };
		}

		/// <summary>
		/// Exports ASN.1 schemas to C#
		/// </summary>
		/// <param name="Settings">C# export settings.</param>
		/// <returns>C# code</returns>
		public async Task<string> ExportCSharp(CSharpExportSettings Settings)
		{
			this.DeclareTypes(Settings);

			StringBuilder Output = new StringBuilder();
			await this.ExportCSharp(Output, Settings);
			return Output.ToString();
		}

		/// <summary>
		/// Exports ASN.1 schemas to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		public async Task ExportCSharp(StringBuilder Output, CSharpExportSettings Settings)
		{
			CSharpExportState State = new CSharpExportState(Settings);
			Output.AppendLine("using Asn1 = Waher.Content.Asn1;");
			Output.AppendLine("using Asn1F = System.Formats.Asn1;");

			if (!(this.root is null))
				await this.root.ExportCSharp(Output, State, 0, CSharpExportPass.Explicit);
		}
	}
}
