using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if an operand is out of bounds.
	/// </summary>
	public class OperandOutOfBoundsScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if an operand is out of bounds.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public OperandOutOfBoundsScriptException(ScriptNode Node)
			: base("Operand out of bounds.", Node)
		{
		}
	}
}
