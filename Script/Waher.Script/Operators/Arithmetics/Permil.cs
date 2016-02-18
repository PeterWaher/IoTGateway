using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Permil operator.
	/// </summary>
	public class Permil : UnaryDoubleOperator 
	{
		/// <summary>
		/// Permil operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Permil(ScriptNode Operand, int Start, int Length)
			: base(Operand, Start, Length)
		{
		}
	
		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Operand)
		{
			return new DoubleNumber(Operand / 1000);
		}
	}
}
