using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Sets a runtime setting with name `Name` to the value `Value`. If `Host` is provided, the corresponding runtime host setting is set.
	/// </summary>
	public class SetSetting : FunctionMultiVariate
	{
		/// <summary>
		/// Sets a runtime setting with name `Name` to the value `Value`. If `Host` is provided, the corresponding runtime host setting is set.
		/// </summary>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="Value">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetSetting(ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Name, Value }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sets a runtime setting with name `Name` to the value `Value`. If `Host` is provided, the corresponding runtime host setting is set.
		/// </summary>
		/// <param name="Host">Name of host.</param>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="Value">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetSetting(ScriptNode Host, ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Name, Value }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(SetSetting);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Name", "Value" };

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
			int i = 0;
			int c = Arguments.Length;
			string Host = c < 3 ? null : Arguments[i++].AssociatedObjectValue?.ToString();
			string Name = Arguments[i++].AssociatedObjectValue?.ToString();
			object Value = Arguments[i].AssociatedObjectValue;
			bool Result;

			if (string.IsNullOrEmpty(Host))
				Result = await RuntimeSettings.SetAsync(Name, Value);
			else
				Result = await HostSettings.SetAsync(Host, Name, Value);

			return new BooleanValue(Result);
		}
	}
}
