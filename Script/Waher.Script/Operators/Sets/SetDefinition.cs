using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Creates a set.
	/// </summary>
	public class SetDefinition : ElementList
	{
		/// <summary>
		/// Creates a set.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public SetDefinition(ScriptNode[] Elements, int Start, int Length)
			: base(Elements, Start, Length)
		{
		}
	}
}
