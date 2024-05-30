using Waher.Script.Exceptions;
using Waher.Script.Units;

namespace Waher.Script.Model
{
	/// <summary>
	/// Script parser, for custom parsers.
	/// </summary>
	public class ScriptParser
	{
		private readonly Expression expression;
		private readonly int start;

		/// <summary>
		/// Script parser, for custom parsers.
		/// </summary>
		/// <param name="Expression">Expression being parsed.</param>
		/// <param name="Start">Start position of subexpression</param>
		internal ScriptParser(Expression Expression, int Start)
		{
			this.start = Start;
			this.expression = Expression;
		}

		/// <summary>
		/// Start position in expression
		/// </summary>
		public int Start => this.start;

		/// <summary>
		/// Length of script parsed
		/// </summary>
		public int Length => this.expression.Position - this.start;

		/// <summary>
		/// Current parsing position.
		/// </summary>
		public int Position => this.expression.Position;

		/// <summary>
		/// Expression being parsed.
		/// </summary>
		public Expression Expression => this.expression;

		/// <summary>
		/// If whitespace can be skipped (true), or if it has semantic meaning to the custom parser (false).
		/// </summary>
		public bool CanSkipWhitespace
		{
			get => this.expression.CanSkipWhitespace;
			set => this.expression.CanSkipWhitespace = value;
		}

		/// <summary>
		/// Returns the next character to be parsed, and moves the position forward one character.
		/// If no character is available, 0 is returned.
		/// </summary>
		/// <returns>Character</returns>
		public char NextChar() => this.expression.NextChar();

		/// <summary>
		/// Returns the next non-whitespace character to be parsed, and moves the 
		/// position forward accordingly. If no character is available, 0 is returned.
		/// </summary>
		/// <returns>Character</returns>
		public char NextNonWhitespaceChar()
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
		/// Undoes a character in the parsing of an expression.
		/// </summary>
		public void UndoChar()
		{
			this.expression.UndoChar();
		}

		/// <summary>
		/// Returns the next given number of characters to be parsed, without moving the position 
		/// forward one character. If no character is available, the string is truncated.
		/// </summary>
		/// <returns>Characters</returns>
		public string PeekNextChars(int NrChars)
		{
			return this.expression.PeekNextChars(NrChars);
		}

		/// <summary>
		/// If the next characters to be parsed is a given token.
		/// </summary>
		/// <param name="Token">Token</param>
		/// <returns>If the token is the next to be parsed.</returns>
		public bool IsNextChars(string Token)
		{
			return this.expression.IsNextChars(Token);
		}

		/// <summary>
		/// If the next characters to be parsed is a given token consisting of
		/// a character being repeated a given amount of times.
		/// </summary>
		/// <param name="ch">Character</param>
		/// <param name="Count">Number of times character is repeated.</param>
		/// <returns>If the token is the next to be parsed.</returns>
		public bool IsNextChars(char ch, int Count)
		{
			return this.expression.IsNextChars(ch, Count);
		}

		/// <summary>
		/// Skips a predefined number of characters.
		/// </summary>
		public void SkipChars(int NrChars)
		{
			this.expression.SkipChars(NrChars);
		}

		/// <summary>
		/// Returns the next character to be parsed, without moving the position forward one character.
		/// If no character is available, 0 is returned.
		/// </summary>
		/// <returns>Character</returns>
		public char PeekNextChar() => this.expression.PeekNextChar();

		/// <summary>
		/// Returns the next token to be parsed, and moves the position forward correspondingly.
		/// If at the end of the expression, <see cref="string.Empty"/> is returned.
		/// </summary>
		/// <returns>Token</returns>
		public string NextToken() => this.expression.NextToken();

		/// <summary>
		/// Returns the next token to be parsed, without moving the position forward.
		/// If at the end of the expression, <see cref="string.Empty"/> is returned.
		/// </summary>
		/// <returns>Token</returns>
		public string PeekNextToken() => this.expression.PeekNextToken();

		/// <summary>
		/// If current position is whitespace, moves the current position forward to the first non-whitespace character,
		/// or the end of the expression.
		/// </summary>
		public void SkipWhiteSpace() => this.expression.SkipWhiteSpace();

