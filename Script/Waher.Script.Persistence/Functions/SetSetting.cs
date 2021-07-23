using System;
using Waher.Runtime.Settings;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Sets a runtime setting with name `Name` to the value `Value`.
	/// </summary>
	public class SetSetting : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Sets a runtime setting with name `Name` to the value `Value`.
		/// </summary>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="Value">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetSetting(ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
			: base(Name, Value, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "SetSetting";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Name", "Value" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			string Name = Argument1.AssociatedObjectValue?.ToString();
			if (string.IsNullOrEmpty(Name))
				throw new ScriptRuntimeException("Name cannot be empty.", this);

			bool Result = RuntimeSettings.Set(Name, Argument2.AssociatedObjectValue); // TODO: Asynchronous overload.

			return new BooleanValue(Result);
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			if (string.IsNullOrEmpty(Argument1))
				throw new ScriptRuntimeException("Name cannot be empty.", this);

			bool Result = RuntimeSettings.Set(Argument1, Argument2); // TODO: Asynchronous overload.

			return new BooleanValue(Result);
		}
	}
}
