using System;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Authentication;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Security
{
	/// <summary>
	/// Authenticates an HTTP request using DIGEST SHA256 authentication.
	/// </summary>
	public class AuthenticateDigestSha256 : FunctionMultiVariate
	{
		/// <summary>
		/// Authenticates an HTTP request using DIGEST SHA256 authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Realm">Realm to authenticate against.</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateDigestSha256(ScriptNode Request, ScriptNode Realm, ScriptNode Users, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Realm, Users },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Authenticates an HTTP request using DIGEST SHA256 authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Realm">Realm to authenticate against.</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="EncryptionStrength">The level of encryption strength required.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateDigestSha256(ScriptNode Request, ScriptNode Realm, ScriptNode Users, ScriptNode EncryptionStrength, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Realm, Users, EncryptionStrength },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AuthenticateDigestSha256);

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
			if (!(Arguments[0].AssociatedObjectValue is HttpRequest Request))
				throw new ScriptRuntimeException("Expected an HTTP Request in the first argument.", this);

			string Realm = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;

			if (!(Arguments[2].AssociatedObjectValue is IUserSource Users))
				throw new ScriptRuntimeException("Expected a users collection as the third argument.", this);

			if (Arguments.Length > 3)
			{
				object Obj = Arguments[3].AssociatedObjectValue;

				if (Obj is bool UseEncryption)
				{
					if (UseEncryption && !Request.Encrypted)
						throw new ForbiddenException("Access to resource requires encryption.");
				}
				else
				{
					double MinStrength = Expression.ToDouble(Obj);

					if (!Request.Encrypted)
						throw new ForbiddenException("Access to resource requires encryption.");

					if (Request.CipherStrength < MinStrength)
						throw new ForbiddenException("Access to resource requires encryption of minimum strength " + Expression.ToString(MinStrength) + ".");
				}
			}

			DigestAuthentication Mechanism = this.last;

			if (Mechanism is null || this.last.Realm != Realm || this.last.Users != Users)
				Mechanism = this.last = new DigestAuthentication(false, 0, DigestAlgorithm.SHA256, Realm, Users);

			if (Request.Header.Authorization is null)
				throw new UnauthorizedException("Unauthorized access prohibited.", new string[] { Mechanism.GetChallenge() });
			else
			{
				IUser User = await Mechanism.IsAuthenticated(Request);
				if (User is null)
					throw new ForbiddenException("Invalid user name or password.");

				return new ObjectValue(User);
			}
		}

		private DigestAuthentication last = null;
	}
}