		/// <summary>
		/// Throws an exception if an operand is not null.
		/// </summary>
		/// <param name="Node">Operand node.</param>
		/// <returns><paramref name="Node"/></returns>
		public ScriptNode AssertOperandNotNull(ScriptNode Node) => this.expression.AssertOperandNotNull(Node);

		/// <summary>
		/// Throws an exception if a right-hand side operand is not null.
		/// </summary>
		/// <param name="Node">Operand node.</param>
		/// <returns><paramref name="Node"/></returns>
		public ScriptNode AssertRightOperandNotNull(ScriptNode Node) => this.expression.AssertRightOperandNotNull(Node);

		/// <summary>
		/// Parses a sequence of statements.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseSequence() => this.expression.ParseSequence();

		/// <summary>
		/// Parses a statement.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseStatement() => this.expression.ParseStatement(false);

		/// <summary>
		/// Parses an element list.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseList() => this.expression.ParseList();

		/// <summary>
		/// Parses a conditional statement.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseIf() => this.expression.ParseIf();

		/// <summary>
		/// Parses an assignment.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseAssignments() => this.expression.ParseAssignments();

		/// <summary>
		/// Parses a lambda expression.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseLambdaExpression() => this.expression.ParseLambdaExpression();

		/// <summary>
		/// Parses an equivalence.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseEquivalence() => this.expression.ParseEquivalence();

		/// <summary>
		/// Parses ORs.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseOrs() => this.expression.ParseOrs();

		/// <summary>
		/// Parses ANDs.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseAnds() => this.expression.ParseAnds();

		/// <summary>
		/// Parses a membership operator.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseMembership() => this.expression.ParseMembership();

		/// <summary>
		/// Parses a comparison.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseComparison() => this.expression.ParseComparison();

		/// <summary>
		/// Parses a shift operator.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseShifts() => this.expression.ParseShifts();

		/// <summary>
		/// Parses unions.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseUnions() => this.expression.ParseUnions();

		/// <summary>
		/// Parses intersections.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseIntersections() => this.expression.ParseIntersections();

		/// <summary>
		/// Parses an interval.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseInterval() => this.expression.ParseInterval();

		/// <summary>
		/// Parses terms.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseTerms() => this.expression.ParseTerms();

		/// <summary>
		/// Parses binomial coefficients.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseBinomialCoefficients() => this.expression.ParseBinomialCoefficients();

		/// <summary>
		/// Parses factors.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseFactors() => this.expression.ParseFactors();

		/// <summary>
		/// Parses powers.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParsePowers() => this.expression.ParsePowers();

		/// <summary>
		/// Parses unary prefix operatorrs.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseUnaryPrefixOperator() => this.expression.ParseUnaryPrefixOperator();

		/// <summary>
		/// Parses suffix operators.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseSuffixOperator() => this.expression.ParseSuffixOperator();

		/// <summary>
		/// Parses a unit.
		/// </summary>
		/// <returns>Script node.</returns>
		public Unit ParseUnit(bool PermitPrefix) => this.expression.ParseUnit(PermitPrefix);

		/// <summary>
		/// Parses an object ex nihilo.
		/// </summary>
		/// <returns>Script node.</returns>
		public ScriptNode ParseObject() => this.expression.ParseObject();

		/// <summary>
		/// If position is at end of script.
		/// </summary>
		public bool EndOfScript => this.expression.EndOfScript;

		/// <summary>
		/// If position is in script.
		/// </summary>
		public bool InScript => this.expression.InScript;

		/// <summary>
		/// If there are a given number of characters left to parse.
		/// </summary>
		/// <param name="NrCharacters">Number of characters to check.</param>
		/// <returns>If the number of characeters are available.</returns>
		public bool HasCharacters(int NrCharacters) => this.expression.Position <= this.expression.Script.Length - NrCharacters;

		/// <summary>
		/// Returns a Syntax Error Exception object.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <returns>Exception object.</returns>
		public SyntaxException SyntaxError(string Message)
		{
			return new SyntaxException(Message, this.Position, this.expression.Script);
		}
	}
}
