using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Vector Index Assignment operator.
	/// </summary>
	public class VectorIndexAssignment : TernaryOperator
	{
		/// <summary>
		/// Vector Index Assignment operator.
		/// </summary>
		/// <param name="VectorIndex">Vector Index</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public VectorIndexAssignment(VectorIndex VectorIndex, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(VectorIndex.LeftOperand, VectorIndex.RightOperand, Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Left = this.left.Evaluate(Variables);
            IVector V = Left as IVector;
            if (V == null)
                throw new ScriptRuntimeException("Vector element assignment can only be performed on vectors.", this);

            IElement Index = this.middle.Evaluate(Variables);
            DoubleNumber IE = Index as DoubleNumber;
            double d;

            if (IE == null || (d=IE.Value) < 0 || d > int.MaxValue || d != Math.Truncate(d))
                throw new ScriptRuntimeException("Index must be a non-negative integer.", this);

            IElement Value = this.right.Evaluate(Variables);
            
            V.SetElement((int)d, Value);

            return Value;
        }

    }
}
