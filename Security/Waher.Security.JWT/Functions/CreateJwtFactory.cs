using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security.JWS;

namespace Waher.Security.JWT.Functions
{
	/// <summary>
	/// Creates a Java Web Token (JWT) Factory
	/// </summary>
	public class CreateJwtFactory : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a Java Web Token (JWT) Factory
		/// </summary>
		/// <param name="Algorithm">JWS Algorithm.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateJwtFactory(ScriptNode Algorithm, int Start, int Length, Expression Expression)
			: base(Algorithm, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(CreateJwtFactory);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Algorithm" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
        {
			if (!(Argument.AssociatedObjectValue is IJwsAlgorithm Algorithm))
				throw new ScriptRuntimeException("Expected a JWS Algorithm argument.", this);

			return new ObjectValue(new JwtFactory(Algorithm));
        }

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}
    }
}
