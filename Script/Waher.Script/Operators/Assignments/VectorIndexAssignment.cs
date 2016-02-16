using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Vector Index Assignment operator.
	/// </summary>
	public class VectorIndexAssignment : UnaryOperator 
	{
		VectorIndex vectorIndex;

		/// <summary>
		/// Vector Index Assignment operator.
		/// </summary>
		/// <param name="VectorIndex">Vector Index</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public VectorIndexAssignment(VectorIndex VectorIndex, ScriptNode Operand, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.vectorIndex = VectorIndex;
		}

		/// <summary>
		/// Vector Index.
		/// </summary>
		public VectorIndex VectorIndex
		{
			get { return this.vectorIndex; }
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
