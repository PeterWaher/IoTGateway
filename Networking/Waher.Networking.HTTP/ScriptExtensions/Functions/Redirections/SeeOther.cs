using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Redirections
{
	/// <summary>
	/// Throws a <see cref="SeeOtherException"/>
	/// </summary>
	public class SeeOther : FunctionOneVariable
	{
		/// <summary>
		/// Throws a <see cref="SeeOtherException"/>
		/// </summary>
		/// <param name="Location">Location</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SeeOther(ScriptNode Location, int Start, int Length, Expression Expression)
			: base(Location, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "SeeOther";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			throw new SeeOtherException(Argument.AssociatedObjectValue?.ToString());
		}
	}
}
