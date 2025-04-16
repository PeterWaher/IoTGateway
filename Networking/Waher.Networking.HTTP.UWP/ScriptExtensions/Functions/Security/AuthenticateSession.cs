using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Security
{
	/// <summary>
	/// Authenticates an HTTP request using an authenticated user in the session.
	/// </summary>
	public class AuthenticateSession : FunctionMultiVariate
	{
		/// <summary>
		/// Authenticates an HTTP request using an authenticated user in the session.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="UserVariable">Name of variable that should contain the authenticate user.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateSession(ScriptNode Request, ScriptNode UserVariable, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, UserVariable },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Authenticates an HTTP request using an authenticated user in the session.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="UserVariable">Name of variable that should contain the authenticate user.</param>
		/// <param name="EncryptionStrength">The level of encryption strength required.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateSession(ScriptNode Request, ScriptNode UserVariable, ScriptNode EncryptionStrength, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, UserVariable, EncryptionStrength },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AuthenticateSession);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Request", "UserVariable", "EncryptionStrength" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is HttpRequest Request))
				throw new ScriptRuntimeException("Expected an HTTP Request in the first argument.", this);

			string UserVariable = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;

			if (Arguments.Length > 2)
			{
				object Obj = Arguments[2].AssociatedObjectValue;

				if (Obj is bool UseEncryption)
				{
					if (UseEncryption && !Request.Encrypted)
						throw new ForbiddenException(Request, "Access to resource requires encryption.");
				}
				else
				{
					double MinStrength = Expression.ToDouble(Obj);

					if (!Request.Encrypted)
						throw new ForbiddenException(Request, "Access to resource requires encryption.");

					if (Request.CipherStrength < MinStrength)
						throw new ForbiddenException(Request, "Access to resource requires encryption of minimum strength " + Expression.ToString(MinStrength) + ".");
				}
			}

			if (!Variables.TryGetVariable(UserVariable, out Variable Variable) ||
				!(Variable.ValueObject is IUser User))
			{
				throw ForbiddenException.AccessDenied(string.Empty, string.Empty, string.Empty);
			}

			return new ObjectValue(User);
		}
	}
}
