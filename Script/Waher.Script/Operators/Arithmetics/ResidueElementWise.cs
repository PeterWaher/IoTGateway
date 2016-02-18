using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Element-wise Residue operator.
	/// </summary>
	public class ResidueElementWise : BinaryScalarOperator 
	{
		/// <summary>
		/// Element-wise Residue operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ResidueElementWise(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Left, IElement Right)
		{
			return Residue.EvaluateResidue(Left, Right, this);
		}

	}
}
