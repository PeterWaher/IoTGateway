using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if an operand is out of bounds.
	/// </summary>
	public class ArgumentNonNegativeIntegerScriptException : ArgumentOutOfRangeScriptException
	{
		/// <summary>
		/// Exception thrown if an operand is out of bounds.
		/// </summary>
		/// <param name="Name">Argument name</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ArgumentNonNegativeIntegerScriptException(string Name, ScriptNode Node)
			: base(Name, Name + " must be a non-negative integer.", Node)
		{
		}
	}
}
