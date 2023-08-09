using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions
{
	/// <summary>
	/// Creates a Duration value.
	/// </summary>
	public class Duration : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a Duration value.
		/// </summary>
		/// <param name="String">String representation to be parsed.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Duration(ScriptNode String, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Duration value.
		/// </summary>
		/// <param name="Years">Years</param>
		/// <param name="Months">Months</param>
		/// <param name="Days">Days</param>
		/// <param name="Hours">Hours</param>
		/// <param name="Minute">Minutes</param>
		/// <param name="Seconds">Seconds</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Duration(ScriptNode Years, ScriptNode Months, ScriptNode Days, ScriptNode Hours, ScriptNode Minute, ScriptNode Seconds,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Years, Months, Days, Hours, Minute, Seconds }, argumentTypes6Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Duration);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Years", "Months", "Days", "Hours", "Minutes", "Seconds" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;

			if (c == 1)
			{
				object Obj = Arguments[0].AssociatedObjectValue;
				string s = Obj?.ToString() ?? string.Empty;

				if (Waher.Content.Duration.TryParse(s, out Waher.Content.Duration D))
					return new ObjectValue(D);
				else
					throw new ScriptRuntimeException("Unable to parse Duration value: " + s, this);
			}

			int[] d = new int[c];
			bool Sign = false;
			DoubleNumber n;
			int i, j, NrNeg = 0, NrPos = 0;

			for (i = 0; i < c; i++)
			{
				n = Arguments[i] as DoubleNumber;
				if (n is null)
					throw new ScriptRuntimeException("Expected number arguments.", this);

				j = (int)n.Value;
				if (j < 0)
					NrNeg++;
				else if (j > 0)
					NrPos++;

				d[i] = j;
			}

			if (NrNeg > NrPos)
			{
				Sign = true;
				for (i = 0; i < c; i++)
					d[i] = -d[i];
			}

			return new ObjectValue(new Waher.Content.Duration(Sign, d[0], d[1], d[2], d[3], d[4], d[5]));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst.AssociatedObjectValue is Waher.Content.Duration D))
			{
				string s = CheckAgainst.AssociatedObjectValue?.ToString() ?? string.Empty;

				if (!Waher.Content.Duration.TryParse(s, out D))
					return PatternMatchResult.NoMatch;
			}

			int c = this.Arguments.Length;
			if (c == 1)
				return this.Arguments[0].PatternMatch(new ObjectValue(D), AlreadyFound);

			if (D.Negation)
				D = new Waher.Content.Duration(false, -D.Years, -D.Months, -D.Days, -D.Hours, -D.Minutes, -D.Seconds);

			PatternMatchResult Result = this.Arguments[0].PatternMatch(new DoubleNumber(D.Years), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[1].PatternMatch(new DoubleNumber(D.Months), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[2].PatternMatch(new DoubleNumber(D.Days), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;
			
			Result = this.Arguments[3].PatternMatch(new DoubleNumber(D.Hours), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;
			
			Result = this.Arguments[4].PatternMatch(new DoubleNumber(D.Minutes), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;
			
			return this.Arguments[5].PatternMatch(new DoubleNumber(D.Seconds), AlreadyFound);
		}
	}
}
