using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Interval operator
	/// </summary>
	public class Interval : TernaryOperator 
	{
		/// <summary>
		/// Interval operator
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Interval(ScriptNode From, ScriptNode To, int Start, int Length, Expression Expression)
			: base(From, To, null, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Interval operator
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="StepSize">Step size.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Interval(ScriptNode From, ScriptNode To, ScriptNode StepSize, int Start, int Length, Expression Expression)
			: base(From, To, StepSize, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement From = this.left.Evaluate(Variables);
            IElement To = this.middle.Evaluate(Variables);
            IElement StepSize = this.right?.Evaluate(Variables);

			DoubleNumber S = StepSize as DoubleNumber;

			if (!(From.AssociatedObjectValue is double F) ||
				!(To.AssociatedObjectValue is double T) ||
				(S is null && !(StepSize is null)))
			{
				throw new ScriptRuntimeException("The interval operator requires double-valued operands.", this);
			}

            return new Objects.Sets.Interval(F, T, true, true, S is null ? (double?)null : S.Value);
        }

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement From = await this.left.EvaluateAsync(Variables);
			IElement To = await this.middle.EvaluateAsync(Variables);
			IElement StepSize = this.right is null ? null : await this.right.EvaluateAsync(Variables);
			double? S;

			if (StepSize?.AssociatedObjectValue is double d)
				S = d;
			else if (!(StepSize is null))
				throw new ScriptRuntimeException("The interval operator requires double-valued operands.", this);
			else
				S = null;

			if (!(From.AssociatedObjectValue is double F) ||
				!(To.AssociatedObjectValue is double T))
			{
				throw new ScriptRuntimeException("The interval operator requires double-valued operands.", this);
			}

			return new Objects.Sets.Interval(F, T, true, true, S);
		}
	}
}
