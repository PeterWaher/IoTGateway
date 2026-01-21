using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if an operand is not an integer.
	/// </summary>
	public class OperandNonIntegerScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if an operand is not an integer.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public OperandNonIntegerScriptException(ScriptNode Node)
			: base("Operands must be integer values.", Node)
		{
		}
	}
}
