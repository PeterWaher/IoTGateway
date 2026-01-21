using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if scalar operands are not Boolean.
	/// </summary>
	public class ScalarOperandsNotBooleanScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if scalar operands are not Boolean.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ScalarOperandsNotBooleanScriptException(ScriptNode Node)
			: base("Scalar operands must be boolean values.", Node)
		{
		}
	}
}
