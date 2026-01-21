using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if binary data is expected.
	/// </summary>
	public class BinaryDataExpectedScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if binary data is expected.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public BinaryDataExpectedScriptException(ScriptNode Node)
			: base("Binary data expected.", Node)
		{
		}
	}
}
