using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown if an operand is out of bounds.
	/// </summary>
	public class ArgumentOutOfRangeScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if an operand is out of bounds.
		/// </summary>
		/// <param name="Name">Argument name</param>
		/// <param name="Message">Error message</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public ArgumentOutOfRangeScriptException(string Name, string Message, ScriptNode Node)
			: base(Message, Node)
		{
			this.Name = Name;
		}

		/// <summary>
		/// Argument name.
		/// </summary>
		public string Name { get; }
	}
}
