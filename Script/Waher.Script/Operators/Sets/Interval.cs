using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Interval operator
	/// </summary>
	public class Interval : TernaryOperator 
	{
		/// <summary>
		/// Interval operator
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Interval(ScriptNode From, ScriptNode To, int Start, int Length)
			: base(From, To, null, Start, Length)
		{
		}

		/// <summary>
		/// Interval operator
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="StepSize">Step size.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Interval(ScriptNode From, ScriptNode To, ScriptNode StepSize, int Start, int Length)
			: base(From, To, StepSize, Start, Length)
		{
		}
	}
}
