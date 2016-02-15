using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Named function call operator
	/// </summary>
	public class NamedFunctionCall : ScriptNode
	{
		private string functionName;
		private ScriptNode[] arguments;

		/// <summary>
		/// Named function call operator
		/// </summary>
		/// <param name="Function">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public NamedFunctionCall(string FunctionName, ScriptNode[] Arguments, int Start, int Length)
			: base(Start, Length)
		{
			this.functionName = FunctionName;
			this.arguments = Arguments;
		}

		/// <summary>
		/// Function name.
		/// </summary>
		public string FunctionName
		{
			get { return this.functionName; }
		}

		/// <summary>
		/// Arguments
		/// </summary>
		public ScriptNode[] Arguments
		{
			get { return this.arguments; }
		}
	}
}
