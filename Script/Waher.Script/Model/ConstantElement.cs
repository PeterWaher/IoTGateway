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

	}
}
