#if !WINDOWS_UWP

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
	/// Authenticates an HTTP request using Mutual-TLS authentication.
	/// </summary>
	public class AuthenticateMutualTls : FunctionMultiVariate
	{
		/// <summary>
		/// Authenticates an HTTP request using Mutual-TLS authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateMutualTls(ScriptNode Request, ScriptNode Users, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Users },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Authenticates an HTTP request using Mutual-TLS authentication.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Users">Users collection.</param>
		/// <param name="EncryptionStrength">The level of encryption strength required.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AuthenticateMutualTls(ScriptNode Request, ScriptNode Users, ScriptNode EncryptionStrength, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Request, Users, EncryptionStrength },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AuthenticateMutualTls);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Request", "Users", "EncryptionStrength" };

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

			if (!(Arguments[1].AssociatedObjectValue is IUserSource Users))
				throw new ScriptRuntimeException("Expected a users collection as the second argument.", this);

			if (!Request.Encrypted)
				throw new ForbiddenException("Access to resource requires encryption.");

			if (Arguments.Length > 2)
			{
				double MinStrength = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

				if (!Request.Encrypted)
					throw new ForbiddenException("Access to resource requires encryption.");

				if (Request.CipherStrength < MinStrength)
					throw new ForbiddenException("Access to resource requires encryption of minimum strength " + Expression.ToString(MinStrength) + ".");
			}

			MutualTlsAuthentication Mechanism = this.last;

			if (Mechanism is null || this.last.Users != Users)
				Mechanism = this.last = new MutualTlsAuthentication(Users);

			IUser User = await Mechanism.IsAuthenticated(Request);
			if (User is null)
				throw new ForbiddenException("Invalid client certificate, or certificate not recognized.");

			return new ObjectValue(User);
		}

		private MutualTlsAuthentication last = null;
	}
}

#endif