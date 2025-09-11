using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWT.Functions
{
	/// <summary>
	/// Validates a Java Web Token (JWT) created by CreateJwt.
	/// </summary>
	public class ValidateJwt : FunctionMultiVariate
	{
		/// <summary>
		/// Validates a Java Web Token (JWT) created by CreateJwt.
		/// </summary>
		/// <param name="Token">Token.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ValidateJwt(ScriptNode Token, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Token }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Validates a Java Web Token (JWT) created by CreateJwt.
		/// </summary>
		/// <param name="Token">Token.</param>
		/// <param name="Factory">JWT Factory</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ValidateJwt(ScriptNode Token, ScriptNode Factory, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Token, Factory }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ValidateJwt);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Token", "Factory" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			object Obj = Arguments[0].AssociatedObjectValue;

			if (!(Obj is JwtToken Token))
			{
				if (Obj is string s)
				{
					if (!JwtToken.TryParse(s, out Token, out string Reason))
						throw new ScriptRuntimeException(Reason, this);
				}
				else
					throw new ScriptRuntimeException("Expected JWT Token, or string, as first argument.", this);
			}

			JwtFactory Factory;

			if (Arguments.Length == 1)
				Factory = CreateJwt.Factory;
			else if (Arguments[1].AssociatedObjectValue is JwtFactory Factory2)
				Factory = Factory2;
			else
				throw new ScriptRuntimeException("Expected a JWT Factory as second argument.", this);

			if (!Factory.IsValid(Token, out Reason Reason2))
				throw new ScriptRuntimeException("Token not valid. Reason: " + Reason2.ToString(), this);

			return new ObjectValue(Token);
		}
	}
}
