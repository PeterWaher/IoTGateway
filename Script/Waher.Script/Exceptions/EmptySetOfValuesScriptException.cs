using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if an empty set of values is encountered.
	/// </summary>
	public class EmptySetOfValuesScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if an empty set of values is encountered.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public EmptySetOfValuesScriptException(ScriptNode Node)
			: base("Empty set of values.", Node)
		{
		}
	}
}
