using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Redirections
{
	/// <summary>
	/// Throws a <see cref="NotModifiedException"/>
	/// </summary>
	public class NotModified : FunctionZeroVariables
	{
		/// <summary>
		/// Throws a <see cref="NotModifiedException"/>
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NotModified(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "NotModified";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			throw new NotModifiedException();
		}
	}
}
