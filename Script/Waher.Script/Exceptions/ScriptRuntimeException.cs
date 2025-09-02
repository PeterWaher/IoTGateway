using System;
using System.Text;
using Waher.Events;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime exception.
	/// </summary>
	public class ScriptRuntimeException : ScriptException, IEventObject
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

		/// <summary>
		/// Script runtime exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		/// <param name="RawMessage">If the raw message should be used (true), or if it should be joined with information
		/// related to the context derived from the script node.</param>
		public ScriptRuntimeException(string Message, ScriptNode Node, bool RawMessage)
			: base(RawMessage ? Message : Join(Message, Node))
		{
			this.node = Node;
		}

		/// <summary>
		/// Script runtime exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		/// <param name="InnerException">Inner exception.</param>
		/// <param name="RawMessage">If the raw message should be used (true), or if it should be joined with information
		/// related to the context derived from the script node.</param>
		public ScriptRuntimeException(string Message, ScriptNode Node, Exception InnerException, bool RawMessage)
			: base(RawMessage ? Message : Join(Message, Node), InnerException)
		{
			this.node = Node;
		}

		private static string Join(string Message, ScriptNode Node)
		{
			if (Node is null)
				return Message;

			StringBuilder sb = new StringBuilder(Message);

			if (!string.IsNullOrEmpty(Node.Expression.Source))
			{
				sb.Append(" (Source: ");
				sb.Append(Node.Expression.Source);
				sb.Append(')');
			}

			string s = Node.SubExpression;
			if (s.Length > 1000)
				s = s.Substring(0, 1000) + "...";

			sb.Append("\r\n\r\n");
			sb.Append(s);

			return sb.ToString();
		}

		/// <summary>
		/// Node where error occurred.
		/// </summary>
		public ScriptNode Node => this.node;

		/// <summary>
		/// Object identifier related to the object.
		/// </summary>
		public string Object => this.node?.Expression?.Source ?? string.Empty;
	}
}
