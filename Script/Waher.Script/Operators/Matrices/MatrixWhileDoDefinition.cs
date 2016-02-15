using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Creates a matrix using a WHILE-DO statement.
	/// </summary>
	public class MatrixWhileDoDefinition : ScriptNode
	{
		private WhileDo elements;

		/// <summary>
		/// Creates a matrix using a WHILE-DO statement.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixWhileDoDefinition(WhileDo Elements, int Start, int Length)
			: base(Start, Length)
		{
			this.elements = Elements;
		}

		/// <summary>
		/// Elements
		/// </summary>
		public WhileDo Elements
		{
			get { return this.elements; }
		}

	}
}
