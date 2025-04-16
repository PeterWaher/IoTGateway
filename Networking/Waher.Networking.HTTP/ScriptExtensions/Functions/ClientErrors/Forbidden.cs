using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.ClientError
{
	/// <summary>
	/// Throws a <see cref="ForbiddenException"/>
	/// </summary>
	public class Forbidden : FunctionOneVariable
	{
		/// <summary>
		/// Throws a <see cref="ForbiddenException"/>
		/// </summary>
		/// <param name="Content">Content to return</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Forbidden(ScriptNode Content, int Start, int Length, Expression Expression)
			: base(Content, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Forbidden);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			if (Variables.TryGetVariable("Request", out Variable v) &&
				v.ValueObject is HttpRequest Request)
			{
				throw new ForbiddenException(Request, Argument.AssociatedObjectValue);
			}
			else
				throw new ForbiddenException(Argument.AssociatedObjectValue);
		}
	}
}
