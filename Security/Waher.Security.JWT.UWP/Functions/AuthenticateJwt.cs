using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWT.Functions
{
	/// <summary>
	/// Authenticates an HTTP request using BEARER and JWT authentication.
	/// </summary>
	public class AuthenticateJwt : FunctionMultiVariate
	{
		/// <summary>
		/// Authenticates an HTTP request using BEARER and JWT authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Realm">Realm to authenticate against.</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateJwt(ScriptNode Request, ScriptNode Realm, ScriptNode Users, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Realm, Users },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Authenticates an HTTP request using BEARER and JWT authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Realm">Realm to authenticate against.</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="EncryptionStrength">The level of encryption strength required.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateJwt(ScriptNode Request, ScriptNode Realm, ScriptNode Users, ScriptNode EncryptionStrength, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Realm, Users, EncryptionStrength },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AuthenticateJwt);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Request", "Realm", "Users", "EncryptionStrength" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;
			int i = 0;

			if (!(Arguments[i++].AssociatedObjectValue is HttpRequest Request))
				throw new ScriptRuntimeException("Expected an HTTP Request in the first argument.", this);

			string Realm = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;

			if (!(Arguments[i++].AssociatedObjectValue is IUserSource Users))
				throw new ScriptRuntimeException("Expected a users collection as the third argument.", this);

			JwtFactory Factory = CreateJwt.Factory;

			while (i < c)
			{
				object Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is JwtFactory Factory2)
					Factory = Factory2;
				else if (Obj is bool UseEncryption)
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

			JwtAuthentication Mechanism = this.last;

			if (Mechanism is null || this.last.Realm != Realm || this.last.Users != Users || this.last.Factory != Factory)
				Mechanism = this.last = new JwtAuthentication(Realm, Users, Factory);

			if (Request.Header.Authorization is null)
				throw new UnauthorizedException("Unauthorized access prohibited.", new string[] { Mechanism.GetChallenge() });
			else
			{
				IUser User = await Mechanism.IsAuthenticated(Request)
					?? throw new ForbiddenException(Request, "Invalid user name or password.");

				return new ObjectValue(User);
			}
		}

		private JwtAuthentication last = null;
	}
}
