using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Syntax error exception.
	/// </summary>
	public class SyntaxException : ScriptException 
	{
		private string script;
		private int position;

		/// <summary>
		/// Syntax error exception.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Position">Position into script where the syntax error was detected.</param>
		/// <param name="Script">Script expression where syntax error was detected.</param>
		public SyntaxException(string Message, int Position, string Script)
			: base(Append(Message, Position, Script))
		{
			this.position = Position;
			this.script = Script;
		}

		/// <summary>
		/// Base class for script exceptions.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="Position">Position into script where the syntax error was detected.</param>
		/// <param name="Script">Script expression where syntax error was detected.</param>
		/// <param name="InnerException">Inner exception.</param>
		public SyntaxException(string Message, int Position, string Script, Exception InnerException)
			: base(Append(Message, Position, Script), InnerException)
		{
			this.position = Position;
			this.script = Script;
		}

		private static string Append(string Message, int Position, string Script)
		{
			Message += "\r\n\r\n";

			int Start = Math.Max(0, Position - 100);
			int End = Math.Min(Script.Length, Position + 100);

			Message += Script.Substring(Start, Position - Start) + "^-------------" + Script.Substring(Position, End - Position);

			return Message;
		}

		/// <summary>
		/// Position into script where the syntax error was detected.
		/// </summary>
		public int Position
		{
			get { return this.position; }
		}

		/// <summary>
		/// Script expression where syntax error was detected.
		/// </summary>
		public string Script
		{
			get { return this.script; }
		}

	}
}
