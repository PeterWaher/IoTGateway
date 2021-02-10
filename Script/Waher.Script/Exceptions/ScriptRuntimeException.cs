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
			: base(Join(Message, Node))
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
			: base(Join(Message, Node), InnerException)
		{
			this.node = Node;
		}

		private static string Join(string Message, ScriptNode Node)
		{
			if (Node is null)
				return Message;

			string s = Node.SubExpression;
			if (s.Length > 1000)
				s = s.Substring(0, 1000) + "...";

			return Message + "\r\n\r\n" + s;
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
