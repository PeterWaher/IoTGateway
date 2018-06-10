using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base interface for lambda expressions.
	/// </summary>
	public interface IDifferentiable
	{
		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		ScriptNode Differentiate(string VariableName, Variables Variables);

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		string DefaultVariableName
		{
			get;
		}
	}
}
