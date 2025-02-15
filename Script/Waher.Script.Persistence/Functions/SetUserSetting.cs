using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Sets a runtime user setting with name `Name` to the value `Value`. `User` defines the user.
	/// </summary>
	public class SetUserSetting : FunctionMultiVariate
	{
		/// <summary>
		/// Sets a runtime user setting with name `Name` to the value `Value`. `User` defines the user.
		/// </summary>
		/// <param name="User">Name of user.</param>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="Value">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetUserSetting(ScriptNode User, ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { User, Name, Value }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(SetUserSetting);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "User", "Name", "Value" };

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
			string User = await GetSetting.GetUser(Arguments[0].AssociatedObjectValue, this);

			if (string.IsNullOrEmpty(User))
				throw new ScriptRuntimeException("User not defined.", this);

			string Name = Arguments[1].AssociatedObjectValue?.ToString();
			object Value = Arguments[2].AssociatedObjectValue;
			bool Result;

			Result = await UserSettings.SetAsync(User, Name, Value);

			return new BooleanValue(Result);
		}
	}
}
