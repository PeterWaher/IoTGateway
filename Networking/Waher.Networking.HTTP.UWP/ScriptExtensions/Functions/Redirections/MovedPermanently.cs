using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Redirections
{
	/// <summary>
	/// Throws a <see cref="MovedPermanentlyException"/>
	/// </summary>
	public class MovedPermanently : FunctionOneVariable
	{
		/// <summary>
		/// Throws a <see cref="MovedPermanentlyException"/>
		/// </summary>
		/// <param name="Location">Location</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MovedPermanently(ScriptNode Location, int Start, int Length, Expression Expression)
			: base(Location, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "MovedPermanently";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			throw new MovedPermanentlyException(Expression.ToString(Argument.AssociatedObjectValue));
		}
	}
}
