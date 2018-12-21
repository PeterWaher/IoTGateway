using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Dynamic Index Assignment operator.
	/// </summary>
	public class DynamicIndexAssignment : UnaryOperator 
	{
		private readonly DynamicIndex dynamicIndex;

		/// <summary>
		/// Dynamic Index Assignment operator.
		/// </summary>
		/// <param name="DynamicIndex">Dynamic Index</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DynamicIndexAssignment(DynamicIndex DynamicIndex, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.dynamicIndex = DynamicIndex;
		}

		/// <summary>
		/// Dynamic Index.
		/// </summary>
		public DynamicIndex DynamicIndex
		{
			get { return this.dynamicIndex; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}

	}
}
