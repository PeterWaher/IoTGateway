using System;
using System.Numerics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators;
using Waher.Script.Operators.Arithmetics;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Binary;
using Waher.Script.Operators.Calculus;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Operators.Conditional;
using Waher.Script.Operators.Logical;
using Waher.Script.Operators.Matrices;
using Waher.Script.Operators.Membership;
using Waher.Script.Operators.Sets;
using Waher.Script.Operators.Vectors;
using Waher.Script.TypeConversion;
using Waher.Script.Units;

namespace Waher.Script
{
	/// <summary>
	/// Class managing a script expression.
	/// </summary>
	public class Expression
	{
		private readonly static object searchSynch = new object();
		private static Dictionary<Type, Dictionary<Type, ITypeConverter>> converters = null;
		private static Dictionary<string, FunctionRef> functions = null;
		private static Dictionary<string, IConstant> constants = null;
		private static Dictionary<string, IKeyWord> customKeyWords = null;
		private static readonly Dictionary<string, bool> keywords = GetKeywords();

		private ScriptNode root;
		private string script;
		private object tag;
		private int pos;
		private readonly int len;
		private bool containsImplicitPrint = false;
		private bool canSkipWhitespace = true;

		/// <summary>
		/// Class managing a script expression.
		/// </summary>
		/// <param name="Script">Script expression.</param>
		public Expression(string Script)
		{
			this.script = Script;
			this.pos = 0;
			this.len = this.script.Length;

			this.root = this.ParseSequence();
			if (this.pos < this.len)
				throw new SyntaxException("Unexpected end of script.", this.pos, this.script);
		}

		static Expression()
		{
			Types.OnInvalidated += new EventHandler(Types_OnInvalidated);
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			functions = null;
			constants = null;
		}

		private static Dictionary<string, bool> GetKeywords()
		{
			Dictionary<string, bool> Result = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase)
			{
				{ "AND", true },
				{ "AS", true },
				{ "CARTESIAN", true },
				{ "CATCH", true },
				{ "CROSS", true },
				{ "DO", true },
				{ "DOT", true },
				{ "EACH", true },
				{ "ELSE", true },
				{ "FINALLY", true },
				{ "FOR", true },
				{ "FOREACH", true },
				{ "IF", true },
				{ "IN", true },
				{ "INTERSECT", true },
				{ "INTERSECTION", true },
				{ "IS", true },
				{ "LIKE", true },
				{ "MOD", true },
				{ "NAND", true },
				{ "NOR", true },
				{ "NOT", true },
				{ "NOTIN", true },
				{ "NOTLIKE", true },
				{ "OR", true },
				{ "OVER", true },
				{ "STEP", true },
				{ "THEN", true },
				{ "TO", true },
				{ "TRY", true },
				{ "UNION", true },
				{ "UNLIKE", true },
				{ "WHILE", true },
				{ "XNOR", true },
				{ "XOR", true }
			};

			if (customKeyWords is null)
				Search();

			foreach (IKeyWord KeyWord in customKeyWords.Values)
			{
				Result[KeyWord.KeyWord.ToUpper()] = true;

				string[] Aliases = KeyWord.Aliases;
				if (!(Aliases is null))
				{
					foreach (string s in Aliases)
						Result[s.ToUpper()] = true;
				}

				Aliases = KeyWord.InternalKeywords;
				if (!(Aliases is null))
				{
					foreach (string s in Aliases)
						Result[s.ToUpper()] = true;
				}
			}

			return Result;
		}

		internal int Position => this.pos;

		internal bool CanSkipWhitespace
		{
			get => this.canSkipWhitespace;
			set => this.canSkipWhitespace = value;
		}

		/// <summary>
		/// Original script string.
		/// </summary>
		public string Script
		{
			get { return this.script; }
		}

		internal char NextChar()
		{
			if (this.pos < this.len)
				return this.script[this.pos++];
			else
				return (char)0;
		}

		internal char PeekNextChar()
		{
			if (this.pos < this.len)
				return this.script[this.pos];
			else
				return (char)0;
		}

		internal string NextToken()
		{
			this.SkipWhiteSpace();

			if (this.pos >= this.len)
				return string.Empty;

			int Start = this.pos;
			char ch = this.script[this.pos];

			if (char.IsLetter(ch))
			{
				while (this.pos < this.len && char.IsLetterOrDigit(this.script[this.pos]))
					this.pos++;
			}
			else if (char.IsDigit(ch))
			{
				while (this.pos < this.len && char.IsDigit(this.script[this.pos]))
					this.pos++;
			}
			else if (char.IsSymbol(ch))
			{
				while (this.pos < this.len && char.IsSymbol(this.script[this.pos]))
					this.pos++;
			}
			else
				this.pos++;

			return this.script.Substring(Start, this.pos - Start);
		}

		internal string PeekNextToken()
		{
			int Bak = this.pos;
			string Token = this.NextToken();
			this.pos = Bak;

			return Token;
		}

		internal void SkipWhiteSpace()
		{
			if (this.canSkipWhitespace)
			{
				char ch;

				while (this.pos < this.len && ((ch = this.script[this.pos]) <= ' ' || ch == 160))
					this.pos++;
			}
		}

		internal ScriptNode AssertOperandNotNull(ScriptNode Node)
		{
			if (Node is null)
				throw new SyntaxException("Operand missing.", this.pos, this.script);

			return Node;
		}

		internal ScriptNode AssertRightOperandNotNull(ScriptNode Node)
		{
			if (Node is null)
				throw new SyntaxException("Right operand missing.", this.pos, this.script);

			return Node;
		}

		internal ScriptNode ParseSequence()
		{
			ScriptNode Node = this.ParseStatement();
			this.SkipWhiteSpace();

			if (Node is null)
			{
				while (Node is null && this.PeekNextChar() == ';')
				{
					this.pos++;
					Node = this.ParseStatement();
					this.SkipWhiteSpace();
				}
			}

			if (Node is null)
				return null;

			int Start = Node.Start;

			if (!(Node is null) && this.PeekNextChar() == ';')
			{
				this.pos++;
				ScriptNode Node2 = this.ParseStatement();
				if (!(Node2 is null))
				{
					LinkedList<ScriptNode> Statements = new LinkedList<ScriptNode>();
					Statements.AddLast(Node);
					Statements.AddLast(Node2);

					this.SkipWhiteSpace();
					while (this.PeekNextChar() == ';')
					{
						this.pos++;
						Node2 = this.ParseStatement();
						if (Node2 is null)
							break;

						Statements.AddLast(Node2);
						this.SkipWhiteSpace();
					}

					Node = new Sequence(Statements, Start, this.pos - Start, this);
				}
			}

			return Node;
		}

		internal ScriptNode ParseStatement()
		{
			this.SkipWhiteSpace();

			int Start = this.pos;

			switch (char.ToUpper(this.PeekNextChar()))
			{
				case 'D':
					if (this.PeekNextToken().ToUpper() == "DO")
					{
						this.pos += 2;

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement());

						this.SkipWhiteSpace();
						if (this.PeekNextToken().ToUpper() != "WHILE")
							throw new SyntaxException("Expected WHILE.", this.pos, this.script);

						this.pos += 5;

						ScriptNode Condition = this.AssertOperandNotNull(this.ParseList());

						return new DoWhile(Statement, Condition, Start, this.pos - Start, this);
					}
					else
						return this.ParseList();

				case 'W':
					if (this.PeekNextToken().ToUpper() == "WHILE")
					{
						this.pos += 5;

						ScriptNode Condition = this.AssertOperandNotNull(this.ParseList());

						this.SkipWhiteSpace();
						if (this.PeekNextChar() == ':')
							this.pos++;
						else if (this.PeekNextToken().ToUpper() == "DO")
							this.pos += 2;
						else
							throw new SyntaxException("DO or : expected.", this.pos, this.script);

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement());

						return new WhileDo(Condition, Statement, Start, this.pos - Start, this);
					}
					else
						return this.ParseList();

				case 'F':
					switch (this.PeekNextToken().ToUpper())
					{
						case "FOREACH":
							this.pos += 7;
							In In = this.AssertOperandNotNull(this.ParseList()) as In;
							if (In is null)
								throw new SyntaxException("IN statement expected", this.pos, this.script);

							VariableReference Ref = In.LeftOperand as VariableReference;
							if (Ref is null)
								throw new SyntaxException("Variable reference expected", Ref.Start, this.script);

							this.SkipWhiteSpace();
							if (this.PeekNextChar() == ':')
								this.pos++;
							else if (this.PeekNextToken().ToUpper() == "DO")
								this.pos += 2;
							else
								throw new SyntaxException("DO or : expected.", this.pos, this.script);

							ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement());

