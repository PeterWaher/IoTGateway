using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// Try-Finally operator.
	/// </summary>
	public class TryFinally : BinaryOperator
	{
		/// <summary>
		/// Try-Finally operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="FinallyStatement">Finally statement.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public TryFinally(ScriptNode Statement, ScriptNode FinallyStatement, int Start, int Length, Expression Expression)
			: base(Statement, FinallyStatement, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            try
            {
                return this.left.Evaluate(Variables);
            }
            finally
            {
                this.right.Evaluate(Variables);
            }
        }
    }
}
