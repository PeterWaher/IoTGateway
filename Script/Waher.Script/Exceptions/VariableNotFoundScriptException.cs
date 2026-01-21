using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if a variable is not found.
	/// </summary>
	public class VariableNotFoundScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if a variable is not found.
		/// </summary>
		/// <param name="VariableName">Name of the variable not found.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public VariableNotFoundScriptException(string VariableName, ScriptNode Node)
			: base("Variable not found: " + VariableName, Node)
		{
			this.VariableName = VariableName;
		}

		/// <summary>
		/// Name of the variable not found.
		/// </summary>
		public string VariableName { get; }
	}
}
