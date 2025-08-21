using System;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime error exception.
	/// </summary>
	public class ScriptRuntimeErrorException : ScriptRuntimeException 
	{
		/// <summary>
		/// Script runtime error exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ScriptRuntimeErrorException(string Message, ScriptNode Node)
			: base(Message, Node)
		{
		}

		/// <summary>
		/// Script runtime exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		/// <param name="InnerErrorException">Inner exception.</param>
		public ScriptRuntimeErrorException(string Message, ScriptNode Node, Exception InnerErrorException)
			: base(Message, Node, InnerErrorException)
		{
		}
	}
}
