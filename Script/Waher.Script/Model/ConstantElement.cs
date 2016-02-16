using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Represents a constant element value.
	/// </summary>
	public class ConstantElement : ScriptNode
	{
		private Element constant;

		/// <summary>
		/// Represents a constant element value.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ConstantElement(Element Constant, int Start, int Length)
			: base(Start, Length)
		{
			this.constant = Constant;
		}

		/// <summary>
		/// Constant value.
		/// </summary>
		public Element Constant
		{
			get { return this.constant; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Element Evaluate(Variables Variables)
		{
			return this.constant;
		}

	}
}
