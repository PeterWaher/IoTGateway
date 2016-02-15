using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector.
	/// </summary>
	public class VectorDefinition : ElementList
	{
		/// <summary>
		/// Creates a vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public VectorDefinition(ScriptNode[] Elements, int Start, int Length)
			: base(Elements, Start, Length)
		{
		}
	}
}
