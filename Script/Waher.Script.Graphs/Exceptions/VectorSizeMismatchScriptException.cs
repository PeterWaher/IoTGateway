using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Exceptions
{
	/// <summary>
	/// Exception thrown if vector sizes do not match.
	/// </summary>
	public class VectorSizeMismatchScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if vector sizes do not match.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public VectorSizeMismatchScriptException(ScriptNode Node)
			: base("Vector size mismatch.", Node)
		{
		}
	}
}