							return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start, this);

						case "FOR":
							this.pos += 3;
							this.SkipWhiteSpace();

							if (this.PeekNextToken().ToUpper() == "EACH")
							{
								this.pos += 4;
								In = this.AssertOperandNotNull(this.ParseList()) as In;
								if (In is null)
									throw new SyntaxException("IN statement expected", this.pos, this.script);

								Ref = In.LeftOperand as VariableReference;
								if (Ref is null)
									throw new SyntaxException("Variable reference expected", Ref.Start, this.script);

								this.SkipWhiteSpace();
								if (this.PeekNextChar() == ':')
									this.pos++;
								else if (this.PeekNextToken().ToUpper() == "DO")
									this.pos += 2;
								else
									throw new SyntaxException("DO or : expected.", this.pos, this.script);

								Statement = this.AssertOperandNotNull(this.ParseStatement());

								return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start, this);
							}
							else
							{
								if (!(this.AssertOperandNotNull(this.ParseList()) is Assignment Assignment))
									throw new SyntaxException("Assignment expected", this.pos, this.script);

								this.SkipWhiteSpace();
								if (this.PeekNextToken().ToUpper() != "TO")
									throw new SyntaxException("Expected TO.", this.pos, this.script);

								this.pos += 2;

								ScriptNode To = this.AssertOperandNotNull(this.ParseList());
								ScriptNode Step;

								this.SkipWhiteSpace();
								if (this.PeekNextToken().ToUpper() == "STEP")
								{
									this.pos += 4;
									Step = this.AssertOperandNotNull(this.ParseList());
								}
								else
									Step = null;

								this.SkipWhiteSpace();
								if (this.PeekNextChar() == ':')
									this.pos++;
								else if (this.PeekNextToken().ToUpper() == "DO")
									this.pos += 2;
								else
									throw new SyntaxException("DO or : expected.", this.pos, this.script);

								Statement = this.AssertOperandNotNull(this.ParseStatement());

								return new For(Assignment.VariableName, Assignment.Operand, To, Step, Statement, Assignment.Start, this.pos - Start, this);
							}

						default:
							return this.ParseList();
					}

				case 'T':
					if (this.PeekNextToken().ToUpper() == "TRY")
					{
						this.pos += 3;

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement());

						this.SkipWhiteSpace();
						switch (this.PeekNextToken().ToUpper())
						{
							case "FINALLY":
								this.pos += 7;
								ScriptNode Finally = this.AssertOperandNotNull(this.ParseStatement());
								return new TryFinally(Statement, Finally, Start, this.pos - Start, this);

							case "CATCH":
								this.pos += 5;
								ScriptNode Catch = this.AssertOperandNotNull(this.ParseStatement());

								this.SkipWhiteSpace();
								if (this.PeekNextToken().ToUpper() == "FINALLY")
								{
									this.pos += 7;
									Finally = this.AssertOperandNotNull(this.ParseStatement());
									return new TryCatchFinally(Statement, Catch, Finally, Start, this.pos - Start, this);
								}
								else
									return new TryCatch(Statement, Catch, Start, this.pos - Start, this);

							default:
								throw new SyntaxException("Expected CATCH or FINALLY.", this.pos, this.script);
						}
					}
					else
						return this.ParseList();

				case ']':
					this.pos++;
					if (this.PeekNextChar() == ']')
					{
						this.pos++;

						StringBuilder sb = new StringBuilder();
						char ch;

						while ((ch = this.NextChar()) != '[' || this.PeekNextChar() != '[')
						{
							if (ch == 0)
								throw new SyntaxException("Expected [[.", this.pos, this.script);

							sb.Append(ch);
						}

						this.pos++;
						this.containsImplicitPrint = true;
						return new ImplicitPrint(sb.ToString(), Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return this.ParseList();
					}

				default:
					return this.ParseList();
			}
		}

		internal ScriptNode ParseList()
		{
			ScriptNode Node = this.ParseIf();
			int Start;

			if (Node is null)   // Allow null
				Start = this.pos;
			else
				Start = Node.Start;

			this.SkipWhiteSpace();
			if (this.PeekNextChar() == ',')
			{
				List<ScriptNode> Elements = new List<ScriptNode>()
				{
					Node
				};

				while (this.PeekNextChar() == ',')
				{
					this.pos++;
					Node = this.ParseIf();

					Elements.Add(Node);

					this.SkipWhiteSpace();
				}

				Node = new ElementList(Elements.ToArray(), Start, this.pos - Start, this);
			}

			return Node;
		}

		internal ScriptNode ParseIf()
		{
			this.SkipWhiteSpace();

			ScriptNode Condition;
			ScriptNode IfTrue;
			ScriptNode IfFalse;
			int Start = this.pos;

			if (char.ToUpper(this.PeekNextChar()) == 'I' && this.PeekNextToken().ToUpper() == "IF")
			{
				this.pos += 2;
				this.SkipWhiteSpace();

				Condition = this.AssertOperandNotNull(this.ParseAssignments());

				this.SkipWhiteSpace();
				if (this.PeekNextToken().ToUpper() == "THEN")
					this.pos += 4;
				else
					throw new SyntaxException("THEN expected.", this.pos, this.script);

				IfTrue = this.AssertOperandNotNull(this.ParseStatement());

				this.SkipWhiteSpace();
				if (this.PeekNextToken().ToUpper() == "ELSE")
				{
					this.pos += 4;
					IfFalse = this.AssertOperandNotNull(this.ParseStatement());
				}
				else
					IfFalse = null;
			}
			else
			{
				Condition = this.ParseAssignments();
				if (Condition is null)
					return null;

				this.SkipWhiteSpace();
				if (this.PeekNextChar() != '?')
					return Condition;

				this.pos++;

				switch (this.PeekNextChar())
				{
					case '.':
					case '(':
					case '[':
					case '{':
						this.pos--;
						return Condition;   // Null-check operator

					case '?':
						this.pos++;
						IfTrue = this.AssertOperandNotNull(this.ParseStatement());

						return new NullCheck(Condition, IfTrue, Start, this.pos - Start, this);

					default:
						IfTrue = this.AssertOperandNotNull(this.ParseStatement());

						this.SkipWhiteSpace();
						if (this.PeekNextChar() == ':')
						{
							this.pos++;
							IfFalse = this.AssertOperandNotNull(this.ParseStatement());
						}
						else
							IfFalse = null;

						break;
				}
			}

			return new If(Condition, IfTrue, IfFalse, Start, this.pos - Start, this);
		}

		internal ScriptNode ParseAssignments()
		{
			ScriptNode Left = this.ParseLambdaExpression();
			if (Left is null)
				return null;

			int Start = Left.Start;
			VariableReference Ref = Left as VariableReference;

			this.SkipWhiteSpace();

			switch (this.PeekNextChar())
			{
				case ':':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;
						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());

						if (!(Ref is null))
							return new Assignment(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember)
							return new NamedMemberAssignment((NamedMember)Left, Right, Start, this.pos - Start, this);
						else if (Left is DynamicMember)
							return new DynamicMemberAssignment((DynamicMember)Left, Right, Start, this.pos - Start, this);
						else if (Left is VectorIndex)
							return new VectorIndexAssignment((VectorIndex)Left, Right, Start, this.pos - Start, this);
						else if (Left is MatrixIndex)
							return new MatrixIndexAssignment((MatrixIndex)Left, Right, Start, this.pos - Start, this);
						else if (Left is ColumnVector)
							return new MatrixColumnAssignment((ColumnVector)Left, Right, Start, this.pos - Start, this);
						else if (Left is RowVector)
							return new MatrixRowAssignment((RowVector)Left, Right, Start, this.pos - Start, this);
						else if (Left is DynamicIndex)
							return new DynamicIndexAssignment((DynamicIndex)Left, Right, Start, this.pos - Start, this);
						else if (Left is NamedFunctionCall f)
						{
							List<string> ArgumentNames = new List<string>();
							List<ArgumentType> ArgumentTypes = new List<ArgumentType>();
							ArgumentType ArgumentType;

							foreach (ScriptNode Argument in f.Arguments)
							{
								if (Argument is ToVector)
								{
									ArgumentType = ArgumentType.Vector;

									if ((Ref = ((ToVector)Argument).Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is ToMatrix)
								{
									ArgumentType = ArgumentType.Matrix;

									if ((Ref = ((ToMatrix)Argument).Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is ToSet)
								{
									ArgumentType = ArgumentType.Set;

									if ((Ref = ((ToSet)Argument).Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is VectorDefinition)
								{
									ArgumentType = ArgumentType.Scalar;

									VectorDefinition Def = (VectorDefinition)Argument;
									if (Def.Elements.Length != 1 || (Ref = Def.Elements[0] as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (!((Ref = Argument as VariableReference) is null))
								{
									ArgumentType = ArgumentType.Normal;
								}
								else
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								if (ArgumentNames.Contains(Ref.VariableName))
									throw new SyntaxException("Argument name already used.", Argument.Start, this.script);

								ArgumentNames.Add(Ref.VariableName);
								ArgumentTypes.Add(ArgumentType);
							}

							return new FunctionDefinition(f.FunctionName, ArgumentNames.ToArray(), ArgumentTypes.ToArray(), Right, Start, this.pos - Start, this);
						}
						else
							return new PatternMatch(Left, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '+':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;

						if (Ref is null)
							throw new SyntaxException("The += operator can only work on variable references.", this.pos, this.script);

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
						return new AddToSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '-':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;

						if (Ref is null)
							throw new SyntaxException("The -= operator can only work on variable references.", this.pos, this.script);

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
						return new SubtractFromSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '⋅':
				case '*':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;

						if (Ref is null)
							throw new SyntaxException("The *= operator can only work on variable references.", this.pos, this.script);

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
						return new MultiplyWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '/':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;

						if (Ref is null)
							throw new SyntaxException("The /= operator can only work on variable references.", this.pos, this.script);

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
						return new DivideFromSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '^':
					this.pos++;
					if (this.PeekNextChar() == '=')
					{
						this.pos++;

						if (Ref is null)
							throw new SyntaxException("The ^= operator can only work on variable references.", this.pos, this.script);

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
						return new PowerOfSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '&':
					this.pos++;
					switch (this.PeekNextChar())
					{
						case '=':
							this.pos++;

							if (Ref is null)
								throw new SyntaxException("The &= operator can only work on variable references.", this.pos, this.script);

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
							return new BinaryAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);

						case '&':
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos++;

								if (Ref is null)
									throw new SyntaxException("The &&= operator can only work on variable references.", this.pos, this.script);

								Right = this.AssertRightOperandNotNull(this.ParseStatement());
								return new LogicalAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
							}
							else
							{
								this.pos -= 2;
								return Left;
							}

						default:
							this.pos--;
							return Left;
					}

				case '|':
					this.pos++;
					switch (this.PeekNextChar())
					{
						case '=':
							this.pos++;

							if (Ref is null)
								throw new SyntaxException("The |= operator can only work on variable references.", this.pos, this.script);

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
							return new BinaryOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);

						case '|':
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos++;

								if (Ref is null)
									throw new SyntaxException("The ||= operator can only work on variable references.", this.pos, this.script);

								Right = this.AssertRightOperandNotNull(this.ParseStatement());
								return new LogicalOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
							}
							else
							{
								this.pos -= 2;
								return Left;
							}

						default:
							this.pos--;
							return Left;
					}

				case '<':
					this.pos++;
					if (this.PeekNextChar() == '<')
					{
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos++;

							if (Ref is null)
								throw new SyntaxException("The <<= operator can only work on variable references.", this.pos, this.script);

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
							return new ShiftSelfLeft(Ref.VariableName, Right, Start, this.pos - Start, this);
						}
						else
						{
							this.pos -= 2;
							return Left;
						}
					}
					else
					{
						this.pos--;
						return Left;
					}

				case '>':
					this.pos++;
					if (this.PeekNextChar() == '>')
					{
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos++;

							if (Ref is null)
								throw new SyntaxException("The >>= operator can only work on variable references.", this.pos, this.script);

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement());
							return new ShiftSelfRight(Ref.VariableName, Right, Start, this.pos - Start, this);
						}
						else
						{
							this.pos -= 2;
							return Left;
						}
					}
					else
					{
						this.pos--;
						return Left;
					}

				default:
					return Left;
			}
		}

		internal ScriptNode ParseLambdaExpression()
		{
			ScriptNode Left = this.ParseEquivalence();
			if (Left is null)
				return null;

			this.SkipWhiteSpace();

			if (this.PeekNextChar() == '-')
			{
				this.pos++;
				if (this.PeekNextChar() == '>')
				{
					this.pos++;

					int Start = Left.Start;
					string[] ArgumentNames;
					ArgumentType[] ArgumentTypes;

					if (Left is VariableReference Ref)
					{
						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Normal };
					}
					else if (Left is ToVector)
					{
						Ref = ((ToVector)Left).Operand as VariableReference;
						if (Ref is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Vector };
					}
					else if (Left is ToMatrix)
					{
						Ref = ((ToMatrix)Left).Operand as VariableReference;
						if (Ref is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Matrix };
					}
					else if (Left is ToSet)
					{
						Ref = ((ToSet)Left).Operand as VariableReference;
						if (Ref is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Set };
					}
					else if (Left is VectorDefinition Def)
					{
						if (Def.Elements.Length != 1 || (Ref = Def.Elements[0] as VariableReference) is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Scalar };
					}
					else if (Left.GetType() == typeof(ElementList))
					{
						ElementList List = (ElementList)Left;
						int i, c = List.Elements.Length;
						ScriptNode Argument;

						ArgumentNames = new string[c];
						ArgumentTypes = new ArgumentType[c];

						for (i = 0; i < c; i++)
						{
							Argument = List.Elements[i];

							if (!((Ref = Argument as VariableReference) is null))
								ArgumentTypes[i] = ArgumentType.Normal;
							else if (Argument is ToVector)
							{
								Ref = ((ToVector)Argument).Operand as VariableReference;
								if (Ref is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Vector;
							}
							else if (Argument is ToMatrix)
							{
								Ref = ((ToMatrix)Argument).Operand as VariableReference;
								if (Ref is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Matrix;
							}
							else if (Argument is ToSet)
							{
								Ref = ((ToSet)Argument).Operand as VariableReference;
								if (Ref is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Set;
							}
							else if (Argument is VectorDefinition Def2)
							{
								if (Def2.Elements.Length != 1 || (Ref = Def2.Elements[0] as VariableReference) is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Left.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Scalar;
							}
							else
							{
								throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
									Argument.Start, this.script);
							}

							ArgumentNames[i] = Ref.VariableName;
						}
					}
					else
						throw new SyntaxException("Invalid argument list.", Left.Start, this.script);

					ScriptNode Operand = this.ParseEquivalence();
					if (Operand is null)
						throw new SyntaxException("Lambda function body missing.", this.pos, this.script);

					return new LambdaDefinition(ArgumentNames, ArgumentTypes, Operand, Start, this.pos - Start, this);
				}

				this.pos--;
			}

			return Left;
		}

		internal ScriptNode ParseEquivalence()
		{
			ScriptNode Left = this.ParseOrs();
			if (Left is null)
				return null;

			int Start = Left.Start;
			char ch;

			this.SkipWhiteSpace();

			if ((ch = this.PeekNextChar()) == '=')
			{
				int Bak = this.pos;

				this.pos++;
				if (this.PeekNextChar() == '>')
				{
					this.pos++;
					ScriptNode Right = this.AssertRightOperandNotNull(this.ParseOrs());
					return new Implication(Left, Right, Start, this.pos - Start, this);
				}

				this.pos = Bak;
			}
			else if (ch == '<')
			{
				int Bak = this.pos;

				this.pos++;
				if ((ch = this.PeekNextChar()) == '=')
				{
					this.pos++;
					if (this.PeekNextChar() == '>')
					{
						this.pos++;
						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseOrs());
						return new Equivalence(Left, Right, Start, this.pos - Start, this);
					}
				}

				this.pos = Bak;
			}

			return Left;
		}

		internal ScriptNode ParseOrs()
		{
			ScriptNode Left = this.ParseAnds();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (char.ToUpper(this.PeekNextChar()))
				{
					case '∨':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseAnds());
						Left = new Operators.Logical.Or(Left, Right, Start, this.pos - Start, this);
						break;

					case '|':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '|':
								this.pos++;
								if (this.PeekNextChar() == '=')
								{
									this.pos -= 2;
									return Left;
								}

								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Logical.Or(Left, Right, Start, this.pos - Start, this);
								break;

							case '=':
								this.pos--;
								return Left;

							default:
								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Binary.Or(Left, Right, Start, this.pos - Start, this);
								break;
						}
						break;

					case 'O':
					case 'X':
					case 'N':
						switch (this.PeekNextToken().ToUpper())
						{
							case "OR":
								this.pos += 2;
								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Dual.Or(Left, Right, Start, this.pos - Start, this);
								continue;

							case "XOR":
								this.pos += 3;
								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Dual.Xor(Left, Right, Start, this.pos - Start, this);
								continue;

							case "XNOR":
								this.pos += 4;
								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Dual.Xnor(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NOR":
								this.pos += 3;
								Right = this.AssertRightOperandNotNull(this.ParseAnds());
								Left = new Operators.Dual.Nor(Left, Right, Start, this.pos - Start, this);
								continue;

							default:
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseAnds()
		{
			ScriptNode Left = this.ParseMembership();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (char.ToUpper(this.PeekNextChar()))
				{
					case '∧':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseMembership());
						Left = new Operators.Logical.And(Left, Right, Start, this.pos - Start, this);
						break;

					case '&':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '&':
								this.pos++;
								if (this.PeekNextChar() == '=')
								{
									this.pos -= 2;
									return Left;
								}

								Right = this.AssertRightOperandNotNull(this.ParseMembership());
								Left = new Operators.Logical.And(Left, Right, Start, this.pos - Start, this);
								break;

							case '=':
								this.pos--;
								return Left;

							default:
								Right = this.AssertRightOperandNotNull(this.ParseMembership());
								Left = new Operators.Binary.And(Left, Right, Start, this.pos - Start, this);
								break;
						}
						break;

					case 'A':
					case 'N':
						switch (this.PeekNextToken().ToUpper())
						{
							case "AND":
								this.pos += 3;
								Right = this.AssertRightOperandNotNull(this.ParseMembership());
								Left = new Operators.Dual.And(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NAND":
								this.pos += 4;
								Right = this.AssertRightOperandNotNull(this.ParseMembership());
								Left = new Operators.Dual.Nand(Left, Right, Start, this.pos - Start, this);
								continue;

							default:
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseMembership()
		{
			ScriptNode Left = this.ParseComparison();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (char.ToUpper(this.PeekNextChar()))
				{
					case 'A':
					case 'I':
					case 'N':
					case '∈':
					case '∉':
						switch (this.PeekNextToken().ToUpper())
						{
							case "IS":
								this.pos += 2;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new Is(Left, Right, Start, this.pos - Start, this);
								continue;

							case "AS":
								this.pos += 2;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new As(Left, Right, Start, this.pos - Start, this);
								continue;

							case "∈":
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new In(Left, Right, Start, this.pos - Start, this);
								continue;

							case "IN":
								this.pos += 2;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new In(Left, Right, Start, this.pos - Start, this);
								continue;

							case "∉":
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new NotIn(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NOTIN":
								this.pos += 5;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new NotIn(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NOT":
								int Bak = this.pos;
								this.pos += 3;

								this.SkipWhiteSpace();
								if (this.PeekNextToken().ToUpper() == "IN")
								{
									this.pos += 2;
									Right = this.AssertRightOperandNotNull(this.ParseComparison());
									Left = new NotIn(Left, Right, Start, this.pos - Start, this);
									continue;
								}
								else
								{
									this.pos = Bak;
									return Left;
								}

							default:
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseComparison()
		{
			ScriptNode Left = this.ParseShifts();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;
			char ch;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (char.ToUpper(this.PeekNextChar()))
				{
					case '<':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=')
						{
							this.pos++;

							if (this.PeekNextChar() == '>')
							{
								this.pos -= 2;
								return Left;
							}
							else
							{
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new LesserThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
							}
						}
						else if (ch == '>')
						{
							this.pos++;
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new NotEqualTo(Left, Right, Start, this.pos - Start, this);
						}
						else if (ch == '-')
						{
							this.pos++;
							if (this.PeekNextChar() == '>')
							{
								this.pos -= 2;
								return Left;
							}
							else
							{
								this.pos--;
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new LesserThan(Left, Right, Start, this.pos - Start, this);
							}
						}
						else if (ch == '<')
						{
							this.pos--;
							return Left;
						}
						else
						{
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new LesserThan(Left, Right, Start, this.pos - Start, this);
						}
						break;

					case '>':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=')
						{
							this.pos++;
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new GreaterThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
						}
						else if (ch == '>')
						{
							this.pos--;
							return Left;
						}
						else
						{
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new GreaterThan(Left, Right, Start, this.pos - Start, this);
						}
						break;

					case '=':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=')
						{
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new IdenticalTo(Left, Right, Start, this.pos - Start, this);
							}
							else
							{
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new EqualTo(Left, Right, Start, this.pos - Start, this);
							}
						}
						else if (ch == '>')
						{
							this.pos--;
							return Left;
						}
						else
						{
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new EqualTo(Left, Right, Start, this.pos - Start, this);
						}
						break;

					case '≠':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseShifts());
						Left = new NotEqualTo(Left, Right, Start, this.pos - Start, this);
						break;

					case '≡':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseShifts());
						Left = new IdenticalTo(Left, Right, Start, this.pos - Start, this);
						break;

					case '≤':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseShifts());
						Left = new LesserThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
						break;

					case '≥':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseShifts());
						Left = new GreaterThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
						break;

					case '!':
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos++;
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							Left = new NotEqualTo(Left, Right, Start, this.pos - Start, this);
						}
						else
						{
							this.pos--;
							return Left;
						}
						break;

					case '.':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '=':
								this.pos++;
								if (this.PeekNextChar() == '=')
								{
									this.pos++;
									if (this.PeekNextChar() == '=')
									{
										this.pos++;
										Right = this.AssertRightOperandNotNull(this.ParseShifts());
										Left = new IdenticalToElementWise(Left, Right, Start, this.pos - Start, this);
									}
									else
									{
										Right = this.AssertRightOperandNotNull(this.ParseShifts());
										Left = new EqualToElementWise(Left, Right, Start, this.pos - Start, this);
									}
								}
								else
								{
									Right = this.AssertRightOperandNotNull(this.ParseShifts());
									Left = new EqualToElementWise(Left, Right, Start, this.pos - Start, this);
								}
								continue;

							case '<':
								this.pos++;
								if (this.PeekNextChar() == '>')
								{
									this.pos++;
									Right = this.AssertRightOperandNotNull(this.ParseShifts());
									Left = new NotEqualToElementWise(Left, Right, Start, this.pos - Start, this);
									continue;
								}
								else
								{
									this.pos -= 2;
									return Left;
								}

							case '!':
								this.pos++;
								if (this.PeekNextChar() == '=')
								{
									this.pos++;
									Right = this.AssertRightOperandNotNull(this.ParseShifts());
									Left = new NotEqualToElementWise(Left, Right, Start, this.pos - Start, this);
									continue;
								}
								else
								{
									this.pos -= 2;
									return Left;
								}

							default:
								this.pos--;
								return Left;
						}

					case 'L':
					case 'N':
					case 'U':
						switch (this.PeekNextToken().ToUpper())
						{
							case "LIKE":
								this.pos += 4;
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new Like(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NOTLIKE":
								this.pos += 7;
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new NotLike(Left, Right, Start, this.pos - Start, this);
								continue;

							case "UNLIKE":
								this.pos += 6;
								Right = this.AssertRightOperandNotNull(this.ParseShifts());
								Left = new NotLike(Left, Right, Start, this.pos - Start, this);
								continue;

							case "NOT":
								int Bak = this.pos;
								this.pos += 3;
								this.SkipWhiteSpace();
								if (this.PeekNextToken().ToUpper() == "LIKE")
								{
									this.pos += 4;
									Right = this.AssertRightOperandNotNull(this.ParseShifts());
									Left = new NotLike(Left, Right, Start, this.pos - Start, this);
									continue;
								}
								else
								{
									this.pos = Bak;
									return Left;
								}

							default:
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseShifts()
		{
			ScriptNode Left = this.ParseUnions();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (this.PeekNextChar())
				{
					case '<':
						this.pos++;
						if (this.PeekNextChar() == '<')
						{
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos -= 2;
								return Left;
							}

							Right = this.AssertRightOperandNotNull(this.ParseUnions());
							Left = new ShiftLeft(Left, Right, Start, this.pos - Start, this);
						}
						else
						{
							this.pos--;
							return Left;
						}
						break;

					case '>':
						this.pos++;
						if (this.PeekNextChar() == '>')
						{
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos -= 2;
								return Left;
							}

							Right = this.AssertRightOperandNotNull(this.ParseUnions());
							Left = new ShiftRight(Left, Right, Start, this.pos - Start, this);
						}
						else
						{
							this.pos--;
							return Left;
						}
						break;

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseUnions()
		{
			ScriptNode Left = this.ParseIntersections();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;
			char ch;

			while (true)
			{
				this.SkipWhiteSpace();
				if (char.ToUpper(ch = this.PeekNextChar()) == 'U')
				{
					if (this.PeekNextToken().ToUpper() == "UNION")
					{
						this.pos += 5;
						Right = this.AssertRightOperandNotNull(this.ParseIntersections());
						Left = new Union(Left, Right, Start, this.pos - Start, this);
					}
					else
						return Left;
				}
				else if (ch == '∪')
				{
					this.pos++;
					Right = this.AssertRightOperandNotNull(this.ParseIntersections());
					Left = new Union(Left, Right, Start, this.pos - Start, this);
				}
				else
					return Left;
			}
		}

		internal ScriptNode ParseIntersections()
		{
			ScriptNode Left = this.ParseInterval();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;
			char ch;

			while (true)
			{
				this.SkipWhiteSpace();
				if (char.ToUpper(ch = this.PeekNextChar()) == 'I')
				{
					switch (this.PeekNextToken().ToUpper())
					{
						case "INTERSECTION":
							this.pos += 12;
							Right = this.AssertRightOperandNotNull(this.ParseInterval());
							Left = new Intersection(Left, Right, Start, this.pos - Start, this);
							continue;

						case "INTERSECT":
							this.pos += 9;
							Right = this.AssertRightOperandNotNull(this.ParseInterval());
							Left = new Intersection(Left, Right, Start, this.pos - Start, this);
							continue;

						default:
							return Left;
					}
				}
				else if (ch == '∩')
				{
					this.pos++;
					Right = this.AssertRightOperandNotNull(this.ParseInterval());
					Left = new Intersection(Left, Right, Start, this.pos - Start, this);
				}
				else
					return Left;
			}
		}

		internal ScriptNode ParseInterval()
		{
			ScriptNode From = this.ParseTerms();
			if (From is null)
				return null;

			this.SkipWhiteSpace();
			if (this.PeekNextChar() != '.')
				return From;

			this.pos++;
			if (this.PeekNextChar() != '.')
			{
				this.pos--;
				return From;
			}

			this.pos++;
			ScriptNode To = this.AssertRightOperandNotNull(this.ParseTerms());
			int Start = From.Start;

			this.SkipWhiteSpace();
			if (this.PeekNextChar() == '|')
			{
				this.pos++;
				ScriptNode StepSize = this.AssertRightOperandNotNull(this.ParseTerms());
				return new Interval(From, To, StepSize, Start, this.pos - Start, this);
			}
			else
				return new Interval(From, To, Start, this.pos - Start, this);
		}

		internal ScriptNode ParseTerms()
		{
			ScriptNode Left = this.ParseBinomialCoefficients();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;
			char ch;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (this.PeekNextChar())
				{
					case '+':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=' || ch == '+')
						{
							this.pos--;
							return Left;
						}

						Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
						Left = new Add(Left, Right, Start, this.pos - Start, this);
						continue;

					case '-':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=' || ch == '>' || ch == '-')
						{
							this.pos--;
							return Left;
						}

						Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
						Left = new Subtract(Left, Right, Start, this.pos - Start, this);
						continue;

					case '.':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '+':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
								Left = new AddElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							case '-':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
								Left = new SubtractElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							default:
								this.pos--;
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseBinomialCoefficients()
		{
			ScriptNode Left = this.ParseFactors();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				if (char.ToUpper(this.PeekNextChar()) == 'O' && this.PeekNextToken().ToUpper() == "OVER")
				{
					this.pos += 4;
					Right = this.AssertRightOperandNotNull(this.ParseFactors());
					Left = new BinomialCoefficient(Left, Right, Start, this.pos - Start, this);
				}
				else
					return Left;
			}
		}

		internal ScriptNode ParseFactors()
		{
			ScriptNode Left = this.ParsePowers();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (this.PeekNextChar())
				{
					case '⋅':
					case '*':
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos--;
							return Left;
						}

						Right = this.AssertRightOperandNotNull(this.ParsePowers());
						Left = new Multiply(Left, Right, Start, this.pos - Start, this);
						continue;

					case '/':
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos--;
							return Left;
						}

						Right = this.AssertRightOperandNotNull(this.ParsePowers());
						Left = new Divide(Left, Right, Start, this.pos - Start, this);
						continue;

					case '\\':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParsePowers());
						Left = new LeftDivide(Left, Right, Start, this.pos - Start, this);
						continue;

					case 'C':
						switch (this.PeekNextToken().ToUpper())
						{
							case "CROSS":
								this.pos += 5;
								Right = this.AssertRightOperandNotNull(this.ParsePowers());
								Left = new CrossProduct(Left, Right, Start, this.pos - Start, this);
								continue;

							case "CARTESIAN":
								this.pos += 9;
								Right = this.AssertRightOperandNotNull(this.ParsePowers());
								Left = new CartesianProduct(Left, Right, Start, this.pos - Start, this);
								continue;

							default:
								return Left;
						}

					case 'D':
						if (this.PeekNextToken().ToUpper() == "DOT")
						{
							this.pos += 3;
							Right = this.AssertRightOperandNotNull(this.ParsePowers());
							Left = new DotProduct(Left, Right, Start, this.pos - Start, this);
							continue;
						}
						else
							return Left;

					case 'M':
						if (this.PeekNextToken().ToUpper() == "MOD")
						{
							this.pos += 3;
							Right = this.AssertRightOperandNotNull(this.ParsePowers());
							Left = new Residue(Left, Right, Start, this.pos - Start, this);
							continue;
						}
						else
							return Left;

					case '.':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '⋅':
							case '*':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParsePowers());
								Left = new MultiplyElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							case '/':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParsePowers());
								Left = new DivideElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							case '\\':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParsePowers());
								Left = new LeftDivideElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							case 'M':
								if (this.PeekNextToken().ToUpper() == "MOD")
								{
									this.pos += 3;
									Right = this.AssertRightOperandNotNull(this.ParsePowers());
									Left = new ResidueElementWise(Left, Right, Start, this.pos - Start, this);
									continue;
								}
								else
								{
									this.pos--;
									return Left;
								}

							default:
								this.pos--;
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParsePowers()
		{
			ScriptNode Left = this.ParseUnaryPrefixOperator();
			if (Left is null)
				return null;

			ScriptNode Right;
			int Start = Left.Start;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (this.PeekNextChar())
				{
					case '^':
						this.pos++;
						if (this.PeekNextChar() == '=')
						{
							this.pos--;
							return Left;
						}

						Right = this.AssertRightOperandNotNull(this.ParseUnaryPrefixOperator());
						Left = new Power(Left, Right, Start, this.pos - Start, this);
						continue;

					case '²':
						this.pos++;
						Left = new Square(Left, Start, this.pos - Start, this);
						continue;

					case '³':
						this.pos++;
						Left = new Cube(Left, Start, this.pos - Start, this);
						continue;

					case '.':
						this.pos++;
						switch (this.PeekNextChar())
						{
							case '^':
								this.pos++;
								Right = this.AssertRightOperandNotNull(this.ParseUnaryPrefixOperator());
								Left = new PowerElementWise(Left, Right, Start, this.pos - Start, this);
								continue;

							default:
								this.pos--;
								return Left;
						}

					default:
						return Left;
				}
			}
		}

		internal ScriptNode ParseUnaryPrefixOperator()
		{
			this.SkipWhiteSpace();

			int Start = this.pos;
			char ch;

			switch (this.PeekNextChar())
			{
				case '-':
					this.pos++;
					if ((ch = this.PeekNextChar()) == '-')
					{
						this.pos++;
						if (!(this.ParseUnaryPrefixOperator() is VariableReference Ref))
							throw new SyntaxException("The -- operator can only work on variable references.", this.pos, this.script);

						return new PreDecrement(Ref.VariableName, Start, this.pos - Start, this);
					}
					else if ((ch >= '0' && ch <= '9') || (ch == '.'))
					{
						this.pos--;
						return this.ParseSuffixOperator();
					}
					else if (ch == '>')
					{
						this.pos--;
						return this.ParseSuffixOperator();
					}
					else
						return new Negate(this.AssertOperandNotNull(this.ParseFactors()), Start, this.pos - Start, this);

				case '+':
					this.pos++;
					if ((ch = this.PeekNextChar()) == '+')
					{
						this.pos++;
						if (!(this.ParseUnaryPrefixOperator() is VariableReference Ref))
							throw new SyntaxException("The ++ operator can only work on variable references.", this.pos, this.script);

						return new PreIncrement(Ref.VariableName, Start, this.pos - Start, this);
					}
					else if ((ch >= '0' && ch <= '9') || (ch == '.'))
						return this.ParseSuffixOperator();
					else
						return this.AssertOperandNotNull(this.ParseFactors());

				case '!':
					this.pos++;
					return new Not(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start, this);

				case 'N':
					if (this.PeekNextToken().ToUpper() == "NOT")
					{
						this.pos += 3;
						return new Not(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start, this);
					}
					else
						return this.ParseSuffixOperator();

				case '~':
					this.pos++;
					return new Complement(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start, this);

				default:
					return this.ParseSuffixOperator();
			}
		}

		internal ScriptNode ParseSuffixOperator()
		{
			ScriptNode Node = this.ParseObject();
			if (Node is null)
				return null;

			bool NullCheck = false;
			int Start = Node.Start;
			char ch;

			while (true)
			{
				this.SkipWhiteSpace();
				switch (ch = this.PeekNextChar())
				{
					case '?':
						if (NullCheck)
						{
							this.pos--;
							return Node;
						}

						this.pos++;
						ch = this.PeekNextChar();
						switch (ch)
						{
							case '.':
							case '(':
							case '[':
							case '{':
								NullCheck = true;
								continue;

							default:
								this.pos--;
								return Node;
						}

					case '.':
						this.pos++;

						ch = this.PeekNextChar();
						if (ch == '=' || ch == '+' || ch == '-' || ch == '^' || ch == '.' || ch == '*' || ch == '⋅' || ch == '/' || ch == '\\' || ch == '<' || ch == '!')
						{
							this.pos--;
							return Node;
						}

						if (char.ToUpper(ch) == 'M' && this.PeekNextToken().ToUpper() == "MOD")
						{
							this.pos--;
							return Node;
						}

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseObject());
						VariableReference Ref = Right as VariableReference;

						if (Ref is null)
							Node = new DynamicMember(Node, Right, NullCheck, Start, this.pos - Start, this);
						else
							Node = new NamedMember(Node, Ref.VariableName, NullCheck, Start, this.pos - Start, this);

						break;

					case '(':
						this.pos++;
						Right = this.ParseList();

						this.SkipWhiteSpace();
						if (this.PeekNextChar() != ')')
							throw new SyntaxException("Expected ).", this.pos, this.script);

						this.pos++;

						Ref = Node as VariableReference;
						if (Ref is null)
						{
							if (Node is NamedMember NamedMember)
							{
								if (Right is null)
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, new ScriptNode[0], NullCheck, Start, this.pos - Start, this);
								else if (Right.GetType() == typeof(ElementList))
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, ((ElementList)Right).Elements, NullCheck, Start, this.pos - Start, this);
								else
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, new ScriptNode[] { Right }, NullCheck, Start, this.pos - Start, this);
							}// TODO: Dynamic named method call.
							else
							{
								if (Right is null)
									Node = new DynamicFunctionCall(Node, new ScriptNode[0], NullCheck, Start, this.pos - Start, this);
								else if (Right.GetType() == typeof(ElementList))
									Node = new DynamicFunctionCall(Node, ((ElementList)Right).Elements, NullCheck, Start, this.pos - Start, this);
								else
									Node = new DynamicFunctionCall(Node, new ScriptNode[] { Right }, NullCheck, Start, this.pos - Start, this);
							}
						}
						else
							Node = GetFunction(Ref.VariableName, Right, NullCheck, Start, this.pos - Start, this);

						break;

					case '[':
						this.pos++;
						Right = this.ParseList();

						this.SkipWhiteSpace();
						if (this.PeekNextChar() != ']')
							throw new SyntaxException("Expected ].", this.pos, this.script);

						this.pos++;

						if (Right is null)
							Node = new ToVector(Node, NullCheck, Start, this.pos - Start, this);
						else if (Right.GetType() == typeof(ElementList))
						{
							ElementList List = (ElementList)Right;

							if (List.Elements.Length == 2)
							{
								if (List.Elements[0] is null)
								{
									if (List.Elements[1] is null)
										Node = new ToMatrix(Node, NullCheck, Start, this.pos - Start, this);
									else
										Node = new RowVector(Node, List.Elements[1], NullCheck, Start, this.pos - Start, this);
								}
								else if (List.Elements[1] is null)
									Node = new ColumnVector(Node, List.Elements[0], NullCheck, Start, this.pos - Start, this);
								else
									Node = new MatrixIndex(Node, List.Elements[0], List.Elements[1], NullCheck, Start, this.pos - Start, this);
							}
							else
								Node = new DynamicIndex(Node, List, NullCheck, Start, this.pos - Start, this);
						}
						else
							Node = new VectorIndex(Node, Right, NullCheck, Start, this.pos - Start, this);
						break;

					case '{':
						int Bak = this.pos;
						this.pos++;
						this.SkipWhiteSpace();
						if (this.PeekNextChar() == '}')
						{
							this.pos++;
							Node = new ToSet(Node, NullCheck, Start, this.pos - Start, this);
							break;
						}
						else
						{
							this.pos = Bak;
							return Node;
						}

					case '+':
						this.pos++;
						if (this.PeekNextChar() == '+')
						{
							this.pos++;
							Ref = Node as VariableReference;
							if (Ref is null)
							{
								this.pos -= 2;  // Can be a prefix operator.
								return Node;
							}

							if (NullCheck)
								throw new SyntaxException("Null-checked post increment operator not defined.", this.pos, this.script);

							Node = new PostIncrement(Ref.VariableName, Start, this.pos - Start, this);
							break;
						}
						else
						{
							this.pos--;
							return Node;
						}

					case '-':
						this.pos++;
						if (this.PeekNextChar() == '-')
						{
							this.pos++;
							Ref = Node as VariableReference;
							if (Ref is null)
							{
								this.pos -= 2;  // Can be a prefix operator.
								return Node;
							}

							if (NullCheck)
								throw new SyntaxException("Null-checked post decrement operator not defined.", this.pos, this.script);

							Node = new PostDecrement(Ref.VariableName, Start, this.pos - Start, this);
							break;
						}
						else
						{
							this.pos--;
							return Node;
						}

					case '%':
						this.pos++;

						if (NullCheck)
							throw new SyntaxException("Null-checked % operator not defined.", this.pos, this.script);

						if (this.PeekNextChar() == '0')
						{
							this.pos++;

							if (this.PeekNextChar() == '0')
							{
								this.pos++;
								Node = new Perdiezmil(Node, Start, this.pos - Start, this);
							}
							else
								Node = new Permil(Node, Start, this.pos - Start, this);
						}
						else
							Node = new Percent(Node, Start, this.pos - Start, this);
						break;

					case '‰':
						this.pos++;

						if (NullCheck)
							throw new SyntaxException("Null-checked ‰ operator not defined.", this.pos, this.script);

						if (this.PeekNextChar() == '0')
						{
							this.pos++;
							Node = new Perdiezmil(Node, Start, this.pos - Start, this);
						}
						else
							Node = new Permil(Node, Start, this.pos - Start, this);
						break;

					case '‱':
						this.pos++;

						if (NullCheck)
							throw new SyntaxException("Null-checked ‱ operator not defined.", this.pos, this.script);

						Node = new Perdiezmil(Node, Start, this.pos - Start, this);
						break;

					case '°':
						this.pos++;

						if (NullCheck)
							throw new SyntaxException("Null-checked ° operator not defined.", this.pos, this.script);

						if ((ch = this.PeekNextChar()) == 'C' || ch == 'F')
						{
							this.pos++;

							Unit Unit = new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("°" + new string(ch, 1)), 1));

							if (Node is ConstantElement ConstantElement)
							{
								if (ConstantElement.Constant is DoubleNumber DoubleNumber)
								{
									Node = new ConstantElement(new PhysicalQuantity(DoubleNumber.Value, Unit),
										ConstantElement.Start, this.pos - ConstantElement.Start, this);
								}
								else if (ConstantElement.Constant is PhysicalQuantity)
									Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);
								else
								{
									this.pos--;
									Node = new DegToRad(Node, Start, this.pos - Start, this);
								}
							}
							else
								Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);
						}
						else
							Node = new DegToRad(Node, Start, this.pos - Start, this);

						break;

					case '\'':
					case '"':
					case '′':
					case '″':
					case '‴':
						int i = 0;

						if (NullCheck)
							throw new SyntaxException("Null-checked differencial operators not defined.", this.pos, this.script);

						while (true)
						{
							switch (this.PeekNextChar())
							{
								case '\'':
								case '′':
									i++;
									this.pos++;
									continue;

								case '"':
								case '″':
									i += 2;
									this.pos++;
									continue;

								case '‴':
									i += 3;
									this.pos++;
									continue;
							}

							break;
						}

						Node = new DefaultDifferentiation(Node, i, NullCheck, Start, this.pos - Start, this);
						break;

					case 'T':
						this.pos++;
						ch = this.PeekNextChar();
						if (char.IsLetter(ch) || char.IsDigit(ch))
						{
							this.pos--;
							return Node;
						}
						else
						{
							if (NullCheck)
								throw new SyntaxException("Null-checked T operator not defined.", this.pos, this.script);

							Node = new Transpose(Node, Start, this.pos - Start, this);
							break;
						}

					case 'H':
						this.pos++;
						ch = this.PeekNextChar();
						if (char.IsLetter(ch) || char.IsDigit(ch))
						{
							this.pos--;
							return Node;
						}
						else
						{
							if (NullCheck)
								throw new SyntaxException("Null-checked H operator not defined.", this.pos, this.script);

							Node = new ConjugateTranspose(Node, Start, this.pos - Start, this);
							break;
						}

					case '†':
						if (NullCheck)
							throw new SyntaxException("Null-checked † operator not defined.", this.pos, this.script);

						this.pos++;
						Node = new ConjugateTranspose(Node, Start, this.pos - Start, this);
						break;

					case '!':
						if (NullCheck)
							throw new SyntaxException("Null-checked ! operator not defined.", this.pos, this.script);

						this.pos++;
						switch (this.PeekNextChar())
						{
							case '!':
								this.pos++;
								Node = new SemiFaculty(Node, Start, this.pos - Start, this);
								break;

							case '=':
								this.pos--;
								return Node;

							default:
								Node = new Faculty(Node, Start, this.pos - Start, this);
								break;
						}
						break;

					default:
						if (NullCheck)
							throw new SyntaxException("Null-checked unit operator not defined.", this.pos, this.script);

						if (char.IsLetter(ch))
						{
							Bak = this.pos;

							Unit Unit = this.ParseUnit(true);
							if (Unit is null)
							{
								this.pos = Bak;
								return Node;
							}

							if (Node is ConstantElement ConstantElement)
							{
								if (ConstantElement.Constant is DoubleNumber DoubleNumber)
								{
									Node = new ConstantElement(new PhysicalQuantity(DoubleNumber.Value, Unit),
										ConstantElement.Start, this.pos - ConstantElement.Start, this);
								}
								else if (ConstantElement.Constant is PhysicalQuantity)
									Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);
								else
								{
									this.pos = Bak;
									return Node;
								}
							}
							else
								Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);
						}
						else
							return Node;
						break;
				}

				NullCheck = false;
			}
		}

		internal Unit ParseUnit(bool PermitPrefix)
		{
			Prefix Prefix;
			LinkedList<KeyValuePair<AtomicUnit, int>> Factors = new LinkedList<KeyValuePair<AtomicUnit, int>>();
			KeyValuePair<AtomicUnit, int>[] CompoundFactors;
			string Name, Name2, s;
			int Start = this.pos;
			int LastCompletion = Start;
			int Exponent;
			int i;
			char ch = this.NextChar();
			bool LastDivision = false;

			if (PermitPrefix)
			{
				if (ch == 'd' && this.PeekNextChar() == 'a')
				{
					this.pos++;
					Prefix = Prefix.Deka;
				}
				else if (!Prefixes.TryParsePrefix(ch, out Prefix))
					this.pos--;

				i = this.pos;
				ch = this.NextChar();
			}
			else
			{
				Prefix = Prefix.None;
				i = this.pos - 1;
			}

			if (!char.IsLetter(ch) && Prefix != Prefix.None)
			{
				this.pos = i = Start;
				Prefix = Prefix.None;
				ch = this.NextChar();
			}
			else if (ch == '/')
			{
				LastDivision = true;
				ch = this.NextChar();
				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = this.NextChar();
			}

			while (char.IsLetter(ch) || ch == '(')
			{
				if (ch == '(')
				{
					Unit Unit = this.ParseUnit(false);

					if (Unit is null)
					{
						this.pos = Start;
						return null;
					}

					ch = this.NextChar();
					while (ch > 0 && (ch <= ' ' || ch == 160))
						ch = this.NextChar();

					if (ch != ')')
						throw new SyntaxException("Expected ).", this.pos, this.script);

					if (ch == '^')
					{
						ch = this.NextChar();
						while (ch > 0 && (ch <= ' ' || ch == 160))
							ch = this.NextChar();

						if (ch == '-' || char.IsDigit(ch))
						{
							i = this.pos - 1;

							if (ch == '-')
								ch = this.NextChar();

							while (char.IsDigit(ch))
								ch = this.NextChar();

							if (ch == 0)
								s = this.script.Substring(i, this.pos - i);
							else
								s = this.script.Substring(i, this.pos - i - 1);

							if (!int.TryParse(s, out Exponent))
							{
								this.pos = Start;
								return null;
							}
						}
						else
						{
							this.pos = Start;
							return null;
						}
					}
					else if (ch == '²')
					{
						Exponent = 2;
						ch = this.NextChar();
					}
					else if (ch == '³')
					{
						Exponent = 3;
						ch = this.NextChar();
					}
					else
						Exponent = 1;

					if (LastDivision)
					{
						foreach (KeyValuePair<AtomicUnit, int> Factor in Unit.Factors)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, -Factor.Value * Exponent));
					}
					else
					{
						foreach (KeyValuePair<AtomicUnit, int> Factor in Unit.Factors)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, Factor.Value * Exponent));
					}

					ch = this.NextChar();
				}
				else
				{
					while (char.IsLetter(ch))
						ch = this.NextChar();

					if (ch == 0)
						Name = this.script.Substring(i, this.pos - i);
					else
						Name = this.script.Substring(i, this.pos - i - 1);

					if (PermitPrefix)
					{
						if (keywords.ContainsKey(Name2 = this.script.Substring(Start, i - Start) + Name))
						{
							this.pos = Start;
							return null;
						}
						else if (Unit.TryGetCompoundUnit(Name2, out CompoundFactors))
						{
							Prefix = Prefix.None;
							Name = Name2;
						}
						else if (Unit.ContainsDerivedOrBaseUnit(Name2))
						{
							Prefix = Prefix.None;
							Name = Name2;
							CompoundFactors = null;
						}
						else if (!Unit.TryGetCompoundUnit(Name, out CompoundFactors))
							CompoundFactors = null;
					}
					else if (!Unit.TryGetCompoundUnit(Name, out CompoundFactors))
						CompoundFactors = null;

					while (ch > 0 && (ch <= ' ' || ch == 160))
						ch = this.NextChar();

					if (ch == '^')
					{
						ch = this.NextChar();
						while (ch > 0 && (ch <= ' ' || ch == 160))
							ch = this.NextChar();

						if (ch == '-' || char.IsDigit(ch))
						{
							i = this.pos - 1;

							if (ch == '-')
								ch = this.NextChar();

							while (char.IsDigit(ch))
								ch = this.NextChar();

							if (ch == 0)
								s = this.script.Substring(i, this.pos - i);
							else
								s = this.script.Substring(i, this.pos - i - 1);

							if (!int.TryParse(s, out Exponent))
							{
								this.pos = Start;
								return null;
							}
						}
						else
						{
							this.pos = Start;
							return null;
						}
					}
					else if (ch == '²')
					{
						Exponent = 2;
						ch = this.NextChar();
					}
					else if (ch == '³')
					{
						Exponent = 3;
						ch = this.NextChar();
					}
					else
						Exponent = 1;

					if (CompoundFactors is null)
					{
						if (LastDivision)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Name), -Exponent));
						else
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Name), Exponent));
					}
					else
					{
						if (LastDivision)
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in CompoundFactors)
								Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Segment.Key, -Segment.Value * Exponent));
						}
						else
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in CompoundFactors)
								Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Segment.Key, Segment.Value * Exponent));
						}
					}
				}

				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = this.NextChar();

				if (ch == 0)
					LastCompletion = this.pos;
				else
					LastCompletion = this.pos - 1;

				if (ch == '*' || ch == '⋅')
					LastDivision = false;
				else if (ch == '/')
					LastDivision = true;
				else
					break;

				ch = this.NextChar();
				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = this.NextChar();

				i = this.pos - 1;
				PermitPrefix = false;
			}

			this.pos = LastCompletion;

			if (Factors.First is null)
			{
				this.pos = Start;
				return null;
			}

			return new Unit(Prefix, Factors);
		}

		private static ScriptNode GetFunction(string FunctionName, ScriptNode Arguments, bool NullCheck, int Start, int Length, Expression Expression)
		{
			Dictionary<string, FunctionRef> F;
			int NrParameters;
			ElementList ElementList = null;
			object[] P;

			if (Arguments is null)
			{
				NrParameters = 0;
				P = new object[3];
			}
			else if (Arguments.GetType() == typeof(ElementList))
			{
				ElementList = (ElementList)Arguments;
				NrParameters = ElementList.Elements.Length;
				P = new object[NrParameters + 3];
				ElementList.Elements.CopyTo(P, 0);
			}
			else
			{
				NrParameters = 1;
				P = new object[4];
				P[0] = Arguments;
			}

			P[NrParameters] = Start;
			P[NrParameters + 1] = Length;
			P[NrParameters + 2] = Expression;

			F = functions;
			if (F is null)
			{
				Search();
				F = functions;
			}

			if (F.TryGetValue(FunctionName + " " + NrParameters.ToString(), out FunctionRef Ref))
				return (Function)Ref.Constructor.Invoke(P);
			else
			{
				if (!(ElementList is null))
					return new NamedFunctionCall(FunctionName, ElementList.Elements, NullCheck, Start, Length, Expression);
				else if (Arguments is null)
					return new NamedFunctionCall(FunctionName, new ScriptNode[0], NullCheck, Start, Length, Expression);
				else
					return new NamedFunctionCall(FunctionName, new ScriptNode[] { Arguments }, NullCheck, Start, Length, Expression);
			}
		}

		internal static bool TryGetConstant(string Name, out IElement ValueElement)
		{
			Dictionary<string, IConstant> C = constants;
			if (C is null)
			{
				Search();
				C = constants;
			}

			if (!C.TryGetValue(Name, out IConstant Constant))
			{
				ValueElement = null;
				return false;
			}

			ValueElement = Constant.ValueElement;
			return true;
		}

		internal static LambdaDefinition GetFunctionLambdaDefinition(string FunctionName, int Start, int Length,
			Expression Expression)
		{
			Dictionary<string, FunctionRef> F;

			F = functions;
			if (F is null)
			{
				Search();
				F = functions;
			}

			if (F.TryGetValue(FunctionName, out FunctionRef Ref))
			{
				string[] ArgumentNames = Ref.Function.DefaultArgumentNames;
				int i, c = ArgumentNames.Length;
				ArgumentType[] ArgumentTypes = new ArgumentType[c];
				object[] Arguments = new object[c + 3];

				Arguments[c] = Start;
				Arguments[c + 1] = Length;
				Arguments[c + 2] = Expression;

				for (i = 0; i < c; i++)
				{
					Arguments[i] = new VariableReference(ArgumentNames[i], Start, Length, Expression);
					ArgumentTypes[i] = ArgumentType.Normal;
				}

				ScriptNode FunctionCall = (ScriptNode)Ref.Constructor.Invoke(Arguments);

				return new LambdaDefinition(ArgumentNames, ArgumentTypes, FunctionCall, Start, Length, Expression);
			}
			else
				return null;
		}

		private static void Search()
		{
			lock (searchSynch)
			{
				if (functions is null)
				{
					Dictionary<int, object[]> ParameterValuesPerNrParameters = new Dictionary<int, object[]>();
					Dictionary<string, FunctionRef> Found = new Dictionary<string, FunctionRef>(StringComparer.CurrentCultureIgnoreCase);
					ParameterInfo[] Parameters;
					ParameterInfo PInfo;
					FunctionRef Ref;
					string[] Aliases;
					Function Function;
					string s;
					int i, c;
					TypeInfo TI;

					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IFunction)))
					{
						TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsGenericTypeDefinition)
							continue;

						foreach (ConstructorInfo CI in TI.DeclaredConstructors)
						{
							Parameters = CI.GetParameters();
							c = Parameters.Length;
							if (c < 3)
								continue;

							PInfo = Parameters[c - 1];
							if (PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(Expression))
								continue;

							PInfo = Parameters[c - 2];
							if (PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(int))
								continue;

							PInfo = Parameters[c - 3];
							if (PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(int))
								continue;

							for (i = c - 4; i >= 0; i--)
							{
								PInfo = Parameters[i];
								if (PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(ScriptNode))
									break;
							}

							if (i >= 0)
								continue;

							try
							{
								if (!ParameterValuesPerNrParameters.TryGetValue(c, out object[] ParameterValues))
								{
									ParameterValues = new object[c];
									ParameterValues[c - 1] = null;
									ParameterValues[c - 2] = 0;
									ParameterValues[c - 3] = 0;
									ParameterValuesPerNrParameters[c] = ParameterValues;
								}

								Function = CI.Invoke(ParameterValues) as Function;
								if (Function is null)
									continue;

								s = Function.FunctionName + " " + (c - 3).ToString();
								if (Found.ContainsKey(s))
								{
									Log.Warning("Function with name " + Function.FunctionName + " and " + (c - 3).ToString() +
										" parameters previously registered. Function ignored.",
										T.FullName, new KeyValuePair<string, object>("Previous", Found[s].Function.GetType().FullName));
								}
								else
								{
									Ref = new FunctionRef()
									{
										Constructor = CI,
										Function = Function,
										NrParameters = c - 3
									};

									Found[s] = Ref;

									if (!Found.ContainsKey(Function.FunctionName))
										Found[Function.FunctionName] = Ref;
								}

								Aliases = Function.Aliases;
								if (!(Aliases is null))
								{
									foreach (string Alias in Aliases)
									{
										s = Alias + " " + (c - 3).ToString();
										if (Found.ContainsKey(s))
										{
											Log.Warning("Function with name " + Alias + " and " + (c - 3).ToString() +
												" parameters previously registered. Function ignored.",
												T.FullName, new KeyValuePair<string, object>("Previous", Found[s].Function.GetType().FullName));
										}
										else
										{
											Ref = new FunctionRef()
											{
												Constructor = CI,
												Function = Function,
												NrParameters = c - 3
											};

											Found[s] = Ref;

											if (!Found.ContainsKey(Alias))
												Found[Alias] = Ref;
										}
									}
								}
							}
							catch (Exception ex)
							{
								ex = Log.UnnestException(ex);

								if (ex is AggregateException ex2)
								{
									foreach (Exception ex3 in ex2.InnerExceptions)
										Log.Critical(ex3);
								}
								else
									Log.Critical(ex);
							}
						}
					}

					functions = Found;
				}

				if (constants is null)
				{
					Dictionary<string, IConstant> Found = new Dictionary<string, IConstant>(StringComparer.CurrentCultureIgnoreCase);
					string[] Aliases;
					string s;
					TypeInfo TI;

					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IConstant)))
					{
						TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsGenericTypeDefinition)
							continue;

						try
						{
							IConstant Constant = (IConstant)Activator.CreateInstance(T);

							s = Constant.ConstantName;
							if (Found.ContainsKey(s))
							{
								Log.Warning("Constant with name " + s + " previously registered. Constant ignored.",
									T.FullName, new KeyValuePair<string, object>("Previous", Constant.GetType().FullName));
							}
							else
								Found[s] = Constant;

							Aliases = Constant.Aliases;
							if (!(Aliases is null))
							{
								foreach (string Alias in Aliases)
								{
									if (Found.ContainsKey(Alias))
									{
										Log.Warning("Constant with name " + Alias + " previously registered. Constant ignored.",
											T.FullName, new KeyValuePair<string, object>("Previous", Constant.GetType().FullName));
									}
									else
										Found[Alias] = Constant;
								}
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					constants = Found;
				}

				if (customKeyWords is null)
				{
					Dictionary<string, IKeyWord> Found = new Dictionary<string, IKeyWord>(StringComparer.CurrentCultureIgnoreCase);
					string[] Aliases;
					string s;
					TypeInfo TI;

					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IKeyWord)))
					{
						TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsGenericTypeDefinition)
							continue;

						try
						{
							IKeyWord KeyWord = (IKeyWord)Activator.CreateInstance(T);

							s = KeyWord.KeyWord;
							if (Found.ContainsKey(s))
							{
								Log.Warning("Keyword with name " + s + " previously registered. Keyword ignored.",
									T.FullName, new KeyValuePair<string, object>("Previous", KeyWord.GetType().FullName));
							}
							else
								Found[s] = KeyWord;

							Aliases = KeyWord.Aliases;
							if (!(Aliases is null))
							{
								foreach (string Alias in Aliases)
								{
									if (Found.ContainsKey(Alias))
									{
										Log.Warning("Keyword with name " + Alias + " previously registered. Keyword ignored.",
											T.FullName, new KeyValuePair<string, object>("Previous", KeyWord.GetType().FullName));
									}
									else
										Found[Alias] = KeyWord;
								}
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					customKeyWords = Found;
				}
			}
		}

		private class FunctionRef
		{
			public ConstructorInfo Constructor;
			public Function Function;
			public int NrParameters;
		}

		internal ScriptNode ParseObject()
		{
			this.SkipWhiteSpace();

			ScriptNode Node;
			int Start = this.pos;
			char ch = this.PeekNextChar();

			if (ch == '(')
			{
				this.pos++;
				Node = this.ParseSequence();

				this.SkipWhiteSpace();
				if (this.PeekNextChar() != ')')
					throw new SyntaxException("Expected ).", this.pos, this.script);

				this.pos++;

				Node.Start = Start;
				return Node;
			}
			else if (ch == '[')
			{
				this.pos++;
				this.SkipWhiteSpace();
				if (this.PeekNextChar() == ']')
				{
					this.pos++;
					return new VectorDefinition(new ScriptNode[0], Start, this.pos - Start, this);
				}

				Node = this.ParseStatement();

				this.SkipWhiteSpace();
				if (this.PeekNextChar() != ']')
					throw new SyntaxException("Expected ].", this.pos, this.script);

				this.pos++;

				if (Node is For For)
				{
					if (IsVectorDefinition(For.RightOperand))
						return new MatrixForDefinition(For, Start, this.pos - Start, this);
					else
						return new VectorForDefinition(For, Start, this.pos - Start, this);
				}
				else if (Node is ForEach ForEach)
				{
					if (IsVectorDefinition(ForEach.RightOperand))
						return new MatrixForEachDefinition(ForEach, Start, this.pos - Start, this);
					else
						return new VectorForEachDefinition(ForEach, Start, this.pos - Start, this);
				}
				else if (Node is DoWhile DoWhile)
				{
					if (IsVectorDefinition(DoWhile.LeftOperand))
						return new MatrixDoWhileDefinition(DoWhile, Start, this.pos - Start, this);
					else
						return new VectorDoWhileDefinition(DoWhile, Start, this.pos - Start, this);
				}
				else if (Node is WhileDo WhileDo)
				{
					if (IsVectorDefinition(WhileDo.RightOperand))
						return new MatrixWhileDoDefinition(WhileDo, Start, this.pos - Start, this);
					else
						return new VectorWhileDoDefinition(WhileDo, Start, this.pos - Start, this);
				}
				else if (Node.GetType() == typeof(ElementList))
				{
					ElementList ElementList = (ElementList)Node;
					bool AllVectors = true;

					foreach (ScriptNode Element in ElementList.Elements)
					{
						if (!IsVectorDefinition(Element))
						{
							AllVectors = false;
							break;
						}
					}

					if (AllVectors)
						return new MatrixDefinition(((ElementList)Node).Elements, Start, this.pos - Start, this);
					else
						return new VectorDefinition(((ElementList)Node).Elements, Start, this.pos - Start, this);
				}
				else if (IsVectorDefinition(Node))
					return new MatrixDefinition(new ScriptNode[] { Node }, Start, this.pos - Start, this);
				else
					return new VectorDefinition(new ScriptNode[] { Node }, Start, this.pos - Start, this);
			}
			else if (ch == '{')
			{
				this.pos++;
				this.SkipWhiteSpace();
				if (this.PeekNextChar() == '}')
				{
					this.pos++;
					return new SetDefinition(new ScriptNode[0], Start, this.pos - Start, this);
				}

				Node = this.ParseStatement();

				this.SkipWhiteSpace();
				if ((ch = this.PeekNextChar()) == ':')
				{
					this.pos++;

					bool DoubleColon = false;

					if (this.PeekNextChar() == ':')
					{
						this.pos++;
						DoubleColon = true;
					}

					if (!DoubleColon && (Node is VariableReference || Node is ConstantElement))
					{
						LinkedList<KeyValuePair<string, ScriptNode>> Members = new LinkedList<KeyValuePair<string, ScriptNode>>();
						Dictionary<string, bool> MembersFound = new Dictionary<string, bool>();
						ConstantElement ConstantElement;
						StringValue StringValue;
						string s;

						if (Node is VariableReference)
							s = ((VariableReference)Node).VariableName;
						else if (!((ConstantElement = Node as ConstantElement) is null) &&
							!((StringValue = ConstantElement.Constant as StringValue) is null))
						{
							s = StringValue.Value;
						}
						else
							throw new SyntaxException("Expected a variable reference or a string constant.", this.pos, this.script);

						MembersFound[s] = true;
						Members.AddLast(new KeyValuePair<string, ScriptNode>(s, this.ParseLambdaExpression()));

						this.SkipWhiteSpace();
						while ((ch = this.PeekNextChar()) == ',')
						{
							this.pos++;
							Node = this.ParseStatement();

							this.SkipWhiteSpace();
							if (this.PeekNextChar() != ':')
								throw new SyntaxException("Expected :.", this.pos, this.script);

							if (Node is VariableReference)
								s = ((VariableReference)Node).VariableName;
							else if (!((ConstantElement = Node as ConstantElement) is null) &&
								!((StringValue = ConstantElement.Constant as StringValue) is null))
							{
								s = StringValue.Value;
							}
							else
								throw new SyntaxException("Expected a variable reference or a string constant.", this.pos, this.script);

							if (MembersFound.ContainsKey(s))
								throw new SyntaxException("Member already defined.", this.pos, this.script);

							this.pos++;
							MembersFound[s] = true;
							Members.AddLast(new KeyValuePair<string, ScriptNode>(s, this.ParseLambdaExpression()));

							this.SkipWhiteSpace();
						}

						if (ch != '}')
							throw new SyntaxException("Expected }.", this.pos, this.script);

						this.pos++;
						return new ObjectExNihilo(Members, Start, this.pos - Start, this);
					}

					ScriptNode Temp = this.ParseList();
					ScriptNode[] Conditions;
					ScriptNode SuperSet;

					if (Temp is ElementList List)
						Conditions = List.Elements;
					else
						Conditions = new ScriptNode[] { Temp };

					if (Node is In In && !(Node is NotIn))
					{
						SuperSet = In.RightOperand;
						Node = In.LeftOperand;
					}
					else
						SuperSet = null;

					this.SkipWhiteSpace();
					if (this.PeekNextChar()!='}')
						throw new SyntaxException("Expected }.", this.pos, this.script);

					this.pos++;

					return new ImplicitSetDefinition(Node, SuperSet, Conditions, DoubleColon, Start, this.pos - Start, this);
				}

				if (ch != '}')
					throw new SyntaxException("Expected }.", this.pos, this.script);

				this.pos++;

				if (Node is For)
					return new SetForDefinition((For)Node, Start, this.pos - Start, this);
				else if (Node is ForEach)
					return new SetForEachDefinition((ForEach)Node, Start, this.pos - Start, this);
				else if (Node is DoWhile)
					return new SetDoWhileDefinition((DoWhile)Node, Start, this.pos - Start, this);
				else if (Node is WhileDo)
					return new SetWhileDoDefinition((WhileDo)Node, Start, this.pos - Start, this);
				else if (Node.GetType() == typeof(ElementList))
					return new SetDefinition(((ElementList)Node).Elements, Start, this.pos - Start, this);
				else
					return new SetDefinition(new ScriptNode[] { Node }, Start, this.pos - Start, this);
			}
			else if ((ch >= '0' && ch <= '9') || ch == '.' || ch == '+' || ch == '-')
			{
				if (ch == '+' || ch == '-')
				{
					this.pos++;
					ch = this.PeekNextChar();
				}

				while (ch >= '0' && ch <= '9')
				{
					this.pos++;
					ch = this.PeekNextChar();
				}

				if (ch == '.')
				{
					this.pos++;
					ch = this.PeekNextChar();

					if (ch >= '0' && ch <= '9')
					{
						while (ch >= '0' && ch <= '9')
						{
							this.pos++;
							ch = this.PeekNextChar();
						}
					}
					else
					{
						this.pos--;
						ch = '.';
					}
				}

				if (char.ToUpper(ch) == 'E')
				{
					this.pos++;
					ch = this.PeekNextChar();

					if (ch == '+' || ch == '-')
					{
						this.pos++;
						ch = this.PeekNextChar();
					}

					while (ch >= '0' && ch <= '9')
					{
						this.pos++;
						ch = this.PeekNextChar();
					}
				}

				if (!double.TryParse(this.script.Substring(Start, this.pos - Start).
					Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out double d))
				{
					throw new SyntaxException("Invalid double number.", this.pos, this.script);
				}

				return new ConstantElement(new DoubleNumber(d), Start, this.pos - Start, this);
			}
			else if (ch == '"' || ch == '\'')
			{
				StringBuilder sb = new StringBuilder();
				char ch2;

				this.pos++;

				while ((ch2 = this.NextChar()) != ch)
				{
					if (ch2 == 0 || ch2 == '\r' || ch2 == '\n')
						throw new SyntaxException("Expected end of string.", this.pos, this.script);

					if (ch2 == '\\')
					{
						ch2 = this.NextChar();
						switch (ch2)
						{
							case (char)0:
								throw new SyntaxException("Expected end of string.", this.pos, this.script);

							case 'n':
								ch2 = '\n';
								break;

							case 'r':
								ch2 = '\r';
								break;

							case 't':
								ch2 = '\t';
								break;

							case 'b':
								ch2 = '\b';
								break;

							case 'f':
								ch2 = '\f';
								break;

							case 'a':
								ch2 = '\a';
								break;

							case 'v':
								ch2 = '\v';
								break;

							case 'x':
								ch2 = this.NextChar();
								if (ch2 >= '0' && ch2 <= '9')
									ch2 -= '0';
								else if (ch2 >= 'a' && ch2 <= 'f')
									ch2 -= (char)('a' - 10);
								else if (ch2 >= 'A' && ch2 <= 'F')
									ch2 -= (char)('A' - 10);
								else
									throw new SyntaxException("Hexadecimal digit expected.", this.pos, this.script);

								char ch3 = this.NextChar();
								if (ch3 >= '0' && ch3 <= '9')
									ch3 -= '0';
								else if (ch3 >= 'a' && ch3 <= 'f')
									ch3 -= (char)('a' - 10);
								else if (ch3 >= 'A' && ch3 <= 'F')
									ch3 -= (char)('A' - 10);
								else
									throw new SyntaxException("Hexadecimal digit expected.", this.pos, this.script);

								ch2 <<= 4;
								ch2 += ch3;
								break;
						}
					}

					sb.Append(ch2);
				}

				return new ConstantElement(new StringValue(sb.ToString()), Start, this.pos - Start, this);
			}
			else if (char.IsLetter(ch) || ch == '_')
			{
				this.pos++;

				if (ch == '_')
				{
					while ((ch = this.PeekNextChar()) == '_')
						this.pos++;

					if (!char.IsLetter(ch))
						throw new SyntaxException("Expected a letter.", this.pos, this.script);
				}

				while (char.IsLetter((ch = this.PeekNextChar())) || char.IsDigit(ch) || ch == '_')
					this.pos++;

				string s = this.script.Substring(Start, this.pos - Start);

				switch (s.ToUpper())
				{
					case "TRUE":
						return new ConstantElement(BooleanValue.True, Start, this.pos - Start, this);

					case "FALSE":
						return new ConstantElement(BooleanValue.False, Start, this.pos - Start, this);

					case "NULL":
						return new ConstantElement(ObjectValue.Null, Start, this.pos - Start, this);

					default:
						if (customKeyWords is null)
							Search();

						if (customKeyWords.TryGetValue(s, out IKeyWord KeyWord))
						{
							ScriptParser Parser = new ScriptParser(this);
							int PosBak = this.pos;
							bool CanParseWhitespace = this.canSkipWhitespace;
							bool Result = KeyWord.TryParse(Parser, out Node);

							this.canSkipWhitespace = CanParseWhitespace;

							if (Result)
								return Node;
							else
								this.pos = PosBak;
						}

						return new VariableReference(s, Start, this.pos - Start, this);
				}
			}
			else
			{
				switch (ch)
				{
					case '∅':
					case '∞':
						this.pos++;
						return new VariableReference(new string(ch, 1), Start, this.pos - Start, this);

					case '⊤':
						this.pos++;
						return new ConstantElement(BooleanValue.True, Start, this.pos - Start, this);

					case '⊥':
						this.pos++;
						return new ConstantElement(BooleanValue.False, Start, this.pos - Start, this);
				}

				return null;
			}
		}

		private static bool IsVectorDefinition(ScriptNode Node)
		{
			return Node is VectorDefinition ||
				Node is VectorForDefinition ||
				Node is VectorForEachDefinition ||
				Node is VectorDoWhileDefinition ||
				Node is VectorWhileDoDefinition;
		}

		/// <summary>
		/// Evaluates the expression, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public object Evaluate(Variables Variables)
		{
			IElement Result;

			try
			{
				Result = this.root.Evaluate(Variables);
			}
			catch (ScriptReturnValueException ex)
			{
				Result = ex.ReturnValue;
			}

			return Result.AssociatedObjectValue;
		}

		/// <summary>
		/// Root script node.
		/// </summary>
		public ScriptNode Root
		{
			get { return this.root; }
		}

		/// <summary>
		/// <see cref="Object.Equals(Object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is Expression Exp)
				return this.script.Equals(Exp.script);
			else
				return false;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.script.GetHashCode();
		}

		/// <summary>
		/// If the expression contains implicit print operations.
		/// </summary>
		public bool ContainsImplicitPrint
		{
			get { return this.containsImplicitPrint; }
		}

		/// <summary>
		/// Transforms a string by executing embedded script.
		/// </summary>
		/// <param name="s">String to transform.</param>
		/// <param name="StartDelimiter">Start delimiter.</param>
		/// <param name="StopDelimiter">Stop delimiter.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Transformed string.</returns>
		public static string Transform(string s, string StartDelimiter, string StopDelimiter, Variables Variables)
		{
			Expression Exp;
			string Script, s2;
			object Result;
			int i = s.IndexOf(StartDelimiter);
			int j;
			int StartLen = StartDelimiter.Length;
			int StopLen = StopDelimiter.Length;

			while (i >= 0)
			{
				j = s.IndexOf(StopDelimiter, i + StartLen);
				if (j < 0)
					break;

				Script = s.Substring(i + StartLen, j - i - StartLen);
				s = s.Remove(i, j - i + StopLen);

				Exp = new Expression(Script);
				Result = Exp.Evaluate(Variables);

				if (!(Result is null))
				{
					s2 = Result.ToString();
					s = s.Insert(i, s2);
					i += s2.Length;
				}

				i = s.IndexOf(StartDelimiter, i);
			}

			return s;
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(double Value)
		{
			return Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(decimal Value)
		{
			return Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(Complex Value)
		{
			return "(" + ToString(Value.Real) + "," + ToString(Value.Imaginary) + ")";
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(bool Value)
		{
			return Value ? "⊤" : "⊥";
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(double[] Value)
		{
			StringBuilder sb = null;

			foreach (double d in Value)
			{
				if (sb is null)
					sb = new StringBuilder("[");
				else
					sb.Append(", ");

				sb.Append(ToString(d));
			}

			if (sb is null)
				return "[]";
			else
			{
				sb.Append(']');
				return sb.ToString();
			}
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(Complex[] Value)
		{
			StringBuilder sb = null;

			foreach (Complex z in Value)
			{
				if (sb is null)
					sb = new StringBuilder("[");
				else
					sb.Append(", ");

				sb.Append(Expression.ToString(z));
			}

			if (sb is null)
				return "[]";
			else
			{
				sb.Append(']');
				return sb.ToString();
			}
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(DateTime Value)
		{
			StringBuilder Output = new StringBuilder();

			Output.Append("DateTime(");
			Output.Append(Value.Year.ToString("D4"));
			Output.Append(',');
			Output.Append(Value.Month.ToString("D2"));
			Output.Append(',');
			Output.Append(Value.Day.ToString("D2"));

			if (Value.Hour != 0 || Value.Minute != 0 || Value.Second != 0 || Value.Millisecond != 0)
			{
				Output.Append(',');
				Output.Append(Value.Hour.ToString("D2"));
				Output.Append(',');
				Output.Append(Value.Minute.ToString("D2"));
				Output.Append(',');
				Output.Append(Value.Second.ToString("D2"));

				if (Value.Millisecond != 0)
				{
					Output.Append(',');
					Output.Append(Value.Millisecond.ToString("D3"));
				}
			}

			Output.Append(')');

			return Output.ToString();
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(TimeSpan Value)
		{
			StringBuilder Output = new StringBuilder();

			Output.Append("TimeSpan(");
			Output.Append(Value.Days.ToString());
			Output.Append(',');
			Output.Append(Value.Hours.ToString("D2"));
			Output.Append(',');
			Output.Append(Value.Minutes.ToString("D2"));
			Output.Append(',');
			Output.Append(Value.Seconds.ToString("D2"));

			if (Value.Milliseconds != 0)
			{
				Output.Append(',');
				Output.Append(Value.Milliseconds.ToString("D3"));
			}

			Output.Append(')');

			return Output.ToString();
		}

		/// <summary>
		/// Converts a string value to a parsable expression string.
		/// </summary>
		/// <param name="s">Value</param>
		/// <returns>Expression representation of string.</returns>
		public static string ToString(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i = s.IndexOfAny(stringCharactersToEscape);
			int j = 0;
			int k;

			sb.Append('"');

			if (i < 0)
				sb.Append(s);
			else
			{
				while (i >= 0)
				{
					if (i > j)
						sb.Append(s.Substring(j, i - j));

					k = Array.IndexOf<char>(stringCharactersToEscape, s[i]);
					sb.Append(stringEscapeSequences[k]);
					j = i + 1;
					i = s.IndexOfAny(stringCharactersToEscape, j);
				}

				if (j < s.Length)
					sb.Append(s.Substring(j));
			}

			sb.Append('"');

			return sb.ToString();
		}

		private static readonly char[] stringCharactersToEscape = new char[] { '\\', '"', '\n', '\r', '\t', '\b', '\f', '\a' };
		private static readonly string[] stringEscapeSequences = new string[] { "\\\\", "\\\"", "\\n", "\\r", "\\t", "\\b", "\\f", "\\a" };

		/// <summary>
		/// Converts an object to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(object Value)
		{
			if (Value is double dbl)
				return ToString(dbl);
			else if (Value is decimal dec)
				return ToString(dec);
			else if (Value is Complex z)
				return ToString(z);
			else if (Value is bool b)
				return ToString(b);
			else if (Value is double[] dblA)
				return ToString(dblA);
			else if (Value is Complex[] zA)
				return ToString(zA);
			else if (Value is TimeSpan TS)
				return ToString(TS);
			else if (Value is DateTime DT)
				return ToString(DT);
			else if (Value is string s)
				return ToString(s);
			else if (Value is Exception ex)
				return ToString(ex.Message);
			else if (Value is Dictionary<string, IElement> ObjExNihilo)
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				sb.Append('{');

				foreach (KeyValuePair<string, IElement> P in ObjExNihilo)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					sb.Append(P.Key);
					sb.Append(':');
					sb.Append(P.Value.ToString());
				}

				sb.Append('}');

				return sb.ToString();
			}
			else if (Value is IEnumerable Enumerable)
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				sb.Append('[');

				foreach (object Element in Enumerable)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					sb.Append(ToString(Element));
				}

				sb.Append(']');

				return sb.ToString();
			}
			else if (Value is null)
				return "null";
			else
				return Value.ToString();
		}

		/// <summary>
		/// Converts an object to a double value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Double value.</returns>
		public static double ToDouble(object Object)
		{
			if (Object is double)
				return (double)Object;
			else if (Object is int)
				return (int)Object;
			else
			{
				if (Object is bool b)
					return b ? 1 : 0;
				else if (Object is byte bt)
					return bt;
				else if (Object is char ch)
					return ch;
				else if (Object is decimal dc)
					return (double)dc;
				else if (Object is double db)
					return db;
				else if (Object is short sh)
					return sh;
				else if (Object is int i)
					return i;
				else if (Object is long l)
					return l;
				else if (Object is sbyte sb)
					return sb;
				else if (Object is float f)
					return f;
				else if (Object is ushort us)
					return us;
				else if (Object is uint ui)
					return ui;
				else if (Object is ulong ul)
					return ul;
				else
				{
					string s = Object.ToString();

					if (double.TryParse(s, out double d))
						return d;

					if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator != "." &&
						double.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out d))
					{
						return d;
					}

					throw new ScriptException("Expected a double value.");
				}
			}
		}

		/// <summary>
		/// Converts an object to a complex value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Complex value.</returns>
		public static Complex ToComplex(object Object)
		{
			if (Object is Complex)
				return (Complex)Object;
			else
				return new Complex(ToDouble(Object), 0);
		}

		/// <summary>
		/// Encapsulates an object.
		/// </summary>
		/// <param name="Value">Object</param>
		/// <returns>Encapsulated object.</returns>
		public static IElement Encapsulate(object Value)
		{
			if (Value is null)
				return ObjectValue.Null;
			else if (Value is double db)
				return new DoubleNumber(db);
			else if (Value is bool b)
				return new BooleanValue(b);
			else if (Value is string s)
				return new StringValue(s);
			else if (Value is int i)
				return new DoubleNumber(i);
			else if (Value is long l)
				return new DoubleNumber(l);
			else if (Value is byte bt)
				return new DoubleNumber(bt);
			else if (Value is char ch)
				return new StringValue(new string(ch, 1));
			else if (Value is DateTime DT)
				return new DateTimeValue(DT);
			else if (Value is decimal dc)
				return new DoubleNumber((double)dc);
			else if (Value is short sh)
				return new DoubleNumber(sh);
			else if (Value is sbyte sb)
				return new DoubleNumber(sb);
			else if (Value is float f)
				return new DoubleNumber(f);
			else if (Value is ushort us)
				return new DoubleNumber(us);
			else if (Value is uint ui)
				return new DoubleNumber(ui);
			else if (Value is ulong ul)
				return new DoubleNumber(ul);
			else
			{
				if (Value is IElement e)
					return e;

				else if (Value is double[] dv)
					return new DoubleVector(dv);
				else if (Value is double[,] dm)
					return new DoubleMatrix(dm);

				else if (Value is Complex c)
					return new ComplexNumber(c);
				else if (Value is Complex[] cv)
					return new ComplexVector(cv);
				else if (Value is Complex[,] cm)
					return new ComplexMatrix(cm);

				else if (Value is bool[] bv)
					return new BooleanVector(bv);
				else if (Value is bool[,] bm)
					return new BooleanMatrix(bm);

				else if (Value is DateTime[] dv2)
					return new DateTimeVector(dv2);

				else if (Value is IElement[] ev)
					return new ObjectVector((ICollection<IElement>)ev);
				else if (Value is IElement[,] em)
					return new ObjectMatrix(em);
				else if (Value is object[] ov)
					return new ObjectVector(ov);
				else if (Value is object[,] om)
					return new ObjectMatrix(om);

				else if (Value is Type t)
					return new TypeValue(t);
				else
					return new ObjectValue(Value);
			}
		}

		/// <summary>
		/// Upgrades elements if necessary, to a common semi-field, trying to make them compatible.
		/// </summary>
		/// <param name="E1">Element 1.</param>
		/// <param name="Set1">Set containing element 1.</param>
		/// <param name="E2">Element 2.</param>
		/// <param name="Set2">Set containing element 2.</param>
		/// <param name="Node">Script node requesting the upgrade.</param>
		/// <returns>If elements have been upgraded to become compatible.</returns>
		public static bool UpgradeSemiGroup(ref IElement E1, ref ISet Set1, ref IElement E2, ref ISet Set2, ScriptNode Node)
		{
			// TODO: Implement pluggable upgrades and a shortest path search to find optimal upgrades.

			if (UpgradeField(ref E1, ref Set1, ref E2, ref Set2, Node))
				return true;
			else if (E1 is StringValue)
			{
				E2 = new StringValue(E2.ToString());
				Set2 = StringValues.Instance;
				return true;
			}
			else if (E2 is StringValue)
			{
				E1 = new StringValue(E1.ToString());
				Set1 = StringValues.Instance;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Upgrades elements if necessary, to a common field extension, trying to make them compatible.
		/// </summary>
		/// <param name="E1">Element 1.</param>
		/// <param name="Set1">Set containing element 1.</param>
		/// <param name="E2">Element 2.</param>
		/// <param name="Set2">Set containing element 2.</param>
		/// <param name="Node">Script node requesting the upgrade.</param>
		/// <returns>If elements have been upgraded to become compatible.</returns>
		public static bool UpgradeField(ref IElement E1, ref ISet Set1, ref IElement E2, ref ISet Set2, ScriptNode Node)
		{
			// TODO: Implement pluggable upgrades and a shortest path search to find optimal upgrades.

			if (E1 is ComplexNumber)
			{
				if (E2 is DoubleNumber D2)
				{
					E2 = new ComplexNumber(D2.Value);
					Set2 = ComplexNumbers.Instance;
					return true;
				}
			}
			else if (E2 is ComplexNumber)
			{
				if (E1 is DoubleNumber D1)
				{
					E1 = new ComplexNumber(D1.Value);
					Set1 = ComplexNumbers.Instance;
					return true;
				}
			}
			else if (E1 is ObjectValue O1 && O1.AssociatedObjectValue is Enum Enum1 && E2 is DoubleNumber)
			{
				Type T1 = Enum.GetUnderlyingType(Enum1.GetType());
				if (T1 == typeof(int))
				{
					E1 = new DoubleNumber(Convert.ToInt32(Enum1));
					Set1 = DoubleNumbers.Instance;
					return true;
				}
			}
			else if (E2 is ObjectValue O2 && O2.AssociatedObjectValue is Enum Enum2 && E1 is DoubleNumber)
			{
				Type T2 = Enum.GetUnderlyingType(Enum2.GetType());
				if (T2 == typeof(int))
				{
					E2 = new DoubleNumber(Convert.ToInt32(Enum2));
					Set2 = DoubleNumbers.Instance;
					return true;
				}
			}

			return false;   // TODO: Implement Upgrade()
		}

		/// <summary>
		/// Tries to conevert an element value to a desired type.
		/// </summary>
		/// <param name="Value">Element value.</param>
		/// <param name="DesiredType">Desired type.</param>
		/// <param name="Node">Script node making the request.</param>
		/// <returns>Converted value.</returns>
		public static object ConvertTo(IElement Value, Type DesiredType, ScriptNode Node)
		{
			return Value.AssociatedObjectValue;    // TODO: Implement .NET type conversion.
		}

		/// <summary>
		/// Reports a preview of the final result.
		/// </summary>
		/// <param name="Result">Preview</param>
		public void Preview(IElement Result)
		{
			PreviewEventHandler h = this.OnPreview;
			if (!(h is null))
			{
				try
				{
					h(this, new PreviewEventArgs(this, Result));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// If previews are desired.
		/// </summary>
		public bool HandlesPreview
		{
			get { return !(this.OnPreview is null); }
		}

		/// <summary>
		/// Event raised when a preview of the final result has been reported.
		/// </summary>
		public event PreviewEventHandler OnPreview = null;

		/// <summary>
		/// Reports current status of execution.
		/// </summary>
		/// <param name="Result">Status Message</param>
		public void Status(string Result)
		{
			StatusEventHandler h = this.OnStatus;
			if (!(h is null))
			{
				try
				{
					h(this, new StatusEventArgs(this, Result));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// If status messages are desired.
		/// </summary>
		public bool HandlesStatus
		{
			get { return !(this.OnStatus is null); }
		}

		/// <summary>
		/// Event raised when a status message has been reported.
		/// </summary>
		public event StatusEventHandler OnStatus = null;

		/// <summary>
		/// This property allows the caller to tag the expression with an arbitrary object.
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		/// <summary>
		/// Calls the callback method for all script nodes defined for the expression.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAll(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!this.root.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			if (!Callback(ref this.root, State))
				return false;

			if (!DepthFirst)
			{
				if (!this.root.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to convert an object <paramref name="Value"/> to an object of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Desired type.</typeparam>
		/// <param name="Value">Value to convert.</param>
		/// <param name="Result">Conversion result.</param>
		/// <returns>If conversion was successful.</returns>
		public static bool TryConvert<T>(object Value, out T Result)
		{
			if (!TryConvert(Value, typeof(T), out object Obj) || !(Obj is T Result2))
			{
				Result = default(T);
				return false;
			}

			Result = Result2;
			return true;
		}

		/// <summary>
		/// Tries to convert an object <paramref name="Value"/> to an object of type <paramref name="DesiredType"/>.
		/// </summary>
		/// <param name="Value">Value to convert.</param>
		/// <param name="DesiredType">Desired type.</param>
		/// <param name="Result">Conversion result.</param>
		/// <returns>If conversion was successful.</returns>
		public static bool TryConvert(object Value, Type DesiredType, out object Result)
		{
			Type T = Value.GetType();
			TypeInfo TI = T.GetTypeInfo();

			if (DesiredType.GetTypeInfo().IsAssignableFrom(TI))
			{
				Result = Value;
				return true;
			}

			if (converters is null)
			{
				Dictionary<Type, Dictionary<Type, ITypeConverter>> Converters = GetTypeConverters();

				if (converters is null)
				{
					converters = Converters;
					Types.OnInvalidated += (sender, e) => converters = GetTypeConverters();
				}
			}

			lock (converters)
			{
				if (!converters.TryGetValue(T, out Dictionary<Type, ITypeConverter> Converters))
				{
					Result = null;
					return false;
				}

				if (Converters.TryGetValue(DesiredType, out ITypeConverter Converter))
				{
					Result = Converter?.Convert(Value);
					return !(Converter is null);
				}

				Dictionary<Type, bool> Explored = new Dictionary<Type, bool>() { { T, true } };
				LinkedList<ITypeConverter> Search = new LinkedList<ITypeConverter>();

				foreach (ITypeConverter Converter3 in Converters.Values)
				{
					Search.AddLast(Converter3);
					Explored[Converter3.To] = true;
				}

				while (!(Search.First is null))
				{
					ITypeConverter C = Search.First.Value;
					Search.RemoveFirst();

					if (converters.TryGetValue(C.To, out Dictionary<Type, ITypeConverter> Converters2))
					{
						if (Converters2.TryGetValue(DesiredType, out ITypeConverter Converter2))
						{
							ConversionSequence ConversionSequence;

							if (C is ConversionSequence Sequence)
							{
								int c = Sequence.Converters.Length + 1;
								ITypeConverter[] A = new ITypeConverter[c];
								Sequence.Converters.CopyTo(A, 0);
								A[c - 1] = Converter2;

								ConversionSequence = new ConversionSequence(A);
							}
							else
								ConversionSequence = new ConversionSequence(C, Converter2);

							Converters[DesiredType] = ConversionSequence;
							Result = ConversionSequence.Convert(Value);
							return true;
						}

						foreach (ITypeConverter Converter3 in Converters2.Values)
						{
							if (!Explored.ContainsKey(Converter3.To))
							{
								Search.AddLast(Converter3);
								Explored[Converter3.To] = true;
							}
						}
					}
				}

				Converters[DesiredType] = null;
				Result = null;
				return false;
			}
		}

		private static Dictionary<Type, Dictionary<Type, ITypeConverter>> GetTypeConverters()
		{
			Dictionary<Type, Dictionary<Type, ITypeConverter>> Converters = new Dictionary<Type, Dictionary<Type, ITypeConverter>>();

			foreach (Type T2 in Types.GetTypesImplementingInterface(typeof(ITypeConverter)))
			{
				TypeInfo TI2 = T2.GetTypeInfo();
				if (TI2.IsAbstract)
					continue;

				ConstructorInfo DefaultConstructor = null;

				foreach (ConstructorInfo CI in TI2.DeclaredConstructors)
				{
					if (CI.GetParameters().Length == 0)
					{
						DefaultConstructor = CI;
						break;
					}
				}

				if (DefaultConstructor is null)
					continue;

				try
				{
					ITypeConverter Converter = (ITypeConverter)DefaultConstructor.Invoke(Types.NoParameters);
					Type From = Converter.From;
					Type To = Converter.To;

					if (!Converters.TryGetValue(From, out Dictionary<Type, ITypeConverter> List))
					{
						List = new Dictionary<Type, ITypeConverter>();
						Converters[From] = List;
					}

					if (List.TryGetValue(To, out ITypeConverter Converter2))
					{
						Log.Warning("There's already a type converter registered converting from " +
							From.FullName + " to " + To.FullName, Converter2.GetType().FullName);
					}
					else
						List[To] = Converter;
				}
				catch (Exception ex)
				{
					Log.Critical(ex, T2.FullName);
				}
			}

			return Converters;
		}

		// TODO: Optimize constants
		// TODO: Implicit sets with conditions. {x:x in Z}, {x in Z: x>10}, {[a,b]: a>b}
		// TODO: Integers (0d, 0x, 0o, 0b), Big Integers (0D, 0X, 0O, 0B)
		// TODO: Upgrade
		// TODO: Matrix*Vector = Vector
		// TODO: Vector*Matrix = Vector
		// TODO: Matrix\Vector = Solutionvector.
		// TODO: Call member method.
		/*
			Covariance
			Correlation

			Linear Algebra:

			Determinant
			Columns
			Rows
			Diagonal
			Eliminate
			FlipLeftRight
			FlipUpDown
			Identity
			IsDiagonal
			IsLowerTriangular
			IsNullMatrix
			IsUpperTriangular
			LookUp
			Ones
			Zeroes
			Rank
			Reduce
			Regression
			Slope
			Trace

			Statistics
			Security
			Polynomials
			Probability

			Strings:

			EndsWith
			StartsWith
			Transform

			Vectors:
			Axis
			Count
			First
			IndexOf
			Join
			Last
			Norm
			Normalize
			Order
			Permutate
			Reverse
			Slice
			Sort

		*/
	}
}
