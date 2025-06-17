﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Collections;
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
using Waher.Script.Output;
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
		private static readonly Dictionary<Type, ICustomStringOutput> output = new Dictionary<Type, ICustomStringOutput>();
		internal static readonly Dictionary<string, bool> keywords = GetKeywords();

		private ScriptNode root;
		private readonly string script;
		private readonly string source;
		private object tag;
		private int pos;
		private readonly int len;
		private bool containsImplicitPrint = false;
		private bool canSkipWhitespace = true;

		/// <summary>
		/// Class managing a script expression.
		/// </summary>
		/// <param name="Script">Script expression.</param>
		/// <param name="Source">Source of script.</param>
		public Expression(string Script, string Source)
			: this(Script)
		{
			this.source = Source;
		}

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
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			functions = null;
			constants = null;

			lock (output)
			{
				output.Clear();
			}
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
				{ "INHERITS", true },
				{ "INTERSECT", true },
				{ "INTERSECTION", true },
				{ "IS", true },
				{ "LIKE", true },
				{ "MATCHES", true },
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

		internal bool EndOfScript => this.pos >= this.len;
		internal bool InScript => this.pos < this.len;

		internal bool CanSkipWhitespace
		{
			get => this.canSkipWhitespace;
			set => this.canSkipWhitespace = value;
		}

		/// <summary>
		/// Original script string.
		/// </summary>
		public string Script => this.script;

		/// <summary>
		/// Source of script, or null if not defined.
		/// </summary>
		public string Source => this.source;

		internal char NextChar()
		{
			if (this.pos < this.len)
				return this.script[this.pos++];
			else
				return (char)0;
		}

		internal void UndoChar()
		{
			if (this.pos > 0)
				this.pos--;
		}

		internal char PeekNextChar()
		{
			if (this.pos < this.len)
				return this.script[this.pos];
			else
				return (char)0;
		}

		internal string PeekNextChars(int NrChars)
		{
			if (this.pos + NrChars > this.len)
				NrChars = this.len - this.pos;

			if (NrChars <= 0)
				return string.Empty;

			return this.script.Substring(this.pos, NrChars);
		}

		internal bool IsNextChars(string Token)
		{
			int c = Token.Length;
			if (c == 0)
				return true;

			if (this.pos + c > this.len)
				return false;

			int i;

			for (i = 0; i < c; i++)
			{
				if (this.script[this.pos + i] != Token[i])
					return false;
			}

			return true;
		}

		internal bool IsNextChars(char ch, int Count)
		{
			if (Count < 0)
				return false;

			if (this.pos + Count > this.len)
				return false;

			int i;

			for (i = 0; i < Count; i++)
			{
				if (this.script[this.pos + i] != ch)
					return false;
			}

			return true;
		}

		internal void SkipChars(int NrChars)
		{
			this.pos += NrChars;
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
			ScriptNode Node = this.ParseStatement(true);
			this.SkipWhiteSpace();

			if (Node is null)
			{
				while (Node is null && this.PeekNextChar() == ';')
				{
					this.pos++;
					Node = this.ParseStatement(true);
					this.SkipWhiteSpace();
				}
			}

			if (Node is null)
				return null;

			int Start = Node.Start;

			if (!(Node is null) && this.PeekNextChar() == ';')
			{
				this.pos++;
				ScriptNode Node2 = this.ParseStatement(true);
				if (!(Node2 is null))
				{
					ChunkedList<ScriptNode> Statements = new ChunkedList<ScriptNode>
					{
						Node,
						Node2
					};

					this.SkipWhiteSpace();
					while (this.PeekNextChar() == ';')
					{
						this.pos++;
						Node2 = this.ParseStatement(true);
						if (Node2 is null)
							break;

						Statements.Add(Node2);
						this.SkipWhiteSpace();
					}

					Node = new Sequence(Statements, Start, this.pos - Start, this);
				}
			}

			return Node;
		}

		internal ScriptNode ParseStatement(bool ParseLists)
		{
			this.SkipWhiteSpace();

			int Start = this.pos;

			switch (char.ToUpper(this.PeekNextChar()))
			{
				case 'D':
					if (string.Compare(this.PeekNextToken(), "DO", true) == 0)
					{
						this.pos += 2;

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement(false));

						this.SkipWhiteSpace();
						if (string.Compare(this.PeekNextToken(), "WHILE", true) != 0)
							throw new SyntaxException("Expected WHILE.", this.pos, this.script);

						this.pos += 5;

						ScriptNode Condition = this.AssertOperandNotNull(this.ParseIf());

						return new DoWhile(Statement, Condition, Start, this.pos - Start, this);
					}
					else
						return ParseLists ? this.ParseList() : this.ParseIf();

				case 'W':
					if (string.Compare(this.PeekNextToken(), "WHILE", true) == 0)
					{
						this.pos += 5;

						ScriptNode Condition = this.AssertOperandNotNull(this.ParseIf());

						this.SkipWhiteSpace();
						if (this.PeekNextChar() == ':')
							this.pos++;
						else if (string.Compare(this.PeekNextToken(), "DO", true) == 0)
							this.pos += 2;
						else
							throw new SyntaxException("DO or : expected.", this.pos, this.script);

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement(false));

						return new WhileDo(Condition, Statement, Start, this.pos - Start, this);
					}
					else
						return ParseLists ? this.ParseList() : this.ParseIf();

				case 'F':
					switch (this.PeekNextToken().ToUpper())
					{
						case "FOREACH":
							this.pos += 7;
							if (!(this.AssertOperandNotNull(this.ParseIf()) is In In))
								throw new SyntaxException("IN statement expected", this.pos, this.script);

							VariableReference Ref = In.LeftOperand as VariableReference;
							if (Ref is null)
								throw new SyntaxException("Variable reference expected", In.LeftOperand.Start, this.script);

							this.SkipWhiteSpace();
							if (this.PeekNextChar() == ':')
								this.pos++;
							else if (string.Compare(this.PeekNextToken(), "DO", true) == 0)
								this.pos += 2;
							else
								throw new SyntaxException("DO or : expected.", this.pos, this.script);

							ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement(false));

							return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start, this);

						case "FOR":
							this.pos += 3;
							this.SkipWhiteSpace();

							if (string.Compare(this.PeekNextToken(), "EACH", true) == 0)
							{
								this.pos += 4;
								In = this.AssertOperandNotNull(this.ParseIf()) as In;
								if (In is null)
									throw new SyntaxException("IN statement expected", this.pos, this.script);

								Ref = In.LeftOperand as VariableReference;
								if (Ref is null)
									throw new SyntaxException("Variable reference expected", In.LeftOperand.Start, this.script);

								this.SkipWhiteSpace();
								if (this.PeekNextChar() == ':')
									this.pos++;
								else if (string.Compare(this.PeekNextToken(), "DO", true) == 0)
									this.pos += 2;
								else
									throw new SyntaxException("DO or : expected.", this.pos, this.script);

								Statement = this.AssertOperandNotNull(this.ParseStatement(false));

								return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start, this);
							}
							else
							{
								if (!(this.AssertOperandNotNull(this.ParseIf()) is Assignment Assignment))
									throw new SyntaxException("Assignment expected", this.pos, this.script);

								this.SkipWhiteSpace();
								if (string.Compare(this.PeekNextToken(), "TO", true) != 0)
									throw new SyntaxException("Expected TO.", this.pos, this.script);

								this.pos += 2;

								ScriptNode To = this.AssertOperandNotNull(this.ParseIf());
								ScriptNode Step;

								this.SkipWhiteSpace();
								if (string.Compare(this.PeekNextToken(), "STEP", true) == 0)
								{
									this.pos += 4;
									Step = this.AssertOperandNotNull(this.ParseIf());
								}
								else
									Step = null;

								this.SkipWhiteSpace();
								if (this.PeekNextChar() == ':')
									this.pos++;
								else if (string.Compare(this.PeekNextToken(), "DO", true) == 0)
									this.pos += 2;
								else
									throw new SyntaxException("DO or : expected.", this.pos, this.script);

								Statement = this.AssertOperandNotNull(this.ParseStatement(false));

								return new For(Assignment.VariableName, Assignment.Operand, To, Step, Statement, Start, this.pos - Start, this);
							}

						default:
							return ParseLists ? this.ParseList() : this.ParseIf();
					}

				case 'T':
					if (string.Compare(this.PeekNextToken(), "TRY", true) == 0)
					{
						this.pos += 3;

						ScriptNode Statement = this.AssertOperandNotNull(this.ParseStatement(false));

						this.SkipWhiteSpace();
						switch (this.PeekNextToken().ToUpper())
						{
							case "FINALLY":
								this.pos += 7;
								ScriptNode Finally = this.AssertOperandNotNull(this.ParseStatement(false));
								return new TryFinally(Statement, Finally, Start, this.pos - Start, this);

							case "CATCH":
								this.pos += 5;
								ScriptNode Catch = this.AssertOperandNotNull(this.ParseStatement(false));

								this.SkipWhiteSpace();
								if (string.Compare(this.PeekNextToken(), "FINALLY", true) == 0)
								{
									this.pos += 7;
									Finally = this.AssertOperandNotNull(this.ParseStatement(false));
									return new TryCatchFinally(Statement, Catch, Finally, Start, this.pos - Start, this);
								}
								else
									return new TryCatch(Statement, Catch, Start, this.pos - Start, this);

							default:
								throw new SyntaxException("Expected CATCH or FINALLY.", this.pos, this.script);
						}
					}
					else
						return ParseLists ? this.ParseList() : this.ParseIf();

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
						return ParseLists ? this.ParseList() : this.ParseIf();
					}

				default:
					return ParseLists ? this.ParseList() : this.ParseIf();
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
				ChunkedList<ScriptNode> Elements = new ChunkedList<ScriptNode>()
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

			if (char.ToUpper(this.PeekNextChar()) == 'I' && string.Compare(this.PeekNextToken(), "IF", true) == 0)
			{
				this.pos += 2;
				this.SkipWhiteSpace();

				Condition = this.AssertOperandNotNull(this.ParseAssignments());

				this.SkipWhiteSpace();
				if (string.Compare(this.PeekNextToken(), "THEN", true) == 0)
					this.pos += 4;
				else
					throw new SyntaxException("THEN expected.", this.pos, this.script);

				IfTrue = this.AssertOperandNotNull(this.ParseStatement(false));

				this.SkipWhiteSpace();
				if (string.Compare(this.PeekNextToken(), "ELSE", true) == 0)
				{
					this.pos += 4;
					IfFalse = this.AssertOperandNotNull(this.ParseStatement(false));
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
						if (this.PeekNextChar() == '?')
						{
							this.pos++;
							IfTrue = this.AssertOperandNotNull(this.ParseStatement(false));
							return new TryCatch(Condition, IfTrue, Start, this.pos - Start, this);
						}
						else
						{
							IfTrue = this.AssertOperandNotNull(this.ParseStatement(false));
							return new NullCheck(Condition, IfTrue, Start, this.pos - Start, this);
						}

					default:
						IfTrue = this.AssertOperandNotNull(this.ParseStatement(false));

						this.SkipWhiteSpace();
						if (this.PeekNextChar() == ':')
						{
							this.pos++;
							IfFalse = this.AssertOperandNotNull(this.ParseStatement(false));
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
						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Assignment(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, Right, Start, this.pos - Start, this);
						else if (Left is DynamicMember DynamicMember)
							return new DynamicMemberAssignment(DynamicMember, Right, Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, Right, Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, Right, Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, Right, Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, Right, Start, this.pos - Start, this);
						else if (Left is DynamicIndex DynamicIndex)
							return new DynamicIndexAssignment(DynamicIndex, Right, Start, this.pos - Start, this);
						else if (Left is NamedFunctionCall f)
						{
							ChunkedList<string> ArgumentNames = new ChunkedList<string>();
							ChunkedList<ArgumentType> ArgumentTypes = new ChunkedList<ArgumentType>();
							ArgumentType ArgumentType;

							foreach (ScriptNode Argument in f.Arguments)
							{
								if (Argument is ToVector ToVector)
								{
									ArgumentType = ArgumentType.Vector;

									if ((Ref = ToVector.Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is ToMatrix ToMatrix)
								{
									ArgumentType = ArgumentType.Matrix;

									if ((Ref = ToMatrix.Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is ToSet ToSet)
								{
									ArgumentType = ArgumentType.Set;

									if ((Ref = ToSet.Operand as VariableReference) is null)
									{
										throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
											Argument.Start, this.script);
									}
								}
								else if (Argument is VectorDefinition Def)
								{
									ArgumentType = ArgumentType.Scalar;

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

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Operators.Assignments.WithSelf.AddToSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new Add(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new Add(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new Add(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new Add(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new Add(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the += operator.", this.pos, this.script);
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

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Operators.Assignments.WithSelf.SubtractFromSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new Subtract(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new Subtract(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new Subtract(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new Subtract(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new Subtract(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the -= operator.", this.pos, this.script);
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

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Operators.Assignments.WithSelf.MultiplyWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new Multiply(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new Multiply(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new Multiply(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new Multiply(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new Multiply(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the *= operator.", this.pos, this.script);
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

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Operators.Assignments.WithSelf.DivideFromSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new Divide(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new Divide(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new Divide(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new Divide(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new Divide(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the /= operator.", this.pos, this.script);
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

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

						if (!(Ref is null))
							return new Operators.Assignments.WithSelf.PowerOfSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
						else if (Left is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new Power(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new Power(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new Power(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new Power(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Left is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new Power(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the ^= operator.", this.pos, this.script);
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

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

							if (!(Ref is null))
								return new Operators.Assignments.WithSelf.BinaryAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
							else if (Left is NamedMember NamedMember)
								return new NamedMemberAssignment(NamedMember, new Operators.Binary.And(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is VectorIndex VectorIndex)
								return new VectorIndexAssignment(VectorIndex, new Operators.Binary.And(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is MatrixIndex MatrixIndex)
								return new MatrixIndexAssignment(MatrixIndex, new Operators.Binary.And(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is ColumnVector ColumnVector)
								return new MatrixColumnAssignment(ColumnVector, new Operators.Binary.And(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is RowVector RowVector)
								return new MatrixRowAssignment(RowVector, new Operators.Binary.And(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else
								throw new SyntaxException("Invalid use of the &= operator.", this.pos, this.script);

						case '&':
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos++;

								Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

								if (!(Ref is null))
									return new Operators.Assignments.WithSelf.LogicalAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
								else if (Left is NamedMember NamedMember)
									return new NamedMemberAssignment(NamedMember, new Operators.Logical.And(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is VectorIndex VectorIndex)
									return new VectorIndexAssignment(VectorIndex, new Operators.Logical.And(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is MatrixIndex MatrixIndex)
									return new MatrixIndexAssignment(MatrixIndex, new Operators.Logical.And(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is ColumnVector ColumnVector)
									return new MatrixColumnAssignment(ColumnVector, new Operators.Logical.And(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is RowVector RowVector)
									return new MatrixRowAssignment(RowVector, new Operators.Logical.And(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else
									throw new SyntaxException("Invalid use of the &&= operator.", this.pos, this.script);
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

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

							if (!(Ref is null))
								return new Operators.Assignments.WithSelf.BinaryOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
							else if (Left is NamedMember NamedMember)
								return new NamedMemberAssignment(NamedMember, new Operators.Binary.Or(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is VectorIndex VectorIndex)
								return new VectorIndexAssignment(VectorIndex, new Operators.Binary.Or(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is MatrixIndex MatrixIndex)
								return new MatrixIndexAssignment(MatrixIndex, new Operators.Binary.Or(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is ColumnVector ColumnVector)
								return new MatrixColumnAssignment(ColumnVector, new Operators.Binary.Or(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is RowVector RowVector)
								return new MatrixRowAssignment(RowVector, new Operators.Binary.Or(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else
								throw new SyntaxException("Invalid use of the |= operator.", this.pos, this.script);

						case '|':
							this.pos++;
							if (this.PeekNextChar() == '=')
							{
								this.pos++;

								Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

								if (!(Ref is null))
									return new Operators.Assignments.WithSelf.LogicalOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start, this);
								else if (Left is NamedMember NamedMember)
									return new NamedMemberAssignment(NamedMember, new Operators.Logical.Or(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is VectorIndex VectorIndex)
									return new VectorIndexAssignment(VectorIndex, new Operators.Logical.Or(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is MatrixIndex MatrixIndex)
									return new MatrixIndexAssignment(MatrixIndex, new Operators.Logical.Or(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is ColumnVector ColumnVector)
									return new MatrixColumnAssignment(ColumnVector, new Operators.Logical.Or(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Left is RowVector RowVector)
									return new MatrixRowAssignment(RowVector, new Operators.Logical.Or(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else
									throw new SyntaxException("Invalid use of the ||= operator.", this.pos, this.script);
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

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

							if (!(Ref is null))
								return new Operators.Assignments.WithSelf.ShiftSelfLeft(Ref.VariableName, Right, Start, this.pos - Start, this);
							else if (Left is NamedMember NamedMember)
								return new NamedMemberAssignment(NamedMember, new ShiftLeft(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is VectorIndex VectorIndex)
								return new VectorIndexAssignment(VectorIndex, new ShiftLeft(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is MatrixIndex MatrixIndex)
								return new MatrixIndexAssignment(MatrixIndex, new ShiftLeft(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is ColumnVector ColumnVector)
								return new MatrixColumnAssignment(ColumnVector, new ShiftLeft(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is RowVector RowVector)
								return new MatrixRowAssignment(RowVector, new ShiftLeft(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else
								throw new SyntaxException("Invalid use of the <<= operator.", this.pos, this.script);
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

							ScriptNode Right = this.AssertRightOperandNotNull(this.ParseStatement(false));

							if (!(Ref is null))
								return new Operators.Assignments.WithSelf.ShiftSelfRight(Ref.VariableName, Right, Start, this.pos - Start, this);
							else if (Left is NamedMember NamedMember)
								return new NamedMemberAssignment(NamedMember, new ShiftRight(NamedMember, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is VectorIndex VectorIndex)
								return new VectorIndexAssignment(VectorIndex, new ShiftRight(VectorIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is MatrixIndex MatrixIndex)
								return new MatrixIndexAssignment(MatrixIndex, new ShiftRight(MatrixIndex, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is ColumnVector ColumnVector)
								return new MatrixColumnAssignment(ColumnVector, new ShiftRight(ColumnVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else if (Left is RowVector RowVector)
								return new MatrixRowAssignment(RowVector, new ShiftRight(RowVector, Right, Start, this.pos - Start, this), Start, this.pos - Start, this);
							else
								throw new SyntaxException("Invalid use of the >>= operator.", this.pos, this.script);
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
					else if (Left is ToVector ToVector)
					{
						Ref = ToVector.Operand as VariableReference;
						if (Ref is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Vector };
					}
					else if (Left is ToMatrix ToMatrix)
					{
						Ref = ToMatrix.Operand as VariableReference;
						if (Ref is null)
						{
							throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
								Left.Start, this.script);
						}

						ArgumentNames = new string[] { Ref.VariableName };
						ArgumentTypes = new ArgumentType[] { ArgumentType.Matrix };
					}
					else if (Left is ToSet ToSet)
					{
						Ref = ToSet.Operand as VariableReference;
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
							else if (Argument is ToVector ToVector2)
							{
								Ref = ToVector2.Operand as VariableReference;
								if (Ref is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Vector;
							}
							else if (Argument is ToMatrix ToMatrix2)
							{
								Ref = ToMatrix2.Operand as VariableReference;
								if (Ref is null)
								{
									throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
										Argument.Start, this.script);
								}

								ArgumentTypes[i] = ArgumentType.Matrix;
							}
							else if (Argument is ToSet ToSet2)
							{
								Ref = ToSet2.Operand as VariableReference;
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

					if (!(this.ParseEquivalence() is ScriptNode Operand))
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
				if (this.PeekNextChar() == '=')
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
					case 'M':
					case 'N':
					case '∈':
					case '∉':
						switch (this.PeekNextToken().ToUpper())
						{
							case "IS":
								this.pos += 2;

								this.SkipWhiteSpace();
								if (string.Compare(this.PeekNextToken(), "NOT", true) == 0)
								{
									this.pos += 3;
									Right = this.AssertRightOperandNotNull(this.ParseComparison());
									Left = new IsNot(Left, Right, Start, this.pos - Start, this);
								}
								else
								{
									Right = this.AssertRightOperandNotNull(this.ParseComparison());
									Left = new Is(Left, Right, Start, this.pos - Start, this);
								}
								continue;

							case "INHERITS":
								this.pos += 8;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new Inherits(Left, Right, Start, this.pos - Start, this);
								continue;

							case "AS":
								this.pos += 2;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new As(Left, Right, Start, this.pos - Start, this);
								continue;

							case "MATCHES":
								this.pos += 7;
								Right = this.AssertRightOperandNotNull(this.ParseComparison());
								Left = new Matches(Left, Right, Start, this.pos - Start, this);
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
								if (string.Compare(this.PeekNextToken(), "IN", true) == 0)
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
								if (Left is LesserThan LT)
									Left = new Range(LT.LeftOperand, LT.RightOperand, Right, false, true, LT.Start, Right.Start + Right.Length - LT.Start, this);
								else if (Left is LesserThanOrEqualTo LTE)
									Left = new Range(LTE.LeftOperand, LTE.RightOperand, Right, true, true, LTE.Start, Right.Start + Right.Length - LTE.Start, this);
								else
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
							if (Left is LesserThan LT)
								Left = new Range(LT.LeftOperand, LT.RightOperand, Right, false, false, LT.Start, Right.Start + Right.Length - LT.Start, this);
							else if (Left is LesserThanOrEqualTo LTE)
								Left = new Range(LTE.LeftOperand, LTE.RightOperand, Right, true, false, LTE.Start, Right.Start + Right.Length - LTE.Start, this);
							else
								Left = new LesserThan(Left, Right, Start, this.pos - Start, this);
						}
						break;

					case '>':
						this.pos++;
						if ((ch = this.PeekNextChar()) == '=')
						{
							this.pos++;
							Right = this.AssertRightOperandNotNull(this.ParseShifts());
							if (Left is GreaterThan GT)
								Left = new Range(Right, GT.RightOperand, GT.LeftOperand, true, false, GT.Start, Right.Start + Right.Length - GT.Start, this);
							else if (Left is GreaterThanOrEqualTo GTE)
								Left = new Range(Right, GTE.RightOperand, GTE.LeftOperand, true, true, GTE.Start, Right.Start + Right.Length - GTE.Start, this);
							else
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
							if (Left is GreaterThan GT)
								Left = new Range(Right, GT.RightOperand, GT.LeftOperand, false, false, GT.Start, Right.Start + Right.Length - GT.Start, this);
							else if (Left is GreaterThanOrEqualTo GTE)
								Left = new Range(Right, GTE.RightOperand, GTE.LeftOperand, false, true, GTE.Start, Right.Start + Right.Length - GTE.Start, this);
							else
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
						{
							if (Left is LesserThan LT)
								Left = new Range(LT.LeftOperand, LT.RightOperand, Right, false, true, LT.Start, Right.Start + Right.Length - LT.Start, this);
							else if (Left is LesserThanOrEqualTo LTE)
								Left = new Range(LTE.LeftOperand, LTE.RightOperand, Right, true, true, LTE.Start, Right.Start + Right.Length - LTE.Start, this);
							else
								Left = new LesserThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
						}
						break;

					case '≥':
						this.pos++;
						Right = this.AssertRightOperandNotNull(this.ParseShifts());
						{
							if (Left is GreaterThan GT)
								Left = new Range(Right, GT.RightOperand, GT.LeftOperand, true, false, GT.Start, Right.Start + Right.Length - GT.Start, this);
							else if (Left is GreaterThanOrEqualTo GTE)
								Left = new Range(Right, GTE.RightOperand, GTE.LeftOperand, true, true, GTE.Start, Right.Start + Right.Length - GTE.Start, this);
							else
								Left = new GreaterThanOrEqualTo(Left, Right, Start, this.pos - Start, this);
						}
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
								if (string.Compare(this.PeekNextToken(), "LIKE", true) == 0)
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
					if (string.Compare(this.PeekNextToken(), "UNION", true) == 0)
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
						ch = this.PeekNextChar();

						if (ch == '=' || ch == '+')
						{
							this.pos--;
							return Left;
						}
						else if (ch == '-')
						{
							this.pos++;

							Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
							Left = new CreateMeasurement(Left, Right, Start, this.pos - Start, this);
						}
						else
						{
							Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
							Left = new Add(Left, Right, Start, this.pos - Start, this);
						}
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

					case '±':
						this.pos++;

						Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
						Left = new CreateMeasurement(Left, Right, Start, this.pos - Start, this);
						break;

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
				if (char.ToUpper(this.PeekNextChar()) == 'O' && string.Compare(this.PeekNextToken(), "OVER", true) == 0)
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
				switch (char.ToUpper(this.PeekNextChar()))
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
						if (string.Compare(this.PeekNextToken(), "DOT", true) == 0)
						{
							this.pos += 3;
							Right = this.AssertRightOperandNotNull(this.ParsePowers());
							Left = new DotProduct(Left, Right, Start, this.pos - Start, this);
							continue;
						}
						else
							return Left;

					case 'M':
						if (string.Compare(this.PeekNextToken(), "MOD", true) == 0)
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
						switch (char.ToUpper(this.PeekNextChar()))
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
								if (string.Compare(this.PeekNextToken(), "MOD", true) == 0)
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

			switch (char.ToUpper(this.PeekNextChar()))
			{
				case '-':
					this.pos++;
					if ((ch = this.PeekNextChar()) == '-')
					{
						this.pos++;

						ScriptNode Op = this.ParseUnaryPrefixOperator();

						if (Op is VariableReference Ref)
							return new Operators.Assignments.Pre.PreDecrement(Ref.VariableName, Start, this.pos - Start, this);
						else if (Op is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new MinusOne(NamedMember, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new MinusOne(VectorIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new MinusOne(MatrixIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new MinusOne(ColumnVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new MinusOne(RowVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the -- operator.", this.pos, this.script);
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

						ScriptNode Op = this.ParseUnaryPrefixOperator();

						if (Op is VariableReference Ref)
							return new Operators.Assignments.Pre.PreIncrement(Ref.VariableName, Start, this.pos - Start, this);
						else if (Op is NamedMember NamedMember)
							return new NamedMemberAssignment(NamedMember, new PlusOne(NamedMember, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is VectorIndex VectorIndex)
							return new VectorIndexAssignment(VectorIndex, new PlusOne(VectorIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is MatrixIndex MatrixIndex)
							return new MatrixIndexAssignment(MatrixIndex, new PlusOne(MatrixIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is ColumnVector ColumnVector)
							return new MatrixColumnAssignment(ColumnVector, new PlusOne(ColumnVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else if (Op is RowVector RowVector)
							return new MatrixRowAssignment(RowVector, new PlusOne(RowVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
						else
							throw new SyntaxException("Invalid use of the ++ operator.", this.pos, this.script);
					}
					else if ((ch >= '0' && ch <= '9') || (ch == '.'))
						return this.ParseSuffixOperator();
					else
						return this.AssertOperandNotNull(this.ParseFactors());

				case '!':
					this.pos++;
					return new Not(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start, this);

				case 'N':
					if (string.Compare(this.PeekNextToken(), "NOT", true) == 0)
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
							this.pos++;
							if (this.PeekNextChar() == '?')
							{
								this.pos -= 2;
								return Node;
							}
							else
							{
								ScriptNode IfNull = this.AssertOperandNotNull(this.ParseStatement(false));
								Node = new NullCheck(Node, IfNull, Start, this.pos - Start, this);
							}
							break;
						}
						else
						{
							this.pos++;
							ch = this.PeekNextChar();
							switch (ch)
							{
								case '.':
								case '(':
								case '[':
								case '{':
								case '?':
									NullCheck = true;
									continue;

								default:
									this.pos--;
									return Node;
							}
						}

					case '.':
						this.pos++;

						ch = this.PeekNextChar();
						if (ch == '=' || ch == '+' || ch == '-' || ch == '^' || ch == '.' || ch == '*' || ch == '⋅' || ch == '/' || ch == '\\' || ch == '<' || ch == '!')
						{
							this.pos--;
							return Node;
						}

						if (char.ToUpper(ch) == 'M' && string.Compare(this.PeekNextToken(), "MOD", true) == 0)
						{
							this.pos--;
							return Node;
						}

						ScriptNode Right = this.AssertRightOperandNotNull(this.ParseObject());

						if (Right is VariableReference Ref)
							Node = new NamedMember(Node, Ref.VariableName, NullCheck, Start, this.pos - Start, this);
						else
							Node = new DynamicMember(Node, Right, NullCheck, Start, this.pos - Start, this);

						break;

					case '(':
						bool WsBak = this.canSkipWhitespace;
						this.canSkipWhitespace = true;
						this.pos++;
						Right = this.ParseList();

						this.SkipWhiteSpace();
						if (this.PeekNextChar() != ')')
							throw new SyntaxException("Expected ).", this.pos, this.script);

						this.canSkipWhitespace = WsBak;
						this.pos++;

						Ref = Node as VariableReference;
						if (Ref is null)
						{
							if (Node is NamedMember NamedMember)
							{
								if (Right is null)
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, Array.Empty<ScriptNode>(), NamedMember.NullCheck || NullCheck, Start, this.pos - Start, this);
								else if (Right.GetType() == typeof(ElementList))
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, ((ElementList)Right).Elements, NamedMember.NullCheck || NullCheck, Start, this.pos - Start, this);
								else
									Node = new NamedMethodCall(NamedMember.Operand, NamedMember.Name, new ScriptNode[] { Right }, NamedMember.NullCheck || NullCheck, Start, this.pos - Start, this);
							}// TODO: Dynamic named method call.
							else
							{
								if (Right is null)
									Node = new DynamicFunctionCall(Node, Array.Empty<ScriptNode>(), NullCheck, Start, this.pos - Start, this);
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
						WsBak = this.canSkipWhitespace;
						this.canSkipWhitespace = true;
						this.pos++;
						Right = this.ParseList();

						this.SkipWhiteSpace();
						if (this.PeekNextChar() != ']')
							throw new SyntaxException("Expected ].", this.pos, this.script);

						this.canSkipWhitespace = WsBak;
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

							if (!(Ref is null))
								Node = new Operators.Assignments.Post.PostIncrement(Ref.VariableName, Start, this.pos - Start, this);
							else
							{
								if (Node is NamedMember NamedMember)
									Node = new NamedMemberAssignment(NamedMember, new PlusOne(NamedMember, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is VectorIndex VectorIndex)
									Node = new VectorIndexAssignment(VectorIndex, new PlusOne(VectorIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is MatrixIndex MatrixIndex)
									Node = new MatrixIndexAssignment(MatrixIndex, new PlusOne(MatrixIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is ColumnVector ColumnVector)
									Node = new MatrixColumnAssignment(ColumnVector, new PlusOne(ColumnVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is RowVector RowVector)
									Node = new MatrixRowAssignment(RowVector, new PlusOne(RowVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else
								{
									this.pos -= 2;  // Can be a prefix operator.
									return Node;
								}

								Node = new MinusOne(Node, Start, this.pos - Start, this);
							}

							if (NullCheck)
								throw new SyntaxException("Null-checked post increment operator not defined.", this.pos, this.script);

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

							if (!(Ref is null))
								Node = new Operators.Assignments.Post.PostDecrement(Ref.VariableName, Start, this.pos - Start, this);
							else
							{
								if (Node is NamedMember NamedMember)
									Node = new NamedMemberAssignment(NamedMember, new MinusOne(NamedMember, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is VectorIndex VectorIndex)
									Node = new VectorIndexAssignment(VectorIndex, new MinusOne(VectorIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is MatrixIndex MatrixIndex)
									Node = new MatrixIndexAssignment(MatrixIndex, new MinusOne(MatrixIndex, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is ColumnVector ColumnVector)
									Node = new MatrixColumnAssignment(ColumnVector, new MinusOne(ColumnVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else if (Node is RowVector RowVector)
									Node = new MatrixRowAssignment(RowVector, new MinusOne(RowVector, Start, this.pos - Start, this), Start, this.pos - Start, this);
								else
								{
									this.pos -= 2;  // Can be a prefix operator.
									return Node;
								}

								Node = new PlusOne(Node, Start, this.pos - Start, this);
							}

							if (NullCheck)
								throw new SyntaxException("Null-checked post increment operator not defined.", this.pos, this.script);

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

							Unit Unit = new Unit(Prefix.None, new UnitFactor("°" + new string(ch, 1)));

							if (Node is ConstantElement ConstantElement)
							{
								IElement C = ConstantElement.Constant;

								if (C.AssociatedObjectValue is double d)
								{
									Node = new ConstantElement(new PhysicalQuantity(d, Unit),
										ConstantElement.Start, this.pos - ConstantElement.Start, this);
								}
								else if (C.AssociatedObjectValue is IPhysicalQuantity)
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

							if (!this.TryParseUnit(ref Node))	// T might be referencing the T prefix.
								return Node;
						}
						else
						{
							if (NullCheck)
								throw new SyntaxException("Null-checked T operator not defined.", this.pos, this.script);

							Node = new Transpose(Node, Start, this.pos - Start, this);
						}
						break;

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
							if (!this.TryParseUnit(ref Node))
								return Node;
						}
						else
							return Node;
						break;
				}

				NullCheck = false;
			}
		}

		private bool TryParseUnit(ref ScriptNode Node)
		{
			int Bak = this.pos;

			Unit Unit = this.ParseUnit(true);
			if (Unit is null)
			{
				this.pos = Bak;
				return false;
			}

			int Start = Node.Start;

			if (Node is ConstantElement ConstantElement)
			{
				IElement C = ConstantElement.Constant;

				if (C.AssociatedObjectValue is double d)
				{
					Node = new ConstantElement(new PhysicalQuantity(d, Unit),
						ConstantElement.Start, this.pos - ConstantElement.Start, this);
				}
				else if (C.AssociatedObjectValue is IPhysicalQuantity)
					Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);
				else
				{
					this.pos = Bak;
					return false;
				}
			}
			else
				Node = new SetUnit(Node, Unit, Start, this.pos - Start, this);

			return true;
		}

		internal Unit ParseUnit(bool PermitPrefix)
		{
			Prefix Prefix;
			ChunkedList<UnitFactor> Factors = new ChunkedList<UnitFactor>();
			KeyValuePair<Prefix, UnitFactor[]> CompoundFactors;
			bool HasCompoundFactors;
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

					ch = this.NextChar();
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

					if (LastDivision)
					{
						foreach (UnitFactor Factor in Unit.Factors)
							Factors.Add(new UnitFactor(Factor.Unit, -Factor.Exponent * Exponent));
					}
					else
					{
						foreach (UnitFactor Factor in Unit.Factors)
							Factors.Add(new UnitFactor(Factor.Unit, Factor.Exponent * Exponent));
					}
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
						else if (HasCompoundFactors = Unit.TryGetCompoundUnit(Name2, out CompoundFactors))
						{
							Prefix = CompoundFactors.Key;
							Name = Name2;
						}
						else if (Unit.ContainsDerivedOrBaseUnit(Name2))
						{
							Prefix = Prefix.None;
							Name = Name2;
						}
						else
							HasCompoundFactors = Unit.TryGetCompoundUnit(Name, out CompoundFactors);
					}
					else
						HasCompoundFactors = Unit.TryGetCompoundUnit(Name, out CompoundFactors);

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

					if (HasCompoundFactors)
					{
						if (LastDivision)
						{
							foreach (UnitFactor Segment in CompoundFactors.Value)
								Factors.Add(new UnitFactor(Segment.Unit, -Segment.Exponent * Exponent));
						}
						else
						{
							foreach (UnitFactor Segment in CompoundFactors.Value)
								Factors.Add(new UnitFactor(Segment.Unit, Segment.Exponent * Exponent));
						}
					}
					else
					{
						if (LastDivision)
							Factors.Add(new UnitFactor(Name, -Exponent));
						else
							Factors.Add(new UnitFactor(Name, Exponent));
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

			if (!Factors.HasFirstItem)
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
					return new NamedFunctionCall(FunctionName, Array.Empty<ScriptNode>(), NullCheck, Start, Length, Expression);
				else
					return new NamedFunctionCall(FunctionName, new ScriptNode[] { Arguments }, NullCheck, Start, Length, Expression);
			}
		}

		/// <summary>
		/// Tries to get a constant value, given its name.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Variables">Current set of variables. Can be null.</param>
		/// <param name="ValueElement">If found, constant value will be placed here.</param>
		/// <returns>If a constant with the given name was found.</returns>
		public static bool TryGetConstant(string Name, Variables Variables, out IElement ValueElement)
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

			ValueElement = Constant.GetValueElement(Variables ?? new Variables());
			return !(ValueElement is null);
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

				if (Ref.Constructor.GetParameters().Length != c + 3)
				{
					if (!F.TryGetValue(FunctionName + " " + c.ToString(), out Ref))
						return null;
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
						if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
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
										Log.Exception(ex3);
								}
								else
									Log.Exception(ex);
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

					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IConstant)))
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(T);
						if (CI is null)
							continue;

						try
						{
							IConstant Constant = (IConstant)CI.Invoke(Types.NoParameters);

							s = Constant.ConstantName;
							if (Found.TryGetValue(s, out IConstant PrevConstant))
							{
								if (PrevConstant.GetType() != T)
								{
									Log.Warning("Constant with name " + s + " previously registered. Constant ignored.",
										T.FullName, new KeyValuePair<string, object>("Previous", Constant.GetType().FullName));
								}
							}
							else
								Found[s] = Constant;

							Aliases = Constant.Aliases;
							if (!(Aliases is null))
							{
								foreach (string Alias in Aliases)
								{
									if (Found.TryGetValue(Alias, out PrevConstant))
									{
										if (PrevConstant.GetType() != T)
										{
											Log.Warning("Constant with name " + Alias + " previously registered. Constant ignored.",
												T.FullName, new KeyValuePair<string, object>("Previous", Constant.GetType().FullName));
										}
									}
									else
										Found[Alias] = Constant;
								}
							}
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					constants = Found;
				}

				if (customKeyWords is null)
				{
					Dictionary<string, IKeyWord> Found = new Dictionary<string, IKeyWord>(StringComparer.CurrentCultureIgnoreCase);
					string[] Aliases;
					string s;

					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IKeyWord)))
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(T);
						if (CI is null)
							continue;

						try
						{
							IKeyWord KeyWord = (IKeyWord)CI.Invoke(Types.NoParameters);

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
							Log.Exception(ex);
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
				bool WsBak = this.canSkipWhitespace;
				this.canSkipWhitespace = true;
				this.pos++;
				Node = this.ParseSequence();

				this.SkipWhiteSpace();
				if (this.PeekNextChar() != ')')
					throw new SyntaxException("Expected ).", this.pos, this.script);

				this.canSkipWhitespace = WsBak;
				this.pos++;

				if (Node is null)
				{
					this.SkipWhiteSpace();
					if (this.PeekNextChar() == '-')
					{
						this.pos++;
						if (this.PeekNextChar() == '>')
						{
							this.pos++;

							if (!(this.ParseEquivalence() is ScriptNode Operand))
								throw new SyntaxException("Lambda function body missing.", this.pos, this.script);

							return new LambdaDefinition(Array.Empty<string>(), Array.Empty<ArgumentType>(), Operand, Start, this.pos - Start, this);
						}
					}

					throw new SyntaxException("Expected argument-less Lambda expression", this.pos, this.script);
				}
				else
				{
					Node.Start = Start;
					Node.Length = this.pos - Start;
					return Node;
				}
			}
			else if (ch == '[')
			{
				this.pos++;
				this.SkipWhiteSpace();
				if (this.PeekNextChar() == ']')
				{
					this.pos++;
					return new VectorDefinition(Array.Empty<ScriptNode>(), Start, this.pos - Start, this);
				}

				bool WsBak = this.canSkipWhitespace;
				this.canSkipWhitespace = true;
				Node = this.ParseStatement(true);

				this.SkipWhiteSpace();
				switch (this.PeekNextChar())
				{
					case ']':
						this.pos++;
						this.canSkipWhitespace = WsBak;

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

					case ':':
						this.pos++;

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
						if (this.PeekNextChar() != ']')
							throw new SyntaxException("Expected ].", this.pos, this.script);

						this.canSkipWhitespace = WsBak;
						this.pos++;

						return new ImplicitVectorDefinition(Node, SuperSet, Conditions, Start, this.pos - Start, this);

					default:
						throw new SyntaxException("Expected ] or :.", this.pos, this.script);
				}
			}
			else if (ch == '{')
			{
				bool ObjectWildcard = false;
				bool WsBak = this.canSkipWhitespace;

				this.pos++;
				this.canSkipWhitespace = true;
				this.SkipWhiteSpace();

				switch (this.PeekNextChar())
				{
					case '}':
						this.pos++;
						this.CanSkipWhitespace = WsBak;
						return new ObjectExNihilo(new ChunkedList<KeyValuePair<string, ScriptNode>>(), false, Start, this.pos - Start, this);

					case '*':
						this.pos++;
						ObjectWildcard = true;
						Node = null;
						break;

					default:
						Node = this.ParseStatement(true);
						break;
				}

				this.SkipWhiteSpace();
				if (ObjectWildcard || (ch = this.PeekNextChar()) == ':')
				{
					bool DoubleColon = false;

					if (!ObjectWildcard)
					{
						this.pos++;

						if (this.PeekNextChar() == ':')
						{
							this.pos++;
							DoubleColon = true;
						}
					}

					if (!DoubleColon && (ObjectWildcard || Node is VariableReference || Node is ConstantElement))
					{
						ChunkedList<KeyValuePair<string, ScriptNode>> Members = new ChunkedList<KeyValuePair<string, ScriptNode>>();
						Dictionary<string, bool> MembersFound = new Dictionary<string, bool>();
						ConstantElement ConstantElement;
						StringValue StringValue;
						string s;

						if (!ObjectWildcard)
						{
							if (Node is VariableReference VariableReference)
								s = VariableReference.VariableName;
							else if (!((ConstantElement = Node as ConstantElement) is null) &&
								!((StringValue = ConstantElement.Constant as StringValue) is null))
							{
								s = StringValue.Value;
							}
							else
								throw new SyntaxException("Expected a variable reference or a string constant.", this.pos, this.script);

							MembersFound[s] = true;
							Members.Add(new KeyValuePair<string, ScriptNode>(s, this.ParseLambdaExpression()));
						
							this.SkipWhiteSpace();
						}

						while ((ch = this.PeekNextChar()) == ',')
						{
							this.pos++;
							this.SkipWhiteSpace();

							if (this.PeekNextChar() == '*')
							{
								this.pos++;
								ObjectWildcard = true;
							}
							else
							{
								Node = this.ParseStatement(false);

								this.SkipWhiteSpace();
								if (this.PeekNextChar() != ':')
									throw new SyntaxException("Expected :.", this.pos, this.script);

								if (Node is VariableReference VariableReference2)
									s = VariableReference2.VariableName;
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
								Members.Add(new KeyValuePair<string, ScriptNode>(s, this.ParseLambdaExpression()));
							}

							this.SkipWhiteSpace();
						}

						if (ch != '}')
							throw new SyntaxException("Expected }.", this.pos, this.script);

						this.canSkipWhitespace = WsBak;
						this.pos++;
						return new ObjectExNihilo(Members, ObjectWildcard, Start, this.pos - Start, this);
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
					if (this.PeekNextChar() != '}')
						throw new SyntaxException("Expected }.", this.pos, this.script);

					this.canSkipWhitespace = WsBak;
					this.pos++;

					return new ImplicitSetDefinition(Node, SuperSet, Conditions, DoubleColon, Start, this.pos - Start, this);
				}

				if (ch != '}')
					throw new SyntaxException("Expected }.", this.pos, this.script);

				this.canSkipWhitespace = WsBak;
				this.pos++;

				if (Node is For For)
					return new SetForDefinition(For, Start, this.pos - Start, this);
				else if (Node is ForEach ForEach)
					return new SetForEachDefinition(ForEach, Start, this.pos - Start, this);
				else if (Node is DoWhile DoWhile)
					return new SetDoWhileDefinition(DoWhile, Start, this.pos - Start, this);
				else if (Node is WhileDo WhileDo)
					return new SetWhileDoDefinition(WhileDo, Start, this.pos - Start, this);
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
			else if (ch == '#')
			{
				char Base;
				bool Sign = false;

				this.pos++;
				ch = this.PeekNextChar();

				if (ch == '-')
				{
					Sign = true;
					this.pos++;
					ch = this.PeekNextChar();
				}
				else if (ch == '+')
				{
					this.pos++;
					ch = this.PeekNextChar();
				}

				ch = char.ToLower(ch);
				int Start2 = this.pos;

				if (ch >= '0' && ch <= '9')
					Base = 'd';
				else if (ch == 'd' || ch == 'x' || ch == 'o' || ch == 'b')
				{
					Base = ch;
					Start2 = ++this.pos;
				}
				else
					throw new SyntaxException("Invalid numerical base.", this.pos, this.script);

				BigInteger n = BigInteger.Zero;

				switch (Base)
				{
					case 'd':
						while (this.pos < this.len && (ch = this.script[this.pos]) >= '0' && ch <= '9')
							this.pos++;

						if (Start2 == this.pos)
							throw new SyntaxException("Invalid integer.", this.pos, this.script);

						n = BigInteger.Parse(this.script.Substring(Start2, this.pos - Start2));
						break;

					case 'x':
						n = 0;
						while (this.pos < this.len)
						{
							ch = this.script[this.pos];

							if (ch >= '0' && ch <= '9')
								ch -= '0';
							else if (ch >= 'a' && ch <= 'f')
								ch -= (char)('a' - 10);
							else if (ch >= 'A' && ch <= 'F')
								ch -= (char)('A' - 10);
							else
								break;

							this.pos++;
							n <<= 4;
							n += ch;
						}
						break;

					case 'o':
						while (this.pos < this.len && (ch = this.script[this.pos]) >= '0' && ch <= '7')
						{
							this.pos++;
							n <<= 3;
							n += ch - '0';
						}
						break;

					case 'b':
						while (this.pos < this.len && (ch = this.script[this.pos]) >= '0' && ch <= '1')
						{
							this.pos++;
							n <<= 1;
							n += ch - '0';
						}
						break;
				}

				if (Start2 == this.pos)
					throw new SyntaxException("Invalid integer.", this.pos, this.script);

				if (Sign)
					n = -n;

				return new ConstantElement(new Integer(n), Start, this.pos - Start, this);
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
						Node = this.ParseCustomNode(s, false, Start);
						if (Node is null)
							return new VariableReference(s, Start, this.pos - Start, this);
						else
							return Node;
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

				return this.ParseCustomNode(new string(ch, 1), true, Start);
			}
		}

		private ScriptNode ParseCustomNode(string KeyWord, bool IncPosIfKeyword, int Start)
		{
			if (customKeyWords is null)
				Search();

			if (customKeyWords.TryGetValue(KeyWord, out IKeyWord KeyWordParser))
			{
				ScriptParser Parser = new ScriptParser(this, Start);
				int PosBak = this.pos;

				if (IncPosIfKeyword)
					this.pos += KeyWord.Length;

				bool CanParseWhitespace = this.canSkipWhitespace;
				bool Result = KeyWordParser.TryParse(Parser, out ScriptNode Node);

				this.canSkipWhitespace = CanParseWhitespace;

				if (Result)
					return Node;
				else
					this.pos = PosBak;
			}

			return null;
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
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public bool IsAsynchronous => this.root?.IsAsynchronous ?? false;

		/// <summary>
		/// Evaluates the expression, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for evaluating expressions in a synchronous (blocking) context.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		[Obsolete("Use the EvaluateAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public object Evaluate(Variables Variables)
		{
			IElement Result;

			try
			{
				if (this.root is null)
					Result = ObjectValue.Null;
				else if (this.root.IsAsynchronous)
					Result = this.root.EvaluateAsync(Variables).Result;
				else
					Result = this.root.Evaluate(Variables);
			}
			catch (ScriptReturnValueException ex)
			{
				Result = ex.ReturnValue;
				//ScriptReturnValueException.Reuse(ex);
			}
			catch (ScriptBreakLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptContinueLoopException.Reuse(ex);
			}

			return Result.AssociatedObjectValue;
		}

		/// <summary>
		/// Evaluates the expression, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for evaluating expressions in an asynchronous (non-blocking) context.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public async Task<object> EvaluateAsync(Variables Variables)
		{
			IElement Result;

			try
			{
				if (this.root is null)
					Result = ObjectValue.Null;
				else if (this.root.IsAsynchronous)
					Result = await this.root.EvaluateAsync(Variables);
				else
					Result = this.root.Evaluate(Variables);
			}
			catch (ScriptReturnValueException ex)
			{
				Result = ex.ReturnValue;
				//ScriptReturnValueException.Reuse(ex);
			}
			catch (ScriptBreakLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptContinueLoopException.Reuse(ex);
			}

			return Result.AssociatedObjectValue;
		}

		/// <summary>
		/// Root script node.
		/// </summary>
		public ScriptNode Root => this.root;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is Expression Exp)
				return this.script.Equals(Exp.script);
			else
				return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.script.GetHashCode();
		}

		/// <summary>
		/// If the expression contains implicit print operations.
		/// </summary>
		public bool ContainsImplicitPrint => this.containsImplicitPrint;

		/// <summary>
		/// If the expression, or any function call references, contain implicit print operations.
		/// </summary>
		/// <param name="Variables">Variables containing available function and lambda definitions.</param>
		/// <returns>If an implicit print operation was found.</returns>
		public bool ReferencesImplicitPrint(Variables Variables)
		{
			if (this.ContainsImplicitPrint)
				return true;

			Dictionary<string, bool> Processed = null;
			bool CheckFunctionCalls(ScriptNode Node, out ScriptNode NewNode, object State)
			{
				NewNode = null;

				if (Node is NamedFunctionCall f)
				{
					Expression Exp;

					if (Variables.TryGetVariable(f.FunctionName + " " + f.Arguments.Length.ToString(), out Variable v) &&
						v.ValueObject is ScriptNode N)
					{
						Exp = N.Expression;
					}
					else if (Variables.TryGetVariable(f.FunctionName, out v) &&
						v.ValueObject is ScriptNode N2)
					{
						Exp = N2.Expression;
					}
					else
						return true;

					if (Processed is null)
						Processed = new Dictionary<string, bool>() { { this.script, true } };

					if (Processed.ContainsKey(Exp.script))
						return true;

					Processed[Exp.script] = true;

					if (Exp.ContainsImplicitPrint || !Exp.ForAll(CheckFunctionCalls, null, SearchMethod.TreeOrder))
						return false;
				}

				return true;
			};

			if (!this.ForAll(CheckFunctionCalls, null, SearchMethod.TreeOrder))
				return true;

			return false;
		}

		/// <summary>
		/// Transforms a string by executing embedded script.
		/// </summary>
		/// <param name="s">String to transform.</param>
		/// <param name="StartDelimiter">Start delimiter.</param>
		/// <param name="StopDelimiter">Stop delimiter.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Transformed string.</returns>
		[Obsolete("Use the TransformAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public static string Transform(string s, string StartDelimiter, string StopDelimiter, Variables Variables)
		{
			return Transform(s, StartDelimiter, StopDelimiter, Variables, null);
		}

		/// <summary>
		/// Transforms a string by executing embedded script.
		/// </summary>
		/// <param name="s">String to transform.</param>
		/// <param name="StartDelimiter">Start delimiter.</param>
		/// <param name="StopDelimiter">Stop delimiter.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <param name="Source">Optional source of <paramref name="s"/>.</param>
		/// <returns>Transformed string.</returns>
		[Obsolete("Use the TransformAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public static string Transform(string s, string StartDelimiter, string StopDelimiter, Variables Variables, string Source)
		{
			int i = s.IndexOf(StartDelimiter);
			if (i < 0)
				return s;

			StringBuilder Transformed = new StringBuilder();
			Expression Exp;
			string Script;
			object Result;
			int j;
			int StartLen = StartDelimiter.Length;
			int StopLen = StopDelimiter.Length;
			int From = 0;

			while (i >= 0)
			{
				j = s.IndexOf(StopDelimiter, i + StartLen);
				if (j < 0)
				{
					if (From == 0)
						return s;
					else
						break;
				}

				if (i > From)
					Transformed.Append(s.Substring(From, i - From));

				From = j + StopLen;

				Script = s.Substring(i + StartLen, j - i - StartLen);

				Exp = new Expression(Script, Source);
				Result = Exp.Evaluate(Variables);

				if (!IsNullOrVoid(Result))
					Transformed.Append(Result.ToString());

				i = s.IndexOf(StartDelimiter, From);
			}

			if (From < s.Length)
				Transformed.Append(s.Substring(From));

			return Transformed.ToString();
		}

		/// <summary>
		/// Transforms a string by executing embedded script.
		/// </summary>
		/// <param name="s">String to transform.</param>
		/// <param name="StartDelimiter">Start delimiter.</param>
		/// <param name="StopDelimiter">Stop delimiter.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Transformed string.</returns>
		public static Task<string> TransformAsync(string s, string StartDelimiter, string StopDelimiter, Variables Variables)
		{
			return TransformAsync(s, StartDelimiter, StopDelimiter, Variables, null);
		}

		/// <summary>
		/// Transforms a string by executing embedded script.
		/// </summary>
		/// <param name="s">String to transform.</param>
		/// <param name="StartDelimiter">Start delimiter.</param>
		/// <param name="StopDelimiter">Stop delimiter.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <param name="Source">Optional source of <paramref name="s"/>.</param>
		/// <returns>Transformed string.</returns>
		public static async Task<string> TransformAsync(string s, string StartDelimiter, string StopDelimiter, Variables Variables, string Source)
		{
			int i = s.IndexOf(StartDelimiter);
			if (i < 0)
				return s;

			StringBuilder Transformed = new StringBuilder();
			ValuePrinter Printer = Variables.Printer;
			Expression Exp;
			string Script;
			object Result;
			int j;
			int StartLen = StartDelimiter.Length;
			int StopLen = StopDelimiter.Length;
			int From = 0;

			while (i >= 0)
			{
				j = s.IndexOf(StopDelimiter, i + StartLen);
				if (j < 0)
				{
					if (From == 0)
						return s;
					else
						break;
				}

				if (i > From)
					Transformed.Append(s.Substring(From, i - From));

				From = j + StopLen;

				Script = s.Substring(i + StartLen, j - i - StartLen);

				Exp = new Expression(Script, Source);
				Result = await Exp.EvaluateAsync(Variables);

				if (!IsNullOrVoid(Result))
					Transformed.Append(Printer is null ? Result.ToString() : await Printer(Result, Variables));

				i = s.IndexOf(StartDelimiter, From);
			}

			if (From < s.Length)
				Transformed.Append(s.Substring(From));

			return Transformed.ToString();
		}

		/// <summary>
		/// Checks if a result object value is equal to null or void 
		/// (i.e. its type equal to System.Threading.Tasks.VoidTaskResult).
		/// </summary>
		/// <param name="Result">Result object.</param>
		/// <returns>If value is null or void.</returns>
		public static bool IsNullOrVoid(object Result)
		{
			if (Result is null)
				return true;
			else
				return IsVoid(Result.GetType());
		}

		/// <summary>
		/// Checks if a result object type is equal to void 
		/// (i.e. its type equal to System.Threading.Tasks.VoidTaskResult).
		/// </summary>
		/// <param name="ResultType">Result type.</param>
		/// <returns>If type is void.</returns>
		public static bool IsVoid(Type ResultType)
		{
			if (VoidTaskResultType is null)
			{
				if (ResultType.FullName == "System.Threading.Tasks.VoidTaskResult")
				{
					VoidTaskResultType = ResultType;
					return true;
				}
				else
					return false;
			}
			else
				return ResultType == VoidTaskResultType;
		}

		private static Type VoidTaskResultType = null;

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
		/// Tries to parse a double-precision floating-point value.
		/// </summary>
		/// <param name="s">String-representation</param>
		/// <param name="Value">Parsed value.</param>
		/// <returns>If parsing was successful.</returns>
		public static bool TryParse(string s, out double Value)
		{
			return double.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Tries to parse a single-precision floating-point value.
		/// </summary>
		/// <param name="s">String-representation</param>
		/// <param name="Value">Parsed value.</param>
		/// <returns>If parsing was successful.</returns>
		public static bool TryParse(string s, out float Value)
		{
			return float.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Tries to parse a decimal-precision floating-point value.
		/// </summary>
		/// <param name="s">String-representation</param>
		/// <param name="Value">Parsed value.</param>
		/// <returns>If parsing was successful.</returns>
		public static bool TryParse(string s, out decimal Value)
		{
			return decimal.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(Complex Value)
		{
			return "(" + ToString(Value.Real) + ", " + ToString(Value.Imaginary) + ")";
		}

		/// <summary>
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(BigInteger Value)
		{
			return "#" + Value.ToString();
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

			Output.Append("DateTime");

			if (Value.Kind == DateTimeKind.Utc)
				Output.Append("Utc");

			Output.Append('(');
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
		/// Converts a value to a string, that can be parsed as part of an expression.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(Enum Value)
		{
			StringBuilder Output = new StringBuilder();

			Output.Append(Value.GetType().FullName);
			Output.Append('.');
			Output.Append(Value.ToString());

			return Output.ToString();
		}

		/// <summary>
		/// Converts a string value to a parsable expression string.
		/// </summary>
		/// <param name="s">Value</param>
		/// <returns>Expression representation of string.</returns>
		public static string ToString(string s)
		{
			if (s is null)
				return "null";

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

					k = Array.IndexOf(stringCharactersToEscape, s[i]);
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

		private static readonly char[] stringCharactersToEscape = new char[] { '\\', '"', '\n', '\r', '\t', '\b', '\f', '\a', '\v' };
		private static readonly string[] stringEscapeSequences = new string[] { "\\\\", "\\\"", "\\n", "\\r", "\\t", "\\b", "\\f", "\\a", "\\v" };

		/// <summary>
		/// Converts an object to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(object Value)
		{
			if (Value is null)
				return "null";
			else
			{
				Type T = Value.GetType();
				bool Found;
				ICustomStringOutput StringOutput;

				lock (output)
				{
					Found = output.TryGetValue(T, out StringOutput);
				}

				if (!Found)
				{
					StringOutput = Types.FindBest<ICustomStringOutput, Type>(T);

					lock (output)
					{
						output[T] = StringOutput;
					}
				}

				if (!(StringOutput is null))
					return StringOutput.GetString(Value);
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
				else
					return Value.ToString();
			}
		}

		/// <summary>
		/// Converts an object to a double value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Double value.</returns>
		public static double ToDouble(object Object)
		{
			if (Object is double db)
				return db;
			else if (Object is int i)
				return i;
			else if (Object is bool b)
				return b ? 1 : 0;
			else if (Object is byte bt)
				return bt;
			else if (Object is char ch)
				return ch;
			else if (Object is decimal dc)
				return (double)dc;
			else if (Object is short sh)
				return sh;
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
			else if (Object is BigInteger i2)
				return (double)i2;
			else if (Object is Complex z)
			{
				if (z.Imaginary == 0)
					return z.Real;
				else
					throw new ScriptException("Expected a double value.");
			}
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

		/// <summary>
		/// Converts an object to a double value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Double value.</returns>
		public static decimal ToDecimal(object Object)
		{
			if (Object is double db)
				return (decimal)db;
			else if (Object is int i)
				return i;
			else if (Object is bool b)
				return b ? 1 : 0;
			else if (Object is byte bt)
				return bt;
			else if (Object is char ch)
				return ch;
			else if (Object is decimal dc)
				return dc;
			else if (Object is short sh)
				return sh;
			else if (Object is long l)
				return l;
			else if (Object is sbyte sb)
				return sb;
			else if (Object is float f)
				return (decimal)f;
			else if (Object is ushort us)
				return us;
			else if (Object is uint ui)
				return ui;
			else if (Object is ulong ul)
				return ul;
			else if (Object is BigInteger i2)
				return (decimal)i2;
			else if (Object is Complex z)
			{
				if (z.Imaginary == 0)
					return (decimal)z.Real;
				else
					throw new ScriptException("Expected a double value.");
			}
			else
			{
				string s = Object.ToString();

				if (decimal.TryParse(s, out decimal d))
					return d;

				if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator != "." &&
					decimal.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out d))
				{
					return d;
				}

				throw new ScriptException("Expected a decimal value.");
			}
		}

		/// <summary>
		/// Converts an object to a complex value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Complex value.</returns>
		public static Complex ToComplex(object Object)
		{
			if (Object is Complex z)
				return z;
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
			else if (Value is Complex c)
				return new ComplexNumber(c);
			else if (Value is BigInteger i2)
				return new Integer(i2);
			else if (Value is Type t)
				return new TypeValue(t);
			else
			{
				if (Value is IElement e)
					return e;

				else if (Value is double[] dv)
					return new DoubleVector(dv);
				else if (Value is double[,] dm)
					return new DoubleMatrix(dm);

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
		/// <returns>If elements have been upgraded to become compatible.</returns>
		public static bool UpgradeSemiGroup(ref IElement E1, ref ISet Set1, ref IElement E2, ref ISet Set2)
		{
			if (E1 is StringValue)
			{
				E2 = new StringValue(E2.AssociatedObjectValue?.ToString() ?? string.Empty);
				Set2 = StringValues.Instance;
				return true;
			}

			if (E2 is StringValue)
			{
				E1 = new StringValue(E1.AssociatedObjectValue?.ToString() ?? string.Empty);
				Set1 = StringValues.Instance;
				return true;
			}

			if (UpgradeField(ref E1, ref Set1, ref E2, ref Set2))
				return true;

			return false;
		}

		/// <summary>
		/// Upgrades elements if necessary, to a common field extension, trying to make them compatible.
		/// </summary>
		/// <param name="E1">Element 1.</param>
		/// <param name="Set1">Set containing element 1.</param>
		/// <param name="E2">Element 2.</param>
		/// <param name="Set2">Set containing element 2.</param>
		/// <returns>If elements have been upgraded to become compatible.</returns>
		public static bool UpgradeField(ref IElement E1, ref ISet Set1, ref IElement E2, ref ISet Set2)
		{
			object O1 = E1?.AssociatedObjectValue;
			object O2 = E2?.AssociatedObjectValue;
			Type T1 = O1?.GetType() ?? typeof(object);
			Type T2 = O2?.GetType() ?? typeof(object);

			if (T1 == T2)
				return true;

			if (TryConvert(E1, T2, out IElement E1asT2))
			{
				E1 = E1asT2;
				Set1 = E1asT2.AssociatedSet;
				return true;
			}

			if (TryConvert(E2, T1, out IElement E2asT1))
			{
				E2 = E2asT1;
				Set2 = E2asT1.AssociatedSet;
				return true;
			}

			// TODO: Update to common extension field

			if (O1 is Enum Enum1 && O2 is double)
			{
				T1 = Enum.GetUnderlyingType(Enum1.GetType());
				if (T1 == typeof(int))
				{
					E1 = new DoubleNumber(Convert.ToInt32(Enum1));
					Set1 = DoubleNumbers.Instance;
					return true;
				}
			}
			else if (O2 is Enum Enum2 && O1 is double)
			{
				T2 = Enum.GetUnderlyingType(Enum2.GetType());
				if (T2 == typeof(int))
				{
					E2 = new DoubleNumber(Convert.ToInt32(Enum2));
					Set2 = DoubleNumbers.Instance;
					return true;
				}
			}

			return false;
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
			return ConvertTo(Value.AssociatedObjectValue, DesiredType, Node);
		}

		/// <summary>
		/// Tries to conevert an object to a desired type.
		/// </summary>
		/// <param name="Obj">Object to convert.</param>
		/// <param name="DesiredType">Desired type.</param>
		/// <param name="Node">Script node making the request.</param>
		/// <returns>Converted value.</returns>
		public static object ConvertTo(object Obj, Type DesiredType, ScriptNode Node)
		{
			if (Obj is null)
				return null;

			if (TryConvert(Obj, DesiredType, out object Result))
				return Result;

			Type T = Obj.GetType();
			if (T == DesiredType)
				return Obj;

			if (DesiredType.IsArray)
			{
				Type DesiredItemType = DesiredType.GetElementType();
				Array Dest;

				if (T.IsArray)
				{
					Array Source = (Array)Obj;
					int c = Source.Length;
					int i;

					Dest = (Array)Activator.CreateInstance(DesiredType, c);

					for (i = 0; i < c; i++)
						Dest.SetValue(ConvertTo(Source.GetValue(i), DesiredItemType, Node), i);
				}
				else
				{
					Dest = (Array)Activator.CreateInstance(DesiredType, 1);
					Dest.SetValue(ConvertTo(Obj, DesiredItemType, Node), 0);
				}

				return Dest;
			}

			return Convert.ChangeType(Obj, DesiredType);
		}

		/// <summary>
		/// This property allows the caller to tag the expression with an arbitrary object.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// Calls the callback method for all script nodes defined for the expression.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		[Obsolete("Use ForAll(ScriptNodeEventHandler, object, SearchMethod) instead.")]
		public bool ForAll(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			return this.ForAll(Callback, State, DepthFirst ? SearchMethod.DepthFirst : SearchMethod.BreadthFirst);
		}

		/// <summary>
		/// Calls the callback method for all script nodes defined for the expression.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAll(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!(this.root?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			if (!(this.root is null))
			{
				if (!Callback(this.root, out ScriptNode NewRoot, State))
					return false;

				if (!(NewRoot is null))
					this.root = NewRoot;
			}

			if (Order != SearchMethod.DepthFirst)
			{
				if (!(this.root?.ForAllChildNodes(Callback, State, Order) ?? true))
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
			if (TryConvert(Value, typeof(T), out object Obj))
			{
				if (Obj is T Result2)
				{
					Result = Result2;
					return true;
				}
				else if (Value is null && !typeof(T).GetTypeInfo().IsValueType)
				{
					Result = default;
					return true;
				}
			}

			Result = default;
			return false;
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
			if (Value is null)
			{
				Result = null;
				return !DesiredType.GetTypeInfo().IsValueType;
			}

			Type T = Value.GetType();
			TypeInfo TI = T.GetTypeInfo();

			if (DesiredType.GetTypeInfo().IsAssignableFrom(TI))
			{
				Result = Value;
				return true;
			}

			if (TryGetTypeConverter(T, DesiredType, out ITypeConverter Converter))
			{
				Result = Converter.Convert(Value);
				return !(Result is null) || (Value is null);
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to convert an element <paramref name="Value"/> to an element whose associated object is of type 
		/// <paramref name="DesiredType"/>.
		/// </summary>
		/// <param name="Value">Element to convert.</param>
		/// <param name="DesiredType">Desired type of associated object.</param>
		/// <param name="Result">Conversion result.</param>
		/// <returns>If conversion was successful.</returns>
		public static bool TryConvert(IElement Value, Type DesiredType, out IElement Result)
		{
			object Obj = Value?.AssociatedObjectValue;
			if (Obj is null)
			{
				Result = ObjectValue.Null;
				return !DesiredType.GetTypeInfo().IsValueType;
			}

			Type T = Obj.GetType();

			if (TryGetTypeConverter(T, DesiredType, out ITypeConverter Converter))
			{
				Result = Converter.ConvertToElement(Obj);
				return !(Result is null);
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to get a type converter, converting objects from type <paramref name="From"/> to objects of type
		/// <paramref name="To"/>.
		/// </summary>
		/// <param name="From">Start type.</param>
		/// <param name="To">Desired type.</param>
		/// <param name="Converter">Type Converter found, or null if not found.</param>
		/// <returns>If a type converter could be found, or constructed from existing converters.</returns>
		public static bool TryGetTypeConverter(Type From, Type To, out ITypeConverter Converter)
		{
			if (converters is null)
			{
				Dictionary<Type, Dictionary<Type, ITypeConverter>> Converters = GetTypeConverters();

				if (converters is null)
				{
					converters = Converters;
					Types.OnInvalidated += (Sender, e) => converters = GetTypeConverters();
				}
			}

			lock (converters)
			{
				if (!converters.TryGetValue(From, out Dictionary<Type, ITypeConverter> Converters) &&
					(!From.GetTypeInfo().IsEnum || !converters.TryGetValue(typeof(Enum), out Converters)))
				{
					Converter = null;
					return false;
				}

				if (Converters.TryGetValue(To, out Converter))
					return !(Converter is null);

				Dictionary<Type, bool> Explored = new Dictionary<Type, bool>() { { From, true } };
				ChunkedList<ITypeConverter> Search = new ChunkedList<ITypeConverter>();

				foreach (ITypeConverter Converter3 in Converters.Values)
				{
					if (!(Converter3 is null))
					{
						Search.Add(Converter3);
						Explored[Converter3.To] = true;
					}
				}

				while (Search.HasFirstItem)
				{
					ITypeConverter C = Search.RemoveFirst();

					if (converters.TryGetValue(C.To, out Dictionary<Type, ITypeConverter> Converters2) && !(Converters2 is null))
					{
						if (Converters2.TryGetValue(To, out ITypeConverter Converter2) && !(Converter2 is null))
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

							Converters[To] = ConversionSequence;
							return true;
						}

						foreach (ITypeConverter Converter3 in Converters2.Values)
						{
							if (!(Converter3 is null))
							{
								if (!Explored.ContainsKey(Converter3.To))
								{
									Search.Add(Converter3);
									Explored[Converter3.To] = true;
								}
							}
						}
					}
				}

				Converters[To] = null;
				Converter = null;
				return false;
			}
		}

		private static Dictionary<Type, Dictionary<Type, ITypeConverter>> GetTypeConverters()
		{
			Dictionary<Type, Dictionary<Type, ITypeConverter>> Converters = new Dictionary<Type, Dictionary<Type, ITypeConverter>>();

			foreach (Type T2 in Types.GetTypesImplementingInterface(typeof(ITypeConverter)))
			{
				ConstructorInfo DefaultConstructor = Types.GetDefaultConstructor(T2);
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
					Log.Exception(ex, T2.FullName);
				}
			}

			return Converters;
		}

		/// <summary>
		/// Evaluates script, in string format.
		/// </summary>
		/// <param name="Script">Script to parse and evaluate.</param>
		/// <returns>Result</returns>
		[Obsolete("Use the EvalAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public static object Eval(string Script)
		{
			return Eval(Script, new Variables());
		}

		/// <summary>
		/// Evaluates script, in string format.
		/// </summary>
		/// <param name="Script">Script to parse and evaluate.</param>
		/// <param name="Variables">Variables</param>
		/// <returns>Result</returns>
		[Obsolete("Use the EvalAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public static object Eval(string Script, Variables Variables)
		{
			Expression Exp = new Expression(Script);
			return Exp.Evaluate(Variables);
		}

		/// <summary>
		/// Evaluates script, in string format.
		/// </summary>
		/// <param name="Script">Script to parse and evaluate.</param>
		/// <returns>Result</returns>
		public static Task<object> EvalAsync(string Script)
		{
			return EvalAsync(Script, new Variables());
		}

		/// <summary>
		/// Evaluates script, in string format.
		/// </summary>
		/// <param name="Script">Script to parse and evaluate.</param>
		/// <param name="Variables">Variables</param>
		/// <returns>Result</returns>
		public static Task<object> EvalAsync(string Script, Variables Variables)
		{
			Expression Exp = new Expression(Script);
			return Exp.EvaluateAsync(Variables);
		}

		// TODO: Optimize constants
		// TODO: Integers (0d, 0x, 0o, 0b), Big Integers (0D, 0X, 0O, 0B)
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
			IsDiagonal
			IsLowerTriangular
			IsNullMatrix
			IsUpperTriangular
			LookUp
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
			Last(s,n)
			First(s,n)

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
