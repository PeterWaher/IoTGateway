using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Fractals.Exceptions
{
	/// <summary>
	/// Exception thrown if a lambda expression with invalid type is encountered.
	/// </summary>
	public class LambdaLengthScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if a lambda expression with invalid type is encountered.
		/// </summary>
		/// <param name="Length">Length returned by lambda expression.</param>
		/// <param name="Expected">Expected length of lambda expression.</param>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public LambdaLengthScriptException(int Length, int Expected, ScriptNode Node)
			: base("Lambda expression must be able to accept complex vectors, " +
				"and return complex vectors of equal length. Length returned: " +
				Length.ToString() + ". Expected: " + Expected.ToString(), Node)
		{
			this.Length = Length;
			this.Expected = Expected;
		}

		/// <summary>
		/// Length returned by lambda expression.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Expected length of lambda expression.
		/// </summary>
		public int Expected { get; }
	}
}
