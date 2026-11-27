using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Waher.Content.Asn1.Exceptions;
using Waher.Content.Asn1.Model;
using Waher.Content.Asn1.Model.Classes;
using Waher.Content.Asn1.Model.Macro;
using Waher.Content.Asn1.Model.Restrictions;
using Waher.Content.Asn1.Model.Sets;
using Waher.Content.Asn1.Model.Types;
using Waher.Content.Asn1.Model.Values;
using Waher.Runtime.IO;
using Waher.Runtime.Collections;
using System.Formats.Asn1;
using System.Numerics;
using System;
using System.IO;
using Waher.Security;
using Waher.Content.Multipart;

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
		internal int row = 0;
		internal int rowPos = 0;
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
				{
					this.pos++;

					if (ch == '\n')
					{
						if (this.pos < this.len && this.text[this.pos] == '\r')
							this.pos++;

						this.row++;
						this.rowPos = this.pos;
					}
					else if (ch == '\r')
					{
						if (this.pos < this.len && this.text[this.pos] == '\n')
							this.pos++;

						this.row++;
						this.rowPos = this.pos;
					}
				}
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

					while (this.pos < this.len)
					{
						ch = this.text[this.pos];
						if (ch == '*' && this.pos < this.len - 1 && this.text[this.pos + 1] == '/')
						{
							this.pos += 2;
							break;
						}
						else if (ch == '\n')
						{
							if (this.pos < this.len && this.text[this.pos] == '\r')
								this.pos++;

							this.row++;
							this.rowPos = this.pos;
						}
						else if (ch == '\r')
						{
							if (this.pos < this.len && this.text[this.pos] == '\n')
								this.pos++;

							this.row++;
							this.rowPos = this.pos;
						}

						this.pos++;
					}
				}
				else
					break;
			}
		}

		internal void SkipWhiteSpace(out string Comment)
		{
			StringBuilder sb = null;
			char ch;

			while (this.pos < this.len)
			{
				ch = this.text[this.pos];

				if (ch <= ' ' || ch == (char)160)
				{
					this.pos++;

					if (ch == '\n')
					{
						if (this.pos < this.len && this.text[this.pos] == '\r')
							this.pos++;

						this.row++;
						this.rowPos = this.pos;
					}
					else if (ch == '\r')
					{
						if (this.pos < this.len && this.text[this.pos] == '\n')
							this.pos++;

						this.row++;
						this.rowPos = this.pos;
					}
				}
				else if (ch == '-' && this.pos < this.lenm1 && this.text[this.pos + 1] == '-')
				{
					this.pos += 2;

					if (sb is null)
						sb = new StringBuilder();

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

						sb.Append(ch);
						this.pos++;
					}
				}
				else if (ch == '/' && this.pos < this.lenm1 && this.text[this.pos + 1] == '*')
				{
					this.pos += 2;

					if (sb is null)
						sb = new StringBuilder();

					while (this.pos < this.len)
					{
						ch = this.text[this.pos];
						if (ch == '*' && this.pos < this.len - 1 && this.text[this.pos + 1] == '/')
						{
							this.pos += 2;
							break;
						}
						else if (ch == '\n')
						{
							if (this.pos < this.len && this.text[this.pos] == '\r')
								this.pos++;

							this.row++;
							this.rowPos = this.pos;
						}
						else if (ch == '\r')
						{
							if (this.pos < this.len && this.text[this.pos] == '\n')
								this.pos++;

							this.row++;
							this.rowPos = this.pos;
						}

						sb.Append(ch);
						this.pos++;
					}
				}
				else
					break;
			}

			Comment = sb?.ToString() ?? string.Empty;
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

			if (this.pos >= this.len)
				return string.Empty;

			char ch = this.text[this.pos++];

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

		internal string PeekNextToken(int PosDelta)
		{
			int Bak = this.pos;
			this.pos += PosDelta;
			string Result = this.PeekNextToken();
			this.pos = Bak;

			return Result;
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
			return new Asn1SyntaxException(Message, this.text, this.pos, this.row + 1,
				this.pos - this.rowPos + 1, Path.GetFileName(this.location));
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

					if (this.namedNodes.ContainsKey(Identifier))
						throw this.SyntaxError("Identifier already defined: " + Identifier);

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

			ChunkedList<Asn1Import> Imports = null;
			ChunkedList<string> Exports = null;

			do
			{
				s = this.PeekNextToken();

				if (s == "IMPORTS")
				{
					this.pos += 7;

					if (Imports is null)
						Imports = new ChunkedList<Asn1Import>();

					ChunkedList<Asn1IdentifierReference> Identifiers = new ChunkedList<Asn1IdentifierReference>();

					do
					{
						string Identifier = this.ParseIdentifier();
						string ModuleRef;
						Asn1ClassReference ClassReference = null;

						s = this.PeekNextToken();

						if (s == "{")
						{
							this.pos++;
							s = this.PeekNextToken();

							if (s != "}")
								ClassReference = this.ParseClassReference();

							this.AssertNextToken("}");
							s = this.PeekNextToken();
						}

						if (s == "FROM")
						{
							this.pos += 4;
							Identifiers.Add(new Asn1IdentifierReference(Identifier,
								ClassReference, this));

							ModuleRef = this.ParseTypeNameIdentifier();

							Asn1Import Import = new Asn1Import(Identifiers.ToArray(),
								ModuleRef, this);

							Imports.Add(Import);
							Identifiers.Clear();

							s = this.PeekNextToken();

							if (s == "{")
							{
								Import.Oid = this.ParseOid();
								if (this.namedNodes.TryGetValue(ModuleRef, out Asn1Node PrevModuleDef))
								{
									if (!Import.Oid.Equals(PrevModuleDef))
										throw this.SyntaxError(ModuleRef + " already defined.");
								}
								else
								{
									if (this.namedNodes.ContainsKey(ModuleRef))
										throw this.SyntaxError("Identifier already defined: " + ModuleRef);

									this.namedNodes[ModuleRef] = Import;
								}

								s = this.PeekNextToken();

								if (s == "WITH")
								{
									this.pos += 4;
									this.AssertNextToken("SUCCESSORS");
									Import.WithSuccessors = true;

									s = this.PeekNextToken();
								}
							}

							if (s != ";")
								continue;
						}
						else if (s == ".")
						{
							this.pos++;
							ModuleRef = Identifier;
							Identifier = this.ParseIdentifier();
							ClassReference = null;

							s = this.PeekNextToken();

							if (s == "{")
							{
								this.pos++;
								s = this.PeekNextToken();

								if (s != "}")
									ClassReference = this.ParseClassReference();

								this.AssertNextToken("}");
							}

							Imports.Add(new Asn1Import(new Asn1IdentifierReference[]
								{
									new Asn1IdentifierReference(Identifier, ClassReference, this)
								}, ModuleRef, this));

							s = this.PeekNextToken();
						}
						else
						{
							Identifiers.Add(new Asn1IdentifierReference(Identifier,
								ClassReference, this));
						}

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
						if (this.Document.namedNodes.ContainsKey(Import.Module))
							continue;   // Already imported

						Asn1Document Doc = await Import.LoadDocument();

						foreach (Asn1IdentifierReference Identifier in Import.Identifiers)
						{
							if (!Doc.namedNodes.TryGetValue(Identifier.Identifier,
								out Asn1Node ImportedNode))
							{
								throw this.SyntaxError(Identifier + " not found in " + Import.Module);
							}

							if (this.namedNodes.ContainsKey(Identifier.Identifier))
							{
								throw this.SyntaxError("Identifier already defined: " +
									Identifier.Identifier);
							}

							this.namedNodes[Identifier.Identifier] = ImportedNode;

							if (Doc.aliases.TryGetValue(Identifier.Identifier,
								out Asn1TypeDefinition TypeDef))
							{
								this.aliases[Identifier.Identifier] = TypeDef;
							}

							if (Doc.values.TryGetValue(Identifier.Identifier,
								out Asn1FieldValueDefinition ValueDef))
							{
								this.values[Identifier.Identifier] = ValueDef;
							}
						}
					}
				}
				else if (s == "EXPORTS")
				{
					this.pos += 7;

					if (Exports is null)
						Exports = new ChunkedList<string>();

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

			ChunkedList<Asn1Node> Items = new ChunkedList<Asn1Node>();

			while ((s = this.PeekNextToken()) != "END")
			{
				if (string.IsNullOrEmpty(s))
					throw this.SyntaxError("END expected.");

				Asn1Node Node = this.ParseStatement(DefaultTags, false, true);
				Items.Add(Node);

				Node.ForEach(Node2 =>
				{
					if (Node2 is INamedNode NamedNode)
					{
						if (this.namedNodes.TryGetValue(NamedNode.Name, out Asn1Node Prev))
						{
							if (NamedNode is Asn1TypeDefinition TypeDef)
							{
								if (this.namedNodes.ContainsKey(NamedNode.Name))
									throw this.SyntaxError("Identifier already defined: " + NamedNode.Name);

								this.namedNodes[NamedNode.Name] = TypeDef;
							}
							else if (!(Prev is Asn1TypeDefinition) && !Prev.Equals(NamedNode))
								throw this.SyntaxError("Duplicate type: " + NamedNode.Name);
						}
						else
						{
							if (this.namedNodes.ContainsKey(NamedNode.Name))
								throw this.SyntaxError("Identifier already defined: " + NamedNode.Name);

							this.namedNodes[NamedNode.Name] = Node2;
						}
					}

					return true;
				});
			}

			this.pos += 3;

			return new Asn1Module(Imports?.ToArray(), Exports?.ToArray(), Items.ToArray(), this);
		}

		private Asn1ClassReference[] ParseClassReferences()
		{
			ChunkedList<Asn1ClassReference> ClassReferences = new ChunkedList<Asn1ClassReference>();

			this.AssertNextToken("{");

			while (this.PeekNextToken() != "}")
			{
				ClassReferences.Add(this.ParseClassReference());

				if (this.PeekNextToken() == ",")
					this.pos++;
				else
					break;
			}

			this.AssertNextToken("}");

			return ClassReferences.ToArray();
		}

		private Asn1ClassReference ParseClassReference()
		{
			string s = this.ParseIdentifier();
			if (this.PeekNextToken() != ":")
				return new Asn1ClassReference(s);

			this.pos++;

			string s2 = this.ParseIdentifier();

			return new Asn1ClassReference(s, s2);
		}

		private Asn1Node ParseStatement(Asn1Tags? DefaultTags, bool ClassInstance,
			bool ModuleStatements)
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

				Asn1ClassReference[] ClassReferences = null;
				Asn1ClassDefinition ClassDefinition = null;

				if (ModuleStatements && s2 == "{")
				{
					ClassReferences = this.ParseClassReferences();
					s2 = this.PeekNextToken();
				}

				if (s2 == "::=")
				{
					if (!char.IsUpper(ch))   // !Type = XML notation
						throw this.SyntaxError("XML notation not supported.");

					this.pos += 3;

					s2 = this.PeekNextToken();

					if (s2 == "CLASS")
					{
						this.pos += 5;

						this.ParsePropertyDefinitions(DefaultTags,
							out Asn1TypeDeclaration[] Types,
							out Asn1PropertyDefinition[] Properties);

						Asn1ClassDefinition ClassDef = new Asn1ClassDefinition(s,
							Types, DefaultTags, Properties, this);

						if (this.namedNodes.ContainsKey(s))
							throw this.SyntaxError("Identifier already defined: " + s);

						this.namedNodes[s] = ClassDef;

						s2 = this.PeekNextToken();
						if (s2 == "WITH")
						{
							this.pos += 4;
							this.AssertNextToken("SYNTAX");

							ClassDef.Syntax = this.ParseSyntaxElements("{", "}", true,
								ClassDef);
						}

						return ClassDef;
					}
					else if (!(ClassReferences is null) &&
						ClassReferences.Length == 1 &&
						this.namedNodes.TryGetValue(ClassReferences[0].ClassName, out Asn1Node Node) &&
						Node is Asn1ClassDefinition ClassRef)
					{
						ClassDefinition = ClassRef;
					}
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
					ChunkedList<SupportingSyntax> SupportingSyntax = new ChunkedList<SupportingSyntax>();

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
				else if (this.namedNodes.TryGetValue(s2, out Asn1Node Node2) &&
					Node2 is Asn1ClassDefinition ClassDef &&
					this.PeekNextToken(s2.Length) == "::=")
				{
					this.pos += s2.Length;

					this.AssertNextToken("::=");

					if (char.IsUpper(s[0]))
					{
						Asn1ClassInstanceElement[] Elements = this.ParseClassInstanceSet(
							ClassDef, DefaultTags, out bool Extensible);

						return new Asn1ClassSet(s, ClassDef, Elements, Extensible, this);
					}
					else
						return this.ParseClassInstance(s, ClassDef, DefaultTags);
				}
				else if (char.IsUpper(ch))   // Type or macro
				{
					int Bak = this.pos;
					this.pos += s2.Length;

					if (this.PeekNextToken() == "::=")
					{
						this.pos += 3;

						this.AssertNextToken("{");
						this.AssertNextToken("...");
						this.AssertNextToken("}");

						return new Asn1ClassSet(s, s2,
							Array.Empty<Asn1ClassInstanceElement>(), true, this);
					}
					else
					{
						this.pos = Bak;

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
					Class = TagClass.ContextSpecific;   // Default class when providing index.

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
						Definition = this.ParseType(s, true, DefaultTags, false);

					Asn1TypeDefinition TypeDef = new Asn1TypeDefinition(s,
						Class.HasValue && TagValue.HasValue ?
						new Asn1Tag(Class.Value, TagValue.Value) : (Asn1Tag?)null,
						Definition, ClassReferences, this);

					if (!Definition.ConstructedType)
						this.aliases[s] = TypeDef;

					if (!(ClassDefinition is null) && ClassDefinition.Serialization is null)
						ClassDefinition.Serialization = TypeDef;

					return TypeDef;
				}
				else                    // name
				{
					if (!IsTypeIdentifier(s2))
						throw this.SyntaxError("Type name expected.");

					Asn1Type Type = this.ParseType(s, false, DefaultTags, false);

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
			else if (s == "&")
			{
				this.pos++;

				string Name = this.ParseIdentifier();

				if (char.IsUpper(Name[0]))
				{
					Asn1Type Type;

					s = this.PeekNextToken();
					if (s == "," || s == ")" || s == "}")
						Type = new Asn1Any(this);
					else
						Type = this.ParseType(s, false, DefaultTags, false);

					return new Asn1PropertyDefinition(Name, Type, this);
				}

				if (ClassInstance)
				{
					Asn1Value Value = this.ParseValue();
					return new Asn1PropertyValue(Name, Value, this);
				}
				else
				{
					Asn1Type Type = this.ParseType(Name, false, DefaultTags, true);
					return new Asn1PropertyDefinition(Name, Type, this);
				}
			}
			else
				throw this.SyntaxError("Identifier expected.");
		}

		private Asn1SyntaxNode[] ParseSyntaxElements(string StartToken, string EndToken,
			bool IncludeStartEndTokensAsElements, Asn1ClassDefinition ClassDef)
		{
			this.AssertNextToken(StartToken);

			ChunkedList<Asn1SyntaxNode> Syntax = new ChunkedList<Asn1SyntaxNode>();

			if (IncludeStartEndTokensAsElements)
				Syntax.Add(new Asn1ExpectedToken(StartToken, this));

			string s2 = this.PeekNextToken();
			while (s2 != EndToken)
			{
				if (s2 == "&")
				{
					this.pos++;
					Syntax.Add(new Asn1PropertyReference(this.ParseIdentifier(), this));
				}
				else if (s2 == "[")
				{
					Syntax.Add(new Asn1OptionalSyntaxGroup(
						this.ParseSyntaxElements("[", "]", false, ClassDef), this));
				}
				else
				{
					this.pos += s2.Length;
					Syntax.Add(new Asn1ExpectedToken(s2, this));

					if (s2 == "IDENTIFIED")
					{
						s2 = this.PeekNextToken();
						if (s2 == "BY" &&
							Syntax.Count >= 2 &&
							Syntax[Syntax.Count - 2] is
							Asn1PropertyReference Identified)
						{
							this.pos += s2.Length;
							Syntax.Add(new Asn1ExpectedToken(s2, this));

							s2 = this.PeekNextToken();
							if (s2 == "&")
							{
								this.pos++;
								Asn1PropertyReference Identifier =
									new Asn1PropertyReference(
										this.ParseIdentifier(), this);

								Syntax.Add(Identifier);

								ClassDef.AddIdentifier(Identified.Property,
									Identifier.Property);
							}
						}
					}
				}

				s2 = this.PeekNextToken();
			}

			if (IncludeStartEndTokensAsElements)
				Syntax.Add(new Asn1ExpectedToken("}", this));

			this.pos += EndToken.Length;

			return Syntax.ToArray();
		}

		private UserDefinedItem ParseUserDefinedOptions(string EndKeyWord, Asn1Tags? DefaultTags)
		{
			UserDefinedItem Item = this.ParseUserDefinedOption(EndKeyWord, DefaultTags);
			ChunkedList<UserDefinedItem> Options = null;

			while (this.PeekNextToken() == "|")
			{
				this.pos++;

				if (Options is null)
					Options = new ChunkedList<UserDefinedItem>();

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
			ChunkedList<UserDefinedItem> Items = new ChunkedList<UserDefinedItem>();
			string s;
			int PosBak = this.pos;

			while ((s = this.PeekNextToken()) != EndKeyWord && s != "::=" && s != "|")
			{
				if (!(Item is null))
				{
					if (Items is null)
						Items = new ChunkedList<UserDefinedItem>();

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
					Asn1Type Type = this.ParseType(Name, false, DefaultTags, false);

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

				case "CONSTRAINED":
					this.pos += 11;
					this.AssertNextToken("BY");
					this.AssertNextToken("{");

					this.SkipWhiteSpace(out string Comment);

					this.AssertNextToken("}");

					return new Asn1ConstrainedBy(Comment, this);

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

		internal string ParseIdentifier()
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

			return
				!string.IsNullOrEmpty(s) &&
				char.IsLetter(ch = s[0]) &&
				char.IsUpper(ch);
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

		internal Asn1Type ParseType(string Name, bool TypeDef, Asn1Tags? DefaultTags,
			bool PermitUndefinedType)
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
			Asn1Type Result = this.ParseDataType(Name, TypeDef, DefaultTags, PermitUndefinedType);
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

						if (this.PeekNextToken() == "{")
						{
							this.AssertNextToken("{");
							s2 = this.ParseIdentifier();
							this.AssertNextToken("}");
							this.AssertNextToken("}");

							Result.ClassSetReference = s2;
						}
						else
						{
							ChunkedList<Asn1NamedValue> NamedOptions = new ChunkedList<Asn1NamedValue>();

							while (true)
							{
								s2 = this.NextToken();
								if (!IsFieldIdentifier(s2))
									throw this.SyntaxError("Value name expected.");

								if (this.PeekNextToken() == "(")
								{
									this.pos++;

									Asn1Value Value = this.ParseValue();

									if (this.namedNodes.ContainsKey(s2))
										throw this.SyntaxError("Identifier already defined: " + s2);

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

		private Asn1Type ParseDataType(string Name, bool TypeDef, Asn1Tags? DefaultTags,
			bool PermitUndefinedType)
		{
			string s = this.NextToken();
			if (string.IsNullOrEmpty(s))
			{
				if (PermitUndefinedType)
					return new Asn1Any(this);
				else
					throw this.SyntaxError("Unexpected end of file.");
			}

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
						Asn1Node[] Nodes = this.ParseList(DefaultTags, false);
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
						Asn1Node[] Nodes = this.ParseList(DefaultTags, false);
						return new Asn1Set(Name, TypeDef, Nodes, this);
					}
					else
					{
						Asn1SetOfValues Size = this.ParseSizeOf(s);
						Asn1Type Type = this.ParseDataType(Name, TypeDef, DefaultTags, false);
						return new Asn1SetOf(Size, Type, this);
					}

				case "SEQUENCE":
					s = this.PeekNextToken();

					if (s == "{")
					{
						Asn1Node[] Nodes = this.ParseList(DefaultTags, false);
						return new Asn1Sequence(Name, TypeDef, Nodes, this);
					}
					else
					{
						Asn1SetOfValues Size = this.ParseSizeOf(s);
						Asn1Type Type = this.ParseDataType(Name, false, DefaultTags, false);
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

				case "&":
					s = this.ParseIdentifier();
					return new Asn1TypeDeclaration(s, this);

				case "ObjectDescriptor":
				case "EXTERNAL":
				case "EMBEDDED":
				case "COMPONENTS":
				case "INSTANCE":
				case "OID-IRI":
					throw this.SyntaxError("Token not implemented: " + s);

				default:
					if (char.IsUpper(s[0]))
					{
						if (this.PeekNextToken() == ".")
						{
							this.pos++;
							this.AssertNextToken("&");

							string PropertyName = this.ParseIdentifier();
							string ClassSet = null;
							string LookupKey = null;

							if (this.PeekNextToken() == "(")
							{
								this.AssertNextToken("(");
								this.AssertNextToken("{");
								ClassSet = this.ParseIdentifier();
								this.AssertNextToken("}");

								if (this.PeekNextToken() == "{")
								{
									this.AssertNextToken("{");
									this.AssertNextToken("@");

									if (this.PeekNextToken() == ".")
										this.pos++;

									LookupKey = this.ParseIdentifier();
									this.AssertNextToken("}");
								}

								this.AssertNextToken(")");
							}

							return new Asn1PropertyTypeReference(s, ClassSet, PropertyName,
								LookupKey, this);
						}
						else
							return new Asn1TypeReference(s, this);
					}
					else if (PermitUndefinedType)
					{
						this.pos -= s.Length;
						return new Asn1Any(this);
					}
					else
						throw this.SyntaxError("Type name expected: " + s);
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

		private void ParsePropertyDefinitions(Asn1Tags? DefaultTags,
			out Asn1TypeDeclaration[] TypeDeclarations,
			out Asn1PropertyDefinition[] PropertyDeclarations)
		{
			Asn1Node[] Fields = this.ParseList(DefaultTags, false);
			ChunkedList<Asn1TypeDeclaration> Types = new ChunkedList<Asn1TypeDeclaration>();
			ChunkedList<Asn1PropertyDefinition> Properties = new ChunkedList<Asn1PropertyDefinition>();

			foreach (Asn1Node Field in Fields)
			{
				if (Field is Asn1TypeDeclaration TypeDef)
					Types.Add(TypeDef);
				else if (Field is Asn1PropertyDefinition PropertyDef)
					Properties.Add(PropertyDef);
				else
					throw this.SyntaxError("Expected field or type declaration.");
			}

			TypeDeclarations = Types.ToArray();
			PropertyDeclarations = Properties.ToArray();
		}

		private Dictionary<string, Asn1Node> ParsePropertyValues(Asn1Tags? DefaultTags)
		{
			Asn1Node[] Fields = this.ParseList(DefaultTags, true);
			int i, c = Fields.Length;
			Dictionary<string, Asn1Node> Properties = new Dictionary<string, Asn1Node>();

			for (i = 0; i < c; i++)
			{
				if (!(Fields[i] is Asn1PropertyValue PropertyValue))
					throw this.SyntaxError("Invalid class instance property value.");

				Properties[PropertyValue.Name] = PropertyValue;
			}

			return Properties;
		}

		private Asn1ClassInstance ParseClassInstance(string Name,
			Asn1ClassDefinition ClassDef, Asn1Tags? DefaultTags)
		{
			Dictionary<string, Asn1Node> Properties;

			if (ClassDef.HasSyntax)
			{
				Properties = new Dictionary<string, Asn1Node>();

				foreach (Asn1SyntaxNode SyntaxNode in ClassDef.Syntax)
				{
					if (!SyntaxNode.MatchesNext(Properties))
						throw SyntaxNode.GetException();
				}
			}
			else
				Properties = this.ParsePropertyValues(DefaultTags);

			foreach (Asn1TypeDeclaration TypeDeclaration in ClassDef.TypeDeclarations)
			{
				if (Properties.TryGetValue(TypeDeclaration.Name, out Asn1Node Value) &&
					Value is Asn1PropertyValue Asn1PropertyValue &&
					Asn1PropertyValue.Value is Asn1TypeValue TypeValue)
				{
					Asn1PropertyValue.Type = TypeValue.Type;   // TODO: Clone & set Parent
				}
			}

			foreach (Asn1PropertyDefinition PropertyDefinition in ClassDef.Properties)
			{
				if (Properties.TryGetValue(PropertyDefinition.Name, out Asn1Node Value) &&
					Value is Asn1PropertyValue Asn1PropertyValue)
				{
					Asn1PropertyValue.Type = PropertyDefinition.Type;   // TODO: Clone & set Parent
				}
			}

			foreach (Asn1Node Value in Properties.Values)
			{
				if (Value is Asn1PropertyValue PropertyValue)
				{
					if (PropertyValue.Type is null)
						throw this.SyntaxError("Property not defined: " + PropertyValue.Name);
				}
			}

			return new Asn1ClassInstance(Name, ClassDef, DefaultTags, Properties, this);
		}

		private Asn1ClassInstanceElement[] ParseClassInstanceSet(
			Asn1ClassDefinition ClassDef, Asn1Tags? DefaultTags, out bool Extensible)
		{
			this.AssertNextToken("{");

			ChunkedList<Asn1ClassInstanceElement> Items = new ChunkedList<Asn1ClassInstanceElement>();
			string s = this.PeekNextToken();

			Extensible = false;

			while (s != "}")
			{
				if (s == "{")
					Items.Add(this.ParseClassInstance(string.Empty, ClassDef, DefaultTags));
				else if (s == "...")
				{
					this.pos += 3;
					Extensible = true;
				}
				else if (!IsIdentifier(s))
					throw this.SyntaxError("Expected identifier or class instance.");
				else
				{
					Items.Add(new Asn1ClassInstanceReference(s, this));
					this.pos += s.Length;
				}

				s = this.PeekNextToken();
				if (s == "," || s == "|")
				{
					this.pos++;
					s = this.PeekNextToken();
					if (s == "}")
						throw this.SyntaxError("Expected identifier.");
				}
				else if (s != "}")
					throw this.SyntaxError(", or | or } expected.");
			}

			this.AssertNextToken("}");

			return Items.ToArray();
		}

		private Asn1Node[] ParseList(Asn1Tags? DefaultTags, bool ClassInstance)
		{
			this.AssertNextToken("{");

			if (this.PeekNextToken() == "}")
			{
				this.pos++;
				return Array.Empty<Asn1Node>();
			}

			ChunkedList<Asn1Node> Items = new ChunkedList<Asn1Node>();

			while (true)
			{
				Items.Add(this.ParseStatement(DefaultTags, ClassInstance, false));

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

			ChunkedList<Asn1Node> Items = new ChunkedList<Asn1Node>();

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
							try
							{
								byte[] Bin = Hashes.StringToBinary(s);
								return new Asn1OctetStringValue(Bin, this);
							}
							catch (Exception)
							{
								throw this.SyntaxError("Invalid hexadecimal string.");
							}

						case "D":
						case "d":
							BigInteger l;

							this.pos++;
							if (string.IsNullOrEmpty(s))
								return new Asn1OctetStringValue(Array.Empty<byte>(), this);
							else if (BigInteger.TryParse(s, out l))
								return new Asn1OctetStringValue(l.ToByteArray(), this);
							else
								throw this.SyntaxError("Invalid decimal string.");

						case "B":
						case "b":
							this.pos++;
							if (string.IsNullOrEmpty(s))
								return new Asn1OctetStringValue(Array.Empty<byte>(), this);
							else if (TryParseBinary(s, out l))
								return new Asn1OctetStringValue(l.ToByteArray(), this);
							else
								throw this.SyntaxError("Invalid binary string.");

						case "O":
						case "o":
							this.pos++;
							if (string.IsNullOrEmpty(s))
								return new Asn1OctetStringValue(Array.Empty<byte>(), this);
							else if (TryParseOctal(s, out l))
								return new Asn1OctetStringValue(l.ToByteArray(), this);
							else
								throw this.SyntaxError("Invalid octal string.");

						default:
							throw this.SyntaxError("Unexpected token.");
					}

				case "{":
					this.pos++;

					ChunkedList<Asn1Value> Items = new ChunkedList<Asn1Value>();
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
						int Bak = this.pos;

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
								{
									this.pos = Bak;

									Asn1Type Type = this.ParseDataType(string.Empty, true,
										null, false);

									return new Asn1TypeValue(Type, this);
								}
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

						if (BigInteger.TryParse(s, out BigInteger l))
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

							return new Asn1IntegerValue(Sign ? -l : l, this);
						}
					}
					break;
			}

			throw this.SyntaxError("Value expected.");
		}

		private static bool TryParseBinary(string s, out BigInteger l)
		{
			l = 0;

			foreach (char ch in s)
			{
				if (ch < '0' || ch > '1')
					return false;

				l <<= 1;
				l += (byte)(ch - '0');
			}

			return true;
		}

		private static bool TryParseOctal(string s, out BigInteger l)
		{
			l = 0;

			foreach (char ch in s)
			{
				if (ch < '0' || ch > '7')
					return false;

				l <<= 3;
				l += (byte)(ch - '0');
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
