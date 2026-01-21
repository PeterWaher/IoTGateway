using System;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Fractals.Exceptions
{
	/// <summary>
	/// Exception thrown if a lambda expression with invalid type is encountered.
	/// </summary>
	public class LambdaTypeScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if a lambda expression with invalid type is encountered.
		/// </summary>
		/// <param name="Type">Type returned by lambda expression.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public LambdaTypeScriptException(Type Type, ScriptNode Node)
			: base("Lambda expression must be able to accept complex vectors, " +
				"and return complex vectors of equal length. Type returned: " +
				Type.FullName, Node)
		{
			this.Type = Type;
		}

		/// <summary>
		/// Type returned by lambda expression.
		/// </summary>
		public Type Type { get; }
	}
}
