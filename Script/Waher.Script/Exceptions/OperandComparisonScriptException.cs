using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if operands cannot be compared.
	/// </summary>
	public class OperandComparisonScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if operands cannot be compared.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public OperandComparisonScriptException(ScriptNode Node)
			: base("Cannot compare operands.", Node)
		{
		}
	}
}
