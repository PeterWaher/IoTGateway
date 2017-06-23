using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all funcions.
	/// </summary>
	public abstract class Function : ScriptNode, IFunction
	{
		/// <summary>
		/// Base class for all funcions.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Function(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public abstract string FunctionName
		{
			get;
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public virtual string[] Aliases
		{
			get { return null; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public abstract string[] DefaultArgumentNames
		{
			get;
		}

	}
}
