using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
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

namespace Waher.Script
{
    /// <summary>
    /// Class managing a script expression.
    /// </summary>
    public class Expression
    {
        private ScriptNode root;
        private string script;
        private int pos;
        private int len;

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

        private char NextChar()
        {
            if (this.pos < this.len)
                return this.script[this.pos++];
            else
                return (char)0;
        }

        private char PeekNextChar()
        {
            if (this.pos < this.len)
                return this.script[this.pos];
            else
                return (char)0;
        }

        private string NextToken()
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

            return this.script.Substring(Start, this.pos - Start);
        }

        private string PeekNextToken()
        {
            int Bak = this.pos;
            string Token = this.NextToken();
            this.pos = Bak;

            return Token;
        }

        private void SkipWhiteSpace()
        {
            while (this.pos < this.len && this.script[this.pos] <= ' ')
                this.pos++;
        }

        private ScriptNode AssertOperandNotNull(ScriptNode Node)
        {
            if (Node == null)
                throw new SyntaxException("Operand missing.", this.pos, this.script);

            return Node;
        }

        private ScriptNode AssertRightOperandNotNull(ScriptNode Node)
        {
            if (Node == null)
                throw new SyntaxException("Right operand missing.", this.pos, this.script);

            return Node;
        }

        private ScriptNode ParseSequence()
        {
            ScriptNode Node = this.ParseStatement();
            this.SkipWhiteSpace();

            if (Node == null)
            {
                while (Node == null && this.PeekNextChar() == ';')
                {
                    this.pos++;
                    Node = this.ParseStatement();
                    this.SkipWhiteSpace();
                }
            }

            if (Node == null)
                return null;

            int Start = Node.Start;

            if (Node != null && this.PeekNextChar() == ';')
            {
                this.pos++;
                ScriptNode Node2 = this.ParseStatement();
                if (Node2 != null)
                {
                    LinkedList<ScriptNode> Statements = new LinkedList<ScriptNode>();
                    Statements.AddLast(Node);
                    Statements.AddLast(Node2);

                    this.SkipWhiteSpace();
                    while (this.PeekNextChar() == ';')
                    {
                        this.pos++;
                        Node2 = this.ParseStatement();
                        if (Node2 == null)
                            break;

                        Statements.AddLast(Node2);
                        this.SkipWhiteSpace();
                    }

                    Node = new Sequence(Statements, Start, this.pos - Start);
                }
            }

            return Node;
        }

