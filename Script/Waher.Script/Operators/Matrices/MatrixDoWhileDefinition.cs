using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Creates a matrix using a DO-WHILE statement.
	/// </summary>
	public class MatrixDoWhileDefinition : ScriptNode
	{
		private DoWhile elements;

		/// <summary>
		/// Creates a matrix using a DO-WHILE statement.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixDoWhileDefinition(DoWhile Elements, int Start, int Length)
			: base(Start, Length)
		{
			this.elements = Elements;
		}

		/// <summary>
		/// Elements
		/// </summary>
		public DoWhile Elements
		{
			get { return this.elements; }
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
