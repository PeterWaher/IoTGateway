using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Functions;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;
using Waher.Script.Content.Functions.Encoding;
using Waher.Script.Cryptography.Functions.Encoding;
using Waher.Script.Cryptography.Functions.HashFunctions;
using Waher.Script.Functions.DateAndTime;
using Waher.Script.Functions.Scalar;
using Waher.Script.Functions.Strings;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Operators.Conditional;
using Waher.Script.Operators.Logical;
using Waher.Script.Operators.Membership;
using Waher.Script.Operators.Vectors;
using Waher.Script.Persistence.SPARQL.Filters;
using Waher.Script.Persistence.SPARQL.Functions;
using Waher.Script.Persistence.SPARQL.Functions.Order;
using Waher.Script.Persistence.SPARQL.Patterns;
using Waher.Script.Statistics.Functions.RandomNumbers;

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

		private static readonly Dictionary<string, IExtensionFunction> functionsPerUri = new Dictionary<string, IExtensionFunction>(StringComparer.CurrentCultureIgnoreCase);

		static SparqlParser()
		{
			Types.OnInvalidated += (Sender, e) =>
			{
				lock (functionsPerUri)
				{
					functionsPerUri.Clear();
				}
			};
		}

		private readonly string preamble;
		private readonly int preambleLen;
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>()
		{
			{ "xsd", XmlSchema.Namespace },
			{ "rdf", Rdf.Namespace },
			{ "rdfs", RdfSchema.Namespace },
			{ "fn", "http://www.w3.org/2005/xpath-functions#" },
			{ "sfn", "http://www.w3.org/ns/sparql#" }
		};
		private QueryType queryType;
		private SparqlRegularPattern currentRegularPattern = null;
		private ISparqlPattern currentPattern = null;
		private int preamblePos;
		private System.Uri baseUri = null;
		private int blankNodeIndex = 0;

		/// <summary>
		/// Parses a SPARQL statement
		/// </summary>
		/// <param name="Preamble">Preamble that needs to be re-parsed.</param>
		public SparqlParser(string Preamble)
		{
			this.queryType = QueryType.Select;
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
		public string[] InternalKeywords => new string[]
		{
			"DISTINCT",
			"REDUCED",
			"FROM",
			"AS",
			"NAMED",
			"WHERE",
			"OPTIONAL",
			"UNION",
			"MINUS",
			"ORDER",
			"BY",
			"ASK",
			"CONSTRUCT",
			"ASC",
			"DESC",
			"VALUES",
			"GRAPH",
			"SERVICE",
			"UNDEF",
			"LIMIT",
			"OFFSET"
		};

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			return this.TryParse(Parser, Parser.Start, out Result);
		}

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Start">Start position of subquery.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, int Start, out ScriptNode Result)
		{
			Result = null;

			ScriptNode Node;
			string s;
			bool Distinct = false;
			bool Reduced = false;
			char ch;

			s = this.PeekNextToken(Parser).ToUpper();
			if (string.IsNullOrEmpty(s))
				return false;

			while (s != "SELECT" && s != "ASK" && s != "CONSTRUCT")
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

			List<ScriptNode> Columns = null;
			List<ScriptNode> ColumnNames = null;
			List<ScriptNode> GroupBy = null;
			List<ScriptNode> GroupByNames = null;
			List<KeyValuePair<ScriptNode, bool>> OrderBy = null;
			SparqlRegularPattern Construct = null;
			ISparqlPattern Where;
			ScriptNode Having;
			List<ScriptNode> From;
			Dictionary<UriNode, ISemanticCube> NamedGraphs = null;

			switch (s)
			{
				case "ASK":
					this.queryType = QueryType.Ask;
					this.NextToken(Parser);
					s = this.PeekNextToken(Parser).ToUpper();
					if (string.IsNullOrEmpty(s))
						return false;
					break;

				case "CONSTRUCT":
					this.queryType = QueryType.Construct;
					this.NextToken(Parser);

					s = Parser.PeekNextToken().ToUpper();
					if (s == "WHERE")
						Construct = null;
					else
					{
						ISparqlPattern Pattern = this.ParsePattern(Parser)
							?? throw Parser.SyntaxError("Expected pattern.");

						if (!(Pattern is SparqlRegularPattern RegularPattern))
							throw Parser.SyntaxError("Expected regular pattern.");

						Construct = RegularPattern;
						if (!(Construct.BoundVariables is null))
							throw Parser.SyntaxError("Bound variables not permitted in construct statement.");

						if (!(Construct.Filter is null))
							throw Parser.SyntaxError("Filters not permitted in construct statement.");

						s = Parser.PeekNextToken().ToUpper();
					}
					break;

				case "SELECT":
					this.queryType = QueryType.Select;
					this.NextToken(Parser);
					s = this.PeekNextToken(Parser).ToUpper();
					if (string.IsNullOrEmpty(s))
						return false;

					switch (s)
					{
						case "DISTINCT":
							this.NextToken(Parser);
							Distinct = true;

							s = this.PeekNextToken(Parser).ToUpper();
							break;

						case "REDUCED":
							this.NextToken(Parser);
							Reduced = true;

							s = this.PeekNextToken(Parser).ToUpper();
							break;
					}

					if (s == "*")
					{
						this.NextToken(Parser);
						s = this.PeekNextToken(Parser).ToUpper();

						Columns = null;
						ColumnNames = null;
					}
					else
					{
						Columns = new List<ScriptNode>();
						ColumnNames = new List<ScriptNode>();

						while (!string.IsNullOrEmpty(s) && s != "WHERE" && s != "FROM" && s != "{")
						{
							Node = this.ParseNamedExpression(Parser);
							if (Node is NamedNode NamedNode)
							{
								Columns.Add(NamedNode.LeftOperand);
								ColumnNames.Add(NamedNode.RightOperand);
							}
							else
							{
								Columns.Add(Node);
								ColumnNames.Add(null);
							}

							s = Parser.PeekNextToken().ToUpper();
						}
					}
					break;

				default:
					throw Parser.SyntaxError("Expected SELECT or ASK.");
			}

			From = null;

			while (s == "FROM")
			{
				if (From is null)
					From = new List<ScriptNode>();

				Parser.NextToken();
				Parser.SkipWhiteSpace();

				switch (Parser.PeekNextChar())
				{
					case '<':
						int Start2 = Parser.Position;
						Parser.NextChar();
						UriNode FromUri = this.ParseUri(Parser);

						From.Add(new ConstantElement(FromUri, Start2, Parser.Position - Start2, Parser.Expression));
						break;

					case 'n':
					case 'N':
						if (Parser.PeekNextToken().ToUpper() == "NAMED")
						{
							Parser.NextToken();
							if (Parser.NextNonWhitespaceChar() != '<')
								throw Parser.SyntaxError("Expected <");

							FromUri = this.ParseUri(Parser);

							if (NamedGraphs is null)
								NamedGraphs = new Dictionary<UriNode, ISemanticCube>();

							NamedGraphs[FromUri] = null;
						}
						else
							From.Add(Parser.ParseObject());
						break;

					default:
						From.Add(Parser.ParseObject());
						break;
				}

				s = Parser.PeekNextToken().ToUpper();
			}

			if (s == "WHERE")
			{
				Parser.NextToken();
				Where = this.ParsePattern(Parser);
				s = Parser.PeekNextToken().ToUpper();
			}
			else if (s == "{")
			{
				Where = this.ParsePattern(Parser);
				s = Parser.PeekNextToken().ToUpper();
			}
			else
				Where = null;

			if (this.queryType == QueryType.Construct && Construct is null)
			{
				if (Where is SparqlRegularPattern RegularPattern)
					Construct = RegularPattern;
				else
					throw Parser.SyntaxError("CONSTRUCT WHERE queries require a regular WHERE pattern.");
			}

			if (s == "VALUES")
			{
				Parser.NextToken();

				ValuesPattern Values = this.ParseValues(Parser);

				if (Where is null)
					Where = Values;
				else
					Where = new IntersectionPattern(Values, Where);

				s = Parser.PeekNextToken().ToUpper();
			}

			if (s == "GROUP")
			{
				Parser.NextToken();

				if (Parser.NextToken().ToUpper() != "BY")
					throw Parser.SyntaxError("Expected BY");

				GroupBy = new List<ScriptNode>();
				GroupByNames = new List<ScriptNode>();

				while (!string.IsNullOrEmpty(s) && s != "HAVING" && s != "ORDER" && s != ";" && s != ")" && s != "}")
				{
					Node = this.ParseNamedExpression(Parser);
					if (Node is NamedNode NamedNode)
					{
						GroupBy.Add(NamedNode.LeftOperand);
						GroupByNames.Add(NamedNode.RightOperand);
					}
					else
					{
						GroupBy.Add(Node);
						GroupByNames.Add(null);
					}

					s = Parser.PeekNextToken().ToUpper();
				}

				if (s == "HAVING")
				{
					Parser.NextToken();
					Having = this.ParseExpression(Parser, false);
					s = Parser.PeekNextToken().ToUpper();
				}
				else
					Having = null;
			}
			else
				Having = null;

			if (s == "ORDER")
			{
				if (this.queryType == QueryType.Ask)
					throw Parser.SyntaxError("ORDER BY not expected in ASK queries.");

				Parser.NextToken();
				s = Parser.NextToken().ToUpper();
				if (s != "BY")
					throw Parser.SyntaxError("Expected BY");

				OrderBy = new List<KeyValuePair<ScriptNode, bool>>();

				while (true)
				{
					Node = this.ParseExpression(Parser, true);
					if (Node is null)
						break;
					else if (Node is Asc Asc)
						OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Asc.Argument, true));
					else if (Node is Desc Desc)
						OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Desc.Argument, false));
					else
						OrderBy.Add(new KeyValuePair<ScriptNode, bool>(Node, true));
				}

				s = Parser.PeekNextToken().ToUpper();
			}

			int? Offset = null;
			int? Limit = null;

			while (true)
			{
				switch (s)
				{
					case "LIMIT":
						Parser.NextToken();
						Limit = this.ParsePositiveInteger(Parser);
						break;

					case "OFFSET":
						Parser.NextToken();
						Offset = this.ParsePositiveInteger(Parser);
						break;

					default:
						Result = new SparqlQuery(this.queryType, Distinct, Reduced, Columns?.ToArray(),
							ColumnNames?.ToArray(), From?.ToArray(), NamedGraphs, Where,
							GroupBy?.ToArray(), GroupByNames?.ToArray(), Having, OrderBy?.ToArray(),
							Limit, Offset, Construct, Start, Parser.Position - Start, Parser.Expression);

						return true;
				}

				s = Parser.PeekNextToken().ToUpper();
			}
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

		private ISparqlPattern ParsePattern(ScriptParser Parser)
		{
			if (Parser.NextNonWhitespaceChar() != '{')
				throw Parser.SyntaxError("Expected {");

			Parser.SkipWhiteSpace();
			if (Parser.PeekNextChar() == '}')
			{
				Parser.NextChar();
				return null;
			}

			SparqlRegularPattern Bak = this.currentRegularPattern;
			ISparqlPattern Bak2 = this.currentPattern;

			this.currentRegularPattern = new SparqlRegularPattern();
			this.currentPattern = this.currentRegularPattern;

			this.ParseTriples(Parser);

			if (Parser.NextNonWhitespaceChar() != '}')
				throw Parser.SyntaxError("Expected }");

			ISparqlPattern Result = this.currentPattern;

			this.currentRegularPattern = Bak;
			this.currentPattern = Bak2;

			return Result;
		}

		private void ParseTriples(ScriptParser Parser)
		{
			this.ParseTriples(Parser, null);
		}

		private void ParseTriples(ScriptParser Parser, ISemanticElement Subject)
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

					switch (Parser.PeekNextChar())
					{
						case '.':
							if (TriplePosition == 0)
								break;

							throw Parser.SyntaxError("Unexpected .");

						case '{':
							ISparqlPattern Left = this.currentPattern;
							ISparqlPattern Pattern = this.ParsePattern(Parser);

							if (Left.IsEmpty)
							{
								this.currentPattern = Pattern;
								this.currentRegularPattern = Pattern as SparqlRegularPattern;
							}
							else
							{
								this.currentRegularPattern = null;
								this.currentPattern = new IntersectionPattern(Left, Pattern);
							}

							Parser.SkipWhiteSpace();
							switch (Parser.PeekNextChar())
							{
								case '.':
									Parser.NextChar();
									Subject = null;
									Predicate = null;
									TriplePosition = 0;
									break;

								case '}':
									return;
							}
							continue;

						case 'o':
						case 'O':
						case 'u':
						case 'U':
						case 'm':
						case 'M':
						case 'v':
						case 'V':
						case 's':
						case 'S':
						case 'g':
						case 'G':
							if (!this.ParsePatternOperator(Parser))
								break;

							Parser.SkipWhiteSpace();
							switch (Parser.PeekNextChar())
							{
								case '.':
									Parser.NextChar();
									Subject = null;
									Predicate = null;
									TriplePosition = 0;
									break;

								case '}':
									return;
							}
							continue;
					}
				}

				Object = this.ParseElement(Parser, TriplePosition);
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
					if (this.currentRegularPattern is null)
					{
						this.currentRegularPattern = new SparqlRegularPattern();
						this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
					}

					this.currentRegularPattern.AddTriple(new SemanticQueryTriple(Subject, Predicate, Object));

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

						case 'b':
						case 'B':
							if (char.ToUpper(Parser.NextChar()) != 'I' ||
								char.ToUpper(Parser.NextChar()) != 'N' ||
								char.ToUpper(Parser.NextChar()) != 'D')
							{
								throw Parser.SyntaxError("Expected BIND");
							}

							if (InBlankNode)
								throw Parser.SyntaxError("Expected ]");

							Subject = null;
							Predicate = null;
							TriplePosition = 0;

							if (Parser.NextNonWhitespaceChar() != '(')
								throw Parser.SyntaxError("Expected (");

							ScriptNode Node = this.ParseNamedExpression(Parser);
							if (!(Node is NamedNode NamedNode))
								throw Parser.SyntaxError("Expected name.");

							if (this.currentRegularPattern is null)
							{
								this.currentRegularPattern = new SparqlRegularPattern();
								this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
							}

							this.currentRegularPattern.AddVariableBinding(
								NamedNode.LeftOperand, NamedNode.RightOperand);

							if (Parser.NextNonWhitespaceChar() != ')')
								throw Parser.SyntaxError("Expected )");

							break;

						case 'f':
						case 'F':
							if (char.ToUpper(Parser.NextChar()) != 'I' ||
								char.ToUpper(Parser.NextChar()) != 'L' ||
								char.ToUpper(Parser.NextChar()) != 'T' ||
								char.ToUpper(Parser.NextChar()) != 'E' ||
								char.ToUpper(Parser.NextChar()) != 'R')
							{
								throw Parser.SyntaxError("Expected FILTER");
							}

							if (InBlankNode)
								throw Parser.SyntaxError("Expected ]");

							Subject = null;
							Predicate = null;
							TriplePosition = 0;

							Node = this.ParseUnary(Parser, false);

							if (this.currentRegularPattern is null)
							{
								this.currentRegularPattern = new SparqlRegularPattern();
								this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
							}

							this.currentRegularPattern.AddFilter(Node);
							break;

						case 'u':
						case 'U':
						case 'o':
						case 'O':
						case 'm':
						case 'M':
						case 'v':
						case 'V':
						case 's':
						case 'S':
						case 'g':
						case 'G':
							Parser.UndoChar();

							if (!this.ParsePatternOperator(Parser))
								throw Parser.SyntaxError("Unexpected token.");

							if (InBlankNode)
								throw Parser.SyntaxError("Expected ]");

							Subject = null;
							Predicate = null;
							TriplePosition = 0;
							break;

						default:
							throw Parser.SyntaxError("Unexpected token.");
					}
				}
			}
		}

		private bool ParsePatternOperator(ScriptParser Parser)
		{
			switch (Parser.PeekNextToken().ToUpper())
			{
				case "OPTIONAL":
					Parser.NextToken();

					ISparqlPattern Left = this.currentPattern;
					ISparqlPattern Pattern = this.ParsePattern(Parser);

					this.currentRegularPattern = null;
					this.currentPattern = new OptionalPattern(Left, Pattern);
					return true;

				case "UNION":
					Parser.NextToken();

					Left = this.currentPattern;
					Pattern = this.ParsePattern(Parser);

					this.currentRegularPattern = null;
					this.currentPattern = new UnionPattern(Left, Pattern);
					return true;

				case "MINUS":
					Parser.NextToken();

					Left = this.currentPattern;
					Pattern = this.ParsePattern(Parser);

					this.currentRegularPattern = null;
					this.currentPattern = new ComplementPattern(Left, Pattern);
					return true;

				case "VALUES":
					Parser.NextToken();

					ValuesPattern Values = this.ParseValues(Parser);

					this.currentRegularPattern = null;

					if (this.currentPattern.IsEmpty)
						this.currentPattern = Values;
					else
						this.currentPattern = new IntersectionPattern(Values, this.currentPattern);

					return true;

				case "SELECT":
					SparqlParser SubParser = new SparqlParser(string.Empty);

					foreach (KeyValuePair<string, string> P in this.namespaces)
						SubParser.namespaces[P.Key] = P.Value;

					if (!SubParser.TryParse(Parser, Parser.Position, out ScriptNode Node))
						throw Parser.SyntaxError("Unable to parse subquery.");

					if (!(Node is SparqlQuery SubQuery))
						throw Parser.SyntaxError("Expected subquery.");

					SubQueryPattern SubQueryPattern = new SubQueryPattern(SubQuery);

					this.currentRegularPattern = null;

					if (this.currentPattern.IsEmpty)
						this.currentPattern = SubQueryPattern;
					else
						this.currentPattern = new IntersectionPattern(this.currentPattern, SubQueryPattern);

					return true;

				case "GRAPH":
					Parser.NextToken();

					ScriptNode Graph = this.ParseExpression(Parser, false);
					Pattern = this.ParsePattern(Parser);

					GraphPattern GraphPattern = new GraphPattern(Graph, Pattern);

					this.currentRegularPattern = null;

					if (this.currentPattern.IsEmpty)
						this.currentPattern = GraphPattern;
					else
						this.currentPattern = new IntersectionPattern(this.currentPattern, GraphPattern);

					return true;


				default:
					return false;
			}
		}

		private ValuesPattern ParseValues(ScriptParser Parser)
		{
			Parser.SkipWhiteSpace();
			switch (Parser.PeekNextChar())
			{
				case '?':
					Parser.NextChar();

					string s = this.ParseName(Parser);

					if (Parser.NextNonWhitespaceChar() != '{')
						throw Parser.SyntaxError("Expected {");

					List<ISemanticElement> Values = new List<ISemanticElement>();

					while (true)
					{
						Parser.SkipWhiteSpace();
						switch (Parser.PeekNextChar())
						{
							case (char)0:
								throw Parser.SyntaxError("Expected }");

							case '}':
								Parser.NextChar();

								return new ValuesPattern(s, Values.ToArray());

							default:
								ISemanticElement Element = this.ParseElement(Parser, 2);
								if (Element is UndefinedLiteral)
									Values.Add(null);
								else
									Values.Add(Element);
								break;
						}
					}

				case '(':
					Parser.NextChar();

					List<string> Names = new List<string>();

					while (true)
					{
						switch (Parser.NextNonWhitespaceChar())
						{
							case '?':
								Names.Add(this.ParseName(Parser));
								continue;

							case ')':
								break;

							default:
								throw Parser.SyntaxError("Expected ? or )");
						}

						break;
					}

					int i, c = Names.Count;
					if (c == 0)
						throw Parser.SyntaxError("Expected variable name.");

					if (Parser.NextNonWhitespaceChar() != '{')
						throw Parser.SyntaxError("Expected {");

					List<ISemanticElement[]> Records = new List<ISemanticElement[]>();

					while (true)
					{
						switch (Parser.NextNonWhitespaceChar())
						{
							case '(':
								Values = new List<ISemanticElement>();

								for (i = 0; i < c; i++)
								{
									ISemanticElement Element = this.ParseElement(Parser, 2);

									if (Element is UndefinedLiteral)
										Values.Add(null);
									else
										Values.Add(Element);
								}

								if (Parser.NextNonWhitespaceChar() != ')')
									throw Parser.SyntaxError("Expected )");

								Records.Add(Values.ToArray());
								continue;

							case '}':
								return new ValuesPattern(Names.ToArray(), Records.ToArray());

							default:
								throw Parser.SyntaxError("Expected ( or }");
						}
					}

				default:
					throw Parser.SyntaxError("Expected ? or (");
			}
		}

		private ScriptNode ParseNamedExpression(ScriptParser Parser)
		{
			ScriptNode Node = this.ParseExpression(Parser, false);
			string s = Parser.PeekNextToken().ToUpper();

			if (s == "AS")
			{
				Parser.NextToken();
				ScriptNode Name = this.ParseExpression(Parser, false);
				Node = new NamedNode(Node, Name, Node.Start, Parser.Position - Node.Start, Parser.Expression);
			}

			return Node;
		}

		private ScriptNode ParseExpression(ScriptParser Parser, bool Optional)
		{
			return this.ParseOrs(Parser, Optional);
		}

		private ScriptNode ParseOrs(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseAnds(Parser, Optional);
			if (Left is null)
				return null;

			Parser.SkipWhiteSpace();
			while (Parser.PeekNextChars(2) == "||")
			{
				Parser.SkipChars(2);
				ScriptNode Right = this.ParseAnds(Parser, false);
				Left = new Operators.Logical.Or(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
				Parser.SkipWhiteSpace();
			}

			return Left;
		}

		private ScriptNode ParseAnds(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseComparisons(Parser, Optional);
			if (Left is null)
				return null;

			Parser.SkipWhiteSpace();
			while (Parser.PeekNextChars(2) == "&&")
			{
				Parser.SkipChars(2);
				ScriptNode Right = this.ParseComparisons(Parser, false);
				Left = new Operators.Logical.And(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
				Parser.SkipWhiteSpace();
			}

			return Left;
		}

		private ScriptNode ParseComparisons(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseTerms(Parser, Optional);
			if (Left is null)
				return null;

			while (true)
			{
				Parser.SkipWhiteSpace();

				switch (Parser.PeekNextChar())
				{
					case '=':
						Parser.NextChar();
						ScriptNode Right = this.ParseTerms(Parser, false);
						Left = new EqualTo(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						break;

					case '!':
						Parser.NextChar();
						if (Parser.PeekNextChar() == '=')
						{
							Parser.NextChar();
							Right = this.ParseTerms(Parser, false);
							Left = new NotEqualTo(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						else
						{
							Parser.UndoChar();
							return Left;
						}
						break;

					case '<':
						Parser.NextChar();

						if (Parser.PeekNextChar() == '=')
						{
							Parser.NextChar();
							Right = this.ParseTerms(Parser, false);
							Left = new LesserThanOrEqualTo(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						else
						{
							Right = this.ParseTerms(Parser, false);
							Left = new LesserThan(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						break;

					case '>':
						Parser.NextChar();

						if (Parser.PeekNextChar() == '=')
						{
							Parser.NextChar();
							Right = this.ParseTerms(Parser, false);
							Left = new GreaterThanOrEqualTo(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						else
						{
							Right = this.ParseTerms(Parser, false);
							Left = new GreaterThan(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						break;

					case 'i':
					case 'I':
						if (Parser.PeekNextToken().ToUpper() == "IN")
						{
							Parser.NextToken();
							Right = this.ParseTerms(Parser, false);
							Left = new In(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						else
							return Left;
						break;

					case 'n':
					case 'N':
						if (Parser.PeekNextToken().ToUpper() == "NOT")
						{
							if (Parser.NextToken().ToUpper() != "IN")
								throw Parser.SyntaxError("Expected IN");

							Right = this.ParseTerms(Parser, false);
							Left = new NotIn(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						}
						else
							return Left;
						break;

					default:
						return Left;
				}
			}
		}

		private ScriptNode ParseTerms(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseFactors(Parser, Optional);

			while (true)
			{
				Parser.SkipWhiteSpace();

				switch (Parser.PeekNextChar())
				{
					case '+':
						Parser.NextChar();
						ScriptNode Right = this.ParseFactors(Parser, false);
						Left = new Add(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						break;

					case '-':
						Parser.NextChar();
						Right = this.ParseFactors(Parser, false);
						Left = new Subtract(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						break;

					default:
						return Left;
				}
			}
		}

		private ScriptNode ParseFactors(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseUnary(Parser, Optional);

			while (true)
			{
				Parser.SkipWhiteSpace();

				switch (Parser.PeekNextChar())
				{
					case '*':
						Parser.NextChar();
						ScriptNode Right = this.ParseUnary(Parser, false);
						Left = new Multiply(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						break;

					case '/':
						Parser.NextChar();
						Right = this.ParseUnary(Parser, false);
						Left = new Divide(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
						break;

					default:
						return Left;
				}
			}
		}

		private ScriptNode ParseUnary(ScriptParser Parser, bool Optional)
		{
			Parser.SkipWhiteSpace();

			int Start = Parser.Position;
			char ch;

			switch (ch = Parser.PeekNextChar())
			{
				case '!':
					Parser.NextChar();
					ScriptNode Node = this.ParseUnary(Parser, false);
					return new Not(Node, Start, Parser.Position - Start, Parser.Expression);

				case '+':
					Parser.NextChar();
					return this.ParseUnary(Parser, false);

				case '-':
					Parser.NextChar();

					switch (Parser.PeekNextChar())
					{
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
						case '.':
							Parser.UndoChar();
							ISemanticElement Element2 = this.ParseElement(Parser, 2);
							return new ConstantElement(Element2, Start, Parser.Position - Start, Parser.Expression);

						default:
							Node = this.ParseUnary(Parser, false);
							return new Negate(Node, Start, Parser.Position - Start, Parser.Expression);
					}

				case '(':
					Parser.NextChar();
					Node = this.ParseNamedExpression(Parser);
					if (Parser.NextChar() != ')')
						throw Parser.SyntaxError("Expected )");
					return Node;

				case '?':
				case '$':
					Parser.NextChar();
					string s = this.ParseName(Parser);
					return new VariableReference(s, Start, Parser.Position - Start, Parser.Expression);

				case '\'':
				case '"':
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
				case '.':
					ISemanticElement Element = this.ParseElement(Parser, 2);
					return new ConstantElement(Element, Start, Parser.Position - Start, Parser.Expression);

				case '<':
					Parser.NextChar();
					Element = this.ParseUri(Parser);
					return new ConstantElement(Element, Start, Parser.Position - Start, Parser.Expression);

				case ':':
					Parser.NextChar();
					Element = this.ParsePrefixedToken(Parser, string.Empty);
					return new ConstantElement(Element, Start, Parser.Position - Start, Parser.Expression);

				default:
					if (!char.IsLetter(ch))
					{
						if (Optional)
							return null;

						throw Parser.SyntaxError("Expected value.");
					}

					s = Parser.NextToken();
					if (Parser.PeekNextChar() == ':')
					{
						Parser.NextChar();
						UriNode Fqn = this.ParsePrefixedToken(Parser, s);

						Parser.SkipWhiteSpace();
						if (Parser.PeekNextChar() == '(')
							return this.ParseExtensionFunction(Fqn.Uri.AbsoluteUri, Parser, Start);
						else
							return new ConstantElement(Fqn, Start, Parser.Position - Start, Parser.Expression);
					}

					return this.ParseFunction(Parser, s, Start, Optional);
			}
		}

		private ScriptNode ParseFunction(ScriptParser Parser, string s, int Start, bool Optional)
		{
			switch (s.ToUpper())
			{
				case "BIND":
					if (Parser.NextNonWhitespaceChar() != '(')
						throw Parser.SyntaxError("Expected (");

					ScriptNode Node = this.ParseNamedExpression(Parser);
					if (!(Node is NamedNode NamedNode))
						throw Parser.SyntaxError("Expected name.");

					if (this.currentRegularPattern is null)
					{
						this.currentRegularPattern = new SparqlRegularPattern();
						this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
					}

					this.currentRegularPattern.AddVariableBinding(NamedNode.LeftOperand, NamedNode.RightOperand);

					if (Parser.NextNonWhitespaceChar() != ')')
						throw Parser.SyntaxError("Expected )");

					return this.ParseUnary(Parser, Optional);

				case "FILTER":
					Node = this.ParseUnary(Parser, false);

					if (this.currentRegularPattern is null)
					{
						this.currentRegularPattern = new SparqlRegularPattern();
						this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
					}

					this.currentRegularPattern.AddFilter(Node);

					return this.ParseUnary(Parser, Optional);

				case "TRUE":
					return new ConstantElement(BooleanValue.True, Start, Parser.Position - Start, Parser.Expression);

				case "FALSE":
					return new ConstantElement(BooleanValue.False, Start, Parser.Position - Start, Parser.Expression);

				case "CONCAT":
					int Start2 = Parser.Position;
					ScriptNode[] Arguments = this.ParseArguments(Parser, 1, int.MaxValue);

					VectorDefinition Vector = new VectorDefinition(Arguments,
						Start2, Parser.Position - Start2, Parser.Expression);

					return new Concat(Vector, Start2, Parser.Position - Start2, Parser.Expression);

				case "ASC":
					Node = this.ParseArgument(Parser);
					return new Asc(Node, Start, Parser.Position - Start, Parser.Expression);

				case "DESC":
					Node = this.ParseArgument(Parser);
					return new Desc(Node, Start, Parser.Position - Start, Parser.Expression);

				case "REGEX":
					Arguments = this.ParseArguments(Parser, 2, 3);
					if (Arguments.Length == 2)
					{
						return new LikeWithOptions(Arguments[0], Arguments[1], null,
							Start, Parser.Position - Start, Parser.Expression);
					}
					else
					{
						return new LikeWithOptions(Arguments[0], Arguments[1], Arguments[2],
							Start, Parser.Position - Start, Parser.Expression);
					}

				case "EXISTS":
					return new Exists(this.ParsePattern(Parser),
						Start, Parser.Position - Start, Parser.Expression);

				case "NOT": // EXISTS
					if (Parser.NextToken() != "EXISTS")
						throw Parser.SyntaxError("Expected EXISTS.");

					return new NotExists(this.ParsePattern(Parser),
						Start, Parser.Position - Start, Parser.Expression);

				case "COUNT":
					Node = this.ParseArgument(Parser);
					return new Count(Node, Start, Parser.Position - Start, Parser.Expression);

				case "SUM":
					Node = this.ParseArgument(Parser);
					return new Sum(Node, Start, Parser.Position - Start, Parser.Expression);

				case "MIN":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Vectors.Min(Node, Start, Parser.Position - Start, Parser.Expression);

				case "MAX":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Vectors.Max(Node, Start, Parser.Position - Start, Parser.Expression);

				case "AVG":
					Node = this.ParseArgument(Parser);
					return new Average(Node, Start, Parser.Position - Start, Parser.Expression);

				case "GROUP_CONCAT":
					Node = this.ParseArgumentOptionalScalarVal(Parser, "separator", out ScriptNode Node2);
					if (Node2 is null)
						return new Concat(Node, Start, Parser.Position - Start, Parser.Expression);
					else
						return new Concat(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "SAMPLE":
					Node = this.ParseArgument(Parser);
					return new Sample(Node, Start, Parser.Position - Start, Parser.Expression);

				case "STR":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Scalar.String(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ABS":
					Node = this.ParseArgument(Parser);
					return new Abs(Node, Start, Parser.Position - Start, Parser.Expression);

				case "CEIL":
					Node = this.ParseArgument(Parser);
					return new Ceiling(Node, Start, Parser.Position - Start, Parser.Expression);

				case "FLOOR":
					Node = this.ParseArgument(Parser);
					return new Floor(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ROUND":
					Node = this.ParseArgument(Parser);
					return new Round(Node, Start, Parser.Position - Start, Parser.Expression);

				case "STRLEN":
					Node = this.ParseArgument(Parser);
					return new Length(Node, Start, Parser.Position - Start, Parser.Expression);

				case "UCASE":
					Node = this.ParseArgument(Parser);
					return new UpperCase(Node, Start, Parser.Position - Start, Parser.Expression);

				case "LCASE":
					Node = this.ParseArgument(Parser);
					return new LowerCase(Node, Start, Parser.Position - Start, Parser.Expression);

				case "CONTAINS":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new Contains(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "STRSTARTS":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new StartsWith(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "STRENDS":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new EndsWith(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "STRBEFORE":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new Before(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "STRAFTER":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new After(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "SUBSTR":
					Arguments = this.ParseArguments(Parser, 2, 3);
					int NodeStart = Arguments[1].Start;
					int NodeLen = Arguments[1].Length;

					Arguments[1] = new Subtract(Arguments[1],
						new ConstantElement(new DoubleNumber(1), NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

					if (Arguments.Length == 2)
					{
						return new Mid(Arguments[0], Arguments[1], null,
							Start, Parser.Position - Start, Parser.Expression);
					}
					else
					{
						return new Mid(Arguments[0], Arguments[1], Arguments[2],
							Start, Parser.Position - Start, Parser.Expression);
					}

				case "YEAR":
					Node = this.ParseArgument(Parser);
					return new Year(Node, Start, Parser.Position - Start, Parser.Expression);

				case "MONTH":
					Node = this.ParseArgument(Parser);
					return new Month(Node, Start, Parser.Position - Start, Parser.Expression);

				case "DAY":
					Node = this.ParseArgument(Parser);
					return new Day(Node, Start, Parser.Position - Start, Parser.Expression);

				case "HOURS":
					Node = this.ParseArgument(Parser);
					return new Hour(Node, Start, Parser.Position - Start, Parser.Expression);

				case "MINUTES":
					Node = this.ParseArgument(Parser);
					return new Minute(Node, Start, Parser.Position - Start, Parser.Expression);

				case "SECONDS":
					Node = this.ParseArgument(Parser);
					return new Second(Node, Start, Parser.Position - Start, Parser.Expression);

				case "NOW":
					this.Parse0Arguments(Parser);
					return new VariableReference("Now", Start, Parser.Position - Start, Parser.Expression);

				case "MD5":
					Node = this.ParseArgument(Parser);
					NodeStart = Node.Start;
					NodeLen = Node.Length;

					return new HexEncode(
						new Md5(Node, NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

				case "SHA1":
					Node = this.ParseArgument(Parser);
					NodeStart = Node.Start;
					NodeLen = Node.Length;

					return new HexEncode(
						new Sha1(Node, NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

				case "SHA256":
					Node = this.ParseArgument(Parser);
					NodeStart = Node.Start;
					NodeLen = Node.Length;

					return new HexEncode(
						new Sha2_256(Node, NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

				case "SHA384":
					Node = this.ParseArgument(Parser);
					NodeStart = Node.Start;
					NodeLen = Node.Length;

					return new HexEncode(
						new Sha2_384(Node, NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

				case "SHA512":
					Node = this.ParseArgument(Parser);
					NodeStart = Node.Start;
					NodeLen = Node.Length;

					return new HexEncode(
						new Sha2_512(Node, NodeStart, NodeLen, Parser.Expression),
						NodeStart, NodeLen, Parser.Expression);

				case "RAND":
					this.Parse0Arguments(Parser);
					return new Uniform(Start, Parser.Position - Start, Parser.Expression);

				case "ENCODE_FOR_URI":
					Node = this.ParseArgument(Parser);
					return new UrlEncode(Node, Start, Parser.Position - Start, Parser.Expression);

				case "STRUUID":
					this.Parse0Arguments(Parser);
					NodeLen = Parser.Position - Start;

					return new Script.Functions.Scalar.String(
						new NewGuid(Start, NodeLen, Parser.Expression),
						Start, NodeLen, Parser.Expression);

				case "UUID":
					this.Parse0Arguments(Parser);
					NodeLen = Parser.Position - Start;

					return new Script.Functions.Scalar.Uri(
						new Add(
							new ConstantElement(new StringValue("urn:uuid:"), Start, NodeLen, Parser.Expression),
							new Script.Functions.Scalar.String(
								new NewGuid(Start, NodeLen, Parser.Expression),
								Start, NodeLen, Parser.Expression),
							Start, NodeLen, Parser.Expression),
						Start, NodeLen, Parser.Expression);

				case "IRI":
				case "URI":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Scalar.Uri(Node, Start, Parser.Position - Start, Parser.Expression);

				case "IF":
					Arguments = this.ParseArguments(Parser, 3, 3);
					return new If(Arguments[0], Arguments[1], Arguments[2],
						Start, Parser.Position - Start, Parser.Expression);

				case "SAMETERM":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new EqualTo(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "BOUND":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Runtime.Exists(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ISIRI":
				case "ISURI":
					Node = this.ParseArgument(Parser);
					return new IsUri(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ISBLANK":
					Node = this.ParseArgument(Parser);
					return new IsBlank(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ISNUMERIC":
					Node = this.ParseArgument(Parser);
					return new IsNumeric(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ISLITERAL":
					Node = this.ParseArgument(Parser);
					return new IsLiteral(Node, Start, Parser.Position - Start, Parser.Expression);

				case "LANG":
					Node = this.ParseArgument(Parser);
					return new Lang(Node, Start, Parser.Position - Start, Parser.Expression);

				case "DATATYPE":
					Node = this.ParseArgument(Parser);
					return new DataType(Node, Start, Parser.Position - Start, Parser.Expression);

				case "BNODE":
					Arguments = this.ParseArguments(Parser, 0, 1);

					if (Arguments.Length == 0)
						return new BNode(Start, Parser.Position - Start, Parser.Expression);
					else
						return new BNode(Arguments[0], Start, Parser.Position - Start, Parser.Expression);

				case "STRDT":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new StrDt(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "STRLANG":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new StrLang(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "LANGMATCHES":
					this.Parse2Arguments(Parser, out Node, out Node2);
					return new LangMatches(Node, Node2, Start, Parser.Position - Start, Parser.Expression);

				case "REPLACE":
					Arguments = this.ParseArguments(Parser, 3, 4);
					if (Arguments.Length == 2)
					{
						return new Replace(Arguments[0], Arguments[1], Arguments[2], true,
							Start, Parser.Position - Start, Parser.Expression);
					}
					else
					{
						return new Replace(Arguments[0], Arguments[1], Arguments[2], Arguments[3],
							Start, Parser.Position - Start, Parser.Expression);
					}

				case "TIMEZONE":
					Node = this.ParseArgument(Parser);
					return new TimeZone(Node, Start, Parser.Position - Start, Parser.Expression);

				case "TZ":
					Node = this.ParseArgument(Parser);
					return new Tz(Node, Start, Parser.Position - Start, Parser.Expression);

				case "COALESCE":
					Arguments = this.ParseArguments(Parser, 1, int.MaxValue);
					return new Coalesce(Arguments, Start, Parser.Position - Start, Parser.Expression);

				case "ERROR":
					Node = this.ParseArgument(Parser);
					return new Script.Functions.Runtime.Error(Node, Start, Parser.Position - Start, Parser.Expression);

				case "TRIPLE":
					Arguments = this.ParseArguments(Parser, 3, 3);
					return new Triple(Arguments[0], Arguments[1], Arguments[2], Start, Parser.Position - Start, Parser.Expression);

				case "SUBJECT":
					Node = this.ParseArgument(Parser);
					return new Subject(Node, Start, Parser.Position - Start, Parser.Expression);

				case "PREDICATE":
					Node = this.ParseArgument(Parser);
					return new Predicate(Node, Start, Parser.Position - Start, Parser.Expression);

				case "OBJECT":
					Node = this.ParseArgument(Parser);
					return new Waher.Content.Semantic.Functions.Object(Node, Start, Parser.Position - Start, Parser.Expression);

				case "ISTRIPLE":
					Node = this.ParseArgument(Parser);
					return new IsTriple(Node, Start, Parser.Position - Start, Parser.Expression);

				default:
					if (Parser.PeekNextChar() == ':')
					{
						s = this.ParsePrefixedToken(Parser, s).Uri.AbsoluteUri;
						return this.ParseExtensionFunction(s, Parser, Start);
					}

					if (Optional)
					{
						int i = s.Length;
						while (i-- > 0)
							Parser.UndoChar();

						return null;
					}

					throw Parser.SyntaxError("Unexpected token: " + s);
			}
		}

		private ScriptNode ParseExtensionFunction(string FullyQualifiedName, ScriptParser Parser, int Start)
		{
			IExtensionFunction Function;

			lock (functionsPerUri)
			{
				if (!functionsPerUri.TryGetValue(FullyQualifiedName, out Function))
				{
					Function = Types.FindBest<IExtensionFunction, string>(FullyQualifiedName);
					functionsPerUri[FullyQualifiedName] = Function;
				}
			}

			if (Function is null)
				throw Parser.SyntaxError("Function not found.");

			ScriptNode[] Arguments = this.ParseArguments(Parser, Function.MinArguments, Function.MaxArguments);

			return Function.CreateFunction(Arguments, Start, Parser.Position - Start, Parser.Expression);
		}

		private void Parse0Arguments(ScriptParser Parser)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			if (Parser.NextNonWhitespaceChar() != ')')
				throw Parser.SyntaxError("Expected )");
		}

		private ScriptNode ParseArgument(ScriptParser Parser)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			ScriptNode Argument = this.ParseExpression(Parser, false);

			if (Parser.NextNonWhitespaceChar() != ')')
				throw Parser.SyntaxError("Expected )");

			return Argument;
		}

		private void Parse2Arguments(ScriptParser Parser, out ScriptNode Argument1, out ScriptNode Argument2)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			Argument1 = this.ParseExpression(Parser, false);

			if (Parser.NextNonWhitespaceChar() != ',')
				throw Parser.SyntaxError("Expected ,");

			Argument2 = this.ParseExpression(Parser, false);

			if (Parser.NextNonWhitespaceChar() != ')')
				throw Parser.SyntaxError("Expected )");
		}

		private ScriptNode ParseArgumentOptionalScalarVal(ScriptParser Parser, string ExpectedScalarName, out ScriptNode ScalarVal)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			ScriptNode Argument = this.ParseExpression(Parser, false);

			switch (Parser.NextNonWhitespaceChar())
			{
				case ')':
					ScalarVal = null;
					return Argument;

				case ';':
					string s = this.ParseName(Parser);
					if (s != ExpectedScalarName)
						throw Parser.SyntaxError("Expected " + ExpectedScalarName);

					if (Parser.NextNonWhitespaceChar() != '=')
						throw Parser.SyntaxError("Expected =");

					ScalarVal = this.ParseExpression(Parser, false);

					if (Parser.NextNonWhitespaceChar() != ')')
						throw Parser.SyntaxError("Expected )");

					return Argument;

				default:
					throw Parser.SyntaxError("Expected ) or ;");
			}
		}

		private ScriptNode[] ParseArguments(ScriptParser Parser, int Min, int Max)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			List<ScriptNode> Arguments = new List<ScriptNode>();

			while (true)
			{
				Arguments.Add(this.ParseExpression(Parser, false));

				if (Parser.NextNonWhitespaceChar() != ',')
				{
					Parser.UndoChar();
					break;
				}
			}

			int c = Arguments.Count;
			if (c < Min)
				throw Parser.SyntaxError("Expected at least " + Min.ToString() + " arguments.");

			if (c > Max)
				throw Parser.SyntaxError("Expected at most " + Max.ToString() + " arguments.");

			if (Parser.NextNonWhitespaceChar() != ')')
				throw Parser.SyntaxError("Expected )");

			return Arguments.ToArray();
		}

		private ISemanticElement ParseElement(ScriptParser Parser, int TriplePosition)
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
						this.ParseTriples(Parser, Node);
						return Node;

					case '(':
						return this.ParseCollection(Parser);

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

							return SemanticElements.Parse(s, DataType, Language);
						}
						else if (!string.IsNullOrEmpty(Language))
							return new StringLiteral(s, Language);
						else
							return new StringLiteral(s);

					case ':':
						return this.ParsePrefixedToken(Parser, string.Empty);

					case '?':
					case '$':
						int Start2 = Parser.Position;
						s = this.ParseName(Parser);
						return new SemanticScriptElement(new VariableReference(s, Start2, Parser.Position - Start2, Parser.Expression));

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
							Start2 = Parser.Position;
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

							if (string.Compare(s, "UNDEF", true) == 0)
							{
								if (TriplePosition != 2)
									throw Parser.SyntaxError("UNDEF not permitted.");

								return new UndefinedLiteral();
							}

							ScriptNode ScriptNode = this.ParseFunction(Parser, s, Start2, true);

							if (ScriptNode is null)
							{
								if (TriplePosition == 0)
								{
									Parser.UndoChar();
									return null;
								}
								else
									throw Parser.SyntaxError("Expected :");
							}

							return new SemanticScriptElement(ScriptNode);
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

		private ISemanticElement ParseCollection(ScriptParser Parser)
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

					if (this.currentRegularPattern is null)
					{
						this.currentRegularPattern = new SparqlRegularPattern();
						this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
					}

					while (!(Loop is null))
					{
						this.currentRegularPattern.AddTriple(new SemanticQueryTriple(Current, RdfDocument.RdfFirst, Loop.Value));

						Loop = Loop.Next;

						if (!(Loop is null))
						{
							BlankNode Next = this.CreateBlankNode();
							this.currentRegularPattern.AddTriple(new SemanticQueryTriple(Current, RdfDocument.RdfRest, Next));
							Current = Next;
						}
					}

					this.currentRegularPattern.AddTriple(new SemanticQueryTriple(Current, RdfDocument.RdfRest, RdfDocument.RdfNil));

					return Result;
				}

				ISemanticElement Element = this.ParseElement(Parser, 2);
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

			return new UriNode(new System.Uri(Namespace + LocalName), Prefix + ":" + LocalName);
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

		private int ParsePositiveInteger(ScriptParser Parser)
		{
			Parser.SkipWhiteSpace();

			int Start = Parser.Position;

			while (char.IsDigit(Parser.PeekNextChar()))
				Parser.NextChar();

			string s = Parser.Expression.Script.Substring(Start, Parser.Position - Start);

			if (!int.TryParse(s, out int i))
				throw Parser.SyntaxError("Expected non-negative integer.");

			return i;
		}

		private SemanticLiteral ParseNumber(ScriptParser Parser)
		{
			int Start = Parser.Position;
			char ch = Parser.PeekNextChar();
			bool HasDigits = false;
			bool HasDecimal = false;
			bool HasExponent = false;

			if (ch == '+' || ch == '-')
			{
				Parser.NextChar();
				ch = Parser.PeekNextChar();
			}

			while (char.IsDigit(ch))
			{
				Parser.NextChar();
				ch = Parser.PeekNextChar();
				HasDigits = true;
			}

			if (ch == '.')
			{
				HasDecimal = true;
				Parser.NextChar();
				ch = Parser.PeekNextChar();

				while (char.IsDigit(ch))
				{
					Parser.NextChar();
					ch = Parser.PeekNextChar();
				}
			}

			if (ch == 'e' || ch == 'E')
			{
				HasExponent = true;
				Parser.NextChar();
				ch = Parser.PeekNextChar();

				if (ch == '+' || ch == '-')
				{
					Parser.NextChar();
					ch = Parser.PeekNextChar();
				}

				while (char.IsDigit(ch))
				{
					Parser.NextChar();
					ch = Parser.PeekNextChar();
				}
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
				if (System.Uri.TryCreate(Short, UriKind.RelativeOrAbsolute, out System.Uri URI))
					return new UriNode(URI, Short);
				else
					throw Parser.SyntaxError("Invalid URI.");
			}
			else
			{
				if (string.IsNullOrEmpty(Short))
					return new UriNode(this.baseUri, Short);
				else if (System.Uri.TryCreate(this.baseUri, Short, out System.Uri URI))
					return new UriNode(URI, Short);
				else
					throw Parser.SyntaxError("Invalid URI.");
			}
		}

	}
}
