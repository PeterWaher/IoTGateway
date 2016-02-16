using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Calculus
{
	/// <summary>
	/// Default Differentiation operator.
	/// </summary>
	public class DefaultDifferentiation : UnaryOperator 
	{
		private int nrDifferentiations;

		/// <summary>
		/// Default Differentiation operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NrDifferentiations">Number of differentiations.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DefaultDifferentiation(ScriptNode Operand, int NrDifferentiations, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.nrDifferentiations = NrDifferentiations;
		}

		/// <summary>
		/// Number of differentiations.
		/// </summary>
		public int NrDifferentiations
		{
			get { return this.nrDifferentiations; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Element Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}
	}
}