        private ScriptNode ParseStatement()
        {
            this.SkipWhiteSpace();

            int Start = this.pos;

            switch (char.ToUpper(this.PeekNextChar()))
            {
                case 'D':
                    if (this.PeekNextToken().ToUpper() == "DO")
                    {
                        this.pos += 2;

                        ScriptNode Statement = this.AssertOperandNotNull(this.ParseList());

                        this.SkipWhiteSpace();
                        if (this.PeekNextToken() != "WHILE")
                            throw new SyntaxException("Expected WHILE.", this.pos, this.script);

                        this.pos += 5;

                        ScriptNode Condition = this.AssertOperandNotNull(this.ParseList());

                        return new DoWhile(Statement, Condition, Start, this.pos - Start);
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

                        ScriptNode Statement = this.AssertOperandNotNull(this.ParseList());

                        return new WhileDo(Condition, Statement, Start, this.pos - Start);
                    }
                    else
                        return this.ParseList();

                case 'F':
                    switch (this.PeekNextToken().ToUpper())
                    {
                        case "FOREACH":
                            this.pos += 7;
                            In In = this.AssertOperandNotNull(this.ParseList()) as In;
                            if (In == null)
                                throw new SyntaxException("IN statement expected", this.pos, this.script);

                            VariableReference Ref = In.LeftOperand as VariableReference;
                            if (Ref == null)
                                throw new SyntaxException("Variable reference expected", Ref.Start, this.script);

                            this.SkipWhiteSpace();
                            if (this.PeekNextChar() == ':')
                                this.pos++;
                            else if (this.PeekNextToken().ToUpper() == "DO")
                                this.pos += 2;

                            ScriptNode Statement = this.AssertOperandNotNull(this.ParseList());

                            return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start);

                        case "FOR":
                            this.pos += 3;
                            this.SkipWhiteSpace();

                            if (this.PeekNextToken().ToUpper() == "EACH")
                            {
                                this.pos += 4;
                                In = this.AssertOperandNotNull(this.ParseList()) as In;
                                if (In == null)
                                    throw new SyntaxException("IN statement expected", this.pos, this.script);

                                Ref = In.LeftOperand as VariableReference;
                                if (Ref == null)
                                    throw new SyntaxException("Variable reference expected", Ref.Start, this.script);

                                this.SkipWhiteSpace();
                                if (this.PeekNextChar() == ':')
                                    this.pos++;
                                else if (this.PeekNextToken().ToUpper() == "DO")
                                    this.pos += 2;

                                Statement = this.AssertOperandNotNull(this.ParseList());

                                return new ForEach(Ref.VariableName, In.RightOperand, Statement, Start, this.pos - Start);
                            }
                            else
                            {
                                Assignment Assignment = this.AssertOperandNotNull(this.ParseList()) as Assignment;
                                if (Assignment == null)
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

                                Statement = this.AssertOperandNotNull(this.ParseList());

                                return new For(Assignment.VariableName, Assignment.Operand, To, Step, Statement, Assignment.Start, this.pos - Start);
                            }

                        default:
                            return this.ParseList();
                    }

                case 'T':
                    if (this.PeekNextToken().ToUpper() == "TRY")
                    {
                        this.pos += 3;

                        ScriptNode Statement = this.AssertOperandNotNull(this.ParseList());

                        this.SkipWhiteSpace();
                        switch (this.PeekNextToken().ToUpper())
                        {
                            case "FINALLY":
                                this.pos += 7;
                                ScriptNode Finally = this.AssertOperandNotNull(this.ParseList());
                                return new TryFinally(Statement, Finally, Start, this.pos - Start);

                            case "CATCH":
                                this.pos += 5;
                                ScriptNode Catch = this.AssertOperandNotNull(this.ParseList());

                                this.SkipWhiteSpace();
                                if (this.PeekNextToken().ToUpper() == "FINALLY")
                                {
                                    this.pos += 7;
                                    Finally = this.AssertOperandNotNull(this.ParseList());
                                    return new TryCatchFinally(Statement, Catch, Finally, Start, this.pos - Start);
                                }
                                else
                                    return new TryCatch(Statement, Catch, Start, this.pos - Start);

                            default:
                                throw new SyntaxException("Expected CATCH or FINALLY.", this.pos, this.script);
                        }
                    }
                    else
                        return this.ParseList();

                default:
                    return this.ParseList();
            }
        }

        private ScriptNode ParseList()
        {
            ScriptNode Node = this.ParseAssignments();
            int Start;

            if (Node == null)   // Allow null
                Start = this.pos;
            else
                Start = Node.Start;

            this.SkipWhiteSpace();
            if (this.PeekNextChar() == ',')
            {
                List<ScriptNode> Elements = new List<ScriptNode>();
                Elements.Add(Node);

                while (this.PeekNextChar() == ',')
                {
                    this.pos++;
                    Node = this.ParseAssignments();
                    Elements.Add(Node);

                    this.SkipWhiteSpace();
                }

                Node = new ElementList(Elements.ToArray(), Start, this.pos - Start);
            }

            return Node;
        }

        private ScriptNode ParseAssignments()
        {
            ScriptNode Left = this.ParseIf();
            if (Left == null)
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
                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());

                        if (Ref != null)
                            return new Assignment(Ref.VariableName, Right, Start, this.pos - Start);
                        else if (Left is NamedMember)
                            return new NamedMemberAssignment((NamedMember)Left, Right, Start, this.pos - Start);
                        else if (Left is DynamicMember)
                            return new DynamicMemberAssignment((DynamicMember)Left, Right, Start, this.pos - Start);
                        else if (Left is VectorIndex)
                            return new VectorIndexAssignment((VectorIndex)Left, Right, Start, this.pos - Start);
                        else if (Left is MatrixIndex)
                            return new MatrixIndexAssignment((MatrixIndex)Left, Right, Start, this.pos - Start);
                        else if (Left is ColumnVector)
                            return new MatrixColumnAssignment((ColumnVector)Left, Right, Start, this.pos - Start);
                        else if (Left is RowVector)
                            return new MatrixRowAssignment((RowVector)Left, Right, Start, this.pos - Start);
                        else if (Left is DynamicIndex)
                            return new DynamicIndexAssignment((DynamicIndex)Left, Right, Start, this.pos - Start);
                        else if (Left is NamedFunctionCall)
                        {
                            NamedFunctionCall f = (NamedFunctionCall)Left;
                            List<string> ArgumentNames = new List<string>();
                            List<ArgumentType> ArgumentTypes = new List<ArgumentType>();
                            ArgumentType ArgumentType;

                            foreach (ScriptNode Argument in f.Arguments)
                            {
                                if (Argument is ToVector)
                                {
                                    ArgumentType = ArgumentType.Vector;

                                    if ((Ref = ((ToVector)Argument).Operand as VariableReference) == null)
                                    {
                                        throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                            Argument.Start, this.script);
                                    }
                                }
                                else if (Argument is ToMatrix)
                                {
                                    ArgumentType = ArgumentType.Matrix;

                                    if ((Ref = ((ToMatrix)Argument).Operand as VariableReference) == null)
                                    {
                                        throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                            Argument.Start, this.script);
                                    }
                                }
                                else if (Argument is ToSet)
                                {
                                    ArgumentType = ArgumentType.Set;

                                    if ((Ref = ((ToSet)Argument).Operand as VariableReference) == null)
                                    {
                                        throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                            Argument.Start, this.script);
                                    }
                                }
                                else if (Argument is VectorDefinition)
                                {
                                    ArgumentType = ArgumentType.Scalar;

                                    VectorDefinition Def = (VectorDefinition)Argument;
                                    if (Def.Elements.Length != 1 || (Ref = Def.Elements[0] as VariableReference) == null)
                                    {
                                        throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                            Argument.Start, this.script);
                                    }
                                }
                                else if ((Ref = Argument as VariableReference) != null)
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

                            return new FunctionDefinition(f.FunctionName, ArgumentNames.ToArray(), ArgumentTypes.ToArray(), Right, Start, this.pos - Start);
                        }
                        else
                            return new PatternMatch(Left, Right, Start, this.pos - Start);
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

                        if (Ref == null)
                            throw new SyntaxException("The += operator can only work on variable references.", this.pos, this.script);

                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                        return new AddToSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                        if (Ref == null)
                            throw new SyntaxException("The -= operator can only work on variable references.", this.pos, this.script);

                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                        return new SubtractFromSelf(Ref.VariableName, Right, Start, this.pos - Start);
                    }
                    else
                    {
                        this.pos--;
                        return Left;
                    }

                case '*':
                    this.pos++;
                    if (this.PeekNextChar() == '=')
                    {
                        this.pos++;

                        if (Ref == null)
                            throw new SyntaxException("The *= operator can only work on variable references.", this.pos, this.script);

                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                        return new MultiplyWithSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                        if (Ref == null)
                            throw new SyntaxException("The /= operator can only work on variable references.", this.pos, this.script);

                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                        return new DivideFromSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                        if (Ref == null)
                            throw new SyntaxException("The ^= operator can only work on variable references.", this.pos, this.script);

                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                        return new PowerOfSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                            if (Ref == null)
                                throw new SyntaxException("The &= operator can only work on variable references.", this.pos, this.script);

                            ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                            return new BinaryAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start);

                        case '&':
                            this.pos++;
                            if (this.PeekNextChar() == '=')
                            {
                                this.pos++;

                                if (Ref == null)
                                    throw new SyntaxException("The &&= operator can only work on variable references.", this.pos, this.script);

                                Right = this.AssertRightOperandNotNull(this.ParseIf());
                                return new LogicalAndWithSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                            if (Ref == null)
                                throw new SyntaxException("The |= operator can only work on variable references.", this.pos, this.script);

                            ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                            return new BinaryOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start);

                        case '|':
                            this.pos++;
                            if (this.PeekNextChar() == '=')
                            {
                                this.pos++;

                                if (Ref == null)
                                    throw new SyntaxException("The ||= operator can only work on variable references.", this.pos, this.script);

                                Right = this.AssertRightOperandNotNull(this.ParseIf());
                                return new LogicalOrWithSelf(Ref.VariableName, Right, Start, this.pos - Start);
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

                            if (Ref == null)
                                throw new SyntaxException("The <<= operator can only work on variable references.", this.pos, this.script);

                            ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                            return new ShiftSelfLeft(Ref.VariableName, Right, Start, this.pos - Start);
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

                            if (Ref == null)
                                throw new SyntaxException("The >>= operator can only work on variable references.", this.pos, this.script);

                            ScriptNode Right = this.AssertRightOperandNotNull(this.ParseIf());
                            return new ShiftSelfRight(Ref.VariableName, Right, Start, this.pos - Start);
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

        private ScriptNode ParseIf()
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

                Condition = this.AssertOperandNotNull(this.ParseLambdaExpression());

                this.SkipWhiteSpace();
                if (this.PeekNextToken().ToUpper() == "THEN")
                    this.pos += 4;

                IfTrue = this.AssertOperandNotNull(this.ParseLambdaExpression());

                this.SkipWhiteSpace();
                if (this.PeekNextToken().ToUpper() == "ELSE")
                {
                    this.pos += 4;
                    IfFalse = this.AssertOperandNotNull(this.ParseLambdaExpression());
                }
                else
                    IfFalse = null;
            }
            else
            {
                Condition = this.ParseLambdaExpression();
                if (Condition == null)
                    return null;

                this.SkipWhiteSpace();
                if (this.PeekNextChar() != '?')
                    return Condition;

                this.pos++;
                IfTrue = this.AssertOperandNotNull(this.ParseLambdaExpression());

                this.SkipWhiteSpace();
                if (this.PeekNextChar() == ':')
                {
                    this.pos++;
                    IfFalse = this.AssertOperandNotNull(this.ParseLambdaExpression());
                }
                else
                    IfFalse = null;
            }

            return new If(Condition, IfTrue, IfFalse, Start, this.pos - Start);
        }

        private ScriptNode ParseLambdaExpression()
        {
            ScriptNode Left = this.ParseEquivalence();
            if (Left == null)
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
                    VariableReference Ref;

                    if ((Ref = Left as VariableReference) != null)
                    {
                        ArgumentNames = new string[] { Ref.VariableName };
                        ArgumentTypes = new ArgumentType[] { ArgumentType.Normal };
                    }
                    else if (Left is ToVector)
                    {
                        Ref = ((ToVector)Left).Operand as VariableReference;
                        if (Ref == null)
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
                        if (Ref == null)
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
                        if (Ref == null)
                        {
                            throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                Left.Start, this.script);
                        }

                        ArgumentNames = new string[] { Ref.VariableName };
                        ArgumentTypes = new ArgumentType[] { ArgumentType.Set };
                    }
                    else if (Left is VectorDefinition)
                    {
                        VectorDefinition Def = (VectorDefinition)Left;
                        if (Def.Elements.Length != 1 || (Ref = Def.Elements[0] as VariableReference) == null)
                        {
                            throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                Left.Start, this.script);
                        }

                        ArgumentNames = new string[] { Ref.VariableName };
                        ArgumentTypes = new ArgumentType[] { ArgumentType.Scalar };
                    }
                    else if (Left is ElementList)
                    {
                        ElementList List = (ElementList)Left;
                        int i, c = List.Elements.Length;
                        ScriptNode Argument;

                        ArgumentNames = new string[c];
                        ArgumentTypes = new ArgumentType[c];

                        for (i = 0; i < c; i++)
                        {
                            Argument = List.Elements[i];

                            if ((Ref = Argument as VariableReference) != null)
                                ArgumentTypes[i] = ArgumentType.Normal;
                            else if (Argument is ToVector)
                            {
                                Ref = ((ToVector)Argument).Operand as VariableReference;
                                if (Ref == null)
                                {
                                    throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                        Argument.Start, this.script);
                                }

                                ArgumentTypes[i] = ArgumentType.Vector;
                            }
                            else if (Argument is ToMatrix)
                            {
                                Ref = ((ToMatrix)Argument).Operand as VariableReference;
                                if (Ref == null)
                                {
                                    throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                        Argument.Start, this.script);
                                }

                                ArgumentTypes[i] = ArgumentType.Matrix;
                            }
                            else if (Argument is ToSet)
                            {
                                Ref = ((ToSet)Argument).Operand as VariableReference;
                                if (Ref == null)
                                {
                                    throw new SyntaxException("Expected variable reference, with optional scalar, vector, set or matrix attribute types.",
                                        Argument.Start, this.script);
                                }

                                ArgumentTypes[i] = ArgumentType.Set;
                            }
                            else if (Left is VectorDefinition)
                            {
                                VectorDefinition Def = (VectorDefinition)Left;
                                if (Def.Elements.Length != 1 || (Ref = Def.Elements[0] as VariableReference) == null)
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
                    if (Operand == null)
                        throw new SyntaxException("Lambda function body missing.", this.pos, this.script);

                    return new LambdaDefinition(ArgumentNames, ArgumentTypes, Operand, Start, this.pos - Start);
                }

                this.pos--;
            }

            return Left;
        }

        private ScriptNode ParseEquivalence()
        {
            ScriptNode Left = this.ParseOrs();
            if (Left == null)
                return null;

            int Start = Left.Start;
            char ch;

            this.SkipWhiteSpace();

            if ((ch = this.PeekNextChar()) == '-' || ch == '=')
            {
                int Bak = this.pos;

                this.pos++;
                if (this.PeekNextChar() == '>')
                {
                    this.pos++;
                    ScriptNode Right = this.AssertRightOperandNotNull(this.ParseOrs());
                    return new Implication(Left, Right, Start, this.pos - Start);
                }

                this.pos = Bak;
            }
            else if (ch == '<')
            {
                int Bak = this.pos;

                this.pos++;
                if ((ch = this.PeekNextChar()) == '-' || ch == '=')
                {
                    this.pos++;
                    if (this.PeekNextChar() == '>')
                    {
                        this.pos++;
                        ScriptNode Right = this.AssertRightOperandNotNull(this.ParseOrs());
                        return new Equivalence(Left, Right, Start, this.pos - Start);
                    }
                }

                this.pos = Bak;
            }

            return Left;
        }

        private ScriptNode ParseOrs()
        {
            ScriptNode Left = this.ParseAnds();
            if (Left == null)
                return null;

            ScriptNode Right;
            int Start = Left.Start;

            while (true)
            {
                this.SkipWhiteSpace();
                switch (char.ToUpper(this.PeekNextChar()))
                {
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
                                Left = new Operators.Logical.Or(Left, Right, Start, this.pos - Start);
                                break;

                            case '=':
                                this.pos--;
                                return Left;

                            default:
                                Right = this.AssertRightOperandNotNull(this.ParseAnds());
                                Left = new Operators.Binary.Or(Left, Right, Start, this.pos - Start);
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
                                Left = new Operators.Dual.Or(Left, Right, Start, this.pos - Start);
                                continue;

                            case "XOR":
                                this.pos += 3;
                                Right = this.AssertRightOperandNotNull(this.ParseAnds());
                                Left = new Operators.Dual.Xor(Left, Right, Start, this.pos - Start);
                                continue;

                            case "XNOR":
                                this.pos += 4;
                                Right = this.AssertRightOperandNotNull(this.ParseAnds());
                                Left = new Operators.Dual.Xnor(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NOR":
                                this.pos += 3;
                                Right = this.AssertRightOperandNotNull(this.ParseAnds());
                                Left = new Operators.Dual.Nor(Left, Right, Start, this.pos - Start);
                                continue;

                            default:
                                return Left;
                        }

                    default:
                        return Left;
                }
            }
        }

        private ScriptNode ParseAnds()
        {
            ScriptNode Left = this.ParseMembership();
            if (Left == null)
                return null;

            ScriptNode Right;
            int Start = Left.Start;

            while (true)
            {
                this.SkipWhiteSpace();
                switch (char.ToUpper(this.PeekNextChar()))
                {
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
                                Left = new Operators.Logical.And(Left, Right, Start, this.pos - Start);
                                break;

                            case '=':
                                this.pos--;
                                return Left;

                            default:
                                Right = this.AssertRightOperandNotNull(this.ParseMembership());
                                Left = new Operators.Binary.And(Left, Right, Start, this.pos - Start);
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
                                Left = new Operators.Dual.And(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NAND":
                                this.pos += 4;
                                Right = this.AssertRightOperandNotNull(this.ParseMembership());
                                Left = new Operators.Dual.Nand(Left, Right, Start, this.pos - Start);
                                continue;

                            default:
                                return Left;
                        }

                    default:
                        return Left;
                }
            }
        }

        private ScriptNode ParseMembership()
        {
            ScriptNode Left = this.ParseComparison();
            if (Left == null)
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
                        switch (this.PeekNextToken().ToUpper())
                        {
                            case "IS":
                                this.pos += 2;
                                Right = this.AssertRightOperandNotNull(this.ParseComparison());
                                Left = new Is(Left, Right, Start, this.pos - Start);
                                continue;

                            case "AS":
                                this.pos += 2;
                                Right = this.AssertRightOperandNotNull(this.ParseComparison());
                                Left = new As(Left, Right, Start, this.pos - Start);
                                continue;

                            case "IN":
                                this.pos += 2;
                                Right = this.AssertRightOperandNotNull(this.ParseComparison());
                                Left = new In(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NOTIN":
                                this.pos += 5;
                                Right = this.AssertRightOperandNotNull(this.ParseComparison());
                                Left = new NotIn(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NOT":
                                int Bak = this.pos;
                                this.pos += 3;

                                this.SkipWhiteSpace();
                                if (this.PeekNextToken().ToUpper() == "IN")
                                {
                                    this.pos += 2;
                                    Right = this.AssertRightOperandNotNull(this.ParseComparison());
                                    Left = new NotIn(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParseComparison()
        {
            ScriptNode Left = this.ParseShifts();
            if (Left == null)
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
                                Left = new LesserThanOrEqualTo(Left, Right, Start, this.pos - Start);
                            }
                        }
                        else if (ch == '>')
                        {
                            this.pos++;
                            Right = this.AssertRightOperandNotNull(this.ParseShifts());
                            Left = new NotEqualTo(Left, Right, Start, this.pos - Start);
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
                                Left = new LesserThan(Left, Right, Start, this.pos - Start);
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
                            Left = new LesserThan(Left, Right, Start, this.pos - Start);
                        }
                        break;

                    case '>':
                        this.pos++;
                        if ((ch = this.PeekNextChar()) == '=')
                        {
                            this.pos++;
                            Right = this.AssertRightOperandNotNull(this.ParseShifts());
                            Left = new GreaterThanOrEqualTo(Left, Right, Start, this.pos - Start);
                        }
                        else if (ch == '>')
                        {
                            this.pos--;
                            return Left;
                        }
                        else
                        {
                            Right = this.AssertRightOperandNotNull(this.ParseShifts());
                            Left = new GreaterThan(Left, Right, Start, this.pos - Start);
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
                                Left = new IdenticalTo(Left, Right, Start, this.pos - Start);
                            }
                            else
                            {
                                Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                Left = new EqualTo(Left, Right, Start, this.pos - Start);
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
                            Left = new EqualTo(Left, Right, Start, this.pos - Start);
                        }
                        break;

                    case '!':
                        this.pos++;
                        if (this.PeekNextChar() == '=')
                        {
                            this.pos++;
                            Right = this.AssertRightOperandNotNull(this.ParseShifts());
                            Left = new NotEqualTo(Left, Right, Start, this.pos - Start);
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
                                        Left = new IdenticalToElementWise(Left, Right, Start, this.pos - Start);
                                    }
                                    else
                                    {
                                        Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                        Left = new EqualToElementWise(Left, Right, Start, this.pos - Start);
                                    }
                                }
                                else
                                {
                                    Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                    Left = new EqualToElementWise(Left, Right, Start, this.pos - Start);
                                }
                                continue;

                            case '<':
                                this.pos++;
                                if (this.PeekNextChar() == '>')
                                {
                                    this.pos++;
                                    Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                    Left = new NotEqualToElementWise(Left, Right, Start, this.pos - Start);
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
                                    Left = new NotEqualToElementWise(Left, Right, Start, this.pos - Start);
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
                                Left = new Like(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NOTLIKE":
                                this.pos += 7;
                                Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                Left = new NotLike(Left, Right, Start, this.pos - Start);
                                continue;

                            case "UNLIKE":
                                this.pos += 6;
                                Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                Left = new NotLike(Left, Right, Start, this.pos - Start);
                                continue;

                            case "NOT":
                                int Bak = this.pos;
                                this.pos += 3;
                                this.SkipWhiteSpace();
                                if (this.PeekNextToken().ToUpper() == "LIKE")
                                {
                                    this.pos += 4;
                                    Right = this.AssertRightOperandNotNull(this.ParseShifts());
                                    Left = new NotLike(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParseShifts()
        {
            ScriptNode Left = this.ParseUnions();
            if (Left == null)
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
                            Left = new ShiftLeft(Left, Right, Start, this.pos - Start);
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
                            Left = new ShiftRight(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParseUnions()
        {
            ScriptNode Left = this.ParseIntersections();
            if (Left == null)
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
                        Left = new Union(Left, Right, Start, this.pos - Start);
                    }
                    else
                        return Left;
                }
                else if (ch == '∪')
                {
                    this.pos++;
                    Right = this.AssertRightOperandNotNull(this.ParseIntersections());
                    Left = new Union(Left, Right, Start, this.pos - Start);
                }
                else
                    return Left;
            }
        }

        private ScriptNode ParseIntersections()
        {
            ScriptNode Left = this.ParseInterval();
            if (Left == null)
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
                            Left = new Intersection(Left, Right, Start, this.pos - Start);
                            continue;

                        case "INTERSECT":
                            this.pos += 9;
                            Right = this.AssertRightOperandNotNull(this.ParseInterval());
                            Left = new Intersection(Left, Right, Start, this.pos - Start);
                            continue;

                        default:
                            return Left;
                    }
                }
                else if (ch == '∩')
                {
                    this.pos++;
                    Right = this.AssertRightOperandNotNull(this.ParseInterval());
                    Left = new Intersection(Left, Right, Start, this.pos - Start);
                }
                else
                    return Left;
            }
        }

        private ScriptNode ParseInterval()
        {
            ScriptNode From = this.ParseTerms();
            if (From == null)
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
                return new Interval(From, To, StepSize, Start, this.pos - Start);
            }
            else
                return new Interval(From, To, Start, this.pos - Start);
        }

        private ScriptNode ParseTerms()
        {
            ScriptNode Left = this.ParseBinomialCoefficients();
            if (Left == null)
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
                        Left = new Add(Left, Right, Start, this.pos - Start);
                        continue;

                    case '-':
                        this.pos++;
                        if ((ch = this.PeekNextChar()) == '=' || ch == '>' || ch == '-')
                        {
                            this.pos--;
                            return Left;
                        }

                        Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
                        Left = new Subtract(Left, Right, Start, this.pos - Start);
                        continue;

                    case '.':
                        this.pos++;
                        switch (this.PeekNextChar())
                        {
                            case '+':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
                                Left = new AddElementWise(Left, Right, Start, this.pos - Start);
                                continue;

                            case '-':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParseBinomialCoefficients());
                                Left = new SubtractElementWise(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParseBinomialCoefficients()
        {
            ScriptNode Left = this.ParseFactors();
            if (Left == null)
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
                    Left = new BinomialCoefficient(Left, Right, Start, this.pos - Start);
                }
                else
                    return Left;
            }
        }

        private ScriptNode ParseFactors()
        {
            ScriptNode Left = this.ParsePowers();
            if (Left == null)
                return null;

            ScriptNode Right;
            int Start = Left.Start;

            while (true)
            {
                this.SkipWhiteSpace();
                switch (this.PeekNextChar())
                {
                    case '*':
                        this.pos++;
                        if (this.PeekNextChar() == '=')
                        {
                            this.pos--;
                            return Left;
                        }

                        Right = this.AssertRightOperandNotNull(this.ParsePowers());
                        Left = new Multiply(Left, Right, Start, this.pos - Start);
                        continue;

                    case '/':
                        this.pos++;
                        if (this.PeekNextChar() == '=')
                        {
                            this.pos--;
                            return Left;
                        }

                        Right = this.AssertRightOperandNotNull(this.ParsePowers());
                        Left = new Divide(Left, Right, Start, this.pos - Start);
                        continue;

                    case '\\':
                        this.pos++;
                        Right = this.AssertRightOperandNotNull(this.ParsePowers());
                        Left = new LeftDivide(Left, Right, Start, this.pos - Start);
                        continue;

                    case 'C':
                        switch (this.PeekNextToken().ToUpper())
                        {
                            case "CROSS":
                                this.pos += 5;
                                Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                Left = new CrossProduct(Left, Right, Start, this.pos - Start);
                                continue;

                            case "CARTESIAN":
                                this.pos += 9;
                                Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                Left = new CartesianProduct(Left, Right, Start, this.pos - Start);
                                continue;

                            default:
                                return Left;
                        }

                    case 'D':
                        if (this.PeekNextToken().ToUpper() == "DOT")
                        {
                            this.pos += 3;
                            Right = this.AssertRightOperandNotNull(this.ParsePowers());
                            Left = new DotProduct(Left, Right, Start, this.pos - Start);
                            continue;
                        }
                        else
                            return Left;

                    case 'M':
                        if (this.PeekNextToken().ToUpper() == "MOD")
                        {
                            this.pos += 3;
                            Right = this.AssertRightOperandNotNull(this.ParsePowers());
                            Left = new Residue(Left, Right, Start, this.pos - Start);
                            continue;
                        }
                        else
                            return Left;

                    case '.':
                        this.pos++;
                        switch (this.PeekNextChar())
                        {
                            case '*':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                Left = new MultiplyElementWise(Left, Right, Start, this.pos - Start);
                                continue;

                            case '/':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                Left = new DivideElementWise(Left, Right, Start, this.pos - Start);
                                continue;

                            case '\\':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                Left = new LeftDivideElementWise(Left, Right, Start, this.pos - Start);
                                continue;

                            case 'M':
                                if (this.PeekNextToken().ToUpper() == "MOD")
                                {
                                    this.pos += 3;
                                    Right = this.AssertRightOperandNotNull(this.ParsePowers());
                                    Left = new ResidueElementWise(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParsePowers()
        {
            ScriptNode Left = this.ParseUnaryPrefixOperator();
            if (Left == null)
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
                        Left = new Power(Left, Right, Start, this.pos - Start);
                        continue;

                    case '²':
                        this.pos++;
                        Left = new Square(Left, Start, this.pos - Start);
                        continue;

                    case '³':
                        this.pos++;
                        Left = new Cube(Left, Start, this.pos - Start);
                        continue;

                    case '.':
                        this.pos++;
                        switch (this.PeekNextChar())
                        {
                            case '^':
                                this.pos++;
                                Right = this.AssertRightOperandNotNull(this.ParseUnaryPrefixOperator());
                                Left = new PowerElementWise(Left, Right, Start, this.pos - Start);
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

        private ScriptNode ParseUnaryPrefixOperator()
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
                        VariableReference Ref = this.ParseUnaryPrefixOperator() as VariableReference;
                        if (Ref == null)
                            throw new SyntaxException("The -- operator can only work on variable references.", this.pos, this.script);

                        return new PreDecrement(Ref.VariableName, Start, this.pos - Start);
                    }
                    else if ((ch >= '0' && ch <= '9') || (ch == '.'))
                        return this.ParseUnarySuffixOperator();
                    else if (ch == '>')
                    {
                        this.pos--;
                        return this.ParseUnarySuffixOperator();
                    }
                    else
                        return new Negate(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start);

                case '+':
                    this.pos++;
                    if ((ch = this.PeekNextChar()) == '+')
                    {
                        this.pos++;
                        VariableReference Ref = this.ParseUnaryPrefixOperator() as VariableReference;
                        if (Ref == null)
                            throw new SyntaxException("The ++ operator can only work on variable references.", this.pos, this.script);

                        return new PreIncrement(Ref.VariableName, Start, this.pos - Start);
                    }
                    else if ((ch >= '0' && ch <= '9') || (ch == '.'))
                        return this.ParseUnarySuffixOperator();
                    else
                        return this.AssertOperandNotNull(this.ParseUnaryPrefixOperator());

                case '!':
                    this.pos++;
                    return new Not(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start);

                case 'N':
                    if (this.PeekNextToken().ToUpper() == "NOT")
                    {
                        this.pos += 3;
                        return new Not(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start);
                    }
                    else
                        return this.ParseUnarySuffixOperator();

                case '~':
                    this.pos++;
                    return new Complement(this.AssertOperandNotNull(this.ParseUnaryPrefixOperator()), Start, this.pos - Start);

                default:
                    return this.ParseUnarySuffixOperator();
            }
        }

        private ScriptNode ParseUnarySuffixOperator()
        {
            ScriptNode Node = this.ParseObject();
            if (Node == null)
                return null;

            int Start = Node.Start;

            while (true)
            {
                this.SkipWhiteSpace();
                switch (this.PeekNextChar())
                {
                    case '.':
                        this.pos++;

                        char ch = this.PeekNextChar();
                        if (ch == '=' || ch == '+' || ch == '-' || ch == '^' || ch == '.' || ch == '*' || ch == '/' || ch == '\\' || ch == '<' || ch == '!')
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

                        if (Ref == null)
                            Node = new DynamicMember(Node, Right, Start, this.pos - Start);
                        else
                            Node = new NamedMember(Node, Ref.VariableName, Start, this.pos - Start);

                        continue;

                    case '(':
                        this.pos++;
                        Right = this.ParseList();
                        Ref = Node as VariableReference;
                        if (Ref == null)
                        {
                            if (Right is ElementList)
                                Node = new DynamicFunctionCall(Node, ((ElementList)Right).Elements, Start, this.pos - Start);
                            else if (Right == null)
                                Node = new DynamicFunctionCall(Node, new ScriptNode[0], Start, this.pos - Start);
                            else
                                Node = new DynamicFunctionCall(Node, new ScriptNode[] { Right }, Start, this.pos - Start);
                        }
                        else
                            Node = GetFunction(Ref.VariableName, Right, Start, this.pos - Start);

                        this.SkipWhiteSpace();
                        if (this.PeekNextChar() != ')')
                            throw new SyntaxException("Expected ).", this.pos, this.script);

                        this.pos++;
                        continue;

                    case '[':
                        this.pos++;
                        Right = this.ParseList();
                        if (Right == null)
                            Node = new ToVector(Node, Start, this.pos - Start);
                        else if (Right is ElementList)
                        {
                            ElementList List = (ElementList)Right;

                            if (List.Elements.Length == 2)
                            {
                                if (List.Elements[0] == null)
                                {
                                    if (List.Elements[1] == null)
                                        Node = new ToMatrix(Node, Start, this.pos - Start);
                                    else
                                        Node = new RowVector(Node, List.Elements[1], Start, this.pos - Start);
                                }
                                else if (List.Elements[1] == null)
                                    Node = new ColumnVector(Node, List.Elements[0], Start, this.pos - Start);
                                else
                                    Node = new MatrixIndex(Node, List.Elements[0], List.Elements[1], Start, this.pos - Start);
                            }
                            else
                                Node = new DynamicIndex(Node, List, Start, this.pos - Start);
                        }
                        else
                            Node = new VectorIndex(Node, Right, Start, this.pos - Start);

                        this.SkipWhiteSpace();
                        if (this.PeekNextChar() != ']')
                            throw new SyntaxException("Expected ].", this.pos, this.script);

                        this.pos++;
                        continue;

                    case '{':
                        int Bak = this.pos;
                        this.pos++;
                        this.SkipWhiteSpace();
                        if (this.PeekNextChar() == '}')
                        {
                            this.pos++;
                            Node = new ToSet(Node, Start, this.pos - Start);
                            continue;
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
                            if (Ref == null)
                            {
                                this.pos -= 2;  // Can be a prefix operator.
                                return Node;
                            }

                            Node = new PostIncrement(Ref.VariableName, Start, this.pos - Start);
                            continue;
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
                            if (Ref == null)
                            {
                                this.pos -= 2;  // Can be a prefix operator.
                                return Node;
                            }

                            Node = new PostDecrement(Ref.VariableName, Start, this.pos - Start);
                            continue;
                        }
                        else
                        {
                            this.pos--;
                            return Node;
                        }

                    case '%':
                        this.pos++;
                        Node = new Percent(Node, Start, this.pos - Start);
                        continue;

                    case '‰':
                        this.pos++;
                        Node = new Permil(Node, Start, this.pos - Start);
                        continue;

                    case '‱':
                        this.pos++;
                        Node = new Perdiezmil(Node, Start, this.pos - Start);
                        continue;

                    case '°':
                        this.pos++;
                        Node = new DegToRad(Node, Start, this.pos - Start);
                        continue;

                    case '\'':
                    case '"':
                    case '′':
                    case '″':
                    case '‴':
                        int i = 0;

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

                        Node = new DefaultDifferentiation(Node, i, Start, this.pos - Start);
                        continue;

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
                            Node = new Transpose(Node, Start, this.pos - Start);
                            continue;
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
                            Node = new ConjugateTranspose(Node, Start, this.pos - Start);
                            continue;
                        }

                    case '†':
                        this.pos++;
                        Node = new ConjugateTranspose(Node, Start, this.pos - Start);
                        continue;

                    case '!':
                        this.pos++;
                        switch (this.PeekNextChar())
                        {
                            case '!':
                                this.pos++;
                                Node = new SemiFaculty(Node, Start, this.pos - Start);
                                continue;

                            case '=':
                                this.pos--;
                                return Node;

                            default:
                                Node = new Faculty(Node, Start, this.pos - Start);
                                continue;
                        }

                    default:
                        // TODO: Physical units.
                        return Node;
                }
            }
        }

        private static ScriptNode GetFunction(string FunctionName, ScriptNode Arguments, int Start, int Length)
        {
            Dictionary<string, FunctionRef> F;
            FunctionRef Ref;
            int NrParameters;
            ElementList ElementList;
            object[] P;

            if ((ElementList = Arguments as ElementList) != null)
            {
                NrParameters = ElementList.Elements.Length;
                P = new object[NrParameters + 2];
                ElementList.Elements.CopyTo(P, 0);
            }
            else if (Arguments == null)
            {
                NrParameters = 0;
                P = new object[2];
            }
            else
            {
                NrParameters = 1;
                P = new object[3];
                P[0] = Arguments;
            }

            P[NrParameters] = Start;
            P[NrParameters + 1] = Length;

            F = functions;
            if (F == null)
            {
                Search();
                F = functions;
            }

            if (F.TryGetValue(FunctionName + " " + NrParameters.ToString(), out Ref))
                return (Function)Ref.Constructor.Invoke(P);
            else
            {
                if (ElementList != null)
                    return new NamedFunctionCall(FunctionName, ElementList.Elements, Start, Length);
                else if (Arguments == null)
                    return new NamedFunctionCall(FunctionName, new ScriptNode[0], Start, Length);
                else
                    return new NamedFunctionCall(FunctionName, new ScriptNode[] { Arguments }, Start, Length);
            }
        }

        internal static bool TryGetConstant(string Name, out IElement ValueElement)
        {
            Dictionary<string, IConstant> C = constants;
            if (C == null)
            {
                Search();
                C = constants;
            }

            IConstant Constant;

            if (!C.TryGetValue(Name, out Constant))
            {
                ValueElement = null;
                return false;
            }

            ValueElement = Constant.ValueElement;
            return true;
        }

        internal static LambdaDefinition GetFunctionLambdaDefinition(string FunctionName, int Start, int Length)
        {
            Dictionary<string, FunctionRef> F;
            FunctionRef Ref;

            F = functions;
            if (F == null)
            {
                Search();
                F = functions;
            }

            if (F.TryGetValue(FunctionName, out Ref))
            {
                string[] ArgumentNames = Ref.Function.DefaultArgumentNames;
                int i, c = ArgumentNames.Length;
                ArgumentType[] ArgumentTypes = new ArgumentType[c];
                object[] Arguments = new ScriptNode[c + 2];

                Arguments[c] = Start;
                Arguments[c + 1] = Length;

                for (i = 0; i < c; i++)
                {
                    Arguments[i] = new VariableReference(ArgumentNames[i], Start, Length);
                    ArgumentTypes[i] = ArgumentType.Normal;
                }

                ScriptNode FunctionCall = (ScriptNode)Ref.Constructor.Invoke(Arguments);

                return new LambdaDefinition(ArgumentNames, ArgumentTypes, FunctionCall, Start, Length);
            }
            else
                return null;
        }

        private static void Search()
        {
            lock (searchSynch)
            {
                if (functions == null)
                {
                    Dictionary<int, object[]> ParameterValuesPerNrParameters = new Dictionary<int, object[]>();
                    Dictionary<string, FunctionRef> Found = new Dictionary<string, FunctionRef>(Types.CaseInsensitiveComparer);
                    ParameterInfo[] Parameters;
                    ParameterInfo PInfo;
                    FunctionRef Ref;
                    object[] ParameterValues;
                    string[] Aliases;
                    Function Function;
                    string s;
                    int i, c;

                    foreach (Type T in Types.GetTypesImplementingInterface(typeof(IFunction)))
                    {
                        if (T.IsAbstract)
                            continue;

                        foreach (ConstructorInfo CI in T.GetConstructors())
                        {
                            Parameters = CI.GetParameters();
                            c = Parameters.Length;
                            if (c < 2)
                                continue;

                            PInfo = Parameters[c - 1];
                            if (!PInfo.IsIn || PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(int))
                                continue;

                            PInfo = Parameters[c - 2];
                            if (!PInfo.IsIn || PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(int))
                                continue;

                            for (i = c - 3; i >= 0; i--)
                            {
                                PInfo = Parameters[i];
                                if (!PInfo.IsIn || PInfo.IsOut || PInfo.IsRetval || PInfo.IsOptional || PInfo.ParameterType != typeof(int))
                                    break;
                            }

                            if (i >= 0)
                                continue;

                            try
                            {
                                if (!ParameterValuesPerNrParameters.TryGetValue(c, out ParameterValues))
                                {
                                    ParameterValues = new object[c];
                                    ParameterValues[c - 1] = 0;
                                    ParameterValues[c - 2] = 0;
                                    ParameterValuesPerNrParameters[c] = ParameterValues;
                                }

                                Function = CI.Invoke(ParameterValues) as Function;
                                if (Function == null)
                                    continue;

                                s = Function.FunctionName + " " + (c - 2).ToString();
                                if (Found.ContainsKey(s))
                                {
                                    Log.Warning("Function with name " + Function.FunctionName + " and " + (c - 2).ToString() +
                                        " parameters previously registered. Function ignored.",
                                        T.FullName, new KeyValuePair<string, object>("Previous", Found[s].Function.GetType().FullName));
                                }
                                else
                                {
                                    Ref = new FunctionRef();
                                    Ref.Constructor = CI;
                                    Ref.Function = Function;
                                    Ref.NrParameters = c - 2;

                                    Found[s] = Ref;

                                    if (!Found.ContainsKey(Function.FunctionName))
                                        Found[Function.FunctionName] = Ref;
                                }

                                Aliases = Function.Aliases;
                                if (Aliases != null)
                                {
                                    foreach (string Alias in Aliases)
                                    {
                                        s = Alias + " " + (c - 2).ToString();
                                        if (Found.ContainsKey(s))
                                        {
                                            Log.Warning("Function with name " + Alias + " and " + (c - 2).ToString() +
                                                " parameters previously registered. Function ignored.",
                                                T.FullName, new KeyValuePair<string, object>("Previous", Found[s].Function.GetType().FullName));
                                        }
                                        else
                                        {
                                            Ref = new FunctionRef();
                                            Ref.Constructor = CI;
                                            Ref.Function = Function;
                                            Ref.NrParameters = c - 2;

                                            Found[s] = Ref;

                                            if (!Found.ContainsKey(Alias))
                                                Found[Alias] = Ref;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Critical(ex);
                            }
                        }
                    }

                    functions = Found;
                }

                if (constants == null)
                {
                    Dictionary<string, IConstant> Found = new Dictionary<string, IConstant>(Types.CaseInsensitiveComparer);
                    string[] Aliases;
                    string s;

                    foreach (Type T in Types.GetTypesImplementingInterface(typeof(IConstant)))
                    {
                        if (T.IsAbstract)
                            continue;

                        ConstructorInfo CI = T.GetConstructor(Types.NoTypes);
                        if (CI == null)
                            continue;

                        try
                        {
                            IConstant Constant = (IConstant)CI.Invoke(Types.NoParameters);

                            s = Constant.ConstantName;
                            if (Found.ContainsKey(s))
                            {
                                Log.Warning("Constant with name " + s + " previously registered. Constant ignored.",
                                    T.FullName, new KeyValuePair<string, object>("Previous", Constant.GetType().FullName));
                            }
                            else
                                Found[s] = Constant;

                            Aliases = Constant.Aliases;
                            if (Aliases != null)
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
            }
        }

        private class FunctionRef
        {
            public ConstructorInfo Constructor;
            public Function Function;
            public int NrParameters;
        }

        private static Dictionary<string, FunctionRef> functions = null;
        private static Dictionary<string, IConstant> constants = null;
        private static object searchSynch = new object();

        private ScriptNode ParseObject()
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

                return Node;
            }
            else if (ch == '[')
            {
                this.pos++;
                Node = this.ParseStatement();

                this.SkipWhiteSpace();
                if (this.PeekNextChar() != ']')
                    throw new SyntaxException("Expected ].", this.pos, this.script);

                this.pos++;

                if (Node is For)
                {
                    For For = (For)Node;
                    if (IsVectorDefinition(For.RightOperand))
                        return new MatrixForDefinition(For, Start, this.pos - Start);
                    else
                        return new VectorForDefinition(For, Start, this.pos - Start);
                }
                else if (Node is ForEach)
                {
                    ForEach ForEach = (ForEach)Node;
                    if (IsVectorDefinition(ForEach.RightOperand))
                        return new MatrixForEachDefinition(ForEach, Start, this.pos - Start);
                    else
                        return new VectorForEachDefinition(ForEach, Start, this.pos - Start);
                }
                else if (Node is DoWhile)
                {
                    DoWhile DoWhile = (DoWhile)Node;
                    if (IsVectorDefinition(DoWhile.LeftOperand))
                        return new MatrixDoWhileDefinition(DoWhile, Start, this.pos - Start);
                    else
                        return new VectorDoWhileDefinition(DoWhile, Start, this.pos - Start);
                }
                else if (Node is WhileDo)
                {
                    WhileDo WhileDo = (WhileDo)Node;
                    if (IsVectorDefinition(WhileDo.RightOperand))
                        return new MatrixWhileDoDefinition(WhileDo, Start, this.pos - Start);
                    else
                        return new VectorWhileDoDefinition(WhileDo, Start, this.pos - Start);
                }
                else if (Node is ElementList)
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
                        return new MatrixDefinition(((ElementList)Node).Elements, Start, this.pos - Start);
                    else
                        return new VectorDefinition(((ElementList)Node).Elements, Start, this.pos - Start);
                }
                else if (IsVectorDefinition(Node))
                    return new MatrixDefinition(new ScriptNode[] { Node }, Start, this.pos - Start);
                else
                    return new VectorDefinition(new ScriptNode[] { Node }, Start, this.pos - Start);
            }
            else if (ch == '{')
            {
                this.pos++;
                Node = this.ParseStatement();

                this.SkipWhiteSpace();
                if ((ch = this.PeekNextChar()) == ':')
                {
                    LinkedList<KeyValuePair<string, ScriptNode>> Members = new LinkedList<KeyValuePair<string, ScriptNode>>();
                    Dictionary<string, bool> MembersFound = new Dictionary<string, bool>();
                    ConstantElement ConstantElement;
                    StringValue StringValue;
                    string s;

                    if (Node is VariableReference)
                        s = ((VariableReference)Node).VariableName;
                    else if ((ConstantElement = Node as ConstantElement) != null && (StringValue = ConstantElement.Constant as StringValue) != null)
                        s = StringValue.Value;
                    else
                        throw new SyntaxException("Expected a variable reference or a string constant.", this.pos, this.script);

                    this.pos++;
                    MembersFound[s] = true;
                    Members.AddLast(new KeyValuePair<string, ScriptNode>(s, this.ParseIf()));

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
                        else if ((ConstantElement = Node as ConstantElement) != null && (StringValue = ConstantElement.Constant as StringValue) != null)
                            s = StringValue.Value;
                        else
                            throw new SyntaxException("Expected a variable reference or a string constant.", this.pos, this.script);

                        if (MembersFound.ContainsKey(s))
                            throw new SyntaxException("Member already defined.", this.pos, this.script);

                        this.pos++;
                        MembersFound[s] = true;
                        Members.AddLast(new KeyValuePair<string, ScriptNode>(s, this.ParseIf()));

                        this.SkipWhiteSpace();
                    }

                    if (ch != '}')
                        throw new SyntaxException("Expected }.", this.pos, this.script);

                    this.pos++;
                    return new ObjectExNihilo(Members, Start, this.pos - Start);
                }

                if (ch != '}')
                    throw new SyntaxException("Expected }.", this.pos, this.script);

                this.pos++;

                if (Node is For)
                    return new SetForDefinition((For)Node, Start, this.pos - Start);
                else if (Node is ForEach)
                    return new SetForEachDefinition((ForEach)Node, Start, this.pos - Start);
                else if (Node is DoWhile)
                    return new SetDoWhileDefinition((DoWhile)Node, Start, this.pos - Start);
                else if (Node is WhileDo)
                    return new SetWhileDoDefinition((WhileDo)Node, Start, this.pos - Start);
                else if (Node is ElementList)
                    return new SetDefinition(((ElementList)Node).Elements, Start, this.pos - Start);
                else
                    return new SetDefinition(new ScriptNode[] { Node }, Start, this.pos - Start);
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

                double d;

                if (!double.TryParse(this.script.Substring(Start, this.pos - Start).
                    Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out d))
                {
                    throw new SyntaxException("Invalid double number.", this.pos, this.script);
                }

                return new ConstantElement(new DoubleNumber(d), Start, this.pos - Start);
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
                        }
                    }

                    sb.Append(ch2);
                }

                return new ConstantElement(new StringValue(sb.ToString()), Start, this.pos - Start);
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
                        return new ConstantElement(new BooleanValue(true), Start, this.pos - Start);

                    case "FALSE":
                        return new ConstantElement(new BooleanValue(false), Start, this.pos - Start);

                    case "NULL":
                        return new ConstantElement(ObjectValue.Null, Start, this.pos - Start);

                    default:
                        return new VariableReference(s, Start, this.pos - Start);
                }
            }
            else if (ch == '∅')
            {
                this.pos++;
                return new VariableReference("∅", Start, this.pos - Start);
            }
            else
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
        /// Evaluates the expression, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public object Evaluate(Variables Variables)
        {
            IElement Result = this.root.Evaluate(Variables);
            return Result.AssociatedObjectValue;
        }

        /// <summary>
        /// Encapsulates an object.
        /// </summary>
        /// <param name="Value">Object</param>
        /// <returns>Encapsulated object.</returns>
        public static IElement Encapsulate(object Value)
        {
            Type T = Value.GetType();
            switch (Type.GetTypeCode(T))
            {
                case TypeCode.Boolean:
                    return new BooleanValue((bool)Value);

                case TypeCode.Byte:
                    return new DoubleNumber((byte)Value);

                case TypeCode.Char:
                    return new StringValue(new string((char)Value, 1));

                case TypeCode.DateTime:
                    return new DateTimeValue((DateTime)Value);

                case TypeCode.DBNull:
                    return ObjectValue.Null;

                case TypeCode.Decimal:
                    return new DoubleNumber((double)((decimal)Value));

                case TypeCode.Double:
                    return new DoubleNumber((double)Value);

                case TypeCode.Empty:
                    return ObjectValue.Null;

                case TypeCode.Int16:
                    return new DoubleNumber((short)Value);

                case TypeCode.Int32:
                    return new DoubleNumber((int)Value);

                case TypeCode.Int64:
                    return new DoubleNumber((long)Value);

                case TypeCode.SByte:
                    return new DoubleNumber((sbyte)Value);

                case TypeCode.Single:
                    return new DoubleNumber((float)Value);

                case TypeCode.String:
                    return new StringValue((string)Value);

                case TypeCode.UInt16:
                    return new DoubleNumber((ushort)Value);

                case TypeCode.UInt32:
                    return new DoubleNumber((uint)Value);

                case TypeCode.UInt64:
                    return new DoubleNumber((ulong)Value);

                case TypeCode.Object:
                default:
                    if (Value is IElement)
                        return (IElement)Value;
                    else if (Value is double[])
                        return new DoubleVector((double[])Value);
                    else if (Value is bool[])
                        return new BooleanMatrix((bool[,])Value);
                    else if (Value is double[,])
                        return new DoubleMatrix((double[,])Value);
                    else if (Value is bool[,])
                        return new BooleanVector((bool[])Value);
                    else if (Value is IElement[])
                        return new ObjectVector((ICollection<IElement>)(IElement[])Value);
                    else if (Value is IElement[,])
                        return new ObjectMatrix((IElement[,])Value);
                    else if (Value is object[])
                        return new ObjectVector((object[])Value);
                    else if (Value is object[,])
                        return new ObjectMatrix((object[,])Value);
                    else
                        return new ObjectValue(Value);
            }
        }

        public static bool Upgrade(ref IElement E1, ref ISet Set1, ref IElement E2, ref ISet S2, ScriptNode Node)
        {
            return false;   // TODO: Implement Upgrade()
        }

        // TODO: ToSet: {}
        // TODO: Optimize constants
        // TODO: Implicit sets with conditions. {x:x in Z}, {x in Z: x>10}, {[a,b]: a>b}
        // TODO: Namespace values.
        // TODO: Type values.
        // TODO: Create/New, Destroy/Delete, Remove, Error
        // TODO: System.Math functions.
        // TODO: Complex numbers & analytic functions in separate module
        // TODO: Integers (0d, 0x, 0b), Big Integers
        // TODO: Push/Pop when calling functions.
        // TODO: Upgrade
        /*
			System.Math.Abs;
			System.Math.Acos;
			System.Math.Asin;
			System.Math.Atan;
			System.Math.Atan2;
			System.Math.BigMul;
			System.Math.Ceiling;
			System.Math.Cos;
			System.Math.Cosh;
			System.Math.DivRem;
			System.Math.Exp;
			System.Math.Floor;
			System.Math.IEEERemainder;
			System.Math.Log;
			System.Math.Log10;
			System.Math.Max;
			System.Math.Min;
			System.Math.Pow;
			System.Math.Round;
			System.Math.Sign;
			System.Math.Sin;
			System.Math.Sinh;
			System.Math.Sqrt;
			System.Math.Tan;
			System.Math.Tanh;
			System.Math.Truncate;
		*/
    }
}
