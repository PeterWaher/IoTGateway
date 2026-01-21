using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if condition does not evaluate to a Boolean value.
	/// </summary>
	public class ConditionNotBooleanScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if condition does not evaluate to a Boolean value.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ConditionNotBooleanScriptException(ScriptNode Node)
			: base("Condition must evaluate to a boolean value.", Node)
		{
		}
	}
}
