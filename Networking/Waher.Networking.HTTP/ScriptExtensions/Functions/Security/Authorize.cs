using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Security;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Security
{
	/// <summary>
	/// Authenticates an HTTP request using an authenticated user in the session.
	/// </summary>
	public class Authorize : FunctionMultiVariate
	{
		/// <summary>
		/// Authenticates an HTTP request using an authenticated user in the session.
		/// </summary>
		/// <param name="User">User object</param>
		/// <param name="Privileges">Vector of privileges required to continue.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Authorize(ScriptNode User, ScriptNode Privileges, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { User, Privileges },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Authenticates an HTTP request using an authenticated user in the session.
		/// </summary>
		/// <param name="User">User object</param>
		/// <param name="Privileges">Vector of privileges required to continue.</param>
		/// <param name="Message">Optional exception message, if user does not have appropriate privileges.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Authorize(ScriptNode User, ScriptNode Privileges, ScriptNode Message, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { User, Privileges, Message },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Authorize);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "User", "Privileges", "Message" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is IUser User))
				throw new ScriptRuntimeException("Expected a User Object in the first argument.", this);

			if (!(Arguments[1] is IVector v))
				throw new ScriptRuntimeException("Expected a vector in the second argument.", this);

			int i, c = v.Dimension;
			string Privilege;

			for (i = 0; i < c; i++)
			{
				Privilege = v.GetElement(i).AssociatedObjectValue?.ToString() ?? string.Empty;
				if (string.IsNullOrEmpty(Privilege))
					continue;

				if (!User.HasPrivilege(Privilege))
				{
					string Message;

					if (Arguments.Length > 2)
						Message = Arguments[2].AssociatedObjectValue?.ToString() ?? "Access denied.";
					else
						Message = "Access denied.";

					throw ForbiddenException.AccessDenied(Message, string.Empty, User.UserName, Privilege);
				}
			}

			return Arguments[0];
		}
	}
}
