using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime exception.
	/// </summary>
	public class ScriptRuntimeException : ScriptException 
	{
		private readonly ScriptNode node;

		/// <summary>
		/// Script runtime exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ScriptRuntimeException(string Message, ScriptNode Node)
			: base(Message)
		{
			this.node = Node;
		}

		/// <summary>
		/// Script runtime exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		/// <param name="InnerException">Inner exception.</param>
		public ScriptRuntimeException(string Message, ScriptNode Node, Exception InnerException)
			: base(Message, InnerException)
		{
			this.node = Node;
		}

		/// <summary>
		/// Node where error occurred.
		/// </summary>
		public ScriptNode Node
		{
			get { return this.node; }
		}

	}
}
