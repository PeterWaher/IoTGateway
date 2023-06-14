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
using Waher.Script.Functions.Strings;
using Waher.Script.Operators.Vectors;
using Waher.Script.Persistence.SPARQL.Functions;
using Waher.Script.Operators.Logical;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Operators.Membership;
using Waher.Script.Operators.Arithmetics;
using Waher.Script.Persistence.SPARQL.Filters;
using Waher.Script.Persistence.SPARQL.Patterns;

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
		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>()
		{
			{ "xsd", "http://www.w3.org/2001/XMLSchema#" },
			{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
			{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
			{ "fn", "http://www.w3.org/2005/xpath-functions#" },
			{ "sfn", "http://www.w3.org/ns/sparql#" }
		};
		private QueryType queryType;
		private SparqlRegularPattern currentRegularPattern = null;
		private ISparqlPattern currentPattern = null;
		private int preamblePos;
		private Uri baseUri = null;
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
		public string[] InternalKeywords => new string[] { "DISTINCT", "FROM", "WHERE", "OPTIONAL", "UNION", "MINUS", "ORDER", "BY", "ASK", "CONSTRUCT" };

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
			List<KeyValuePair<ScriptNode, bool>> OrderBy = null;
			SparqlRegularPattern Construct = null;
			ISparqlPattern Where;
			ScriptNode From;

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
					break;

				case "SELECT":
					this.queryType = QueryType.Select;
					this.NextToken(Parser);
					s = this.PeekNextToken(Parser).ToUpper();
					if (string.IsNullOrEmpty(s))
						return false;

					if (s == "DISTINCT")
					{
						this.NextToken(Parser);
						Distinct = true;

						s = this.PeekNextToken(Parser).ToUpper();
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

						while (!string.IsNullOrEmpty(s) && s != "WHERE" && s != "FROM")
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

			if (s == "ORDER")
			{
				if (Columns is null)
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

				//s = Parser.PeekNextToken().ToUpper();
			}

			Result = new SparqlQuery(this.queryType, Distinct, Columns?.ToArray(), 
				ColumnNames?.ToArray(), From, Where, OrderBy?.ToArray(), Construct,
				Parser.Start, Parser.Length, Parser.Expression);

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
						case '{':
							ISparqlPattern Left = this.currentPattern;
							this.currentRegularPattern = new SparqlRegularPattern();
							this.currentPattern = this.currentRegularPattern;

							ISparqlPattern Pattern = this.ParsePattern(Parser);

							if (this.currentPattern.IsEmpty)
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

							if (!this.ParsePatternOperator(Parser))
								break;

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

					bool Again;

					do
					{
						Again = false;
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

								Again = true;
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

								Node = this.ParseUnary(Parser, false);

								if (this.currentRegularPattern is null)
								{
									this.currentRegularPattern = new SparqlRegularPattern();
									this.currentPattern = new IntersectionPattern(this.currentPattern, this.currentRegularPattern);
								}

								this.currentRegularPattern.AddFilter(Node);

								Again = true;
								break;

							case 'u':
							case 'U':
							case 'o':
							case 'O':
							case 'm':
							case 'M':
								Parser.UndoChar();

								if (this.ParsePatternOperator(Parser))
									Again = true;
								else
									throw Parser.SyntaxError("Unexpected token.");
								break;

							default:
								throw Parser.SyntaxError("Unexpected token.");
						}
					}
					while (Again);
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

				default:
					return false;
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

			while (Parser.PeekNextToken() == "||")
			{
				Parser.NextToken();
				ScriptNode Right = this.ParseAnds(Parser, false);
				Left = new Operators.Logical.Or(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
			}

			return Left;
		}

		private ScriptNode ParseAnds(ScriptParser Parser, bool Optional)
		{
			ScriptNode Left = this.ParseComparisons(Parser, Optional);
			if (Left is null)
				return null;

			while (Parser.PeekNextToken() == "&&")
			{
				Parser.NextToken();
				ScriptNode Right = this.ParseComparisons(Parser, false);
				Left = new Operators.Logical.And(Left, Right, Left.Start, Parser.Position - Left.Start, Parser.Expression);
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
							return new ConstantElement(new ObjectValue(Element2), Start, Parser.Position - Start, Parser.Expression);

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
					return new ConstantElement(new ObjectValue(Element), Start, Parser.Position - Start, Parser.Expression);

				case '<':
					Parser.NextChar();
					Element = this.ParseUri(Parser);
					return new ConstantElement(new ObjectValue(Element), Start, Parser.Position - Start, Parser.Expression);

				case ':':
					Parser.NextChar();
					Element = this.ParsePrefixedToken(Parser, string.Empty);
					return new ConstantElement(new ObjectValue(Element), Start, Parser.Position - Start, Parser.Expression);

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
						Element = this.ParsePrefixedToken(Parser, s);
						return new ConstantElement(new ObjectValue(Element), Start, Parser.Position - Start, Parser.Expression);
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
					ScriptNode[] Arguments = this.ParseArguments(Parser, 1, int.MaxValue, true);

					VectorDefinition Vector = new VectorDefinition(Arguments,
						Start2, Parser.Position - Start2, Parser.Expression);

					return new Concat(Vector, Start, Parser.Position - Start, Parser.Expression);

				case "ASC":
					Node = this.ParseArgument(Parser, false);
					return new Asc(Node, Start, Parser.Position - Start, Parser.Expression);

				case "DESC":
					Node = this.ParseArgument(Parser, false);
					return new Desc(Node, Start, Parser.Position - Start, Parser.Expression);

				case "REGEX":
					Arguments = this.ParseArguments(Parser, 2, 3, true);
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

				// Aggregates

				case "COUNT":
				case "SUM":
				case "MIN":
				case "MAX":
				case "AVG":
				case "SAMPLE":
				case "GROUP_CONCAT":

				// Built-in functions

				case "STR":
				case "LANG":
				case "LANGMATCHES":
				case "DATATYPE":
				case "BOUND":
				case "IRI":
				case "URI":
				case "BNODE":
				case "RAND":
				case "ABS":
				case "CEIL":
				case "FLOOR":
				case "ROUND":
				case "STRLEN":
				case "UCASE":
				case "LCASE":
				case "ENCODE_FOR_URI":
				case "CONTAINS":
				case "STRSTARTS":
				case "STRENDS":
				case "STRBEFORE":
				case "STRAFTER":
				case "YEAR":
				case "MONTH":
				case "DAY":
				case "HOURS":
				case "MINUTES":
				case "SECONDS":
				case "TIMEZONE":
				case "TZ":
				case "NOW":
				case "UUID":
				case "STRUUID":
				case "MD5":
				case "SHA1":
				case "SHA256":
				case "SHA384":
				case "SHA512":
				case "COALESCE":
				case "IF":
				case "STRLANG":
				case "STRDT":
				case "SAMETERM":
				case "ISIRI":
				case "ISURI":
				case "ISBLANK":
				case "ISLITERAL":
				case "ISNUMERIC":
				case "SUBSTR":
				case "REPLACE":
				default:
					// TODO: Extensible functions

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

		private ScriptNode ParseArgument(ScriptParser Parser, bool ScriptValueNodes)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			ScriptNode Argument = this.ParseExpression(Parser, false);

			if (ScriptValueNodes)
				Argument = new ScriptValueNode(Argument);

			if (Parser.NextNonWhitespaceChar() != ')')
				throw Parser.SyntaxError("Expected )");

			return Argument;
		}

		private ScriptNode[] ParseArguments(ScriptParser Parser, int Min, int Max,
			bool ScriptValueNodes)
		{
			if (Parser.NextNonWhitespaceChar() != '(')
				throw Parser.SyntaxError("Expected (");

			List<ScriptNode> Arguments = new List<ScriptNode>();

			while (true)
			{
				ScriptNode Node = this.ParseExpression(Parser, false);
				if (ScriptValueNodes)
					Node = new ScriptValueNode(Node);

				Arguments.Add(Node);

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
					case '$':
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
							Start = Parser.Position;
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

							ScriptNode ScriptNode = this.ParseFunction(Parser, s, Start, true);

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
