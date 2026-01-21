using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if argument dimensions are not consistent.
	/// </summary>
	public class ArgumentDimensionsInconsistentScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if argument dimensions are not consistent.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ArgumentDimensionsInconsistentScriptException(ScriptNode Node)
			: base("Argument dimensions not consistent.", Node)
		{
		}
	}
}
