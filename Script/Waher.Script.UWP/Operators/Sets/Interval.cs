using System;
using System.Collections.Generic;
using System.Text;
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
		public Interval(ScriptNode From, ScriptNode To, int Start, int Length)
			: base(From, To, null, Start, Length)
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
		public Interval(ScriptNode From, ScriptNode To, ScriptNode StepSize, int Start, int Length)
			: base(From, To, StepSize, Start, Length)
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
            IElement StepSize = this.right == null ? null : this.right.Evaluate(Variables);

            DoubleNumber F = From as DoubleNumber;
            DoubleNumber T = To as DoubleNumber;
            DoubleNumber S = StepSize as DoubleNumber;

            if (F == null || T == null || (S == null && StepSize != null))
                throw new ScriptRuntimeException("The interval operator requires double-valued operands.", this);

            return new Objects.Sets.Interval(F.Value, T.Value, true, true, S == null ? (double?)null : S.Value);
        }
    }
}
