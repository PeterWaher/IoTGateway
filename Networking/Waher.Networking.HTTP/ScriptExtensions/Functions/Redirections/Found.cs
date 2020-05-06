using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Redirections
{
	/// <summary>
	/// Throws a <see cref="FoundException"/>
	/// </summary>
	public class Found : FunctionOneVariable
	{
		/// <summary>
		/// Throws a <see cref="FoundException"/>
		/// </summary>
		/// <param name="Location">Location</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Found(ScriptNode Location, int Start, int Length, Expression Expression)
			: base(Location, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Found";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			throw new FoundException(Argument.AssociatedObjectValue?.ToString());
		}
	}
}
