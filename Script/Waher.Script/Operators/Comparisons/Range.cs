using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Comparisons
{
	/// <summary>
	/// Range operator
	/// </summary>
	public class Range : TernaryOperator
    {
		private readonly bool leftInclusive;
		private readonly bool rightInclusive;

		/// <summary>
		/// Range operator
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Middle">Middle operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="LeftInclusive">If the value specified by <paramref name="Left"/> is included in the range.</param>
		/// <param name="RightInclusive">If the value specified by <paramref name="Right"/> is included in the range.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Range(ScriptNode Left, ScriptNode Middle, ScriptNode Right, bool LeftInclusive, bool RightInclusive,
			int Start, int Length, Expression Expression)
			: base(Left, Middle, Right, Start, Length, Expression)
		{
			this.leftInclusive = LeftInclusive;
			this.rightInclusive = RightInclusive;
		}

		/// <summary>
		/// If the value specified by <see cref="LeftOperand"/> is included in the range.
		/// </summary>
		public bool LeftInclusive => this.leftInclusive;

		/// <summary>
		/// If the value specified by <see cref="RightOperand"/> is included in the range.
		/// </summary>
		public bool RightInclusive => this.rightInclusive;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			IElement Middle = this.middle.Evaluate(Variables);
			IElement Right = this.right.Evaluate(Variables);

			if (!(Middle.AssociatedSet is IOrderedSet S))
				throw new ScriptRuntimeException("Cannot compare operands.", this);

			int i = S.Compare(Middle, Left);

			if (i < 0 || (i == 0 && !this.leftInclusive))
				return BooleanValue.False;

			i = S.Compare(Middle, Right);

			if (i > 0 || (i == 0 && !this.rightInclusive))
				return BooleanValue.False;

			return BooleanValue.True;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (this.left is ConstantElement LeftConstant &&
				this.right is ConstantElement RightConstant)
			{
				if (!(CheckAgainst.AssociatedSet is IOrderedSet S))
					return PatternMatchResult.NoMatch;

				int i = S.Compare(CheckAgainst, LeftConstant.Constant);

				if (i < 0 || (i == 0 && !this.leftInclusive))
					return PatternMatchResult.NoMatch;

				i = S.Compare(CheckAgainst, RightConstant.Constant);

				if (i > 0 || (i == 0 && !this.rightInclusive))
					return PatternMatchResult.NoMatch;

				return this.middle.PatternMatch(CheckAgainst, AlreadyFound);
			}
			else
				return PatternMatchResult.NoMatch;
		}

	}
}
