using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents a list of elements.
	/// </summary>
	public class ElementList : ScriptNode
	{
		ScriptNode[] elements;

		/// <summary>
		/// Represents a list of elements.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ElementList(ScriptNode[] Elements, int Start, int Length)
			: base(Start, Length)
		{
			this.elements = Elements;
		}

		/// <summary>
		/// Elements.
		/// </summary>
		public ScriptNode[] Elements
		{
			get { return this.elements; }
		}

	}
}
